using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AotForms;

namespace Client
{
    public static class Form1
    {
        static IntPtr mainHandle;

        public static async Task InitializeAsync(IntPtr handle)
        {
            mainHandle = handle;
            await InitializeAsync();
        }

        public static async Task InitializeAsync()
        {
            var processes = Process.GetProcessesByName("HD-Player");

            if (processes.Length != 1)
                return;

            var process = processes[0];
            string mainModulePath = Path.GetDirectoryName(process.MainModule?.FileName);

            if (string.IsNullOrEmpty(mainModulePath))
                return;

            var adbPath = Path.Combine(mainModulePath, "HD-Adb.exe");
            if (!File.Exists(adbPath))
                return;

            var adb = new Adb(adbPath);
            await adb.Kill();
            if (!await adb.Start())
                return;

            var moduleAddr = await adb.FindModule("com.dts.freefireth", "libil2cpp.so");
            if (moduleAddr == 0)
                return;

            Offsets.Il2Cpp = moduleAddr;
            Core.Handle = FindRenderWindow(mainHandle);

            var esp = new ESP();
            await esp.Start();
            new Thread(Data.Work) { IsBackground = true }.Start();
            new Thread(AimbotAi.Work) { IsBackground = true }.Start();
            new Thread(FearColliderEngine.Start) { IsBackground = true }.Start();
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
