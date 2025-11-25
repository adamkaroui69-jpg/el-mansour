# El Mansour Syndic Manager - Cloud Deployment Guide

## Supabase Deployment

### Step 1: Create Supabase Project

1. Go to https://supabase.com
2. Sign up / Login
3. Click "New Project"
4. Enter project details:
   - **Name**: El Mansour Syndic Manager
   - **Database Password**: (save securely)
   - **Region**: Choose closest region
5. Wait for project creation (2-3 minutes)

### Step 2: Deploy Database Schema

1. Go to **SQL Editor** in Supabase Dashboard
2. Open `deployment/cloud/supabase-deploy.sql`
3. Copy entire SQL script
4. Paste into SQL Editor
5. Click **Run**
6. Verify all tables created (check Tables section)

### Step 3: Setup Storage Buckets

1. Go to **Storage** in Supabase Dashboard
2. Run `deployment/cloud/supabase-storage-setup.sql` in SQL Editor
3. Or create buckets manually:
   - Click "New bucket"
   - Create each bucket:
     - `receipts` (Private)
     - `documents` (Private)
     - `backups` (Private)
     - `reports` (Private)
     - `signatures` (Private)

### Step 4: Configure Storage Policies

1. Go to **Storage** > **Policies**
2. For each bucket, create policies:
   - **Upload Policy**: Authenticated users can upload
   - **Read Policy**: Authenticated users can read
   - **Delete Policy**: Admins can delete

Example policy (SQL):
```sql
CREATE POLICY "Users can upload receipts" ON storage.objects
    FOR INSERT WITH CHECK (
        bucket_id = 'receipts' AND
        auth.role() = 'authenticated'
    );
```

### Step 5: Deploy Edge Functions

1. **Install Supabase CLI**
   ```bash
   npm install -g supabase
   ```

2. **Login to Supabase**
   ```bash
   supabase login
   ```

3. **Link Project**
   ```bash
   supabase link --project-ref YOUR_PROJECT_ID
   ```

4. **Deploy Functions**
   ```bash
   cd deployment/cloud/supabase-edge-functions
   supabase functions deploy backup-cron
   supabase functions deploy unpaid-notifications
   ```

5. **Schedule Cron Jobs**
   - Go to Supabase Dashboard > Edge Functions
   - For `backup-cron`:
     - Schedule: `0 2 * * *` (2 AM daily)
   - For `unpaid-notifications`:
     - Schedule: `0 9 * * *` (9 AM daily)

### Step 6: Configure RLS (Row Level Security)

1. Go to **Authentication** > **Policies**
2. Review RLS policies for each table
3. Adjust based on your security requirements

Example policies:
- Users can only see their own data
- Admins can see all data
- Syndic members can see payments for all houses

### Step 7: Insert Seed Data

1. **Generate Password Hashes**
   ```powershell
   cd deployment/scripts
   .\generate-seed-data.ps1 -AdminCode "123456" -MemberCodes @("111111","222222","333333","444444")
   ```

2. **Run Seed Data Script**
   - Go to SQL Editor
   - Run `deployment/seed-data/seed-data.sql`
   - Update with generated password hashes from step 1

3. **Verify Data**
   - Check `users` table has Admin + 4 members
   - Check `houses` table has all houses
   - Check `buildings` table has all buildings

## Firestore Alternative (Optional)

If using Firestore instead of Supabase:

1. **Create Firestore Database**
   - Go to Firebase Console
   - Create Firestore database
   - Choose production mode

2. **Deploy Security Rules**
   ```javascript
   // Firestore security rules
   rules_version = '2';
   service cloud.firestore {
     match /databases/{database}/documents {
       match /users/{userId} {
         allow read, write: if request.auth != null && request.auth.uid == userId;
       }
       // Add more rules...
     }
   }
   ```

3. **Deploy Cloud Functions**
   - Use Firebase CLI
   - Deploy scheduled functions

## Encryption Keys

### Backup Encryption Key

**Important**: Change default encryption key in production!

1. Generate secure key:
   ```powershell
   $key = [Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
   ```

2. Store securely:
   - Azure Key Vault
   - AWS Secrets Manager
   - Environment variables
   - Secure configuration file

3. Update `appsettings.json`:
   ```json
   "Backup": {
     "EncryptionKey": "YOUR_SECURE_KEY_HERE"
   }
   ```

## Monitoring Setup

### Application Insights (Optional)

1. Create Application Insights resource
2. Get instrumentation key
3. Add to `appsettings.json`:
   ```json
   "ApplicationInsights": {
     "InstrumentationKey": "YOUR_KEY"
   }
   ```

### Logging

1. Configure Serilog or similar
2. Set log levels
3. Configure log destinations:
   - File
   - Cloud (Application Insights, etc.)

## Security Checklist

- [ ] RLS policies configured
- [ ] Storage policies configured
- [ ] API keys secured (not in code)
- [ ] Encryption keys secured
- [ ] HTTPS enabled
- [ ] Authentication required
- [ ] Audit logging enabled
- [ ] Backup encryption enabled
- [ ] Regular security reviews scheduled

## Maintenance

### Regular Tasks

1. **Weekly**
   - Review audit logs
   - Check backup status
   - Monitor error logs

2. **Monthly**
   - Review RLS policies
   - Check storage usage
   - Review edge function logs

3. **Quarterly**
   - Security audit
   - Performance review
   - Update dependencies

## Support

For cloud deployment issues:
1. Check Supabase/Firebase logs
2. Review edge function logs
3. Check RLS policies
4. Verify API keys
5. Contact support

