namespace GrabberCleanerApp
{
    partial class InspectionResultForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel pnlVerdictHeader;
        private System.Windows.Forms.Label lblVerdict;
        
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.Label lblFilePath;
        private System.Windows.Forms.Label lblFileType;
        private System.Windows.Forms.Label lblFileSize;
        private System.Windows.Forms.Label lblEntropy;
        private System.Windows.Forms.Label lblRiskScore;

        private System.Windows.Forms.ListBox lstUrls;
        private System.Windows.Forms.ListBox lstKeywords;
        private System.Windows.Forms.RichTextBox txtAnalysisNotes;

        private System.Windows.Forms.Button btnDeleteFile;
        private System.Windows.Forms.Button btnClose;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlVerdictHeader = new System.Windows.Forms.Panel();
            this.lblVerdict = new System.Windows.Forms.Label();
            this.lblFileName = new System.Windows.Forms.Label();
            this.lblFilePath = new System.Windows.Forms.Label();
            this.lblFileType = new System.Windows.Forms.Label();
            this.lblFileSize = new System.Windows.Forms.Label();
            this.lblEntropy = new System.Windows.Forms.Label();
            this.lblRiskScore = new System.Windows.Forms.Label();
            this.lstUrls = new System.Windows.Forms.ListBox();
            this.lstKeywords = new System.Windows.Forms.ListBox();
            this.txtAnalysisNotes = new System.Windows.Forms.RichTextBox();
            this.btnDeleteFile = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlVerdictHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlVerdictHeader
            // 
            this.pnlVerdictHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(150)))), ((int)(((byte)(70)))));
            this.pnlVerdictHeader.Controls.Add(this.lblVerdict);
            this.pnlVerdictHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlVerdictHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlVerdictHeader.Name = "pnlVerdictHeader";
            this.pnlVerdictHeader.Size = new System.Drawing.Size(850, 60);
            this.pnlVerdictHeader.TabIndex = 0;
            // 
            // lblVerdict
            // 
            this.lblVerdict.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblVerdict.Font = new System.Drawing.Font("Segoe UI", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblVerdict.ForeColor = System.Drawing.Color.White;
            this.lblVerdict.Location = new System.Drawing.Point(0, 0);
            this.lblVerdict.Name = "lblVerdict";
            this.lblVerdict.Size = new System.Drawing.Size(850, 60);
            this.lblVerdict.TabIndex = 0;
            this.lblVerdict.Text = "VERDICT";
            this.lblVerdict.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblFileName.ForeColor = System.Drawing.Color.White;
            this.lblFileName.Location = new System.Drawing.Point(20, 75);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(102, 28);
            this.lblFileName.TabIndex = 1;
            this.lblFileName.Text = "Filename";
            // 
            // lblFilePath
            // 
            this.lblFilePath.AutoSize = true;
            this.lblFilePath.ForeColor = System.Drawing.Color.Gray;
            this.lblFilePath.Location = new System.Drawing.Point(22, 105);
            this.lblFilePath.Name = "lblFilePath";
            this.lblFilePath.Size = new System.Drawing.Size(63, 20);
            this.lblFilePath.TabIndex = 2;
            this.lblFilePath.Text = "File Path";
            // 
            // lblFileType
            // 
            this.lblFileType.AutoSize = true;
            this.lblFileType.ForeColor = System.Drawing.Color.LightBlue;
            this.lblFileType.Location = new System.Drawing.Point(22, 135);
            this.lblFileType.Name = "lblFileType";
            this.lblFileType.Size = new System.Drawing.Size(67, 20);
            this.lblFileType.TabIndex = 3;
            this.lblFileType.Text = "File Type";
            // 
            // lblFileSize
            // 
            this.lblFileSize.AutoSize = true;
            this.lblFileSize.ForeColor = System.Drawing.Color.LightGray;
            this.lblFileSize.Location = new System.Drawing.Point(350, 135);
            this.lblFileSize.Name = "lblFileSize";
            this.lblFileSize.Size = new System.Drawing.Size(63, 20);
            this.lblFileSize.TabIndex = 4;
            this.lblFileSize.Text = "File Size";
            // 
            // lblEntropy
            // 
            this.lblEntropy.AutoSize = true;
            this.lblEntropy.ForeColor = System.Drawing.Color.Khaki;
            this.lblEntropy.Location = new System.Drawing.Point(550, 135);
            this.lblEntropy.Name = "lblEntropy";
            this.lblEntropy.Size = new System.Drawing.Size(60, 20);
            this.lblEntropy.TabIndex = 5;
            this.lblEntropy.Text = "Entropy";
            // 
            // lstUrls
            // 
            this.lstUrls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(24)))), ((int)(((byte)(32)))));
            this.lstUrls.ForeColor = System.Drawing.Color.Salmon;
            this.lstUrls.FormattingEnabled = true;
            this.lstUrls.ItemHeight = 20;
            this.lstUrls.Location = new System.Drawing.Point(22, 175);
            this.lstUrls.Name = "lstUrls";
            this.lstUrls.Size = new System.Drawing.Size(806, 104);
            this.lstUrls.TabIndex = 6;
            // 
            // lstKeywords
            // 
            this.lstKeywords.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(24)))), ((int)(((byte)(32)))));
            this.lstKeywords.ForeColor = System.Drawing.Color.Gold;
            this.lstKeywords.FormattingEnabled = true;
            this.lstKeywords.ItemHeight = 20;
            this.lstKeywords.Location = new System.Drawing.Point(22, 290);
            this.lstKeywords.Name = "lstKeywords";
            this.lstKeywords.Size = new System.Drawing.Size(806, 84);
            this.lstKeywords.TabIndex = 7;
            // 
            // txtAnalysisNotes
            // 
            this.txtAnalysisNotes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.txtAnalysisNotes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtAnalysisNotes.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtAnalysisNotes.ForeColor = System.Drawing.Color.LightGray;
            this.txtAnalysisNotes.Location = new System.Drawing.Point(22, 385);
            this.txtAnalysisNotes.Name = "txtAnalysisNotes";
            this.txtAnalysisNotes.ReadOnly = true;
            this.txtAnalysisNotes.Size = new System.Drawing.Size(806, 120);
            this.txtAnalysisNotes.TabIndex = 8;
            this.txtAnalysisNotes.Text = "";
            // 
            // btnDeleteFile
            // 
            this.btnDeleteFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(57)))), ((int)(((byte)(43)))));
            this.btnDeleteFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteFile.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnDeleteFile.ForeColor = System.Drawing.Color.White;
            this.btnDeleteFile.Location = new System.Drawing.Point(22, 515);
            this.btnDeleteFile.Name = "btnDeleteFile";
            this.btnDeleteFile.Size = new System.Drawing.Size(260, 40);
            this.btnDeleteFile.TabIndex = 9;
            this.btnDeleteFile.Text = "🧹 ANNIHILATE / DELETE FILE";
            this.btnDeleteFile.UseVisualStyleBackColor = false;
            this.btnDeleteFile.Click += new System.EventHandler(this.btnDeleteFile_Click);
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(55)))), ((int)(((byte)(70)))));
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(688, 515);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(140, 40);
            this.btnClose.TabIndex = 10;
            this.btnClose.Text = "CLOSE";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // InspectionResultForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(17)))), ((int)(((byte)(23)))));
            this.ClientSize = new System.Drawing.Size(850, 570);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnDeleteFile);
            this.Controls.Add(this.txtAnalysisNotes);
            this.Controls.Add(this.lstKeywords);
            this.Controls.Add(this.lstUrls);
            this.Controls.Add(this.lblEntropy);
            this.Controls.Add(this.lblFileSize);
            this.Controls.Add(this.lblFileType);
            this.Controls.Add(this.lblFilePath);
            this.Controls.Add(this.lblFileName);
            this.Controls.Add(this.pnlVerdictHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InspectionResultForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "🎯 Deep Binary & EXE Inspection Report";
            this.Load += new System.EventHandler(this.InspectionResultForm_Load);
            this.pnlVerdictHeader.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
