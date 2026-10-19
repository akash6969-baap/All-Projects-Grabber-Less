using AotForms;
using AotForms;
using Celestial;
using Client;
using DiscordRPC;
using Guna.UI2.WinForms;
using Helper;
using Microsoft.VisualBasic.Logging;
using Reborn;
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

        Celestial.Cosmic cosmic = new Celestial.Cosmic();
        public static readonly Cosmic Memory = new Cosmic();


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


            if (e.KeyCode == Keys.F9)
            {
                spwawnkill();
            }


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

        private bool connected = false;
        private bool startk = false;

        private async Task HookGameAsync()
        {
            try
            {
                if (connected)
                {
                    AddStatus("Already connected.");
                    return;
                }

                AddStatus("Establishing connection...");

                var processes = Process.GetProcessesByName("HD-Player");
                if (processes.Length != 1)
                {
                    AddStatus("Error: HD-Player process not found or multiple instances running.");
                    return;
                }

                var process = processes[0];
                var mainModulePath = Path.GetDirectoryName(process.MainModule.FileName);
                var adbPath = Path.Combine(mainModulePath, "HD-Adb.exe");

                if (!File.Exists(adbPath))
                {
                    AddStatus("ADB executable not found.");
                    return;
                }

                var adb = new Adb(adbPath);

                // Kill existing adb (non-blocking)
                await adb.Kill();

                // Start adb
                var started = await adb.Start();
                if (!started)
                {
                    AddStatus("Error: failed to start ADB server.");
                    return;
                }

                string pkg = "com.dts.freefireth";
                string lib = "libil2cpp.so";

                bool isFreeFireMax = false; // set true if you want Max
                if (isFreeFireMax)
                    pkg = "com.dts.freefiremax";

                var moduleAddr = await adb.FindModule(pkg, lib);
                if (moduleAddr == 0)
                {
                    AddStatus("Target module not found.");
                    return;
                }

                Offsets.Il2Cpp = moduleAddr;

                // Ensure mainHandle / FindRenderWindow exist and are valid in your code
                Core.Handle = FindRenderWindow(mainHandle);

                // Start ESP and other components
                var esp = new ESP();
                await esp.Start();

                // Start background worker threads
                new Thread(Data.Work) { IsBackground = true }.Start();
                new Thread(Aimbot.Work) { IsBackground = true }.Start();
                new Thread(SilentC.Work) { IsBackground = true }.Start();
                new Thread(AimbotVisible.Work) { IsBackground = true }.Start();

                AddStatus("Connection established — systems online.");
                DoneSound();

                startk = true;
                connected = true;
            }
            catch (Exception ex)
            {
                // show error nicely
                AddStatus("Connection failed: " + ex.Message);
            }

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


        }
        private void guna2CustomCheckBox27_Click(object sender, EventArgs e)
        {
            Config.Notif();
            Config.NoRecoil = guna2CustomCheckBox27.Checked;
            if (guna2CustomCheckBox27.Checked)
            {
                statuslbl.Text = "No Recoil Enabled";
            }
            else
            {
                statuslbl.Text = "No Recoil Disabled";
            }
        }

        private void guna2CustomCheckBox29_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox28_Click(object sender, EventArgs e)
        {
            Config.Notif();
            Config.IgnoreKnocked = guna2CustomCheckBox28.Checked;
            if (guna2CustomCheckBox28.Checked)
            {
                statuslbl.Text = "Ignore Knock Enabled";
            }
            else
            {
                statuslbl.Text = "Ignore Knock Disabled";
            }


        }

        private void guna2CustomCheckBox26_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox34_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox33_Click(object sender, EventArgs e)
        {


        }

        private void guna2CustomCheckBox32_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox31_Click(object sender, EventArgs e)
        {


        }

        private void guna2CustomCheckBox30_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox39_Click(object sender, EventArgs e)
        {

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
            aimbotpanel.Location = new Point(8, 66);
            guna2Panel4.Location = new Point(9999, 9999);
            configpanel.Location = new Point(9999, 9999);
        }

        private void guna2ControlBox1_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
            Environment.Exit(0);
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
            guna2Panel4.Location = new Point(8, 66);
            configpanel.Location = new Point(9999, 9999);
            aimbotpanel.Location = new Point(9999, 9999);
        }

        private void rebornLabel6_Click(object sender, EventArgs e)
        {
            rebornLabel6.ForeColor = Color.White;
            rebornLabel5.ForeColor = Color.FromArgb(75, 75, 75);
            rebornLabel1.ForeColor = Color.FromArgb(75, 75, 75);
            rebornLabel7.ForeColor = Color.FromArgb(75, 75, 75);
            configpanel.Location = new Point(8, 66);
            guna2Panel4.Location = new Point(9999, 9999);
            aimbotpanel.Location = new Point(9999, 9999);

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

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void label19_Click(object sender, EventArgs e)
        {

        }

        private void trackBarFloatClient4_Event_0(object sender, EventArgs e)
        {
            var Aimfov = trackBarFloatClient4.Value;

            Config.Aimfov = Aimfov;
        }

        private void trackBarFloatClient1_Event_0_1(object sender, EventArgs e)
        {

        }

        private void trackBarFloatClient2_Event_0_1(object sender, EventArgs e)
        {
            var distance = trackBarFloatClient2.Value;

            Config.AimBotMaxDistance = distance;
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



        public void ghoston()
        {


        }
        public void ghostoff()
        {

        }


        private List<long> UNCLEaddress = new List<long>();
        private string UNCLEsearch;
        private string UNCLEreplace;
        private string UNCLENORMAL;
        private async void guna2CustomCheckBox7_Click_2(object sender, EventArgs e)
        {

        }

        private void AddStatus(string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            guna2TextBox1.AppendText($"[{time}] {message}\r\n");  // messages TextBox me add honge
            guna2TextBox1.SelectionStart = guna2TextBox1.TextLength;
            guna2TextBox1.ScrollToCaret();  // auto-scroll
        }




        private void guna2CustomCheckBox6_Click_3(object sender, EventArgs e)
        {

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
        private IntPtr GetSafeIntPtr(long address, Process targetProcess)
        {
            bool isTarget64Bit = false;

            try
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    // Check if target process is 64-bit
                    isTarget64Bit = (targetProcess.Modules[0].ModuleMemorySize > uint.MaxValue) || Environment.Is64BitProcess;
                }
            }
            catch
            {
                isTarget64Bit = false;
            }

            if (isTarget64Bit)
                return new IntPtr(address);
            else
            {
                if (address > int.MaxValue)
                    throw new OverflowException("Address too high for 32-bit process!");
                return new IntPtr((int)address);
            }
        }

        private bool IsTarget64Bit(Process targetProcess)
        {
            if (!Environment.Is64BitOperatingSystem) return false;
            try
            {
                return (targetProcess.Modules[0].ModuleMemorySize > uint.MaxValue) || Environment.Is64BitProcess;
            }
            catch
            {
                return false;
            }
        }
        private void newmain_Paint(object sender, PaintEventArgs e)
        {

        }

        private void status_Click(object sender, EventArgs e)
        {

        }

        private void guna2Panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button2_Click_2(object sender, EventArgs e)
        {
            Config.Notif();
            aimbotpanel.Show();
            guna2Panel4.Hide();
            configpanel.Hide();
        }

        private void guna2Button3_Click_2(object sender, EventArgs e)
        {
            Config.Notif();
            aimbotpanel.Hide();
            guna2Panel4.Show();
            configpanel.Hide();
        }

        private void guna2Button4_Click_2(object sender, EventArgs e)
        {
            Config.Notif();
            aimbotpanel.Hide();
            guna2Panel4.Hide();
            configpanel.Show();
        }

        private void guna2Button5_Click_1(object sender, EventArgs e)
        {
            Config.ESPLine = true;       // Enable ESP Line
            statuslbl.Text = "ESP Line Enabled";
            Config.Notif();              // Optional: show notification
        }

        private void guna2Button6_Click(object sender, EventArgs e)
        {
            Config.ESPLine = false;      // Disable ESP Line
            statuslbl.Text = "ESP Line Disabled";
            Config.Notif();              // Optional: show notification
        }

        private Celestial.Cosmic memory = new Celestial.Cosmic();
        private List<long> foundBases = new List<long>();
        private Dictionary<long, int> originalValues = new Dictionary<long, int>();

        // Neck Offsets
        private const long NECK_READ_OFFSET = 0xAA;
        private const long NECK_WRITE_OFFSET = 0xA6;

        // Drag Offsets
        private const long DRAG_READ_OFFSET = 0xD2;
        private const long DRAG_WRITE_OFFSET = 0x9E;

        // Common AoB Pattern
        private const string AOB_PATTERN =
        @"FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 FF FF FF FF FF FF FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 00 00 00 00 00 00 00 00 00 00 A5 43 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ?? ?? ?? ?? 00 00 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 00 00";


        // === LOAD Button ===

        private async void guna2Button22_Click(object sender, EventArgs e)
        {
            try
            {
                AddStatus("🔎 Attaching to emulator...");
                bool attached = memory.SetProcess(new string[] { "HD-Player", "HD-Player64", "hd-player", "HD-Player.exe" });

                if (!attached || memory._processHandle == IntPtr.Zero || memory.processId == 0)
                {
                    AddStatus("❌ Emulator not found / attach failed. Start emulator and try again.");
                    return;
                }

                AddStatus($"✅ Attached to process (pid: {memory.processId}). Starting AoB scan...");

                var scanResults = await memory.AoBScan(AOB_PATTERN);

                if (scanResults == null || !scanResults.Any())
                {
                    AddStatus("❌ Pattern not found. (0 results)");
                    return;
                }

                foundBases = scanResults.ToList();
                originalValues.Clear();

                AddStatus($"✅ Scan completed. Found {foundBases.Count} base addresses.");
            }
            catch (Exception ex)
            {
                AddStatus("❌ Error in Load: " + ex.Message);
            }
        }

        private void guna2Button38_Click(object sender, EventArgs e)
        {
            Config.ESPLine = true;
            AddStatus("ESP Line Activated"); // Activated status
        }

        private void guna2Button27_Click(object sender, EventArgs e)
        {
            Config.ESPLine = false;
            AddStatus("ESP Line Deactivated"); // Deactivated status
        }

        private void guna2Button26_Click(object sender, EventArgs e)
        {
            Config.ESPBox2 = true;
            AddStatus("ESP Box Activated"); // Activated status
        }

        private void guna2Button25_Click(object sender, EventArgs e)
        {
            Config.ESPBox2 = false;
            AddStatus("ESP Box Deactivated"); // Activated status
        }

        private void guna2Button24_Click(object sender, EventArgs e)
        {
            Config.ESPFillBox = true;
            AddStatus("ESP Fill Box Activated"); // Activated status
        }

        private void guna2Button23_Click(object sender, EventArgs e)
        {
            Config.ESPFillBox = false;
            AddStatus("ESP Fill Box Deactivated"); // Deactivated status
        }

        private void guna2Button35_Click(object sender, EventArgs e)
        {
            Config.ESPName = true;
            AddStatus("ESP Name Activated"); // Activated status
        }

        private void guna2Button34_Click(object sender, EventArgs e)
        {
            Config.ESPName = false;
            AddStatus("ESP Name Deactivated"); // Deactivated status
        }

        private void guna2Button33_Click(object sender, EventArgs e)
        {
            Config.ESPHealth = true;
            AddStatus("ESP Health Activated"); // Activated status
        }

        private void guna2Button32_Click(object sender, EventArgs e)
        {
            Config.ESPHealth = false;
            AddStatus("ESP Health Deactivated"); // Deactivated status
        }

        private void guna2Button31_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button29_Click(object sender, EventArgs e)
        {
            Config.minimap = true;
            AddStatus("Mini Map Activated"); // Activated status
        }

        private void guna2Button28_Click(object sender, EventArgs e)
        {
            Config.minimap = false;
            AddStatus("Mini Map Deactivated"); // Deactivated status
        }

        private void guna2Button37_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button36_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button10_Click(object sender, EventArgs e)
        {
            Config.AIMVISIBLE = true;
            AddStatus("Aimbot Visible Activated"); // Activated status
        }

        private void guna2Button9_Click(object sender, EventArgs e)
        {
            Config.AIMVISIBLE = false;
            AddStatus("Aimbot AI Deactivated"); // Deactivated status
        }

        private void guna2Button8_Click(object sender, EventArgs e)
        {
            Config.AimBot = true;
            AddStatus("Aimbot Activated"); // Activated status
        }

        private void guna2Button7_Click(object sender, EventArgs e)
        {
            Config.AimBot = false;
            AddStatus("Aimbot Deactivated"); // Deactivated status
        }

        private void guna2Button12_Click(object sender, EventArgs e)
        {
            Config.SILENT = true;
            AddStatus("Silent Aim Activated"); // Activated status
        }

        private void guna2Button11_Click(object sender, EventArgs e)
        {
            Config.SILENT = false;
            AddStatus("Silent Aim Deactivated"); // Deactivated status
        }

        private void guna2Button40_Click(object sender, EventArgs e)
        {
            Config.Aimfovc = true;
            AddStatus("Aim FOV Circle Activated"); // Activated status
        }

        private void guna2Button39_Click(object sender, EventArgs e)
        {
            Config.Aimfovc = false;
            AddStatus("Aim FOV Circle Deactivated"); // Deactivated status
        }

        private async void guna2Button30_Click(object sender, EventArgs e)
        {
            // Disable button to prevent concurrent clicks
            guna2Button30.Enabled = false;
            guna2Button30.Text = "Connecting...";

            // Call the async connect method
            await HookGameAsync();

            // Re-enable button (you can keep it disabled if you want persistent connect)
            guna2Button30.Enabled = true;
            guna2Button30.Text = "Connected";   // show Connected inside button
        }

        private void guna2Button14_Click(object sender, EventArgs e)
        {
            try
            {
                if (!EnsureAttached()) return;
                if (!EnsureLoaded()) return;

                int writes = 0;
                foreach (var baseAddr in foundBases)
                {
                    long readAddr = baseAddr + NECK_READ_OFFSET;
                    long writeAddr = baseAddr + NECK_WRITE_OFFSET;

                    try
                    {
                        // Backup if missing
                        if (!originalValues.ContainsKey(writeAddr))
                            originalValues[writeAddr] = memory.ReadInt(writeAddr);

                        int val = memory.ReadInt(readAddr);
                        if (memory.AobReplace(writeAddr, val)) writes++;
                    }
                    catch { }
                }

                AddStatus($"🎯 Neck Aimbot ON ({writes}/{foundBases.Count})");
                Console.Beep(2000, 150);
            }
            catch (Exception ex)
            {
                AddStatus("❌ Error in Neck ON: " + ex.Message);
            }
        }

        private void guna2Button13_Click(object sender, EventArgs e)
        {
            try
            {
                if (!EnsureAttached()) return;
                if (!EnsureLoaded()) return;

                int restores = 0;
                foreach (var kv in originalValues)
                {
                    if (memory.AobReplace(kv.Key, kv.Value)) restores++;
                }

                AddStatus($"♻️ Neck Aimbot OFF ({restores}/{originalValues.Count})");
                Console.Beep(1000, 150);
            }
            catch (Exception ex)
            {
                AddStatus("❌ Error in Neck OFF: " + ex.Message);
            }
        }
        private bool EnsureAttached()
        {
            if (memory._processHandle == IntPtr.Zero || memory.processId == 0)
            {
                AddStatus("ℹ️ Process not attached. Reattaching...");
                memory.SetProcess(new string[] { "HD-Player", "HD-Player64", "hd-player", "HD-Player.exe" });
            }
            return memory._processHandle != IntPtr.Zero && memory.processId != 0;
        }

        private bool EnsureLoaded()
        {
            if (foundBases == null || foundBases.Count == 0)
            {
                AddStatus("⚠️ No scanned addresses. Click LOAD first!");
                return false;
            }
            return true;
        }

        // Store addresses
        private IEnumerable<long> speedHackAddresses = Enumerable.Empty<long>();
        private async void guna2Button42_Click(object sender, EventArgs e)
        {
            if (!cosmic.SetProcess(new string[] { "HD-Player" }))
            {
                AddStatus("❌ Emulator not found!");
                return;
            }

            cosmic.CheckProcess();

            AddStatus("🔍 Scanning for Speed Hack...");

            string originalPattern = "40 02 2B 07 3D 02 2B 07 3D 02 2B 07 3D 00 00 00";
            speedHackAddresses = await cosmic.AoBScan(originalPattern);

            if (speedHackAddresses.Any())
                AddStatus($"✅ Speed Hack Found ({speedHackAddresses.Count()})");
            else
                AddStatus("❌ Speed Hack Not Found (check pattern or emulator version)");
        }

        private void guna2Button41_Click(object sender, EventArgs e)
        {
            if (!speedHackAddresses.Any())
            {
                AddStatus("⚠️ Please load Speed Hack first!");
                return;
            }

            string replacementPattern = "40 02 2B 9B 3C 02 2B 9B 3C 02 2B 07 3D 00 00 00";

            int patched = 0;
            foreach (var address in speedHackAddresses)
            {
                if (cosmic.AobReplace(address, replacementPattern))
                    patched++;
            }

            AddStatus($"⚡ Speed Hack Enabled ({patched})");
        }

        private void guna2Button31_Click_1(object sender, EventArgs e)
        {
            if (!speedHackAddresses.Any())
            {
                AddStatus("⚠️ Please load Speed Hack first!");
                return;
            }

            string originalPattern = "40 02 2B 07 3D 02 2B 07 3D 02 2B 07 3D 00 00 00";

            int patched = 0;
            foreach (var address in speedHackAddresses)
            {
                if (cosmic.AobReplace(address, originalPattern))
                    patched++;
            }

            AddStatus($"🛑 Speed Hack Disabled ({patched})");
        }


        // Addresses store karne ke liye
        private IEnumerable<long> wallHackAddresses = Enumerable.Empty<long>();
        private async void guna2Button45_Click(object sender, EventArgs e)
        {
            if (!cosmic.SetProcess(new string[] { "HD-Player" }))
            {
                AddStatus("❌ Emulator not found!");
                return;
            }

            cosmic.CheckProcess();

            AddStatus("🔍 Scanning for Wall Hack...");

            string originalPattern = "00 00 00 00 00 AE 47 81 3F AE 47 81 3F AE 47 81 3F AE 47 81 3F 00 1A B7 EE DC 3A 9F ED 30 00 4F E2 43 2A B0 EE EF 0A 60 F4 43 6A F0 EE 1C 00 8A E2 43 5A F0 EE 8F 0A 48 F4 43 2A F0 EE 43 7A B0 EE 8F 0A 40 F4 41 AA B0 EE FE";
            wallHackAddresses = await cosmic.AoBScan(originalPattern);

            if (wallHackAddresses.Any())
                AddStatus($"✅ Wall Hack Found ({wallHackAddresses.Count()})");
            else
                AddStatus("❌ Wall Hack Not Found");
        }

        private void guna2Button44_Click(object sender, EventArgs e)
        {
            if (!wallHackAddresses.Any())
            {
                AddStatus("⚠️ Please load Wall Hack first!");
                return;
            }

            string replacementPattern = "00 00 00 00 00 AE 47 81 3F AE 47 81 3F AE 47 81 BF AE 47 81 3F AE 47 81 BF AE 47 81 3F 00 1A B7 EE DC 3A 9F ED";

            int patched = 0;
            foreach (var address in wallHackAddresses)
            {
                if (cosmic.AobReplace(address, replacementPattern))
                    patched++;
            }

            AddStatus($"🟢 Wall Hack Enabled ({patched})");
        }

        private void guna2Button43_Click(object sender, EventArgs e)
        {
            if (!wallHackAddresses.Any())
            {
                AddStatus("⚠️ Please load Wall Hack first!");
                return;
            }

            string originalPattern = "00 00 00 00 00 AE 47 81 3F AE 47 81 3F AE 47 81 3F AE 47 81 3F 00 1A B7 EE DC 3A 9F ED 30 00 4F E2 43 2A B0 EE EF 0A 60 F4 43 6A F0 EE 1C 00 8A E2 43 5A F0 EE 8F 0A 48 F4 43 2A F0 EE 43 7A B0 EE 8F 0A 40 F4 41 AA B0 EE FE";

            int patched = 0;
            foreach (var address in wallHackAddresses)
            {
                if (cosmic.AobReplace(address, originalPattern))
                    patched++;
            }

            AddStatus($"🔴 Wall Hack Disabled ({patched})");
        }

        // Store addresses for Wall Hack V2
        private IEnumerable<long> wallHackV2Addresses = Enumerable.Empty<long>();
        private async void guna2Button48_Click(object sender, EventArgs e)
        {
            if (!cosmic.SetProcess(new string[] { "HD-Player" }))
            {
                AddStatus("❌ Emulator not found!");
                return;
            }

            cosmic.CheckProcess();

            AddStatus("🔍 Scanning for WALL HACK V2...");

            // original / search pattern provided by you
            string originalPattern = "3F AE 47 81 3F AE 47 81 3F AE 47 81 3F 00 1A B7 EE DC 3A 9F ED 30 00 4F E2 43 2A B0 EE EF 0A 60 F4 43 6A F0 EE 1C 00 8A E2 43 5A F0 EE 8F 0A 48 F4 43 2A F0 EE 43 7A B0 EE 8F 0A 40 F4 41 AA B0 EE FE 05 A0 E3 41 1A F0 EE 2C 00 8A E5 10";

            wallHackV2Addresses = await cosmic.AoBScan(originalPattern);

            if (wallHackV2Addresses != null && wallHackV2Addresses.Any())
                AddStatus($"✅ WALL HACK V2 Found ({wallHackV2Addresses.Count()})");
            else
                AddStatus("❌ WALL HACK V2 Not Found (check pattern or game/emulator version)");
        }

        private void guna2Button47_Click(object sender, EventArgs e)
        {
            if (wallHackV2Addresses == null || !wallHackV2Addresses.Any())
            {
                AddStatus("⚠️ Please load WALL HACK V2 first!");
                return;
            }

            // replacement pattern provided by you
            string replacementPattern = "3F AE 47 81 3F AE 47 81 BF AE 47 81 3F AE 1A B7 EE DC 3A 9F ED 30 00 4F E2 43 2A B0 EE EF 0A 60 F4 43 6A F0 EE 1C 00 8A E2 43 5A F0 EE 8F 0A 48 F4 43 2A F0 EE 43 7A B0 EE 8F 0A 40 F4 41 AA B0 EE FE 05 A0 E3 41 1A F0 EE 2C 00 8A E5 10";

            int patched = 0;
            foreach (var address in wallHackV2Addresses)
            {
                try
                {
                    if (cosmic.AobReplace(address, replacementPattern))
                        patched++;
                }
                catch (Exception ex)
                {
                    // optional: log per-address error
                    AddStatus($"⚠️ Error patching {address:X}: {ex.Message}");
                }
            }

            AddStatus($"⚡ WALL HACK V2 Enabled ({patched})");
        }

        private void guna2Button46_Click(object sender, EventArgs e)
        {
            if (wallHackV2Addresses == null || !wallHackV2Addresses.Any())
            {
                AddStatus("⚠️ Please load WALL HACK V2 first!");
                return;
            }

            // restore with original/search pattern
            string originalPattern = "3F AE 47 81 3F AE 47 81 3F AE 47 81 3F 00 1A B7 EE DC 3A 9F ED 30 00 4F E2 43 2A B0 EE EF 0A 60 F4 43 6A F0 EE 1C 00 8A E2 43 5A F0 EE 8F 0A 48 F4 43 2A F0 EE 43 7A B0 EE 8F 0A 40 F4 41 AA B0 EE FE 05 A0 E3 41 1A F0 EE 2C 00 8A E5 10";

            int restored = 0;
            foreach (var address in wallHackV2Addresses)
            {
                try
                {
                    if (cosmic.AobReplace(address, originalPattern))
                        restored++;
                }
                catch (Exception ex)
                {
                    AddStatus($"⚠️ Error restoring {address:X}: {ex.Message}");
                }
            }

            AddStatus($"🛑 WALL HACK V2 Disabled ({restored})");
        }

        // Store addresses for Vision 10x
        private IEnumerable<long> vision10xAddresses = Enumerable.Empty<long>();
        private async void guna2Button51_Click(object sender, EventArgs e)
        {
            if (!cosmic.SetProcess(new string[] { "HD-Player" }))
            {
                AddStatus("❌ Emulator/game process not found!");
                return;
            }

            cosmic.CheckProcess();

            AddStatus("🔍 Scanning for Vision 10x...");

            // Search pattern (S) you provided
            string searchPattern = "DB 0F 49 40 10 2A 00 EE 00 10 80 E5 10 3A 01 EE 14 10 80 E5 00 2A 30 EE 00 10 00 E3 41 3A 30 EE 80 1F 4B E3 01 0A 30 EE 2C 10 80 E5 50 00 C0 F2 04 10 80 E2 00 20 A0 E3 30 20 80 E5 34 20 80 E5 01 1A 22 EE 3C 20 80 E5 8F 0A 41 F4 18 10 80 E2 03 0A 80 EE 03 1A 81 EE 8F 0A 41 F4 0A 0A 80 ED";

            vision10xAddresses = await cosmic.AoBScan(searchPattern);

            if (vision10xAddresses != null && vision10xAddresses.Any())
                AddStatus($"✅ Vision 10x Found ({vision10xAddresses.Count()})");
            else
                AddStatus("❌ Vision 10x Not Found (check pattern or emulator/game version)");
        }

        private void guna2Button50_Click(object sender, EventArgs e)
        {
            if (vision10xAddresses == null || !vision10xAddresses.Any())
            {
                AddStatus("⚠️ Please load Vision 10x first!");
                return;
            }

            // Replacement pattern (R) you provided
            string replacementPattern = "DB 0F E3 40 10 2A 00 EE 00 10 80 E5 10 3A 01 EE 14 10 80 E5 00 2A 30 EE 00 10 00 E3 41 3A 30 EE 80 1F 4B E3 01 0A 30 EE 2C 10 80 E5 50 00 C0 F2 04 10 80 E2 00 20 A0 E3 30 20 80 E5 34 20 80 E5 01 1A 22 EE 3C 20 80 E5 8F 0A 41 F4 18 10 80 E2 03 0A 80 EE 03 1A 81 EE 8F 0A 41 F4 0A 0A 80 ED";

            int patched = 0;
            foreach (var address in vision10xAddresses)
            {
                try
                {
                    if (cosmic.AobReplace(address, replacementPattern))
                        patched++;
                }
                catch (Exception ex)
                {
                    AddStatus($"⚠️ Error patching {address:X}: {ex.Message}");
                }
            }

            AddStatus($"⚡ Vision 10x Enabled ({patched})");
        }

        private void guna2Button49_Click(object sender, EventArgs e)
        {
            if (vision10xAddresses == null || !vision10xAddresses.Any())
            {
                AddStatus("⚠️ Please load Vision 10x first!");
                return;
            }

            // Restore with original/search pattern
            string originalPattern = "DB 0F 49 40 10 2A 00 EE 00 10 80 E5 10 3A 01 EE 14 10 80 E5 00 2A 30 EE 00 10 00 E3 41 3A 30 EE 80 1F 4B E3 01 0A 30 EE 2C 10 80 E5 50 00 C0 F2 04 10 80 E2 00 20 A0 E3 30 20 80 E5 34 20 80 E5 01 1A 22 EE 3C 20 80 E5 8F 0A 41 F4 18 10 80 E2 03 0A 80 EE 03 1A 81 EE 8F 0A 41 F4 0A 0A 80 ED";

            int restored = 0;
            foreach (var address in vision10xAddresses)
            {
                try
                {
                    if (cosmic.AobReplace(address, originalPattern))
                        restored++;
                }
                catch (Exception ex)
                {
                    AddStatus($"⚠️ Error restoring {address:X}: {ex.Message}");
                }
            }

            AddStatus($"🛑 Vision 10x Disabled ({restored})");
        }

        // form-level fields (agar pehle se nahi hain)
        IEnumerable<long> sniperScopeAddresses = Enumerable.Empty<long>();
        IEnumerable<long> sniperSwitchAddresses = Enumerable.Empty<long>();
        IEnumerable<long> sniperAimAddresses = Enumerable.Empty<long>();

        // patterns (tumhare diye hue)
        string scopeOriginal = "FF FF FF FF 08 00 00 00 00 00 60 40 CD CC 8C 3F 8F C2 F5 3C CD CC CC 3D 06 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 3F 33 33 13 40 00 00 B0 3F 00 00 80 3F 01";
        string scopeReplacement = "FF FF FF FF 08 00 00 00 00 00 60 40 CD CC 8C 3F 8F C2 F5 3C CD CC CC 3D 06 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 01 33 33 13 40 00 00 B0 3F 00 00 80 3F 01";

        string switchOriginal = "05 00 00 00 01 00 00 00 B4 C8 D6 3F 01 00 00 00 B4 C8 D6 3F 00 00 00 00 B4 C8 D6 3F 00 00 80 3F 00 00 80 3F 0A D7 A3 3D 00 00 00 00 00 00 5C 43 00 00 90 42 00 00 B4 42 96 00 00 00 00 00 00 00 00 00 00 3F 00 00 80 3E 00 00 00 00 04 00 00 00 00 00 80 3F 00 00 20 41 00 00 34 42 01 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 80 3F 8F C2 35 3F 9A 99 99 3F 00 00 80 3F 00 00 00 00 00 00 80 3F 00 00 80 3F 00 00 80 3F 00 00 00 00 00 00 00 00 00 00";
        string switchReplacement = "05 00 00 00 01 00 00 00 B4 C8 D6 3F 01 00 00 00 B4 C8 D6 3F 00 00 00 00 B4 C8 D6 3F 00 00 80 3F 00 00 80 3F 0A D7 A3 3D 00 00 00 00 00 00 5C 43 00 00 90 42 00 00 B4 42 96 00 00 00 00 00 00 00 00 00 00 3F 00 00 80 3E 00 00 00 3C 04 00 00 00 00 00 80 3F 00 00 20 41 00 00 34 42 01 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 80 3F 8F C2 35 3F 9A 99 99 3F 00 00 80 3F 00 00 00 00 00 00 80 3F 00 00 80 3F 00 00 80 3F 00 00 00 00 00 00 00 00 00 00";

        string aimSearch = "EA 00 60 A0 E3 06 00 A0 E1 18 D0 4B E2 02 8B BD EC 70 8C";
        string aimReplace = "01 00 AF E3";


        // === LOAD button (updated) ===

        private async void guna2Button21_Click(object sender, EventArgs e)
        {
            try
            {
                if (!cosmic.SetProcess(new string[] { "HD-Player" }))
                {
                    AddStatus("❌ Emulator Not Found!");
                    return;
                }

                cosmic.CheckProcess();

                // Sniper Feature Scan (Scope + Switch + Aim)
                AddStatus("🔍 Initializing Sniper Functions...");

                // do scans (scope, switch, aim)
                sniperScopeAddresses = await cosmic.AoBScan(scopeOriginal);
                sniperSwitchAddresses = await cosmic.AoBScan(switchOriginal);
                sniperAimAddresses = await cosmic.AoBScan(aimSearch);

                int cntScope = sniperScopeAddresses?.Count() ?? 0;
                int cntSwitch = sniperSwitchAddresses?.Count() ?? 0;
                int cntAim = sniperAimAddresses?.Count() ?? 0;

                AddStatus($"🔎 Results — Scope: {cntScope}, Switch: {cntSwitch}, Aim: {cntAim}");

                if (cntScope > 0 && cntSwitch > 0)
                {
                    AddStatus("✅ Sniper Architect Enabled");
                }
                else
                {
                    AddStatus("❌ Error: Sniper features incomplete (scope/switch missing)");
                }
            }
            catch (Exception ex)
            {
                AddStatus("❌ Error scanning sniper features: " + ex.Message);
            }
        }

        private void guna2Button20_Click(object sender, EventArgs e)
        {
            if (!sniperScopeAddresses.Any()) { AddStatus("⚠️ Load Sniper Feature First!"); return; }

            int patched = sniperScopeAddresses.Count(addr => cosmic.AobReplace(addr, scopeReplacement));
            AddStatus($"✅ Sniper Auto Aim Set To Enemy ({patched})");
        }

        private void guna2Button19_Click(object sender, EventArgs e)
        {
            if (!sniperScopeAddresses.Any()) { AddStatus("⚠️ Load Sniper Feature First!"); return; }

            int restored = sniperScopeAddresses.Count(addr => cosmic.AobReplace(addr, scopeOriginal));
            AddStatus($"⛔ Sniper Auto Aim Set To Normal ({restored})");
        }

        private void guna2Button18_Click(object sender, EventArgs e)
        {
            if (!sniperSwitchAddresses.Any()) { AddStatus("⚠️ Load Sniper Feature First!"); return; }

            int patched = sniperSwitchAddresses.Count(addr => cosmic.AobReplace(addr, switchReplacement));
            AddStatus($"✅ Sniper Fast Switch Enabled ({patched})");
        }

        private void guna2Button17_Click(object sender, EventArgs e)
        {
            if (!sniperSwitchAddresses.Any()) { AddStatus("⚠️ Load Sniper Feature First!"); return; }

            int restored = sniperSwitchAddresses.Count(addr => cosmic.AobReplace(addr, switchOriginal));
            AddStatus($"⛔ Sniper Fast Switch Disabled ({restored})");
        }

        private void guna2Button16_Click(object sender, EventArgs e)
        {
            try
            {
                if (!sniperAimAddresses.Any()) { AddStatus("⚠️ Load Sniper Feature First!"); return; }

                int patched = sniperAimAddresses.Count(addr => cosmic.AobReplace(addr, aimReplace));
                AddStatus($"✅ Sniper Delay Enabled ({patched})");
            }
            catch (Exception ex)
            {
                AddStatus("❌ Error enabling delay: " + ex.Message);
            }
        }

        private void guna2Button15_Click(object sender, EventArgs e)
        {
            try
            {
                if (!sniperAimAddresses.Any()) { AddStatus("⚠️ Load Sniper Feature First!"); return; }

                int restored = sniperAimAddresses.Count(addr => cosmic.AobReplace(addr, aimSearch));
                AddStatus($"⛔ Sniper Delay Disabled ({restored})");
            }
            catch (Exception ex)
            {
                AddStatus("❌ Error disabling Delay: " + ex.Message);
            }
        }

        private void guna2Button64_Click(object sender, EventArgs e)
        {
            TopMost = true;
            AddStatus("✅ Window set to TopMost");
        }

        private void guna2Button63_Click(object sender, EventArgs e)
        {
            TopMost = false;
            AddStatus("🛑 TopMost disabled");
        }

        private void guna2Button62_Click(object sender, EventArgs e)
        {
            ShowInTaskbar = false;
            AddStatus("✅ Window hidden from taskbar");
        }

        private void guna2Button61_Click(object sender, EventArgs e)
        {
            ShowInTaskbar = true;
            AddStatus("🟢 Window visible in taskbar");
        }


        private void SetStreamMode(bool state)
        {
            foreach (var obj in Application.OpenForms)
            {
                var form = obj as Form;
                if (form == null) continue;

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
        private void guna2Button60_Click(object sender, EventArgs e)
        {
            SetStreamMode(true);
            AddStatus("✅ Stream mode enabled (window excluded from capture)");
        }

        private void guna2Button59_Click(object sender, EventArgs e)
        {
            SetStreamMode(false);
            AddStatus("🛑 Stream mode disabled (window can be captured)");
        }

        private void guna2Button56_Click(object sender, EventArgs e)
        {
            Config.sound = true;
            Config.Notif(); // call notification setup if needed
            AddStatus("✅ Notification sound enabled");
        }

        private void guna2Button55_Click(object sender, EventArgs e)
        {
            Config.sound = false;
            Config.Notif(); // update config
            AddStatus("🛑 Notification sound disabled");
        }



        private void guna2Button66_Click(object sender, EventArgs e)
        {
            SetStreamMode(true);
            Config.StreamMode = true;
            Config.sound = true;   // enable sound as well
            AddStatus("✅ Stream Mode enabled (window excluded from capture, sound enabled)");
        }

        private void guna2Button65_Click(object sender, EventArgs e)
        {
            SetStreamMode(false);
            Config.StreamMode = false;
            Config.sound = false;  // disable sound
            AddStatus("🛑 Stream Mode disabled (window can be captured, sound disabled)");
        }

        private void guna2Button58_Click(object sender, EventArgs e)
        {
            Config.Notif();

            // Add rules
            ExecuteCommand(@"netsh advfirewall firewall add rule name=""TemporaryBlock0"" dir=in action=block profile=any program=""C:\Program Files\BlueStacks_nxt\HD-Player.exe""");
            ExecuteCommand(@"netsh advfirewall firewall add rule name=""TemporaryBlock0"" dir=out action=block profile=any program=""C:\Program Files\BlueStacks_nxt\HD-Player.exe""");
            ExecuteCommand(@"netsh advfirewall firewall add rule name=""TemporaryBlock1"" dir=in action=block profile=any program=""C:\Program Files\BlueStacks_msi5\HD-Player.exe""");
            ExecuteCommand(@"netsh advfirewall firewall add rule name=""TemporaryBlock1"" dir=out action=block profile=any program=""C:\Program Files\BlueStacks_msi5\HD-Player.exe""");
            ExecuteCommand(@"netsh advfirewall firewall add rule name=""TemporaryBlock2"" dir=in action=block profile=any program=""C:\Program Files\BlueStacks\HD-Player.exe""");
            ExecuteCommand(@"netsh advfirewall firewall add rule name=""TemporaryBlock2"" dir=out action=block profile=any program=""C:\Program Files\BlueStacks\HD-Player.exe""");
            ExecuteCommand(@"netsh advfirewall firewall add rule name=""TemporaryBlock3"" dir=in action=block profile=any program=""C:\Program Files\BlueStacks_msi2\Bluestacks.exe""");
            ExecuteCommand(@"netsh advfirewall firewall add rule name=""TemporaryBlock3"" dir=in action=block profile=any program=""C:\Program Files\BlueStacks_msi2\HD-Player.exe""");
            ExecuteCommand(@"netsh advfirewall firewall add rule name=""TemporaryBlock3"" dir=out action=block profile=any program=""C:\Program Files\BlueStacks_msi2\HD-Player.exe""");

            AddStatus("✅ Firewall rules applied (Bluestacks blocked)");
        }

        private void guna2Button57_Click(object sender, EventArgs e)
        {
            DeleteFirewallRule(@"C:\Program Files\BlueStacks_nxt\HD-Player.exe");
            DeleteFirewallRule(@"C:\Program Files\BlueStacks_msi5\HD-Player.exe");
            DeleteFirewallRule(@"C:\Program Files\BlueStacks\HD-Player.exe");
            DeleteFirewallRule(@"C:\Program Files\BlueStacks_msi2\HD-Player.exe");

            AddStatus("🛑 Firewall rules removed (Bluestacks unblocked)");
        }

        private void guna2Button67_Click(object sender, EventArgs e)
        {
            Core.Entities = new();
            InternalMemory.Cache = new();
        }
    }
}
