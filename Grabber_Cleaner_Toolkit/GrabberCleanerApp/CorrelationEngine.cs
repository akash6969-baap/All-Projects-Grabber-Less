using System;
using System.Collections.Generic;
using System.Linq;

namespace GrabberCleanerApp
{
    public static class CorrelationEngine
    {
        public static void Correlate(FileInspectionResult result, List<EvidenceItem> evidenceList)
        {
            // ── CHAIN 1: CREDENTIAL & DISCORD TOKEN STEALER (CHAIN-OF-CRIME) ──────
            bool hasCredentialTarget = evidenceList.Any(e => e.Category == EvidenceCategory.CREDENTIAL && !e.IsMitigating);
            bool hasDpapiAccess = result.CredentialAPIs.Any(a => a.Contains("CryptUnprotectData", StringComparison.OrdinalIgnoreCase) || a.Contains("BCryptDecrypt", StringComparison.OrdinalIgnoreCase));
            bool hasExfiltration = evidenceList.Any(e => e.Category == EvidenceCategory.EXFILTRATION);

            if (hasExfiltration && (hasCredentialTarget || hasDpapiAccess))
            {
                result.IsConfirmedStealer = true;
                result.TechnicalAnalysisNotes.Add("🚨 CONFIRMED TOKEN & CREDENTIAL STEALER: Target credential storage/DPAPI + Exfiltration Webhooks correlated!");
                result.CorrelatedEvidence.Add("Chain-of-Crime: Exfiltration Webhook + Credential Database Target Access");

                evidenceList.Add(new EvidenceItem
                {
                    Category = EvidenceCategory.CREDENTIAL,
                    Indicator = "Correlated Credential Stealer Chain",
                    Description = "Verified complete chain-of-crime: Targets browser/Discord credentials, accesses DPAPI decryption, and transmits stolen data via external Webhook/C2.",
                    Severity = EvidenceSeverity.Critical,
                    Confidence = 98,
                    IsBehavioral = true,
                    AnalyzerName = nameof(CorrelationEngine)
                });
            }
            else if (hasExfiltration)
            {
                result.IsConfirmedStealer = true;
                result.TechnicalAnalysisNotes.Add("🚨 EXFILTRATION CHANNEL: Hardcoded Discord/Telegram Webhook C2 URL detected!");
                result.CorrelatedEvidence.Add("Hardcoded Discord / Telegram Exfiltration Webhook C2");

                evidenceList.Add(new EvidenceItem
                {
                    Category = EvidenceCategory.EXFILTRATION,
                    Indicator = "Active Exfiltration Channel",
                    Description = "Contains hardcoded Discord Webhook or Telegram Bot exfiltration destination.",
                    Severity = EvidenceSeverity.High,
                    Confidence = 88,
                    IsBehavioral = true,
                    AnalyzerName = nameof(CorrelationEngine)
                });
            }

            // ── CHAIN 2: DROPPER / DOWNLOADER CORRELATION ─────────────────────────
            bool hasDownloaderAPI = result.DetectedDownloaderAPIs.Count > 0;
            bool hasPayloadUrl = result.ExtractedDropperPayloadUrls.Count > 0;

            if (hasDownloaderAPI && hasPayloadUrl)
            {
                result.IsConfirmedDropper = true;
                result.TechnicalAnalysisNotes.Add("🚨 CONFIRMED PAYLOAD DROPPER: Downloader APIs AND hardcoded remote executable payload URL matched!");
                result.CorrelatedEvidence.Add("Downloader API Import + Direct Hardcoded Remote Payload URL");

                evidenceList.Add(new EvidenceItem
                {
                    Category = EvidenceCategory.DROPPER,
                    Indicator = "Correlated Payload Dropper Chain",
                    Description = "Imports downloader APIs to fetch an external executable payload from a remote URL.",
                    Severity = EvidenceSeverity.Critical,
                    Confidence = 95,
                    IsBehavioral = true,
                    AnalyzerName = nameof(CorrelationEngine)
                });
            }

            // ── CHAIN 3: CAMOUFLAGE DEFENSE ───────────────────────────────────────
            if (result.IsKeyAuthDetected)
            {
                string authTitle = !string.IsNullOrEmpty(result.DetectedAuthName) ? result.DetectedAuthName : "Legitimate Auth System";

                if (result.IsConfirmedStealer)
                {
                    result.TechnicalAnalysisNotes.Add($"🚨 CRITICAL CAMOUFLAGE: Stealer payload detected inside [{authTitle}] binary! Whitelist overridden.");
                    result.CorrelatedEvidence.Add($"Malware stealer camouflaged inside {authTitle} wrapper");

                    evidenceList.Add(new EvidenceItem
                    {
                        Category = EvidenceCategory.AUTHENTICATION,
                        Indicator = "Camouflaged Stealer in Auth Wrapper",
                        Description = $"Malicious credential stealer packaged inside legitimate {authTitle} wrapper.",
                        Severity = EvidenceSeverity.Critical,
                        Confidence = 95,
                        IsBehavioral = true,
                        AnalyzerName = nameof(CorrelationEngine)
                    });
                }
                else if (result.IsConfirmedDropper)
                {
                    result.TechnicalAnalysisNotes.Add($"🚨 CRITICAL CAMOUFLAGE: Dropper payload detected inside [{authTitle}] binary! Whitelist overridden.");
                    result.CorrelatedEvidence.Add($"Malware dropper camouflaged inside {authTitle} wrapper");

                    evidenceList.Add(new EvidenceItem
                    {
                        Category = EvidenceCategory.AUTHENTICATION,
                        Indicator = "Camouflaged Dropper in Auth Wrapper",
                        Description = $"Malicious dropper packaged inside legitimate {authTitle} wrapper.",
                        Severity = EvidenceSeverity.Critical,
                        Confidence = 95,
                        IsBehavioral = true,
                        AnalyzerName = nameof(CorrelationEngine)
                    });
                }
            }
        }
    }
}
