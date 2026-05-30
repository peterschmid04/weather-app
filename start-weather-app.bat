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
set "SCRIPT_EXIT=%ERRORLEVEL%"

if not "%SCRIPT_EXIT%"=="0" (
    echo.
    echo Setup oder Docker-Start wurde mit Fehler beendet.
    echo Lies die Meldung direkt ueber dieser Zeile. Wenn keine .env erstellt wurde,
    echo war meistens eine Eingabe ungueltig oder die Datei konnte nicht geschrieben werden.
    if exist "%~dp0.env" (
        echo .env ist im Projektordner vorhanden.
    ) else (
        echo .env wurde nicht erstellt.
    )
    pause
    exit /b %SCRIPT_EXIT%
)

if /I "%~1"=="-ValidateOnly" (
    exit /b 0
)

if not exist "%~dp0.env" (
    echo.
    echo Es wurde keine .env im Projektordner erstellt.
    echo Starte die Datei ohne -ValidateOnly und pruefe die Eingaben.
    pause
    exit /b 1
)

exit /b 0
