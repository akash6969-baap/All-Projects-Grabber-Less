using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GrabberCleanerApp
{
    public partial class InspectionResultForm : Form
    {
        private FileInspectionResult _result;

        public InspectionResultForm(FileInspectionResult result)
        {
            InitializeComponent();
            _result = result;
        }

        private void InspectionResultForm_Load(object sender, EventArgs e)
        {
            lblFileName.Text = _result.FileName;
            string authBadge = _result.IsKeyAuthDetected ? $" | 🛡️ {(_result.DetectedAuthName ?? "Licensed Auth")}" : "";
            string sigBadge = _result.IsDigitallySigned ? $" | ✍️ {_result.CertificateSigner}" : "";
            lblFileType.Text = _result.FileTypeDescription + authBadge + sigBadge;
            lblFileSize.Text = _result.FormattedSize;
            lblEntropy.Text = $"{_result.Entropy:F2} / 8.00 {(_result.IsPackedOrEncrypted ? "(PACKED / ENCRYPTED)" : "(NORMAL)")}";
            lblRiskScore.Text = $"{_result.RiskScore}% (Confidence: {_result.ConfidenceScore}%)";
            lblVerdict.Text = _result.ThreatVerdict;

            if (_result.IsConfirmedStealer || _result.IsConfirmedDropper || _result.HasValidatedEmbeddedPE || _result.RiskScore >= 50)
            {
                pnlVerdictHeader.BackColor = Color.FromArgb(190, 35, 35);
                lblVerdict.ForeColor = Color.White;
            }
            else if (_result.IsKeyAuthDetected && !_result.IsConfirmedStealer && !_result.IsConfirmedDropper)
            {
                pnlVerdictHeader.BackColor = Color.FromArgb(40, 120, 180);
                lblVerdict.ForeColor = Color.White;
            }
            else if (_result.RiskScore >= 20 || _result.DetectedDownloaderAPIs.Count > 0 || _result.IsPackedOrEncrypted)
            {
                pnlVerdictHeader.BackColor = Color.FromArgb(210, 140, 20);
                lblVerdict.ForeColor = Color.Black;
            }
            else
            {
                pnlVerdictHeader.BackColor = Color.FromArgb(40, 150, 70);
                lblVerdict.ForeColor = Color.White;
            }

            // Proof Items: Exfiltration Webhooks List
            if (_result.ExtractedWebhooksAndUrls.Count > 0)
            {
                foreach (var url in _result.ExtractedWebhooksAndUrls)
                {
                    lstUrls.Items.Add($"🚨 EXFILTRATION HOOK: {url}");
                }
            }

            // Proof Items: Dropper Payload URLs List
            if (_result.ExtractedDropperPayloadUrls.Count > 0)
            {
                foreach (var durl in _result.ExtractedDropperPayloadUrls)
                {
                    lstUrls.Items.Add($"📥 DROPPER DOWNLOAD TARGET: {durl}");
                }
            }

            // Proof Items: Validated Secondary Embedded PE Offset
            if (_result.HasValidatedEmbeddedPE)
            {
                lstUrls.Items.Add($"🚨 EMBEDDED PE EXECUTABLE: Validated PE NT Header found at Offset 0x{_result.EmbeddedPEOffset:X}");
            }

            if (_result.ExtractedWebhooksAndUrls.Count == 0 && _result.ExtractedDropperPayloadUrls.Count == 0 && !_result.HasValidatedEmbeddedPE)
            {
                lstUrls.Items.Add("ℹ️ No hardcoded Webhooks, C2 Exfiltration URLs, or Payload Download Links extracted from binary strings.");
            }

            // Downloader & Stealer Signatures List
            if (_result.DetectedDownloaderAPIs.Count > 0)
            {
                foreach (var api in _result.DetectedDownloaderAPIs)
                {
                    lstKeywords.Items.Add($"📥 Downloader API: {api}");
                }
            }

            if (_result.MatchedSuspiciousSignatures.Count > 0)
            {
                foreach (var sig in _result.MatchedSuspiciousSignatures)
                {
                    lstKeywords.Items.Add($"⚠️ Stealer Target Indicator: {sig}");
                }
            }

            if (_result.DetectedDownloaderAPIs.Count == 0 && _result.MatchedSuspiciousSignatures.Count == 0)
            {
                lstKeywords.Items.Add("✔ No suspicious downloader APIs or stealer targets matched.");
            }

            // Correlated Evidence Items
            if (_result.CorrelatedEvidence.Count > 0)
            {
                foreach (var ev in _result.CorrelatedEvidence)
                {
                    lstKeywords.Items.Add($"🔬 EVIDENCE: {ev}");
                }
            }

            // Structured Behavioral Evidence Items
            if (_result.StructuredEvidences.Count > 0)
            {
                txtAnalysisNotes.AppendText($"--- STRUCTURED FORENSIC EVIDENCE ({_result.StructuredEvidences.Count}) ---{Environment.NewLine}");
                foreach (var ev in _result.StructuredEvidences)
                {
                    txtAnalysisNotes.AppendText($"{ev}{Environment.NewLine}");
                }
                txtAnalysisNotes.AppendText(Environment.NewLine);
            }

            // Technical Analysis Notes
            foreach (var note in _result.TechnicalAnalysisNotes)
            {
                txtAnalysisNotes.AppendText($"{note}{Environment.NewLine}");
            }

            // PE Section Information
            if (_result.PESections.Count > 0)
            {
                txtAnalysisNotes.AppendText($"{Environment.NewLine}--- PE SECTIONS ANALYSIS ({_result.PESections.Count}) ---{Environment.NewLine}");
                foreach (var sec in _result.PESections)
                {
                    string secWarn = sec.IsWritableAndExecutable ? " [🚨 W^X VIOLATION]" : (sec.Entropy > 7.2 ? " [PACKED/ENCRYPTED]" : "");
                    txtAnalysisNotes.AppendText($"Section '{sec.Name}': VirtSize={sec.VirtualSize}, RawSize={sec.RawSize}, Entropy={sec.Entropy:F2}/8.0{secWarn}{Environment.NewLine}");
                }
            }
        }

        private void btnDeleteFile_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show(
                $"Are you sure you want to PERMANENTLY DELETE this file?\n\nFile: {_result.FilePath}",
                "Confirm File Annihilation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    if (File.Exists(_result.FilePath))
                    {
                        File.Delete(_result.FilePath);
                        MessageBox.Show("✅ File successfully annihilated and permanently deleted!", "File Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to delete file: {ex.Message}", "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
