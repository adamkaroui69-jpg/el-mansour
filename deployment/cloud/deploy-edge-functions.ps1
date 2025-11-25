# Deploy Supabase Edge Functions
# Requires: Supabase CLI (npm install -g supabase)

param(
    [string]$SupabaseProjectId,
    [string]$SupabaseAccessToken
)

$ErrorActionPreference = "Stop"

Write-Host "Deploying Supabase Edge Functions..." -ForegroundColor Green

# Set environment variables
$env:SUPABASE_ACCESS_TOKEN = $SupabaseAccessToken
$env:SUPABASE_PROJECT_ID = $SupabaseProjectId

# Deploy backup cron function
Write-Host "Deploying backup-cron function..." -ForegroundColor Yellow
supabase functions deploy backup-cron --project-ref $SupabaseProjectId

# Deploy unpaid notifications function
Write-Host "Deploying unpaid-notifications function..." -ForegroundColor Yellow
supabase functions deploy unpaid-notifications --project-ref $SupabaseProjectId

# Schedule cron jobs (via Supabase Dashboard or API)
Write-Host "`nNote: Schedule cron jobs in Supabase Dashboard:" -ForegroundColor Cyan
Write-Host "- backup-cron: Daily at 2 AM (0 2 * * *)" -ForegroundColor Cyan
Write-Host "- unpaid-notifications: Daily at 9 AM (0 9 * * *)" -ForegroundColor Cyan

Write-Host "`nDeployment completed!" -ForegroundColor Green

