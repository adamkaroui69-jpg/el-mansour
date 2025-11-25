# Update Application Version
# Updates version in all relevant files

param(
    [string]$Version = "1.0.0.0"
)

$ErrorActionPreference = "Stop"

Write-Host "Updating application version to $Version..." -ForegroundColor Green

$ProjectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

# Update .csproj file
$csprojPath = Join-Path $ProjectRoot "src\ElMansourSyndicManager\ElMansourSyndicManager.csproj"
if (Test-Path $csprojPath) {
    $content = Get-Content $csprojPath -Raw
    $content = $content -replace '<Version>.*</Version>', "<Version>$Version</Version>"
    $content | Set-Content $csprojPath
    Write-Host "✓ Updated .csproj" -ForegroundColor Green
}

# Update appsettings.json
$appSettingsPath = Join-Path $ProjectRoot "src\ElMansourSyndicManager\appsettings.json"
if (Test-Path $appSettingsPath) {
    $config = Get-Content $appSettingsPath | ConvertFrom-Json
    $config.Application.Version = $Version
    $config | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath -Encoding UTF8
    Write-Host "✓ Updated appsettings.json" -ForegroundColor Green
}

# Update MSIX manifest
$manifestPath = Join-Path $ProjectRoot "deployment\MSIX\Package.appxmanifest"
if (Test-Path $manifestPath) {
    $content = Get-Content $manifestPath -Raw
    $content = $content -replace 'Version="[^"]*"', "Version=`"$Version`""
    $content | Set-Content $manifestPath
    Write-Host "✓ Updated Package.appxmanifest" -ForegroundColor Green
}

Write-Host "`nVersion updated to $Version" -ForegroundColor Green

