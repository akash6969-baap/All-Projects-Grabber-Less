namespace GrabberCleanerApp
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSystemStatus;
        
        private System.Windows.Forms.Panel statsPanel;
        private System.Windows.Forms.Label lblTotalThreatsLabel;
        private System.Windows.Forms.Label lblTotalThreatsValue;
        private System.Windows.Forms.Label lblCriticalLabel;
        private System.Windows.Forms.Label lblCriticalValue;
        private System.Windows.Forms.Label lblCleanedLabel;
        private System.Windows.Forms.Label lblCleanedValue;

        private System.Windows.Forms.FlowLayoutPanel actionBar;
        private System.Windows.Forms.Button btnFullScan;
        private System.Windows.Forms.Button btnScanFolder;
        private System.Windows.Forms.Button btnScanProjects;
        private System.Windows.Forms.Button btnScanProcesses;
        private System.Windows.Forms.Button btnHuntVSInfection;
        private System.Windows.Forms.Button btnTraceC2Droppers;
        private System.Windows.Forms.Button btnInspectExe;
        private System.Windows.Forms.Button btnCleanAll;
        private System.Windows.Forms.Button btnExportReport;

        private System.Windows.Forms.DataGridView gridThreats;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCategory;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPath;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSignature;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRootCause;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSeverity;

        private System.Windows.Forms.RichTextBox txtConsole;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblStatusText;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.topPanel = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSystemStatus = new System.Windows.Forms.Label();
            this.statsPanel = new System.Windows.Forms.Panel();
            this.lblTotalThreatsLabel = new System.Windows.Forms.Label();
            this.lblTotalThreatsValue = new System.Windows.Forms.Label();
            this.lblCriticalLabel = new System.Windows.Forms.Label();
            this.lblCriticalValue = new System.Windows.Forms.Label();
            this.lblCleanedLabel = new System.Windows.Forms.Label();
            this.lblCleanedValue = new System.Windows.Forms.Label();
            this.actionBar = new System.Windows.Forms.FlowLayoutPanel();
            this.btnFullScan = new System.Windows.Forms.Button();
            this.btnScanFolder = new System.Windows.Forms.Button();
            this.btnScanProjects = new System.Windows.Forms.Button();
            this.btnScanProcesses = new System.Windows.Forms.Button();
            this.btnHuntVSInfection = new System.Windows.Forms.Button();
            this.btnTraceC2Droppers = new System.Windows.Forms.Button();
            this.btnCleanAll = new System.Windows.Forms.Button();
            this.btnExportReport = new System.Windows.Forms.Button();
            this.btnInspectExe = new System.Windows.Forms.Button();
            this.gridThreats = new System.Windows.Forms.DataGridView();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCategory = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSignature = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRootCause = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSeverity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.txtConsole = new System.Windows.Forms.RichTextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblStatusText = new System.Windows.Forms.Label();
            
            this.topPanel.SuspendLayout();
            this.statsPanel.SuspendLayout();
            this.actionBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridThreats)).BeginInit();
            this.SuspendLayout();
            // 
            // topPanel
            // 
            this.topPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(28)))), ((int)(((byte)(36)))));
            this.topPanel.Controls.Add(this.lblTitle);
            this.topPanel.Controls.Add(this.lblSystemStatus);
            this.topPanel.Controls.Add(this.statsPanel);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Name = "topPanel";
            this.topPanel.Size = new System.Drawing.Size(1200, 95);
            this.topPanel.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(18, 15);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(560, 37);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "🛡️ GRABBER & MSBUILD ANNIHILATOR v2.0";
            // 
            // lblSystemStatus
            // 
            this.lblSystemStatus.AutoSize = true;
            this.lblSystemStatus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblSystemStatus.ForeColor = System.Drawing.Color.LightGreen;
            this.lblSystemStatus.Location = new System.Drawing.Point(23, 57);
            this.lblSystemStatus.Name = "lblSystemStatus";
            this.lblSystemStatus.Size = new System.Drawing.Size(262, 23);
            this.lblSystemStatus.TabIndex = 1;
            this.lblSystemStatus.Text = "SYSTEM STATUS: SECURE & CLEAN";
            // 
            // statsPanel
            // 
            this.statsPanel.Controls.Add(this.lblTotalThreatsLabel);
            this.statsPanel.Controls.Add(this.lblTotalThreatsValue);
            this.statsPanel.Controls.Add(this.lblCriticalLabel);
            this.statsPanel.Controls.Add(this.lblCriticalValue);
            this.statsPanel.Controls.Add(this.lblCleanedLabel);
            this.statsPanel.Controls.Add(this.lblCleanedValue);
            this.statsPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.statsPanel.Location = new System.Drawing.Point(680, 0);
            this.statsPanel.Name = "statsPanel";
            this.statsPanel.Size = new System.Drawing.Size(520, 95);
            this.statsPanel.TabIndex = 2;
            // 
            // lblTotalThreatsLabel
            // 
            this.lblTotalThreatsLabel.AutoSize = true;
            this.lblTotalThreatsLabel.ForeColor = System.Drawing.Color.Gray;
            this.lblTotalThreatsLabel.Location = new System.Drawing.Point(30, 20);
            this.lblTotalThreatsLabel.Name = "lblTotalThreatsLabel";
            this.lblTotalThreatsLabel.Size = new System.Drawing.Size(91, 20);
            this.lblTotalThreatsLabel.Text = "Total Threats";
            // 
            // lblTotalThreatsValue
            // 
            this.lblTotalThreatsValue.AutoSize = true;
            this.lblTotalThreatsValue.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTotalThreatsValue.ForeColor = System.Drawing.Color.White;
            this.lblTotalThreatsValue.Location = new System.Drawing.Point(30, 42);
            this.lblTotalThreatsValue.Name = "lblTotalThreatsValue";
            this.lblTotalThreatsValue.Size = new System.Drawing.Size(32, 37);
            this.lblTotalThreatsValue.Text = "0";
            // 
            // lblCriticalLabel
            // 
            this.lblCriticalLabel.AutoSize = true;
            this.lblCriticalLabel.ForeColor = System.Drawing.Color.Gray;
            this.lblCriticalLabel.Location = new System.Drawing.Point(190, 20);
            this.lblCriticalLabel.Name = "lblCriticalLabel";
            this.lblCriticalLabel.Size = new System.Drawing.Size(126, 20);
            this.lblCriticalLabel.Text = "Critical Infections";
            // 
            // lblCriticalValue
            // 
            this.lblCriticalValue.AutoSize = true;
            this.lblCriticalValue.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblCriticalValue.ForeColor = System.Drawing.Color.Salmon;
            this.lblCriticalValue.Location = new System.Drawing.Point(190, 42);
            this.lblCriticalValue.Name = "lblCriticalValue";
            this.lblCriticalValue.Size = new System.Drawing.Size(32, 37);
            this.lblCriticalValue.Text = "0";
            // 
            // lblCleanedLabel
            // 
            this.lblCleanedLabel.AutoSize = true;
            this.lblCleanedLabel.ForeColor = System.Drawing.Color.Gray;
            this.lblCleanedLabel.Location = new System.Drawing.Point(370, 20);
            this.lblCleanedLabel.Name = "lblCleanedLabel";
            this.lblCleanedLabel.Size = new System.Drawing.Size(103, 20);
            this.lblCleanedLabel.Text = "Cleaned Items";
            // 
            // lblCleanedValue
            // 
            this.lblCleanedValue.AutoSize = true;
            this.lblCleanedValue.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblCleanedValue.ForeColor = System.Drawing.Color.LightGreen;
            this.lblCleanedValue.Location = new System.Drawing.Point(370, 42);
            this.lblCleanedValue.Name = "lblCleanedValue";
            this.lblCleanedValue.Size = new System.Drawing.Size(32, 37);
            this.lblCleanedValue.Text = "0";
            // 
            // actionBar
            // 
            this.actionBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(20)))), ((int)(((byte)(26)))));
            this.actionBar.Controls.Add(this.btnFullScan);
            this.actionBar.Controls.Add(this.btnScanProcesses);
            this.actionBar.Controls.Add(this.btnHuntVSInfection);
            this.actionBar.Controls.Add(this.btnTraceC2Droppers);
            this.actionBar.Controls.Add(this.btnScanFolder);
            this.actionBar.Controls.Add(this.btnScanProjects);
            this.actionBar.Controls.Add(this.btnInspectExe);
            this.actionBar.Controls.Add(this.btnCleanAll);
            this.actionBar.Controls.Add(this.btnExportReport);
            this.actionBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.actionBar.Padding = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this.actionBar.Location = new System.Drawing.Point(0, 95);
            this.actionBar.Name = "actionBar";
            this.actionBar.AutoSize = true;
            this.actionBar.WrapContents = true;
            // 
            // btnScanFolder
            // 
            this.btnScanFolder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(126)))), ((int)(((byte)(34)))));
            this.btnScanFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnScanFolder.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnScanFolder.ForeColor = System.Drawing.Color.White;
            this.btnScanFolder.Location = new System.Drawing.Point(228, 13);
            this.btnScanFolder.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
            this.btnScanFolder.Name = "btnScanFolder";
            this.btnScanFolder.Size = new System.Drawing.Size(210, 38);
            this.btnScanFolder.TabIndex = 6;
            this.btnScanFolder.Text = "📂 SCAN PROJECT FOLDER";
            this.btnScanFolder.UseVisualStyleBackColor = false;
            this.btnScanFolder.Click += new System.EventHandler(this.btnScanFolder_Click);
            // 
            // btnHuntVSInfection
            // 
            this.btnHuntVSInfection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(57)))), ((int)(((byte)(43)))));
            this.btnHuntVSInfection.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHuntVSInfection.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnHuntVSInfection.ForeColor = System.Drawing.Color.White;
            this.btnHuntVSInfection.Location = new System.Drawing.Point(3, 13);
            this.btnHuntVSInfection.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
            this.btnHuntVSInfection.Name = "btnHuntVSInfection";
            this.btnHuntVSInfection.Size = new System.Drawing.Size(275, 38);
            this.btnHuntVSInfection.TabIndex = 7;
            this.btnHuntVSInfection.Text = "🔨 HUNT VS & MSBUILD INFECTION";
            this.btnHuntVSInfection.UseVisualStyleBackColor = false;
            this.btnHuntVSInfection.Click += new System.EventHandler(this.btnHuntVSInfection_Click);
            // 
            // btnTraceC2Droppers
            // 
            this.btnTraceC2Droppers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(160)))), ((int)(((byte)(133)))));
            this.btnTraceC2Droppers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTraceC2Droppers.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnTraceC2Droppers.ForeColor = System.Drawing.Color.White;
            this.btnTraceC2Droppers.Location = new System.Drawing.Point(3, 13);
            this.btnTraceC2Droppers.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
            this.btnTraceC2Droppers.Name = "btnTraceC2Droppers";
            this.btnTraceC2Droppers.Size = new System.Drawing.Size(260, 38);
            this.btnTraceC2Droppers.TabIndex = 8;
            this.btnTraceC2Droppers.Text = "🕵️ TRACE C2 & DROPPED SCRIPTS";
            this.btnTraceC2Droppers.UseVisualStyleBackColor = false;
            this.btnTraceC2Droppers.Click += new System.EventHandler(this.btnTraceC2Droppers_Click);
            // 
            // btnInspectExe
            // 
            this.btnInspectExe.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(68)))), ((int)(((byte)(173)))));
            this.btnInspectExe.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInspectExe.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnInspectExe.ForeColor = System.Drawing.Color.White;
            this.btnInspectExe.Location = new System.Drawing.Point(660, 13);
            this.btnInspectExe.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
            this.btnInspectExe.Name = "btnInspectExe";
            this.btnInspectExe.Size = new System.Drawing.Size(190, 38);
            this.btnInspectExe.TabIndex = 5;
            this.btnInspectExe.Text = "🎯 INSPECT EXE / FILE";
            this.btnInspectExe.UseVisualStyleBackColor = false;
            this.btnInspectExe.Click += new System.EventHandler(this.btnInspectExe_Click);
            // 
            // btnFullScan
            // 
            this.btnFullScan.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnFullScan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFullScan.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnFullScan.ForeColor = System.Drawing.Color.White;
            this.btnFullScan.Location = new System.Drawing.Point(15, 13);
            this.btnFullScan.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
            this.btnFullScan.Name = "btnFullScan";
            this.btnFullScan.Size = new System.Drawing.Size(200, 38);
            this.btnFullScan.TabIndex = 0;
            this.btnFullScan.Text = "🚀 FULL DEEP SCAN";
            this.btnFullScan.UseVisualStyleBackColor = false;
            this.btnFullScan.Click += new System.EventHandler(this.btnFullScan_Click);
            // 
            // btnScanProjects
            // 
            this.btnScanProjects.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(50)))), ((int)(((byte)(65)))));
            this.btnScanProjects.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnScanProjects.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnScanProjects.ForeColor = System.Drawing.Color.White;
            this.btnScanProjects.Location = new System.Drawing.Point(228, 13);
            this.btnScanProjects.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
            this.btnScanProjects.Name = "btnScanProjects";
            this.btnScanProjects.Size = new System.Drawing.Size(200, 38);
            this.btnScanProjects.TabIndex = 1;
            this.btnScanProjects.Text = "🔍 SCAN PROJECTS";
            this.btnScanProjects.UseVisualStyleBackColor = false;
            this.btnScanProjects.Click += new System.EventHandler(this.btnScanProjects_Click);
            // 
            // btnScanProcesses
            // 
            this.btnScanProcesses.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.btnScanProcesses.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnScanProcesses.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnScanProcesses.ForeColor = System.Drawing.Color.White;
            this.btnScanProcesses.Location = new System.Drawing.Point(441, 13);
            this.btnScanProcesses.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
            this.btnScanProcesses.Name = "btnScanProcesses";
            this.btnScanProcesses.Size = new System.Drawing.Size(210, 38);
            this.btnScanProcesses.TabIndex = 2;
            this.btnScanProcesses.Text = "⚡ SMART FAST SCAN (5s)";
            this.btnScanProcesses.UseVisualStyleBackColor = false;
            this.btnScanProcesses.Click += new System.EventHandler(this.btnScanProcesses_Click);
            // 
            // btnCleanAll
            // 
            this.btnCleanAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(57)))), ((int)(((byte)(43)))));
            this.btnCleanAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCleanAll.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnCleanAll.ForeColor = System.Drawing.Color.White;
            this.btnCleanAll.Location = new System.Drawing.Point(664, 13);
            this.btnCleanAll.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
            this.btnCleanAll.Name = "btnCleanAll";
            this.btnCleanAll.Size = new System.Drawing.Size(230, 38);
            this.btnCleanAll.TabIndex = 3;
            this.btnCleanAll.Text = "🧹 1-CLICK CLEAN & UDANA";
            this.btnCleanAll.UseVisualStyleBackColor = false;
            this.btnCleanAll.Click += new System.EventHandler(this.btnCleanAll_Click);
            // 
            // btnExportReport
            // 
            this.btnExportReport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(50)))), ((int)(((byte)(65)))));
            this.btnExportReport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportReport.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnExportReport.ForeColor = System.Drawing.Color.White;
            this.btnExportReport.Location = new System.Drawing.Point(907, 13);
            this.btnExportReport.Name = "btnExportReport";
            this.btnExportReport.Size = new System.Drawing.Size(180, 38);
            this.btnExportReport.TabIndex = 4;
            this.btnExportReport.Text = "💾 EXPORT REPORT";
            this.btnExportReport.UseVisualStyleBackColor = false;
            this.btnExportReport.Click += new System.EventHandler(this.btnExportReport_Click);
            // 
            // gridThreats
            // 
            this.gridThreats.AllowUserToAddRows = false;
            this.gridThreats.AllowUserToDeleteRows = false;
            this.gridThreats.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridThreats.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(17)))), ((int)(((byte)(23)))));
            this.gridThreats.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(32)))), ((int)(((byte)(44)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gridThreats.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.gridThreats.ColumnHeadersHeight = 35;
            this.gridThreats.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colStatus,
            this.colCategory,
            this.colName,
            this.colPath,
            this.colSignature,
            this.colRootCause,
            this.colSeverity});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(22)))), ((int)(((byte)(30)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.LightGray;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(45)))), ((int)(((byte)(60)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridThreats.DefaultCellStyle = dataGridViewCellStyle2;
            this.gridThreats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridThreats.EnableHeadersVisualStyles = false;
            this.gridThreats.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(40)))), ((int)(((byte)(52)))));
            this.gridThreats.Location = new System.Drawing.Point(0, 155);
            this.gridThreats.MultiSelect = false;
            this.gridThreats.Name = "gridThreats";
            this.gridThreats.ReadOnly = true;
            this.gridThreats.RowHeadersVisible = false;
            this.gridThreats.RowHeadersWidth = 51;
            this.gridThreats.RowTemplate.Height = 32;
            this.gridThreats.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridThreats.Size = new System.Drawing.Size(1200, 365);
            this.gridThreats.TabIndex = 2;
            this.gridThreats.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridThreats_CellDoubleClick);
            // 
            // colStatus
            // 
            this.colStatus.FillWeight = 55F;
            this.colStatus.HeaderText = "Status";
            this.colStatus.MinimumWidth = 6;
            this.colStatus.Name = "colStatus";
            this.colStatus.ReadOnly = true;
            // 
            // colCategory
            // 
            this.colCategory.FillWeight = 85F;
            this.colCategory.HeaderText = "Category";
            this.colCategory.MinimumWidth = 6;
            this.colCategory.Name = "colCategory";
            this.colCategory.ReadOnly = true;
            // 
            // colName
            // 
            this.colName.FillWeight = 110F;
            this.colName.HeaderText = "Threat Name";
            this.colName.MinimumWidth = 6;
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            // 
            // colPath
            // 
            this.colPath.FillWeight = 160F;
            this.colPath.HeaderText = "Location / Path";
            this.colPath.MinimumWidth = 6;
            this.colPath.Name = "colPath";
            this.colPath.ReadOnly = true;
            // 
            // colSignature
            // 
            this.colSignature.FillWeight = 90F;
            this.colSignature.HeaderText = "Signature";
            this.colSignature.MinimumWidth = 6;
            this.colSignature.Name = "colSignature";
            this.colSignature.ReadOnly = true;
            // 
            // colRootCause
            // 
            this.colRootCause.FillWeight = 180F;
            this.colRootCause.HeaderText = "Root Cause (Kyu Aur Kaise Aaya)";
            this.colRootCause.MinimumWidth = 6;
            this.colRootCause.Name = "colRootCause";
            this.colRootCause.ReadOnly = true;
            // 
            // colSeverity
            // 
            this.colSeverity.FillWeight = 55F;
            this.colSeverity.HeaderText = "Severity";
            this.colSeverity.MinimumWidth = 6;
            this.colSeverity.Name = "colSeverity";
            this.colSeverity.ReadOnly = true;
            // 
            // txtConsole
            // 
            this.txtConsole.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(12)))), ((int)(((byte)(16)))));
            this.txtConsole.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtConsole.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtConsole.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtConsole.ForeColor = System.Drawing.Color.LightGray;
            this.txtConsole.Location = new System.Drawing.Point(0, 520);
            this.txtConsole.Name = "txtConsole";
            this.txtConsole.ReadOnly = true;
            this.txtConsole.Size = new System.Drawing.Size(1200, 150);
            this.txtConsole.TabIndex = 3;
            this.txtConsole.Text = "";
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar.Location = new System.Drawing.Point(0, 670);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(1200, 15);
            this.progressBar.TabIndex = 4;
            // 
            // lblStatusText
            // 
            this.lblStatusText.AutoSize = true;
            this.lblStatusText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(12)))), ((int)(((byte)(16)))));
            this.lblStatusText.ForeColor = System.Drawing.Color.Gray;
            this.lblStatusText.Location = new System.Drawing.Point(10, 672);
            this.lblStatusText.Name = "lblStatusText";
            this.lblStatusText.Size = new System.Drawing.Size(140, 20);
            this.lblStatusText.TabIndex = 5;
            this.lblStatusText.Text = "System Ready.";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(17)))), ((int)(((byte)(23)))));
            this.ClientSize = new System.Drawing.Size(1200, 685);
            this.Controls.Add(this.gridThreats);
            this.Controls.Add(this.txtConsole);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.actionBar);
            this.Controls.Add(this.topPanel);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "🛡️ Grabber & MSBuild Malware Cleaner Toolkit";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            this.statsPanel.ResumeLayout(false);
            this.statsPanel.PerformLayout();
            this.actionBar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridThreats)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
    }
}
