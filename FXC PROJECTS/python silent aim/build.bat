@echo off
title Building EXE with PyInstaller
setlocal

echo === CHECKING REQUIREMENTS ===

REM Check if PyInstaller is installed
pyinstaller --version >nul 2>&1
if %errorlevel% neq 0 (
    echo PyInstaller not found. Installing...
    pip install pyinstaller
)

REM Check if main Python file exists
if not exist "FearMods.py" (
    echo ERROR: FearMods.py not found in current directory!
    pause
    exit /b 1
)

REM Check DLL files
if not exist "Client.dll" (
    echo WARNING: Client.dll not found! Build may fail or cheat won't work.
)
if not exist "cimgui.dll" (
    echo WARNING: cimgui.dll not found! Build may fail or cheat won't work.
)
if not exist "AotBst.dll" (
    echo WARNING: AotBst.dll not found! Build may fail or cheat won't work.
)
REM Check Succes.wav file
if not exist "Succes.wav" (
    echo WARNING: Succes.wav not found! Build may lack custom completion audio.
)

echo.
echo === CLEANING OLD FILES ===
rmdir /s /q build >nul 2>&1
rmdir /s /q dist >nul 2>&1

echo.
echo === COMPILING WITH PYINSTALLER SPEC ===

pyinstaller ^
  --clean ^
  SilentAim.spec

REM Add DLLs and assets to output if they exist
if exist "Client.dll" (
    echo Copying Client.dll to build folder...
    copy "Client.dll" "build\Client.dll" >nul
)
if exist "cimgui.dll" (
    echo Copying cimgui.dll to build folder...
    copy "cimgui.dll" "build\cimgui.dll" >nul
)
if exist "AotBst.dll" (
    echo Copying AotBst.dll to build folder...
    copy "AotBst.dll" "build\AotBst.dll" >nul
)
if exist "Succes.wav" (
    echo Copying Succes.wav to build folder...
    copy "Succes.wav" "build\Succes.wav" >nul
)

echo.
echo === BUILD DONE! ===
echo Output EXE is in: build\SilentAim.exe
echo.
echo NOTE: Make sure Client.dll, cimgui.dll, AotBst.dll, and Succes.wav are in the same folder as the EXE!
pause
