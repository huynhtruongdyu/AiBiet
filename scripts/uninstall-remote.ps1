# AiBiet Remote Uninstaller
# Usage: iex (irm https://raw.githubusercontent.com/huynhtruongdyu/AiBiet/main/scripts/uninstall-remote.ps1)

$ErrorActionPreference = "Stop"

Write-Host "--- AiBiet Online Uninstaller ---" -ForegroundColor Yellow

# 1. Uninstall the tool
$isInstalled = dotnet tool list -g | Select-String "aibiet.cli"

if ($isInstalled) {
    Write-Host "Uninstalling AiBiet CLI tool..." -ForegroundColor Green
    dotnet tool uninstall --global AiBiet.CLI
} else {
    Write-Host "AiBiet CLI is not installed globally." -ForegroundColor Gray
}

# 2. Optional: Clean up configuration
$configDir = Join-Path $env:USERPROFILE ".aibiet"
if (Test-Path $configDir) {
    Write-Host ""
    Write-Host "Configuration found at: $configDir" -ForegroundColor Cyan
    $confirmation = Read-Host "Do you want to delete the configuration folder and all settings? [y/N]"
    if ($confirmation -eq 'y' -or $confirmation -eq 'Y') {
        Write-Host "Removing configuration directory..." -ForegroundColor Green
        try {
            Remove-Item $configDir -Recurse -Force
        } catch {
            Write-Host "[ERROR] Could not remove configuration directory." -ForegroundColor Red
            Write-Host "Please ensure no other applications (like VS Code or another Terminal) are using the files in: $configDir" -ForegroundColor Yellow
            Write-Host "Error Details: $($_.Exception.Message)" -ForegroundColor Gray
        }
    } else {
        Write-Host "Keeping configuration directory." -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "SUCCESS: AiBiet CLI has been uninstalled." -ForegroundColor Green
