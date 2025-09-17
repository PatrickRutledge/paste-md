@echo off
echo ========================================
echo       paste-md Uninstaller
echo ========================================
echo.

REM Kill running paste-md
echo Stopping paste-md if running...
taskkill /F /IM paste-md.exe >nul 2>&1

REM Remove from startup
echo Removing from startup...
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "paste-md" /f >nul 2>&1

REM Remove Start Menu shortcut
echo Removing Start Menu shortcut...
del "%ProgramData%\Microsoft\Windows\Start Menu\Programs\paste-md.lnk" >nul 2>&1

REM Remove installation directory
set "INSTALL_DIR=%ProgramFiles%\paste-md"
if exist "%INSTALL_DIR%" (
    echo Removing installation files...
    rmdir /S /Q "%INSTALL_DIR%"
)

REM Clean up shell extension registration if exists
echo Cleaning registry...
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe "%INSTALL_DIR%\paste-md.Core.dll" /u >nul 2>&1

echo.
echo ========================================
echo    Uninstall Complete!
echo ========================================
echo.
echo paste-md has been removed from your system.
echo.
pause