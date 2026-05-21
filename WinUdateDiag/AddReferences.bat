@echo off
echo ============================================================
echo  Adding Missing References to WinUdateDiag Project
echo ============================================================
echo.
echo IMPORTANT: You must CLOSE Visual Studio before running this!
echo.
pause
echo.

cd /d "%~dp0"

echo Checking if Visual Studio is closed...
tasklist /FI "IMAGENAME eq devenv.exe" 2>NUL | find /I /N "devenv.exe">NUL
if "%ERRORLEVEL%"=="0" (
    echo ERROR: Visual Studio is still running!
    echo Please close Visual Studio and run this script again.
    pause
    exit /b 1
)

echo Visual Studio is closed. Proceeding...
echo.

powershell.exe -ExecutionPolicy Bypass -File "AddReferences.ps1"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ============================================================
    echo  SUCCESS! References have been added.
    echo ============================================================
    echo.
    echo Next steps:
    echo   1. Open Visual Studio
    echo   2. Reload the solution
    echo   3. Build the project ^(Ctrl+Shift+B^)
    echo.
) else (
    echo.
    echo ERROR: Failed to add references.
    echo.
)

pause
