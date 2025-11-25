# El Mansour Syndic Manager - Installation Guide

## Prerequisites

- **Windows 10/11** (64-bit)
- **.NET 8.0 Runtime** (included in MSIX package)
- **Internet connection** (for cloud sync)

## Installation Methods

### Method 1: MSIX Package (Recommended)

1. **Download the MSIX package**
   - Download `ElMansourSyndicManager_1.0.0.0.msix` from releases

2. **Install the package**
   - Double-click the `.msix` file
   - Click "Install" when prompted
   - Wait for installation to complete

3. **Launch the application**
   - Find "El Mansour Syndic Manager" in Start Menu
   - Click to launch

### Method 2: ClickOnce (Alternative)

1. **Download the ClickOnce installer**
   - Download `setup.exe` from releases

2. **Run the installer**
   - Double-click `setup.exe`
   - Follow the installation wizard
   - Click "Install" to complete

3. **Launch the application**
   - The application will launch automatically
   - Or find it in Start Menu

## Initial Setup

### 1. First Launch

On first launch, the application will:
- Create local database (`data\local.db`)
- Create necessary folders (Receipts, Documents, Reports, Backups)
- Prompt for cloud connection settings

### 2. Configure Cloud Connection

1. Go to **Paramètres** (Settings)
2. Enter Supabase credentials:
   - **Supabase URL**: `https://your-project.supabase.co`
   - **Supabase Key**: Your anon key
   - **Service Key**: Your service role key (for admin operations)

3. Click **Tester la Connexion** (Test Connection)
4. Click **Enregistrer** (Save)

### 3. Initial Login

**Admin User**:
- **House Code**: `B-SYNDIC`
- **6-digit Code**: `123456` (change immediately after first login)

**Syndic Members**:
- **House Codes**: `A01`, `B01`, `C01`, `D01`
- **6-digit Codes**: `111111`, `222222`, `333333`, `444444` (change after first login)

### 4. Change Default Passwords

**Important**: Change all default 6-digit codes immediately!

1. Login as Admin
2. Go to **Utilisateurs** (Users)
3. Select each user
4. Click **Réinitialiser le Code** (Reset Code)
5. Enter new 6-digit code
6. Save

### 5. Upload Signatures

For each Syndic Member:
1. Go to **Utilisateurs** (Users)
2. Select the member
3. Click **Télécharger Signature** (Upload Signature)
4. Select PNG image file
5. Save

## Cloud Setup

### Supabase Setup

1. **Create Supabase Project**
   - Go to https://supabase.com
   - Create new project
   - Note your project URL and keys

2. **Run Database Schema**
   - Go to SQL Editor in Supabase Dashboard
   - Run `deployment/cloud/supabase-deploy.sql`
   - Verify all tables are created

3. **Setup Storage Buckets**
   - Go to Storage in Supabase Dashboard
   - Run `deployment/cloud/supabase-storage-setup.sql`
   - Or create buckets manually:
     - `receipts` (private)
     - `documents` (private)
     - `backups` (private)
     - `reports` (private)
     - `signatures` (private)

4. **Deploy Edge Functions**
   - Install Supabase CLI: `npm install -g supabase`
   - Run: `deployment/cloud/deploy-edge-functions.ps1`
   - Schedule cron jobs in Supabase Dashboard:
     - `backup-cron`: Daily at 2 AM
     - `unpaid-notifications`: Daily at 9 AM

5. **Configure RLS Policies**
   - Review and adjust Row Level Security policies
   - Ensure proper access control

### Seed Data

1. **Run Seed Data Script**
   - Go to SQL Editor in Supabase Dashboard
   - Run `deployment/seed-data/seed-data.sql`
   - Verify data is inserted

2. **Generate User Passwords**
   - Run `deployment/scripts/generate-seed-data.ps1`
   - This generates proper password hashes
   - Update seed data SQL with generated hashes

## Configuration Files

### appsettings.json

Located in application directory:
```json
{
  "ConnectionStrings": {
    "LocalDatabase": "Data Source=data\\local.db",
    "SupabaseUrl": "https://your-project.supabase.co",
    "SupabaseKey": "your-anon-key",
    "SupabaseServiceKey": "your-service-key"
  },
  "Backup": {
    "Enabled": true,
    "ScheduleTime": "02:00:00",
    "RetentionDays": 30
  }
}
```

## Updates

### Automatic Updates (MSIX)

MSIX packages support automatic updates:
- Updates are downloaded automatically
- User is notified when update is available
- Click "Update" to install

### Manual Updates (ClickOnce)

1. Launch the application
2. If update is available, you'll be prompted
3. Click "Update" to download and install

## Troubleshooting

### Database Connection Issues

1. Check `appsettings.json` configuration
2. Verify Supabase credentials
3. Check internet connection
4. Review logs in `data\logs\`

### Backup Issues

1. Check backup folder permissions
2. Verify disk space
3. Check encryption key in settings
4. Review backup logs

### Notification Issues

1. Check Windows notification settings
2. Verify app has notification permissions
3. Check notification service configuration

## Uninstallation

### MSIX

1. Go to **Settings** > **Apps**
2. Find "El Mansour Syndic Manager"
3. Click **Uninstall**

### ClickOnce

1. Go to **Control Panel** > **Programs**
2. Find "El Mansour Syndic Manager"
3. Click **Uninstall**

**Note**: Uninstallation does NOT delete:
- Local database (`data\local.db`)
- User data folders
- Backups

To completely remove, manually delete:
- `%LocalAppData%\ElMansourSyndicManager\`
- `%AppData%\ElMansourSyndicManager\`

## Support

For issues or questions:
- Check logs in `data\logs\`
- Review documentation
- Contact support

