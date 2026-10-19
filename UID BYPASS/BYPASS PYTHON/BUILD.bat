@echo off
title UID BYPASS - EXE Builder
color 0C

echo  [*] Terminating existing instances...
taskkill /F /IM UID_BYPASS.exe >nul 2>&1
taskkill /F /IM main.exe >nul 2>&1

echo.
echo  [*] Installing required packages...
pip install pyinstaller PyQt5 psutil --quiet

echo.
echo  [*] Building EXE...
pyinstaller --clean main.spec

echo.
if exist "dist\UID_BYPASS.exe" (
    echo  [OK] BUILD SUCCESSFUL!
    echo  [>>] EXE Location: dist\UID_BYPASS.exe
    start "" "dist"
) else (
    echo  [!!] BUILD FAILED - Check errors above.
)

pause
