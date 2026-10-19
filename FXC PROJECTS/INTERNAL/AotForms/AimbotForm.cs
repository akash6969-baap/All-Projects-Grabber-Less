using AotForms;
using CXMem;
using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Net;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Guna.UI2.Native.WinApi;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using FearAuth;

namespace Client
{
    public partial class AimbotForm : Form
    {

        public static api FearAuthApp = new api(
   name: "INTERNAL",
   ownerid: "e281b844-6413-4fb5-b9ad-3287d85ddb59",
   secret: "f3094fbb7330b8b90a2ac8fae700cbc16fbf0242bc9a7c4b",
   version: "1.0"

);

        public static Label StatusLbl;
        IntPtr mainHandle;


        CaXMem memory = new CaXMem();


        public AimbotForm(IntPtr handle)
        {
            mainHandle = handle;
            InitializeComponent();


        }

        private void ShowPanel(Panel panelToShow)
        {
            // Hide all panels
            suarpanel.Visible = false;
            landpanel.Visible = false;
            jhantpanel.Visible = false;
            chutpanel.Visible = false;


            // Show the specified panel
            panelToShow.Visible = true;
        }
        private void guna2ToggleSwitch1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void guna2ToggleSwitch3_CheckedChanged(object sender, EventArgs e)
        {
            Config.Notif();
        }

        private void guna2TrackBar2_Scroll(object sender, ScrollEventArgs e)
        {

            var DELAY = guna2TrackBar2.Value;

            lblDistance.Text = $"({DELAY})";

            Config.aimdelay = DELAY;
        }
        private void guna2TrackBar1_Scroll(object sender, ScrollEventArgs e)
        {
            var distance = guna2TrackBar1.Value;
            aimfovtxt.Text = $"({distance})";
            Config.Aimfov = guna2TrackBar1.Value;
            // DrawSmoothCircle(Config.Aimfov, ColorToUint32(Config.Aimfovcolor), 1.0f);
        }

        private void guna2TrackBar3_Scroll(object sender, ScrollEventArgs e)
        {
            var distance = guna2TrackBar3.Value;

            rangeaim.Text = $"({distance})";

            Config.AimBotMaxDistance = distance;
        }

        private void AimbotForm_Load(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox9_Click(object sender, EventArgs e)
        {

        }


        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //ComboBox comboBox = (ComboBox)sender;
            //string selectedItem = (string)comboBox.SelectedItem;

            //if (selectedItem == "Bottom")
            //{
            //    Config.espcfx = true;
            //}
            //else if (selectedItem == "Up")
            //{
            //    Config.espcfx = false;
            //}
        }

        private async void hookscheckbox_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox2_Click(object sender, EventArgs e)
        {
            Config.FixEsp = guna2CustomCheckBox2.Checked;
            Core.Entities = new();
            InternalMemory.Cache = new();
        }

        private void guna2CustomCheckBox7_Click(object sender, EventArgs e)
        {
            Config.Notif();
            Core.Entities = new();
        }

        private void guna2CustomCheckBox6_Click(object sender, EventArgs e)
        {
            WindowState ^= FormWindowState.Minimized;
        }

        private void guna2CustomCheckBox3_Click(object sender, EventArgs e)
        {
            Config.Notif();
            if (guna2CustomCheckBox3.Checked)

                SetStreamMode(guna2CustomCheckBox3.Checked);
            Config.StreamMode = guna2CustomCheckBox3.Checked;
            Config.sound = guna2CustomCheckBox3.Checked;
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
        [DllImport("user32.dll")]
        static extern uint SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

        const uint WDA_NONE = 0x00000000;
        const uint WDA_MONITOR = 0x00000001;
        const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

        public void ExecuteCommand(string command)
        {
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe")
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = psi })
            {
                process.Start();
                process.StandardInput.WriteLine(command);
                process.StandardInput.Flush();
                process.StandardInput.Close();
                process.WaitForExit();
            }
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



        private async void guna2CustomCheckBox34_Click(object sender, EventArgs e)
        {
            var processes = Process.GetProcessesByName("HD-Player");
            if (processes.Length != 1)
            {
                sts.Text = "Emulator Not Found";
                return;
            }

            var process = processes[0];
            var mainModulePath = Path.GetDirectoryName(process.MainModule.FileName);
            var adbPath = Path.Combine(mainModulePath, "HD-Adb.exe");

            if (!File.Exists(adbPath))
            {
                sts.Text = "ReInstall Your Emulator Adb";
                return;
            }


            var adb = new Adb(adbPath);
            await adb.Kill();

            var started = await adb.Start();
            if (!started)
            {
                sts.Text = "Enable Emulator Adb";
                Application.Exit();
                return;
            }

            var moduleAddr = await adb.FindModule("com.dts.freefireth", "libil2cpp.so");

            Offsets.Il2Cpp = moduleAddr;
            Core.Handle = FindRenderWindow(mainHandle);

            var esp = new ESP();
            await esp.Start();

            new Thread(Data.Work) { IsBackground = true }.Start();
            new Thread(Aimbot.Work) { IsBackground = true }.Start();


            sts.Text = "Status : OK! - " + moduleAddr.ToString("X");
            Console.Beep();
        }

        private void guna2CustomCheckBox36_Click(object sender, EventArgs e)
        {
            Config.Notif(); // Substitua pela URL correta
            Config.AimBot = guna2CustomCheckBox36.Checked;
        }

        private void guna2CustomCheckBox35_Click(object sender, EventArgs e)
        {
            Config.Notif(); // Substitua pela URL correta
            Config.IgnoreKnocked = guna2CustomCheckBox35.Checked;
        }

        private void guna2CustomCheckBox33_Click(object sender, EventArgs e)
        {
            Config.Notif(); // Substitua pela URL correta
            Config.NoRecoil = guna2CustomCheckBox33.Checked;
        }

        private void guna2CustomCheckBox30_Click(object sender, EventArgs e)
        {
            Config.Notif(); // Substitua pela URL correta
            Config.Aimfovc = guna2CustomCheckBox30.Checked;
        }

        private void guna2CustomCheckBox21_Click(object sender, EventArgs e)
        {
            Config.Notif(); // Substitua pela URL correta
            Config.ESPLine = guna2CustomCheckBox21.Checked;
        }

        private void guna2CustomCheckBox23_Click(object sender, EventArgs e)
        {
            Config.Notif(); // Substitua pela URL correta
            Config.ESPBox = guna2CustomCheckBox23.Checked;
        }

        private void guna2CustomCheckBox22_Click(object sender, EventArgs e)
        {
            Config.Notif(); // Substitua pela URL correta
            Config.ESPFillBox = guna2CustomCheckBox22.Checked;
        }

        private void guna2CustomCheckBox20_Click(object sender, EventArgs e)
        {
            Config.Notif();
            Config.ESPBox2 = guna2CustomCheckBox20.Checked;
        }

        private void guna2CustomCheckBox4_Click(object sender, EventArgs e)
        {
            Config.Notif();
            Config.ESPSkeleton = guna2CustomCheckBox4.Checked;
        }

        private void guna2CustomCheckBox8_Click(object sender, EventArgs e)
        {
            Config.Notif(); // Substitua pela URL correta
            Config.ESPHealth = guna2CustomCheckBox8.Checked;
        }

        private void guna2CustomCheckBox5_Click(object sender, EventArgs e)
        {
            Config.Notif();
            Config.ESPName = guna2CustomCheckBox5.Checked;
        }

        private void guna2CustomCheckBox24_Click(object sender, EventArgs e)
        {
            Config.Notif();
            Config.ESPDistance = guna2CustomCheckBox24.Checked;
        }

        private void guna2CustomCheckBox26_Click(object sender, EventArgs e)
        {
            Config.Notif();
            Config.minimap = guna2CustomCheckBox26.Checked;
        }

        private void guna2CustomCheckBox25_Click(object sender, EventArgs e)
        {
            Config.Notif();
            Config.CrossHair = guna2CustomCheckBox25.Checked;
        }

        private void guna2CircleButton1_Click(object sender, EventArgs e)
        {
            Config.Notif(); // Substitua pela URL correta
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                guna2CircleButton1.FillColor = picker.Color;
                Config.ESPLineColor = picker.Color;
            }
        }

        private void guna2CircleButton2_Click(object sender, EventArgs e)
        {
            Config.Notif(); // Substitua pela URL correta
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                guna2CircleButton2.FillColor = picker.Color;
                Config.ESPBoxColor = picker.Color;
            }
        }

        private void guna2CircleButton3_Click(object sender, EventArgs e)
        {
            Config.Notif(); // Substitua pela URL correta
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                guna2CircleButton3.FillColor = picker.Color;
                Config.ESPFillBoxColor = picker.Color;
            }
        }

        private void guna2CircleButton4_Click(object sender, EventArgs e)
        {
            Config.Notif(); // Substitua pela URL correta
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                guna2CircleButton4.FillColor = picker.Color;
                Config.ESPBoxColor = picker.Color;
            }
        }

        private void guna2CircleButton5_Click(object sender, EventArgs e)
        {
            Config.Notif(); // Substitua pela URL correta
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                guna2CircleButton5.FillColor = picker.Color;
                Config.ESPSkeletonColor = picker.Color;
            }
        }

        private IEnumerable<long> speedResult;
        private void guna2CustomCheckBox17_Click(object sender, EventArgs e)
        {
            Config.Notif(); // Substitua pela URL correta
            Config.speedint = guna2CustomCheckBox17.Checked;
        }

        private async void guna2CustomCheckBox19_Click(object sender, EventArgs e)
        {

        }

        private IEnumerable<long> wallResult;
        private async void guna2CustomCheckBox18_Click(object sender, EventArgs e)
        {
            string[] pocessname = { "HD-Player" };
            bool success = memory.SetProcess(pocessname);

            if (!success)
            {
                return;
            }

            wallResult = await memory.AoBScan("AE 47 81 3F AE 47 81 3F 00 1A B7 EE DC 3A 9F ED");
            sts.Text = " Wall Hack Loaded";
            sts.ForeColor = Color.Lime;
            Console.Beep(400, 200);
        }

        private async void guna2CustomCheckBox16_Click(object sender, EventArgs e)
        {
            if (guna2CustomCheckBox16.Checked)
            {
                foreach (long id in wallResult)
                {
                    memory.AobReplace(id, "DB 1A 81 C2 AE 17 81 3F 00 1A B7 EE DC 3A 9F ED");
                }
                sts.Text = " Wall Hack Enable";
                sts.ForeColor = Color.Lime;
                Console.Beep(400, 200);
            }
            else
            {
                foreach (long id in wallResult)
                {
                    memory.AobReplace(id, "AE 47 81 3F AE 47 81 3F 00 1A B7 EE DC 3A 9F ED");
                }
                sts.Text = " Wall Hack Disable";
                sts.ForeColor = Color.Red;
                Console.Beep(400, 200);
            }
        }

        private void guna2CustomCheckBox11_Click(object sender, EventArgs e)
        {
            Config.Notif();
            Config.sound = guna2CustomCheckBox11.Checked;
        }

        private void guna2CustomCheckBox13_Click(object sender, EventArgs e)
        {
            if (guna2CustomCheckBox13.Checked)
            {
                this.TopMost = true;
            }
            else
            {
                this.TopMost = false;
            }
        }

        private void guna2CustomCheckBox12_Click(object sender, EventArgs e)
        {
            Config.Notif();
            try
            {
                var processes = Process.GetProcessesByName("HD-Player");

                foreach (var process in processes)
                {
                    process.Kill();
                    process.WaitForExit();
                }


            }
            catch (Exception ex)
            {

            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            suarpanel.BringToFront();
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            landpanel.BringToFront();
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            string key = txtLicense.Text.Trim();

            if (string.IsNullOrEmpty(key))
            {
                MessageBox.Show("Please enter your license key!");
                return;
            }

            try
            {

                FearAuthApp.init();


                FearAuthApp.license(key);

                if (FearAuthApp.response.success)
                {



                    chutpanel.Hide();

                    
                }
                else
                {
                    MessageBox.Show("Invalid License Key!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            chutpanel.BringToFront();
        }

        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {
            Console.Beep(500, 700);
        }

        private void guna2PictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void guna2ControlBox1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
            Application.Exit();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void AimPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label49_Click(object sender, EventArgs e)
        {

        }

        private void guna2CustomCheckBox14_Click(object sender, EventArgs e)
        {

        }

        private void guna2CirclePictureBox2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void guna2CirclePictureBox1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
            Application.Exit();
        }
    }
}
   