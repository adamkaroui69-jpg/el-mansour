# El Mansour Syndic Manager - Database Schema

## Overview

The application uses a dual-database architecture:
- **Local SQLite**: Primary database for offline operations
- **Cloud Supabase (PostgreSQL)**: Cloud database for synchronization and backup

Both databases share the same schema structure with additional fields for sync management.

---

## Local SQLite Schema

### 1. Houses Table

Stores information about all houses, shops, and special units.

```sql
CREATE TABLE Houses (
    Code TEXT PRIMARY KEY,                    -- Format: "A01", "B02", "M01", etc.
    Building TEXT NOT NULL,                    -- 'A', 'B', 'C', 'D', 'E'
    Floor INTEGER NOT NULL,                    -- Floor number (0 = Ground, 1-4)
    UnitNumber INTEGER NOT NULL,               -- Unit number on floor
    Type TEXT NOT NULL,                        -- 'House', 'Shop', 'Office', 'Concierge'
    MonthlyAmount REAL NOT NULL,               -- Fixed monthly payment amount
    IsActive INTEGER NOT NULL DEFAULT 1,       -- 1 = Active, 0 = Inactive
    CreatedAt TEXT NOT NULL,                   -- ISO 8601 format
    UpdatedAt TEXT NOT NULL,                   -- ISO 8601 format
    CONSTRAINT CHK_Building CHECK (Building IN ('A', 'B', 'C', 'D', 'E')),
    CONSTRAINT CHK_Type CHECK (Type IN ('House', 'Shop', 'Office', 'Concierge'))
);

CREATE INDEX IX_Houses_Building ON Houses(Building);
CREATE INDEX IX_Houses_Type ON Houses(Type);
```

**Initial Data**:
- Buildings A, C, D, E: 3 floors × 4 houses = 12 houses each
- Building B: 4 floors × 4 houses = 16 houses + 2 special units (4th floor)
- Building A ground floor: Shop M01
- Building B ground floor: Shops M02, M03

---

### 2. Users Table

Stores user accounts (1 Admin + up to 4 Syndic Members).

```sql
CREATE TABLE Users (
    Id TEXT PRIMARY KEY,                      -- GUID
    Name TEXT NOT NULL,
    Surname TEXT NOT NULL,
    HouseCode TEXT UNIQUE NOT NULL,            -- References Houses.Code
    PasswordHash TEXT NOT NULL,                -- PBKDF2 hash
    Salt TEXT NOT NULL,                        -- Salt for password hashing
    Role TEXT NOT NULL,                        -- 'Admin' or 'SyndicMember'
    SignaturePath TEXT,                        -- Path to PNG signature file
    IsActive INTEGER NOT NULL DEFAULT 1,       -- 1 = Active, 0 = Inactive
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    LastSyncAt TEXT,                           -- Last sync timestamp
    CloudId TEXT,                              -- Cloud database ID
    SyncStatus TEXT NOT NULL DEFAULT 'Pending', -- 'Pending', 'Synced', 'Conflict'
    FOREIGN KEY (HouseCode) REFERENCES Houses(Code)
);

CREATE INDEX IX_Users_HouseCode ON Users(HouseCode);
CREATE INDEX IX_Users_Role ON Users(Role);
CREATE INDEX IX_Users_SyncStatus ON Users(SyncStatus);
```

**Constraints**:
- Only one Admin user
- Maximum 4 active Syndic Members
- HouseCode must be unique
- SignaturePath required for Syndic Members

---

### 3. Payments Table

Stores monthly payment records.

```sql
CREATE TABLE Payments (
    Id TEXT PRIMARY KEY,                      -- GUID
    HouseCode TEXT NOT NULL,                   -- References Houses.Code
    Amount REAL NOT NULL,                      -- Payment amount
    PaymentDate TEXT NOT NULL,                 -- ISO 8601 format
    Month TEXT NOT NULL,                       -- Format: "YYYY-MM"
    Year INTEGER NOT NULL,                     -- Year for quick filtering
    ReceiptPath TEXT,                          -- Path to generated PDF receipt
    RecordedBy TEXT NOT NULL,                 -- References Users.Id
    Status TEXT NOT NULL DEFAULT 'Paid',       -- 'Paid', 'Unpaid', 'Overdue', 'Cancelled'
    Notes TEXT,                                -- Optional notes
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    LastSyncAt TEXT,
    CloudId TEXT,
    SyncStatus TEXT NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (HouseCode) REFERENCES Houses(Code),
    FOREIGN KEY (RecordedBy) REFERENCES Users(Id),
    CONSTRAINT CHK_MonthFormat CHECK (Month GLOB '[0-9][0-9][0-9][0-9]-[0-9][0-9]'),
    CONSTRAINT CHK_Status CHECK (Status IN ('Paid', 'Unpaid', 'Overdue', 'Cancelled'))
);

CREATE UNIQUE INDEX IX_Payments_HouseCode_Month ON Payments(HouseCode, Month);
CREATE INDEX IX_Payments_PaymentDate ON Payments(PaymentDate);
CREATE INDEX IX_Payments_Month ON Payments(Month);
CREATE INDEX IX_Payments_Year ON Payments(Year);
CREATE INDEX IX_Payments_Status ON Payments(Status);
CREATE INDEX IX_Payments_SyncStatus ON Payments(SyncStatus);
```

**Business Rules**:
- One payment per house per month (enforced by unique index)
- Amount must match house's MonthlyAmount
- Cannot delete payments (only mark as Cancelled)

---

### 4. Maintenance Table

Stores maintenance requests and costs.

```sql
CREATE TABLE Maintenance (
    Id TEXT PRIMARY KEY,                      -- GUID
    Description TEXT NOT NULL,
    Type TEXT NOT NULL,                        -- 'Repair', 'Cleaning', 'Security', 'Other'
    Cost REAL NOT NULL,                        -- Maintenance cost
    Status TEXT NOT NULL DEFAULT 'Pending',    -- 'Pending', 'InProgress', 'Completed', 'Cancelled'
    CreatedBy TEXT NOT NULL,                   -- References Users.Id
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    CompletedAt TEXT,                          -- ISO 8601 format (nullable)
    Notes TEXT,                                -- Optional notes
    LastSyncAt TEXT,
    CloudId TEXT,
    SyncStatus TEXT NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT CHK_Type CHECK (Type IN ('Repair', 'Cleaning', 'Security', 'Other')),
    CONSTRAINT CHK_Status CHECK (Status IN ('Pending', 'InProgress', 'Completed', 'Cancelled'))
);

CREATE INDEX IX_Maintenance_Type ON Maintenance(Type);
CREATE INDEX IX_Maintenance_Status ON Maintenance(Status);
CREATE INDEX IX_Maintenance_CreatedAt ON Maintenance(CreatedAt);
CREATE INDEX IX_Maintenance_SyncStatus ON Maintenance(SyncStatus);
```

---

### 5. MaintenanceDocuments Table

Stores justificative documents attached to maintenance requests.

```sql
CREATE TABLE MaintenanceDocuments (
    Id TEXT PRIMARY KEY,                      -- GUID
    MaintenanceId TEXT NOT NULL,              -- References Maintenance.Id
    DocumentPath TEXT NOT NULL,               -- Local file path
    DocumentType TEXT NOT NULL,               -- 'Justificative', 'Invoice', 'Other'
    FileName TEXT NOT NULL,                   -- Original file name
    FileSize INTEGER NOT NULL,                -- File size in bytes
    MimeType TEXT NOT NULL,                   -- MIME type (e.g., 'application/pdf')
    UploadedBy TEXT NOT NULL,                 -- References Users.Id
    UploadedAt TEXT NOT NULL,
    LastSyncAt TEXT,
    CloudId TEXT,
    CloudStoragePath TEXT,                    -- Cloud storage path
    SyncStatus TEXT NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (MaintenanceId) REFERENCES Maintenance(Id) ON DELETE CASCADE,
    FOREIGN KEY (UploadedBy) REFERENCES Users(Id),
    CONSTRAINT CHK_DocumentType CHECK (DocumentType IN ('Justificative', 'Invoice', 'Other'))
);

CREATE INDEX IX_MaintenanceDocuments_MaintenanceId ON MaintenanceDocuments(MaintenanceId);
CREATE INDEX IX_MaintenanceDocuments_SyncStatus ON MaintenanceDocuments(SyncStatus);
```

---

### 6. AuditLogs Table

Stores audit trail of all user actions.

```sql
CREATE TABLE AuditLogs (
    Id TEXT PRIMARY KEY,                      -- GUID
    UserId TEXT NOT NULL,                     -- References Users.Id
    Action TEXT NOT NULL,                     -- 'Create', 'Update', 'Delete', 'View', 'Login', 'Logout'
    EntityType TEXT NOT NULL,                 -- 'Payment', 'Maintenance', 'User', etc.
    EntityId TEXT,                            -- ID of affected entity (nullable)
    Details TEXT,                             -- JSON string with additional details
    IpAddress TEXT,                           -- IP address (nullable)
    UserAgent TEXT,                           -- User agent (nullable)
    Timestamp TEXT NOT NULL,                  -- ISO 8601 format
    LastSyncAt TEXT,
    CloudId TEXT,
    SyncStatus TEXT NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT CHK_Action CHECK (Action IN ('Create', 'Update', 'Delete', 'View', 'Login', 'Logout', 'Export', 'Backup', 'Restore'))
);

CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_Timestamp ON AuditLogs(Timestamp);
CREATE INDEX IX_AuditLogs_EntityType ON AuditLogs(EntityType);
CREATE INDEX IX_AuditLogs_Action ON AuditLogs(Action);
```

---

### 7. SyncQueue Table

Stores pending changes to be synchronized with cloud.

```sql
CREATE TABLE SyncQueue (
    Id TEXT PRIMARY KEY,                      -- GUID
    EntityType TEXT NOT NULL,                 -- 'User', 'Payment', 'Maintenance', etc.
    EntityId TEXT NOT NULL,                   -- ID of entity to sync
    Operation TEXT NOT NULL,                  -- 'Create', 'Update', 'Delete'
    Data TEXT NOT NULL,                       -- JSON serialized entity data
    CreatedAt TEXT NOT NULL,
    RetryCount INTEGER NOT NULL DEFAULT 0,
    LastError TEXT,                           -- Last error message (nullable)
    LastAttemptAt TEXT,                       -- Last sync attempt timestamp
    Status TEXT NOT NULL DEFAULT 'Pending',   -- 'Pending', 'Processing', 'Failed', 'Completed'
    Priority INTEGER NOT NULL DEFAULT 0,      -- Higher priority syncs first
    CONSTRAINT CHK_Operation CHECK (Operation IN ('Create', 'Update', 'Delete')),
    CONSTRAINT CHK_Status CHECK (Status IN ('Pending', 'Processing', 'Failed', 'Completed'))
);

CREATE INDEX IX_SyncQueue_Status ON SyncQueue(Status);
CREATE INDEX IX_SyncQueue_CreatedAt ON SyncQueue(CreatedAt);
CREATE INDEX IX_SyncQueue_Priority ON SyncQueue(Priority DESC, CreatedAt);
```

---

### 8. AppSettings Table

Stores application settings and configuration.

```sql
CREATE TABLE AppSettings (
    Key TEXT PRIMARY KEY,
    Value TEXT NOT NULL,
    Type TEXT NOT NULL,                       -- 'String', 'Int', 'Bool', 'DateTime'
    Description TEXT,
    UpdatedAt TEXT NOT NULL,
    UpdatedBy TEXT,                            -- References Users.Id (nullable)
    FOREIGN KEY (UpdatedBy) REFERENCES Users(Id)
);
```

**Default Settings**:
- `SyncIntervalMinutes`: 5
- `BackupEnabled`: true
- `BackupTime`: "02:00"
- `Language`: "fr-FR"
- `Currency`: "MAD"
- `CloudApiUrl`: (Supabase URL)
- `CloudApiKey`: (encrypted)

---

## Cloud Schema (Supabase/PostgreSQL)

The cloud schema mirrors the local schema with additional fields for:
- `created_at` (timestamp with timezone)
- `updated_at` (timestamp with timezone)
- `deleted_at` (timestamp with timezone, for soft deletes)
- Row Level Security (RLS) policies
- Real-time subscriptions

### Example: Cloud Payments Table

```sql
CREATE TABLE payments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    house_code TEXT NOT NULL,
    amount DECIMAL(10,2) NOT NULL,
    payment_date TIMESTAMPTZ NOT NULL,
    month TEXT NOT NULL,
    year INTEGER NOT NULL,
    receipt_path TEXT,
    recorded_by UUID NOT NULL,
    status TEXT NOT NULL DEFAULT 'Paid',
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,
    CONSTRAINT fk_house FOREIGN KEY (house_code) REFERENCES houses(code),
    CONSTRAINT fk_user FOREIGN KEY (recorded_by) REFERENCES users(id),
    CONSTRAINT unique_house_month UNIQUE (house_code, month) WHERE deleted_at IS NULL
);

CREATE INDEX idx_payments_payment_date ON payments(payment_date);
CREATE INDEX idx_payments_month ON payments(month);
CREATE INDEX idx_payments_status ON payments(status);

-- Row Level Security
ALTER TABLE payments ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can view payments" ON payments
    FOR SELECT USING (auth.role() = 'authenticated');

CREATE POLICY "Users can insert payments" ON payments
    FOR INSERT WITH CHECK (auth.role() = 'authenticated');

CREATE POLICY "Users can update payments" ON payments
    FOR UPDATE USING (auth.role() = 'authenticated');
```

---

## Database Initialization

### Seed Data Script

```sql
-- Insert Houses
-- Building A (3 floors × 4 houses)
INSERT INTO Houses (Code, Building, Floor, UnitNumber, Type, MonthlyAmount, IsActive, CreatedAt, UpdatedAt)
VALUES 
    ('A01', 'A', 1, 1, 'House', 1500.00, 1, datetime('now'), datetime('now')),
    ('A02', 'A', 1, 2, 'House', 1500.00, 1, datetime('now'), datetime('now')),
    -- ... (12 houses for Building A)
    ('M01', 'A', 0, 1, 'Shop', 2000.00, 1, datetime('now'), datetime('now'));

-- Building B (4 floors × 4 houses + 2 special units)
-- ... (16 houses + Office + Concierge)

-- Buildings C, D, E (3 floors × 4 houses each)
-- ... (12 houses each)

-- Insert Admin User (default password: 123456)
INSERT INTO Users (Id, Name, Surname, HouseCode, PasswordHash, Salt, Role, IsActive, CreatedAt, UpdatedAt)
VALUES (
    '00000000-0000-0000-0000-000000000001',
    'Admin',
    'System',
    'B40', -- Syndic Office
    '...', -- Hashed password
    '...', -- Salt
    'Admin',
    1,
    datetime('now'),
    datetime('now')
);
```

---

## Database Migrations

### Migration Strategy
- Version-based migrations
- SQL scripts in `scripts/migrations/`
- Migration tracking table

### Migration Table

```sql
CREATE TABLE Migrations (
    Version INTEGER PRIMARY KEY,
    Name TEXT NOT NULL,
    AppliedAt TEXT NOT NULL,
    Checksum TEXT NOT NULL
);
```

---

## Backup and Restore

### Backup Format
- Full SQLite database dump
- Compressed archive with documents
- Metadata JSON file

### Restore Process
1. Validate backup integrity
2. Close database connections
3. Restore database file
4. Restore documents
5. Verify data integrity
6. Trigger full sync

