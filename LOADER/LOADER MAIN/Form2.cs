using Guna.UI2.WinForms;
using LITEXPARTICLES;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using static Guna.UI2.Native.WinApi;
using static Guna.UI2.WinForms.Suite.Descriptions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Timer = System.Windows.Forms.Timer;

namespace LOADER_MAIN
{
    public partial class Form2 : Form
    {



        private ParticlesBy_By _particlesBy_By;

        private Color startColor = Color.Red;
        private Color endColor = Color.FromArgb(10, 10, 10);
        private int steps = 1000;
        private int currentStep = 900;
        private bool goingToEndColor = true; // Indica si el color está cambiando hacia el color final

        [DllImport("kernel32.dll")]
        private static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public Form2()
        {
            InitializeComponent();
            _particlesBy_By = new ParticlesBy_By();

            Timer timer = new Timer
            {
                Interval = 3 // Roughly 60 FPS
            };
            timer.Tick += (sender, args) =>
            {
                _particlesBy_By.UpdateParticles();
                Invalidate(); // Causes the form to be redrawn
            };
            timer.Start();

            


        }

        private string[] possiblePaths = new string[]
   {
        @"C:\Program Files\BlueStacks_msi5\HD-Player.exe", // MSI version
        @"C:\Program Files\BlueStacks_nxt\HD-Player.exe", // BlueStacks 4
        @"C:\Program Files\BlueStacks\HD-Player.exe",     // BlueStacks 5 default
        @"C:\Program Files (x86)\BlueStacks\HD-Player.exe" // 32-bit version
   };
        public void StartAllEmulators()
        {
            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    StartEmulator(path);
                }
                else
                {
                    Console.WriteLine($"{path} does not exist.");
                }
            }
        }
        private void StartEmulator(string emulatorPath)
        {
            try
            {
                Process emulatorProcess = Process.Start(emulatorPath, "--instance Nougat32");
                if (emulatorProcess != null)
                {
                    Console.WriteLine($"Starting emulator: {emulatorPath}");

                    // Wait for the emulator to be ready
                    bool isReady = WaitForEmulatorReady(emulatorProcess);
                    if (isReady)
                    {
                        Console.WriteLine($"{emulatorPath} is ready.");
                        OpenFreeFire(emulatorPath);
                    }
                    else
                    {
                        Console.WriteLine($"{emulatorPath} did not respond in time.");
                    }
                }
                else
                {
                    Console.WriteLine($"Failed to start emulator: {emulatorPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting emulator at {emulatorPath}: {ex.Message}");
            }
        }
        private bool WaitForEmulatorReady(Process emulator)
        {
            bool isReady = false;
            int maxWaitTime = 30000; // Max wait time in milliseconds (30 seconds)
            int waitInterval = 1000; // Check every second
            int waitedTime = 0;

            while (waitedTime < maxWaitTime)
            {
                // Check if the emulator is running and responsive
                if (emulator.Responding)
                {
                    isReady = true;
                    break;
                }

                Thread.Sleep(waitInterval);
                waitedTime += waitInterval;
            }

            return isReady;
        }

        // Method to open Free Fire using HD-Adb
        private void OpenFreeFire(string emulatorPath)
        {
            // Determine the path for HD-Adb.exe based on the emulator's directory
            string adbPath = Path.Combine(Path.GetDirectoryName(emulatorPath), "HD-Adb.exe");

            // Check if Free Fire is already running
            if (IsFreeFireRunning(adbPath))
            {
                Console.WriteLine("Free Fire is already running.");
                return;
            }

            int retryCount = 3; // Number of retries
            for (int i = 0; i < retryCount; i++)
            {
                Process adbProcess = new Process();
                adbProcess.StartInfo.FileName = adbPath;
                adbProcess.StartInfo.Arguments = "shell monkey -p com.dts.freefireth -c android.intent.category.LAUNCHER 1";
                adbProcess.StartInfo.UseShellExecute = false;
                adbProcess.StartInfo.RedirectStandardOutput = true;
                adbProcess.StartInfo.RedirectStandardError = true;

                try
                {
                    adbProcess.Start();
                    adbProcess.WaitForExit(); // Wait for the ADB command to finish

                    // Check the output for any errors
                    string output = adbProcess.StandardOutput.ReadToEnd();
                    string errorOutput = adbProcess.StandardError.ReadToEnd();

                    Console.WriteLine("ADB Output: " + output);
                    Console.WriteLine("ADB Error: " + errorOutput);

                    // Wait for a bit to give Free Fire time to open
                    Thread.Sleep(5000); // Adjust this wait time if needed

                    // Check if Free Fire is running after attempting to launch
                    if (IsFreeFireRunning(adbPath))
                    {
                        Console.WriteLine("Free Fire successfully opened.");
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Attempt to open Free Fire failed, retrying...");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error opening Free Fire: {ex.Message}");
                }

                Thread.Sleep(2000); // Wait before retrying
            }

            Console.WriteLine("Failed to open Free Fire after multiple attempts.");
        }

        private bool IsFreeFireRunning(string adbPath)
        {
            Process adbProcess = new Process();
            adbProcess.StartInfo.FileName = adbPath;
            adbProcess.StartInfo.Arguments = "shell pidof com.dts.freefireth";
            adbProcess.StartInfo.UseShellExecute = false;
            adbProcess.StartInfo.RedirectStandardOutput = true;
            adbProcess.StartInfo.RedirectStandardError = true;

            try
            {
                adbProcess.Start();
                string output = adbProcess.StandardOutput.ReadToEnd();
                adbProcess.WaitForExit();

                // If output contains a PID, Free Fire is running
                return !string.IsNullOrWhiteSpace(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking Free Fire status: {ex.Message}");
                return false;
            }
        }
        private void Form2_Load(object sender, EventArgs e)
        {
          
        }
        private static void ExtractEmbeddedResource(string resourceName, string outputPath)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();

            // Get the embedded resource stream
            using (Stream resourceStream = executingAssembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                {
                    throw new ArgumentException($"Resource '{resourceName}' not found.");
                }

                // Read the embedded resource and save it to the specified path
                using (FileStream fileStream = new FileStream(outputPath, FileMode.Create))
                {
                    byte[] buffer = new byte[resourceStream.Length];
                    resourceStream.Read(buffer, 0, buffer.Length);
                    fileStream.Write(buffer, 0, buffer.Length);
                }
            }
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        const uint PROCESS_CREATE_THREAD = 0x2;
        const uint PROCESS_QUERY_INFORMATION = 0x400;
        const uint PROCESS_VM_OPERATION = 0x8;
        const uint PROCESS_VM_WRITE = 0x20;
        const uint PROCESS_VM_READ = 0x10;

        const uint MEM_COMMIT = 0x1000;
        const uint PAGE_READWRITE = 4;
        void injectDll(string name, string tmpname)
        {
            string processName = "HD-Player"; // Specify your target process name
            string dllResourceName = name; // Correct resource name

            // Extract the embedded msdrmi.dll to a temporary file
            string tempDllPath = Path.Combine(Path.GetTempPath(), tmpname);
            ExtractEmbeddedResource(dllResourceName, tempDllPath);

            Console.WriteLine($"DLL extracted successfully to: {tempDllPath}");


            Process[] targetProcesses = Process.GetProcessesByName(processName);
            if (targetProcesses.Length == 0)
            {
                Console.WriteLine($"Waiting for {processName}.exe...");
            }
            else
            {
                Process targetProcess = targetProcesses[0];
                IntPtr hProcess = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, targetProcess.Id);

                IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
                IntPtr allocMemAddress = VirtualAllocEx(hProcess, IntPtr.Zero, (IntPtr)tempDllPath.Length, MEM_COMMIT, PAGE_READWRITE);

                IntPtr bytesWritten;
                WriteProcessMemory(hProcess, allocMemAddress, System.Text.Encoding.ASCII.GetBytes(tempDllPath), (uint)tempDllPath.Length, out bytesWritten);

                CreateRemoteThread(hProcess, IntPtr.Zero, IntPtr.Zero, loadLibraryAddr, allocMemAddress, 0, IntPtr.Zero);
            }
        }
        private async void guna2Button1_Click_5(object sender, EventArgs e)
        {
            string dllName = "Client.dll";
            string tempPath = Path.Combine("C:\\Windows\\Temp", dllName);
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
            ExtractEmbeddedResource("LOADER_MAIN.Client.dll", tempPath);
            injectDll("LOADER_MAIN.AotBst.dll", "AotBst.dll");
            injectDll("LOADER_MAIN.cimgui.dll", "cimgui.dll");
            Environment.Exit(10);
        }

        private async void guna2Button2_Click(object sender, EventArgs e)
        {
            
        }

        // ✅ Universal BlueStacks path finder (32-bit + 64-bit)
        private string FindAnyBlueStacks()
        {
            string[] possiblePaths =
            {
                @"C:\Program Files\BlueStacks_nxt\HD-Player.exe",
                @"C:\ProgramData\BlueStacks_nxt\HD-Player.exe",
                @"C:\Program Files (x86)\BlueStacks_nxt\HD-Player.exe",
                @"C:\ProgramData\BlueStacks\HD-Player.exe",
                @"C:\Program Files\BlueStacks\HD-Player.exe",
                @"C:\Program Files (x86)\BlueStacks\HD-Player.exe"
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path)) return path;
            }
            return null;
        }

        // ✅ Free Fire shortcut finder
        private string FindShortcutForApp(string keyword)
        {
            var places = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory),
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)
            };

            foreach (var place in places)
            {
                if (string.IsNullOrEmpty(place) || !Directory.Exists(place)) continue;
                var files = Directory.GetFiles(place, "*.lnk", SearchOption.AllDirectories);
                foreach (var f in files)
                {
                    if (Path.GetFileName(f).ToLower().Contains(keyword.ToLower()))
                        return f;
                }
            }
            return null;
        }

        // ✅ Status label update helper
        private void SetStatus(string text, System.Drawing.Color color)
        {
            
        }

        private void guna2ControlBox1_Click(object sender, EventArgs e)
        {
            
        }

        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
    
}
