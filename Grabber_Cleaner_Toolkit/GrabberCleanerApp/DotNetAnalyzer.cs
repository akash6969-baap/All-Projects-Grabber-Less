using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrabberCleanerApp
{
    public static class DotNetAnalyzer
    {
        public static void Analyze(byte[] bytes, FileInspectionResult result, List<EvidenceItem> evidenceList)
        {
            if (bytes.Length < 512) return;

            string asciiHeader = Encoding.ASCII.GetString(bytes.Take(Math.Min(bytes.Length, 16384)).ToArray());

            bool hasBSJB = asciiHeader.Contains("BSJB");
            bool hasMscoree = asciiHeader.Contains("mscoree.dll");
            bool hasCorLib = asciiHeader.Contains("mscorlib") || asciiHeader.Contains("System.Runtime");

            if (hasBSJB || (hasMscoree && hasCorLib))
            {
                result.IsDotNetAssembly = true;
                result.FileTypeDescription = "Windows .NET Managed Assembly";
                result.TechnicalAnalysisNotes.Add("ℹ️ Executable is a .NET C# / VB.NET Assembly.");

                evidenceList.Add(new EvidenceItem
                {
                    Category = EvidenceCategory.DOTNET,
                    Indicator = ".NET Managed Assembly",
                    Description = "Target is a compiled .NET Common Language Runtime (CLR) assembly.",
                    Severity = EvidenceSeverity.Info,
                    Confidence = 95,
                    AnalyzerName = nameof(DotNetAnalyzer)
                });

                // Deep Static String & Metadata Inspection for Dangerous .NET APIs
                string allAscii = Encoding.ASCII.GetString(bytes);

                // 1. Dynamic Assembly Loading from Memory (Stagers / Droppers)
                bool hasAssemblyLoad = allAscii.Contains("Assembly.Load", StringComparison.OrdinalIgnoreCase) ||
                                       allAscii.Contains("AppDomain.CurrentDomain.Load", StringComparison.OrdinalIgnoreCase);

                bool hasFromBase64 = allAscii.Contains("FromBase64String", StringComparison.OrdinalIgnoreCase);

                if (hasAssemblyLoad && hasFromBase64)
                {
                    result.TechnicalAnalysisNotes.Add("⚠️ .NET Dynamic Assembly Loading: Contains Base64 decoding AND Assembly.Load(byte[]) pattern (In-Memory Payload Dropper).");
                    result.CorrelatedEvidence.Add(".NET In-Memory Dynamic Assembly Execution (FromBase64 + Assembly.Load)");

                    evidenceList.Add(new EvidenceItem
                    {
                        Category = EvidenceCategory.DROPPER,
                        Indicator = ".NET Memory Assembly Loader",
                        Description = "Loads dynamic .NET assembly directly into memory from Base64 decoded byte array (In-Memory payload execution).",
                        Severity = EvidenceSeverity.High,
                        Confidence = 85,
                        IsBehavioral = true,
                        AnalyzerName = nameof(DotNetAnalyzer)
                    });
                }
                else if (hasAssemblyLoad)
                {
                    evidenceList.Add(new EvidenceItem
                    {
                        Category = EvidenceCategory.DOTNET,
                        Indicator = "Dynamic Assembly Loading",
                        Description = "Calls Assembly.Load to dynamically execute code at runtime.",
                        Severity = EvidenceSeverity.Medium,
                        Confidence = 65,
                        AnalyzerName = nameof(DotNetAnalyzer)
                    });
                }

                // 2. Reflection Invocations
                if (allAscii.Contains("MethodInfo.Invoke", StringComparison.OrdinalIgnoreCase) ||
                    allAscii.Contains("Type.GetType", StringComparison.OrdinalIgnoreCase))
                {
                    evidenceList.Add(new EvidenceItem
                    {
                        Category = EvidenceCategory.DOTNET,
                        Indicator = ".NET Reflection Invocations",
                        Description = "Uses reflection methods to dynamically resolve and invoke internal APIs.",
                        Severity = EvidenceSeverity.Low,
                        Confidence = 50,
                        AnalyzerName = nameof(DotNetAnalyzer)
                    });
                }

                // 3. P/Invoke Native API Invocations
                if (allAscii.Contains("DllImportAttribute", StringComparison.OrdinalIgnoreCase) ||
                    allAscii.Contains("[DllImport", StringComparison.OrdinalIgnoreCase))
                {
                    evidenceList.Add(new EvidenceItem
                    {
                        Category = EvidenceCategory.IMPORT,
                        Indicator = ".NET P/Invoke Interop",
                        Description = "Contains native unmanaged Win32 API calls via P/Invoke.",
                        Severity = EvidenceSeverity.Low,
                        Confidence = 60,
                        AnalyzerName = nameof(DotNetAnalyzer)
                    });
                }
            }

            // PyInstaller Detection
            if (asciiHeader.Contains("pyi-windows-manifest-filename") ||
                asciiHeader.Contains("PyInstaller") ||
                asciiHeader.Contains("PYTHON3"))
            {
                result.IsPyInstallerBundle = true;
                result.FileTypeDescription = "PyInstaller Compiled Python Executable";
                result.TechnicalAnalysisNotes.Add("⚠️ Identified as PyInstaller Executable Bundle (Frequently used by Discord Token Grabbers).");
                result.CorrelatedEvidence.Add("PyInstaller Python executable bundle");

                evidenceList.Add(new EvidenceItem
                {
                    Category = EvidenceCategory.RESOURCE,
                    Indicator = "PyInstaller Bundle",
                    Description = "Compiled Python standalone bundle (Standard delivery format for modern Discord grabbers).",
                    Severity = EvidenceSeverity.Medium,
                    Confidence = 75,
                    AnalyzerName = nameof(DotNetAnalyzer)
                });
            }
        }
    }
}
