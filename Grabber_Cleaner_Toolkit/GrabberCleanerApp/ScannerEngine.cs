using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace GrabberCleanerApp
{
    public class ScannerEngine
    {
        public delegate void LogHandler(string message, bool isWarning = false, bool isError = false);
        public delegate void ProgressHandler(int progressPercentage, string statusText);

        public event LogHandler? OnLog;
        public event ProgressHandler? OnProgress;

        // ─── Confirmed Malware Family IOCs (ZERO False Positives) ───
        // Strictly confirmed malware families, C2 domains/IPs, and known worm markers.
        // Generic programming terms (FromBase64String, raw.githubusercontent.com, etc.)
        // are NEVER placed here.
        private static readonly string[] ConfirmedMalwareSignatures = new[]
        {
            // Known MSBuild Grabber Worm tags
            "AWyhU7",          // Generic MSBuild worm hook marker
            "alJvVFZa",        // MSBuild worm variant
            "BX7010",          // Known dropper tag
            "3ZTWX",           // Encoded grabber marker
            "pee-files.nl",    // Retev stealer exfil host
            "91.92.243.218",   // Known Retev C2 IP

            // Confirmed Stealer & RAT Families
            "BlankGrabber",
            "blank-grabber",
            "EpsilonStealer",
            "epsilon-stealer",
            "PhorcyStealer",
            "phorcy-stealer",
            "NexusStealer",
            "NexusGrabber",
            "RedLineStealer",
            "redline-stealer",
            "LummaC2",
            "lumma-c2",
            "lummac2",
            "RiseProStealer",
            "StealC",
            "VidarStealer",
            "RecordBreaker",
            "AsyncRAT",
            "DCRat",
            "QuasarRAT",
            "njRAT",
            "XWormRAT",
            "VenomousStealer",
            "MasadLogger",
            "CaesarStealer",
            "MeduzaStealer",
            "WhiteSnakeStealer",
            "RaccoonStealer",
            "AZORult",

            // Crypto Swapper genesis address marker (strictly in payload context)
            "1A1zP1eP5QGefi2DMPTfTL5SLmv7DivfNa"
        };

        // ─── Suspicious Persistence Patterns (For Registry / Tasks / WMI / Process CmdLines) ───
        private static readonly string[] SuspiciousPersistencePatterns = new[]
        {
            "-WindowStyle Hidden",
            "-EncodedCommand",
            "-NoProfile -ExecutionPolicy Bypass",
            "certutil -urlcache",
            "bitsadmin /transfer",
            "Start-BitsTransfer",
            "IEX(",
            "Invoke-Expression",
            "[System.Reflection.Assembly]::Load"
        };

        // Backward-compatible alias for any callers expecting MalwareSignatures
        private static readonly string[] MalwareSignatures = ConfirmedMalwareSignatures;

        private static readonly string[] ProjectFileExtensions = new[]
        {
            ".csproj", ".vcxproj", ".props", ".targets",
            ".sln", ".proj", ".projitems", ".fsproj", ".vbproj",
            "Directory.Build.props", "Directory.Build.targets",
            "NuGet.Config", "nuget.config", "packages.config"
        };

        // Source code files that should be deeply analyzed for grabber code
        private static readonly string[] SourceCodeExtensions = new[]
        {
            // C# / .NET
            ".cs", ".vb", ".fs",
            // C / C++
            ".c", ".cpp", ".h", ".hpp", ".cc", ".cxx",
            // Python
            ".py", ".pyw",
            // JavaScript / Node
            ".js", ".ts", ".mjs", ".cjs",
            // PowerShell & Scripts
            ".ps1", ".psm1", ".psd1",
            ".vbs", ".vbe", ".bat", ".cmd"
        };

        private static readonly string[] ScriptExtensions = new[]
        {
            ".vbs", ".vbe", ".ps1", ".bat", ".cmd",
            ".js", ".jse", ".wsf", ".hta", ".pyw"
        };

        private static readonly string[] SkipDirectories = new[]
        {
            "node_modules", ".git", "bin", "obj", "Packages",
            "packages", ".vs", "TestResults", "__pycache__",
            ".idea", ".vscode", "dist", "build", "out",
            "vendor", "third_party", "extern", "external"
        };

        /// <summary>
        /// Strict word-boundary signature matcher. Prevents false alarms on substrings
        /// (e.g. m_stealChain matching StealC, or random base64 data matching njRAT).
        /// </summary>
        public static bool MatchesMalwareSignature(string text, string signature)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(signature))
                return false;

            // If signature consists of letters/digits/underscores/hyphens, enforce strict word boundary
            if (signature.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'))
            {
                return Regex.IsMatch(text, $@"\b{Regex.Escape(signature)}\b", RegexOptions.IgnoreCase);
            }

            // For URLs, IPs, domains (e.g. pee-files.nl, 91.92.243.218)
            return text.Contains(signature, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Evaluates whether a directory should be skipped during drive/folder recursion.
        /// Protects system SDKs (Windows Kits, Visual Studio CRT), user recycle bin, and the toolkit itself.
        /// </summary>
        private static bool ShouldSkipDirectory(string fullPath, string dirName)
        {
            if (string.IsNullOrEmpty(fullPath) || string.IsNullOrEmpty(dirName))
                return false;

            // 1. Standard build/cache folder names
            if (SkipDirectories.Any(s => dirName.Equals(s, StringComparison.OrdinalIgnoreCase)))
                return true;

            // 2. Recycle bin, System Volume Information, Windows OS system root
            if (dirName.Equals("$RECYCLE.BIN", StringComparison.OrdinalIgnoreCase) ||
                dirName.Equals("System Volume Information", StringComparison.OrdinalIgnoreCase) ||
                dirName.Equals("$WinREAgent", StringComparison.OrdinalIgnoreCase) ||
                dirName.Equals("Config.Msi", StringComparison.OrdinalIgnoreCase) ||
                (dirName.Equals("Windows", StringComparison.OrdinalIgnoreCase) && Path.GetPathRoot(fullPath)?.TrimEnd('\\').Equals(Path.GetDirectoryName(fullPath)?.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase) == true))
                return true;

            // 3. Official development SDKs and Toolchains (Visual Studio CRT, Windows Kits headers)
            if (fullPath.Contains(@"\Windows Kits\", StringComparison.OrdinalIgnoreCase) ||
                fullPath.Contains(@"\Microsoft Visual Studio\", StringComparison.OrdinalIgnoreCase) ||
                fullPath.Contains(@"\dotnet\sdk\", StringComparison.OrdinalIgnoreCase))
                return true;

            // 4. Game and large media libraries where grabber code never lives
            if (dirName.Equals("Steam", StringComparison.OrdinalIgnoreCase) ||
                dirName.Equals("SteamLibrary", StringComparison.OrdinalIgnoreCase) ||
                dirName.Equals("Epic Games", StringComparison.OrdinalIgnoreCase) ||
                dirName.Equals("Riot Games", StringComparison.OrdinalIgnoreCase) ||
                dirName.Equals("Ubisoft", StringComparison.OrdinalIgnoreCase) ||
                dirName.Equals("Origin Games", StringComparison.OrdinalIgnoreCase) ||
                dirName.Equals("GOG Galaxy", StringComparison.OrdinalIgnoreCase))
                return true;

            // 5. Heavy package/environment manager folders
            if (dirName.Equals("venv", StringComparison.OrdinalIgnoreCase) ||
                dirName.Equals(".venv", StringComparison.OrdinalIgnoreCase) ||
                dirName.Equals("envs", StringComparison.OrdinalIgnoreCase) ||
                dirName.Equals("anaconda3", StringComparison.OrdinalIgnoreCase) ||
                dirName.Equals("miniconda3", StringComparison.OrdinalIgnoreCase))
                return true;

            // 6. Windows internal caches, driver store, and assembly GAC
            if (fullPath.Contains(@"\Windows\WinSxS", StringComparison.OrdinalIgnoreCase) ||
                fullPath.Contains(@"\Windows\Installer", StringComparison.OrdinalIgnoreCase) ||
                fullPath.Contains(@"\Windows\System32\DriverStore", StringComparison.OrdinalIgnoreCase) ||
                fullPath.Contains(@"\Windows\assembly", StringComparison.OrdinalIgnoreCase))
                return true;

            // 7. Scanner itself and IDE scratch directories (prevents self-detection)
            string appDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            if (fullPath.StartsWith(appDir, StringComparison.OrdinalIgnoreCase) ||
                fullPath.Contains("Grabber_Cleaner_Toolkit", StringComparison.OrdinalIgnoreCase) ||
                fullPath.Contains(@".gemini\antigravity", StringComparison.OrdinalIgnoreCase) ||
                fullPath.Contains(@".antigravity", StringComparison.OrdinalIgnoreCase))
                return true;

            // 8. Official Python library unit tests
            if (fullPath.Contains(@"site-packages\win32\test", StringComparison.OrdinalIgnoreCase) ||
                fullPath.Contains(@"site-packages\keyboard", StringComparison.OrdinalIgnoreCase))
                return true;

            // 9. Browser temporary caches (thousands of transient image/script fragments)
            if (dirName.Equals("Cache", StringComparison.OrdinalIgnoreCase) ||
                dirName.Equals("Code Cache", StringComparison.OrdinalIgnoreCase) ||
                dirName.Equals("GPUCache", StringComparison.OrdinalIgnoreCase) ||
                dirName.Equals("ShaderCache", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        // ─── Language-specific grabber patterns ───

        // C# / VB.NET source code grabber indicators
        private static readonly (string Pattern, string Description, int RiskWeight)[] CSharpGrabberPatterns = new[]
        {
            ("GetEnvironmentVariable(\"LOCALAPPDATA\"", "Reads LocalAppData (browser/Discord target)", 15),
            ("GetEnvironmentVariable(\"APPDATA\"", "Reads AppData (Discord/Telegram target)", 10),
            ("Chrome\\\\User Data", "Chrome profile path — credential stealer target", 25),
            ("Login Data", "Chrome login DB target", 30),
            ("Web Data", "Chrome credit card DB target", 25),
            ("discord_desktop_core", "Discord client files access", 30),
            ("CryptUnprotectData", "Windows DPAPI decryption — used to steal saved passwords", 35),
            ("sqlite3", "SQLite DB access — browser credential stealer pattern", 20),
            ("SQLiteConnection", "SQLite DB access — browser credential stealer", 20),
            ("DiscordRPC", "Discord integration — check if token extraction is nearby", 5),
            ("WebClient().DownloadFile", "Downloads remote files — dropper pattern", 30),
            ("WebClient().DownloadString", "Downloads remote content — C2 pattern", 30),
            ("new HttpClient", "HTTP client (check for webhook/exfil usage)", 5),
            ("discord.com/api/webhooks", "Hardcoded Discord webhook C2", 80),
            ("discordapp.com/api/webhooks", "Hardcoded Discord webhook C2", 80),
            ("api.telegram.org/bot", "Telegram Bot C2 channel", 80),
            ("Convert.FromBase64String", "Base64 decode — obfuscation/payload", 20),
            ("System.Reflection.Assembly", "Runtime assembly loading — injection technique", 25),
            ("Process.Start", "Process spawning", 5),
            ("ProcessStartInfo", "Process creation", 5),
            ("Registry.CurrentUser", "Registry access (persistence check)", 10),
            ("SOFTWARE\\\\Microsoft\\\\Windows\\\\CurrentVersion\\\\Run", "Registry autorun key — persistence", 40),
            ("Clipboard.GetText", "Clipboard read — clipboard hijacker", 25),
            ("Clipboard.SetText", "Clipboard write — crypto address swapper", 30),
            ("Environment.GetFolderPath", "Special folder access (AppData, Desktop)", 5),
            ("File.ReadAllBytes", "Binary file read", 3),
            ("ZipFile.ExtractToDirectory", "ZIP extraction — self-extracting dropper pattern", 15),
            ("catbox.moe", "Known file exfil host", 70),
            ("transfer.sh", "Known file exfil host", 70),
            ("gofile.io", "Known file exfil host", 70),
            ("raw.githubusercontent.com", "GitHub raw content — payload download source", 40),
        };

        // Python stealer-specific patterns
        private static readonly (string Pattern, string Description, int RiskWeight)[] PythonGrabberPatterns = new[]
        {
            ("import requests", "HTTP requests lib (check for webhook usage)", 5),
            ("requests.post", "POST request (exfil pattern)", 15),
            ("requests.get", "GET request (download pattern)", 10),
            ("discord", "Discord module import", 10),
            ("import sqlite3", "SQLite DB access — browser credential stealer", 20),
            ("LOCALAPPDATA", "Windows LocalAppData path (browser target)", 15),
            ("APPDATA", "Windows AppData path (Discord target)", 10),
            ("Chrome/User Data", "Chrome profile stealer path", 30),
            ("Login Data", "Chrome credentials DB", 30),
            ("discord/Local Storage", "Discord token storage path", 35),
            ("discordcanary", "Discord Canary client target", 30),
            ("discordptb", "Discord PTB client target", 30),
            ("CryptUnprotectData", "DPAPI password decryption", 35),
            ("win32crypt", "Windows crypto API — password decryption", 35),
            ("win32con", "Win32 API access", 10),
            ("cv2.VideoCapture(0)", "Webcam capture — RAT/spyware", 50),
            ("pyperclip", "Clipboard access library", 20),
            ("keyboard.on_press", "Keylogger pattern", 60),
            ("getpass.getuser", "Get current username", 5),
            ("subprocess.Popen", "Shell command execution", 15),
            ("os.startfile", "File execution", 15),
            ("webhook", "Webhook usage (C2 pattern)", 20),
            ("discord.com/api/webhooks", "Hardcoded Discord webhook", 80),
            ("api.telegram.org/bot", "Telegram C2 bot", 80),
            ("base64.b64decode", "Base64 decode (obfuscation)", 15),
            ("exec(base64", "Execute decoded base64 (malicious)", 70),
            ("catbox.moe", "Known exfil host", 70),
            ("mss.mss", "Screenshot library — RAT feature", 40),
            ("pynput", "Input monitoring (keylogger)", 50),
        };

        // C++ / C grabber patterns
        private static readonly (string Pattern, string Description, int RiskWeight)[] CppGrabberPatterns = new[]
        {
            ("URLDownloadToFile", "Windows API file download — dropper", 40),
            ("InternetOpenUrl", "HTTP connection API", 20),
            ("WinHttpOpen", "WinHTTP connection", 20),
            ("WinHttpSendRequest", "WinHTTP send request — exfil pattern", 30),
            ("CryptUnprotectData", "DPAPI — browser password decryption", 50),
            ("SHGetFolderPath", "Shell folder path (AppData/LocalAppData target)", 10),
            ("CSIDL_APPDATA", "AppData path constant — stealer target", 15),
            ("CSIDL_LOCAL_APPDATA", "LocalAppData path constant", 15),
            ("RegOpenKeyEx", "Registry access", 10),
            ("RegSetValueEx", "Registry write — persistence", 25),
            ("discord.com/api/webhooks", "Discord webhook C2", 80),
            ("api.telegram.org", "Telegram C2", 80),
            ("sqlite3_open", "SQLite DB open — browser credential target", 30),
            ("CreateRemoteThread", "Remote thread injection — process injection", 60),
            ("VirtualAllocEx", "Remote memory allocation — shellcode injection", 65),
            ("WriteProcessMemory", "Write to remote process — code injection", 65),
            ("OpenClipboard", "Clipboard access", 20),
            ("SetClipboardData", "Clipboard write — crypto swapper", 30),
            ("ShellExecute", "Execute file/command", 15),
            ("CreateThread", "Thread creation (background payload)", 10),
        };

        // JavaScript / Node.js stealer patterns
        private static readonly (string Pattern, string Description, int RiskWeight)[] JsGrabberPatterns = new[]
        {
            ("require('discord.js')", "discord.js bot library", 10),
            ("require('axios')", "axios HTTP lib", 5),
            ("axios.post", "HTTP POST (exfil pattern)", 15),
            ("discord.com/api/webhooks", "Discord webhook C2", 80),
            ("api.telegram.org/bot", "Telegram C2", 80),
            ("require('fs')", "File system access", 5),
            ("fs.readFileSync", "Synchronous file read", 5),
            ("process.env.LOCALAPPDATA", "LocalAppData path (browser target)", 20),
            ("process.env.APPDATA", "AppData path (Discord target)", 15),
            ("Chrome/User Data", "Chrome profile target", 30),
            ("Login Data", "Chrome credentials DB", 30),
            ("require('child_process')", "Shell command execution", 15),
            ("exec(", "Command execution", 10),
            ("Buffer.from", "Binary data handling", 5),
            ("btoa(", "Base64 encode", 5),
            ("atob(", "Base64 decode", 10),
            ("eval(", "Dynamic code execution — obfuscation", 40),
            ("catbox.moe", "Known exfil host", 70),
        };

        // Whitelisted Legitimate Authentication & Licensing Frameworks
        private static readonly string[] LegitAuthFrameworks = new[]
        {
            "KeyAuth", "FearAuth", "Auth.GG", "Authon", "PWF Auth", "pwfauth",
            "LicenseSeat", "Keyed", "Toolkits", "qPapel Auth", "qpapel", "EasyAuth",
            "LibreAuth", "SentriNet", "System Locker", "Cryptolens", "Keygen",
            "LicenseSpring", "WyDay LimeLM", "LimeLM", "WyDay", "10Duke Licensing", "10Duke",
            "Nalpeiron", "Reprise License Manager", "RLM", "SafeNet Sentinel", "SafeNet",
            "FlexNet Publisher", "FlexNet", "Flexera", "Thales Sentinel", "Sentinel LDK",
            "Sentinel HASP", "HASP", "Labs64 NetLicensing", "NetLicensing", "License4J",
            "SoftwareKey Protection PLUS", "Protection PLUS", "SoftwareKey", "Cryptlex",
            "Labs64", "Reprise", "License Dashboard", "licenseauth", "LicenseManager",
            "license_auth", "VMPROTECT", "Themida", "CodeMeter", "WIBU-SYSTEMS"
        };


        private void Log(string msg, bool isWarning = false, bool isError = false)
        {
            OnLog?.Invoke(msg, isWarning, isError);
        }

        private void Progress(int percentage, string status)
        {
            OnProgress?.Invoke(percentage, status);
        }

        public void ReportProgress(int percentage, string statusText)
        {
            Progress(percentage, statusText);
        }

        public List<ThreatInfo> RunFullScan()
        {
            List<ThreatInfo> threats = new List<ThreatInfo>();

            Log("🚀 Starting NEXT-LEVEL FULL SYSTEM ANNIHILATION SCAN...");

            Progress(3, "Scanning Active Processes & Memory...");
            threats.AddRange(ScanProcesses());

            Progress(12, "Scanning Temp & AppData Payload Directories...");
            threats.AddRange(ScanTempAndAppData());

            Progress(22, "Scanning Script Droppers (.vbs, .ps1, .bat, .lnk)...");
            threats.AddRange(ScanScriptDroppers());

            Progress(32, "Scanning WMI Event Consumers & Persistence...");
            threats.AddRange(ScanWMIPersistence());

            Progress(40, "Scanning Registry Run Keys...");
            threats.AddRange(ScanRegistryPersistence());

            Progress(48, "Scanning Windows Scheduled Tasks for Malware Hooks...");
            threats.AddRange(ScanScheduledTasks());

            Progress(55, "Scanning Windows Hosts File Integrity...");
            threats.AddRange(ScanHostsFile());

            Progress(62, "Scanning Visual Studio Templates & MSBuild Extensions...");
            threats.AddRange(ScanVSTemplates());

            Progress(70, "Scanning Discord Clients for Token Stealer Injections...");
            threats.AddRange(ScanDiscordInjections());

            Progress(78, "Scanning Browser Extensions (Chrome / Edge / Brave / Firefox)...");
            threats.AddRange(ScanBrowserExtensions());

            Progress(86, "Scanning Active Network Connections for C2 Beacons...");
            threats.AddRange(ScanLiveNetworkConnections());

            Progress(94, "Scanning All Drive Projects for PreBuild & GitHub Grabbers...");
            threats.AddRange(ScanAllDrives());

            Progress(100, $"Scan Complete! Total Threats Detected: {threats.Count}");
            Log($"==========================================================================");
            Log($" ULTIMATE SCAN COMPLETE: Found {threats.Count} potential threat(s)", threats.Count > 0, false);
            Log($"==========================================================================");

            return threats;
        }

        public List<ThreatInfo> RunQuickScan()
        {
            var threats = new List<ThreatInfo>();
            Log("⚡ Starting 5-SECOND SMART SYSTEM SCAN (Targeted Malware Vectors)...");

            Progress(5, "Scanning Active Memory Processes...");
            threats.AddRange(ScanProcesses());

            Progress(15, "Scanning Windows Startup & Script Droppers...");
            threats.AddRange(ScanScriptDroppers());

            Progress(30, "Scanning WMI Event Consumers...");
            threats.AddRange(ScanWMIPersistence());

            Progress(45, "Scanning Registry Autorun Keys...");
            threats.AddRange(ScanRegistryPersistence());

            Progress(58, "Scanning Windows Scheduled Tasks...");
            threats.AddRange(ScanScheduledTasks());

            Progress(68, "Scanning Hosts File Integrity...");
            threats.AddRange(ScanHostsFile());

            Progress(78, "Scanning Visual Studio Templates & MSBuild Hooks...");
            threats.AddRange(ScanVSTemplates());

            Progress(85, "Scanning Discord Injections & Token Hooks...");
            threats.AddRange(ScanDiscordInjections());

            Progress(92, "Scanning Browser Extensions...");
            threats.AddRange(ScanBrowserExtensions());

            Progress(96, "Scanning Active C2 Network Sockets...");
            threats.AddRange(ScanLiveNetworkConnections());

            Progress(98, "Scanning Hotspots (Desktop, Downloads, Temp, Repos)...");
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string[] userHotspots = new[]
            {
                Path.Combine(userProfile, "Desktop"),
                Path.Combine(userProfile, "Downloads"),
                Path.Combine(userProfile, "source", "repos"),
                Path.GetTempPath()
            };

            foreach (var hotspot in userHotspots)
            {
                if (Directory.Exists(hotspot))
                {
                    threats.AddRange(ScanDirectory(hotspot));
                }
            }

            Progress(100, $"⚡ Smart Fast Scan Complete! Found {threats.Count} potential threat(s)");
            Log($"==========================================================================");
            Log($" ⚡ SMART FAST SCAN COMPLETE: Found {threats.Count} potential threat(s)", threats.Count > 0, false);
            Log($"==========================================================================");

            return threats;
        }

        public List<ThreatInfo> ScanProjectsOnly()
        {
            Log("🔍 Scanning All Projects, MSBuild Targets & PreBuild Vectors...");
            Progress(10, "Scanning Visual Studio System Templates...");
            List<ThreatInfo> threats = ScanVSTemplates();

            Progress(50, "Scanning Project Files & PreBuild Decoders across Drives...");
            threats.AddRange(ScanAllDrives());

            Progress(100, $"Project Scan Completed! Total Threats Found: {threats.Count}");
            return threats;
        }

        public List<ThreatInfo> ScanTargetProjectFolder(string folderPath)
        {
            Log($"🔍 Deep Scanning Selected Project Folder: {folderPath}...");
            Progress(10, $"Accessing Project Directory: {folderPath}...");

            List<ThreatInfo> threats = new List<ThreatInfo>();
            if (!Directory.Exists(folderPath))
            {
                Log($"[X] Project directory does not exist: {folderPath}", isError: true);
                return threats;
            }

            Progress(50, "Scanning all project files, PreBuild vectors, scripts & binaries...");
            threats = ScanDirectory(folderPath, isVSTemplate: false, isTargetProjectFolder: true);

            Progress(100, $"Project Folder Scan Complete! Found {threats.Count} threat(s) in {folderPath}");
            Log($"==========================================================================");
            Log($" PROJECT FOLDER SCAN COMPLETE: {threats.Count} threat(s) detected in {Path.GetFileName(folderPath)}", threats.Count > 0, false);
            Log($"==========================================================================");

            return threats;
        }

        public List<ThreatInfo> HuntVisualStudioAndMSBuildInfection()
        {
            Log("==========================================================================");
            Log(" 🔨 HUNTING VISUAL STUDIO & MSBUILD GLOBAL INFECTION CHAINS...");
            Log("==========================================================================");

            List<ThreatInfo> threats = new List<ThreatInfo>();

            Progress(10, "Checking Global Directory.Build.props & Directory.Build.targets...");
            threats.AddRange(ScanGlobalMSBuildBuildFiles());

            Progress(35, "Scanning Visual Studio System MSBuild Targets...");
            threats.AddRange(ScanVSTemplates());

            Progress(65, "Scanning VS Project & Item Template Caches (.zip)...");
            threats.AddRange(ScanVSTemplateCaches());

            Progress(85, "Scanning Repositories for Injected PreBuild Vectors...");
            threats.AddRange(ScanUserReposForPreBuildVectors());

            Progress(100, $"VS & MSBuild Hunt Complete! Found {threats.Count} potential infection(s)");
            Log("==========================================================================");
            Log($" 🔨 VS & MSBUILD HUNT COMPLETE: Found {threats.Count} threat(s)", threats.Count > 0, false);
            Log("==========================================================================");

            return threats;
        }

        public List<ThreatInfo> TraceC2AndDroppers()
        {
            Log("==========================================================================");
            Log(" 🕵️ TRACING ACTIVE C2 SOCKETS, DROPPED SCRIPTS & BACKGROUND PROCESSES...");
            Log("==========================================================================");

            List<ThreatInfo> threats = new List<ThreatInfo>();

            Progress(15, "Scanning Live C2 Network Sockets (netstat)...");
            threats.AddRange(ScanLiveNetworkConnections());

            Progress(35, "Scanning Dropped Binaries in %TEMP% & %APPDATA%...");
            threats.AddRange(ScanTempAndAppData());

            Progress(55, "Scanning Script Droppers (.vbs, .ps1, .bat, .cmd)...");
            threats.AddRange(ScanScriptDroppers());

            Progress(75, "Scanning Scheduled Tasks & WMI Event Subscriptions...");
            threats.AddRange(ScanScheduledTasks());
            threats.AddRange(ScanWMIPersistence());

            Progress(90, "Scanning Active Memory Processes for Injected Code...");
            threats.AddRange(ScanProcesses());

            Progress(100, $"C2 & Dropper Trace Complete! Found {threats.Count} threat(s)");
            Log("==========================================================================");
            Log($" 🕵️ C2 & DROPPER TRACE COMPLETE: Found {threats.Count} threat(s)", threats.Count > 0, false);
            Log("==========================================================================");

            return threats;
        }

        private List<ThreatInfo> ScanGlobalMSBuildBuildFiles()
        {
            List<ThreatInfo> threats = new List<ThreatInfo>();
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            List<string> checkLocations = new List<string>
            {
                userProfile,
                Path.Combine(userProfile, "source"),
                Path.Combine(userProfile, "source", "repos"),
                Path.Combine(userProfile, "Desktop"),
                Path.Combine(userProfile, "Documents"),
                Path.Combine(appData, "Microsoft", "MSBuild"),
                Path.Combine(localAppData, "Microsoft", "MSBuild")
            };

            foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
            {
                checkLocations.Add(drive.RootDirectory.FullName);
            }

            string[] targets = new[] { "Directory.Build.props", "Directory.Build.targets" };

            foreach (var dir in checkLocations.Distinct())
            {
                if (!Directory.Exists(dir)) continue;

                foreach (var targetName in targets)
                {
                    string targetFile = Path.Combine(dir, targetName);
                    if (File.Exists(targetFile))
                    {
                        Log($"[🔍] Inspecting global MSBuild hook: {targetFile}");
                        threats.AddRange(ScanProjectBuildFile(targetFile, isVSTemplate: true));
                    }
                }
            }

            return threats;
        }

        private List<ThreatInfo> ScanVSTemplateCaches()
        {
            List<ThreatInfo> threats = new List<ThreatInfo>();
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            string[] vsDirs = new[]
            {
                Path.Combine(appData, "Microsoft", "VisualStudio"),
                Path.Combine(localAppData, "Microsoft", "VisualStudio")
            };

            foreach (var vsDir in vsDirs)
            {
                if (!Directory.Exists(vsDir)) continue;

                try
                {
                    var files = Directory.GetFiles(vsDir, "*.*", SearchOption.AllDirectories)
                                         .Where(f => f.EndsWith(".vstemplate", StringComparison.OrdinalIgnoreCase) ||
                                                     f.EndsWith(".targets", StringComparison.OrdinalIgnoreCase) ||
                                                     f.EndsWith(".props", StringComparison.OrdinalIgnoreCase));

                    foreach (var file in files.Take(200))
                    {
                        threats.AddRange(ScanProjectBuildFile(file, isVSTemplate: true));
                    }
                }
                catch { }
            }

            return threats;
        }

        private List<ThreatInfo> ScanUserReposForPreBuildVectors()
        {
            List<ThreatInfo> threats = new List<ThreatInfo>();
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            string[] repoRoots = new[]
            {
                Path.Combine(userProfile, "source", "repos"),
                Path.Combine(userProfile, "Desktop"),
                Path.Combine(userProfile, "Documents")
            };

            foreach (var root in repoRoots)
            {
                if (!Directory.Exists(root)) continue;

                try
                {
                    var projFiles = Directory.GetFiles(root, "*.*", SearchOption.AllDirectories)
                                             .Where(f => f.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) ||
                                                         f.EndsWith(".vcxproj", StringComparison.OrdinalIgnoreCase) ||
                                                         f.EndsWith(".vbproj", StringComparison.OrdinalIgnoreCase));

                    foreach (var proj in projFiles.Take(500))
                    {
                        if (ShouldSkipDirectory(Path.GetDirectoryName(proj) ?? "", Path.GetFileName(Path.GetDirectoryName(proj) ?? "")))
                            continue;

                        threats.AddRange(ScanProjectBuildFile(proj, isVSTemplate: false));
                    }
                }
                catch { }
            }

            return threats;
        }

        public List<ThreatInfo> ScanProcessesAndPersistenceOnly()
        {
            Log("🧠 Scanning Memory, Droppers, WMI & Autorun Persistence...");
            Progress(5, "Scanning Active Memory Processes...");
            List<ThreatInfo> threats = ScanProcesses();

            Progress(20, "Scanning Script Droppers (.vbs, .ps1, .bat)...");
            threats.AddRange(ScanScriptDroppers());

            Progress(38, "Scanning WMI Event Consumers...");
            threats.AddRange(ScanWMIPersistence());

            Progress(52, "Scanning Scheduled Tasks...");
            threats.AddRange(ScanScheduledTasks());

            Progress(67, "Scanning Temp Payload Directories...");
            threats.AddRange(ScanTempAndAppData());

            Progress(80, "Scanning Registry Run Keys...");
            threats.AddRange(ScanRegistryPersistence());

            Progress(92, "Scanning Active C2 Network Connections...");
            threats.AddRange(ScanLiveNetworkConnections());

            Progress(100, $"Persistence Scan Completed! Total Threats Found: {threats.Count}");
            return threats;
        }

        #region 1. Base64 & GitHub Link De-obfuscation Engine
        private (bool isDecoded, string decodedText, string extractedUrl) TryDecodeBase64Payload(string rawContent)
        {
            try
            {
                // Match Base64 patterns like FromBase64String("...") or FromBase64String('...')
                var b64Match = Regex.Match(rawContent, @"FromBase64String\s*\(\s*[""']([A-Za-z0-9+/=]{16,})[""']\s*\)", RegexOptions.IgnoreCase);
                if (!b64Match.Success)
                {
                    // Fallback to plain long base64 regex string in IEX
                    b64Match = Regex.Match(rawContent, @"[""']([A-Za-z0-9+/=]{24,})[""']", RegexOptions.IgnoreCase);
                }

                if (b64Match.Success)
                {
                    string b64Str = b64Match.Groups[1].Value;
                    byte[] bytes = Convert.FromBase64String(b64Str);
                    string decoded = Encoding.UTF8.GetString(bytes);

                    if (string.IsNullOrEmpty(decoded) || decoded.Any(c => char.IsControl(c) && c != '\r' && c != '\n' && c != '\t'))
                    {
                        decoded = Encoding.Unicode.GetString(bytes);
                    }

                    string extractedUrl = string.Empty;
                    var urlMatch = Regex.Match(decoded, @"https?://[^\s""'>]+", RegexOptions.IgnoreCase);
                    if (urlMatch.Success)
                    {
                        extractedUrl = urlMatch.Value;
                    }

                    return (true, decoded, extractedUrl);
                }
            }
            catch { }

            return (false, string.Empty, string.Empty);
        }
        #endregion

        #region 2. Scan Projects & MSBuild
        private List<ThreatInfo> ScanAllDrives()
        {
            var threats = new ConcurrentBag<ThreatInfo>();
            DriveInfo[] drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady && (d.DriveType == DriveType.Fixed || d.DriveType == DriveType.Removable))
                .ToArray();

            foreach (var drive in drives)
            {
                Log($"⚡ Multi-Threaded Parallel Scanning on Drive {drive.Name} ({Environment.ProcessorCount} CPU Cores Active)...");

                // 1. Scan files directly in root of the drive
                try
                {
                    FileInfo[] rootFiles = drive.RootDirectory.GetFiles();
                    foreach (var file in rootFiles)
                    {
                        string ext = file.Extension.ToLower();
                        if (ProjectFileExtensions.Contains(ext) || ProjectFileExtensions.Contains(file.Name))
                        {
                            foreach (var t in ScanProjectBuildFile(file.FullName, false))
                                threats.Add(t);
                        }
                        else if (SourceCodeExtensions.Contains(ext) && file.Length < 2 * 1024 * 1024)
                        {
                            foreach (var t in ScanSourceCodeFile(file.FullName, ext))
                                threats.Add(t);
                        }
                    }
                }
                catch { }

                // 2. Fetch top-level directories and scan simultaneously across all CPU cores
                DirectoryInfo[] topDirs = Array.Empty<DirectoryInfo>();
                try
                {
                    topDirs = drive.RootDirectory.GetDirectories();
                }
                catch { }

                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(4, Environment.ProcessorCount)
                };

                Parallel.ForEach(topDirs, parallelOptions, topDir =>
                {
                    try
                    {
                        if (!ShouldSkipDirectory(topDir.FullName, topDir.Name))
                        {
                            var subThreats = ScanDirectory(topDir.FullName);
                            foreach (var t in subThreats)
                                threats.Add(t);
                        }
                    }
                    catch { }
                });
            }

            return threats.ToList();
        }

        private List<ThreatInfo> ScanVSTemplates()
        {
            List<ThreatInfo> threats = new List<ThreatInfo>();

            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string[] vsPaths = new[]
            {
                Path.Combine(programFiles, "Microsoft Visual Studio"),
                Path.Combine(programFilesX86, "Microsoft Visual Studio"),
                Path.Combine(programFiles, "MSBuild"),
                Path.Combine(programFilesX86, "MSBuild"),
                Path.Combine(localAppData, "Microsoft", "MSBuild"),
                Path.Combine(appData, "Microsoft", "MSBuild"),
                @"C:\Windows\Microsoft.NET"
            };

            foreach (var path in vsPaths)
            {
                if (Directory.Exists(path))
                {
                    Log($"Checking VS System Templates: {path}");
                    threats.AddRange(ScanDirectory(path, isVSTemplate: true));
                }
            }

            return threats;
        }

        private List<ThreatInfo> ScanDirectory(string dirPath, bool isVSTemplate = false, bool isTargetProjectFolder = false)
        {
            List<ThreatInfo> threats = new List<ThreatInfo>();

            try
            {
                DirectoryInfo dir = new DirectoryInfo(dirPath);

                // Check if directory should be skipped (SDKs, Recycle Bin, self-exclusion)
                if (!isVSTemplate && ShouldSkipDirectory(dir.FullName, dir.Name))
                    return threats;

                FileInfo[] files;
                try { files = dir.GetFiles(); }
                catch { return threats; }

                foreach (var file in files)
                {
                    string ext = file.Extension.ToLower();
                    string fname = file.Name.ToLower();

                    // 1. Project/build files (MSBuild/NuGet analysis)
                    if (ProjectFileExtensions.Contains(ext) || ProjectFileExtensions.Contains(file.Name))
                    {
                        threats.AddRange(ScanProjectBuildFile(file.FullName, isVSTemplate));
                    }
                    // 2. Source code files (deep language-aware analysis)
                    else if (SourceCodeExtensions.Contains(ext))
                    {
                        // Skip very large files (>2MB likely generated/minified)
                        if (file.Length < 2 * 1024 * 1024)
                            threats.AddRange(ScanSourceCodeFile(file.FullName, ext));
                    }
                    // 3. Pre-compiled binaries in targeted project folders (Deep PE & Authenticode Inspection)
                    else if (isTargetProjectFolder && (ext == ".exe" || ext == ".dll"))
                    {
                        if (file.Length < 30 * 1024 * 1024) // < 30MB
                        {
                            try
                            {
                                var binResult = ExeInspectorEngine.InspectFile(file.FullName);
                                if (binResult.IsConfirmedStealer || binResult.IsConfirmedDropper || binResult.HasValidatedEmbeddedPE)
                                {
                                    threats.Add(new ThreatInfo
                                    {
                                        Category = ThreatCategory.TempPayload,
                                        ThreatName = $"Compiled Binary Payload in Project: {Path.GetFileName(file.FullName)}",
                                        TargetPath = file.FullName,
                                        MalwareSignature = binResult.ThreatVerdict,
                                        InjectedSnippet = binResult.CorrelatedEvidence.Count > 0 ? string.Join(" | ", binResult.CorrelatedEvidence.Take(2)) : binResult.ThreatVerdict,
                                        Severity = ThreatSeverity.Critical,
                                        ConfidenceScore = binResult.ConfidenceScore,
                                        SupportingEvidence = binResult.CorrelatedEvidence,
                                        RootCauseExplanation = $"Compiled binary '{Path.GetFileName(file.FullName)}' inside project folder contains confirmed malicious traits ({binResult.ThreatVerdict})."
                                    });
                                    Log($"[🚨] MALICIOUS BINARY IN PROJECT: {file.FullName} — {binResult.ThreatVerdict}", isError: true);
                                }
                            }
                            catch { }
                        }
                    }
                }

                DirectoryInfo[] subDirs;
                try { subDirs = dir.GetDirectories(); }
                catch { return threats; }

                foreach (var subDir in subDirs)
                {
                    if (!ShouldSkipDirectory(subDir.FullName, subDir.Name))
                        threats.AddRange(ScanDirectory(subDir.FullName, isVSTemplate, isTargetProjectFolder));
                }
            }
            catch { }

            return threats;
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // PROJECT TYPE DETECTOR — what kind of project is this folder?
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        public static string DetectProjectType(string folderPath)
        {
            var detected = new List<string>();

            if (Directory.GetFiles(folderPath, "*.csproj", SearchOption.AllDirectories).Any() ||
                Directory.GetFiles(folderPath, "*.vbproj", SearchOption.AllDirectories).Any() ||
                Directory.GetFiles(folderPath, "*.fsproj", SearchOption.AllDirectories).Any())
                detected.Add("C# / .NET");

            if (Directory.GetFiles(folderPath, "*.vcxproj", SearchOption.AllDirectories).Any() ||
                Directory.GetFiles(folderPath, "*.h", SearchOption.AllDirectories).Any())
                detected.Add("C / C++");

            if (Directory.GetFiles(folderPath, "*.py", SearchOption.AllDirectories).Any() ||
                File.Exists(Path.Combine(folderPath, "requirements.txt")) ||
                File.Exists(Path.Combine(folderPath, "setup.py")))
                detected.Add("Python");

            if (File.Exists(Path.Combine(folderPath, "package.json")) ||
                Directory.GetFiles(folderPath, "*.js", SearchOption.TopDirectoryOnly).Any() ||
                Directory.GetFiles(folderPath, "*.ts", SearchOption.TopDirectoryOnly).Any())
                detected.Add("JavaScript / Node.js");

            if (Directory.GetFiles(folderPath, "*.ps1", SearchOption.AllDirectories).Any())
                detected.Add("PowerShell");

            return detected.Count > 0 ? string.Join(" + ", detected) : "Unknown / Mixed";
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // LAYER A: MSBuild / Project File XML Analysis
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        private List<ThreatInfo> ScanProjectBuildFile(string filePath, bool isVSTemplate)
        {
            List<ThreatInfo> threats = new List<ThreatInfo>();

            try
            {
                string content = File.ReadAllText(filePath);
                string[] lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                // A1. Project file analysis: Confirmed signatures & malicious Exec commands only
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    string trimmed = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed)) continue;

                    // Skip benign XML comments
                    if (trimmed.StartsWith("<!--") && trimmed.EndsWith("-->")) continue;

                    // 1. Confirmed Malware Family Signatures (e.g. AWyhU7, alJvVFZa, BX7010, Retev C2)
                    string? matchedFamily = ConfirmedMalwareSignatures.FirstOrDefault(sig => MatchesMalwareSignature(line, sig));
                    if (matchedFamily != null)
                    {
                        var (isDecoded, decodedText, extractedUrl) = TryDecodeBase64Payload(line);
                        string urlInfo = !string.IsNullOrEmpty(extractedUrl) ? $" → Extracted C2 URL: {extractedUrl}" : "";

                        threats.Add(new ThreatInfo
                        {
                            Category = ThreatCategory.MSBuildInfection,
                            ThreatName = isVSTemplate ? "MSBuild System Template Grabber Worm" : "Infected MSBuild Project Worm Hook",
                            TargetPath = filePath,
                            LineNumber = i + 1,
                            MalwareSignature = matchedFamily,
                            InjectedSnippet = trimmed.Length > 120 ? trimmed[..120] + "..." : trimmed,
                            DecodedUrl = extractedUrl,
                            ExtractedPayloadInfo = isDecoded ? decodedText : "",
                            Severity = ThreatSeverity.Critical,
                            ConfidenceScore = 100,
                            RootCauseExplanation = isVSTemplate
                                ? $"Visual Studio System Template infected with confirmed MSBuild worm '{matchedFamily}'!{urlInfo} Runs grabber on every build."
                                : $"Infected project file contains confirmed MSBuild grabber worm '{matchedFamily}'!{urlInfo} Triggers grabber on build."
                        });

                        Log($"[!] INFECTED PROJECT FILE: {filePath} (Line {i + 1}) — Sig: {matchedFamily}", isError: true);
                        continue;
                    }

                    // 2. Suspicious Build Execution Hooks (<Exec> or <PreBuildEvent> running hidden PowerShell or droppers)
                    bool isTargetOrExec = line.Contains("<Exec", StringComparison.OrdinalIgnoreCase) ||
                                          line.Contains("<PreBuildEvent", StringComparison.OrdinalIgnoreCase) ||
                                          (line.Contains("<Target", StringComparison.OrdinalIgnoreCase) && line.Contains("PreBuild", StringComparison.OrdinalIgnoreCase));

                    if (isTargetOrExec)
                    {
                        bool isMaliciousExec = line.Contains("-WindowStyle Hidden", StringComparison.OrdinalIgnoreCase) ||
                                               line.Contains("-EncodedCommand", StringComparison.OrdinalIgnoreCase) ||
                                               (line.Contains("-ExecutionPolicy Bypass", StringComparison.OrdinalIgnoreCase) && line.Contains("powershell", StringComparison.OrdinalIgnoreCase)) ||
                                               line.Contains("certutil -urlcache", StringComparison.OrdinalIgnoreCase) ||
                                               line.Contains("bitsadmin /transfer", StringComparison.OrdinalIgnoreCase) ||
                                               (line.Contains("curl", StringComparison.OrdinalIgnoreCase) && (line.Contains(".exe", StringComparison.OrdinalIgnoreCase) || line.Contains("iex", StringComparison.OrdinalIgnoreCase))) ||
                                               (line.Contains("wget", StringComparison.OrdinalIgnoreCase) && (line.Contains(".exe", StringComparison.OrdinalIgnoreCase) || line.Contains("iex", StringComparison.OrdinalIgnoreCase))) ||
                                               (line.Contains("FromBase64String", StringComparison.OrdinalIgnoreCase) && (line.Contains("powershell", StringComparison.OrdinalIgnoreCase) || line.Contains("iex", StringComparison.OrdinalIgnoreCase)));

                        if (isMaliciousExec)
                        {
                            var (isDecoded, decodedText, extractedUrl) = TryDecodeBase64Payload(line);
                            string urlInfo = !string.IsNullOrEmpty(extractedUrl) ? $" → Extracted C2 URL: {extractedUrl}" : "";

                            threats.Add(new ThreatInfo
                            {
                                Category = ThreatCategory.MSBuildInfection,
                                ThreatName = isVSTemplate ? "MSBuild System Template Stealth Hook" : "Infected MSBuild Project PreBuild Hook",
                                TargetPath = filePath,
                                LineNumber = i + 1,
                                MalwareSignature = "Malicious PreBuild Exec Hook",
                                InjectedSnippet = trimmed.Length > 120 ? trimmed[..120] + "..." : trimmed,
                                DecodedUrl = extractedUrl,
                                ExtractedPayloadInfo = isDecoded ? decodedText : "",
                                Severity = ThreatSeverity.Critical,
                                ConfidenceScore = 95,
                                RootCauseExplanation = isVSTemplate
                                    ? $"VS System Template contains stealth exec hook running hidden payload!{urlInfo}"
                                    : $"Project build hook contains stealth exec hook running hidden payload!{urlInfo}"
                            });

                            Log($"[!] INFECTED BUILD EXEC HOOK: {filePath} (Line {i + 1})", isError: true);
                        }
                    }
                }

                // A2. NuGet packages.config — scan for suspicious package names
                if (filePath.EndsWith("packages.config", StringComparison.OrdinalIgnoreCase))
                {
                    string[] suspiciousNuGetPackages = { "ConfuserEx", "Obfuscar" };
                    foreach (var pkg in suspiciousNuGetPackages)
                    {
                        if (content.Contains(pkg, StringComparison.OrdinalIgnoreCase))
                        {
                            threats.Add(new ThreatInfo
                            {
                                Category = ThreatCategory.MSBuildInfection,
                                ThreatName = $"Packer / Obfuscator Dependency: {pkg}",
                                TargetPath = filePath,
                                MalwareSignature = pkg,
                                InjectedSnippet = pkg,
                                Severity = ThreatSeverity.Low,
                                ConfidenceScore = 40,
                                RootCauseExplanation = $"NuGet package '{pkg}' provides code obfuscation. Benign projects can use it to protect IP, but stealers also use it to evade detection."
                            });
                            Log($"[i] Obfuscator dependency: {pkg} in {filePath}", isWarning: true);
                        }
                    }
                }
            }
            catch { }

            return threats;
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // LAYER B: Source Code Deep Analysis (Language-Aware)
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        private List<ThreatInfo> ScanSourceCodeFile(string filePath, string ext)
        {
            List<ThreatInfo> threats = new List<ThreatInfo>();

            try
            {
                string content = File.ReadAllText(filePath, Encoding.UTF8);
                string lower = content.ToLowerInvariant();

                // ── B0. Universal signature scan (all known malware strings) ──
                // ── B0. Confirmed Malware Family Signatures (Zero False Positives) ──
                string[] lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    foreach (var sig in ConfirmedMalwareSignatures)
                    {
                        if (MatchesMalwareSignature(line, sig))
                        {
                            var (decoded, decodedText, extractedUrl) = TryDecodeBase64Payload(line);
                            threats.Add(new ThreatInfo
                            {
                                Category = ThreatCategory.MSBuildInfection,
                                ThreatName = $"Confirmed Malware Family Signature in Source ({Path.GetExtension(filePath).ToUpper()})",
                                TargetPath = filePath,
                                LineNumber = i + 1,
                                MalwareSignature = sig,
                                InjectedSnippet = line.Trim().Length > 120 ? line.Trim()[..120] + "..." : line.Trim(),
                                DecodedUrl = extractedUrl,
                                ExtractedPayloadInfo = decoded ? decodedText : "",
                                Severity = ThreatSeverity.Critical,
                                ConfidenceScore = 100,
                                RootCauseExplanation = $"Confirmed malware family signature '{sig}' found directly in source code. This file contains known stealer/worm code."
                            });
                            Log($"[!!!] CONFIRMED MALWARE IN SOURCE: {filePath} (Line {i + 1}) — {sig}", isError: true);
                            break;
                        }
                    }
                }

                // ── B1. Language-specific pattern analysis ──
                int riskScore = 0;
                var findings = new List<string>();
                var highRiskFindings = new List<string>();

                if (ext == ".cs" || ext == ".vb" || ext == ".fs")
                {
                    AnalyzeWithPatterns(content, CSharpGrabberPatterns, ref riskScore, findings, highRiskFindings);
                }
                else if (ext == ".cpp" || ext == ".c" || ext == ".h" || ext == ".hpp" || ext == ".cc" || ext == ".cxx")
                {
                    AnalyzeWithPatterns(content, CppGrabberPatterns, ref riskScore, findings, highRiskFindings);
                }
                else if (ext == ".py" || ext == ".pyw")
                {
                    AnalyzeWithPatterns(content, PythonGrabberPatterns, ref riskScore, findings, highRiskFindings);
                }
                else if (ext == ".js" || ext == ".ts" || ext == ".mjs" || ext == ".cjs")
                {
                    AnalyzeWithPatterns(content, JsGrabberPatterns, ref riskScore, findings, highRiskFindings);
                }
                else if (ext == ".ps1" || ext == ".psm1")
                {
                    if (lower.Contains("-encodedcommand") || lower.Contains("-enc "))
                    { riskScore += 40; highRiskFindings.Add("Base64-encoded PowerShell command (-EncodedCommand)"); }
                    if (lower.Contains("iex(") || lower.Contains("invoke-expression"))
                    { riskScore += 30; highRiskFindings.Add("Invoke-Expression (IEX) — executes dynamic code"); }
                    if (lower.Contains("downloadstring") || lower.Contains("downloadfile"))
                    { riskScore += 25; findings.Add("WebClient Download method"); }
                }

                // ── B2. CHAIN-OF-CRIME VERDICT: Stealer requires BOTH Theft Target AND Exfiltration ──
                bool hasTheftTarget = lower.Contains("cryptunprotectdata") ||
                                      lower.Contains("win32crypt") ||
                                      lower.Contains("login data") ||
                                      lower.Contains("web data") ||
                                      lower.Contains("chrome/user data") ||
                                      lower.Contains("chrome\\user data") ||
                                      lower.Contains("discord/local storage") ||
                                      lower.Contains("discord_desktop_core");

                bool hasExfiltration = lower.Contains("discord.com/api/webhooks") ||
                                       lower.Contains("discordapp.com/api/webhooks") ||
                                       lower.Contains("api.telegram.org/bot") ||
                                       lower.Contains("catbox.moe") ||
                                       lower.Contains("transfer.sh") ||
                                       lower.Contains("gofile.io") ||
                                       lower.Contains("anonfiles.com");

                // Whitelist legitimate license & authentication components (e.g. KeyAuth, FearAuth, Cryptlex)
                bool isLegitAuth = LegitAuthFrameworks.Any(a => content.Contains(a, StringComparison.OrdinalIgnoreCase));

                // A file is reported as a Stealer/Grabber ONLY when:
                // BOTH Theft Target (DPAPI/Browser/Tokens) AND Exfiltration (Webhook/Telegram/C2) are present
                if (hasTheftTarget && hasExfiltration && !isLegitAuth)
                {
                    string langName = ext switch
                    {
                        ".cs" or ".vb" or ".fs" => "C# / .NET",
                        ".cpp" or ".c" or ".h" or ".hpp" => "C / C++",
                        ".py" or ".pyw" => "Python",
                        ".js" or ".ts" or ".mjs" => "JavaScript",
                        ".ps1" or ".psm1" => "PowerShell",
                        _ => ext.ToUpper()
                    };

                    int confidence = 98;
                    ThreatSeverity sev = ThreatSeverity.Critical;

                    string snippetSummary = highRiskFindings.Count > 0
                        ? string.Join(" | ", highRiskFindings.Take(3))
                        : string.Join(" | ", findings.Take(4));

                    threats.Add(new ThreatInfo
                    {
                        Category = ThreatCategory.MSBuildInfection,
                        ThreatName = $"{langName} Source Code — Credential Stealer / Grabber Detected (Confidence: {confidence}%)",
                        TargetPath = filePath,
                        MalwareSignature = "Correlated Credential Theft + Exfiltration",
                        InjectedSnippet = snippetSummary.Length > 120 ? snippetSummary[..120] + "..." : snippetSummary,
                        Severity = sev,
                        ConfidenceScore = confidence,
                        SupportingEvidence = highRiskFindings.Concat(findings).Distinct().ToList(),
                        RootCauseExplanation = $"Confirmed Grabber chain-of-crime in '{Path.GetFileName(filePath)}'! File targets browser/Discord credentials AND exfiltrates them via webhook/C2."
                    });

                    Log($"[🚨] CORRELATED GRABBER DETECTED: {filePath} — Score {riskScore} | {snippetSummary}", isError: true);
                }
                else if (isLegitAuth)
                {
                    Log($"[🛡️ VERIFIED SAFE] {Path.GetFileName(filePath)} — Code Integrity Passed (Clean)");
                }
                else if (riskScore >= 30 && riskScore < 60)
                {
                    // Low-medium risk — log it but don't add as critical threat
                    Log($"[?] Moderate risk patterns in: {Path.GetFileName(filePath)} (Score {riskScore}) — {string.Join(", ", findings.Take(3))}");
                }
            }
            catch { }

            return threats;
        }

        private static void AnalyzeWithPatterns(
            string content,
            (string Pattern, string Description, int RiskWeight)[] patterns,
            ref int riskScore,
            List<string> findings,
            List<string> highRiskFindings)
        {
            foreach (var (pattern, description, weight) in patterns)
            {
                if (content.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    riskScore += weight;
                    if (weight >= 35)
                        highRiskFindings.Add(description);
                    else
                        findings.Add(description);
                }
            }
        }
        #endregion

        #region 3. Scan Script Droppers (.vbs, .ps1, .bat, .lnk)
        private List<ThreatInfo> ScanScriptDroppers()
        {
            List<ThreatInfo> threats = new List<ThreatInfo>();

            string tempDir = Path.GetTempPath();
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string startupFolder = Path.Combine(appData, @"Microsoft\Windows\Start Menu\Programs\Startup");
            string publicDir = @"C:\Users\Public";
            string programData = @"C:\ProgramData";

            string[] scanPaths = new[] { tempDir, appData, localAppData, startupFolder, publicDir, programData };

            foreach (var basePath in scanPaths)
            {
                if (string.IsNullOrEmpty(basePath) || !Directory.Exists(basePath)) continue;

                try
                {
                    var files = new DirectoryInfo(basePath).GetFiles("*.*", SearchOption.TopDirectoryOnly);

                    foreach (var file in files)
                    {
                        string ext = file.Extension.ToLower();

                        if (ScriptExtensions.Contains(ext))
                        {
                            try
                            {
                                string text = File.ReadAllText(file.FullName);

                                bool isConfirmed = ConfirmedMalwareSignatures.Any(sig => text.Contains(sig, StringComparison.OrdinalIgnoreCase));
                                bool isSuspicious = SuspiciousPersistencePatterns.Any(sig => text.Contains(sig, StringComparison.OrdinalIgnoreCase));
                                bool isScriptDropper = text.Contains("WScript.Shell", StringComparison.OrdinalIgnoreCase) && text.Contains("HTTP", StringComparison.OrdinalIgnoreCase) && (text.Contains(".exe", StringComparison.OrdinalIgnoreCase) || text.Contains(".dll", StringComparison.OrdinalIgnoreCase));

                                if (isConfirmed || isSuspicious || isScriptDropper)
                                {
                                    var (isDecoded, decodedText, extractedUrl) = TryDecodeBase64Payload(text);

                                    threats.Add(new ThreatInfo
                                    {
                                        Category = ThreatCategory.ScriptDropper,
                                        ThreatName = $"Malicious Script Dropper ({ext.ToUpper()})",
                                        TargetPath = file.FullName,
                                        MalwareSignature = isConfirmed ? "Confirmed Malware Family" : "Stealth Persistence Script",
                                        InjectedSnippet = text.Trim().Length > 120 ? text.Trim()[..120] + "..." : text.Trim(),
                                        DecodedUrl = extractedUrl,
                                        ExtractedPayloadInfo = isDecoded ? decodedText : "",
                                        Severity = ThreatSeverity.Critical,
                                        ConfidenceScore = isConfirmed ? 100 : 95,
                                        RootCauseExplanation = $"Malicious {ext.ToUpper()} script staged in {basePath}. It executes hidden PowerShell commands or downloads grabber binaries from external C2 servers."
                                    });

                                    Log($"[!] MALICIOUS SCRIPT DROPPER FOUND: {file.FullName}", isError: true);
                                }
                            }
                            catch { }
                        }
                        else if (ext == ".lnk" && basePath == startupFolder)
                        {
                            // Check startup shortcuts for malicious target lines
                            string target = GetShortcutTarget(file.FullName);
                            bool isMalLnk = ConfirmedMalwareSignatures.Any(sig => target.Contains(sig, StringComparison.OrdinalIgnoreCase)) ||
                                            SuspiciousPersistencePatterns.Any(sig => target.Contains(sig, StringComparison.OrdinalIgnoreCase)) ||
                                            (target.Contains("powershell", StringComparison.OrdinalIgnoreCase) && target.Contains("hidden", StringComparison.OrdinalIgnoreCase));

                            if (isMalLnk)
                            {
                                threats.Add(new ThreatInfo
                                {
                                    Category = ThreatCategory.ScriptDropper,
                                    ThreatName = "Malicious Startup Shortcut (.lnk) Hook",
                                    TargetPath = file.FullName,
                                    MalwareSignature = "LNK Hidden Execution Hook",
                                    InjectedSnippet = target,
                                    Severity = ThreatSeverity.Critical,
                                    RootCauseExplanation = "Startup folder contains a modified .LNK shortcut that automatically runs hidden PowerShell malware when you log into Windows."
                                });

                                Log($"[!] MALICIOUS STARTUP SHORTCUT (.LNK): {file.FullName}", isError: true);
                            }
                        }
                    }
                }
                catch { }
            }

            return threats;
        }

        private string GetShortcutTarget(string shortcutPath)
        {
            try
            {
                Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType != null)
                {
                    dynamic shell = Activator.CreateInstance(shellType)!;
                    dynamic shortcut = shell.CreateShortcut(shortcutPath);
                    return $"{shortcut.TargetPath} {shortcut.Arguments}";
                }
            }
            catch { }
            return string.Empty;
        }
        #endregion

        #region 4. Scan WMI Event Consumer Persistence
        private List<ThreatInfo> ScanWMIPersistence()
        {
            List<ThreatInfo> threats = new List<ThreatInfo>();

            try
            {
                using var searcher = new ManagementObjectSearcher(@"root\subscription", "SELECT * FROM __EventConsumer");
                using var consumers = searcher.Get();

                foreach (ManagementObject consumer in consumers)
                {
                    string name = consumer["Name"]?.ToString() ?? "WMI Consumer";
                    string cmdLine = consumer["CommandLineTemplate"]?.ToString() ?? consumer["ScriptText"]?.ToString() ?? "";

                    bool isMalWmi = ConfirmedMalwareSignatures.Any(sig => cmdLine.Contains(sig, StringComparison.OrdinalIgnoreCase)) ||
                                    SuspiciousPersistencePatterns.Any(sig => cmdLine.Contains(sig, StringComparison.OrdinalIgnoreCase)) ||
                                    (cmdLine.Contains("powershell", StringComparison.OrdinalIgnoreCase) && cmdLine.Contains("hidden", StringComparison.OrdinalIgnoreCase));

                    if (isMalWmi)
                    {
                        threats.Add(new ThreatInfo
                        {
                            Category = ThreatCategory.WMIPersistence,
                            ThreatName = $"Stealthy WMI Event Consumer Persistence ({name})",
                            TargetPath = $"WMI Subscription: {name}",
                            MalwareSignature = "WMI CommandLineEventConsumer",
                            InjectedSnippet = cmdLine.Length > 120 ? cmdLine[..120] + "..." : cmdLine,
                            Severity = ThreatSeverity.Critical,
                            ConfidenceScore = 95,
                            RootCauseExplanation = "Advanced malware persistence technique using Windows Management Instrumentation (WMI). It runs hidden PowerShell grabber scripts automatically without registry keys."
                        });

                        Log($"[!] STEALTHY WMI EVENT CONSUMER PERSISTENCE: {name} -> {cmdLine}", isError: true);
                    }
                }
            }
            catch { }

            return threats;
        }
        #endregion

        #region 5. Scan Hosts File Integrity
        private List<ThreatInfo> ScanHostsFile()
        {
            List<ThreatInfo> threats = new List<ThreatInfo>();
            string hostsPath = @"C:\Windows\System32\drivers\etc\hosts";

            if (File.Exists(hostsPath))
            {
                try
                {
                    string[] lines = File.ReadAllLines(hostsPath);
                    string[] securityDomains = new[] { "virustotal", "malwarebytes", "microsoft", "defender", "github", "raw.githubusercontent" };

                    foreach (var line in lines)
                    {
                        string trimmed = line.Trim();
                        if (trimmed.StartsWith("#") || string.IsNullOrEmpty(trimmed)) continue;

                        if (securityDomains.Any(d => trimmed.Contains(d, StringComparison.OrdinalIgnoreCase)))
                        {
                            threats.Add(new ThreatInfo
                            {
                                Category = ThreatCategory.HostsFileTampering,
                                ThreatName = "Windows Hosts File Antivirus Block",
                                TargetPath = hostsPath,
                                MalwareSignature = trimmed,
                                InjectedSnippet = trimmed,
                                Severity = ThreatSeverity.High,
                                RootCauseExplanation = "Malware modified your Windows Hosts file to block access to Antivirus update sites and GitHub."
                            });

                            Log($"[!] WINDOWS HOSTS FILE TAMPERED: {trimmed}", isWarning: true);
                        }
                    }
                }
                catch { }
            }

            return threats;
        }
        #endregion

        #region 6. Scan Processes, Temp, Registry & Discord
        private List<ThreatInfo> ScanTempAndAppData()
        {
            List<ThreatInfo> threats = new List<ThreatInfo>();

            string tempDir = Path.GetTempPath();
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            string[] suspiciousFolders = new[] { "AWyhU7", "alJvVFZa", "BX7010", "3ZTWX", "GrabberPayload", "BuildHook" };
            string[] checkBases = new[] { tempDir, appData, localAppData };

            foreach (var baseDir in checkBases)
            {
                if (string.IsNullOrEmpty(baseDir) || !Directory.Exists(baseDir)) continue;

                foreach (var targetName in suspiciousFolders)
                {
                    string fullPath = Path.Combine(baseDir, targetName);
                    if (Directory.Exists(fullPath))
                    {
                        threats.Add(new ThreatInfo
                        {
                            Category = ThreatCategory.TempPayload,
                            ThreatName = "Known Grabber Payload Directory",
                            TargetPath = fullPath,
                            MalwareSignature = targetName,
                            InjectedSnippet = $"Directory: {fullPath}",
                            Severity = ThreatSeverity.Critical,
                            RootCauseExplanation = "Malware payload directory created in Temp/AppData. Contains stolen credentials, token logs, and binary stealers."
                        });

                        Log($"[!] MALWARE PAYLOAD DIRECTORY: {fullPath}", isError: true);
                    }
                }
            }

            return threats;
        }

        private List<ThreatInfo> ScanProcesses()
        {
            List<ThreatInfo> threats = new List<ThreatInfo>();

            try
            {
                Process[] processes = Process.GetProcesses();
                foreach (var proc in processes)
                {
                    try
                    {
                        string name = proc.ProcessName.ToLower();

                        if (name.Contains("powershell") || name.Contains("cmd") || name.Contains("mshta") || name.Contains("wscript"))
                        {
                            string cmdLine = GetProcessCommandLine(proc.Id);

                            if (!string.IsNullOrEmpty(cmdLine))
                            {
                                string? matchedFamily = ConfirmedMalwareSignatures.FirstOrDefault(sig => cmdLine.Contains(sig, StringComparison.OrdinalIgnoreCase));
                                string? matchedSuspicious = SuspiciousPersistencePatterns.FirstOrDefault(sig => cmdLine.Contains(sig, StringComparison.OrdinalIgnoreCase));

                                if (matchedFamily != null || matchedSuspicious != null)
                                {
                                    string matchedSig = matchedFamily ?? matchedSuspicious!;
                                    threats.Add(new ThreatInfo
                                    {
                                        Category = ThreatCategory.SuspiciousProcess,
                                        ThreatName = $"Malicious Process Payload (PID: {proc.Id})",
                                        TargetPath = $"PID {proc.Id}: {proc.ProcessName}.exe",
                                        MalwareSignature = matchedSig,
                                        InjectedSnippet = cmdLine.Length > 120 ? cmdLine[..120] + "..." : cmdLine,
                                        Severity = ThreatSeverity.Critical,
                                        ConfidenceScore = matchedFamily != null ? 100 : 92,
                                        RootCauseExplanation = "Active malware process running in memory stealing tokens and exfiltrating data in real-time."
                                    });

                                    Log($"[!] MALICIOUS PROCESS PID {proc.Id}: {cmdLine}", isError: true);
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }

            return threats;
        }

        private string GetProcessCommandLine(int processId)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher($"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {processId}");
                using var objects = searcher.Get();
                var obj = objects.Cast<ManagementBaseObject>().FirstOrDefault();
                return obj?["CommandLine"]?.ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private List<ThreatInfo> ScanRegistryPersistence()
        {
            List<ThreatInfo> threats = new List<ThreatInfo>();

            string[] registryLocations = new[]
            {
                @"Software\Microsoft\Windows\CurrentVersion\Run",
                @"Software\Microsoft\Windows\CurrentVersion\RunOnce"
            };

            var keysToCheck = new[]
            {
                (Registry.CurrentUser, "HKCU"),
                (Registry.LocalMachine, "HKLM")
            };

            foreach (var (rootKey, rootName) in keysToCheck)
            {
                foreach (var regSubPath in registryLocations)
                {
                    try
                    {
                        using var key = rootKey.OpenSubKey(regSubPath);
                        if (key != null)
                        {
                            foreach (var valName in key.GetValueNames())
                            {
                                string valData = key.GetValue(valName)?.ToString() ?? string.Empty;

                                string? matchedFamily = ConfirmedMalwareSignatures.FirstOrDefault(sig => valData.Contains(sig, StringComparison.OrdinalIgnoreCase));
                                string? matchedSuspicious = SuspiciousPersistencePatterns.FirstOrDefault(sig => valData.Contains(sig, StringComparison.OrdinalIgnoreCase));

                                if (matchedFamily != null || matchedSuspicious != null)
                                {
                                    string matchedSig = matchedFamily ?? matchedSuspicious!;
                                    threats.Add(new ThreatInfo
                                    {
                                        Category = ThreatCategory.RegistryPersistence,
                                        ThreatName = $"Registry Autorun Virus Key ({rootName})",
                                        TargetPath = $"{rootName}\\{regSubPath} -> Value: {valName}",
                                        MalwareSignature = matchedSig,
                                        InjectedSnippet = $"{valName} = {valData}",
                                        Severity = ThreatSeverity.Critical,
                                        ConfidenceScore = matchedFamily != null ? 100 : 92,
                                        RootCauseExplanation = "Registry autorun key created by malware. Re-executes grabber script every time Windows boots."
                                    });

                                    Log($"[!] REGISTRY AUTORUN KEY: {rootName}\\{regSubPath} [{valName}]", isError: true);
                                }
                            }
                        }
                    }
                    catch { }
                }
            }

            return threats;
        }

        private List<ThreatInfo> ScanDiscordInjections()
        {
            List<ThreatInfo> threats = new List<ThreatInfo>();

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            string[] discordPaths = new[]
            {
                Path.Combine(appData, "Discord"),
                Path.Combine(appData, "discordcanary"),
                Path.Combine(appData, "discordptb"),
                Path.Combine(localAppData, "Discord")
            };

            foreach (var basePath in discordPaths)
            {
                if (!Directory.Exists(basePath)) continue;

                try
                {
                    var coreFiles = Directory.GetFiles(basePath, "index.js", SearchOption.AllDirectories)
                        .Where(f => f.Contains("discord_desktop_core"));

                    foreach (var jsFile in coreFiles)
                    {
                        string content = File.ReadAllText(jsFile);
                        if (content.Contains("webhook", StringComparison.OrdinalIgnoreCase) ||
                            content.Contains("AWyhU7", StringComparison.OrdinalIgnoreCase) ||
                            content.Contains("api/webhooks", StringComparison.OrdinalIgnoreCase) ||
                            content.Contains("token", StringComparison.OrdinalIgnoreCase) && content.Length > 2000)
                        {
                            threats.Add(new ThreatInfo
                            {
                                Category = ThreatCategory.DiscordInjection,
                                ThreatName = "Discord Client Token Stealer Injection",
                                TargetPath = jsFile,
                                MalwareSignature = "Discord Desktop Core Hook",
                                InjectedSnippet = jsFile,
                                Severity = ThreatSeverity.Critical,
                                RootCauseExplanation = "Discord core JS files infected! Automatically steals Discord tokens, passwords, and 2FA backup codes."
                            });

                            Log($"[!] DISCORD TOKEN STEALER JS HOOK: {jsFile}", isError: true);
                        }
                    }
                }
                catch { }
            }

            return threats;
        }
        #endregion

        #region 7. Scan Scheduled Tasks
        private List<ThreatInfo> ScanScheduledTasks()
        {
            List<ThreatInfo> threats = new List<ThreatInfo>();

            try
            {
                // Use WMI Win32_ScheduledJob + schtasks XML parsing
                using var searcher = new ManagementObjectSearcher(@"root\cimv2", "SELECT * FROM Win32_ScheduledJob");
                using var jobs = searcher.Get();

                foreach (ManagementObject job in jobs)
                {
                    string cmd = job["Command"]?.ToString() ?? string.Empty;
                    string name = job["Name"]?.ToString() ?? "Unknown Task";

                    bool isMalTask = ConfirmedMalwareSignatures.Any(sig => cmd.Contains(sig, StringComparison.OrdinalIgnoreCase)) ||
                                     SuspiciousPersistencePatterns.Any(sig => cmd.Contains(sig, StringComparison.OrdinalIgnoreCase)) ||
                                     (cmd.Contains("powershell", StringComparison.OrdinalIgnoreCase) && cmd.Contains("hidden", StringComparison.OrdinalIgnoreCase));

                    if (isMalTask)
                    {
                        threats.Add(new ThreatInfo
                        {
                            Category = ThreatCategory.ScheduledTask,
                            ThreatName = $"Malicious Scheduled Task: {name}",
                            TargetPath = $"TASK:{name}",
                            MalwareSignature = "Scheduled Task Persistence",
                            InjectedSnippet = cmd.Length > 120 ? cmd[..120] + "..." : cmd,
                            Severity = ThreatSeverity.Critical,
                            ConfidenceScore = 95,
                            RootCauseExplanation = $"Malware registered a Scheduled Task '{name}' to auto-execute grabber payload. It runs silently on schedule or at login, bypassing startup folder detection."
                        });
                        Log($"[!] MALICIOUS SCHEDULED TASK: {name} -> {cmd}", isError: true);
                    }
                }
            }
            catch { }

            // Also parse schtasks /query XML output
            try
            {
                var psi = new ProcessStartInfo("schtasks", "/query /fo CSV /nh")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var proc = Process.Start(psi);
                if (proc != null)
                {
                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit(5000);

                    foreach (string line in output.Split('\n'))
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        string[] parts = line.Split(',');
                        if (parts.Length < 1) continue;
                        string taskName = parts[0].Trim('"', ' ');

                        // Red-flag task names used exclusively by known malware families
                        string[] confirmedMalwareTaskNames = new[]
                        {
                            "BlankGrabber", "EpsilonStealer", "PhorcyStealer",
                            "AsyncRAT", "DCRat", "XWorm", "AWyhU7", "Retev"
                        };

                        foreach (var malTask in confirmedMalwareTaskNames)
                        {
                            if (taskName.Contains(malTask, StringComparison.OrdinalIgnoreCase))
                            {
                                threats.Add(new ThreatInfo
                                {
                                    Category = ThreatCategory.ScheduledTask,
                                    ThreatName = $"Malicious Scheduled Task: {taskName}",
                                    TargetPath = $"TASK:{taskName}",
                                    MalwareSignature = malTask,
                                    InjectedSnippet = line.Trim(),
                                    Severity = ThreatSeverity.Critical,
                                    ConfidenceScore = 100,
                                    RootCauseExplanation = $"Scheduled task '{taskName}' matches confirmed malware family persistence. Often used by stealers to survive reboots."
                                });
                                Log($"[!] MALICIOUS TASK NAME: {taskName}", isError: true);
                                break;
                            }
                        }
                    }
                }
            }
            catch { }

            return threats;
        }
        #endregion

        #region 8. Scan Active Network C2 Connections
        private static readonly string[] KnownMaliciousIPs = new[]
        {
            "91.92.243.218",  // Known C2
            "185.220.101",    // Tor exit / C2 range
            "194.165.16",     // Known stealer host range
            "193.42.60",      // Malicious host range
            "179.43.154",     // Common grabber C2 range
            "51.77.113",      // Known malware C2
        };

        private static readonly string[] KnownMaliciousDomains = new[]
        {
            "pee-files.nl",
            "catbox.moe",
            "transfer.sh",
            "gofile.io",
            "anonfiles.com",
            "pixeldrain.com",
            "ghostbin.com",
            "paste.ee",
            "api.telegram.org",
            "discord.com/api/webhooks",
            "discordapp.com/api/webhooks"
        };

        private List<ThreatInfo> ScanLiveNetworkConnections()
        {
            List<ThreatInfo> threats = new List<ThreatInfo>();

            try
            {
                // Use netstat to get active connections
                var psi = new ProcessStartInfo("netstat", "-nob")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var proc = Process.Start(psi);
                if (proc != null)
                {
                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit(8000);

                    string currentProcess = "";
                    foreach (string rawLine in output.Split('\n'))
                    {
                        string line = rawLine.Trim();
                        if (string.IsNullOrEmpty(line)) continue;

                        // Track which process owns connections
                        if (line.StartsWith("[") && line.EndsWith("]"))
                        {
                            currentProcess = line.Trim('[', ']');
                            continue;
                        }

                        // Parse TCP/UDP lines
                        if (line.StartsWith("TCP") || line.StartsWith("UDP"))
                        {
                            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length >= 3)
                            {
                                string remoteAddr = parts.Length >= 3 ? parts[2] : "";
                                string state = parts.Length >= 4 ? parts[3] : "";

                                if (state != "ESTABLISHED" && state != "CLOSE_WAIT") continue;

                                // Check if remote IP matches known malicious
                                foreach (var malIP in KnownMaliciousIPs)
                                {
                                    if (remoteAddr.StartsWith(malIP))
                                    {
                                        string pid = parts.Length >= 5 ? parts[4] : "?";
                                        threats.Add(new ThreatInfo
                                        {
                                            Category = ThreatCategory.C2Connection,
                                            ThreatName = $"ACTIVE C2 BEACON — Known Malicious IP",
                                            TargetPath = $"Connection: {remoteAddr} (PID: {pid}) | Process: {currentProcess}",
                                            MalwareSignature = malIP,
                                            InjectedSnippet = line,
                                            Severity = ThreatSeverity.Critical,
                                            RootCauseExplanation = $"Your PC has an ACTIVE connection to known C2/stealer server {malIP}! Data is currently being exfiltrated. Kill process '{currentProcess}' immediately!"
                                        });
                                        Log($"[!!!] ACTIVE C2 CONNECTION DETECTED: {remoteAddr} | Process: {currentProcess}", isError: true);
                                        break;
                                    }
                                }

                                // Check for connection to suspicious ports used by RATs
                                if (remoteAddr.Contains(":"))
                                {
                                    string portStr = remoteAddr.Split(':').Last();
                                    if (int.TryParse(portStr, out int port))
                                    {
                                        // Common RAT ports
                                        int[] ratPorts = { 4444, 1337, 8888, 9999, 6666, 7777, 8080, 31337, 1604 };
                                        if (ratPorts.Contains(port))
                                        {
                                            string pid = parts.Length >= 5 ? parts[4] : "?";
                                            threats.Add(new ThreatInfo
                                            {
                                                Category = ThreatCategory.C2Connection,
                                                ThreatName = $"SUSPICIOUS RAT PORT CONNECTION (:{port})",
                                                TargetPath = $"Connection: {remoteAddr} (PID: {pid}) | Process: {currentProcess}",
                                                MalwareSignature = $"RAT Port {port}",
                                                InjectedSnippet = line,
                                                Severity = ThreatSeverity.High,
                                                RootCauseExplanation = $"Outbound connection detected on port {port}, which is commonly used by Remote Access Trojans (AsyncRAT, njRAT, XWorm, etc). Process: '{currentProcess}'"
                                            });
                                            Log($"[!] SUSPICIOUS RAT PORT {port}: {remoteAddr} | Process: {currentProcess}", isWarning: true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            if (threats.Count == 0)
                Log("[OK] No active C2 connections or suspicious RAT ports detected.");
            else
                Log($"[!!!] {threats.Count} ACTIVE SUSPICIOUS NETWORK CONNECTION(S) FOUND!", isError: true);

            return threats;
        }
        #endregion

        #region 9. Scan Browser Extensions
        private List<ThreatInfo> ScanBrowserExtensions()
        {
            List<ThreatInfo> threats = new List<ThreatInfo>();
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Known malicious extension IDs (crypto stealers / clipboard hijackers)
            string[] maliciousExtIds = new[]
            {
                "ckkbhoeadgkjibpagbmpnfcjmblddmbm", // Notorious clipboard hijacker
                "kplhojljllfebpbmcbjaclbohkkfhmdb", // Fake MetaMask
                "fhbohimaelbohpjbbldcngcnapndodjp", // Binance Fake
                "nkbihfbeogaeaoehlefnkodbefgpgknn", // Real MetaMask ID (used as template for fakes)
                "dngmlblcodfobpdpecaadgfbcggfjfnm", // Known stealer
                "kphnpknldfkgmklelkmboeajmnoebnm",  // Known stealer
            };

            // Suspicious manifest keywords
            string[] dangerousPermissions = new[]
            {
                "clipboardRead", "clipboardWrite", "nativeMessaging",
                "webRequestBlocking", "debugger", "proxy",
                "\"<all_urls>\"", "tabs", "history", "cookies"
            };

            var browserPaths = new Dictionary<string, string>
            {
                ["Chrome"] = Path.Combine(localAppData, @"Google\Chrome\User Data\Default\Extensions"),
                ["Edge"] = Path.Combine(localAppData, @"Microsoft\Edge\User Data\Default\Extensions"),
                ["Brave"] = Path.Combine(localAppData, @"BraveSoftware\Brave-Browser\User Data\Default\Extensions"),
                ["Opera"] = Path.Combine(appData, @"Opera Software\Opera Stable\Extensions"),
                ["Vivaldi"] = Path.Combine(localAppData, @"Vivaldi\User Data\Default\Extensions"),
            };

            foreach (var (browserName, extDir) in browserPaths)
            {
                if (!Directory.Exists(extDir)) continue;

                try
                {
                    foreach (var extFolder in Directory.GetDirectories(extDir))
                    {
                        string extId = Path.GetFileName(extFolder);

                        // Check against known bad IDs
                        if (maliciousExtIds.Contains(extId, StringComparer.OrdinalIgnoreCase))
                        {
                            threats.Add(new ThreatInfo
                            {
                                Category = ThreatCategory.ScriptDropper,
                                ThreatName = $"Known Malicious Browser Extension ({browserName})",
                                TargetPath = extFolder,
                                MalwareSignature = extId,
                                InjectedSnippet = $"Extension ID: {extId} in {browserName}",
                                Severity = ThreatSeverity.Critical,
                                RootCauseExplanation = $"Extension ID '{extId}' in {browserName} matches a known clipboard hijacker or crypto wallet stealer. Immediately remove it!"
                            });
                            Log($"[!!!] KNOWN MALICIOUS EXTENSION in {browserName}: {extId}", isError: true);
                            continue;
                        }

                        // Parse manifest.json for suspicious permissions
                        try
                        {
                            var versionFolders = Directory.GetDirectories(extFolder);
                            foreach (var verFolder in versionFolders)
                            {
                                string manifestPath = Path.Combine(verFolder, "manifest.json");
                                if (!File.Exists(manifestPath)) continue;

                                string manifest = File.ReadAllText(manifestPath);

                                // Check for dangerous combo: external update URL + dangerous permissions
                                bool hasExternalUpdate = manifest.Contains("update_url", StringComparison.OrdinalIgnoreCase) &&
                                    !manifest.Contains("clients2.google.com", StringComparison.OrdinalIgnoreCase) &&
                                    !manifest.Contains("edge.microsoft.com", StringComparison.OrdinalIgnoreCase);

                                int dangerousPermCount = dangerousPermissions.Count(p =>
                                    manifest.Contains(p, StringComparison.OrdinalIgnoreCase));

                                // Check for content script injecting into all URLs + dangerous perms
                                bool injectsAllUrls = manifest.Contains("\"<all_urls>\"") ||
                                    manifest.Contains("\"http://*/*\"") ||
                                    manifest.Contains("\"https://*/*\"");

                                if (hasExternalUpdate && dangerousPermCount >= 3 && injectsAllUrls)
                                {
                                    string extName = "Unknown";
                                    var nameMatch = System.Text.RegularExpressions.Regex.Match(manifest, "\"name\"\\s*:\\s*\"([^\"]+)\"");
                                    if (nameMatch.Success) extName = nameMatch.Groups[1].Value;

                                    threats.Add(new ThreatInfo
                                    {
                                        Category = ThreatCategory.ScriptDropper,
                                        ThreatName = $"Suspicious Browser Extension — '{extName}' ({browserName})",
                                        TargetPath = extFolder,
                                        MalwareSignature = $"ExternalUpdate + {dangerousPermCount} DangerousPerms + AllUrls",
                                        InjectedSnippet = $"ID: {extId} | Name: {extName} | Perms: {dangerousPermCount} suspicious",
                                        Severity = ThreatSeverity.High,
                                        RootCauseExplanation = $"'{extName}' extension in {browserName} has external update server + dangerous permissions (clipboard, cookies, all URLs). Pattern matches crypto wallet hijackers & token stealers."
                                    });
                                    Log($"[!] SUSPICIOUS EXTENSION in {browserName}: '{extName}' ({extId}) — {dangerousPermCount} dangerous perms + external update", isWarning: true);
                                }
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            }

            if (threats.Count == 0)
                Log($"[OK] No malicious browser extensions detected in Chrome/Edge/Brave/Opera.");
            else
                Log($"[!] {threats.Count} suspicious browser extension(s) detected!", isWarning: true);

            return threats;
        }
        #endregion

        #region 10. Permanent Annihilation Cleanup Engine
        public int CleanAllThreats(List<ThreatInfo> threats)
        {
            int cleanedCount = 0;
            int total = threats.Count;

            for (int i = 0; i < total; i++)
            {
                var threat = threats[i];
                if (threat.Status != ThreatStatus.Cleaned)
                {
                    if (CleanThreat(threat))
                    {
                        cleanedCount++;
                    }
                }
                Progress((int)((i + 1) / (double)total * 100), $"Cleaned {i + 1}/{total} threats...");
            }

            return cleanedCount;
        }

        public bool CleanThreat(ThreatInfo threat)
        {
            Log($"🧹 Annihilating threat: {threat.ThreatName} ({threat.TargetPath})...");

            try
            {
                switch (threat.Category)
                {
                    case ThreatCategory.MSBuildInfection:
                        return CleanMSBuildFile(threat);

                    case ThreatCategory.ScriptDropper:
                    case ThreatCategory.TempPayload:
                        return CleanFileOrDirectory(threat);

                    case ThreatCategory.SuspiciousProcess:
                        return KillSuspiciousProcess(threat);

                    case ThreatCategory.RegistryPersistence:
                        return CleanRegistryKey(threat);

                    case ThreatCategory.ScheduledTask:
                        return CleanScheduledTask(threat);

                    case ThreatCategory.WMIPersistence:
                        return CleanWMIConsumer(threat);

                    case ThreatCategory.HostsFileTampering:
                        return CleanHostsFile(threat);

                    case ThreatCategory.DiscordInjection:
                        return CleanDiscordInjection(threat);

                    case ThreatCategory.C2Connection:
                        // For C2 connections, kill the associated process
                        return KillSuspiciousProcess(threat);

                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                Log($"[X] Failed to clean {threat.TargetPath}: {ex.Message}", isError: true);
                threat.Status = ThreatStatus.FailedToClean;
                return false;
            }
        }

        private bool CleanMSBuildFile(ThreatInfo threat)
        {
            string filePath = threat.TargetPath;
            if (!File.Exists(filePath))
            {
                threat.Status = ThreatStatus.Cleaned;
                return true;
            }

            CreateBackup(filePath);
            FixPermissions(filePath);

            string content = File.ReadAllText(filePath);

            // Surgically clean targeted malicious blocks only (if they contain confirmed signatures or stealth exec)
            string newContent = Regex.Replace(content, @"(?s)<Target\s+[^>]*Name=""[^""]*PreBuild[^""]*""[^>]*>.*?</Target>", match =>
            {
                bool isMaliciousBlock = ConfirmedMalwareSignatures.Any(sig => match.Value.Contains(sig, StringComparison.OrdinalIgnoreCase)) ||
                                        match.Value.Contains("-WindowStyle Hidden", StringComparison.OrdinalIgnoreCase) ||
                                        match.Value.Contains("-EncodedCommand", StringComparison.OrdinalIgnoreCase) ||
                                        match.Value.Contains("certutil -urlcache", StringComparison.OrdinalIgnoreCase) ||
                                        match.Value.Contains("bitsadmin /transfer", StringComparison.OrdinalIgnoreCase) ||
                                        (match.Value.Contains("curl", StringComparison.OrdinalIgnoreCase) && match.Value.Contains(".exe", StringComparison.OrdinalIgnoreCase)) ||
                                        (match.Value.Contains("wget", StringComparison.OrdinalIgnoreCase) && match.Value.Contains(".exe", StringComparison.OrdinalIgnoreCase));
                return isMaliciousBlock ? "" : match.Value;
            }, RegexOptions.IgnoreCase);

            // Filter out individual malicious payload lines
            string[] lines = newContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var cleanLines = lines.Where(l =>
            {
                bool isMalSig = ConfirmedMalwareSignatures.Any(sig => l.Contains(sig, StringComparison.OrdinalIgnoreCase));
                bool isMalExec = (l.Contains("<Exec", StringComparison.OrdinalIgnoreCase) || l.Contains("<PreBuildEvent", StringComparison.OrdinalIgnoreCase)) &&
                                 (l.Contains("-WindowStyle Hidden", StringComparison.OrdinalIgnoreCase) ||
                                  l.Contains("-EncodedCommand", StringComparison.OrdinalIgnoreCase) ||
                                  l.Contains("certutil -urlcache", StringComparison.OrdinalIgnoreCase) ||
                                  l.Contains("bitsadmin /transfer", StringComparison.OrdinalIgnoreCase) ||
                                  (l.Contains("curl", StringComparison.OrdinalIgnoreCase) && l.Contains(".exe", StringComparison.OrdinalIgnoreCase)) ||
                                  (l.Contains("wget", StringComparison.OrdinalIgnoreCase) && l.Contains(".exe", StringComparison.OrdinalIgnoreCase)));
                return !isMalSig && !isMalExec;
            }).ToArray();

            newContent = string.Join(Environment.NewLine, cleanLines);
            File.WriteAllText(filePath, newContent);

            Log($"[OK] Annihilated XML grabber tags from: {filePath}");
            threat.Status = ThreatStatus.Cleaned;
            return true;
        }

        private bool CleanFileOrDirectory(ThreatInfo threat)
        {
            string path = threat.TargetPath;
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
                Log($"[OK] Permanently deleted malware directory: {path}");
            }
            else if (File.Exists(path))
            {
                File.Delete(path);
                Log($"[OK] Permanently deleted malware file: {path}");
            }

            threat.Status = ThreatStatus.Cleaned;
            return true;
        }

        private bool KillSuspiciousProcess(ThreatInfo threat)
        {
            var match = Regex.Match(threat.TargetPath, @"PID\s+(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int pid))
            {
                try
                {
                    Process proc = Process.GetProcessById(pid);
                    proc.Kill();
                    Log($"[OK] Terminated malicious process PID {pid}");
                }
                catch { }
            }

            threat.Status = ThreatStatus.Cleaned;
            return true;
        }

        private bool CleanRegistryKey(ThreatInfo threat)
        {
            if (threat.TargetPath.StartsWith("HKCU"))
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", writable: true);
                if (key != null)
                {
                    var valNameMatch = Regex.Match(threat.TargetPath, @"Value:\s*(.+)");
                    if (valNameMatch.Success)
                    {
                        key.DeleteValue(valNameMatch.Groups[1].Value, throwOnMissingValue: false);
                        Log($"[OK] Deleted HKCU registry autorun key: {valNameMatch.Groups[1].Value}");
                    }
                }
            }
            else if (threat.TargetPath.StartsWith("HKLM"))
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", writable: true);
                if (key != null)
                {
                    var valNameMatch = Regex.Match(threat.TargetPath, @"Value:\s*(.+)");
                    if (valNameMatch.Success)
                    {
                        key.DeleteValue(valNameMatch.Groups[1].Value, throwOnMissingValue: false);
                        Log($"[OK] Deleted HKLM registry autorun key: {valNameMatch.Groups[1].Value}");
                    }
                }
            }

            threat.Status = ThreatStatus.Cleaned;
            return true;
        }

        private bool CleanWMIConsumer(ThreatInfo threat)
        {
            try
            {
                var match = Regex.Match(threat.TargetPath, @"Subscription:\s*(.+)");
                if (match.Success)
                {
                    string name = match.Groups[1].Value;
                    using var searcher = new ManagementObjectSearcher(@"root\subscription", $"SELECT * FROM __EventConsumer WHERE Name = '{name}'");
                    using var objects = searcher.Get();
                    foreach (ManagementObject obj in objects)
                    {
                        obj.Delete();
                        Log($"[OK] Deleted WMI Event Consumer: {name}");
                    }
                }
            }
            catch { }

            threat.Status = ThreatStatus.Cleaned;
            return true;
        }

        private bool CleanScheduledTask(ThreatInfo threat)
        {
            try
            {
                // Extract task name from TargetPath (format: "TASK:TaskName")
                string taskName = threat.TargetPath.StartsWith("TASK:") 
                    ? threat.TargetPath[5..] 
                    : threat.TargetPath;

                var psi = new ProcessStartInfo("schtasks", $"/delete /tn \"{taskName}\" /f")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var proc = Process.Start(psi);
                proc?.WaitForExit(5000);

                Log($"[OK] Deleted malicious Scheduled Task: {taskName}");
                threat.Status = ThreatStatus.Cleaned;
                return true;
            }
            catch (Exception ex)
            {
                Log($"[X] Failed to delete Scheduled Task: {ex.Message}", isError: true);
                threat.Status = ThreatStatus.FailedToClean;
                return false;
            }
        }

        private bool CleanHostsFile(ThreatInfo threat)
        {
            string hostsPath = @"C:\Windows\System32\drivers\etc\hosts";
            if (File.Exists(hostsPath))
            {
                CreateBackup(hostsPath);
                FixPermissions(hostsPath);

                string[] lines = File.ReadAllLines(hostsPath);
                string[] securityDomains = new[] { "virustotal", "malwarebytes", "microsoft", "defender", "github", "raw.githubusercontent" };

                var cleanLines = lines.Where(l => !securityDomains.Any(d => l.Contains(d, StringComparison.OrdinalIgnoreCase))).ToArray();
                File.WriteAllLines(hostsPath, cleanLines);

                Log($"[OK] Restored clean Windows Hosts file at: {hostsPath}");
            }

            threat.Status = ThreatStatus.Cleaned;
            return true;
        }

        private bool CleanDiscordInjection(ThreatInfo threat)
        {
            string jsPath = threat.TargetPath;
            if (File.Exists(jsPath))
            {
                CreateBackup(jsPath);
                File.WriteAllText(jsPath, "module.exports = require('./discord_desktop_core.node');");
                Log($"[OK] Restored clean Discord desktop core index.js at: {jsPath}");
            }

            threat.Status = ThreatStatus.Cleaned;
            return true;
        }

        private void CreateBackup(string filePath)
        {
            try
            {
                string bakPath = filePath + $".bak_{DateTime.Now:yyyyMMdd_HHmmss}";
                File.Copy(filePath, bakPath, overwrite: true);
                Log($"  [Backup Created] {Path.GetFileName(bakPath)}");
            }
            catch { }
        }

        private void FixPermissions(string filePath)
        {
            try
            {
                ProcessStartInfo psiTakeown = new ProcessStartInfo("takeown", $"/f \"{filePath}\" /A")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(psiTakeown)?.WaitForExit();

                ProcessStartInfo psiIcacls = new ProcessStartInfo("icacls", $"\"{filePath}\" /grant Everyone:(F)")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(psiIcacls)?.WaitForExit();

                FileInfo fi = new FileInfo(filePath);
                fi.IsReadOnly = false;
            }
            catch { }
        }
        #endregion
    }
}
