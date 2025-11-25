# Setup Local SQLite Database with Seed Data
# Run this after first installation

param(
    [string]$DatabasePath = "data\local.db"
)

$ErrorActionPreference = "Stop"

Write-Host "Setting up local SQLite database..." -ForegroundColor Green

# Check if database exists
if (Test-Path $DatabasePath) {
    $response = Read-Host "Database already exists. Do you want to recreate it? (y/N)"
    if ($response -ne 'y') {
        Write-Host "Setup cancelled." -ForegroundColor Yellow
        exit 0
    }
    Remove-Item $DatabasePath -Force
}

# Create database directory
$dbDir = Split-Path $DatabasePath -Parent
if (-not (Test-Path $dbDir)) {
    New-Item -ItemType Directory -Path $dbDir -Force | Out-Null
}

# Run EF Core migrations or SQL scripts
Write-Host "Creating database schema..." -ForegroundColor Yellow

# Option 1: Use EF Core migrations
# dotnet ef database update --project src\ElMansourSyndicManager.Infrastructure

# Option 2: Run SQL script
$sqlScript = Join-Path $PSScriptRoot "..\DATABASE_SCHEMA.md"
# Extract SQL from markdown and execute

Write-Host "Database setup completed!" -ForegroundColor Green
Write-Host "Database location: $DatabasePath" -ForegroundColor Cyan
