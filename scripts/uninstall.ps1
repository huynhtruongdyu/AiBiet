# AiBiet CLI Uninstaller for Windows
# Usage: .\uninstall.ps1

$ErrorActionPreference = "Stop"

Write-Host "----------------------------------" -ForegroundColor Yellow
Write-Host "   AiBiet CLI Uninstaller (Win)   " -ForegroundColor Yellow
Write-Host "----------------------------------" -ForegroundColor Yellow

# 1. Uninstall the tool
$isInstalled = dotnet tool list -g | Select-String "aibiet.cli"

if ($isInstalled) {
    Write-Host "[1/2] Uninstalling AiBiet CLI tool..." -ForegroundColor Green
    dotnet tool uninstall --global AiBiet.CLI
} else {
    Write-Host "[1/2] AiBiet CLI is not installed globally." -ForegroundColor Gray
}

# 2. Optional: Clean up configuration
$configDir = Join-Path $env:USERPROFILE ".aibiet"
if (Test-Path $configDir) {
    $confirmation = Read-Host "Do you want to delete the configuration folder and all settings? ($configDir) [y/N]"
    if ($confirmation -eq 'y' -or $confirmation -eq 'Y') {
        Write-Host "[2/2] Removing configuration directory..." -ForegroundColor Green
        try {
            Remove-Item $configDir -Recurse -Force
        } catch {
            Write-Host "[ERROR] Could not remove configuration directory." -ForegroundColor Red
            Write-Host "Please ensure no other applications (like VS Code or another Terminal) are using the files in: $configDir" -ForegroundColor Yellow
            Write-Host "Error Details: $($_.Exception.Message)" -ForegroundColor Gray
        }
    } else {
        Write-Host "[2/2] Keeping configuration directory." -ForegroundColor Gray
    }
} else {
    Write-Host "[2/2] No configuration directory found." -ForegroundColor Gray
}

Write-Host ""
Write-Host "SUCCESS: AiBiet CLI has been uninstalled." -ForegroundColor Green
Write-Host ""
