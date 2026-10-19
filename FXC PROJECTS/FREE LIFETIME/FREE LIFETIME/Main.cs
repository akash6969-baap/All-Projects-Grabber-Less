using Celestial;
using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;

namespace FREE_LIFETIME
{
    public partial class Main : Form
    {
        Celestial.Cosmic cosmic = new Celestial.Cosmic();
        public static readonly Cosmic Memory = new Cosmic();
        private SniperScopeForCosmic sniperScope;

        int sniperKey = 0x01; // default left click
        bool isBinding = false;

        // ===== SNIPER DUMMY SYSTEM =====
        bool sniperScanned = false;
        bool sniperActive = false;
        List<long> sniperAddresses = new List<long>();



        // DllImports for Windows API
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("kernel32.dll")]
        private static extern int Beep(int dwFreq, int dwDuration);

        [DllImport("kernel32.dll")]
        private static extern bool SetThreadPriority(IntPtr hThread, int nPriority);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentThread();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

        private const int VK_LBUTTON = 0x01;
        private const int THREAD_PRIORITY_ABOVE_NORMAL = 1;

        private Cosmic _cosmic;
        private byte[] sniperOriginalBuffer;
        private byte[] sniperModifiedBuffer;
        private bool bSniperScanned = false;
        private bool bSniperScopeActive = false;
        private bool isPatching = false;
        private int timingSeed;
        private object patchLock = new object();

        [DllImport("kernel32.dll")]
        private static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", SetLastError = true)]

        public static extern IntPtr SendMessageW(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);


        //  Declare SoundPlayers at top of your form/class
        SoundPlayer soundActivated = new SoundPlayer("AIMBOT ACTIVATED.wav");
        SoundPlayer soundDeactivated = new SoundPlayer("AIMBOT DEACTIVATED.wav");
        SoundPlayer soundScan = new SoundPlayer("AIMBOT SCAN.wav");
        //  Declare at top of your class/form
        SoundPlayer soundPatch = new SoundPlayer("SCAN RESULT PATCH.wav");
        SoundPlayer soundRestore = new SoundPlayer("SCAN RESULT RESTORED.wav");


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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (isBinding)
            {
                sniperKey = e.KeyValue;
                isBinding = false;

                Status.Text = $"Key set: {e.KeyCode}";
                guna2Button4.Text = $"Key: {e.KeyCode}"; //  YE MAIN CHEEZ
            }

            base.OnKeyDown(e);
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



        bool scopeActive = false;
        bool switchActive = false;
        bool delayFixActive = false;
        bool noScopeActive = false;
        bool fastReloadActive = false;
        bool noRecoilActive = false;
        bool unlimitedAmmoActive = false;
        bool glitchFireActive = false;
        bool fastLandingActive = false;
        bool blackSkyActive = false;
        bool visionActive = false;
        bool camLeftActive = false;
        bool wallActive = false;
        bool speedActive = false;

        public Main(string user)
        {
            InitializeComponent();
        }



        public Main(string user, long expiryTimestamp) : this(user)
        {
            ExpiryTimestamp = expiryTimestamp;
        }




        public long ExpiryTimestamp { get; }

        private void Main_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true; //  YE IMPORTANT

            sniperScope = new SniperScopeForCosmic(cosmic, () => sniperKey);

            Task.Run(() =>
            {
                while (true)
                {
                    sniperScope.UpdateSniperScope();
                    Thread.Sleep(1);
                }
            });
        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private bool EnsureAttached()
        {
            if (memory._processHandle == IntPtr.Zero || memory.processId == 0)
            {
                Status.Text = (" Process not attached. Reattaching...");
                memory.SetProcess(new string[] { "HD-Player", "HD-Player64", "hd-player", "HD-Player.exe" });
            }
            return memory._processHandle != IntPtr.Zero && memory.processId != 0;
        }

        private bool EnsureLoaded()
        {
            if (foundBases == null || foundBases.Count == 0)
            {
                Status.Text = (" No scanned addresses. Click LOAD first!");
                return false;
            }
            return true;
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


        private void label1_Click(object sender, EventArgs e)
        {

        }
        private Celestial.Cosmic memory = new Celestial.Cosmic();
        private List<long> foundBases = new List<long>();
        private Dictionary<long, int> originalValues = new Dictionary<long, int>();

        // Neck Offsets
        // [OLD] private const long NECK_READ_OFFSET = 0xB8;
        // [OLD] private const long NECK_WRITE_OFFSET = 0xB4;
        private const long NECK_READ_OFFSET = 0xB5;  // Updated
        private const long NECK_WRITE_OFFSET = 0xB1; // Updated

        // ============================================================
        //            AOB PATTERNS & ADDRESS VARIABLES
        //         (Saare patterns ek jagah — easy maintenance)
        // ============================================================

        // --- NECK AIMBOT (Main Load) ---
        // [OLD] private const string AOB_PATTERN =
        // @"FF FF FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 FF FF FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 00 00 ?? ?? ?? ?? 00 00 00 00 ?? ?? ?? ?? 00 00 00 00 00 00 00 00 00 00 00 00 00 00 A5 43";
        private const string AOB_PATTERN =
        @"00 00 00 00 00 FF FF FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 00 00 ?? ?? ?? ?? 00 00 00 00 ?? ?? ?? ?? 00 00 00 00 00 00 00 00 00 00 00 00 00 00 A5 43";

        // --- SNIPER SWITCH ---
        string switchSearch  = "3F 00 00 80 3E 00 00 00 00 04 00 00 00 00 00 80 3F 00 00 20 41 00 00 34 42 01 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 80 3F";
        string switchReplace = "01 00 00 80 00 00 00 00 00 04 00 00 00 00 00 80 3F 00 00 20 41 00 00 34 42 01 00 00 00 01";

        // --- SNIPER DELAY FIX ---
        string delaySearch  = "";
        string delayReplace = "";

        // --- SNIPER SCOPE V2 ---
        string scopeV2Search  = "FF FF 08 00 00 00 00 00 60 40 CD CC 8C 3F 8F C2 F5 3C CD CC CC 3D 06 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00";
        string scopeV2Replace = "FF FF 08 00 00 00 00 00 60 40 CD CC 8C 3F 8F C2 F5 3C CD CC CC 3D 06 00 00 00 00 00 80 3E 00 00 00 00 00 00 00 00 00 00";

        // --- SNIPER NO SCOPE ---
        string noScopeSearch  = "";
        string noScopeReplace = "";

        // --- SPEED HACK ---
        string speedSearch  = "40 02 2B 07 3D 02 2B 07 3D 02 2B 07 3D 00 00 00";
        string speedReplace = "40 02 2B 9B 3C 02 2B 9B 3C 02 2B 07 3D 00 00 00";

        // --- WALL HACK ---
        string wallSearch  = "3F AE 47 81 3F AE 47 81 3F AE 47 81 3F 00 1A B7 EE DC 3A 9F ED 30 00 4F E2 43 2A B0 EE EF 0A 60 F4 43 6A F0 EE 1C 00 8A E2 43 5A F0 EE 8F 0A 48 F4 43 2A F0 EE 43 7A B0 EE 8F 0A 40 F4 41 AA B0 EE FE 05 A0 E3 41 1A F0 EE 2C 00 8A E5 10";
        string wallReplace = "3F AE 47 81 3F AE 47 81 BF AE 47 81 3F AE 1A B7 EE DC 3A 9F ED 30 00 4F E2 43 2A B0 EE EF 0A 60 F4 43 6A F0 EE 1C 00 8A E2 43 5A F0 EE 8F 0A 48 F4 43 2A F0 EE 43 7A B0 EE 8F 0A 40 F4 41 AA B0 EE FE 05 A0 E3 41 1A F0 EE 2C 00 8A E5 10";

        // --- VISION HACK + AIM FOV ---
        // [OLD] string visionSearch  = "DB 0F 49 40 10 2A 00 EE 00 10 80 E5 10 3A 01 EE 14 10 80 E5 00 2A 30 EE 00 10 00 E3 41 3A 30 EE 80 1F 4B E3 01 0A 30 EE 2C 10 80 E5 50 00 C0 F2 04 10";
        // [OLD] string visionReplace = "DB 0F E3 40 10 2A 00 EE 00 10 80 E5 10 3A 01 EE 14 10 80 E5 00 2A 30 EE 00 10 00 E3 41 3A 30 EE 80 1F 4B E3 01 0A 30 EE 2C 10 80 E5 50 00 C0 F2 04 10";
        string visionSearch  = "B4 43 DB 0F 49 40 10 2A 00 EE 00 10 80 E5 10 3A 01 EE 14 10 80 E5 00 2A 30 EE 00 10 00 E3 41 3A 30 EE";
        string visionReplace = "B4 43 DB 0F 78 40 10 2A 00 EE 00 10 80 E5 10 3A 01 EE 14 10 80 E5 00 2A 30 EE 00 10 00 E3 41 3A 30 EE";

        // --- CAMERA LEFT ---
        string camLeftSearch  = "00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00";
        string camLeftReplace = "00 00 00 00 00 00 96 40 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00";

        // --- BLACK SKY ---
        string blackSkySearch  = "F0 8F BD E8 A4 70 7D 3F 3A CD 13 3F 0A D7 23 3C BD 37 86 35";
        string blackSkyReplace = "F0 8F BD E8 A4 70 7D 3F 3A CD 13 3F 0A D7 23 3C BD 37 86 B5";

        // --- FAST RELOAD ---
        string fastReloadSearch  = "10 0A 18 EE 02 8B BD EC 30 88 BD E8 30 48 2D E9 08 B0 8D E2 00 40 A0 E1 DC";
        string fastReloadReplace = "10 0A 18 EE 02 8B BD EC 30 88 BD E8 FF 00 45 E3 1E FF 2F E1";

        // --- NO RECOIL ---
        string noRecoilSearch  = "00 0a 81 ee 10 0a 10 ee 10 8c bd e8 00 00 7a 44 f0 48 2d e9 10 b0 8d e2 02 8b 2d ed 08 d0 4d e2 00 50 a0 e1 10 1a 08 ee 08 40 95 e5 00 00 54 e3";
        string noRecoilReplace = "00 0a 81 ee 10 0a 10 ee 10 8c bd e8 00 00 ef 44 f0 48 2d e9 10 b0 8d e2 02 8b 2d ed 08 d0 4d e2 00 50 a0 e1 10 1a 08 ee 08 40 95 e5 00 00 54 e3";

        // --- UNLIMITED AMMO ---
        string unlimitedAmmoSearch  = "";
        string unlimitedAmmoReplace = "";

        // --- GLITCH FIRE ---
        string glitchFireSearch  = "90 C1 00 00 70 41 01 00 00 00 00 00 C0 3F 00 00 00 3F 00 00 80 3F";
        string glitchFireReplace = "90 C1 00 00 70 41 01 00 00 00 00 00 C0 00 00 00 00 3F 00 00 80 3F";

        // --- FAST LANDING ---
        string fastLandingSearch  = "00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 80 7F 00 00 80 7F 00 00 80 7F 00 00 80 FF";
        string fastLandingReplace = "00 00 00 00 00 00 FF 41 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 80 7F 00 00 80 7F 00 00 80 7F 00 00 80 FF ";

        // --- RESET GUEST ---
        string resetguestSearch  = "10 40 2D E9 D0 40 9F E5 04 40 8F E0 00 00 D4 E5 00 00 50 E3 04 00 00 1A C0 00 9F E5 00 00 9F E7 F1 65 9E EB 01 00 A0 E3 00 00 C4 E5 0D 05 03 E3";
        string resetguestReplace = "01 00 A0 E3 1E FF 2F E1 04 40 8F E0 00 00 D4 E5 00 00 50 E3 04 00 00 1A C0 00 9F E5 00 00 9F E7 F1 65 9E EB 01 00 A0 E3 00 00 C4 E5 0D 05 03 E3";

        // --- AWM SWITCH ---
        string awmSwitchSearch  = "00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00";
        string awmSwitchReplace = "00 00 00 00 00 00 80 40 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 80 BF";

        // --- DOUBLE RATE OF FIRE ---
        string rateOfFireSearch  = "02 2B 07 3D 02 2B 07 3D 02 2B 07 3D";
        string rateOfFireReplace = "0A D7 A3 3C 0A D7 A3 3C 0A D7 A3 3C";

        // --- FLY HACK OB55 ---
        string flySearch  = "9A 99 99 3E A4 70 3D 3F 01";
        string flyReplace = "9A 99 BF 3F A4 70 3D 3F 01";

        // ============================================================
        //              SCANNED ADDRESS VARIABLES
        //         (Saare address lists ek jagah)
        // ============================================================
        IEnumerable<long> speedAddresses        = Enumerable.Empty<long>();
        IEnumerable<long> wallAddresses         = Enumerable.Empty<long>();
        IEnumerable<long> visionAddresses       = Enumerable.Empty<long>();
        IEnumerable<long> camLeftAddresses      = Enumerable.Empty<long>();
        IEnumerable<long> blackSkyAddresses     = Enumerable.Empty<long>();
        IEnumerable<long> flyAddresses          = Enumerable.Empty<long>();
        IEnumerable<long> fastReloadAddresses   = Enumerable.Empty<long>();
        IEnumerable<long> noRecoilAddresses     = Enumerable.Empty<long>();
        IEnumerable<long> unlimitedAmmoAddresses= Enumerable.Empty<long>();
        IEnumerable<long> glitchFireAddresses   = Enumerable.Empty<long>();
        IEnumerable<long> fastLandingAddresses  = Enumerable.Empty<long>();
        IEnumerable<long> resetguestAddresses   = Enumerable.Empty<long>();
        // New feature address lists
        IEnumerable<long> awmSwitchAddresses    = Enumerable.Empty<long>();
        IEnumerable<long> rateOfFireAddresses   = Enumerable.Empty<long>();
        bool awmSwitchActive    = false;
        bool rateOfFireActive   = false;
        bool flyActive          = false;


        // === LOAD Button ===
        private async void guna2Button1_Click(object sender, EventArgs e)
        {

            try
            {
                Status.Text = (" Attaching to emulator...");
                bool attached = memory.SetProcess(new string[] { "HD-Player", "HD-Player64", "hd-player", "HD-Player.exe" });

                if (!attached || memory._processHandle == IntPtr.Zero || memory.processId == 0)
                {
                    Status.Text = (" Emulator not found / attach failed. Start emulator and try again.");
                    return;
                }

                Status.Text = ($" Attached to process (pid: {memory.processId}). Starting AoB scan...");

                var scanResults = await memory.AoBScan(AOB_PATTERN);

                if (scanResults == null || !scanResults.Any())
                {
                    Status.Text = (" Pattern not found. (0 results)");
                    return; //  FAIL  no sound
                }

                //  SUCCESS  play sound
                soundScan.Play();

                foundBases = scanResults.ToList();
                originalValues.Clear();

                Status.Text = ($" Scan completed. Found {foundBases.Count} base addresses.");
            }
            catch (Exception ex)
            {
                Status.Text = (" Error in Load: " + ex.Message);
                //  ERROR  no sound
            }
        }

        // ====== PATTERNS (form-level) — Ab saare patterns upar consolidated section mein hain ======




        // --- helper: write patch to all found addresses ---
        private async Task<bool> ApplyPatchAsync(IEnumerable<long> addresses, string hexBytes)
        {
            try
            {
                var data = HexStringToBytes(hexBytes);
                if (addresses == null) return false;

                // Run writes asynchronously to avoid UI freeze
                await Task.Run(() =>
                {
                    foreach (var addr in addresses)
                    {
                        try
                        {
                            // --- ADJUST THIS LINE IF YOUR 'cosmic' API DIFFERS ---
                            // assumed signature: cosmic.WriteMemory(long address, byte[] data)
                            cosmic.WriteMemory(addr, data);

                            // If your API is different, example alternatives:
                            // cosmic.WriteBytes(addr, data);
                            // cosmic.WriteMemory(addr, BitConverter.ToString(data).Replace("-", " "));
                        }
                        catch
                        {
                            // ignore single-address write failures, continue others
                        }
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                // Optionally log ex.ToString()
                return false;
            }
        }

        private void SetStatus(string text)
        {
            if (Status == null) return;
            if (Status.InvokeRequired)
                Status.Invoke(new Action(() => Status.Text = text));
            else
                Status.Text = text;
        }

        // --- helper: convert hex-string (with spaces) to byte[] ---
        private byte[] HexStringToBytes(string hexWithSpaces)
        {
            var cleaned = new string(hexWithSpaces.Where(c => !char.IsWhiteSpace(c)).ToArray());
            if (cleaned.Length % 2 != 0) throw new ArgumentException("Hex string length must be even.");
            var bytes = new byte[cleaned.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(cleaned.Substring(i * 2, 2), 16);
            return bytes;
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





        private async void guna2Button15_Click(object sender, EventArgs e)
        {
            try
            {
                if (!cosmic.SetProcess(new[] { "HD-Player" }))
                {
                    Status.Text = " Emulator Not Found!";
                    return;
                }

                cosmic.CheckProcess();

                // ============================
                //     SWITCH TURN ON
                // ============================
                if (!switchActive)
                {


                    Status.Text = " Switch: searching...";
                    var found = await cosmic.AoBScan(switchSearch);

                    if (found == null || !found.Any())
                    {
                        Status.Text = " Switch not found";
                        return;
                    }

                    int patched = 0;
                    foreach (var a in found)
                        if (cosmic.AobReplace(a, switchReplace))
                            patched++;

                    switchActive = true;
                    guna2Button15.Text = "Deactivate";   //  TEXT CHANGE

                    Status.Text = $" Switch ON  Patched:{patched}";
                }
                else
                {


                    // ============================
                    //     SWITCH TURN OFF
                    // ============================
                    Status.Text = " Switch: searching patched...";
                    var found = await cosmic.AoBScan(switchReplace);

                    if (found == null || !found.Any())
                    {
                        Status.Text = " Patched switch not found";
                        return;
                    }

                    int restored = 0;
                    foreach (var a in found)
                        if (cosmic.AobReplace(a, switchSearch))
                            restored++;

                    switchActive = false;
                    guna2Button15.Text = "Activate";   //  TEXT CHANGE

                    Status.Text = $" Switch OFF  Restored:{restored}";
                }
            }
            catch (Exception ex)
            {
                Status.Text = " Toggle error: " + ex.Message;
            }
        }



        private async void guna2Button17_Click(object sender, EventArgs e)
        {
            try
            {
                if (!cosmic.SetProcess(new[] { "HD-Player" }))
                {
                    Status.Text = " Emulator Not Found!";
                    return;
                }

                cosmic.CheckProcess();

                // ============================
                //       DELAY FIX ON
                // ============================
                if (!delayFixActive)
                {
                    Status.Text = " DelayFix: searching...";
                    var found = await cosmic.AoBScan(delaySearch);

                    if (found == null || !found.Any())
                    {
                        Status.Text = " Delay pattern not found";
                        return;
                    }

                    int patched = 0;
                    foreach (var a in found)
                        if (cosmic.AobReplace(a, delayReplace))
                            patched++;

                    delayFixActive = true;
                    guna2Button17.Text = "Deactivate";   //  TEXT CHANGE

                    Status.Text = $" DelayFix ON  Patched:{patched}";
                }
                else
                {
                    // ============================
                    //       DELAY FIX OFF
                    // ============================
                    Status.Text = " DelayFix: searching patched...";
                    var found = await cosmic.AoBScan(delayReplace);

                    if (found == null || !found.Any())
                    {
                        Status.Text = " Patched delay not found";
                        return;
                    }

                    int restored = 0;
                    foreach (var a in found)
                        if (cosmic.AobReplace(a, delaySearch))
                            restored++;

                    delayFixActive = false;
                    guna2Button17.Text = "Activate";   //  TEXT CHANGE

                    Status.Text = $" DelayFix OFF  Restored:{restored}";
                }
            }
            catch (Exception ex)
            {
                Status.Text = " DelayFix toggle error: " + ex.Message;
            }
        }



        private async void guna2Button19_Click(object sender, EventArgs e)
        {
            try
            {
                if (!cosmic.SetProcess(new[] { "HD-Player" }))
                {
                    Status.Text = " Emulator Not Found!";
                    return;
                }

                cosmic.CheckProcess();

                // ============================
                //        NOSCOPE ON
                // ============================
                if (!noScopeActive)
                {
                    Status.Text = " NoScope: searching...";
                    var found = await cosmic.AoBScan(noScopeSearch);

                    if (found == null || !found.Any())
                    {
                        Status.Text = " NoScope not found";
                        return;
                    }

                    int patched = 0;
                    foreach (var a in found)
                        if (cosmic.AobReplace(a, noScopeReplace))
                            patched++;

                    noScopeActive = true;
                    guna2Button19.Text = "Deactivate";   //  Text change

                    Status.Text = $" NoScope ON  Patched:{patched}";
                }
                else
                {
                    // ============================
                    //        NOSCOPE OFF
                    // ============================
                    Status.Text = " NoScope: searching patched...";
                    var found = await cosmic.AoBScan(noScopeReplace);

                    if (found == null || !found.Any())
                    {
                        Status.Text = " Patched NoScope not found";
                        return;
                    }

                    int restored = 0;
                    foreach (var a in found)
                        if (cosmic.AobReplace(a, noScopeSearch))
                            restored++;

                    noScopeActive = false;
                    guna2Button19.Text = "Activate";   //  Text change

                    Status.Text = $" NoScope OFF  Restored:{restored}";
                }
            }
            catch (Exception ex)
            {
                Status.Text = " NoScope toggle error: " + ex.Message;
            }
        }

        // ================= Addresses & Patterns — Ab saare upar consolidated section mein hain =================

        // ================= BUTTON METHODS =================



        private async void guna2Button31_Click(object sender, EventArgs e)
        {
            try
            {
                if (!cosmic.SetProcess(new[] { "HD-Player" }))
                {
                    Status.Text = " Emulator Not Found!";
                    return;
                }

                cosmic.CheckProcess();

                // ===================================
                //            SPEED ON
                // ===================================
                if (!speedActive)
                {
                    Status.Text = " SpeedHack: searching...";
                    speedAddresses = await cosmic.AoBScan(speedSearch);

                    if (speedAddresses == null || !speedAddresses.Any())
                    {
                        Status.Text = " Speed pattern not found";
                        return;
                    }

                    int patched = speedAddresses.Count(addr => cosmic.AobReplace(addr, speedReplace));

                    speedActive = true;
                    guna2Button31.Text = "Deactivate";   //  text change

                    Status.Text = $" Speed Hack ON ({patched})";
                }
                else
                {
                    // ===================================
                    //            SPEED OFF
                    // ===================================
                    if (speedAddresses == null || !speedAddresses.Any())
                    {
                        Status.Text = " No stored speed addresses!";
                        return;
                    }

                    int restored = speedAddresses.Count(addr => cosmic.AobReplace(addr, speedSearch));

                    speedActive = false;
                    guna2Button31.Text = "Activate";   //  text change

                    Status.Text = $" Speed Hack OFF ({restored})";
                }
            }
            catch (Exception ex)
            {
                Status.Text = " Speed Hack toggle error: " + ex.Message;
            }
        }



        private async void guna2Button29_Click(object sender, EventArgs e)
        {
            try
            {
                if (!cosmic.SetProcess(new[] { "HD-Player" }))
                {
                    Status.Text = " Emulator Not Found!";
                    return;
                }

                cosmic.CheckProcess();

                // ===============================
                //           WALL ON
                // ===============================
                if (!wallActive)
                {
                    Status.Text = " WallHack: searching...";
                    wallAddresses = await cosmic.AoBScan(wallSearch);

                    if (wallAddresses == null || !wallAddresses.Any())
                    {
                        Status.Text = " Wall pattern not found";
                        return;
                    }

                    int patched = wallAddresses.Count(addr => cosmic.AobReplace(addr, wallReplace));

                    wallActive = true;
                    guna2Button29.Text = "Deactivate";

                    Status.Text = $" Wall Hack ON ({patched})";
                }
                else
                {
                    // ===============================
                    //           WALL OFF
                    // ===============================
                    if (wallAddresses == null || !wallAddresses.Any())
                    {
                        Status.Text = " No stored wall addresses!";
                        return;
                    }

                    int restored = wallAddresses.Count(addr => cosmic.AobReplace(addr, wallSearch));

                    wallActive = false;
                    guna2Button29.Text = "Activate";

                    Status.Text = $" Wall Hack OFF ({restored})";
                }
            }
            catch (Exception ex)
            {
                Status.Text = " WallHack toggle error: " + ex.Message;
            }
        }



        private async void guna2Button27_Click(object sender, EventArgs e)
        {
            try
            {
                if (!cosmic.SetProcess(new[] { "HD-Player" }))
                {
                    Status.Text = " Emulator Not Found!";
                    return;
                }

                cosmic.CheckProcess();

                // ===========================
                //       CAMERA LEFT ON
                // ===========================
                if (!camLeftActive)
                {
                    Status.Text = " Camera Left: searching...";
                    camLeftAddresses = await cosmic.AoBScan(camLeftSearch);

                    if (camLeftAddresses == null || !camLeftAddresses.Any())
                    {
                        Status.Text = " Camera Left pattern not found";
                        return;
                    }

                    int patched = camLeftAddresses.Count(addr => cosmic.AobReplace(addr, camLeftReplace));

                    camLeftActive = true;
                    guna2Button27.Text = "Deactivate";

                    Status.Text = $" Camera Left ON ({patched})";
                }
                else
                {
                    // ===========================
                    //       CAMERA LEFT OFF
                    // ===========================
                    if (camLeftAddresses == null || !camLeftAddresses.Any())
                    {
                        Status.Text = " No stored camera-left addresses!";
                        return;
                    }

                    int restored = camLeftAddresses.Count(addr => cosmic.AobReplace(addr, camLeftSearch));

                    camLeftActive = false;
                    guna2Button27.Text = "Activate";

                    Status.Text = $" Camera Left OFF ({restored})";
                }
            }
            catch (Exception ex)
            {
                Status.Text = " Camera Left toggle error: " + ex.Message;
            }
        }



        private async void guna2Button25_Click(object sender, EventArgs e)
        {
            try
            {
                if (!cosmic.SetProcess(new[] { "HD-Player" }))
                {
                    Status.Text = " Emulator Not Found!";
                    return;
                }

                cosmic.CheckProcess();

                // ==========================
                //       VISION ON
                // ==========================
                if (!visionActive)
                {
                    Status.Text = " Vision: searching...";

                    // Your scan call
                    visionAddresses = await cosmic.AoBScan(visionSearch);

                    if (visionAddresses == null || !visionAddresses.Any())
                    {
                        Status.Text = " Vision pattern not found";
                        return;
                    }

                    // Your patch call
                    int patched = visionAddresses.Count(addr => cosmic.AobReplace(addr, visionReplace));

                    visionActive = true;
                    guna2Button25.Text = "Deactivate";

                    Status.Text = $" Vision ON ({patched})";
                }
                else
                {
                    // ==========================
                    //       VISION OFF
                    // ==========================
                    if (visionAddresses == null || !visionAddresses.Any())
                    {
                        Status.Text = " No stored vision addresses!";
                        return;
                    }

                    // Your restore call
                    int restored = visionAddresses.Count(addr => cosmic.AobReplace(addr, visionSearch));

                    visionActive = false;
                    guna2Button25.Text = "Activate";

                    Status.Text = $" Vision OFF ({restored})";
                }
            }
            catch (Exception ex)
            {
                Status.Text = " Vision toggle error: " + ex.Message;
            }
        }



        private async void guna2Button33_Click(object sender, EventArgs e)
        {
            try
            {
                if (!cosmic.SetProcess(new[] { "HD-Player" }))
                {
                    Status.Text = " Emulator Not Found!";
                    return;
                }

                cosmic.CheckProcess();

                // ===========================
                //        BLACK SKY ON
                // ===========================
                if (!blackSkyActive)
                {
                    Status.Text = " BlackSky: searching...";

                    blackSkyAddresses = await cosmic.AoBScan(blackSkySearch);

                    if (blackSkyAddresses == null || !blackSkyAddresses.Any())
                    {
                        Status.Text = " BlackSky pattern not found";
                        return;
                    }

                    int patched = blackSkyAddresses.Count(addr => cosmic.AobReplace(addr, blackSkyReplace));

                    blackSkyActive = true;
                    guna2Button33.Text = "Deactivate";

                    Status.Text = $" BlackSky ON ({patched})";
                }
                else
                {
                    // ===========================
                    //        BLACK SKY OFF
                    // ===========================
                    if (blackSkyAddresses == null || !blackSkyAddresses.Any())
                    {
                        Status.Text = " No stored BlackSky addresses!";
                        return;
                    }

                    int restored = blackSkyAddresses.Count(addr => cosmic.AobReplace(addr, blackSkySearch));

                    blackSkyActive = false;
                    guna2Button33.Text = "Activate";

                    Status.Text = $" BlackSky OFF ({restored})";
                }
            }
            catch (Exception ex)
            {
                Status.Text = " BlackSky toggle error: " + ex.Message;
            }
        }


        // ================= BUTTON METHODS =================


        private async void guna2Button38_Click(object sender, EventArgs e)
        {
            try
            {
                if (!cosmic.SetProcess(new[] { "HD-Player" }))
                {
                    Status.Text = " Emulator Not Found!";
                    return;
                }

                cosmic.CheckProcess();

                if (!fastReloadActive)
                {
                    Status.Text = " Fast Reload: searching...";
                    fastReloadAddresses = await cosmic.AoBScan(fastReloadSearch);

                    if (fastReloadAddresses == null || !fastReloadAddresses.Any())
                    {
                        Status.Text = " Fast Reload pattern not found";
                        return;
                    }

                    int patched = fastReloadAddresses.Count(a => cosmic.AobReplace(a, fastReloadReplace));

                    fastReloadActive = true;
                    guna2Button38.Text = "Deactivate";

                    Status.Text = $" Fast Reload ON ({patched})";
                }
                else
                {
                    if (fastReloadAddresses == null || !fastReloadAddresses.Any())
                    {
                        Status.Text = " No stored Fast Reload addresses!";
                        return;
                    }

                    int restored = fastReloadAddresses.Count(a => cosmic.AobReplace(a, fastReloadSearch));

                    fastReloadActive = false;
                    guna2Button38.Text = "Activate";

                    Status.Text = $" Fast Reload OFF ({restored})";
                }
            }
            catch (Exception ex)
            {
                Status.Text = " Fast Reload toggle error: " + ex.Message;
            }
        }

        private async void guna2Button35_Click(object sender, EventArgs e)
        {

        }

        private async void guna2Button36_Click(object sender, EventArgs e)
        {
            try
            {
                if (!cosmic.SetProcess(new[] { "HD-Player" }))
                {
                    Status.Text = " Emulator Not Found!";
                    return;
                }

                cosmic.CheckProcess();

                if (!noRecoilActive)
                {
                    Status.Text = " No Recoil: searching...";
                    noRecoilAddresses = await cosmic.AoBScan(noRecoilSearch);

                    if (noRecoilAddresses == null || !noRecoilAddresses.Any())
                    {
                        Status.Text = " No Recoil pattern not found";
                        return;
                    }

                    int patched = noRecoilAddresses.Count(a => cosmic.AobReplace(a, noRecoilReplace));

                    noRecoilActive = true;
                    guna2Button36.Text = "Deactivate";

                    Status.Text = $" No Recoil ON ({patched})";
                }
                else
                {
                    if (noRecoilAddresses == null || !noRecoilAddresses.Any())
                    {
                        Status.Text = " No stored No Recoil addresses!";
                        return;
                    }

                    int restored = noRecoilAddresses.Count(a => cosmic.AobReplace(a, noRecoilSearch));

                    noRecoilActive = false;
                    guna2Button36.Text = "Activate";

                    Status.Text = $" No Recoil OFF ({restored})";
                }
            }
            catch (Exception ex)
            {
                Status.Text = " No Recoil toggle error: " + ex.Message;
            }
        }

        private async void guna2Button21_Click(object sender, EventArgs e)
        {

        }

        private async void guna2Button34_Click(object sender, EventArgs e)
        {
            try
            {
                if (!cosmic.SetProcess(new[] { "HD-Player" }))
                {
                    Status.Text = " Emulator Not Found!";
                    return;
                }

                cosmic.CheckProcess();

                if (!unlimitedAmmoActive)
                {
                    Status.Text = " Unlimited Ammo: searching...";
                    unlimitedAmmoAddresses = await cosmic.AoBScan(unlimitedAmmoSearch);

                    if (unlimitedAmmoAddresses == null || !unlimitedAmmoAddresses.Any())
                    {
                        Status.Text = " Unlimited Ammo pattern not found";
                        return;
                    }

                    int patched = unlimitedAmmoAddresses.Count(a => cosmic.AobReplace(a, unlimitedAmmoReplace));

                    unlimitedAmmoActive = true;
                    guna2Button34.Text = "Deactivate";

                    Status.Text = $" Unlimited Ammo ON ({patched})";
                }
                else
                {
                    if (unlimitedAmmoAddresses == null || !unlimitedAmmoAddresses.Any())
                    {
                        Status.Text = " No stored Unlimited Ammo addresses!";
                        return;
                    }

                    int restored = unlimitedAmmoAddresses.Count(a => cosmic.AobReplace(a, unlimitedAmmoSearch));

                    unlimitedAmmoActive = false;
                    guna2Button34.Text = "Activate";

                    Status.Text = $" Unlimited Ammo OFF ({restored})";
                }
            }
            catch (Exception ex)
            {
                Status.Text = " Unlimited Ammo toggle error: " + ex.Message;
            }
        }



        private async void guna2Button20_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (!cosmic.SetProcess(new[] { "HD-Player" }))
                {
                    Status.Text = " Emulator Not Found!";
                    return;
                }

                cosmic.CheckProcess();

                if (!glitchFireActive)
                {
                    Status.Text = " Glitch Fire: searching...";
                    glitchFireAddresses = await cosmic.AoBScan(glitchFireSearch);

                    if (glitchFireAddresses == null || !glitchFireAddresses.Any())
                    {
                        Status.Text = " Glitch Fire pattern not found";
                        return;
                    }

                    int patched = glitchFireAddresses.Count(a => cosmic.AobReplace(a, glitchFireReplace));

                    glitchFireActive = true;
                    guna2Button20.Text = "Deactivate";

                    Status.Text = $" Glitch Fire ON ({patched})";
                }
                else
                {
                    if (glitchFireAddresses == null || !glitchFireAddresses.Any())
                    {
                        Status.Text = " No stored Glitch Fire addresses!";
                        return;
                    }

                    int restored = glitchFireAddresses.Count(a => cosmic.AobReplace(a, glitchFireSearch));

                    glitchFireActive = false;
                    guna2Button20.Text = "Activate";

                    Status.Text = $" Glitch Fire OFF ({restored})";
                }
            }
            catch (Exception ex)
            {
                Status.Text = " Glitch Fire toggle error: " + ex.Message;
            }
        }



        private async void guna2Button10_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (!cosmic.SetProcess(new[] { "HD-Player" }))
                {
                    Status.Text = " Emulator Not Found!";
                    return;
                }

                cosmic.CheckProcess();

                if (!fastLandingActive)
                {
                    Status.Text = " Fast Landing: searching...";
                    fastLandingAddresses = await cosmic.AoBScan(fastLandingSearch);

                    if (fastLandingAddresses == null || !fastLandingAddresses.Any())
                    {
                        Status.Text = " Fast Landing pattern not found";
                        return;
                    }

                    int patched = fastLandingAddresses.Count(a => cosmic.AobReplace(a, fastLandingReplace));

                    fastLandingActive = true;
                    guna2Button10.Text = "Deactivate";

                    Status.Text = $" Fast Landing ON ({patched})";
                }
                else
                {
                    if (fastLandingAddresses == null || !fastLandingAddresses.Any())
                    {
                        Status.Text = " No stored Fast Landing addresses!";
                        return;
                    }

                    int restored = fastLandingAddresses.Count(a => cosmic.AobReplace(a, fastLandingSearch));

                    fastLandingActive = false;
                    guna2Button10.Text = "Activate";

                    Status.Text = $" Fast Landing OFF ({restored})";
                }
            }
            catch (Exception ex)
            {
                Status.Text = " Fast Landing toggle error: " + ex.Message;
            }
        }

        private async void guna2Button2_Click_1(object sender, EventArgs e)
        {
            string dllName = "Client.dll";
            string tempPath = Path.Combine("C:\\Windows\\Temp", dllName);
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
            ExtractEmbeddedResource("FEAR_BRUTALS.Client.dll", tempPath);
            injectDll("FEAR_BRUTALS.AotBst.dll", "AotBst.dll");
            injectDll("FEAR_BRUTALS.cimgui.dll", "cimgui.dll");

        }










        // resetguestAddresses, resetguestSearch, resetguestReplace — Ab saare upar consolidated section mein hain
        private async void guna2Button7_Click_2(object sender, EventArgs e)
        {
            if (!cosmic.SetProcess(new[] { "HD-Player" })) { Status.Text = " Emulator Not Found!"; return; }
            cosmic.CheckProcess();
            resetguestAddresses = await cosmic.AoBScan(resetguestSearch);
            int patched = resetguestAddresses.Count(addr => cosmic.AobReplace(addr, resetguestReplace));
            Status.Text = $" GUEST ID RESET COMPLEATED ({patched})";
        }



        bool neckActive = false;




        private void guna2Button41_Click(object sender, EventArgs e)
        {
            try
            {
                if (!EnsureAttached()) return;
                if (!EnsureLoaded()) return;

                // ================================
                //          TURN ON
                // ================================
                if (!neckActive)
                {
                    soundActivated.Play(); //  Play Activated sound

                    Status.Text = " Neck Aimbot: scanning & patching...";

                    int writes = 0;

                    foreach (var baseAddr in foundBases)
                    {
                        long readAddr = baseAddr + NECK_READ_OFFSET;
                        long writeAddr = baseAddr + NECK_WRITE_OFFSET;

                        try
                        {
                            // Backup original value
                            if (!originalValues.ContainsKey(writeAddr))
                                originalValues[writeAddr] = memory.ReadInt(writeAddr);

                            // Read and patch
                            int val = memory.ReadInt(readAddr);
                            if (memory.AobReplace(writeAddr, val))
                                writes++;
                        }
                        catch { }
                    }

                    neckActive = true;
                    guna2Button41.Text = "Deactivate";

                    Status.Text = $" Neck Aimbot ON ({writes}/{foundBases.Count})";
                }
                else
                {
                    // ================================
                    //          TURN OFF
                    // ================================
                    soundDeactivated.Play(); //  Play Deactivated sound

                    Status.Text = " Restoring Neck Aimbot...";

                    int restores = 0;

                    foreach (var kv in originalValues)
                    {
                        try
                        {
                            if (memory.AobReplace(kv.Key, kv.Value))
                                restores++;
                        }
                        catch { }
                    }

                    neckActive = false;
                    guna2Button41.Text = "Activate";

                    Status.Text = $" Neck Aimbot OFF ({restores}/{originalValues.Count})";
                }
            }
            catch (Exception ex)
            {
                Status.Text = " Neck Aimbot toggle error: " + ex.Message;
            }
        }






        // ============================================================
        // Required by Designer — DO NOT REMOVE these empty handlers
        // They are subscribed in Main.Designer.cs
        // ============================================================
        private void guna2Panel4_Paint(object sender, PaintEventArgs e) { }   // Line 175
        private void tabPage3_Click(object sender, EventArgs e) { }            // Line 439
        private void label21_Click(object sender, EventArgs e) { }             // Line 789

        public class SniperScopeForCosmic
        {
            private Cosmic _cosmic;
            private Func<int> getKey;
            private List<long> sniperAddresses = new List<long>();
            private bool bSniperScanned = false;
            private bool bSniperScopeActive = false;
            private bool isKeyDown = false;

            // =====================================================
            //   SNIPER SCOPE PATTERNS  (sOrig = Search, sMod = Replace)
            //   Bilkul baaki features jaisa — simple search/replace
            // =====================================================

            // Search Pattern (scan ke liye)
            private const string sOrig =
                "9A 99 99 3E FF FF FF FF 08 00 00 00 00 00 60 40 " +
                "CD CC 8C 3F 8F C2 F5 3C CD CC CC 3D 06 00 00 00 " +
                "00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 3F " +
                "33 33 13 40 00 00 B0 3F 00 00 80 3F";

            // Replace Pattern (ON karne pe ye bytes likhte hain)
            private const string sMod =
                "9A 99 99 3E FF FF FF FF 08 00 00 00 00 00 60 40 " +
                "CD CC 8C 3F 8F C2 F5 3C CD CC CC 3D 06 00 00 00 " +
                "3E 00 00 00 00 00 00 00 00 00 00 00 00 00 80 3F " +
                "33 33 13 40 00 00 B0 3F 00 00 80 3F";

            public SniperScopeForCosmic(Cosmic cosmic, Func<int> keyProvider = null)
            {
                _cosmic = cosmic;
                getKey = keyProvider;
            }

            // =====================================================
            //   SCAN — sOrig pattern dhundho, addresses store karo
            // =====================================================
            public async Task<bool> ScanSniperScope()
            {
                if (_cosmic._processHandle == IntPtr.Zero)
                    return false;

                sniperAddresses.Clear();

                IEnumerable<long> found = await _cosmic.AoBScan(sOrig);
                sniperAddresses = found?.ToList() ?? new List<long>();
                bSniperScanned = sniperAddresses.Count > 0;

                if (bSniperScanned)
                    Beep(250, 300);

                return bSniperScanned;
            }

            // =====================================================
            //   ACTIVATE / DEACTIVATE
            // =====================================================
            public bool SetSniperScopeActive(bool active)
            {
                if (!bSniperScanned || sniperAddresses.Count == 0)
                    return false;

                if (active)
                {
                    int patched = sniperAddresses.Count(addr => _cosmic.AobReplace(addr, sMod));
                    bSniperScopeActive = true;
                    return true;
                }
                else
                {
                    int restored = sniperAddresses.Count(addr => _cosmic.AobReplace(addr, sOrig));
                    bSniperScopeActive = false;
                    return true;
                }
            }

            // =====================================================
            //   TOGGLE — ON: sMod likhdo | OFF: sOrig restore karo
            // =====================================================
            public bool Toggle()
            {
                return SetSniperScopeActive(!bSniperScopeActive);
            }

            // =====================================================
            //   KEYBIND POLLING UPDATE (Main_Load loop se chalta hai)
            // =====================================================
            public void UpdateSniperScope()
            {
                if (!bSniperScanned || sniperAddresses.Count == 0 || getKey == null)
                    return;

                int key = getKey();
                if (key <= 0)
                    return;

                bool pressed = (GetAsyncKeyState(key) & 0x8000) != 0;
                if (pressed && !isKeyDown)
                {
                    isKeyDown = true;
                    SetSniperScopeActive(true);
                }
                else if (!pressed && isKeyDown)
                {
                    isKeyDown = false;
                    SetSniperScopeActive(false);
                }
            }

            public bool IsScanned => bSniperScanned;
            public bool IsActive => bSniperScopeActive;
            public int Count => sniperAddresses.Count;
        }

        private async void guna2Button3_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (!cosmic.SetProcess(new[] { "HD-Player" }))
                { Status.Text = " Emulator Not Found!"; return; }
                cosmic.CheckProcess();
                Status.Text = " Sniper: scanning...";
                bool ok = await sniperScope.ScanSniperScope();
                Status.Text = ok ? $" Sniper SCAN DONE ({sniperScope.Count} found)" : " Sniper NOT FOUND";
            }
            catch (Exception ex) { Status.Text = " Sniper scan error: " + ex.Message; }
        }

        private void guna2Panel3_Paint(object sender, PaintEventArgs e) { }

        private void guna2Button5_Click_2(object sender, EventArgs e)
        {
            if (!sniperScope.IsScanned) { Status.Text = " Pehle scan karo!"; return; }
            sniperScope.Toggle();
            bool nowActive = sniperScope.IsActive;
            Status.Text = nowActive ? " SNIPER ON" : " SNIPER OFF";
            guna2Button5.Text = nowActive ? "Deactivate" : "Activate";
        }

        private async void guna2Button4_Click_1(object sender, EventArgs e)
        {
            isBinding = true;
            Status.Text = "Press any key or mouse button...";
            guna2Button4.Text = "...";
            await Task.Run(() =>
            {
                while (isBinding)
                {
                    for (int i = 1; i <= 255; i++)
                    {
                        if ((GetAsyncKeyState(i) & 0x8000) != 0)
                        { sniperKey = i; isBinding = false; break; }
                    }
                    Thread.Sleep(10);
                }
            });
            guna2Button4.Invoke((MethodInvoker)(() =>
            {
                guna2Button4.Text = $"Key: {sniperKey}";
                Status.Text = $"Key set: {sniperKey}";
            }));
        }
    }
}
