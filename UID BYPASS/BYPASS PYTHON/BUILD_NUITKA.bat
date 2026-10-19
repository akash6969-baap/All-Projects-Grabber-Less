@echo off
title UID BYPASS - Nuitka Standalone Builder
color 0A

echo.
echo === CHECKING REQUIREMENTS ===

python -m nuitka --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Nuitka not found. Installing Nuitka and dependencies...
    venv\Scripts\python.exe -m pip install nuitka ordered-set zstandard
)

if not exist "main.py" (
    echo ERROR: main.py not found in current directory!
    pause
    exit /b 1
)

if not exist "UIDBypassDll.dll" (
    echo ERROR: UIDBypassDll.dll not found in current directory!
    pause
    exit /b 1
)

echo.
echo === CLEANING OLD FILES ===
rmdir /s /q dist_nuitka >nul 2>&1
del /f /q UID_BYPASS_RELEASE.zip >nul 2>&1

echo.
echo === COMPILING WITH NUITKA (STANDALONE MODE) ===
echo This will compile Python code directly to C and produce a protected native EXE.
echo Please wait, this process can take 5 to 15 minutes...
echo.

venv\Scripts\python.exe -m nuitka --standalone --windows-console-mode=disable --windows-uac-admin --enable-plugin=pyqt5 --include-data-files=UIDBypassDll.dll=UIDBypassDll.dll --include-data-dir=mycerts=mycerts --windows-icon-from-ico=logo.ico --output-dir=dist_nuitka --output-filename=UID_BYPASS.exe main.py

if %errorlevel% neq 0 (
    echo.
    echo ERROR: Nuitka compilation failed!
    pause
    exit /b %errorlevel%
)

echo.
echo === CREATING DISTRIBUTION ZIP ===
powershell -Command "Compress-Archive -Path '.\dist_nuitka\main.dist\*' -DestinationPath '.\UID_BYPASS_RELEASE.zip' -Force"

echo.
echo === BUILD DONE! ===
echo.
echo Output Folder: dist_nuitka\main.dist
echo Output ZIP:    UID_BYPASS_RELEASE.zip
echo.
echo NOTE: Share 'UID_BYPASS_RELEASE.zip' with your users.
echo       They must extract the ZIP and run 'UID_BYPASS.exe' as Administrator.
echo       The source code is 100%% protected and compiled to C!
echo.
pause



