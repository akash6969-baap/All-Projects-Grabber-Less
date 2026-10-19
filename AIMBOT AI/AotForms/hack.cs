using AotForms;
using AotForms;
using Client;
using DiscordRPC;
using Guna.UI2.WinForms;
using Helper;
using Microsoft.VisualBasic.Logging;
using FearAuth;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing;
using System.Linq;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Forms;
using UnovaMemory;
using static AotForms.MouseHook;
using static Guna.UI2.Native.WinApi;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace Client
{

    public partial class hack : Form
    {


        KeyHelper kh = new KeyHelper();
        bool shift;
        IntPtr mainHandle;

        public hack(IntPtr handle)
        {
            InitializeComponent();

            kh.KeyDown += Kh_KeyDown;
            kh.KeyUp += Kh_KeyUp;
            mainHandle = handle;


            AttachProcess();

        }

        private async void Kh_KeyDown(object sender, KeyEventArgs e)
        {





        }
        private void Kh_KeyUp(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey) shift = false;

        }










        private bool dragging = false;
        private Point startPoint = new Point(0, 0);

        private void Panel_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            startPoint = new Point(e.X, e.Y);
        }
        private void Panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point p = PointToScreen(e.Location);
                this.Location = new Point(p.X - this.startPoint.X, p.Y - this.startPoint.Y);
            }

        }
        private void Panel_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }
        private void AttachProcess()
        {
            Process[] processes = Process.GetProcessesByName("GameProcessName"); // Change this to your game
            if (processes.Length > 0)
            {
                IntPtr processHandle = processes[0].Handle;

                Console.WriteLine("Game attached successfully!");
            }
            else
            {
                Console.WriteLine("Game process not found!");
            }
        }
        private void guna2ToggleSwitch1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void guna2ToggleSwitch2_CheckedChanged(object sender, EventArgs e)
        {



        }

        private void guna2ToggleSwitch3_CheckedChanged(object sender, EventArgs e)
        {
            Config.Notif();
        }

        private void guna2TrackBar2_Scroll(object sender, ScrollEventArgs e)
        {

        }

        private void guna2ToggleSwitch4_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void guna2ToggleSwitch5_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void guna2ToggleSwitch6_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void guna2TrackBar1_Scroll(object sender, ScrollEventArgs e)
        {

        }

        private void guna2TrackBar3_Scroll(object sender, ScrollEventArgs e)
        {


        }


        public async void enablehook()
        {

        }
        private void HideAllPanels()
        {
            guna2Panel2.Visible = false;
            guna2Panel3.Visible = false;
            guna2Panel4.Visible = false;

        }

        private async void AimbotForm_Load(object sender, EventArgs e)
        {
            timer1.Interval = 50; // Adjust as needed
            timer1.Tick += timer1_Tick;
            await Task.Delay(2000);





            ///316, 361
            //this.TopMost = true;

        }

        private void guna2PictureBox9_Click(object sender, EventArgs e)
        {

        }

        private void guna2ControlBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {

        }

        private void label36_Click(object sender, EventArgs e)
        {

        }

        private void aimfovtxt_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox1_Click(object sender, EventArgs e)
        {


        }

        private void guna2CustomCheckBox2_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox3_Click(object sender, EventArgs e)
        {

        }
        private string PID;


        private async void guna2CustomCheckBox4_Click(object sender, EventArgs e)
        {

        }

        private async void guna2CustomCheckBox5_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox6_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox12_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox11_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox10_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox9_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox8_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox7_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox18_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox17_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox16_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox15_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox14_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox13_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox24_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox23_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox22_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox21_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox20_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox19_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {

        }

        private void COLOUR_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label28_Click(object sender, EventArgs e)
        {

        }

        private void guna2CirclePictureBox4_Click(object sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    //Config.ESPFillBoxGlowColor = colorDialog.Color; // Save picked color
                }
            }
        }

        private void guna2CustomCheckBox25_Click(object sender, EventArgs e)
        {

            //Config.AimKillEnabled = guna2CustomCheckBox25.Checked;
            //Config.flyme = guna2CustomCheckBox25.Checked;


            /*
            Config.Notif();
            Config.AimBot = guna2CustomCheckBox25.Checked;

            if (guna2CustomCheckBox25.Checked)
            {
                statuslbl.Text = "Aimbot Enabled";

            }
            else
            {
                statuslbl.Text = "Aimbot Disabled";
            }
            */
        }
        private void guna2CustomCheckBox27_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox29_Click(object sender, EventArgs e)
        {
            Config.Notif();
            Config.Aimfovc = guna2CustomCheckBox29.Checked;
            Config.Aimfovc = guna2CustomCheckBox29.Checked;


            if (guna2CustomCheckBox29.Checked)
            {
                statuslbl.Text = "Aimfov Circle Enabled";
            }
            else
            {
                statuslbl.Text = "Aimfov Circle Disabled";
            }
        }

        private void guna2CustomCheckBox28_Click(object sender, EventArgs e)
        {



        }

        private void guna2CustomCheckBox26_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox34_Click(object sender, EventArgs e)
        {
            Config.Notif();
            Config.ESPLine = guna2CustomCheckBox34.Checked;
            Config.ESPLine = guna2CustomCheckBox34.Checked;

            if (guna2CustomCheckBox34.Checked)
            {
                statuslbl.Text = "Esp Line Enabled";
            }
            else
            {
                statuslbl.Text = "Esp Line Disabled";
            }
        }

        private void guna2CustomCheckBox33_Click(object sender, EventArgs e)
        {
            Config.Notif();
            Config.ESPBox2 = guna2CustomCheckBox33.Checked;
            if (guna2CustomCheckBox33.Checked)
            {
                statuslbl.Text = "EspBox Enabled";
            }
            else
            {
                statuslbl.Text = "EspBox Disabled";
            }

        }

        private void guna2CustomCheckBox32_Click(object sender, EventArgs e)
        {
            Config.Notif();
            Config.ESPFillBox = guna2CustomCheckBox32.Checked;
            if (guna2CustomCheckBox32.Checked)
            {
                statuslbl.Text = "Esp Fill Box Enabled";
            }
            else
            {
                statuslbl.Text = "Esp Fill Box Disabled";
            }
        }

        private void guna2CustomCheckBox31_Click(object sender, EventArgs e)
        {


        }

        private void guna2CustomCheckBox30_Click(object sender, EventArgs e)
        {
            Config.Notif();
            Config.ESPName = guna2CustomCheckBox30.Checked;

            if (guna2CustomCheckBox30.Checked)
            {
                statuslbl.Text = "Esp Name Enabled";
            }
            else
            {
                statuslbl.Text = "Esp Name Disabled";
            }
        }

        private void guna2CustomCheckBox39_Click(object sender, EventArgs e)
        {
            Config.Notif();
            Config.ESPHealth = guna2CustomCheckBox39.Checked;

            if (guna2CustomCheckBox39.Checked)
            {
                statuslbl.Text = "Esp Health Enabled";
            }
            else
            {
                statuslbl.Text = "Esp Health Disabled";
            }
        }

        private void guna2CustomCheckBox35_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox36_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox37_Click(object sender, EventArgs e)
        {



        }

        private void guna2CustomCheckBox38_Click(object sender, EventArgs e)
        {


        }

        private void guna2CirclePictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox5_Click(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox6_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox6_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox10_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox9_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox8_Click_1(object sender, EventArgs e)
        {


        }


        [DllImport("user32.dll")]
        static extern uint SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);
        const uint WDA_NONE = 0x00000000;
        const uint WDA_MONITOR = 0x00000001;
        const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;
        private void guna2CustomCheckBox7_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox1_Click_1(object sender, EventArgs e)
        {

        }



        static void ExecuteCommand(string command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c " + command,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process { StartInfo = startInfo };
            process.Start();
            process.WaitForExit();
        }

        static void DeleteFirewallRule(string programPath)
        {
            ExecuteCommand("netsh advfirewall firewall delete rule name=all program=\"" + programPath + "\"");
        }

        private void guna2CustomCheckBox5_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click_2(object sender, EventArgs e)
        {

        }

        private void guna2Button2_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2Button3_Click_1(object sender, EventArgs e)
        {

        }


        private bool isProcessRunning = false;

        private void UpdateStatuss(string statusText, Color Color)
        {
            statuslbl.Text = statusText;
            statuslbl.ForeColor = Color.Green;
            status.ForeColor = Color.Green;
        }
        static IntPtr FindRenderWindow(IntPtr parent)
        {
            IntPtr renderWindow = IntPtr.Zero;
            WinAPI.EnumChildWindows(parent, (hWnd, lParam) =>
            {
                StringBuilder sb = new StringBuilder(256);
                WinAPI.GetWindowText(hWnd, sb, sb.Capacity);
                string windowName = sb.ToString();
                if (!string.IsNullOrEmpty(windowName))
                {
                    if (windowName != "HD-Player")
                    {
                        renderWindow = hWnd;
                    }
                }
                return true;
            }, IntPtr.Zero);

            return renderWindow;
        }

        private async void guna2Button4_Click_1(object sender, EventArgs e)
        {

        }
        bool startk = false;
        bool connected = false;
        public static void DoneSound()
        {

        }
        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void aimss_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2CustomCheckBox2_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox3_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox4_Click_1(object sender, EventArgs e)
        {
            Config.Notif();

        }

        private void rebornLabel1_Click(object sender, EventArgs e)
        {
            rebornLabel1.ForeColor = Color.White;
            rebornLabel5.ForeColor = Color.FromArgb(75, 75, 75);
            rebornLabel6.ForeColor = Color.FromArgb(75, 75, 75);
            rebornLabel7.ForeColor = Color.FromArgb(75, 75, 75);
            newmain.Location = new Point(8, 66);

            newconfig.Location = new Point(9999, 9999);
        }

        private void guna2ControlBox1_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox3_Click_2(object sender, EventArgs e)
        {
            Config.Notif();
            Config.FixEsp = guna2CustomCheckBox3.Checked;
        }

        private void guna2CustomCheckBox12_Click_1(object sender, EventArgs e)
        {
            if (guna2CustomCheckBox12.Checked)
            {
                TopMost = true;
            }
            else
            {
                TopMost = false;
            }
        }

        private void guna2CustomCheckBox13_Click_1(object sender, EventArgs e)
        {
            if (guna2CustomCheckBox13.Checked)
            {
                ShowInTaskbar = false;
            }
            else
            {
                ShowInTaskbar = true;
            }
        }

        private void guna2CustomCheckBox14_Click_1(object sender, EventArgs e)
        {
            SetStreamMode(guna2CustomCheckBox14.Checked);
            void SetStreamMode(bool state)
            {
                foreach (var obj in Application.OpenForms)
                {
                    var form = obj as Form;

                    if (state)
                    {
                        SetWindowDisplayAffinity(form.Handle, WDA_EXCLUDEFROMCAPTURE);

                    }
                    else
                    {

                        SetWindowDisplayAffinity(form.Handle, WDA_NONE);

                    }
                }
            }
        }

        private void guna2CustomCheckBox15_Click_1(object sender, EventArgs e)
        {
            SetStreamMode(guna2CustomCheckBox15.Checked);
            Config.StreamMode = guna2CustomCheckBox15.Checked;
            Config.sound = guna2CustomCheckBox15.Checked;
            void SetStreamMode(bool state)
            {
                foreach (var obj in Application.OpenForms)
                {
                    var form = obj as Form;

                    if (state)
                    {
                        SetWindowDisplayAffinity(form.Handle, WDA_EXCLUDEFROMCAPTURE);

                    }
                    else
                    {

                        SetWindowDisplayAffinity(form.Handle, WDA_NONE);

                    }
                }
            }
        }

        private void guna2CustomCheckBox16_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox17_Click_1(object sender, EventArgs e)
        {
            Config.Notif();
            if (guna2CustomCheckBox17.Checked == true)
            {
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock0\" dir=in action=block profile=any program=\"C:\\Program Files\\BlueStacks_nxt\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock0\" dir=out action=block profile=any program=\"C:\\Program Files\\BlueStacks_nxt\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock1\" dir=in action=block profile=any program=\"C:\\Program Files\\BlueStacks_msi5\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock1\" dir=out action=block profile=any program=\"C:\\Program Files\\BlueStacks_msi5\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock2\" dir=in action=block profile=any program=\"C:\\Program Files\\BlueStacks\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock2\" dir=out action=block profile=any program=\"C:\\Program Files\\BlueStacks\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock3\" dir=in action=block profile=any program=\"C:\\Program Files\\BlueStacks_msi2\\Bluestacks.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock3\" dir=in action=block profile=any program=\"C:\\Program Files\\BlueStacks_msi2\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock3\" dir=out action=block profile=any program=\"C:\\Program Files\\BlueStacks_msi2\\HD-Player.exe\"");

            }
            else
            {
                DeleteFirewallRule("C:\\Program Files\\BlueStacks_nxt\\HD-Player.exe");
                DeleteFirewallRule("C:\\Program Files\\BlueStacks_msi5\\HD-Player.exe");
                DeleteFirewallRule("C:\\Program Files\\BlueStacks\\HD-Player.exe");
                DeleteFirewallRule("C:\\Program Files\\BlueStacks_msi2\\HD-Player.exe");

            }
        }

        private void guna2CustomCheckBox11_Click_1(object sender, EventArgs e)
        {
            Config.Notif();
            Config.sound = guna2CustomCheckBox11.Checked;
        }

        private void rebornLabel5_Click(object sender, EventArgs e)
        {
            rebornLabel1.ForeColor = Color.FromArgb(75, 75, 75);
            rebornLabel5.ForeColor = Color.White;
            rebornLabel6.ForeColor = Color.FromArgb(75, 75, 75);
            rebornLabel7.ForeColor = Color.FromArgb(75, 75, 75);

            newconfig.Location = new Point(9999, 9999);
            newmain.Location = new Point(9999, 9999);
        }

        private void rebornLabel6_Click(object sender, EventArgs e)
        {
            rebornLabel6.ForeColor = Color.White;
            rebornLabel5.ForeColor = Color.FromArgb(75, 75, 75);
            rebornLabel1.ForeColor = Color.FromArgb(75, 75, 75);
            rebornLabel7.ForeColor = Color.FromArgb(75, 75, 75);
            newconfig.Location = new Point(8, 66);

            newmain.Location = new Point(9999, 9999);

        }

        private void rebornLabel7_Click(object sender, EventArgs e)
        {

        }

        private void trackBarFloatClient1_Event_0(object sender, EventArgs e)
        {

        }

        private void trackBarFloatClient2_Event_0(object sender, EventArgs e)
        {
            var DELAY = trackBarFloatClient2.Value;

            //lblDistance.Text = $"({DELAY})";

            Config.thread = DELAY;
        }

        private void trackBarFloatClient3_Event_0(object sender, EventArgs e)
        {
            var distance = trackBarFloatClient3.Value;

            //rangeaim.Text = $"({distance})";

            Config.AimBotMaxDistance = distance;
        }

        private void guna2CustomCheckBox1_Click_2(object sender, EventArgs e)
        {
            Config.Notif();
            Config.minimap = guna2CustomCheckBox1.Checked;
            if (guna2CustomCheckBox1.Checked)
            {
                statuslbl.Text = "Mini Map Enabled";
            }
            else
            {
                statuslbl.Text = "Mini Map Disabled";
            }
        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void label19_Click(object sender, EventArgs e)
        {
            label19.ForeColor = Color.White;
            label12.ForeColor = Color.FromArgb(88, 88, 88);
            label13.ForeColor = Color.FromArgb(88, 88, 88);
            newconfig.Location = new Point(8, 66);

            newmain.Location = new Point(9999, 9999);
        }

        private void trackBarFloatClient4_Event_0(object sender, EventArgs e)
        {
            var aimfov1 = trackBarFloatClient4.Value;

            Config.AimBotFov = aimfov1;

        }

        private void trackBarFloatClient1_Event_0_1(object sender, EventArgs e)
        {

        }

        private void trackBarFloatClient2_Event_0_1(object sender, EventArgs e)
        {
            var distance = trackBarFloatClient2.Value;

            Config.AimBotMaxDistance = distance;
        }

        private void trackBarFloatClient3_Event_0_1(object sender, EventArgs e)
        {



        }

        private void guna2CustomCheckBox5_Click_2(object sender, EventArgs e)
        {


        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

        }
        private void guna2CustomCheckBox6_Click_2(object sender, EventArgs e)
        {

        }

        private void guna2TrackBar1_Scroll_1(object sender, ScrollEventArgs e)
        {

        }

        private void guna2TrackBar2_Scroll_1(object sender, ScrollEventArgs e)
        {

        }


        public async void spwawnkill()
        {

        }
        private async void guna2CustomCheckBox9_Click_2(object sender, EventArgs e)
        {

        }

        UnovaMem memory = new UnovaMem();

        public void ghoston()
        {
            foreach (var address in UNCLEaddress)
            {
                memory.WriteMemory(address.ToString("X"), "bytes", UNCLEreplace);
            }

        }
        public void ghostoff()
        {
            foreach (var address in UNCLEaddress)
            {
                memory.WriteMemory(address.ToString("X"), "bytes", UNCLENORMAL);
            }

        }


        private List<long> UNCLEaddress = new List<long>();
        private string UNCLEsearch;
        private string UNCLEreplace;
        private string UNCLENORMAL;
        private async void guna2CustomCheckBox7_Click_2(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox6_Click_3(object sender, EventArgs e)
        {
            Config.Notif();
            Config.AIMVISIBLE = guna2CustomCheckBox6.Checked;
            if (guna2CustomCheckBox6.Checked)
            {
                statuslbl.Text = "Aimbot Ai Enabled";
            }
            else
            {
                statuslbl.Text = "Aimbot Ai Disabled";
            }

        }

        private void guna2CustomCheckBox4_Click_2(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox8_Click_2(object sender, EventArgs e)
        {

        }

        private void guna2ProgressBar1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void newconfig_Paint(object sender, PaintEventArgs e)
        {

        }

        private void newmain_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label61_Click(object sender, EventArgs e)
        {

        }

        private void newesp_Paint(object sender, PaintEventArgs e)
        {

        }

        private void status_Click(object sender, EventArgs e)
        {

        }

        private void label34_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void guna2ControlBox2_Click(object sender, EventArgs e)
        {

        }

        private void guna2Panel2_Paint_1(object sender, PaintEventArgs e)
        {

        }


        private void timer2_Tick(object sender, EventArgs e)
        {

        }

        private void guna2Panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void guna2ToggleSwitch2_CheckedChanged_1(object sender, EventArgs e)
        {
            Config.Notif();
            Config.AimbotVisible = guna2ToggleSwitch2.Checked;
            if (guna2ToggleSwitch2.Checked)
            {
                statuslbl.Text = "Aimbot Ai Enabled";
            }
            else
            {
                statuslbl.Text = "Aimbot Ai Disabled";
            }
        }

        private void guna2ToggleSwitch4_CheckedChanged_1(object sender, EventArgs e)
        {

        }

        private void guna2ToggleSwitch3_CheckedChanged_1(object sender, EventArgs e)
        {
            Config.Notif();
            Config.NoRecoil = guna2ToggleSwitch3.Checked;
            if (Config.NoRecoil)
            {
                statuslbl.Text = "No Recoil Activated";
            }
            else
            {
                statuslbl.Text = "No Recoil Deactivated";
            }
        }

        private void guna2ToggleSwitch8_CheckedChanged(object sender, EventArgs e)
        {
            Config.Notif();
            Config.AimFov = guna2ToggleSwitch8.Checked;
            Config.Aimfovc = guna2ToggleSwitch8.Checked;
            Config.Aimfovc = guna2ToggleSwitch8.Checked;

            if (guna2ToggleSwitch8.Checked)
            {
                statuslbl.Text = "Aimfov Circle Enabled";
            }
            else
            {
                statuslbl.Text = "Aimfov Circle Disabled";
            }
        }

        private void guna2ToggleSwitch7_CheckedChanged(object sender, EventArgs e)
        {
            SetStreamMode(guna2ToggleSwitch7.Checked);
            Config.StreamMode = guna2ToggleSwitch7.Checked;
            Config.sound = guna2ToggleSwitch7.Checked;
            void SetStreamMode(bool state)
            {
                foreach (var obj in Application.OpenForms)
                {
                    var form = obj as Form;

                    if (state)
                    {
                        SetWindowDisplayAffinity(form.Handle, WDA_EXCLUDEFROMCAPTURE);

                    }
                    else
                    {

                        SetWindowDisplayAffinity(form.Handle, WDA_NONE);

                    }
                }
            }
        }

        private void guna2ToggleSwitch6_CheckedChanged_1(object sender, EventArgs e)
        {
            Config.Notif();
            Config.ESPLine = guna2ToggleSwitch6.Checked;
            Config.EspUp = guna2ToggleSwitch6.Checked;

            if (guna2ToggleSwitch6.Checked)
            {
                statuslbl.Text = "Esp Line Enabled";
            }
            else
            {
                statuslbl.Text = "Esp Line Disabled";
            }
        }

        private void guna2ToggleSwitch5_CheckedChanged_1(object sender, EventArgs e)
        {
            Config.Notif();
            Config.ESPBox = guna2ToggleSwitch5.Checked;
            if (guna2ToggleSwitch5.Checked)
            {
                statuslbl.Text = "EspBox Enabled";
            }
            else
            {
                statuslbl.Text = "EspBox Disabled";
            }
        }

        private void guna2ToggleSwitch10_CheckedChanged(object sender, EventArgs e)
        {
            Config.Notif();
            Config.ESPFillBox = guna2ToggleSwitch10.Checked;
            if (guna2ToggleSwitch10.Checked)
            {
                statuslbl.Text = "Esp Fill Box Enabled";
            }
            else
            {
                statuslbl.Text = "Esp Fill Box Disabled";
            }
        }

        private void guna2ToggleSwitch9_CheckedChanged(object sender, EventArgs e)
        {
            Config.Notif();
            Config.ESPName = guna2ToggleSwitch9.Checked;
            Config.ESPDistance = guna2ToggleSwitch9.Checked;
            Config.ESPHealth = guna2ToggleSwitch9.Checked;

            if (guna2ToggleSwitch9.Checked)
            {
                statuslbl.Text = "ESP INFO Enabled";
            }
            else
            {
                statuslbl.Text = "ESP INFO Disabled";
            }
        }

        private void guna2ToggleSwitch14_CheckedChanged(object sender, EventArgs e)
        {
            Config.Notif();
            Config.ESPSkeleton = guna2ToggleSwitch14.Checked;

            if (guna2ToggleSwitch14.Checked)
            {
                statuslbl.Text = "ESP Skeleton Enabled";
            }
            else
            {
                statuslbl.Text = "ESP Skeleton Disabled";
            }
        }

        private void guna2ToggleSwitch13_CheckedChanged(object sender, EventArgs e)
        {
            Config.Notif();
            Config.FixEsp = guna2ToggleSwitch13.Checked;

            if (guna2ToggleSwitch13.Checked)
            {
                statuslbl.Text = "Fix ESP Enabled";

                timer2.Interval = 5000; // 5 sec
                timer2.Tick += (s, ev) =>
                {
                    Core.Entities = new();
                    InternalMemory.Cache = new();
                };
                timer2.Start();
            }
            else
            {
                status.Text = "Fix ESP Disabled";
                timer2.Stop();
            }
        }

        private void guna2ToggleSwitch12_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void guna2ToggleSwitch11_CheckedChanged(object sender, EventArgs e)
        {
            Config.Notif();
            Config.minimap = guna2ToggleSwitch11.Checked;
            if (guna2ToggleSwitch11.Checked)
            {
                statuslbl.Text = "Mini Map Enabled";
            }
            else
            {
                statuslbl.Text = "Mini Map Disabled";
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void guna2ToggleSwitch19_CheckedChanged(object sender, EventArgs e)
        {
            if (guna2ToggleSwitch19.Checked)
            {
                TopMost = true;
            }
            else
            {
                TopMost = false;
            }
        }

        private void guna2ToggleSwitch19_CheckedChanged_1(object sender, EventArgs e)
        {
            if (guna2ToggleSwitch19.Checked)
            {
                TopMost = true;
            }
            else
            {
                TopMost = false;
            }
        }

        private void guna2ToggleSwitch18_CheckedChanged(object sender, EventArgs e)
        {
            if (guna2ToggleSwitch18.Checked)
            {
                ShowInTaskbar = false;
            }
            else
            {
                ShowInTaskbar = true;
            }
        }

        private void guna2ToggleSwitch17_CheckedChanged(object sender, EventArgs e)
        {
            SetStreamMode(guna2ToggleSwitch17.Checked);
            void SetStreamMode(bool state)
            {
                foreach (var obj in Application.OpenForms)
                {
                    var form = obj as Form;

                    if (state)
                    {
                        SetWindowDisplayAffinity(form.Handle, WDA_EXCLUDEFROMCAPTURE);

                    }
                    else
                    {

                        SetWindowDisplayAffinity(form.Handle, WDA_NONE);

                    }
                }
            }
        }

        private void guna2ToggleSwitch16_CheckedChanged(object sender, EventArgs e)
        {
            Config.Notif();
            if (guna2ToggleSwitch16.Checked == true)
            {
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock0\" dir=in action=block profile=any program=\"C:\\Program Files\\BlueStacks_nxt\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock0\" dir=out action=block profile=any program=\"C:\\Program Files\\BlueStacks_nxt\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock1\" dir=in action=block profile=any program=\"C:\\Program Files\\BlueStacks_msi5\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock1\" dir=out action=block profile=any program=\"C:\\Program Files\\BlueStacks_msi5\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock2\" dir=in action=block profile=any program=\"C:\\Program Files\\BlueStacks\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock2\" dir=out action=block profile=any program=\"C:\\Program Files\\BlueStacks\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock3\" dir=in action=block profile=any program=\"C:\\Program Files\\BlueStacks_msi2\\Bluestacks.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock3\" dir=in action=block profile=any program=\"C:\\Program Files\\BlueStacks_msi2\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock3\" dir=out action=block profile=any program=\"C:\\Program Files\\BlueStacks_msi2\\HD-Player.exe\"");

            }
            else
            {
                DeleteFirewallRule("C:\\Program Files\\BlueStacks_nxt\\HD-Player.exe");
                DeleteFirewallRule("C:\\Program Files\\BlueStacks_msi5\\HD-Player.exe");
                DeleteFirewallRule("C:\\Program Files\\BlueStacks\\HD-Player.exe");
                DeleteFirewallRule("C:\\Program Files\\BlueStacks_msi2\\HD-Player.exe");

            }
        }

        private void guna2ToggleSwitch15_CheckedChanged(object sender, EventArgs e)
        {
            Config.Notif();
            Config.sound = guna2ToggleSwitch15.Checked;
        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox1_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2Button5_Click_1(object sender, EventArgs e)
        {
            Core.Entities = new();
            InternalMemory.Cache = new();
        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void guna2ComboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            // Get the selected mode from the combo box
            string selectedMode = guna2ComboBox1.SelectedItem?.ToString() ?? "NONE";

            // Update the AimBotMode in the configuration based on the selected mode
            switch (selectedMode)
            {
                case "NONE":
                    Config.ESPBox = false;
                    Config.ESPBox2 = false;
                    Config.ESPFillBox = false;
                    statuslbl.Text = "Status : ESP Box Disabled";
                    break;

                case "FULL":
                    Config.ESPBox2 = true;
                    Config.ESPBox = false;
                    statuslbl.Text = "Status : ESP Box Full Activated";
                    break;

                case "HALF":
                    Config.ESPBox = true;
                    Config.ESPBox2 = false;
                    statuslbl.Text = "Status : ESP Box Corner Activated";
                    break;

                case "FILL":
                    Config.ESPFillBox = !Config.ESPFillBox; // Toggle FillBox on/off
                    string fillStatus = Config.ESPFillBox ? "Activated" : "Disabled";
                    statuslbl.Text = "Status : ESP Fill Box {fillStatus}";
                    break;

                default:
                    Config.ESPBox = false;
                    Config.ESPBox2 = false;
                    Config.ESPFillBox = false;
                    statuslbl.Text = "Status : ESP Box Disabled";
                    break;
            }
        }

        private void guna2ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private async void guna2ToggleSwitch1_CheckedChanged_1(object sender, EventArgs e)
        {
            if (!connected)
            {
                statuslbl.Text = "Connecting ...";


                var processes = Process.GetProcessesByName("HD-Player");

                if (processes.Length != 1)
                {
                    statuslbl.ForeColor = Color.Red;

                    statuslbl.Text = "error";
                    return;
                }

                var process = processes[0];
                var mainModulePath = Path.GetDirectoryName(process.MainModule.FileName);
                var adbPath = Path.Combine(mainModulePath, "HD-Adb.exe");

                if (!File.Exists(adbPath))
                {
                    statuslbl.ForeColor = Color.Red;

                    statuslbl.Text = "MODULE FOUND";
                    return;
                }


                var adb = new Adb(adbPath);
                await adb.Kill();

                var started = await adb.Start();
                if (!started)
                {

                    statuslbl.ForeColor = Color.Red;
                    statuslbl.Text = "error";
                    return;
                }

                String pkg = "com.dts.freefireth";
                String lib = "libil2cpp.so";

                bool isFreeFireMax = false;
                if (isFreeFireMax)
                {
                    pkg = "com.dts.freefiremax";
                }

                var moduleAddr = await adb.FindModule(pkg, lib);
                if (moduleAddr == 0) // If the module address is not found
                {
                    statuslbl.ForeColor = Color.Red;

                    statuslbl.Text = "Module Not Found";
                    return;
                }

                Offsets.Il2Cpp = moduleAddr;
                Core.Handle = FindRenderWindow(mainHandle);

                var esp = new ESP();
                await esp.Start();

                new Thread(Data.Work) { IsBackground = true }.Start();
                new Thread(Aimbot.Work) { IsBackground = true }.Start();
                new Thread(AimbotAi.Work) { IsBackground = true }.Start();



                statuslbl.ForeColor = Color.LimeGreen;

                statuslbl.Text = "Conneted";
                DoneSound();
                startk = true;
                connected = true;
            }
            else
            {

            }
        }

        private void guna2ToggleSwitch4_CheckedChanged_2(object sender, EventArgs e)
        {
            Config.Notif();
            Config.IgnoreKnocked = guna2ToggleSwitch4.Checked;
            if (Config.IgnoreKnocked)
            {
                statuslbl.Text = "Ignore Knock Activated";
            }
            else
            {
                statuslbl.Text = "Ignore Knock Deactivated";
            }
        }

        private void guna2CirclePictureBox1_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2CirclePictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void guna2ControlBox1_Click_2(object sender, EventArgs e)
        {
            Environment.Exit(0);
            Application.Exit();
        }

        private void guna2ControlBox3_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void guna2ToggleSwitch12_CheckedChanged_1(object sender, EventArgs e)
        {
            Config.Notif();
            Config.AimBot = guna2ToggleSwitch12.Checked;
            if (guna2ToggleSwitch12.Checked)
            {
                statuslbl.Text = "Aimbot Rage Enabled";
            }
            else
            {
                statuslbl.Text = "Aimbot Rage Disabled";
            }
        }

        private void guna2TrackBar1_Scroll_2(object sender, ScrollEventArgs e)
        {
            var aimfov1 = guna2TrackBar1.Value;

            Config.AimBotFov = aimfov1;
        }

        private void guna2TrackBar2_Scroll_2(object sender, ScrollEventArgs e)
        {
            var distance = guna2TrackBar2.Value;
            statuslbl.Text = $"({distance})";
            Config.AimBotMaxDistance = distance;
        }

        private void guna2Button2_Click_2(object sender, EventArgs e)
        {
            HideAllPanels();
            guna2Panel2.Visible = true;
        }

        private void guna2Button3_Click_2(object sender, EventArgs e)
        {
            HideAllPanels();
            guna2Panel3.Visible = true;
        }

        private void guna2Button4_Click_2(object sender, EventArgs e)
        {
            HideAllPanels();
            guna2Panel4.Visible = true;
        }
    }
}