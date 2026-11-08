@echo off
title UTHM Varsity Tracker - Web Backend Server
echo ============================================================
echo   UTHM Varsity Tracker - ASP.NET Core Web Backend
echo   Theme: S.P.O.R.T.S. (BIE33103 .NET Programming)
echo ============================================================
echo Starting Kestrel Web Server on http://localhost:5299 and https://localhost:7291...
echo.
cd /d "%~dp0VarsityTracker.Web"
dotnet run --launch-profile "VarsityTracker.Web"
pause

