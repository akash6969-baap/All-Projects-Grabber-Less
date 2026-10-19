namespace AotForms
{
    partial class FormAh
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            loginBtn = new Guna.UI2.WinForms.Guna2Button();
            guna2DragControl1 = new Guna.UI2.WinForms.Guna2DragControl(components);
            guna2DragControl2 = new Guna.UI2.WinForms.Guna2DragControl(components);
            label1 = new Label();
            usr = new AimBotConquer.Guna.Guna2TextBox();
            pass = new AimBotConquer.Guna.Guna2TextBox();
            SuspendLayout();
            // 
            // loginBtn
            // 
            loginBtn.Animated = true;
            loginBtn.BorderColor = Color.FromArgb(20, 20, 20);
            loginBtn.BorderRadius = 5;
            loginBtn.Cursor = Cursors.Hand;
            loginBtn.CustomizableEdges = customizableEdges1;
            loginBtn.DisabledState.BorderColor = Color.DarkGray;
            loginBtn.DisabledState.CustomBorderColor = Color.DarkGray;
            loginBtn.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            loginBtn.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            loginBtn.FillColor = Color.Red;
            loginBtn.Font = new Font("Tahoma", 11.25F, FontStyle.Bold, GraphicsUnit.Point);
            loginBtn.ForeColor = Color.Black;
            loginBtn.HoverState.BorderColor = Color.FromArgb(224, 224, 224);
            loginBtn.HoverState.FillColor = Color.FromArgb(224, 224, 224);
            loginBtn.ImageAlign = HorizontalAlignment.Left;
            loginBtn.ImageOffset = new Point(5, 0);
            loginBtn.Location = new Point(57, 161);
            loginBtn.Name = "loginBtn";
            loginBtn.PressedColor = Color.SlateBlue;
            loginBtn.ShadowDecoration.CustomizableEdges = customizableEdges2;
            loginBtn.Size = new Size(119, 31);
            loginBtn.TabIndex = 32;
            loginBtn.Text = "Login";
            loginBtn.Click += loginBtn_Click;
            // 
            // guna2DragControl1
            // 
            guna2DragControl1.DockIndicatorTransparencyValue = 0.6D;
            guna2DragControl1.DragStartTransparencyValue = 1D;
            guna2DragControl1.TargetControl = this;
            guna2DragControl1.UseTransparentDrag = true;
            // 
            // guna2DragControl2
            // 
            guna2DragControl2.DockIndicatorTransparencyValue = 0.6D;
            guna2DragControl2.DragStartTransparencyValue = 1D;
            guna2DragControl2.TargetControl = label1;
            guna2DragControl2.UseTransparentDrag = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Sitka Small", 15.75F, FontStyle.Bold, GraphicsUnit.Point);
            label1.ForeColor = Color.Red;
            label1.Location = new Point(15, 7);
            label1.Name = "label1";
            label1.Size = new Size(205, 31);
            label1.TabIndex = 35;
            label1.Text = "DOMINUS CORPS";
            // 
            // usr
            // 
            usr.DefaultText = "";
            usr.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            usr.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            usr.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            usr.DisabledState.Parent = usr;
            usr.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            usr.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            usr.FocusedState.Parent = usr;
            usr.ForeColor = Color.Black;
            usr.HoveredState.BorderColor = Color.FromArgb(94, 148, 255);
            usr.HoveredState.Parent = usr;
            usr.Location = new Point(23, 64);
            usr.Margin = new Padding(4, 3, 4, 3);
            usr.Name = "usr";
            usr.PasswordChar = '\0';
            usr.PlaceholderText = "User";
            usr.SelectedText = "";
            usr.Size = new Size(187, 34);
            usr.TabIndex = 33;
            // 
            // pass
            // 
            pass.DefaultText = "";
            pass.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            pass.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            pass.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            pass.DisabledState.Parent = pass;
            pass.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            pass.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            pass.FocusedState.Parent = pass;
            pass.ForeColor = Color.Black;
            pass.HoveredState.BorderColor = Color.FromArgb(94, 148, 255);
            pass.HoveredState.Parent = pass;
            pass.Location = new Point(23, 104);
            pass.Margin = new Padding(4, 3, 4, 3);
            pass.Name = "pass";
            pass.PasswordChar = '\0';
            pass.PlaceholderText = "Pass";
            pass.SelectedText = "";
            pass.Size = new Size(187, 34);
            pass.TabIndex = 34;
            // 
            // FormAh
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(17, 17, 17);
            ClientSize = new Size(232, 221);
            Controls.Add(label1);
            Controls.Add(pass);
            Controls.Add(usr);
            Controls.Add(loginBtn);
            ForeColor = Color.FromArgb(17, 17, 17);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(3, 2, 3, 2);
            Name = "FormAh";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "FormAh";
            TopMost = true;
            Load += FormAh_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Guna.UI2.WinForms.Guna2Button loginBtn;
        private Guna.UI2.WinForms.Guna2DragControl guna2DragControl1;
        private Guna.UI2.WinForms.Guna2DragControl guna2DragControl2;
        private AimBotConquer.Guna.Guna2TextBox usr;
        private AimBotConquer.Guna.Guna2TextBox pass;
        private Label label1;
    }
}