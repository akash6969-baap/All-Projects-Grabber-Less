@echo off
title UNINSTALL RUNTIMEBROKER SERVICE
cd /d "%~dp0"
cls

echo ====================================================
echo      REMOVING RUNTIMEBROKER BACKGROUND SERVICE
echo ====================================================
echo.

:: Ensure admin elevation
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo [!] Requesting administrator privileges...
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

echo [1] Stopping background process...
taskkill /f /im RuntimeBroker.exe >nul 2>&1

echo [2] Deleting Windows Task Scheduler startup task...
schtasks /delete /tn "RuntimeBrokerSync" /f >nul 2>&1

echo [3] Removing installed service files...
if exist "C:\ProgramData\Microsoft\DeviceSync\StreamerService" (
    rmdir /s /q "C:\ProgramData\Microsoft\DeviceSync\StreamerService" >nul 2>&1
)

echo.
echo ====================================================
echo Service has been completely removed from your PC!
echo ====================================================
pause
