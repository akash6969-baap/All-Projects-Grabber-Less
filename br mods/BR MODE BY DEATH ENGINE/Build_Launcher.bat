@echo off
setlocal enabledelayedexpansion

echo ============================================================
echo   Fear Mods - Full Build (DLL + Embedded EXE)
echo ============================================================

set "ROOT=%~dp0"
set "PROJ=%ROOT%example_win32_directx11"
set "OUT=%ROOT%x64\Release"
set "DLL_PATH=%OUT%\Fear Mods.dll"
set "EXE_PATH=%OUT%\Fear Mods Launcher.exe"
set "RC_FILE=%PROJ%\launcher_resources.rc"
set "RES_FILE=%PROJ%\launcher_resources.res"
set "ENTRY=%PROJ%\launcher_entry.cpp"

set "MSBUILD=C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
set "VCVARS=C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat"

:: RC.exe - Windows SDK 10.0.26100.0
set "RC_EXE=C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\rc.exe"
if not exist "%RC_EXE%" (
    :: Fallback: search any version
    for /f "delims=" %%f in ('dir /b /s "C:\Program Files (x86)\Windows Kits\10\bin\*.0\x64\rc.exe" 2^>nul') do set "RC_EXE=%%f"
)
if not exist "%RC_EXE%" (
    echo [ERROR] rc.exe not found. Install Windows 10 SDK.
    pause
    exit /b 1
)
echo [INFO] Using RC: %RC_EXE%

echo.
echo [Step 1/4] Building Fear Mods.dll ...
echo ------------------------------------------------------------
"%MSBUILD%" "%ROOT%BR MODE.sln" /p:Configuration=Release /p:Platform=x64 /t:Build /m /nologo
if errorlevel 1 (
    echo [ERROR] DLL build failed!
    pause
    exit /b 1
)

if not exist "%DLL_PATH%" (
    echo [ERROR] DLL not found: %DLL_PATH%
    pause
    exit /b 1
)
echo [OK] DLL built successfully!

echo.
echo [Step 2/4] Writing resource file ...
echo ------------------------------------------------------------
:: Write RC file — use short (8.3) path to avoid space issues in RC
for %%F in ("%DLL_PATH%") do set "DLL_SHORT=%%~sF"
echo 101 DLLBIN "%DLL_SHORT%"> "%RC_FILE%"
echo [OK] RC file: %RC_FILE%
echo [OK] DLL path (short): %DLL_SHORT%

echo.
echo [Step 3/4] Compiling resource file ...
echo ------------------------------------------------------------
"%RC_EXE%" /fo "%RES_FILE%" "%RC_FILE%"
if errorlevel 1 (
    echo [ERROR] RC compile failed!
    pause
    exit /b 1
)
echo [OK] Resource compiled!

echo.
echo [Step 4/4] Building Fear Mods Launcher.exe ...
echo ------------------------------------------------------------
call "%VCVARS%" >nul 2>&1

:: Use short paths to avoid spaces in CL.exe args
for %%F in ("%ENTRY%") do set "ENTRY_SHORT=%%~sF"
for %%F in ("%RES_FILE%") do set "RES_SHORT=%%~sF"
for %%F in ("%EXE_PATH%") do set "EXE_SHORT=%%~sF"

cl.exe /nologo /W3 /O2 /MT /EHsc ^
    /Fe:"%EXE_PATH%" ^
    "%ENTRY_SHORT%" ^
    "%RES_SHORT%" ^
    /link /SUBSYSTEM:WINDOWS /MACHINE:X64 user32.lib kernel32.lib
if errorlevel 1 (
    echo [ERROR] Launcher EXE build failed!
    pause
    exit /b 1
)
echo [OK] Launcher EXE built!

echo.
echo ============================================================
echo   BUILD COMPLETE!
echo ============================================================
echo   DLL : %DLL_PATH%
echo   EXE : %EXE_PATH%
echo.
echo   EXE ke andar DLL embedded hai!
echo   Sirf "Fear Mods Launcher.exe" share karo - bas!
echo ============================================================
echo.
pause
