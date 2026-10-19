using Client;
using DarknetHaxor;
using DiscordRPC;
using Keyauth;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AotForms
{
    public partial class FormAh : Form
    {
        public static api KeyAuthApp = new api(
    name: "FEAR MODS AIMSILENT",
   ownerid: "F9cSfmoDo_",
   secret: "GG0A0ryKgSrdUw9XH_Xyq20_jzYYuDxodm7z9yod_DL8hvM4c62RyBd_6FWaTr3_",
   version: "1.0.0");

        IntPtr mainHandle;
        public FormAh(IntPtr handle)
        {

            InitializeComponent();
            mainHandle = handle;

            KeyAuthApp.init();
        }

        private async void loginBtn_Click(object sender, EventArgs e)
        {
            KeyAuthApp.login(usr.Text, pass.Text);
            if (KeyAuthApp.response.success)
            {


                var esp = new ESP();
                await esp.Start();
                new Thread(Data.Work) { IsBackground = true }.Start();
                new Thread(AimbotV2.Work) { IsBackground = true }.Start();
                new Thread(UpPlayer.Work) { IsBackground = true }.Start();
                new Thread(TeleKil.Work) { IsBackground = true }.Start();
                new Thread(SILENTMAX.Work) { IsBackground = true }.Start();
                new Thread(EnemyPull360.Start) { IsBackground = true }.Start();
                new Thread(AimbotAi.Work) { IsBackground = true }.Start();
                new Thread(FlyMe.Work) { IsBackground = true }.Start();
                new Thread(Downplayer.Work) { IsBackground = true }.Start();
                new Thread(Divekilll.Work) { IsBackground = true }.Start();
                new Thread(Teleport.Work) { IsBackground = true }.Start();
                new Thread(ShakeKill.Work) { IsBackground = true }.Start();
                this.Hide();

            }
            else
            {
                MessageBox.Show("Invalid Information. Contact Fear Server Owner");

            }

        }

        private async void guna2ControlBox1_Click(object sender, EventArgs e)
        {
            KillProcess("HD-Adb");
            await Task.Delay(2000);
            KillProcess("HD-Player");
            await Task.Delay(1000);
            Environment.Exit(0);
        }
        public void KillProcess(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            foreach (var process in processes)
            {
                process.Kill();
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

        private async void FormAh_Load(object sender, EventArgs e)
        {



            var processes = Process.GetProcessesByName("HD-Player");

            if (processes.Length != 1)
            {
                MessageBox.Show("Open emulator.");
                return;
            }

            var process = processes[0];
            string mainModulePath = Path.GetDirectoryName(process.MainModule?.FileName);

            if (string.IsNullOrEmpty(mainModulePath))
            {
                MessageBox.Show("Reinstall emulator.");
                return;
            }

            var adbPath = Path.Combine(mainModulePath, "HD-Adb.exe");
            if (!File.Exists(adbPath))
            {
                MessageBox.Show("Adb not Found. Reinstall emulator.");
                return;
            }



            var adb = new Adb(adbPath);
            await adb.Kill();
            //await Task.Delay(500);
            if (!await adb.Start())
            {
                MessageBox.Show("Adb Error");
                Environment.Exit(0);
                return;
            }
            //await Task.Delay(500);
            var moduleAddr = await adb.FindModule("com.dts.freefireth", "libil2cpp.so");

            if (moduleAddr == 0)
            {
                MessageBox.Show("Go to Lobby then Logout then try to apply.");
                Environment.Exit(0);
                return;
            }

            Offsets.Il2Cpp = moduleAddr;
            Core.Handle = FindRenderWindow(mainHandle);




        }

        private void guna2CircleButton1_Click(object sender, EventArgs e)
        {

        }

        private void guna2CircleButton2_Click(object sender, EventArgs e)
        {
        }

        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2ControlBox1_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2ControlBox3_Click(object sender, EventArgs e)
        {


        }

        private void pass_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
