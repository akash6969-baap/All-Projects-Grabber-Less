using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GrabberCleanerApp
{
    public partial class MainForm : Form
    {
        private ScannerEngine _engine;
        private List<ThreatInfo> _detectedThreats;

        public MainForm()
        {
            InitializeComponent();
            _engine = new ScannerEngine();
            _detectedThreats = new List<ThreatInfo>();

            _engine.OnLog += Engine_OnLog;
            _engine.OnProgress += Engine_OnProgress;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LogToConsole("🚀 Next-Level Grabber & MSBuild Annihilator Suite Initialized.");
            LogToConsole("🛡️ Protection Modules: PreBuild Base64 Decoders, GitHub EXE Hunter, Script Dropper Purger (.vbs/.ps1/.bat), WMI Hunter & Discord Stealer Cleaner.");
            UpdateStats();
        }

        private void Engine_OnLog(string message, bool isWarning, bool isError)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Engine_OnLog(message, isWarning, isError)));
                return;
            }

            LogToConsole(message, isWarning ? Color.Orange : (isError ? Color.Salmon : Color.LightGray));
        }

        private void Engine_OnProgress(int percentage, string statusText)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Engine_OnProgress(percentage, statusText)));
                return;
            }

            progressBar.Value = Math.Clamp(percentage, 0, 100);
            lblStatusText.Text = statusText;
        }

        private void LogToConsole(string message, Color? color = null)
        {
            txtConsole.SelectionStart = txtConsole.TextLength;
            txtConsole.SelectionLength = 0;
            txtConsole.SelectionColor = color ?? Color.LightGray;
            txtConsole.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            txtConsole.ScrollToCaret();
        }

        private async void btnFullScan_Click(object sender, EventArgs e)
        {
            SetControlsEnabled(false);
            gridThreats.Rows.Clear();
            _detectedThreats.Clear();
            UpdateStats();

            LogToConsole("🔥 Launching ULTIMATE FULL SYSTEM ANNIHILATION SCAN...", Color.Cyan);

            _detectedThreats = await Task.Run(() => _engine.RunFullScan());

            PopulateThreatGrid();
            UpdateStats();
            SetControlsEnabled(true);

            if (_detectedThreats.Count > 0)
            {
                MessageBox.Show($"🚨 CRITICAL ALERTS DETECTED!\n\nFound {_detectedThreats.Count} grabber threat(s), droppers, or persistent hooks on your system!\n\nClick '🧹 1-CLICK CLEAN & UDANA' to permanently annihilate them.", "Threats Detected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show("✅ 100% CLEAN & PROTECTED!\n\nNo MSBuild grabbers, GitHub droppers, VBS scripts, or stealer persistence found on your system.", "System Clean", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void btnScanFolder_Click(object sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog
            {
                Description = "📂 Select C#, C++, Python or any Project Folder to Deep Scan",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = false
            };

            if (fbd.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(fbd.SelectedPath))
            {
                SetControlsEnabled(false);
                gridThreats.Rows.Clear();
                _detectedThreats.Clear();
                UpdateStats();

                string folderPath = fbd.SelectedPath;

                // Detect project type first
                string projectType = ScannerEngine.DetectProjectType(folderPath);
                LogToConsole($"📂 PROJECT SELECTED: {folderPath}", Color.Orange);
                LogToConsole($"🔬 DETECTED PROJECT TYPE: {projectType}", Color.Cyan);
                LogToConsole($"🔍 DEEP SCANNING ALL SOURCE FILES (.cs / .py / .cpp / .h / .js / .ts / .vb / .ps1 + project files)...", Color.Yellow);

                _detectedThreats = await Task.Run(() => _engine.ScanTargetProjectFolder(folderPath));

                PopulateThreatGrid();
                UpdateStats();
                SetControlsEnabled(true);

                if (_detectedThreats.Count > 0)
                {
                    MessageBox.Show($"🚨 INFECTED PROJECT DETECTED!\n\nProject Type: {projectType}\nFound {_detectedThreats.Count} threat(s) or malicious pattern(s) inside:\n{folderPath}\n\nClick '🧹 1-CLICK CLEAN & UDANA' to clean this project!", "Project Threat Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show($"✅ PROJECT 100% CLEAN!\n\nProject Type: {projectType}\nNo PreBuild grabbers, Base64 vectors, stealer source code, or script droppers found inside:\n{folderPath}", "Clean Project", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }


        private async void btnScanProjects_Click(object sender, EventArgs e)
        {
            SetControlsEnabled(false);
            gridThreats.Rows.Clear();
            _detectedThreats.Clear();
            UpdateStats();

            LogToConsole("🔍 Scanning MSBuild Targets, PreBuild Events & GitHub Links...", Color.Cyan);

            _detectedThreats = await Task.Run(() => _engine.ScanProjectsOnly());

            PopulateThreatGrid();
            UpdateStats();
            SetControlsEnabled(true);
        }

        private async void btnScanProcesses_Click(object sender, EventArgs e)
        {
            SetControlsEnabled(false);
            gridThreats.Rows.Clear();
            _detectedThreats.Clear();
            UpdateStats();

            LogToConsole("⚡ Launching 5-SECOND SMART SYSTEM SCAN (RAM, Persistence, Hooks & Hotspots)...", Color.LimeGreen);

            _detectedThreats = await Task.Run(() => _engine.RunQuickScan());

            PopulateThreatGrid();
            UpdateStats();
            SetControlsEnabled(true);

            if (_detectedThreats.Count > 0)
            {
                MessageBox.Show($"🚨 CRITICAL ALERTS DETECTED!\n\nFound {_detectedThreats.Count} grabber threat(s), droppers, or persistent hooks on your system!\n\nClick '🧹 1-CLICK CLEAN & UDANA' to permanently annihilate them.", "Threats Detected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show("✅ 100% CLEAN & PROTECTED!\n\nNo active memory stealers, droppers, registry autoruns, Discord hooks, or grabbers detected on your system.", "System Clean", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void btnHuntVSInfection_Click(object sender, EventArgs e)
        {
            SetControlsEnabled(false);
            gridThreats.Rows.Clear();
            _detectedThreats.Clear();
            UpdateStats();

            LogToConsole("🔨 Hunting Visual Studio & MSBuild Global Infection Chains...", Color.OrangeRed);

            _detectedThreats = await Task.Run(() => _engine.HuntVisualStudioAndMSBuildInfection());

            PopulateThreatGrid();
            UpdateStats();
            SetControlsEnabled(true);

            if (_detectedThreats.Count > 0)
            {
                MessageBox.Show($"🚨 VISUAL STUDIO INFECTIONS DETECTED!\n\nFound {_detectedThreats.Count} MSBuild/VS global infection vector(s)!\n\nReview them in the table and click '🧹 ANNIHILATE ALL THREATS' to clean them.", "VS Infection Detected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show("✅ VISUAL STUDIO & MSBUILD CLEAN!\n\nNo malicious Directory.Build hooks, injected PreBuild tasks, or compromised VS templates found.", "System Clean", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void btnTraceC2Droppers_Click(object sender, EventArgs e)
        {
            SetControlsEnabled(false);
            gridThreats.Rows.Clear();
            _detectedThreats.Clear();
            UpdateStats();

            LogToConsole("🕵️ Tracing Active C2 Sockets, Dropped Scripts (.vbs, .ps1, .bat) & Payloads...", Color.Turquoise);

            _detectedThreats = await Task.Run(() => _engine.TraceC2AndDroppers());

            PopulateThreatGrid();
            UpdateStats();
            SetControlsEnabled(true);

            if (_detectedThreats.Count > 0)
            {
                MessageBox.Show($"🚨 C2 DROPPER INFECTIONS DETECTED!\n\nFound {_detectedThreats.Count} dropped script(s), C2 network socket(s), or payload(s)!\n\nClick '🧹 ANNIHILATE ALL THREATS' to permanently remove them.", "C2 Threats Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show("✅ NO C2 DROPPERS DETECTED!\n\nNo active malicious C2 sockets, dropped .vbs/.ps1/.bat scripts, or background droppers found.", "System Clean", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnInspectExe_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "🎯 Select Executable or Script File to Inspect Deeply",
                Filter = "Executables & Scripts (*.exe;*.dll;*.scr;*.msi;*.vbs;*.ps1;*.bat;*.cmd;*.js)|*.exe;*.dll;*.scr;*.msi;*.vbs;*.ps1;*.bat;*.cmd;*.js|All Files (*.*)|*.*"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LogToConsole($"🎯 Running Deep Binary & String Inspection on: {ofd.FileName}...", Color.Magenta);

                var result = ExeInspectorEngine.InspectFile(ofd.FileName);

                using var form = new InspectionResultForm(result);
                form.ShowDialog(this);
            }
        }

        private async void btnCleanAll_Click(object sender, EventArgs e)
        {
            if (_detectedThreats.Count == 0)
            {
                MessageBox.Show("No threats currently detected to clean. Perform a scan first!", "No Threats", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Are you sure you want to PERMANENTLY ANNIHILATE ALL {_detectedThreats.Count} detected threat(s)?\n\n" +
                "✔ Creates safety backups (.bak) for project files.\n" +
                "✔ Strips malicious XML PreBuild & Base64 tags.\n" +
                "✔ Kills background stealer processes.\n" +
                "✔ Deletes .vbs / .ps1 / .bat script droppers & temp binaries.\n" +
                "✔ Purges malicious WMI Event Consumers & Registry keys.\n" +
                "✔ Restores clean Discord client index.js files & Windows Hosts file.",
                "Confirm Permanent Annihilation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes) return;

            SetControlsEnabled(false);
            LogToConsole("🧹 Executing Permanent Threat Annihilation...", Color.Gold);

            int cleanedCount = 0;

            await Task.Run(() =>
            {
                cleanedCount = _engine.CleanAllThreats(_detectedThreats);
            });

            PopulateThreatGrid();
            UpdateStats();
            SetControlsEnabled(true);

            MessageBox.Show($"🎉 TOTAL ANNIHILATION COMPLETE!\n\nSuccessfully destroyed {cleanedCount} threat(s). Your PC, project files, and Discord tokens are 100% safe!", "Annihilation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnExportReport_Click(object sender, EventArgs e)
        {
            if (_detectedThreats.Count == 0)
            {
                MessageBox.Show("No threats to export. Perform a scan first.", "Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "Text Document (*.txt)|*.txt|JSON Report (*.json)|*.json",
                FileName = $"Grabber_Annihilator_Report_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                using var writer = new StreamWriter(sfd.FileName);
                writer.WriteLine("==========================================================================");
                writer.WriteLine("   GRABBER & MSBUILD MALWARE ANNIHILATOR TOOLKIT — DIAGNOSTIC REPORT");
                writer.WriteLine($"   Generated: {DateTime.Now}");
                writer.WriteLine("==========================================================================");
                writer.WriteLine();

                foreach (var threat in _detectedThreats)
                {
                    writer.WriteLine($"[THREAT ID]  : {threat.Id}");
                    writer.WriteLine($"Status       : {threat.Status}");
                    writer.WriteLine($"Category     : {threat.CategoryText}");
                    writer.WriteLine($"Threat Name  : {threat.ThreatName}");
                    writer.WriteLine($"Target Path  : {threat.TargetPath}");
                    writer.WriteLine($"Line Number  : {threat.LineNumber}");
                    writer.WriteLine($"Signature    : {threat.MalwareSignature}");
                    writer.WriteLine($"Decoded URL  : {(string.IsNullOrEmpty(threat.DecodedUrl) ? "N/A" : threat.DecodedUrl)}");
                    writer.WriteLine($"Severity     : {threat.SeverityText}");
                    writer.WriteLine($"Root Cause   : {threat.RootCauseExplanation}");
                    writer.WriteLine($"Snippet      : {threat.InjectedSnippet}");
                    if (!string.IsNullOrEmpty(threat.ExtractedPayloadInfo))
                    {
                        writer.WriteLine($"Decoded Text : {threat.ExtractedPayloadInfo}");
                    }
                    writer.WriteLine(new string('-', 70));
                }

                MessageBox.Show($"Detailed diagnostic report saved to: {sfd.FileName}", "Report Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void PopulateThreatGrid()
        {
            gridThreats.Rows.Clear();

            foreach (var threat in _detectedThreats)
            {
                int rowIndex = gridThreats.Rows.Add(
                    threat.Status == ThreatStatus.Cleaned ? "✅ CLEANED" : "🚨 DETECTED",
                    threat.CategoryText,
                    threat.ThreatName,
                    threat.TargetPath,
                    string.IsNullOrEmpty(threat.DecodedUrl) ? threat.MalwareSignature : $"URL: {threat.DecodedUrl}",
                    threat.RootCauseExplanation,
                    threat.SeverityText
                );

                DataGridViewRow row = gridThreats.Rows[rowIndex];
                row.Tag = threat;

                if (threat.Status == ThreatStatus.Cleaned)
                {
                    row.DefaultCellStyle.ForeColor = Color.LightGreen;
                }
                else if (threat.Severity == ThreatSeverity.Critical)
                {
                    row.DefaultCellStyle.ForeColor = Color.Salmon;
                }
                else
                {
                    row.DefaultCellStyle.ForeColor = Color.Gold;
                }
            }
        }

        private void UpdateStats()
        {
            int total = _detectedThreats.Count;
            int critical = _detectedThreats.Count(t => t.Severity == ThreatSeverity.Critical);
            int cleaned = _detectedThreats.Count(t => t.Status == ThreatStatus.Cleaned);

            lblTotalThreatsValue.Text = total.ToString();
            lblCriticalValue.Text = critical.ToString();
            lblCleanedValue.Text = cleaned.ToString();

            if (total == 0)
            {
                lblSystemStatus.Text = "SYSTEM STATUS: SECURE & 100% CLEAN";
                lblSystemStatus.ForeColor = Color.LightGreen;
            }
            else if (cleaned == total)
            {
                lblSystemStatus.Text = "SYSTEM STATUS: ALL THREATS ANNIHILATED";
                lblSystemStatus.ForeColor = Color.LightGreen;
            }
            else
            {
                lblSystemStatus.Text = $"SYSTEM STATUS: {total - cleaned} ACTIVE THREATS DETECTED!";
                lblSystemStatus.ForeColor = Color.Salmon;
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            btnFullScan.Enabled = enabled;
            btnScanFolder.Enabled = enabled;
            btnScanProjects.Enabled = enabled;
            btnScanProcesses.Enabled = enabled;
            btnHuntVSInfection.Enabled = enabled;
            btnTraceC2Droppers.Enabled = enabled;
            btnInspectExe.Enabled = enabled;
            btnCleanAll.Enabled = enabled;
            btnExportReport.Enabled = enabled;
        }

        private void gridThreats_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && gridThreats.Rows[e.RowIndex].Tag is ThreatInfo threat)
            {
                string decodedInfo = !string.IsNullOrEmpty(threat.DecodedUrl)
                    ? $"🔗 EXPORTED / DECODED C2 LINK:\n{threat.DecodedUrl}\n\n"
                    : "";

                string details = $"📌 THREAT DIAGNOSTICS & ROOT CAUSE\n\n" +
                                 $"• Category    : {threat.CategoryText}\n" +
                                 $"• Threat Name : {threat.ThreatName}\n" +
                                 $"• Location    : {threat.TargetPath}\n" +
                                 $"• Signature   : {threat.MalwareSignature}\n" +
                                 $"• Severity    : {threat.SeverityText}\n" +
                                 $"• Status      : {threat.Status}\n\n" +
                                 $"{decodedInfo}" +
                                 $"❓ ROOT CAUSE (KYU AUR KAHA SE AAYA):\n{threat.RootCauseExplanation}\n\n" +
                                 $"💻 INJECTED SNIPPET:\n{threat.InjectedSnippet}";

                MessageBox.Show(details, "Threat Diagnostics", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
