# AiBiet Remote Installer
# This script clones the repository to a temporary folder and runs the installer.
# Usage: iex (irm https://raw.githubusercontent.com/huynhtruongdyu/AiBiet/main/install-remote.ps1)

$ErrorActionPreference = "Stop"

$tempDir = Join-Path $env:TEMP "AiBiet_Install_$(Get-Random)"
if (Test-Path $tempDir) { Remove-Item $tempDir -Recurse -Force }
New-Item -ItemType Directory -Path $tempDir | Out-Null

# Check for .NET SDK early
if (!(Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "[ERROR] .NET SDK not found." -ForegroundColor Red
    Write-Host "Please install .NET 10 or later from: https://dotnet.microsoft.com/download"
    exit 1
}

$dotnetVersion = dotnet --version
if ($dotnetVersion -match '^(\d+)') {
    $major = [int]$matches[1]
    if ($major -lt 10) {
        Write-Host "[ERROR] .NET SDK 10 or later is required. You have version $dotnetVersion." -ForegroundColor Red
        Write-Host "Please update from: https://dotnet.microsoft.com/download"
        exit 1
    }
}

Write-Host "--- AiBiet Online Installer ---" -ForegroundColor Cyan

try {
    Write-Host "Downloading source code..." -ForegroundColor Gray
    if (Get-Command git -ErrorAction SilentlyContinue) {
        & git clone --depth 1 https://github.com/huynhtruongdyu/AiBiet.git $tempDir
    } else {
        $zipFile = Join-Path $tempDir "repo.zip"
        Invoke-WebRequest -Uri "https://github.com/huynhtruongdyu/AiBiet/archive/refs/heads/main.zip" -OutFile $zipFile
        Expand-Archive -Path $zipFile -DestinationPath $tempDir -Force
        
        # Move files up from the extracted subfolder
        $extractedFolder = Get-ChildItem $tempDir -Directory | Where-Object { $_.Name -like "AiBiet-*" } | Select-Object -First 1
        if ($extractedFolder) {
            Get-ChildItem $extractedFolder.FullName | Move-Item -Destination $tempDir -Force
        }
    }

    if (Test-Path (Join-Path $tempDir "install.ps1")) {
        Push-Location $tempDir
        try {
            .\install.ps1
        } finally {
            Pop-Location
        }
    } else {
        Write-Error "Could not find install.ps1 in the downloaded repository."
    }
} catch {
    Write-Host "[ERROR] Installation failed: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    Write-Host "Cleaning up temporary files..." -ForegroundColor Gray
    if (Test-Path $tempDir) {
        Remove-Item $tempDir -Recurse -Force
    }
}
