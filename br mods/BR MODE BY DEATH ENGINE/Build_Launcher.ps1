# Fear Mods - Full Build Script (PowerShell)
# Step 1: Build DLL via MSBuild
# Step 2: Embed DLL into Launcher EXE using RC + CL

$ErrorActionPreference = "Stop"

$ROOT    = Split-Path $MyInvocation.MyCommand.Path
$PROJ    = Join-Path $ROOT "example_win32_directx11"
$OUT     = Join-Path $ROOT "x64\Release"
$DLL     = Join-Path $OUT  "Fear Mods.dll"
$EXE     = Join-Path $OUT  "Fear Mods Launcher.exe"
$RC_FILE = Join-Path $PROJ "launcher_resources.rc"
$RES     = Join-Path $PROJ "launcher_resources.res"
$ENTRY   = Join-Path $PROJ "launcher_entry.cpp"

$MSBUILD = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
$VCVARS  = "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat"
$RC_EXE  = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\rc.exe"

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  Fear Mods - Full Build (DLL + Embedded EXE)" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

# ── Step 1: Build DLL ────────────────────────────────────────
Write-Host "`n[Step 1/4] Building Fear Mods.dll ..." -ForegroundColor Yellow
$sln = Join-Path $ROOT "BR MODE.sln"
& $MSBUILD $sln /p:Configuration=Release /p:Platform=x64 /t:Build /m /nologo
if ($LASTEXITCODE -ne 0) { Write-Error "DLL build failed!"; exit 1 }
if (-not (Test-Path $DLL)) { Write-Error "DLL not found: $DLL"; exit 1 }
Write-Host "[OK] DLL built: $DLL" -ForegroundColor Green

# ── Step 2: Write RC file ────────────────────────────────────
Write-Host "`n[Step 2/4] Writing resource file ..." -ForegroundColor Yellow
# Use forward slashes — RC.exe accepts them and no escape issues
$DLL_RC = $DLL.Replace('\','/')
$rcContent = "101 DLLBIN `"$DLL_RC`""
[System.IO.File]::WriteAllText($RC_FILE, $rcContent, [System.Text.Encoding]::ASCII)
Write-Host "[OK] RC file written: $RC_FILE"
Write-Host "[OK] DLL path in RC : $DLL_RC"

# ── Step 3: Compile RC ───────────────────────────────────────
Write-Host "`n[Step 3/4] Compiling resource file ..." -ForegroundColor Yellow
& $RC_EXE /fo $RES $RC_FILE
if ($LASTEXITCODE -ne 0) { Write-Error "RC compile failed!"; exit 1 }
Write-Host "[OK] Resource compiled: $RES" -ForegroundColor Green

# ── Step 4: Build Launcher EXE ───────────────────────────────
Write-Host "`n[Step 4/4] Building Fear Mods Launcher.exe ..." -ForegroundColor Yellow

# Setup MSVC environment via a temp batch file
$tmpBat = Join-Path $env:TEMP "build_launcher_tmp.bat"
@"
@echo off
call "$VCVARS" >nul 2>&1
cl.exe /nologo /W3 /O2 /MT /EHsc /Fe:"$EXE" "$ENTRY" "$RES" /link /SUBSYSTEM:WINDOWS /MACHINE:X64 user32.lib kernel32.lib
exit /b %errorlevel%
"@ | Set-Content $tmpBat -Encoding ASCII

cmd.exe /c $tmpBat
$clExit = $LASTEXITCODE
Remove-Item $tmpBat -Force -ErrorAction SilentlyContinue

if ($clExit -ne 0) { Write-Error "Launcher EXE build failed!"; exit 1 }
if (-not (Test-Path $EXE)) { Write-Error "EXE not found after build!"; exit 1 }

$exeSize = [math]::Round((Get-Item $EXE).Length / 1MB, 2)
Write-Host "[OK] Launcher EXE built! Size: $exeSize MB" -ForegroundColor Green

Write-Host "`n============================================================" -ForegroundColor Cyan
Write-Host "  BUILD COMPLETE!" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  DLL : $DLL"
Write-Host "  EXE : $EXE"
Write-Host ""
Write-Host "  EXE ke andar DLL embedded hai!" -ForegroundColor Green
Write-Host "  Sirf 'Fear Mods Launcher.exe' share karo - alag DLL ki zaroorat NAHI!" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Cyan

Read-Host "`nPress Enter to exit"
