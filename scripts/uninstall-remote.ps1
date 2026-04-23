# AiBiet Remote Uninstaller (No .NET Required)
# Usage: iex (irm https://raw.githubusercontent.com/huynhtruongdyu/AiBiet/main/scripts/uninstall-remote.ps1)

$ErrorActionPreference = "Stop"

$BinaryName = "aibiet.exe"
$InstallDir = Join-Path $env:USERPROFILE ".aibiet\bin"
$ConfigDir  = Join-Path $env:USERPROFILE ".aibiet"

Write-Host "----------------------------------" -ForegroundColor Yellow
Write-Host "   AiBiet Online Uninstaller      " -ForegroundColor Yellow
Write-Host "----------------------------------" -ForegroundColor Yellow

# 1. Remove the binary
$binaryPath = Join-Path $InstallDir $BinaryName
if (Test-Path $binaryPath) {
    Write-Host "[1/3] Removing AiBiet binary..." -ForegroundColor Green
    try {
        Remove-Item $binaryPath -Force
        Write-Host "  Removed: $binaryPath" -ForegroundColor Gray
    } catch {
        Write-Host "[ERROR] Could not remove the binary. It may be in use." -ForegroundColor Red
        Write-Host "  Path: $binaryPath" -ForegroundColor Yellow
        Write-Host "  Please close any running instances of AiBiet and try again." -ForegroundColor Yellow
    }
} else {
    Write-Host "[1/3] AiBiet binary not found at $binaryPath. Skipping." -ForegroundColor Gray
}

# Remove the bin directory if empty
if ((Test-Path $InstallDir) -and (Get-ChildItem $InstallDir -ErrorAction SilentlyContinue).Count -eq 0) {
    Remove-Item $InstallDir -Force -ErrorAction SilentlyContinue
}

# 2. Remove InstallDir from User PATH
Write-Host "[2/3] Cleaning up PATH..." -ForegroundColor Green
$userPath = [System.Environment]::GetEnvironmentVariable("PATH", "User")
if ($userPath -like "*$InstallDir*") {
    $newPath = ($userPath -split ";" | Where-Object { $_ -ne $InstallDir }) -join ";"
    [System.Environment]::SetEnvironmentVariable("PATH", $newPath, "User")
    Write-Host "  Removed '$InstallDir' from your PATH." -ForegroundColor Gray
} else {
    Write-Host "  '$InstallDir' was not found in PATH. Skipping." -ForegroundColor Gray
}

# 3. Optionally remove config
Write-Host "[3/3] Configuration cleanup..." -ForegroundColor Green
if (Test-Path $ConfigDir) {
    Write-Host ""
    Write-Host "Configuration found at: $ConfigDir" -ForegroundColor Cyan
    $confirmation = Read-Host "Do you want to delete all AiBiet settings and configuration? [y/N]"
    if ($confirmation -eq 'y' -or $confirmation -eq 'Y') {
        try {
            Remove-Item $ConfigDir -Recurse -Force
            Write-Host "  Configuration removed." -ForegroundColor Gray
        } catch {
            Write-Host "[ERROR] Could not remove configuration directory." -ForegroundColor Red
            Write-Host "  Please ensure no applications are using files in: $ConfigDir" -ForegroundColor Yellow
            Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Gray
        }
    } else {
        Write-Host "  Configuration kept at: $ConfigDir" -ForegroundColor Gray
    }
} else {
    Write-Host "  No configuration directory found. Skipping." -ForegroundColor Gray
}

Write-Host ""
Write-Host "SUCCESS: AiBiet has been uninstalled." -ForegroundColor Green
Write-Host "(Restart your terminal for PATH changes to take effect)" -ForegroundColor Gray
Write-Host ""
