# Complete Production Setup Script
# Run this after initial deployment to configure production environment

param(
    [string]$SupabaseUrl,
    [string]$SupabaseKey,
    [string]$SupabaseServiceKey,
    [string]$BackupEncryptionKey
)

$ErrorActionPreference = "Stop"

Write-Host "Setting up El Mansour Syndic Manager for production..." -ForegroundColor Green

# Validate parameters
if ([string]::IsNullOrWhiteSpace($SupabaseUrl)) {
    Write-Host "Error: SupabaseUrl is required" -ForegroundColor Red
    exit 1
}

if ([string]::IsNullOrWhiteSpace($SupabaseKey)) {
    Write-Host "Error: SupabaseKey is required" -ForegroundColor Red
    exit 1
}

# Update appsettings.json
Write-Host "`nUpdating configuration..." -ForegroundColor Yellow
$appSettingsPath = Join-Path $PSScriptRoot "..\..\src\ElMansourSyndicManager\appsettings.json"

if (Test-Path $appSettingsPath) {
    $config = Get-Content $appSettingsPath | ConvertFrom-Json
    
    $config.ConnectionStrings.SupabaseUrl = $SupabaseUrl
    $config.ConnectionStrings.SupabaseKey = $SupabaseKey
    
    if (-not [string]::IsNullOrWhiteSpace($SupabaseServiceKey)) {
        $config.ConnectionStrings.SupabaseServiceKey = $SupabaseServiceKey
    }
    
    if (-not [string]::IsNullOrWhiteSpace($BackupEncryptionKey)) {
        $config.Backup.EncryptionKey = $BackupEncryptionKey
    }
    
    $config | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath -Encoding UTF8
    Write-Host "✓ Configuration updated" -ForegroundColor Green
} else {
    Write-Host "⚠ appsettings.json not found, skipping..." -ForegroundColor Yellow
}

# Generate secure backup encryption key if not provided
if ([string]::IsNullOrWhiteSpace($BackupEncryptionKey)) {
    Write-Host "`nGenerating backup encryption key..." -ForegroundColor Yellow
    $keyBytes = 1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }
    $BackupEncryptionKey = [Convert]::ToBase64String($keyBytes)
    Write-Host "Generated key: $BackupEncryptionKey" -ForegroundColor Cyan
    Write-Host "⚠ Save this key securely! It's required for backup decryption." -ForegroundColor Yellow
}

# Create production checklist
Write-Host "`nCreating production checklist..." -ForegroundColor Yellow
$checklist = @"
# Production Setup Checklist

## Completed
- [x] Configuration updated
- [x] Backup encryption key generated

## Remaining Tasks
- [ ] Deploy database schema to Supabase
- [ ] Create storage buckets
- [ ] Configure RLS policies
- [ ] Deploy edge functions
- [ ] Schedule cron jobs
- [ ] Insert seed data
- [ ] Change default passwords
- [ ] Upload signatures
- [ ] Test all features
- [ ] Configure monitoring
- [ ] Setup backup schedule
- [ ] Train users

## Security
- [ ] Change default admin password
- [ ] Change default member passwords
- [ ] Secure API keys
- [ ] Review RLS policies
- [ ] Enable audit logging
- [ ] Configure backup encryption

## Backup Encryption Key
$BackupEncryptionKey

**IMPORTANT**: Store this key securely! It's required for backup restoration.
"@

$checklistPath = Join-Path $PSScriptRoot "..\PRODUCTION_CHECKLIST.md"
$checklist | Out-File -FilePath $checklistPath -Encoding UTF8

Write-Host "✓ Production checklist created: $checklistPath" -ForegroundColor Green

Write-Host "`n" + ("="*50) -ForegroundColor Cyan
Write-Host "Production Setup Summary" -ForegroundColor Cyan
Write-Host ("="*50) -ForegroundColor Cyan
Write-Host "Supabase URL: $SupabaseUrl" -ForegroundColor Green
Write-Host "Backup Key: $BackupEncryptionKey" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Deploy database schema (supabase-deploy.sql)" -ForegroundColor Cyan
Write-Host "2. Setup storage buckets" -ForegroundColor Cyan
Write-Host "3. Deploy edge functions" -ForegroundColor Cyan
Write-Host "4. Insert seed data" -ForegroundColor Cyan
Write-Host "5. Review production checklist" -ForegroundColor Cyan

Write-Host "`nSetup completed!" -ForegroundColor Green

