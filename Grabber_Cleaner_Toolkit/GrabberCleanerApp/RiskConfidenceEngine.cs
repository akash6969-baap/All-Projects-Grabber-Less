using System;
using System.Collections.Generic;
using System.Linq;

namespace GrabberCleanerApp
{
    public static class RiskConfidenceEngine
    {
        public static void Calculate(FileInspectionResult result, List<EvidenceItem> evidenceList)
        {
            // Initial scores start at 0 (No artificial hardcoded defaults!)
            int risk = 0;
            int confidence = 0;

            // 1. Evidence-Driven Risk Accumulation
            foreach (var ev in evidenceList.Where(e => !e.IsMitigating))
            {
                risk += ev.Severity switch
                {
                    EvidenceSeverity.Critical => 40,
                    EvidenceSeverity.High => 25,
                    EvidenceSeverity.Medium => 15,
                    EvidenceSeverity.Low => 5,
                    _ => 0
                };
            }

            // 2. Behavioral Verdict Boosts
            if (result.IsConfirmedStealer && result.IsConfirmedDropper)
            {
                risk = Math.Max(risk, 98);
                confidence = 98;
            }
            else if (result.IsConfirmedStealer)
            {
                risk = Math.Max(risk, 95);
                confidence = 98;
            }
            else if (result.IsConfirmedDropper)
            {
                risk = Math.Max(risk, 90);
                confidence = 95;
            }
            else if (result.HasValidatedEmbeddedPE)
            {
                risk = Math.Max(risk, 65);
                confidence = Math.Max(confidence, 85);
            }
            else if (result.HasWritableExecutableSections)
            {
                risk = Math.Max(risk, 55);
                confidence = Math.Max(confidence, 80);
            }
            else if (result.ProcessInjectionAPIs.Count >= 2)
            {
                risk = Math.Max(risk, 50);
                confidence = Math.Max(confidence, 75);
            }
            else if (result.IsPackedOrEncrypted)
            {
                risk = Math.Max(risk, 30);
                confidence = Math.Max(confidence, 60);
            }
            else if (evidenceList.Any(e => e.Severity == EvidenceSeverity.Medium || e.Severity == EvidenceSeverity.High))
            {
                confidence = Math.Max(confidence, 50);
            }
            else
            {
                confidence = 95; // Confidently clean
            }

            // 3. Mitigating Factors
            if (result.IsKeyAuthDetected && !result.IsConfirmedStealer && !result.IsConfirmedDropper)
            {
                // Legitimate licensed tool (e.g. game mod, custom software with KeyAuth)
                result.MitigatingFactors.Add($"Verified Legitimate Protection & Licensing Framework ({result.DetectedAuthName ?? "Licensed"})");
                risk = Math.Min(risk, 35); // Cap risk for legitimate auth software
                confidence = Math.Max(confidence, 90);
            }

            if (result.IsDigitallySigned && result.IsAuthenticodeValid && !result.IsConfirmedStealer && !result.IsConfirmedDropper)
            {
                // Legitimate signed application
                result.MitigatingFactors.Add($"Digitally signed and verified by: {result.CertificateSigner}");
                risk = 0; // Verified authentic
                confidence = 95;
            }

            result.RiskScore = Math.Clamp(risk, 0, 100);
            result.ConfidenceScore = Math.Clamp(confidence, 0, 100);

            // 4. Final Verdict Assignment
            AssignFinalVerdict(result);
        }

        private static void AssignFinalVerdict(FileInspectionResult result)
        {
            if (result.IsConfirmedStealer && result.IsConfirmedDropper)
            {
                result.ThreatVerdict = "🚨 CRITICAL: COMBINED STEALER & PAYLOAD DROPPER MALWARE";
            }
            else if (result.IsConfirmedStealer)
            {
                result.ThreatVerdict = result.IsKeyAuthDetected
                    ? $"🚨 CRITICAL: TOKEN / CREDENTIAL STEALER (CAMOUFLAGED IN {result.DetectedAuthName})"
                    : "🚨 CRITICAL: CONFIRMED DISCORD / CREDENTIAL STEALER";
            }
            else if (result.IsConfirmedDropper)
            {
                result.ThreatVerdict = result.IsKeyAuthDetected
                    ? $"🚨 CRITICAL: MALICIOUS DROPPER (CAMOUFLAGED IN {result.DetectedAuthName})"
                    : "🚨 CRITICAL: CONFIRMED PAYLOAD DROPPER MALWARE";
            }
            else if (result.HasValidatedEmbeddedPE)
            {
                result.ThreatVerdict = "🚨 SUSPICIOUS: VALIDATED EMBEDDED PE EXECUTABLE IN OVERLAY";
            }
            else if (result.IsKeyAuthDetected && result.RiskScore <= 35)
            {
                string authLabel = !string.IsNullOrEmpty(result.DetectedAuthName) ? result.DetectedAuthName : "Licensed";
                result.ThreatVerdict = $"🛡️ SAFE / VERIFIED LICENSED SOFTWARE ({authLabel})";
            }
            else if (result.IsDigitallySigned && result.IsAuthenticodeValid && result.RiskScore == 0)
            {
                result.ThreatVerdict = $"✅ VERIFIED DIGITALLY SIGNED APPLICATION ({result.CertificateSigner})";
            }
            else if (result.HasWritableExecutableSections)
            {
                result.ThreatVerdict = "⚠️ HIGH RISK: MEMORY INJECTION RISK (WRITABLE + EXECUTABLE SECTIONS)";
            }
            else if (result.RiskScore >= 60)
            {
                result.ThreatVerdict = "⚠️ HIGH RISK / SUSPICIOUS PACKED EXECUTABLE";
            }
            else if (result.RiskScore >= 20 || result.IsPackedOrEncrypted || result.DetectedDownloaderAPIs.Count > 0)
            {
                result.ThreatVerdict = "⚠️ SUSPICIOUS: PACKED BINARY";
            }
            else
            {
                result.ThreatVerdict = "✅ CLEAN / SAFE EXECUTABLE";
            }
        }
    }
}
