# El Mansour Syndic Manager - Deployment Guide

## Overview

Complete deployment guide for packaging, deploying, and maintaining the El Mansour Syndic Manager WPF application.

## Quick Start

1. **Build MSIX Package**
   ```powershell
   cd deployment/MSIX
   .\build-msix.ps1 -Configuration Release -Version 1.0.0.0
   ```

2. **Deploy Cloud Services**
   ```powershell
   cd deployment/cloud
   .\deploy-edge-functions.ps1 -SupabaseProjectId YOUR_PROJECT_ID -SupabaseAccessToken YOUR_TOKEN
   ```

3. **Run Database Setup**
   - Execute `supabase-deploy.sql` in Supabase SQL Editor
   - Execute `supabase-storage-setup.sql` for storage buckets
   - Execute `seed-data.sql` for initial data

## Directory Structure

```
deployment/
├── MSIX/                    # MSIX packaging files
│   ├── Package.appxmanifest
│   ├── Assets/              # App icons and logos
│   └── build-msix.ps1
├── cloud/                   # Cloud deployment
│   ├── supabase-deploy.sql
│   ├── supabase-storage-setup.sql
│   ├── supabase-edge-functions/
│   │   ├── backup-cron/
│   │   └── unpaid-notifications/
│   └── deploy-edge-functions.ps1
├── seed-data/               # Seed data scripts
│   └── seed-data.sql
├── scripts/                 # Utility scripts
│   ├── generate-seed-data.ps1
│   └── setup-local-database.ps1
├── INSTALLATION_GUIDE.md
├── TESTING_CHECKLIST.md
└── README.md
```

## Build Process

### 1. Build Application

```powershell
dotnet publish src/ElMansourSyndicManager/ElMansourSyndicManager.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true
```

### 2. Create MSIX Package

```powershell
cd deployment/MSIX
.\build-msix.ps1 -Configuration Release -Version 1.0.0.0
```

### 3. Sign Package (Optional)

```powershell
signtool sign /f certificate.pfx /p password /fd SHA256 ElMansourSyndicManager.msix
```

## Cloud Deployment

### Supabase Setup

1. **Create Project**
   - Go to https://supabase.com
   - Create new project
   - Note project URL and keys

2. **Deploy Database**
   - Run `supabase-deploy.sql` in SQL Editor
   - Verify all tables created

3. **Setup Storage**
   - Run `supabase-storage-setup.sql`
   - Or create buckets manually in Dashboard

4. **Deploy Edge Functions**
   ```powershell
   supabase functions deploy backup-cron --project-ref YOUR_PROJECT_ID
   supabase functions deploy unpaid-notifications --project-ref YOUR_PROJECT_ID
   ```

5. **Schedule Cron Jobs**
   - Go to Supabase Dashboard > Edge Functions
   - Schedule `backup-cron`: `0 2 * * *` (2 AM daily)
   - Schedule `unpaid-notifications`: `0 9 * * *` (9 AM daily)

### Security Configuration

1. **RLS Policies**
   - Review and configure Row Level Security
   - Test access control

2. **Storage Policies**
   - Configure bucket access policies
   - Test file upload/download

3. **API Keys**
   - Store keys securely
   - Use environment variables
   - Rotate keys regularly

## Configuration

### Application Settings

Edit `appsettings.json`:
- Database connection strings
- Supabase credentials
- Backup schedule
- Notification settings

### Environment Variables

For production, use environment variables:
- `SUPABASE_URL`
- `SUPABASE_KEY`
- `SUPABASE_SERVICE_KEY`
- `BACKUP_ENCRYPTION_KEY`

## CI/CD Pipeline

### GitHub Actions

The workflow (`/.github/workflows/build-and-deploy.yml`) automatically:
- Builds the application
- Runs tests
- Creates MSIX package
- Deploys to cloud (on main branch)
- Creates release (on release/* branches)

### Setup Secrets

In GitHub repository settings, add:
- `SUPABASE_PROJECT_ID`
- `SUPABASE_ACCESS_TOKEN`
- `SUPABASE_SERVICE_KEY`

## Monitoring

### Logs

Application logs are stored in:
- `data\logs\application.log`
- `data\logs\errors.log`

### Telemetry (Optional)

For production monitoring:
- Application Insights
- Sentry
- Custom logging service

## Updates

### MSIX Auto-Update

1. Host MSIX on web server
2. Configure update URL in manifest
3. Users get automatic updates

### ClickOnce Updates

1. Publish to web server
2. Users get update prompts
3. Click "Update" to install

## Troubleshooting

### Common Issues

1. **MSIX Installation Fails**
   - Check Windows version (10.0.17763+)
   - Verify certificate
   - Check dependencies

2. **Cloud Connection Fails**
   - Verify Supabase credentials
   - Check network connectivity
   - Review RLS policies

3. **Backup Fails**
   - Check disk space
   - Verify folder permissions
   - Check encryption key

## Support

For deployment issues:
1. Check logs
2. Review documentation
3. Contact support team

## Next Steps

After deployment:
1. Run testing checklist
2. Configure monitoring
3. Train users
4. Schedule regular backups
5. Set up update schedule

