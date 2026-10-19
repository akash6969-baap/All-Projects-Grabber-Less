using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.ApplicationServices;
using Reborn;


namespace AotForms
{
    public partial class FormAh : Form
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        public static api KeyAuthApp = new api(
   name: "Kripamayd's Application",
   ownerid: "ScPf9TjHsz",
   secret: "6e71408cc6b90de63157713a84666826dbd27ac5004777940f9005abf92f936a",
   version: "1.0"
);

        IntPtr mainHandle;
        public FormAh(IntPtr handle)
        {
            this.Hide();
            InitializeComponent();
            mainHandle = handle;
            KeyAuthApp.init();
            //AllocConsole();
            //Console.OutputEncoding = Encoding.UTF8;
        }

        private async void loginBtn_Click(object sender, EventArgs e)
        {
            KeyAuthApp.login(user.Text, pass.Text);
            if (KeyAuthApp.response.success)
            {

            var esp = new ESP();
            await esp.Start();
            new Thread(Data.Work) { IsBackground = true }.Start();
            new Thread(AimbotAi.Work) { IsBackground = true }.Start();
            new Thread(AimbotV2.Work) { IsBackground = true }.Start();
            new Thread(UpPlayer.Work) { IsBackground = true }.Start();
            new Thread(TeleKillChain.Work) { IsBackground = true }.Start();
            new Thread(FLYHACKX40.Work) { IsBackground = true }.Start();
            new Thread(AimbotRageMain.Work) { IsBackground = true }.Start();
            await Task.Delay(1000);

            this.Hide();
            }

            else
            {

                MessageBox.Show(KeyAuthApp.response.message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            this.Size = new System.Drawing.Size(0, 0);

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
            if (!await adb.Start())
            {
                MessageBox.Show("Adb Error");
                Environment.Exit(0);
                return;
            }
            var moduleAddr = await adb.FindModule("com.dts.freefireth", "libil2cpp.so");

            if (moduleAddr == 0)
            {
                MessageBox.Show("Go to Lobby then Logout then try to apply.");
                Environment.Exit(0);
                return;
            }

            Offsets.Il2Cpp = moduleAddr;
            Core.Handle = FindRenderWindow(mainHandle);


            sta.Text = "Login now";

            // Start your various worker threads
            var esp = new ESP();
            await esp.Start();
            new Thread(Data.Work) { IsBackground = true }.Start();
            new Thread(AimbotAi.Work) { IsBackground = true }.Start();
            new Thread(AimbotV2.Work) { IsBackground = true }.Start();
            new Thread(KCBRUTALSILENT.Work) { IsBackground = true }.Start();
            new Thread(UpPlayer.Work) { IsBackground = true }.Start();
            new Thread(TeleKillChain.Work) { IsBackground = true }.Start();
            new Thread(FLYHACKX40.Work) { IsBackground = true }.Start();
            new Thread(EnemyPull360.Start) { IsBackground = true }.Start();
            new Thread(ShakeKill.Work) { IsBackground = true }.Start();
            new Thread(telepapa.Work) { IsBackground = true }.Start();
            new Thread(TeleportV22.Work) { IsBackground = true }.Start();
            new Thread(ClimbUp.Start) { IsBackground = true }.Start();
            new Thread(divekill.Start) { IsBackground = true }.Start();
            new Thread(AimbotRageMain.Work) { IsBackground = true }.Start();
            RapidSpin.Activate();
            new Thread(() => PipeServer.Start()) { IsBackground = true }.Start();
            await Task.Delay(1000);
        }

        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
