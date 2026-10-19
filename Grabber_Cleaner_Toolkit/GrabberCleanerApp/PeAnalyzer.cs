using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GrabberCleanerApp
{
    public static class PeAnalyzer
    {
        public static void Analyze(byte[] bytes, FileInspectionResult result, List<EvidenceItem> evidenceList)
        {
            if (bytes.Length < 0x40) return;

            // Check DOS Header "MZ"
            if (bytes[0] != 0x4D || bytes[1] != 0x5A)
            {
                result.FileTypeDescription = "Non-PE Data / Script";
                return;
            }

            result.FileTypeDescription = "Windows PE Executable (.EXE / .DLL)";

            int e_lfanew = BitConverter.ToInt32(bytes, 0x3C);
            if (e_lfanew <= 0 || e_lfanew + 248 > bytes.Length)
            {
                evidenceList.Add(new EvidenceItem
                {
                    Category = EvidenceCategory.PE,
                    Indicator = "Malformed DOS Header",
                    Description = "e_lfanew points outside the file boundaries.",
                    Severity = EvidenceSeverity.Low,
                    Confidence = 40,
                    AnalyzerName = nameof(PeAnalyzer)
                });
                return;
            }

            // NT Signature: PE\0\0
            if (bytes[e_lfanew] != 0x50 || bytes[e_lfanew + 1] != 0x45 ||
                bytes[e_lfanew + 2] != 0x00 || bytes[e_lfanew + 3] != 0x00)
            {
                evidenceList.Add(new EvidenceItem
                {
                    Category = EvidenceCategory.PE,
                    Indicator = "Invalid NT Signature",
                    Description = "MZ header present but PE signature missing at expected offset.",
                    Severity = EvidenceSeverity.Low,
                    Confidence = 30,
                    AnalyzerName = nameof(PeAnalyzer)
                });
                return;
            }

            ushort machine = BitConverter.ToUInt16(bytes, e_lfanew + 4);
            string arch = machine switch
            {
                0x014c => "x86 (32-bit)",
                0x8664 => "x64 (64-bit AMD/Intel)",
                0xaa64 => "ARM64",
                _ => $"0x{machine:X4}"
            };

            ushort numberOfSections = BitConverter.ToUInt16(bytes, e_lfanew + 6);
            ushort sizeOfOptionalHeader = BitConverter.ToUInt16(bytes, e_lfanew + 20);
            ushort characteristics = BitConverter.ToUInt16(bytes, e_lfanew + 22);

            result.TechnicalAnalysisNotes.Add($"ℹ️ Architecture: {arch} | Sections: {numberOfSections} | Characteristics: 0x{characteristics:X4}");

            evidenceList.Add(new EvidenceItem
            {
                Category = EvidenceCategory.PE,
                Indicator = "Valid PE NT Header",
                Description = $"Target binary is a valid {arch} PE file with {numberOfSections} sections.",
                Severity = EvidenceSeverity.Info,
                Confidence = 95,
                Location = $"Offset 0x{e_lfanew:X}",
                Offset = e_lfanew,
                AnalyzerName = nameof(PeAnalyzer)
            });

            int optionalHeaderOffset = e_lfanew + 24;
            if (optionalHeaderOffset + sizeOfOptionalHeader > bytes.Length) return;

            // Section Headers start right after Optional Header
            int sectionHeadersOffset = optionalHeaderOffset + sizeOfOptionalHeader;
            uint highestRawEnd = 0;

            for (int s = 0; s < numberOfSections; s++)
            {
                int secOffset = sectionHeadersOffset + (s * 40);
                if (secOffset + 40 > bytes.Length) break;

                string secName = Encoding.ASCII.GetString(bytes, secOffset, 8).TrimEnd('\0', ' ');
                uint vSize = BitConverter.ToUInt32(bytes, secOffset + 8);
                uint vAddr = BitConverter.ToUInt32(bytes, secOffset + 12);
                uint rawSize = BitConverter.ToUInt32(bytes, secOffset + 16);
                uint rawOffset = BitConverter.ToUInt32(bytes, secOffset + 20);
                uint secChars = BitConverter.ToUInt32(bytes, secOffset + 36);

                if (rawOffset + rawSize > highestRawEnd)
                {
                    highestRawEnd = rawOffset + rawSize;
                }

                double secEntropy = 0;
                if (rawOffset + rawSize <= (uint)bytes.Length && rawSize > 0)
                {
                    secEntropy = CalculateEntropySlice(bytes, (int)rawOffset, (int)rawSize);
                }

                var secInfo = new PESectionInfo
                {
                    Name = secName,
                    VirtualSize = vSize,
                    VirtualAddress = vAddr,
                    RawSize = rawSize,
                    RawOffset = rawOffset,
                    Characteristics = secChars,
                    Entropy = secEntropy
                };
                result.PESections.Add(secInfo);

                // Check W^X violation (Writable + Executable memory)
                if (secInfo.IsWritableAndExecutable)
                {
                    result.HasWritableExecutableSections = true;
                    result.TechnicalAnalysisNotes.Add($"⚠️ W^X Violation in section '{secName}': Memory has both WRITE & EXECUTE permissions (Code injection vector).");
                    result.CorrelatedEvidence.Add($"Writable & Executable Section '{secName}' (W^X violation)");

                    evidenceList.Add(new EvidenceItem
                    {
                        Category = EvidenceCategory.MEMORY,
                        Indicator = "W^X Section Violation",
                        Description = $"Section '{secName}' is both Writable and Executable (RWX). Common memory injection vector.",
                        Severity = EvidenceSeverity.High,
                        Confidence = 80,
                        Location = secName,
                        Offset = rawOffset,
                        IsBehavioral = true,
                        AnalyzerName = nameof(PeAnalyzer)
                    });
                }

                // Check high entropy per section
                if (secEntropy > 7.4 && rawSize > 16384)
                {
                    evidenceList.Add(new EvidenceItem
                    {
                        Category = EvidenceCategory.OBFUSCATION,
                        Indicator = "High Section Entropy",
                        Description = $"Section '{secName}' has high entropy ({secEntropy:F2}/8.00). Indicates packing, compression, or encryption.",
                        Severity = EvidenceSeverity.Medium,
                        Confidence = 70,
                        Location = secName,
                        Offset = rawOffset,
                        AnalyzerName = nameof(PeAnalyzer)
                    });
                }

                // Check packer section names
                if (secName.Equals(".upx0", StringComparison.OrdinalIgnoreCase) ||
                    secName.Equals(".upx1", StringComparison.OrdinalIgnoreCase) ||
                    secName.Contains("vmp", StringComparison.OrdinalIgnoreCase) ||
                    secName.Contains("themida", StringComparison.OrdinalIgnoreCase))
                {
                    evidenceList.Add(new EvidenceItem
                    {
                        Category = EvidenceCategory.OBFUSCATION,
                        Indicator = "Packer Section Name",
                        Description = $"Section '{secName}' matches known commercial packer / protector signature.",
                        Severity = EvidenceSeverity.Low,
                        Confidence = 75,
                        Location = secName,
                        Offset = rawOffset,
                        AnalyzerName = nameof(PeAnalyzer)
                    });
                }
            }

            // Overlay Detection
            if (highestRawEnd > 0 && bytes.Length > highestRawEnd + 1024)
            {
                result.HasPEOverlayData = true;
                result.OverlaySizeBytes = bytes.Length - highestRawEnd;
                result.TechnicalAnalysisNotes.Add($"ℹ️ Binary contains PE Overlay data ({result.OverlaySizeBytes / 1024.0:F1} KB attached after section headers).");

                evidenceList.Add(new EvidenceItem
                {
                    Category = EvidenceCategory.RESOURCE,
                    Indicator = "PE Overlay Data",
                    Description = $"File contains {result.OverlaySizeBytes / 1024.0:F1} KB of overlay data appended past the final PE section.",
                    Severity = EvidenceSeverity.Low,
                    Confidence = 60,
                    Location = $"Offset 0x{highestRawEnd:X}",
                    Offset = highestRawEnd,
                    AnalyzerName = nameof(PeAnalyzer)
                });
            }
        }

        public static double CalculateEntropySlice(byte[] bytes, int offset, int length)
        {
            if (length <= 0 || offset + length > bytes.Length) return 0.0;
            int[] map = new int[256];
            for (int i = offset; i < offset + length; i++) map[bytes[i]]++;

            double entropy = 0;
            double len = length;
            for (int i = 0; i < 256; i++)
            {
                if (map[i] > 0)
                {
                    double p = map[i] / len;
                    entropy -= p * Math.Log2(p);
                }
            }
            return entropy;
        }
    }
}
