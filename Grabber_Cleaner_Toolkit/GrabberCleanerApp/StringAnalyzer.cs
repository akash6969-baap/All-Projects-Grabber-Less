using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GrabberCleanerApp
{
    public static class StringAnalyzer
    {
        private static readonly string[] StealerIndicators = new[]
        {
            "Local Storage\\leveldb", "Chrome\\User Data", "Login Data", "Web Data",
            "CryptUnprotectData", "passwords.txt", "tokens.txt", "discordcanary",
            "discordptb", "solana", "metamask", "exodus", "brave-browser",
            "opera software", "Atomic Wallet", "Electrum", "discord_desktop_core"
        };

        private static readonly string[] LegitAuthFrameworks = new[]
        {
            "KeyAuth", "FearAuth", "Auth.GG", "Authon", "PWF Auth",
            "LicenseSeat", "qPapel Auth", "EasyAuth", "LibreAuth",
            "SentriNet", "System Locker", "Cryptolens", "Keygen",
            "LicenseSpring", "LimeLM", "WyDay", "10Duke",
            "Nalpeiron", "Reprise", "Flexera", "Thales Sentinel",
            "Sentinel LDK", "Sentinel HASP", "NetLicensing", "License4J",
            "SoftwareKey", "Cryptlex", "LicenseManager", "license_auth",
            "VMPROTECT", "Themida", "CodeMeter", "WIBU-SYSTEMS"
        };

        private static readonly string[] LegitAuthDomains = new[]
        {
            "keyauth.win", "keyauth.cc", "keyauth.uk", "fearauth.com", "fearauth.net",
            "auth.gg", "authon.dev", "licenseseat.com", "cryptolens.io",
            "keygen.sh", "licensespring.com", "wyday.com", "10duke.com",
            "netlicensing.io", "license4j.com", "cryptlex.com"
        };

        public static void Analyze(byte[] bytes, FileInspectionResult result, List<EvidenceItem> evidenceList)
        {
            List<string> stringsList = new List<string>();

            // 1. Extract ASCII
            ExtractASCIIStrings(bytes, stringsList);

            // 2. Extract Unicode UTF-16
            try
            {
                string uStr = Encoding.Unicode.GetString(bytes);
                var uMatches = Regex.Matches(uStr, @"[A-Za-z0-9\s:/\.\-_]{6,}");
                foreach (Match m in uMatches) stringsList.Add(m.Value);
            }
            catch { }

            // 3. De-obfuscate Base64
            DeobfuscateBase64Strings(stringsList, result, evidenceList);

            // 4. Decode Reversed Strings
            DecodeReversedStrings(stringsList, result, evidenceList);

            // 5. Categorize and evaluate strings
            HashSet<string> webhooks = new HashSet<string>(result.ExtractedWebhooksAndUrls);
            HashSet<string> dropperUrls = new HashSet<string>();
            HashSet<string> matchedSigs = new HashSet<string>();

            foreach (string str in stringsList)
            {
                // Legit Auth Frameworks
                foreach (var auth in LegitAuthFrameworks)
                {
                    if (str.Contains(auth, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!result.DetectedAuthFrameworks.Contains(auth))
                        {
                            result.DetectedAuthFrameworks.Add(auth);
                        }
                        result.IsKeyAuthDetected = true;
                    }
                }

                // URL Extraction & Classification
                var urlMatches = Regex.Matches(str, @"https?://[^\s""'>]+", RegexOptions.IgnoreCase);
                foreach (Match m in urlMatches)
                {
                    string url = m.Value.Trim();

                    bool isAuthUrl = LegitAuthDomains.Any(domain => url.Contains(domain, StringComparison.OrdinalIgnoreCase));
                    if (isAuthUrl)
                    {
                        result.IsKeyAuthDetected = true;
                        continue;
                    }

                    // Discord Webhooks & Telegram Bot Endpoints
                    if (Regex.IsMatch(url, @"discord(app)?\.com/api/webhooks/\d+/[A-Za-z0-9_-]+", RegexOptions.IgnoreCase) ||
                        Regex.IsMatch(url, @"api\.telegram\.org/bot\d+:[A-Za-z0-9_-]+", RegexOptions.IgnoreCase) ||
                        url.Contains("catbox.moe") || url.Contains("transfer.sh") || url.Contains("gofile.io") ||
                        url.Contains("anonfiles.com") || url.Contains("pee-files.nl") || url.Contains("temp.sh"))
                    {
                        webhooks.Add(url);

                        evidenceList.Add(new EvidenceItem
                        {
                            Category = EvidenceCategory.EXFILTRATION,
                            Indicator = "Exfiltration Webhook / C2 URL",
                            Description = $"Found hardcoded exfiltration channel: {url}",
                            Severity = EvidenceSeverity.High,
                            Confidence = 90,
                            Location = url,
                            IsBehavioral = true,
                            AnalyzerName = nameof(StringAnalyzer)
                        });
                    }
                    else if (url.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                             url.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
                             url.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase) ||
                             url.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) ||
                             url.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ||
                             url.Contains("raw.githubusercontent.com"))
                    {
                        dropperUrls.Add(url);

                        evidenceList.Add(new EvidenceItem
                        {
                            Category = EvidenceCategory.DOWNLOADER,
                            Indicator = "Remote Payload URL",
                            Description = $"Contains direct remote download target: {url}",
                            Severity = EvidenceSeverity.Medium,
                            Confidence = 70,
                            Location = url,
                            AnalyzerName = nameof(StringAnalyzer)
                        });
                    }
                }

                // Stealer Targets
                foreach (var st in StealerIndicators)
                {
                    if (str.Contains(st, StringComparison.OrdinalIgnoreCase))
                    {
                        matchedSigs.Add(st);
                    }
                }
            }

            foreach (var st in matchedSigs)
            {
                evidenceList.Add(new EvidenceItem
                {
                    Category = EvidenceCategory.CREDENTIAL,
                    Indicator = $"Stealer Target: {st}",
                    Description = $"References credential database or token storage: '{st}'",
                    Severity = EvidenceSeverity.Medium,
                    Confidence = 75,
                    IsBehavioral = true,
                    AnalyzerName = nameof(StringAnalyzer)
                });
            }

            if (result.DetectedAuthFrameworks.Count > 0)
            {
                result.DetectedAuthName = string.Join(", ", result.DetectedAuthFrameworks.Take(3));

                evidenceList.Add(new EvidenceItem
                {
                    Category = EvidenceCategory.AUTHENTICATION,
                    Indicator = $"Licensing Framework: {result.DetectedAuthName}",
                    Description = $"Identified legitimate commercial licensing/protection framework: {result.DetectedAuthName}",
                    Severity = EvidenceSeverity.Info,
                    Confidence = 90,
                    IsMitigating = true,
                    AnalyzerName = nameof(StringAnalyzer)
                });
            }

            result.ExtractedWebhooksAndUrls = webhooks.Distinct().ToList();
            result.ExtractedDropperPayloadUrls = dropperUrls.Distinct().ToList();
            result.MatchedSuspiciousSignatures = matchedSigs.Distinct().ToList();
        }

        private static void DeobfuscateBase64Strings(List<string> stringsList, FileInspectionResult result, List<EvidenceItem> evidenceList)
        {
            foreach (var str in stringsList.Where(s => s.Length >= 24 && s.Length <= 500))
            {
                if (!Regex.IsMatch(str, @"^[A-Za-z0-9+/=]+$")) continue;

                try
                {
                    byte[] decoded = Convert.FromBase64String(str);
                    string decodedStr = Encoding.UTF8.GetString(decoded);

                    if (decodedStr.Contains("discord.com/api/webhooks", StringComparison.OrdinalIgnoreCase) ||
                        decodedStr.Contains("api.telegram.org/bot", StringComparison.OrdinalIgnoreCase) ||
                        decodedStr.Contains("catbox.moe", StringComparison.OrdinalIgnoreCase))
                    {
                        result.DecodedBase64Webhooks.Add(decodedStr);
                        result.ExtractedWebhooksAndUrls.Add(decodedStr);
                        result.TechnicalAnalysisNotes.Add($"🚨 DE-OBFUSCATED BASE64 C2 WEBHOOK: {decodedStr}");
                        result.CorrelatedEvidence.Add("De-obfuscated Base64 Discord/Telegram Webhook C2");

                        evidenceList.Add(new EvidenceItem
                        {
                            Category = EvidenceCategory.EXFILTRATION,
                            Indicator = "De-obfuscated Base64 Webhook",
                            Description = $"Decoded hidden C2 Webhook from Base64: {decodedStr}",
                            Severity = EvidenceSeverity.Critical,
                            Confidence = 95,
                            IsBehavioral = true,
                            AnalyzerName = nameof(StringAnalyzer)
                        });
                    }
                }
                catch { }
            }
        }

        private static void DecodeReversedStrings(List<string> stringsList, FileInspectionResult result, List<EvidenceItem> evidenceList)
        {
            foreach (var str in stringsList.Where(s => s.Length >= 20 && s.Length <= 250))
            {
                if (str.Contains("skooheb/ipa/moc.drocsid", StringComparison.OrdinalIgnoreCase) ||
                    str.Contains("tob/gro.margelet", StringComparison.OrdinalIgnoreCase))
                {
                    char[] charArray = str.ToCharArray();
                    Array.Reverse(charArray);
                    string reversed = new string(charArray);

                    result.ExtractedWebhooksAndUrls.Add(reversed);
                    result.TechnicalAnalysisNotes.Add($"🚨 DE-OBFUSCATED REVERSED WEBHOOK C2: {reversed}");
                    result.CorrelatedEvidence.Add("De-obfuscated Reversed Exfiltration Webhook");

                    evidenceList.Add(new EvidenceItem
                    {
                        Category = EvidenceCategory.EXFILTRATION,
                        Indicator = "De-obfuscated Reversed Webhook",
                        Description = $"Decoded hidden reverse-string Webhook: {reversed}",
                        Severity = EvidenceSeverity.Critical,
                        Confidence = 95,
                        IsBehavioral = true,
                        AnalyzerName = nameof(StringAnalyzer)
                    });
                }
            }
        }

        private static void ExtractASCIIStrings(byte[] bytes, List<string> outStrings)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                byte b = bytes[i];
                if (b >= 32 && b <= 126)
                {
                    sb.Append((char)b);
                }
                else
                {
                    if (sb.Length >= 4) outStrings.Add(sb.ToString());
                    sb.Clear();
                }
            }
            if (sb.Length >= 4) outStrings.Add(sb.ToString());
        }
    }
}
