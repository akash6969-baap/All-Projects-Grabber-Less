
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AotForms
{
    public partial class ServerADB : Form
    {

        IntPtr mainHandle;
        public ServerADB(IntPtr handle)
        {
            mainHandle = handle;
            InitializeComponent();
        }

        private async void ServerADB_Load(object sender, EventArgs e)
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

            var esp = new ESP();
            await esp.Start();
            new Thread(Data.Work) { IsBackground = true }.Start();
            new Thread(AimbotAi.Work) { IsBackground = true }.Start();
            new Thread(AimbotV2.Work) { IsBackground = true }.Start();
            new Thread(UpPlayer.Work) { IsBackground = true }.Start();
            new Thread(TeleKillChain.Work) { IsBackground = true }.Start();
            new Thread(FLYHACKX40.Work) { IsBackground = true }.Start();
            new Thread(KCBRUTALSILENT.Work) { IsBackground = true }.Start();
            new Thread(EnemyPull360.Start) { IsBackground = true }.Start();
            new Thread(ShakeKill.Work) { IsBackground = true }.Start();
            new Thread(telepapa.Work) { IsBackground = true }.Start();
            new Thread(TeleportV22.Work) { IsBackground = true }.Start();
            new Thread(ClimbUp.Start) { IsBackground = true }.Start();
            new Thread(divekill.Start) { IsBackground = true }.Start();
            new Thread(AimbotRageMain.Work) { IsBackground = true }.Start();
            new Thread(() => PipeServer.Start()) { IsBackground = true }.Start();
            await Task.Delay(1000);

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
    }
}
