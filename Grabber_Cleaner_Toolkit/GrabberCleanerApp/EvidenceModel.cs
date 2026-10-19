using System;
using System.Collections.Generic;

namespace GrabberCleanerApp
{
    public enum EvidenceCategory
    {
        PE,
        IMPORT,
        MEMORY,
        INJECTION,
        CREDENTIAL,
        TOKEN,
        BROWSER,
        WALLET,
        NETWORK,
        EXFILTRATION,
        DOWNLOADER,
        DROPPER,
        PERSISTENCE,
        OBFUSCATION,
        DOTNET,
        RESOURCE,
        EMBEDDED_PAYLOAD,
        SIGNATURE,
        AUTHENTICATION,
        FILESYSTEM,
        PROCESS
    }

    public enum EvidenceSeverity
    {
        Info,
        Low,
        Medium,
        High,
        Critical
    }

    public enum ThreatVerdict
    {
        CLEAN,
        LOW_RISK,
        SUSPICIOUS,
        HIGH_RISK,
        MALICIOUS,
        UNABLE_TO_ANALYZE
    }

    public enum ThreatClassification
    {
        NONE,
        STEALER,
        GRABBER,
        DOWNLOADER,
        DROPPER,
        RAT,
        CREDENTIAL_THEFT,
        TOKEN_THEFT,
        EXFILTRATION,
        PERSISTENCE,
        INJECTION,
        PACKED,
        OBFUSCATED,
        UNKNOWN_SUSPICIOUS
    }

    public class EvidenceItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
        public EvidenceCategory Category { get; set; }
        public string Indicator { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public EvidenceSeverity Severity { get; set; } = EvidenceSeverity.Info;
        public int Confidence { get; set; } = 0; // 0 to 100%
        public string Location { get; set; } = string.Empty;
        public long Offset { get; set; } = 0;
        public List<string> RelatedEvidenceIds { get; set; } = new List<string>();
        public string AnalyzerName { get; set; } = string.Empty;
        public bool IsBehavioral { get; set; } = false;
        public bool IsIndependent { get; set; } = true;
        public bool IsMitigating { get; set; } = false;

        public override string ToString()
        {
            string prefix = IsMitigating ? "🛡️ [MITIGATING]" : $"[{Severity.ToString().ToUpper()}]";
            return $"{prefix} {Indicator}: {Description} (Confidence: {Confidence}%)";
        }
    }

    public class PESectionInfo
    {
        public string Name { get; set; } = string.Empty;
        public uint VirtualSize { get; set; }
        public uint VirtualAddress { get; set; }
        public uint RawSize { get; set; }
        public uint RawOffset { get; set; }
        public uint Characteristics { get; set; }
        public double Entropy { get; set; }
        public bool IsExecutable => (Characteristics & 0x20000000) != 0;
        public bool IsWritable => (Characteristics & 0x80000000) != 0;
        public bool IsWritableAndExecutable => IsExecutable && IsWritable;
    }

    public class FileInspectionResult
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; } = 0;
        public string FileTypeDescription { get; set; } = "Unknown Binary";
        public double Entropy { get; set; } = 0.0;
        public bool IsPackedOrEncrypted { get; set; } = false;
        public bool IsDotNetAssembly { get; set; } = false;
        public bool IsPyInstallerBundle { get; set; } = false;
        public bool HasPEOverlayData { get; set; } = false;
        public long OverlaySizeBytes { get; set; } = 0;
        public bool HasValidatedEmbeddedPE { get; set; } = false;
        public int EmbeddedPEOffset { get; set; } = 0;
        public bool IsKeyAuthDetected { get; set; } = false;
        public string? DetectedAuthName { get; set; } = null;
        public List<string> DetectedAuthFrameworks { get; set; } = new List<string>();
        public bool IsConfirmedDropper { get; set; } = false;
        public bool IsConfirmedStealer { get; set; } = false;

        // Hash Identification
        public string Sha256Hash { get; set; } = string.Empty;
        public string Sha1Hash { get; set; } = string.Empty;
        public string Md5Hash { get; set; } = string.Empty;

        // Evidence & Authenticode Engine
        public bool IsDigitallySigned { get; set; } = false;
        public string? CertificateSigner { get; set; } = null;
        public bool IsAuthenticodeValid { get; set; } = false;
        public bool HasWritableExecutableSections { get; set; } = false;
        public int ConfidenceScore { get; set; } = 0; // Starts at 0% (Dynamic, non-hardcoded!)
        public List<PESectionInfo> PESections { get; set; } = new List<PESectionInfo>();
        public List<string> CorrelatedEvidence { get; set; } = new List<string>();
        public List<string> MitigatingFactors { get; set; } = new List<string>();

        // Structured Evidence Model
        public List<EvidenceItem> StructuredEvidences { get; set; } = new List<EvidenceItem>();

        // API Behavioral Categories
        public List<string> ImportedAPIs { get; set; } = new List<string>();
        public List<string> ProcessInjectionAPIs { get; set; } = new List<string>();
        public List<string> MemoryAPIs { get; set; } = new List<string>();
        public List<string> CredentialAPIs { get; set; } = new List<string>();
        public List<string> PersistenceAPIs { get; set; } = new List<string>();
        public List<string> DetectedDownloaderAPIs { get; set; } = new List<string>();

        // String & Webhook Intelligence
        public List<string> ExtractedWebhooksAndUrls { get; set; } = new List<string>();
        public List<string> DecodedBase64Webhooks { get; set; } = new List<string>();
        public List<string> ExtractedDropperPayloadUrls { get; set; } = new List<string>();
        public List<string> MatchedSuspiciousSignatures { get; set; } = new List<string>();
        public List<string> TechnicalAnalysisNotes { get; set; } = new List<string>();

        public int RiskScore { get; set; } = 0; // 0 to 100
        public string ThreatVerdict { get; set; } = "CLEAN";
        public ThreatClassification Classification { get; set; } = ThreatClassification.NONE;

        public string FormattedSize => FileSizeBytes switch
        {
            < 1024 => $"{FileSizeBytes} Bytes",
            < 1024 * 1024 => $"{FileSizeBytes / 1024.0:F2} KB",
            _ => $"{FileSizeBytes / (1024.0 * 1024.0):F2} MB"
        };
    }
}
