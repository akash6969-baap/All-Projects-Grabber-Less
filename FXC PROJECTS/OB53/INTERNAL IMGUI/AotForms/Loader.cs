using FearAuth;
using Kanishk_MaX;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AotForms
{
    public static class Loader
    {
        public static api FearAuthApp = new api(
            name: "INTERNAL SAFE",
            ownerid: "e54a0e1d-62b3-45a5-bad4-ef93b4fc005e",
            secret: "54d9d438ab3b8a100874e1d3233db3c5adfbae952b044c1a",
            version: "1.0"
        );

        public static async Task Initialize(nint mainWindowHandle)
        {
            FearAuthApp.init();
            FearAuthApp.check();

            var processes = Process.GetProcessesByName("HD-Player");

            if (processes.Length != 1)
            {
                MessageBox.Show("Open emulator.");
                Environment.Exit(0);
                return;
            }

            var process = processes[0];
            string mainModulePath = Path.GetDirectoryName(process.MainModule?.FileName);

            if (string.IsNullOrEmpty(mainModulePath))
            {
                MessageBox.Show("Reinstall emulator.");
                Environment.Exit(0);
                return;
            }

            var adbPath = Path.Combine(mainModulePath, "HD-Adb.exe");
            if (!File.Exists(adbPath))
            {
                MessageBox.Show("Adb not Found. Reinstall emulator.");
                Environment.Exit(0);
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
            Core.Handle = FindRenderWindow(mainWindowHandle);

            var esp = new ESP();
            _ = esp.Start();
        }

        public static void StartCheats()
        {
            new Thread(Data.Work) { IsBackground = true }.Start();
            new Thread(AimbotAi.Work) { IsBackground = true }.Start();
            new Thread(FearSpinBot.Work) { IsBackground = true }.Start();
            new Thread(FearColliderEngine.Start) { IsBackground = true }.Start();
            new Thread(GhostLock.Work) { IsBackground = true }.Start();
        }

        public static void KillProcess(string processName)
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
    }
}
