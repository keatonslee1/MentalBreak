# update-version.ps1
# Run this AFTER completing a Unity WebGL build to update version tracking
#
# Usage:
#   .\update-version.ps1              # Auto-increment build number
#   .\update-version.ps1 -Bump minor  # Bump minor version (4.0.0 -> 4.1.0)
#   .\update-version.ps1 -Bump major  # Bump major version (4.0.0 -> 5.0.0)
#   .\update-version.ps1 -Version "4.1.0"  # Set specific version

param(
    [string]$Bump = "",
    [string]$Version = ""
)

$ErrorActionPreference = "Stop"

$projectRoot = $PSScriptRoot
$webglBuild = Join-Path $projectRoot "unity-project\webgl-build"
$versionFile = Join-Path $webglBuild "version.json"

# HTML files to update
$htmlFiles = @(
    (Join-Path $webglBuild "index.html"),
    (Join-Path $webglBuild "play.html"),
    (Join-Path $webglBuild "mobile-play.html")
)

Write-Host "=== Mental Break Version Updater ===" -ForegroundColor Cyan

# Read current version
if (Test-Path $versionFile) {
    $versionData = Get-Content $versionFile -Raw | ConvertFrom-Json
    $currentVersion = $versionData.version
    $buildNumber = $versionData.buildNumber
} else {
    $currentVersion = "4.0.0"
    $buildNumber = 0
}

Write-Host "Current version: $currentVersion (build $buildNumber)"

# Calculate new version
if ($Version) {
    $newVersion = $Version
    $buildNumber = 1
} elseif ($Bump -eq "major") {
    $parts = $currentVersion.Split('.')
    $newVersion = "$([int]$parts[0] + 1).0.0"
    $buildNumber = 1
} elseif ($Bump -eq "minor") {
    $parts = $currentVersion.Split('.')
    $newVersion = "$($parts[0]).$([int]$parts[1] + 1).0"
    $buildNumber = 1
} elseif ($Bump -eq "patch") {
    $parts = $currentVersion.Split('.')
    $newVersion = "$($parts[0]).$($parts[1]).$([int]$parts[2] + 1)"
    $buildNumber = 1
} else {
    # Default: increment build number
    $newVersion = $currentVersion
    $buildNumber++
}

# Generate build hash from timestamp + random
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$random = -join ((65..90) + (97..122) | Get-Random -Count 4 | ForEach-Object {[char]$_})
$buildHash = "$timestamp-$random"

# Create version object
$newVersionData = @{
    version = $newVersion
    buildNumber = $buildNumber
    buildTime = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    buildHash = $buildHash
}

# Write version.json
$json = $newVersionData | ConvertTo-Json -Depth 10
Set-Content -Path $versionFile -Value $json -Encoding UTF8
Write-Host "Updated version.json: $newVersion (build $buildNumber)" -ForegroundColor Green

# Update HTML files
foreach ($htmlFile in $htmlFiles) {
    if (Test-Path $htmlFile) {
        $content = Get-Content $htmlFile -Raw

        # Update productVersion in JavaScript
        $content = $content -replace 'productVersion:\s*"[^"]*"', "productVersion: `"$newVersion`""

        # Update version comment at top
        $content = $content -replace '<!-- Version: [^|]+\|', "<!-- Version: $newVersion |"

        Set-Content -Path $htmlFile -Value $content -Encoding UTF8
        Write-Host "Updated: $(Split-Path $htmlFile -Leaf)" -ForegroundColor Green
    }
}

# Update index.html title and meta tags
$indexHtml = Join-Path $webglBuild "index.html"
if (Test-Path $indexHtml) {
    $content = Get-Content $indexHtml -Raw

    # Update title (Alpha v3.5 -> v4.0.0)
    $content = $content -replace '<title>Mental Break [^<]*</title>', "<title>Mental Break v$newVersion</title>"
    $content = $content -replace 'og:title" content="Mental Break [^"]*"', "og:title`" content=`"Mental Break v$newVersion`""
    $content = $content -replace 'twitter:title" content="Mental Break [^"]*"', "twitter:title`" content=`"Mental Break v$newVersion`""

    Set-Content -Path $indexHtml -Value $content -Encoding UTF8
    Write-Host "Updated index.html meta tags" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== Version Update Complete ===" -ForegroundColor Cyan
Write-Host "New version: $newVersion (build $buildNumber)" -ForegroundColor Yellow
Write-Host "Build hash: $buildHash" -ForegroundColor Yellow
Write-Host ""
Write-Host "Next steps:" -ForegroundColor White
Write-Host "  1. git add ." -ForegroundColor Gray
Write-Host "  2. git commit -m 'Deploy v$newVersion'" -ForegroundColor Gray
Write-Host "  3. Push via GitHub Desktop" -ForegroundColor Gray
