using AotForms;
using Client;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AotForms
{
    internal static class Program
    {
        [UnmanagedCallersOnly(EntryPoint = "Load")]
        public static void Load(nint pVM)
        {
            if (pVM != 0)
            {
                InternalMemory.Initialize(pVM);
                var process = Process.GetCurrentProcess();
                _ = Form1.InitializeAsync(process.MainWindowHandle);
            }
        }
    }
}