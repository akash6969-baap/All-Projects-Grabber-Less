using Client;
using DiscordRPC;
using Microsoft.VisualBasic.ApplicationServices;
using Reborn;
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
      name: "AIMBOT AI ",
     ownerid: "XtpdNePfy9",
     secret: "2EaW22OM3dLtib9D611KCFiUoi-MmRNgXrvcfT2UmnPcDhZ8vNFja6YA_NbhfqaE",
     version: "1.0.0");

        IntPtr mainHandle;
        public FormAh(IntPtr handle)
        {

            InitializeComponent();
            mainHandle = handle;
            KeyAuthApp.init();
            KeyAuthApp.check();

        }

        private async void loginBtn_Click(object sender, EventArgs e)
        {
            KeyAuthApp.login(guna2TextBox1.Text, guna2TextBox2.Text);
            if (KeyAuthApp.response.success)
            {

                var esp = new ESP();
                await esp.Start();              
                new Thread(Data.Work) { IsBackground = true }.Start();
                new Thread(AimbotV2.Work) { IsBackground = true }.Start();             
                new Thread(EnemyPull360.Start) { IsBackground = true }.Start();
                new Thread(AimbotAi.Work) { IsBackground = true }.Start();                          
                await Task.Delay(5);            
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
            Environment.Exit(0);
        }

        private void guna2CircleButton2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
