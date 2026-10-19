@echo off
title MSI + BlueStacks Lag Fixer - BY FEAR
color 0a

echo ============================================
echo        MSI + BlueStacks Lag Fixer
echo                BY FEAR
echo ============================================
echo.

:: Step 1 - Clean Windows Temp
echo Cleaning Temp folders...
del /f /s /q %temp%\*.* >nul 2>&1
rd /s /q %temp% >nul 2>&1
md %temp% >nul 2>&1

del /f /s /q %windir%\Temp\*.* >nul 2>&1
rd /s /q %windir%\Temp >nul 2>&1
md %windir%\Temp >nul 2>&1

del /f /s /q C:\Windows\Prefetch\*.* >nul 2>&1

echo Temp folders cleaned.
echo.

:: Step 2 - Empty Recycle Bin
echo Emptying Recycle Bin...
rd /s /q %systemdrive%\$Recycle.Bin >nul 2>&1
echo Recycle Bin cleaned.
echo.

:: Step 3 - Flush DNS + Clear Memory Cache
echo Flushing DNS and cleaning memory cache...
ipconfig /flushdns >nul
ipconfig /release >nul
ipconfig /renew >nul

:: Clear standby memory (Windows 10/11 only)
echo Freeing RAM cache...
powershell.exe -command "Clear-Content -Path $env:temp\* -Force" >nul 2>&1
echo RAM cache cleared.
echo.

:: Step 4 - Restart Windows Installer Service
echo Restarting Windows Installer Service (MSI)...
net stop msiserver >nul 2>&1
net start msiserver >nul 2>&1
echo Windows Installer restarted.
echo.

:: Step 5 - BlueStacks Cache Cleaner
echo Cleaning BlueStacks cache...
del /f /s /q "%programdata%\BlueStacks_nxt\Engine\Cache\*.*" >nul 2>&1
del /f /s /q "%localappdata%\BlueStacks_nxt\Cache\*.*" >nul 2>&1
echo BlueStacks cache cleaned.
echo.

echo ============================================
echo     ✅ Lag Fix Completed Successfully!
echo             BY FEAR
echo ============================================
pause
