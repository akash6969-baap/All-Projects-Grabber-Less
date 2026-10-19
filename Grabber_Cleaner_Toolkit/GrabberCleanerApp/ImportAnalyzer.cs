using System;
using System.Collections.Generic;
using System.Text;

namespace GrabberCleanerApp
{
    public static class ImportAnalyzer
    {
        private static readonly string[] ProcessAPIs = new[]
        {
            "OpenProcess", "CreateProcessA", "CreateProcessW", "TerminateProcess"
        };

        private static readonly string[] InjectionAPIs = new[]
        {
            "CreateRemoteThread", "NtCreateThreadEx", "QueueUserAPC", "SetThreadContext",
            "WriteProcessMemory", "VirtualAllocEx", "RtlCreateUserThread"
        };

        private static readonly string[] MemoryAPIsList = new[]
        {
            "VirtualProtect", "VirtualProtectEx", "VirtualAlloc", "ReadProcessMemory",
            "NtAllocateVirtualMemory", "NtProtectVirtualMemory", "MapViewOfFile"
        };

        private static readonly string[] CredentialAPIsList = new[]
        {
            "CryptUnprotectData", "CredReadA", "CredReadW", "CredEnumerateA", "CredEnumerateW",
            "LsaRetrievePrivateData", "SamIGetPrivateData", "BCryptDecrypt"
        };

        private static readonly string[] PersistenceAPIsList = new[]
        {
            "RegSetValueExA", "RegSetValueExW", "CreateServiceA", "CreateServiceW",
            "ChangeServiceConfigA", "ChangeServiceConfigW"
        };

        private static readonly string[] DownloaderAPIs = new[]
        {
            "URLDownloadToFile", "InternetOpenUrl", "InternetReadFile", "WinHttpOpen",
            "WinHttpConnect", "WinHttpSendRequest", "HttpSendRequest", "WebClient",
            "DownloadFile", "DownloadString", "Invoke-WebRequest", "iwr -Uri",
            "certutil -urlcache", "bitsadmin /transfer", "curl.exe"
        };

        public static void Analyze(byte[] bytes, FileInspectionResult result, List<EvidenceItem> evidenceList)
        {
            string allAscii = Encoding.ASCII.GetString(bytes);

            // Process APIs
            foreach (var api in ProcessAPIs)
            {
                if (allAscii.Contains(api, StringComparison.OrdinalIgnoreCase))
                {
                    result.ImportedAPIs.Add(api);
                }
            }

            // Injection APIs
            foreach (var api in InjectionAPIs)
            {
                if (allAscii.Contains(api, StringComparison.OrdinalIgnoreCase))
                {
                    result.ProcessInjectionAPIs.Add(api);
                    result.ImportedAPIs.Add(api);

                    evidenceList.Add(new EvidenceItem
                    {
                        Category = EvidenceCategory.INJECTION,
                        Indicator = $"API: {api}",
                        Description = $"Imports Process Injection API: {api}",
                        Severity = EvidenceSeverity.Medium,
                        Confidence = 70,
                        AnalyzerName = nameof(ImportAnalyzer)
                    });
                }
            }

            // Memory APIs
            foreach (var api in MemoryAPIsList)
            {
                if (allAscii.Contains(api, StringComparison.OrdinalIgnoreCase))
                {
                    result.MemoryAPIs.Add(api);
                    result.ImportedAPIs.Add(api);
                }
            }

            // Credential / DPAPI APIs
            foreach (var api in CredentialAPIsList)
            {
                if (allAscii.Contains(api, StringComparison.OrdinalIgnoreCase))
                {
                    result.CredentialAPIs.Add(api);
                    result.ImportedAPIs.Add(api);

                    evidenceList.Add(new EvidenceItem
                    {
                        Category = EvidenceCategory.CREDENTIAL,
                        Indicator = $"API: {api}",
                        Description = $"Imports Windows DPAPI / Credential Decryption API: {api}",
                        Severity = EvidenceSeverity.High,
                        Confidence = 85,
                        IsBehavioral = true,
                        AnalyzerName = nameof(ImportAnalyzer)
                    });
                }
            }

            // Persistence APIs
            foreach (var api in PersistenceAPIsList)
            {
                if (allAscii.Contains(api, StringComparison.OrdinalIgnoreCase))
                {
                    result.PersistenceAPIs.Add(api);
                    result.ImportedAPIs.Add(api);

                    evidenceList.Add(new EvidenceItem
                    {
                        Category = EvidenceCategory.PERSISTENCE,
                        Indicator = $"API: {api}",
                        Description = $"Imports Registry/Service Persistence API: {api}",
                        Severity = EvidenceSeverity.Medium,
                        Confidence = 60,
                        AnalyzerName = nameof(ImportAnalyzer)
                    });
                }
            }

            // Downloader / Network APIs
            foreach (var api in DownloaderAPIs)
            {
                if (allAscii.Contains(api, StringComparison.OrdinalIgnoreCase))
                {
                    result.DetectedDownloaderAPIs.Add(api);
                    result.ImportedAPIs.Add(api);

                    evidenceList.Add(new EvidenceItem
                    {
                        Category = EvidenceCategory.DOWNLOADER,
                        Indicator = $"API: {api}",
                        Description = $"Imports Downloader/Network API: {api}",
                        Severity = EvidenceSeverity.Low,
                        Confidence = 50,
                        AnalyzerName = nameof(ImportAnalyzer)
                    });
                }
            }

            // Process Injection Combination Check
            if (result.ProcessInjectionAPIs.Count >= 2)
            {
                result.TechnicalAnalysisNotes.Add($"⚠️ Process Injection API Combination: {string.Join(", ", result.ProcessInjectionAPIs)}");
                result.CorrelatedEvidence.Add($"Process Injection API combination ({string.Join(", ", result.ProcessInjectionAPIs.Take(3))})");

                evidenceList.Add(new EvidenceItem
                {
                    Category = EvidenceCategory.INJECTION,
                    Indicator = "Process Injection Combination",
                    Description = $"Imports correlated process injection APIs: {string.Join(", ", result.ProcessInjectionAPIs)}",
                    Severity = EvidenceSeverity.High,
                    Confidence = 85,
                    IsBehavioral = true,
                    AnalyzerName = nameof(ImportAnalyzer)
                });
            }
        }
    }
}
