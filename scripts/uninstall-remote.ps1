# AiBiet Remote Uninstaller (No .NET Required)
# Usage: iex (irm https://raw.githubusercontent.com/huynhtruongdyu/AiBiet/main/scripts/uninstall-remote.ps1)

$ErrorActionPreference = "Stop"

$BinaryName = "aibiet.exe"
$InstallDir = Join-Path $env:USERPROFILE ".aibiet\bin"
$ConfigDir  = Join-Path $env:USERPROFILE ".aibiet"

Write-Host "----------------------------------" -ForegroundColor Yellow
Write-Host "   AiBiet Online Uninstaller      " -ForegroundColor Yellow
Write-Host "----------------------------------" -ForegroundColor Yellow

# 1. Uninstall .NET Global Tool
Write-Host "[1/4] Checking for .NET Global Tool..." -ForegroundColor Green
$dotnetTool = dotnet tool list -g 2>&1 | Select-String "aibiet.cli"
if ($dotnetTool) {
    Write-Host "  Found .NET Global Tool 'aibiet.cli'. Uninstalling..." -ForegroundColor Gray
    dotnet tool uninstall -g aibiet.cli 2>&1 | Out-Null
    Write-Host "  Removed .NET Global Tool." -ForegroundColor Gray
} else {
    Write-Host "  .NET Global Tool not found. Skipping." -ForegroundColor Gray
}

# 2. Remove the binary
$binaryPath = Join-Path $InstallDir $BinaryName
if (Test-Path $binaryPath) {
    Write-Host "[2/4] Removing AiBiet binary..." -ForegroundColor Green
    try {
        Remove-Item $binaryPath -Force
        Write-Host "  Removed: $binaryPath" -ForegroundColor Gray
    } catch {
        Write-Host "[ERROR] Could not remove the binary. It may be in use." -ForegroundColor Red
        Write-Host "  Path: $binaryPath" -ForegroundColor Yellow
        Write-Host "  Please close any running instances of AiBiet and try again." -ForegroundColor Yellow
    }
} else {
    Write-Host "[2/4] AiBiet binary not found at $binaryPath. Skipping." -ForegroundColor Gray
}

# Remove the bin directory if empty
if ((Test-Path $InstallDir) -and (Get-ChildItem $InstallDir -ErrorAction SilentlyContinue).Count -eq 0) {
    Remove-Item $InstallDir -Force -ErrorAction SilentlyContinue
}

# 3. Remove InstallDir from User PATH
Write-Host "[3/4] Cleaning up PATH..." -ForegroundColor Green
$userPath = [System.Environment]::GetEnvironmentVariable("PATH", "User")
if ($userPath -like "*$InstallDir*") {
    $newPath = ($userPath -split ";" | Where-Object { $_ -ne $InstallDir }) -join ";"
    [System.Environment]::SetEnvironmentVariable("PATH", $newPath, "User")
    Write-Host "  Removed '$InstallDir' from your PATH." -ForegroundColor Gray
} else {
    Write-Host "  '$InstallDir' was not found in PATH. Skipping." -ForegroundColor Gray
}

# 4. Optionally remove config
Write-Host "[4/4] Configuration cleanup..." -ForegroundColor Green
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
