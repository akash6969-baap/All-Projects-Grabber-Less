using System;
using System.Collections.Generic;
using System.Linq;

namespace GrabberCleanerApp
{
    public static class EmbeddedPayloadAnalyzer
    {
        private const int MaxRecursionDepth = 2;
        private const int MaxPayloadSize = 32 * 1024 * 1024; // 32 MB limit

        public static void Analyze(byte[] bytes, FileInspectionResult result, List<EvidenceItem> evidenceList, int currentDepth = 0)
        {
            if (currentDepth >= MaxRecursionDepth || bytes.Length < 4096) return;

            // Search for secondary "MZ" header past initial 4096 bytes
            int startSearch = 4096;
            int limit = Math.Min(bytes.Length - 0x40, MaxPayloadSize);

            for (int i = startSearch; i < limit; i++)
            {
                if (bytes[i] == 0x4D && bytes[i + 1] == 0x5A) // "MZ"
                {
                    int e_lfanew = BitConverter.ToInt32(bytes, i + 0x3C);

                    if (e_lfanew > 0 && e_lfanew < 1024 && (i + e_lfanew + 4) <= bytes.Length)
                    {
                        int ntOffset = i + e_lfanew;
                        if (bytes[ntOffset] == 0x50 && bytes[ntOffset + 1] == 0x45 &&
                            bytes[ntOffset + 2] == 0x00 && bytes[ntOffset + 3] == 0x00) // "PE\0\0"
                        {
                            result.HasValidatedEmbeddedPE = true;
                            result.EmbeddedPEOffset = i;
                            result.TechnicalAnalysisNotes.Add($"🚨 CRITICAL: Verified Secondary Embedded PE Binary (Valid PE NT Header) at offset 0x{i:X}!");
                            result.CorrelatedEvidence.Add($"Hidden secondary PE executable embedded at offset 0x{i:X}");

                            evidenceList.Add(new EvidenceItem
                            {
                                Category = EvidenceCategory.EMBEDDED_PAYLOAD,
                                Indicator = "Embedded Secondary PE Executable",
                                Description = $"Found verified secondary Windows PE executable embedded inside file at offset 0x{i:X}.",
                                Severity = EvidenceSeverity.High,
                                Confidence = 90,
                                Location = $"Offset 0x{i:X}",
                                Offset = i,
                                IsBehavioral = true,
                                AnalyzerName = nameof(EmbeddedPayloadAnalyzer)
                            });

                            // Analyze child PE slice statically
                            byte[] childSlice = bytes.Skip(i).ToArray();
                            StringAnalyzer.Analyze(childSlice, result, evidenceList);

                            // Recurse with incremented depth
                            Analyze(childSlice, result, evidenceList, currentDepth + 1);
                            break;
                        }
                    }
                }
            }
        }
    }
}
