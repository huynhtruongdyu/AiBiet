# AiBiet Native AOT Publisher
# Usage: .\scripts\publish.ps1 [-Runtime <win-x64|linux-x64|osx-arm64>]

param (
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

Write-Host "----------------------------------" -ForegroundColor Cyan
Write-Host "   AiBiet Native AOT Publisher    " -ForegroundColor Cyan
Write-Host "   Target: $Runtime ($Configuration)" -ForegroundColor Cyan
Write-Host "----------------------------------" -ForegroundColor Cyan

# 1. Check for .NET SDK
if (!(Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "[ERROR] .NET SDK not found." -ForegroundColor Red
    exit 1
}

# 2. Define Paths
$projectPath = Join-Path $PSScriptRoot "..\src\AiBiet.CLI\AiBiet.CLI.csproj"
$distPath = Join-Path $PSScriptRoot "..\dist\native\$Runtime"

if (Test-Path $distPath) {
    Write-Host "Cleaning previous build..." -ForegroundColor Gray
    Remove-Item $distPath -Recurse -Force
}

# 3. Publish
Write-Host "Building standalone self-contained binary..." -ForegroundColor Green
dotnet publish $projectPath `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -o $distPath `
    /p:PublishSingleFile=true `
    /p:DebugType=None `
    /p:DebugSymbols=false

if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Publish failed." -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "SUCCESS! Native binary is ready." -ForegroundColor Green
Write-Host "Location: " -NoNewline; Write-Host "$distPath" -ForegroundColor Yellow
Write-Host ""

if ($Runtime.StartsWith("win")) {
    $exeName = "AiBiet.exe"
    if (Test-Path (Join-Path $distPath "AiBiet.CLI.exe")) { $exeName = "AiBiet.CLI.exe" }
    
    Write-Host "To test it, run:" -ForegroundColor Gray
    Write-Host "  $distPath\$exeName" -ForegroundColor White
}
