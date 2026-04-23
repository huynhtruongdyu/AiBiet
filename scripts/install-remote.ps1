# AiBiet Remote Installer (No .NET Required)
# Downloads the latest pre-built native binary from GitHub Releases.
# Usage:
#   iex (irm https://raw.githubusercontent.com/huynhtruongdyu/AiBiet/main/scripts/install-remote.ps1)
#   iex (irm https://raw.githubusercontent.com/huynhtruongdyu/AiBiet/main/scripts/install-remote.ps1) -Version v0.1.1
#   iex (irm https://raw.githubusercontent.com/huynhtruongdyu/AiBiet/main/scripts/install-remote.ps1) -PreRelease

# Support both direct execution and iex (irm ...) pattern
param(
    [string]$Version = $env:AIBIET_INSTALL_VERSION,
    [switch]$PreRelease = [bool]$env:AIBIET_INSTALL_PRERELEASE
)

# If called via iex (irm ...), parameters might not work, so check env vars
if (-not $Version -and $args -match '-Version') {
    $Version = ($args -split '-Version')[1].Trim().Split()[0].Trim('-').Trim()
}
if (-not $PreRelease -and ($args -match '-PreRelease' -or $args -match '-prerelease')) {
    $PreRelease = $true
}

$ErrorActionPreference = "Stop"

$Repo       = "huynhtruongdyu/AiBiet"
$AssetName  = "aibiet-win-x64.zip"
$BinaryName = "aibiet.exe"
$InstallDir = Join-Path $env:USERPROFILE ".aibiet\bin"

Write-Host "----------------------------------" -ForegroundColor Cyan
Write-Host "   AiBiet Online Installer        " -ForegroundColor Cyan
Write-Host "----------------------------------" -ForegroundColor Cyan

# 1. Fetch release info from GitHub API
Write-Host "[1/4] Fetching release..." -ForegroundColor Green

if ($Version) {
    $releaseUrl = "https://api.github.com/repos/$Repo/releases/tags/$Version"
} elseif ($PreRelease) {
    $releasesUrl = "https://api.github.com/repos/$Repo/releases?per_page=100"
    try {
        $releases = Invoke-RestMethod -Uri $releasesUrl -Headers @{ "User-Agent" = "AiBiet-Installer" }
        $release = $releases | Where-Object { $_.prerelease -eq $true } | Select-Object -First 1
        if (-not $release) {
            Write-Host "[ERROR] No pre-release found." -ForegroundColor Red
            exit 1
        }
    } catch {
        Write-Host "[ERROR] Could not fetch releases from GitHub." -ForegroundColor Red
        Write-Host "Check your internet connection or visit: https://github.com/$Repo/releases"
        exit 1
    }
} else {
    # Fetch latest non-prerelease release
    $releasesUrl = "https://api.github.com/repos/$Repo/releases?per_page=100"
    try {
        $releases = Invoke-RestMethod -Uri $releasesUrl -Headers @{ "User-Agent" = "AiBiet-Installer" }
        $release = $releases | Where-Object { $_.prerelease -eq $false } | Select-Object -First 1
        if (-not $release) {
            Write-Host "[ERROR] No stable release found. Use -PreRelease flag to install pre-releases." -ForegroundColor Red
            Write-Host "Visit https://github.com/$Repo/releases to view available releases."
            exit 1
        }
    } catch {
        Write-Host "[ERROR] Could not fetch releases from GitHub." -ForegroundColor Red
        Write-Host "Check your internet connection or visit: https://github.com/$Repo/releases"
        exit 1
    }
}

if (-not $release) {
    Write-Host "[ERROR] Could not determine release." -ForegroundColor Red
    exit 1
}

$version = $release.tag_name
Write-Host "  Found version: $version" -ForegroundColor Gray

# 2. Find the Windows asset download URL
$asset = $release.assets | Where-Object { $_.name -eq $AssetName } | Select-Object -First 1
if (-not $asset) {
    Write-Host "[ERROR] Could not find '$AssetName' in release $version." -ForegroundColor Red
    Write-Host "Visit https://github.com/$Repo/releases to download manually."
    exit 1
}

# 3. Download and extract
Write-Host "[2/4] Downloading $AssetName ($version)..." -ForegroundColor Green
$tempZip = Join-Path $env:TEMP "aibiet-install-$version.zip"
$tempExtract = Join-Path $env:TEMP "aibiet-install-$version"

try {
    Invoke-WebRequest -Uri $asset.browser_download_url -OutFile $tempZip -UseBasicParsing
} catch {
    Write-Host "[ERROR] Download failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "[3/4] Installing to $InstallDir..." -ForegroundColor Green
if (Test-Path $tempExtract) { Remove-Item $tempExtract -Recurse -Force }
Expand-Archive -Path $tempZip -DestinationPath $tempExtract -Force

# Move binary to install dir
if (-not (Test-Path $InstallDir)) {
    New-Item -ItemType Directory -Path $InstallDir | Out-Null
}
$binarySource = Join-Path $tempExtract $BinaryName
Copy-Item -Path $binarySource -Destination (Join-Path $InstallDir $BinaryName) -Force

# 4. Add InstallDir to User PATH if not already present
Write-Host "[4/4] Updating PATH..." -ForegroundColor Green
$userPath = [System.Environment]::GetEnvironmentVariable("PATH", "User")
if ($userPath -notlike "*$InstallDir*") {
    [System.Environment]::SetEnvironmentVariable("PATH", "$userPath;$InstallDir", "User")
    Write-Host "  Added '$InstallDir' to your PATH." -ForegroundColor Gray
    Write-Host "  Please restart your terminal for the PATH change to take effect." -ForegroundColor Yellow
} else {
    Write-Host "  '$InstallDir' is already in your PATH." -ForegroundColor Gray
}

# Cleanup temp files
Remove-Item $tempZip -Force -ErrorAction SilentlyContinue
Remove-Item $tempExtract -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "SUCCESS: AiBiet $version is ready!" -ForegroundColor Green
Write-Host "Try running: " -NoNewline; Write-Host "aibiet" -ForegroundColor Yellow
Write-Host "(You may need to restart your terminal first)" -ForegroundColor Gray
Write-Host ""
