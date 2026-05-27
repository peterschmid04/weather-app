@echo off
setlocal
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0start-weather-app.ps1"
exit /b %ERRORLEVEL%
