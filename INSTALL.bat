@echo off
echo ========================================
echo       paste-md Quick Installer
echo ========================================
echo.

REM Check for admin rights
net session >nul 2>&1
if %errorLevel% == 0 (
    echo Administrative rights confirmed.
) else (
    echo Please run as Administrator for startup option.
    echo.
)

REM Create paste-md folder in Program Files
set "INSTALL_DIR=%ProgramFiles%\paste-md"
echo Creating installation directory...
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"

REM Copy files
echo Installing paste-md...
xcopy /Y /E "src\paste-md.TrayApp\bin\Release\net48\*.*" "%INSTALL_DIR%\" >nul 2>&1

REM Create Start Menu shortcut
set "STARTMENU=%ProgramData%\Microsoft\Windows\Start Menu\Programs"
powershell -Command "$WshShell = New-Object -ComObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%STARTMENU%\paste-md.lnk'); $Shortcut.TargetPath = '%INSTALL_DIR%\paste-md.exe'; $Shortcut.Description = 'paste-md - Render Markdown with Ctrl+Shift+V'; $Shortcut.Save()"

REM Ask about startup
echo.
choice /C YN /M "Add paste-md to Windows startup"
if %errorlevel%==1 (
    REM Add to startup
    reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "paste-md" /d "%INSTALL_DIR%\paste-md.exe" /f >nul
    echo Added to startup.
) else (
    echo Skipped startup configuration.
)

echo.
echo ========================================
echo    Installation Complete!
echo ========================================
echo.
echo paste-md has been installed to: %INSTALL_DIR%
echo.
echo To use paste-md:
echo   1. Start from Start Menu: paste-md
echo   2. Copy any Markdown text
echo   3. Press Ctrl+Shift+V in Word, OneNote, or Outlook
echo.
echo Starting paste-md now...
start "" "%INSTALL_DIR%\paste-md.exe"

pause