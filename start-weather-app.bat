@echo off
rem Windows CMD wrapper. The real setup logic lives in start-weather-app.ps1.
rem This file keeps double-click/Command Prompt startup simple and UTF-8 safe.
chcp 65001 >nul
setlocal
cd /d "%~dp0"

if not exist "%~dp0start-weather-app.ps1" (
    echo start-weather-app.ps1 wurde nicht gefunden.
    pause
    exit /b 1
)

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0start-weather-app.ps1" %*
exit /b %ERRORLEVEL%
