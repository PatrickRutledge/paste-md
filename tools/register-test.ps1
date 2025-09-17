# paste-md Test Registration Script
# Run this as Administrator to register the shell extension for testing

param(
    [switch]$Unregister = $false
)

$ErrorActionPreference = "Stop"

# Check if running as Administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{
    Write-Host "This script requires Administrator privileges." -ForegroundColor Red
    Write-Host "Please run PowerShell as Administrator and try again." -ForegroundColor Yellow
    exit 1
}

# Paths
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptPath
$dllPath = Join-Path $projectRoot "src\paste-md.Core\bin\x64\Debug\net6.0-windows\paste-md.Core.dll"

# Check if DLL exists
if (!(Test-Path $dllPath)) {
    Write-Host "DLL not found at: $dllPath" -ForegroundColor Red
    Write-Host "Please build the project first: dotnet build --configuration Debug" -ForegroundColor Yellow
    exit 1
}

# Find RegAsm.exe
$regAsm = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe"
if (!(Test-Path $regAsm)) {
    Write-Host "RegAsm.exe not found at: $regAsm" -ForegroundColor Red
    exit 1
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "     paste-md Test Registration         " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($Unregister) {
    # Unregister
    Write-Host "Unregistering shell extension..." -ForegroundColor Yellow

    & $regAsm "$dllPath" /u

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Shell extension unregistered successfully" -ForegroundColor Green
    } else {
        Write-Host "✗ Failed to unregister shell extension" -ForegroundColor Red
        exit 1
    }
} else {
    # Register
    Write-Host "DLL Path: $dllPath" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Registering shell extension..." -ForegroundColor Yellow

    # First try to unregister if already registered
    & $regAsm "$dllPath" /u 2>$null | Out-Null

    # Now register with codebase
    & $regAsm "$dllPath" /codebase

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Shell extension registered successfully" -ForegroundColor Green
        Write-Host ""

        # Restart Explorer
        Write-Host "Restarting Windows Explorer..." -ForegroundColor Yellow

        # Kill all explorer processes
        Get-Process explorer -ErrorAction SilentlyContinue | Stop-Process -Force
        Start-Sleep -Seconds 2

        # Start new explorer
        Start-Process explorer

        Write-Host "✓ Windows Explorer restarted" -ForegroundColor Green
        Write-Host ""

        # Show test instructions
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "        Ready for Testing!              " -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "TEST INSTRUCTIONS:" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "1. Copy this sample Markdown text:" -ForegroundColor White
        Write-Host ""
        Write-Host "   # Hello World" -ForegroundColor Gray
        Write-Host "   **This is bold** and *this is italic*" -ForegroundColor Gray
        Write-Host "   " -ForegroundColor Gray
        Write-Host '   ```python' -ForegroundColor Gray
        Write-Host "   def hello():" -ForegroundColor Gray
        Write-Host '       print("Hello paste-md!")' -ForegroundColor Gray
        Write-Host '   ```' -ForegroundColor Gray
        Write-Host ""
        Write-Host "2. Open Microsoft Word or OneNote" -ForegroundColor White
        Write-Host ""
        Write-Host "3. Right-click in the document" -ForegroundColor White
        Write-Host ""
        Write-Host "4. Look for 'Paste as Rendered Markdown' in the context menu" -ForegroundColor White
        Write-Host ""
        Write-Host "5. Click it to see the formatted result!" -ForegroundColor White
        Write-Host ""
        Write-Host "LOG FILE:" -ForegroundColor Yellow
        Write-Host "$env:LOCALAPPDATA\paste-md\paste-md.log" -ForegroundColor Gray
        Write-Host ""
        Write-Host "To unregister later, run:" -ForegroundColor Yellow
        Write-Host ".\tools\register-test.ps1 -Unregister" -ForegroundColor Gray

    } else {
        Write-Host "✗ Failed to register shell extension" -ForegroundColor Red
        Write-Host "Make sure you're running as Administrator" -ForegroundColor Yellow
        exit 1
    }
}