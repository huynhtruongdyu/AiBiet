# AiBiet CLI Installer for Windows
# Usage: .\install.ps1

$ErrorActionPreference = "Stop"

Write-Host "----------------------------------" -ForegroundColor Cyan
Write-Host "   AiBiet CLI Installer (Win)     " -ForegroundColor Cyan
Write-Host "----------------------------------" -ForegroundColor Cyan

# 1. Check for .NET SDK
if (!(Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "[ERROR] .NET SDK not found." -ForegroundColor Red
    Write-Host "Please install .NET 10 or later from: https://dotnet.microsoft.com/download"
    exit 1
}

# 2. Pack the project
$distPath = Join-Path $PSScriptRoot "dist"
Write-Host "[1/3] Building and packaging AiBiet.CLI..." -ForegroundColor Green
dotnet pack src/AiBiet.CLI/AiBiet.CLI.csproj -c Release -o $distPath --nologo -v q

# 3. Install or Update
$isInstalled = dotnet tool list -g | Select-String "aibiet.cli"

if ($isInstalled) {
    Write-Host "[2/3] Updating AiBiet CLI..." -ForegroundColor Green
    dotnet tool update --global AiBiet.CLI --add-source $distPath
} else {
    Write-Host "[2/3] Installing AiBiet CLI..." -ForegroundColor Green
    dotnet tool install --global AiBiet.CLI --add-source $distPath
}

# 4. Cleanup
Write-Host "[3/3] Cleaning up..." -ForegroundColor Gray
if (Test-Path $distPath) {
    Remove-Item $distPath -Recurse -Force
}

Write-Host ""
Write-Host "SUCCESS: AiBiet CLI is ready!" -ForegroundColor Green
Write-Host "Try running: " -NoNewline; Write-Host "aibiet" -ForegroundColor Yellow
Write-Host ""
