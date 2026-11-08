@echo off
title UTHM Varsity Tracker - Multi-Project Launcher
echo ============================================================
echo   UTHM Varsity Tracker - Multi-Project Launcher
echo   Web Management Portal + .NET MAUI Mobile App
echo ============================================================
echo.
echo 1. Starting Web Backend Server (Kestrel http://localhost:5299)...
start "VarsityTracker Web Server" cmd /k "cd /d "%~dp0VarsityTracker.Web" && dotnet run --launch-profile "VarsityTracker.Web""

echo 2. Waiting 3 seconds for Web Server to initialize...
timeout /t 3 /nobreak > nul

echo 3. Launching .NET MAUI Windows Mobile Client...
start "VarsityTracker Mobile" cmd /k "cd /d "%~dp0VarsityTracker.Mobile" && dotnet run -f net10.0-windows10.0.19041.0"

echo.
echo Both projects have been launched successfully!
echo Web Portal:   http://localhost:5299
echo Mobile App:   Running in Windows MAUI window
echo.
pause

