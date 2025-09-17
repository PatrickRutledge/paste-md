# paste-md Build Script
# Builds the paste-md shell extension and optionally creates installer

param(
    [Parameter()]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [Parameter()]
    [switch]$CreateInstaller = $false,

    [Parameter()]
    [switch]$SignBinaries = $false,

    [Parameter()]
    [string]$CertificatePath = "",

    [Parameter()]
    [string]$CertificatePassword = "",

    [Parameter()]
    [switch]$RegisterLocal = $false,

    [Parameter()]
    [switch]$Clean = $false
)

$ErrorActionPreference = "Stop"

# Paths
$RootDir = Split-Path -Parent $PSScriptRoot
$SrcDir = Join-Path $RootDir "src"
$ProjectFile = Join-Path $SrcDir "paste-md.Core\paste-md.Core.csproj"
$OutputDir = Join-Path $RootDir "build\$Configuration"
$BinDir = Join-Path $OutputDir "bin"
$InstallerDir = Join-Path $RootDir "installer"
$ToolsDir = Join-Path $RootDir "tools"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "       paste-md Build Script            " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Clean if requested
if ($Clean) {
    Write-Host "Cleaning build directory..." -ForegroundColor Yellow
    if (Test-Path $OutputDir) {
        Remove-Item -Path $OutputDir -Recurse -Force
    }
    Write-Host "Clean complete." -ForegroundColor Green
}

# Check for .NET SDK
Write-Host "Checking for .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "Found .NET SDK version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "ERROR: .NET SDK not found. Please install .NET 6.0 SDK or later." -ForegroundColor Red
    exit 1
}

# Create output directory
if (!(Test-Path $OutputDir)) {
    New-Item -Path $OutputDir -ItemType Directory | Out-Null
}
if (!(Test-Path $BinDir)) {
    New-Item -Path $BinDir -ItemType Directory | Out-Null
}

# Restore packages
Write-Host ""
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore $ProjectFile
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Package restore failed." -ForegroundColor Red
    exit 1
}
Write-Host "Package restore complete." -ForegroundColor Green

# Build the project
Write-Host ""
Write-Host "Building paste-md.Core ($Configuration)..." -ForegroundColor Yellow
dotnet build $ProjectFile --configuration $Configuration --output $BinDir --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed." -ForegroundColor Red
    exit 1
}
Write-Host "Build complete." -ForegroundColor Green

# Sign binaries if requested
if ($SignBinaries) {
    Write-Host ""
    Write-Host "Signing binaries..." -ForegroundColor Yellow

    if (![string]::IsNullOrEmpty($CertificatePath) -and (Test-Path $CertificatePath)) {
        $mainDll = Join-Path $BinDir "paste-md.Core.dll"

        # Find signtool
        $signtool = "${env:ProgramFiles(x86)}\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe"
        if (!(Test-Path $signtool)) {
            $signtool = "${env:ProgramFiles(x86)}\Windows Kits\10\bin\10.0.19041.0\x64\signtool.exe"
        }

        if (Test-Path $signtool) {
            & $signtool sign /f $CertificatePath /p $CertificatePassword /t http://timestamp.digicert.com $mainDll
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Binary signed successfully." -ForegroundColor Green
            } else {
                Write-Host "WARNING: Binary signing failed." -ForegroundColor Yellow
            }
        } else {
            Write-Host "WARNING: signtool.exe not found. Skipping signing." -ForegroundColor Yellow
        }
    } else {
        Write-Host "WARNING: Certificate not found. Skipping signing." -ForegroundColor Yellow
    }
}

# Register locally for testing (Debug mode only)
if ($RegisterLocal -and $Configuration -eq "Debug") {
    Write-Host ""
    Write-Host "Registering shell extension locally for testing..." -ForegroundColor Yellow

    $regasm = "${env:ProgramFiles(x86)}\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\x64\RegAsm.exe"
    if (!(Test-Path $regasm)) {
        $regasm = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe"
    }

    if (Test-Path $regasm) {
        $mainDll = Join-Path $BinDir "paste-md.Core.dll"

        # Unregister first if already registered
        Start-Process -FilePath $regasm -ArgumentList "/u `"$mainDll`"" -Wait -NoNewWindow -PassThru | Out-Null

        # Register with codebase
        $process = Start-Process -FilePath $regasm -ArgumentList "`"$mainDll`" /codebase" -Wait -NoNewWindow -PassThru
        if ($process.ExitCode -eq 0) {
            Write-Host "Shell extension registered successfully." -ForegroundColor Green
            Write-Host "Restarting Windows Explorer..." -ForegroundColor Yellow

            # Restart Explorer
            Stop-Process -Name explorer -Force -ErrorAction SilentlyContinue
            Start-Sleep -Seconds 2
            Start-Process explorer

            Write-Host "Windows Explorer restarted." -ForegroundColor Green
            Write-Host ""
            Write-Host "TESTING INSTRUCTIONS:" -ForegroundColor Cyan
            Write-Host "1. Copy some Markdown text (e.g., **bold** or # Header)" -ForegroundColor White
            Write-Host "2. Right-click in any text editor" -ForegroundColor White
            Write-Host "3. Look for 'Paste as Rendered Markdown' in the context menu" -ForegroundColor White
        } else {
            Write-Host "WARNING: Registration failed. Try running as Administrator." -ForegroundColor Yellow
        }
    } else {
        Write-Host "WARNING: RegAsm.exe not found. Cannot register locally." -ForegroundColor Yellow
    }
}

# Create installer if requested
if ($CreateInstaller) {
    Write-Host ""
    Write-Host "Creating installer..." -ForegroundColor Yellow

    # Check for WiX Toolset
    $wixPath = "${env:ProgramFiles(x86)}\WiX Toolset v4\bin"
    if (!(Test-Path $wixPath)) {
        $wixPath = "${env:ProgramFiles}\WiX Toolset v4\bin"
    }

    if (Test-Path $wixPath) {
        $env:Path += ";$wixPath"

        # Copy License.rtf if it doesn't exist
        $licenseFile = Join-Path $InstallerDir "License.rtf"
        if (!(Test-Path $licenseFile)) {
            Write-Host "Creating default license file..." -ForegroundColor Yellow
            @"
{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033{\fonttbl{\f0\fnil\fcharset0 Segoe UI;}}
{\*\generator Riched20 10.0.19041}\viewkind4\uc1
\pard\sa200\sl276\slmult1\f0\fs22\lang9 paste-md License Agreement\par
\par
Copyright (c) 2025 paste-md\par
\par
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:\par
\par
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.\par
\par
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.\par
}
"@ | Out-File -FilePath $licenseFile -Encoding ASCII
        }

        $wixFile = Join-Path $InstallerDir "Product.wxs"
        $wixObj = Join-Path $OutputDir "Product.wixobj"
        $msiFile = Join-Path $OutputDir "paste-md-$Configuration-x64.msi"

        # Compile WiX source
        & candle.exe -dProjectDir=$RootDir -dConfiguration=$Configuration -arch x64 -out $wixObj $wixFile
        if ($LASTEXITCODE -eq 0) {
            # Link to create MSI
            & light.exe -ext WixUIExtension -ext WixNetFxExtension -out $msiFile $wixObj
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Installer created successfully: $msiFile" -ForegroundColor Green

                if ($SignBinaries -and (Test-Path $signtool)) {
                    Write-Host "Signing installer..." -ForegroundColor Yellow
                    & $signtool sign /f $CertificatePath /p $CertificatePassword /t http://timestamp.digicert.com $msiFile
                    if ($LASTEXITCODE -eq 0) {
                        Write-Host "Installer signed successfully." -ForegroundColor Green
                    }
                }
            } else {
                Write-Host "ERROR: Failed to create MSI installer." -ForegroundColor Red
                exit 1
            }
        } else {
            Write-Host "ERROR: Failed to compile WiX source." -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "WARNING: WiX Toolset not found. Skipping installer creation." -ForegroundColor Yellow
        Write-Host "Download WiX Toolset from: https://wixtoolset.org/releases/" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "       Build Complete!                  " -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Output Directory: $OutputDir" -ForegroundColor Cyan
Write-Host ""

if ($Configuration -eq "Debug" -and !$RegisterLocal) {
    Write-Host "TIP: Use -RegisterLocal to register the shell extension for testing" -ForegroundColor Yellow
}
if (!$CreateInstaller) {
    Write-Host "TIP: Use -CreateInstaller to build the MSI installer" -ForegroundColor Yellow
}