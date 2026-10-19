using System;

namespace GrabberCleanerApp
{
    public enum ThreatSeverity
    {
        Critical,
        High,
        Medium,
        Low
    }

    public enum ThreatCategory
    {
        MSBuildInfection,
        TempPayload,
        ScriptDropper,
        SuspiciousProcess,
        RegistryPersistence,
        ScheduledTask,
        WMIPersistence,
        DiscordInjection,
        HostsFileTampering,
        C2Connection
    }

    public enum ThreatStatus
    {
        Detected,
        Cleaned,
        Ignored,
        FailedToClean
    }

    public class ThreatInfo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
        public ThreatCategory Category { get; set; }
        public string ThreatName { get; set; } = string.Empty;
        public string TargetPath { get; set; } = string.Empty;
        public int LineNumber { get; set; } = 0;
        public string MalwareSignature { get; set; } = string.Empty;
        public string InjectedSnippet { get; set; } = string.Empty;
        public string DecodedUrl { get; set; } = string.Empty;
        public string ExtractedPayloadInfo { get; set; } = string.Empty;
        public string RootCauseExplanation { get; set; } = string.Empty;
        public ThreatSeverity Severity { get; set; } = ThreatSeverity.Critical;
        public int ConfidenceScore { get; set; } = 85; // 0 to 100%
        public List<string> SupportingEvidence { get; set; } = new List<string>();
        public List<string> MitigatingFactors { get; set; } = new List<string>();
        public ThreatStatus Status { get; set; } = ThreatStatus.Detected;
        public DateTime DetectionTime { get; set; } = DateTime.Now;

        public string SeverityText => Severity switch
        {
            ThreatSeverity.Critical => "CRITICAL",
            ThreatSeverity.High => "HIGH",
            ThreatSeverity.Medium => "MEDIUM",
            _ => "LOW"
        };

        public string CategoryText => Category switch
        {
            ThreatCategory.MSBuildInfection => "MSBuild / Project Vector",
            ThreatCategory.TempPayload => "Temp / AppData Binary Payload",
            ThreatCategory.ScriptDropper => "Script Dropper (.vbs/.ps1/.bat/.lnk)",
            ThreatCategory.SuspiciousProcess => "Active Process Payload",
            ThreatCategory.RegistryPersistence => "Registry Autorun Persistence",
            ThreatCategory.ScheduledTask => "Scheduled Task Hook",
            ThreatCategory.WMIPersistence => "WMI Event Consumer Persistence",
            ThreatCategory.DiscordInjection => "Discord Client Token Stealer",
            ThreatCategory.HostsFileTampering => "Hosts File Security Block",
            ThreatCategory.C2Connection => "Active C2 Stealer Connection",
            _ => "Unknown"
        };
    }
}
