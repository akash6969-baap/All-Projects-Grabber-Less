@echo off
title FEAR-X Discord Bot Runner
echo ===========================================
echo   FEAR-X Discord Bot - Installer & Runner
echo ===========================================
echo.
echo [1/2] Checking and installing dependencies...
pip install -r requirements.txt
if %errorlevel% neq 0 (
    echo.
    echo [!] Warning: Failed to install packages using global pip.
    echo Trying to run bot anyway...
)
echo.
echo [2/2] Starting Discord Bot...
echo.
python bot.py
pause
