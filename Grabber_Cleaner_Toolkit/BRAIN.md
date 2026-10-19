# 🧠 GRABBER & MSBUILD MALWARE CLEANER TOOLKIT v3.0 — BRAIN.MD

> **Location**: `C:\Users\akash\Desktop\Grabber_Cleaner_Toolkit\`  
> **Updated**: October 2026 — v3.0 with 10-Layer Full Scan System  
> **Status**: ✅ Fully Operational | Build: `dotnet build GrabberCleanerApp`

---

## 📌 OVERVIEW & PURPOSE

Complete **10-Layer Protection Suite** against grabbers, MSBuild worms, RATs, Discord stealers, browser extension hijackers, crypto clipboard swappers, and C2 network beacons. Run **`GrabberCleaner.exe`**, click Scan → view Root Cause Diagnostics → 1-Click Destroy.

---

## 🛠️ TOOLKIT CONTENTS

| File / Component | Type | Description |
| :--- | :--- | :--- |
| **`GrabberCleanerApp\`** | **C# .NET 9 WinForms App** | Main GUI toolkit — 10-layer scanner + 1-click cleaner |
| **`Build_Executable.bat`** | Batch Builder | Rebuilds `.exe` from source with .NET 9 |
| **`Run_As_Admin_Cleaner.bat`** | Admin Script | Quick admin-level auto-cleaner |
| **`cleaner.ps1`** | PowerShell Script | PowerShell-native cleaner |

---

## 🔬 10-LAYER SCAN ENGINE (v3.0)

| # | Module | What It Detects |
|---|---|---|
| 1 | **Active Process Scanner** | Running PowerShell/CMD/MSHTA with grabber signatures |
| 2 | **Temp & AppData Scanner** | Payload dirs (AWyhU7, BX7010, etc.) in Temp/AppData |
| 3 | **Script Dropper Scanner** | Malicious .vbs/.ps1/.bat/.lnk in startup/temp |
| 4 | **WMI Event Consumer Scanner** | Hidden WMI persistence |
| 5 | **Registry Autorun Scanner** | HKCU/HKLM Run keys injected by grabbers |
| 6 | **Scheduled Task Scanner** (NEW) | schtasks + WMI malware hook detection |
| 7 | **Hosts File Integrity Scanner** | Malware blocking Defender/GitHub/VirusTotal |
| 8 | **Discord Injection Scanner** | Infected index.js in discord_desktop_core |
| 9 | **Browser Extension Scanner** (NEW) | Chrome/Edge/Brave/Opera suspicious extension analysis |
| 10 | **Live C2 Network Scanner** | netstat ESTABLISHED connections to known C2 IPs + RAT ports |
| 11 | **MSBuild/Project Scanner** | All drive .csproj/.vcxproj PreBuild hook scan + bin/obj binaries |
| 12 | **VS Template Scanner** | Visual Studio system templates infected by MSBuild worm |
| 13 | **PE Section & W^X Analyzer** (NEW) | DOS/NT/Section headers, W^X violations, section entropy |
| 14 | **Authenticode Signature Engine** (NEW) | X.509 PKCS#7 digital signature check (Microsoft/Google) |
| 15 | **Evidence Correlation Matrix** (NEW) | Multi-indicator correlation (DPAPI + Webhook + DB = Stealer) |
| 16 | **Anti-Camouflage Whitelist Guard** (NEW) | Whitelists legit licensing but blocks hidden stealers/droppers |

---

## 🦠 MALWARE SIGNATURE DATABASE (v3.0) — 70+ signatures

**Grabber Families**: AWyhU7, alJvVFZa, BX7010, 3ZTWX, Retev, BlankGrabber, Epsilon, Phorcy, NexusStealer, RedLineStealer, LummaC2, RisePro, StealC, Vidar, RecordBreaker, RaccoonStealer, AZORult, Meduza, WhiteSnake

**RATs**: AsyncRAT, DCRat, QuasarRAT, njRAT, XWorm

**PowerShell Obfuscation**: -WindowStyle Hidden, -EncodedCommand, -NoProfile -ExecutionPolicy Bypass, FromBase64String, IEX(, Invoke-Expression

**C2/Exfil Hosts**: pee-files.nl, catbox.moe, transfer.sh, gofile.io, anonfiles.com, pixeldrain.com, api.telegram.org/bot, api/webhooks

**Known C2 IPs**: 91.92.243.218, 185.220.101.x, 194.165.16.x, 193.42.60.x

**RAT Ports**: 4444, 1337, 8888, 9999, 6666, 7777, 31337, 1604

**Whitelisted Legit Auth / License Systems (Anti-False-Positive)**:
KeyAuth, FearAuth, Auth.GG, Authon, PWF Auth, LicenseSeat, Keyed, Toolkits, qPapel Auth, EasyAuth, LibreAuth, SentriNet, System Locker, Cryptolens, Keygen, LicenseSpring, WyDay LimeLM, 10Duke Licensing, Nalpeiron, Reprise License Manager (RLM), SafeNet Sentinel, FlexNet Publisher, Thales Sentinel, Labs64 NetLicensing, License4J, SoftwareKey Protection PLUS, Cryptlex, Labs64, Reprise, License Dashboard, licenseauth, VMProtect, Themida, CodeMeter.

---

## 🛡️ EMERGENCY RECOVERY CHECKLIST

1. **Disconnect Wi-Fi / Ethernet immediately.**
2. Run **`GrabberCleaner.exe`** → Click **FULL DEEP SYSTEM SCAN**
3. Click **1-CLICK CLEAN & UDANA** to destroy all threats.
4. **Change all passwords** (Google, GitHub, Discord, Steam) from a **clean phone**.
5. Enable **2-Factor Authentication (2FA)** on all accounts.
6. **Revoke all active Discord sessions** (User Settings → Devices).
7. Check **crypto wallets** for unauthorized transactions.

---

## 🔍 HOW PREBUILD GRABBER INFECTS YOUR SYSTEM

1. **Infection Vector**: Infected project/cracked tool compiled → injects `<Target Name="PreBuild">` into VS templates.
2. **Re-infection Loop**: Every MSBuild/VS build triggers encoded PowerShell payload.
3. **Payload Actions**: Downloads grabber EXE to %TEMP%, steals Discord/Chrome/MetaMask, spreads to all .csproj files, registers Registry + Scheduled Task + WMI persistence.


---

## 📌 OVERVIEW & PURPOSE

This toolkit provides **complete 1-Click protection and permanent elimination** against **PreBuild Grabber Malware, MSBuild Injector Worms, Discord Token Stealers, and Autorun Persistence**.

If any grabber ever infects your system or project files (`.csproj`, `.vcxproj`), you can open **`GrabberCleaner.exe`**, view the exact diagnostic location and reason why it came, and destroy it with **1-Click Udana**.

---

## 🛠️ TOOLKIT CONTENTS & FILES

| File / Component | Type | Description | How to Run |
| :--- | :--- | :--- | :--- |
| **`GrabberCleaner.exe`** | **Standalone Windows App** | **High-Tech GUI Cleaner App**. Includes Full System Scan, Threat Location & Root Cause Diagnostics, and 1-Click Permanent Cleanup with Auto-Backup. | **Double-Click / Run as Admin** |
| **`Build_Executable.bat`** | Batch Builder | **1-Click Exe Rebuilder**. Compiles `GrabberCleanerApp` into a standalone `.exe` anytime using .NET 9. | Double-Click |
| **`Run_As_Admin_Cleaner.bat`** | Admin Script | **1-Click Auto Cleaner & Fixer**. Auto-elevates to Admin, kills temp malware, takes ownership of system templates, strips malware tags across all drives. | Right-Click -> Run as administrator |
| **`deep_scan.py`** | Python Scanner | **Deep Malware Scanner**. Scans all drives for infected project files and known grabber signatures (`AWyhU7`, `alJvVFZa`, `BX7010`, `3ZTWX`, `Retev.php`, `pee-files.nl`). | `py deep_scan.py` |
| **`clean_projects.py`** | Python Cleaner | **Python Stripper**. Takes ownership of read-only templates and strips `<Target Name="PreBuild">` payload tags. | `py clean_projects.py` |
| **`cleaner.ps1`** | PowerShell Script | Native Windows PowerShell cleaner for fast command-line execution. | `powershell -ExecutionPolicy Bypass -File cleaner.ps1` |

---

## ⚡ HOW TO USE THIS TOOLKIT

### Option 1: Open `GrabberCleaner.exe` (GUI App)
1. Double-click **`GrabberCleaner.exe`** (or `GrabberCleaner.bat`).
2. Quick Check (5s): Click **`⚡ SMART FAST SCAN (5s)`** — checks active RAM, startup, tasks, C2 sockets, and hotspots.
3. Target VS Infection: Click **`🔨 HUNT VS & MSBUILD INFECTION`** — hunts rogue Directory.Build hooks, system targets & repos.
4. Trace Droppers: Click **`🕵️ TRACE C2 & DROPPED SCRIPTS`** — traces active C2 sockets & dropped .vbs/.ps1/.exe files.
5. Deep File X-Ray: Click **`🎯 INSPECT EXE / FILE`** — runs 10-stage static forensic pipeline with structured evidence.
6. Full Drive Scan: Click **`🚀 FULL DEEP SCAN`** — multi-core parallel whole-disk inspection.
7. Click **`🧹 ANNIHILATE ALL THREATS`** to permanently destroy all detected viruses with automatic backup!

---

## 🏗️ ENTERPRISE-GRADE FORENSIC ENGINE (v4.0 MODULAR ARCHITECTURE)

| Module | Responsibility |
|---|---|
| **`EvidenceModel.cs`** | Structured `EvidenceItem`, 21 behavioral categories, 5 severities, 0% initial confidence. |
| **`PeAnalyzer.cs`** | PE32/PE32+ headers, section table, per-section Shannon entropy, W^X violations, overlay detector. |
| **`DotNetAnalyzer.cs`** | Static CLR metadata, P/Invoke, `Assembly.Load(byte[])` memory droppers, reflection, PyInstaller. |
| **`ImportAnalyzer.cs`** | Win32 native APIs grouped into Process, Memory, Injection, Credential/DPAPI, Persistence, Network. |
| **`StringAnalyzer.cs`** | ASCII, Unicode, Base64 & reversed string de-obfuscation (extracts hidden C2 webhooks, browser DBs). |
| **`EmbeddedPayloadAnalyzer.cs`** | Recursive static analysis of embedded secondary PE executables (Max depth 2, duplicate protection). |
| **`CorrelationEngine.cs`** | Behavioral chain-of-crime evaluation (Theft Target + DPAPI + Webhook = 98% Confirmed Stealer). |
| **`RiskConfidenceEngine.cs`** | Dynamic Risk (0-100) & Confidence (0-100%) computation with mitigating factors (Authenticode/Auth). |
| **`ExeInspector.cs`** | Master pipeline coordinator linking modular analyzers to UI. |

---

## 🔮 TOMORROW'S ROADMAP & PLANNED UPGRADES
* **Next-Level Engine Upgrades**: Deep memory string unpacker, advanced anti-evasion detection, sandbox emulation.
* **Modern Cyberpunk / Futuristic UI Overhaul**: Transform standard WinForms appearance into a sleek, dark-themed, state-of-the-art cybersecurity HUD with custom animated progress, glowing telemetry cards, and modern typography.
* **Current Phase**: Testing & Calibration Phase (Current EXE is fully functional for live validation).
