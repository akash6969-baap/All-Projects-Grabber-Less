using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace GrabberCleanerApp
{
    public static class ExeInspectorEngine
    {
        public static FileInspectionResult InspectFile(string filePath)
        {
            var result = new FileInspectionResult
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath)
            };

            var evidenceList = new List<EvidenceItem>();

            // ── STAGE 1: FILE VALIDATION & IDENTITY ──────────────────────────────
            if (!File.Exists(filePath))
            {
                result.ThreatVerdict = "ERROR: FILE NOT FOUND";
                result.TechnicalAnalysisNotes.Add("[X] File does not exist at path: " + filePath);
                return result;
            }

            FileInfo fi;
            try
            {
                fi = new FileInfo(filePath);
                result.FileSizeBytes = fi.Length;
            }
            catch (Exception ex)
            {
                result.ThreatVerdict = $"ERROR: FILE ACCESS ERROR ({ex.Message})";
                return result;
            }

            // Cryptographic Hashes (SHA-256, SHA-1, MD5)
            ComputeHashes(filePath, result);

            // Bounded Read (Up to 64MB for safe memory footprint)
            byte[] fileBytes;
            try
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                int bytesToRead = (int)Math.Min(fi.Length, 64 * 1024 * 1024);
                fileBytes = new byte[bytesToRead];
                int read = fs.Read(fileBytes, 0, bytesToRead);
                if (read < bytesToRead)
                {
                    Array.Resize(ref fileBytes, read);
                }
            }
            catch (Exception ex)
            {
                result.ThreatVerdict = $"ERROR: COULD NOT READ FILE ({ex.Message})";
                result.TechnicalAnalysisNotes.Add("[X] Could not read file stream: " + ex.Message);
                return result;
            }

            // ── STAGE 2: DIGITAL SIGNATURE & AUTHENTICODE ────────────────────────
            CheckDigitalSignature(filePath, result, evidenceList);

            // ── STAGE 3: PE STRUCTURE & SECTION PARSING ──────────────────────────
            PeAnalyzer.Analyze(fileBytes, result, evidenceList);

            // ── STAGE 4: SHANNON ENTROPY ─────────────────────────────────────────
            result.Entropy = CalculateEntropy(fileBytes);
            if (result.Entropy > 7.15)
            {
                result.IsPackedOrEncrypted = true;
                result.TechnicalAnalysisNotes.Add($"⚠️ High Binary Entropy ({result.Entropy:F2}/8.00). Binary is compressed, packed, or encrypted.");
                result.CorrelatedEvidence.Add($"High Entropy ({result.Entropy:F2}/8.00) indicates packing or encrypted payload");

                evidenceList.Add(new EvidenceItem
                {
                    Category = EvidenceCategory.OBFUSCATION,
                    Indicator = "High Whole-File Entropy",
                    Description = $"Binary has entropy of {result.Entropy:F2}/8.00 indicating compression or encryption.",
                    Severity = EvidenceSeverity.Low,
                    Confidence = 60,
                    AnalyzerName = nameof(ExeInspectorEngine)
                });
            }
            else
            {
                result.TechnicalAnalysisNotes.Add($"✔ Binary Entropy is Normal ({result.Entropy:F2}/8.00).");
            }

            // ── STAGE 5: .NET STATIC ANALYSIS ────────────────────────────────────
            DotNetAnalyzer.Analyze(fileBytes, result, evidenceList);

            // ── STAGE 6: IMPORT ADDRESS TABLE & NATIVE API GROUPS ────────────────
            ImportAnalyzer.Analyze(fileBytes, result, evidenceList);

            // ── STAGE 7: STRING INTELLIGENCE & DE-OBFUSCATION ────────────────────
            StringAnalyzer.Analyze(fileBytes, result, evidenceList);

            // ── STAGE 8: RECURSIVE EMBEDDED PAYLOAD ANALYSIS ─────────────────────
            EmbeddedPayloadAnalyzer.Analyze(fileBytes, result, evidenceList);

            // ── STAGE 9: BEHAVIORAL CHAIN CORRELATION ENGINE ─────────────────────
            CorrelationEngine.Correlate(result, evidenceList);

            // ── STAGE 10: DYNAMIC RISK & CONFIDENCE ENGINE ───────────────────────
            RiskConfidenceEngine.Calculate(result, evidenceList);

            // Save structured evidence items into result
            result.StructuredEvidences = evidenceList;

            return result;
        }

        private static void ComputeHashes(string filePath, FileInspectionResult result)
        {
            try
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                using (var sha256 = SHA256.Create())
                {
                    byte[] hash = sha256.ComputeHash(fs);
                    result.Sha256Hash = Convert.ToHexString(hash).ToLowerInvariant();
                }

                fs.Position = 0;
                using (var sha1 = SHA1.Create())
                {
                    byte[] hash = sha1.ComputeHash(fs);
                    result.Sha1Hash = Convert.ToHexString(hash).ToLowerInvariant();
                }

                fs.Position = 0;
                using (var md5 = MD5.Create())
                {
                    byte[] hash = md5.ComputeHash(fs);
                    result.Md5Hash = Convert.ToHexString(hash).ToLowerInvariant();
                }

                result.TechnicalAnalysisNotes.Add($"🔑 SHA-256: {result.Sha256Hash}");
                result.TechnicalAnalysisNotes.Add($"🔑 SHA-1  : {result.Sha1Hash}");
                result.TechnicalAnalysisNotes.Add($"🔑 MD5    : {result.Md5Hash}");
            }
            catch (Exception ex)
            {
                result.TechnicalAnalysisNotes.Add($"[!] Could not compute full hashes: {ex.Message}");
            }
        }

        private static void CheckDigitalSignature(string filePath, FileInspectionResult result, List<EvidenceItem> evidenceList)
        {
            try
            {
#pragma warning disable SYSLIB0057
                using var cert = System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromSignedFile(filePath);
#pragma warning restore SYSLIB0057
                if (cert != null)
                {
                    result.IsDigitallySigned = true;
                    using var cert2 = new System.Security.Cryptography.X509Certificates.X509Certificate2(cert);
                    result.IsAuthenticodeValid = cert2.Verify();
                    string signer = cert2.GetNameInfo(System.Security.Cryptography.X509Certificates.X509NameType.SimpleName, false) ?? "Signed Binary";
                    result.CertificateSigner = signer;

                    bool isMajorVendor = signer.Contains("Microsoft", StringComparison.OrdinalIgnoreCase) ||
                                         signer.Contains("Valve", StringComparison.OrdinalIgnoreCase) ||
                                         signer.Contains("Google", StringComparison.OrdinalIgnoreCase) ||
                                         signer.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase);

                    string vendorTag = isMajorVendor ? " (Trusted Major Vendor)" : "";
                    result.TechnicalAnalysisNotes.Add($"🛡️ Digital Signature: '{signer}'{vendorTag} | Authenticode: {(result.IsAuthenticodeValid ? "VERIFIED VALID" : "SELF-SIGNED / UNVERIFIED")}.");

                    evidenceList.Add(new EvidenceItem
                    {
                        Category = EvidenceCategory.SIGNATURE,
                        Indicator = $"Digital Signature: {signer}",
                        Description = $"Digitally signed by '{signer}' (Authenticode: {(result.IsAuthenticodeValid ? "Valid" : "Unverified")}).",
                        Severity = EvidenceSeverity.Info,
                        Confidence = 95,
                        IsMitigating = result.IsAuthenticodeValid,
                        AnalyzerName = nameof(ExeInspectorEngine)
                    });
                }
            }
            catch
            {
                result.TechnicalAnalysisNotes.Add("ℹ️ Binary is unsigned (Standard for indie, opensource, or custom tools).");
            }
        }

        private static double CalculateEntropy(byte[] bytes)
        {
            if (bytes.Length == 0) return 0.0;

            int[] map = new int[256];
            foreach (byte b in bytes) map[b]++;

            double entropy = 0;
            double len = bytes.Length;

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
