# Validate Deployment - Run after installation
# Checks all components are properly configured

$ErrorActionPreference = "Stop"

Write-Host "Validating El Mansour Syndic Manager deployment..." -ForegroundColor Green

$errors = @()
$warnings = @()

# Check application files
Write-Host "`nChecking application files..." -ForegroundColor Yellow
$appPath = "$env:LOCALAPPDATA\ElMansourSyndicManager"
if (-not (Test-Path $appPath)) {
    $errors += "Application directory not found: $appPath"
} else {
    Write-Host "✓ Application directory exists" -ForegroundColor Green
}

# Check database
Write-Host "`nChecking database..." -ForegroundColor Yellow
$dbPath = Join-Path $appPath "data\local.db"
if (-not (Test-Path $dbPath)) {
    $warnings += "Database file not found: $dbPath (will be created on first launch)"
} else {
    Write-Host "✓ Database file exists" -ForegroundColor Green
}

# Check folders
Write-Host "`nChecking data folders..." -ForegroundColor Yellow
$folders = @("Receipts", "Documents", "Reports", "Backups")
foreach ($folder in $folders) {
    $folderPath = Join-Path $appPath "data\$folder"
    if (-not (Test-Path $folderPath)) {
        $warnings += "Folder not found: $folderPath (will be created on first launch)"
    } else {
        Write-Host "✓ Folder exists: $folder" -ForegroundColor Green
    }
}

# Check configuration
Write-Host "`nChecking configuration..." -ForegroundColor Yellow
$configPath = Join-Path $appPath "appsettings.json"
if (Test-Path $configPath) {
    $config = Get-Content $configPath | ConvertFrom-Json
    if ($config.ConnectionStrings.SupabaseUrl -eq "https://your-project.supabase.co") {
        $warnings += "Supabase URL not configured (using default)"
    } else {
        Write-Host "✓ Configuration file exists" -ForegroundColor Green
    }
} else {
    $warnings += "Configuration file not found (will be created on first launch)"
}

# Check .NET Runtime
Write-Host "`nChecking .NET Runtime..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET Runtime: $dotnetVersion" -ForegroundColor Green
} catch {
    $errors += ".NET Runtime not found"
}

# Summary
Write-Host "`n" + ("="*50) -ForegroundColor Cyan
Write-Host "Validation Summary" -ForegroundColor Cyan
Write-Host ("="*50) -ForegroundColor Cyan

if ($errors.Count -eq 0) {
    Write-Host "✓ No errors found" -ForegroundColor Green
} else {
    Write-Host "✗ Errors found:" -ForegroundColor Red
    foreach ($error in $errors) {
        Write-Host "  - $error" -ForegroundColor Red
    }
}

if ($warnings.Count -gt 0) {
    Write-Host "`n⚠ Warnings:" -ForegroundColor Yellow
    foreach ($warning in $warnings) {
        Write-Host "  - $warning" -ForegroundColor Yellow
    }
}

Write-Host "`nValidation completed!" -ForegroundColor Green

if ($errors.Count -gt 0) {
    exit 1
}

