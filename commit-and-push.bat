@echo off
REM Script to commit and push WinUdateDiag changes to GitHub
REM Schedule this with Windows Task Scheduler to run at 9pm Eastern

PowerShell -ExecutionPolicy Bypass -File "%~dp0commit-and-push.ps1"
pause
