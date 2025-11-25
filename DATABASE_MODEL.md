# El Mansour Syndic Manager - Complete Database Model

## Overview

This document provides the complete database model for both **Supabase (PostgreSQL)** and **SQLite** implementations, including EF Core models, relationships, indexes, and seed data.

---

## Database Architecture

### Dual Database Strategy
- **SQLite (Local)**: Primary database for offline operations
- **Supabase (PostgreSQL)**: Cloud database for synchronization and backup

### Sync Strategy
- All tables include sync metadata: `CloudId`, `LastSyncAt`, `SyncStatus`
- SyncQueue table tracks pending changes
- Bidirectional synchronization with conflict resolution

---

## Entity Relationship Diagram

```
Residence (1) ──→ (N) Buildings ──→ (N) Houses
                                              │
                                              │
Users (N) ──→ (1) Houses
              │
              │
              └──→ (N) Payments ──→ (1) Receipts
              │
              └──→ (N) Maintenance ──→ (N) MaintenanceDocuments
              │
              └──→ (N) Expenses
              │
              └──→ (N) AuditLogs
              │
              └──→ (N) Notifications
```

---

## Table Definitions

### 1. Residences

**Purpose**: Store residence information (currently only "El Mansour")

#### Supabase Schema
```sql
CREATE TABLE residences (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name TEXT NOT NULL UNIQUE,
    address TEXT,
    city TEXT,
    postal_code TEXT,
    country TEXT DEFAULT 'Morocco',
    phone TEXT,
    email TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,
    CONSTRAINT unique_residence_name UNIQUE (name) WHERE deleted_at IS NULL
);

CREATE INDEX idx_residences_name ON residences(name);
CREATE INDEX idx_residences_deleted_at ON residences(deleted_at);
```

#### SQLite Schema
```sql
CREATE TABLE Residences (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL UNIQUE,
    Address TEXT,
    City TEXT,
    PostalCode TEXT,
    Country TEXT DEFAULT 'Morocco',
    Phone TEXT,
    Email TEXT,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    DeletedAt TEXT,
    LastSyncAt TEXT,
    CloudId TEXT,
    SyncStatus TEXT NOT NULL DEFAULT 'Pending'
);

CREATE INDEX IX_Residences_Name ON Residences(Name);
CREATE INDEX IX_Residences_DeletedAt ON Residences(DeletedAt);
```

---

### 2. Buildings

**Purpose**: Store building information (A, B, C, D, E)

#### Supabase Schema
```sql
CREATE TABLE buildings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    residence_id UUID NOT NULL REFERENCES residences(id) ON DELETE CASCADE,
    code TEXT NOT NULL, -- 'A', 'B', 'C', 'D', 'E'
    name TEXT NOT NULL, -- 'Bâtiment A', etc.
    floors INTEGER NOT NULL,
    description TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,
    CONSTRAINT unique_building_code UNIQUE (residence_id, code) WHERE deleted_at IS NULL,
    CONSTRAINT chk_building_code CHECK (code IN ('A', 'B', 'C', 'D', 'E'))
);

CREATE INDEX idx_buildings_residence_id ON buildings(residence_id);
CREATE INDEX idx_buildings_code ON buildings(code);
CREATE INDEX idx_buildings_deleted_at ON buildings(deleted_at);
```

#### SQLite Schema
```sql
CREATE TABLE Buildings (
    Id TEXT PRIMARY KEY,
    ResidenceId TEXT NOT NULL,
    Code TEXT NOT NULL,
    Name TEXT NOT NULL,
    Floors INTEGER NOT NULL,
    Description TEXT,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    DeletedAt TEXT,
    LastSyncAt TEXT,
    CloudId TEXT,
    SyncStatus TEXT NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (ResidenceId) REFERENCES Residences(Id),
    CONSTRAINT CHK_BuildingCode CHECK (Code IN ('A', 'B', 'C', 'D', 'E'))
);

CREATE INDEX IX_Buildings_ResidenceId ON Buildings(ResidenceId);
CREATE INDEX IX_Buildings_Code ON Buildings(Code);
```

---

### 3. Houses

**Purpose**: Store all units (houses, shops, offices, concierge)

#### Supabase Schema
```sql
CREATE TABLE houses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    building_id UUID NOT NULL REFERENCES buildings(id) ON DELETE CASCADE,
    code TEXT NOT NULL UNIQUE, -- 'A01', 'B02', 'M01', 'B-SYNDIC', 'B-CONCIERGE'
    floor INTEGER NOT NULL,
    unit_number INTEGER NOT NULL,
    type TEXT NOT NULL, -- 'House', 'Shop', 'Office', 'Concierge'
    monthly_amount DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    owner_name TEXT,
    owner_phone TEXT,
    owner_email TEXT,
    is_active BOOLEAN NOT NULL DEFAULT true,
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,
    CONSTRAINT unique_house_code UNIQUE (code) WHERE deleted_at IS NULL,
    CONSTRAINT chk_house_type CHECK (type IN ('House', 'Shop', 'Office', 'Concierge')),
    CONSTRAINT chk_house_floor CHECK (floor >= 0)
);

CREATE INDEX idx_houses_building_id ON houses(building_id);
CREATE INDEX idx_houses_code ON houses(code);
CREATE INDEX idx_houses_type ON houses(type);
CREATE INDEX idx_houses_is_active ON houses(is_active);
CREATE INDEX idx_houses_deleted_at ON houses(deleted_at);
```

#### SQLite Schema
```sql
CREATE TABLE Houses (
    Id TEXT PRIMARY KEY,
    BuildingId TEXT NOT NULL,
    Code TEXT NOT NULL UNIQUE,
    Floor INTEGER NOT NULL,
    UnitNumber INTEGER NOT NULL,
    Type TEXT NOT NULL,
    MonthlyAmount REAL NOT NULL DEFAULT 0.00,
    OwnerName TEXT,
    OwnerPhone TEXT,
    OwnerEmail TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    Notes TEXT,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    DeletedAt TEXT,
    LastSyncAt TEXT,
    CloudId TEXT,
    SyncStatus TEXT NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (BuildingId) REFERENCES Buildings(Id),
    CONSTRAINT CHK_HouseType CHECK (Type IN ('House', 'Shop', 'Office', 'Concierge')),
    CONSTRAINT CHK_HouseFloor CHECK (Floor >= 0)
);

CREATE INDEX IX_Houses_BuildingId ON Houses(BuildingId);
CREATE INDEX IX_Houses_Code ON Houses(Code);
CREATE INDEX IX_Houses_Type ON Houses(Type);
CREATE INDEX IX_Houses_IsActive ON Houses(IsActive);
```

---

### 4. Users

**Purpose**: Store user accounts (1 Admin + up to 4 Syndic Members)

#### Supabase Schema
```sql
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    house_id UUID REFERENCES houses(id) ON DELETE SET NULL,
    name TEXT NOT NULL,
    surname TEXT NOT NULL,
    house_code TEXT NOT NULL UNIQUE, -- For quick lookup
    password_hash TEXT NOT NULL, -- PBKDF2 hash
    salt TEXT NOT NULL, -- Salt for password hashing
    role TEXT NOT NULL, -- 'Admin' or 'SyndicMember'
    signature_path TEXT, -- Path to PNG signature file
    signature_cloud_path TEXT, -- Cloud storage path
    is_active BOOLEAN NOT NULL DEFAULT true,
    last_login_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,
    CONSTRAINT unique_user_house_code UNIQUE (house_code) WHERE deleted_at IS NULL,
    CONSTRAINT chk_user_role CHECK (role IN ('Admin', 'SyndicMember'))
);

CREATE INDEX idx_users_house_id ON users(house_id);
CREATE INDEX idx_users_house_code ON users(house_code);
CREATE INDEX idx_users_role ON users(role);
CREATE INDEX idx_users_is_active ON users(is_active);
CREATE INDEX idx_users_deleted_at ON users(deleted_at);
```

#### SQLite Schema
```sql
CREATE TABLE Users (
    Id TEXT PRIMARY KEY,
    HouseId TEXT,
    Name TEXT NOT NULL,
    Surname TEXT NOT NULL,
    HouseCode TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    Salt TEXT NOT NULL,
    Role TEXT NOT NULL,
    SignaturePath TEXT,
    SignatureCloudPath TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    LastLoginAt TEXT,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    DeletedAt TEXT,
    LastSyncAt TEXT,
    CloudId TEXT,
    SyncStatus TEXT NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (HouseId) REFERENCES Houses(Id),
    CONSTRAINT CHK_UserRole CHECK (Role IN ('Admin', 'SyndicMember'))
);

CREATE INDEX IX_Users_HouseId ON Users(HouseId);
CREATE INDEX IX_Users_HouseCode ON Users(HouseCode);
CREATE INDEX IX_Users_Role ON Users(Role);
CREATE INDEX IX_Users_IsActive ON Users(IsActive);
```

---

### 5. Payments

**Purpose**: Store monthly payment records

#### Supabase Schema
```sql
CREATE TABLE payments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    house_id UUID NOT NULL REFERENCES houses(id) ON DELETE CASCADE,
    house_code TEXT NOT NULL, -- Denormalized for quick access
    amount DECIMAL(10,2) NOT NULL,
    payment_date DATE NOT NULL,
    month TEXT NOT NULL, -- Format: 'YYYY-MM'
    year INTEGER NOT NULL,
    receipt_id UUID REFERENCES receipts(id) ON DELETE SET NULL,
    recorded_by UUID NOT NULL REFERENCES users(id),
    status TEXT NOT NULL DEFAULT 'Paid', -- 'Paid', 'Unpaid', 'Overdue', 'Cancelled'
    payment_method TEXT, -- 'Cash', 'BankTransfer', 'Check', etc.
    reference_number TEXT,
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,
    CONSTRAINT unique_house_month UNIQUE (house_code, month) WHERE deleted_at IS NULL,
    CONSTRAINT chk_payment_month_format CHECK (month ~ '^[0-9]{4}-[0-9]{2}$'),
    CONSTRAINT chk_payment_status CHECK (status IN ('Paid', 'Unpaid', 'Overdue', 'Cancelled'))
);

CREATE INDEX idx_payments_house_id ON payments(house_id);
CREATE INDEX idx_payments_house_code ON payments(house_code);
CREATE INDEX idx_payments_month ON payments(month);
CREATE INDEX idx_payments_year ON payments(year);
CREATE INDEX idx_payments_payment_date ON payments(payment_date);
CREATE INDEX idx_payments_status ON payments(status);
CREATE INDEX idx_payments_recorded_by ON payments(recorded_by);
CREATE INDEX idx_payments_deleted_at ON payments(deleted_at);
```

#### SQLite Schema
```sql
CREATE TABLE Payments (
    Id TEXT PRIMARY KEY,
    HouseId TEXT NOT NULL,
    HouseCode TEXT NOT NULL,
    Amount REAL NOT NULL,
    PaymentDate TEXT NOT NULL,
    Month TEXT NOT NULL,
    Year INTEGER NOT NULL,
    ReceiptId TEXT,
    RecordedBy TEXT NOT NULL,
    Status TEXT NOT NULL DEFAULT 'Paid',
    PaymentMethod TEXT,
    ReferenceNumber TEXT,
    Notes TEXT,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    DeletedAt TEXT,
    LastSyncAt TEXT,
    CloudId TEXT,
    SyncStatus TEXT NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (HouseId) REFERENCES Houses(Id),
    FOREIGN KEY (ReceiptId) REFERENCES Receipts(Id),
    FOREIGN KEY (RecordedBy) REFERENCES Users(Id),
    CONSTRAINT CHK_PaymentMonthFormat CHECK (Month GLOB '[0-9][0-9][0-9][0-9]-[0-9][0-9]'),
    CONSTRAINT CHK_PaymentStatus CHECK (Status IN ('Paid', 'Unpaid', 'Overdue', 'Cancelled'))
);

CREATE UNIQUE INDEX IX_Payments_HouseCode_Month ON Payments(HouseCode, Month) WHERE DeletedAt IS NULL;
CREATE INDEX IX_Payments_HouseId ON Payments(HouseId);
CREATE INDEX IX_Payments_HouseCode ON Payments(HouseCode);
CREATE INDEX IX_Payments_Month ON Payments(Month);
CREATE INDEX IX_Payments_Year ON Payments(Year);
CREATE INDEX IX_Payments_PaymentDate ON Payments(PaymentDate);
CREATE INDEX IX_Payments_Status ON Payments(Status);
```

---

### 6. Receipts

**Purpose**: Store PDF receipt information and signature metadata

#### Supabase Schema
```sql
CREATE TABLE receipts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    payment_id UUID NOT NULL REFERENCES payments(id) ON DELETE CASCADE,
    file_path TEXT NOT NULL, -- Local file path
    cloud_storage_path TEXT, -- Cloud storage path
    file_name TEXT NOT NULL,
    file_size BIGINT NOT NULL,
    mime_type TEXT NOT NULL DEFAULT 'application/pdf',
    generated_by UUID NOT NULL REFERENCES users(id),
    signature_user_id UUID REFERENCES users(id), -- User whose signature is on receipt
    generated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ
);

CREATE INDEX idx_receipts_payment_id ON receipts(payment_id);
CREATE INDEX idx_receipts_generated_by ON receipts(generated_by);
CREATE INDEX idx_receipts_signature_user_id ON receipts(signature_user_id);
CREATE INDEX idx_receipts_generated_at ON receipts(generated_at);
```

#### SQLite Schema
```sql
CREATE TABLE Receipts (
    Id TEXT PRIMARY KEY,
    PaymentId TEXT NOT NULL,
    FilePath TEXT NOT NULL,
    CloudStoragePath TEXT,
    FileName TEXT NOT NULL,
    FileSize INTEGER NOT NULL,
    MimeType TEXT NOT NULL DEFAULT 'application/pdf',
    GeneratedBy TEXT NOT NULL,
    SignatureUserId TEXT,
    GeneratedAt TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    DeletedAt TEXT,
    LastSyncAt TEXT,
    CloudId TEXT,
    SyncStatus TEXT NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (PaymentId) REFERENCES Payments(Id),
    FOREIGN KEY (GeneratedBy) REFERENCES Users(Id),
    FOREIGN KEY (SignatureUserId) REFERENCES Users(Id)
);

CREATE INDEX IX_Receipts_PaymentId ON Receipts(PaymentId);
CREATE INDEX IX_Receipts_GeneratedBy ON Receipts(GeneratedBy);
```

---

### 7. Expenses

**Purpose**: Store expenses (maintenance costs, utilities, etc.)

#### Supabase Schema
```sql
CREATE TABLE expenses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    description TEXT NOT NULL,
    category TEXT NOT NULL, -- 'Maintenance', 'Utilities', 'Insurance', 'Other'
    amount DECIMAL(10,2) NOT NULL,
    expense_date DATE NOT NULL,
    month TEXT NOT NULL, -- Format: 'YYYY-MM'
    year INTEGER NOT NULL,
    recorded_by UUID NOT NULL REFERENCES users(id),
    maintenance_id UUID REFERENCES maintenance(id) ON DELETE SET NULL,
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,
    CONSTRAINT chk_expense_month_format CHECK (month ~ '^[0-9]{4}-[0-9]{2}$'),
    CONSTRAINT chk_expense_category CHECK (category IN ('Maintenance', 'Utilities', 'Insurance', 'Other'))
);

CREATE INDEX idx_expenses_category ON expenses(category);
CREATE INDEX idx_expenses_month ON expenses(month);
CREATE INDEX idx_expenses_year ON expenses(year);
CREATE INDEX idx_expenses_expense_date ON expenses(expense_date);
CREATE INDEX idx_expenses_recorded_by ON expenses(recorded_by);
CREATE INDEX idx_expenses_maintenance_id ON expenses(maintenance_id);
CREATE INDEX idx_expenses_deleted_at ON expenses(deleted_at);
```

#### SQLite Schema
```sql
CREATE TABLE Expenses (
    Id TEXT PRIMARY KEY,
    Description TEXT NOT NULL,
    Category TEXT NOT NULL,
    Amount REAL NOT NULL,
    ExpenseDate TEXT NOT NULL,
    Month TEXT NOT NULL,
    Year INTEGER NOT NULL,
    RecordedBy TEXT NOT NULL,
    MaintenanceId TEXT,
    Notes TEXT,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    DeletedAt TEXT,
    LastSyncAt TEXT,
    CloudId TEXT,
    SyncStatus TEXT NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (RecordedBy) REFERENCES Users(Id),
    FOREIGN KEY (MaintenanceId) REFERENCES Maintenance(Id),
    CONSTRAINT CHK_ExpenseMonthFormat CHECK (Month GLOB '[0-9][0-9][0-9][0-9]-[0-9][0-9]'),
    CONSTRAINT CHK_ExpenseCategory CHECK (Category IN ('Maintenance', 'Utilities', 'Insurance', 'Other'))
);

CREATE INDEX IX_Expenses_Category ON Expenses(Category);
CREATE INDEX IX_Expenses_Month ON Expenses(Month);
CREATE INDEX IX_Expenses_Year ON Expenses(Year);
CREATE INDEX IX_Expenses_ExpenseDate ON Expenses(ExpenseDate);
```

---

### 8. Maintenance

**Purpose**: Store maintenance requests and work orders

#### Supabase Schema
```sql
CREATE TABLE maintenance (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    description TEXT NOT NULL,
    type TEXT NOT NULL, -- 'Repair', 'Cleaning', 'Security', 'Other'
    cost DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    status TEXT NOT NULL DEFAULT 'Pending', -- 'Pending', 'InProgress', 'Completed', 'Cancelled'
    priority TEXT DEFAULT 'Normal', -- 'Low', 'Normal', 'High', 'Urgent'
    created_by UUID NOT NULL REFERENCES users(id),
    assigned_to UUID REFERENCES users(id),
    started_at TIMESTAMPTZ,
    completed_at TIMESTAMPTZ,
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,
    CONSTRAINT chk_maintenance_type CHECK (type IN ('Repair', 'Cleaning', 'Security', 'Other')),
    CONSTRAINT chk_maintenance_status CHECK (status IN ('Pending', 'InProgress', 'Completed', 'Cancelled')),
    CONSTRAINT chk_maintenance_priority CHECK (priority IN ('Low', 'Normal', 'High', 'Urgent'))
);

CREATE INDEX idx_maintenance_type ON maintenance(type);
CREATE INDEX idx_maintenance_status ON maintenance(status);
CREATE INDEX idx_maintenance_priority ON maintenance(priority);
CREATE INDEX idx_maintenance_created_by ON maintenance(created_by);
CREATE INDEX idx_maintenance_assigned_to ON maintenance(assigned_to);
CREATE INDEX idx_maintenance_created_at ON maintenance(created_at);
CREATE INDEX idx_maintenance_deleted_at ON maintenance(deleted_at);
```

#### SQLite Schema
```sql
CREATE TABLE Maintenance (
    Id TEXT PRIMARY KEY,
    Description TEXT NOT NULL,
    Type TEXT NOT NULL,
    Cost REAL NOT NULL DEFAULT 0.00,
    Status TEXT NOT NULL DEFAULT 'Pending',
    Priority TEXT DEFAULT 'Normal',
    CreatedBy TEXT NOT NULL,
    AssignedTo TEXT,
    StartedAt TEXT,
    CompletedAt TEXT,
    Notes TEXT,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    DeletedAt TEXT,
    LastSyncAt TEXT,
    CloudId TEXT,
    SyncStatus TEXT NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    FOREIGN KEY (AssignedTo) REFERENCES Users(Id),
    CONSTRAINT CHK_MaintenanceType CHECK (Type IN ('Repair', 'Cleaning', 'Security', 'Other')),
    CONSTRAINT CHK_MaintenanceStatus CHECK (Status IN ('Pending', 'InProgress', 'Completed', 'Cancelled')),
    CONSTRAINT CHK_MaintenancePriority CHECK (Priority IN ('Low', 'Normal', 'High', 'Urgent'))
);

CREATE INDEX IX_Maintenance_Type ON Maintenance(Type);
CREATE INDEX IX_Maintenance_Status ON Maintenance(Status);
CREATE INDEX IX_Maintenance_Priority ON Maintenance(Priority);
CREATE INDEX IX_Maintenance_CreatedBy ON Maintenance(CreatedBy);
CREATE INDEX IX_Maintenance_CreatedAt ON Maintenance(CreatedAt);
```

---

### 9. MaintenanceDocuments

**Purpose**: Store justificative documents attached to maintenance requests

#### Supabase Schema
```sql
CREATE TABLE maintenance_documents (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    maintenance_id UUID NOT NULL REFERENCES maintenance(id) ON DELETE CASCADE,
    file_path TEXT NOT NULL, -- Local file path
    cloud_storage_path TEXT, -- Cloud storage path
    file_name TEXT NOT NULL,
    file_size BIGINT NOT NULL,
    mime_type TEXT NOT NULL,
    document_type TEXT NOT NULL, -- 'Justificative', 'Invoice', 'Photo', 'Other'
    uploaded_by UUID NOT NULL REFERENCES users(id),
    uploaded_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,
    CONSTRAINT chk_document_type CHECK (document_type IN ('Justificative', 'Invoice', 'Photo', 'Other'))
);

CREATE INDEX idx_maintenance_documents_maintenance_id ON maintenance_documents(maintenance_id);
CREATE INDEX idx_maintenance_documents_uploaded_by ON maintenance_documents(uploaded_by);
CREATE INDEX idx_maintenance_documents_document_type ON maintenance_documents(document_type);
CREATE INDEX idx_maintenance_documents_uploaded_at ON maintenance_documents(uploaded_at);
```

#### SQLite Schema
```sql
CREATE TABLE MaintenanceDocuments (
    Id TEXT PRIMARY KEY,
    MaintenanceId TEXT NOT NULL,
    FilePath TEXT NOT NULL,
    CloudStoragePath TEXT,
    FileName TEXT NOT NULL,
    FileSize INTEGER NOT NULL,
    MimeType TEXT NOT NULL,
    DocumentType TEXT NOT NULL,
    UploadedBy TEXT NOT NULL,
    UploadedAt TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    DeletedAt TEXT,
    LastSyncAt TEXT,
    CloudId TEXT,
    SyncStatus TEXT NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (MaintenanceId) REFERENCES Maintenance(Id),
    FOREIGN KEY (UploadedBy) REFERENCES Users(Id),
    CONSTRAINT CHK_DocumentType CHECK (DocumentType IN ('Justificative', 'Invoice', 'Photo', 'Other'))
);

CREATE INDEX IX_MaintenanceDocuments_MaintenanceId ON MaintenanceDocuments(MaintenanceId);
CREATE INDEX IX_MaintenanceDocuments_UploadedBy ON MaintenanceDocuments(UploadedBy);
```

---

### 10. Notifications

**Purpose**: Store system notifications (e.g., unpaid houses)

#### Supabase Schema
```sql
CREATE TABLE notifications (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(id) ON DELETE CASCADE, -- NULL = all users
    type TEXT NOT NULL, -- 'UnpaidHouse', 'MaintenanceDue', 'System', 'Info'
    title TEXT NOT NULL,
    message TEXT NOT NULL,
    related_entity_type TEXT, -- 'Payment', 'Maintenance', etc.
    related_entity_id UUID,
    is_read BOOLEAN NOT NULL DEFAULT false,
    read_at TIMESTAMPTZ,
    priority TEXT DEFAULT 'Normal', -- 'Low', 'Normal', 'High', 'Urgent'
    expires_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_notification_type CHECK (type IN ('UnpaidHouse', 'MaintenanceDue', 'System', 'Info')),
    CONSTRAINT chk_notification_priority CHECK (priority IN ('Low', 'Normal', 'High', 'Urgent'))
);

CREATE INDEX idx_notifications_user_id ON notifications(user_id);
CREATE INDEX idx_notifications_type ON notifications(type);
CREATE INDEX idx_notifications_is_read ON notifications(is_read);
CREATE INDEX idx_notifications_created_at ON notifications(created_at);
CREATE INDEX idx_notifications_expires_at ON notifications(expires_at);
```

#### SQLite Schema
```sql
CREATE TABLE Notifications (
    Id TEXT PRIMARY KEY,
    UserId TEXT,
    Type TEXT NOT NULL,
    Title TEXT NOT NULL,
    Message TEXT NOT NULL,
    RelatedEntityType TEXT,
    RelatedEntityId TEXT,
    IsRead INTEGER NOT NULL DEFAULT 0,
    ReadAt TEXT,
    Priority TEXT DEFAULT 'Normal',
    ExpiresAt TEXT,
    CreatedAt TEXT NOT NULL,
    LastSyncAt TEXT,
    CloudId TEXT,
    SyncStatus TEXT NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT CHK_NotificationType CHECK (Type IN ('UnpaidHouse', 'MaintenanceDue', 'System', 'Info')),
    CONSTRAINT CHK_NotificationPriority CHECK (Priority IN ('Low', 'Normal', 'High', 'Urgent'))
);

CREATE INDEX IX_Notifications_UserId ON Notifications(UserId);
CREATE INDEX IX_Notifications_Type ON Notifications(Type);
CREATE INDEX IX_Notifications_IsRead ON Notifications(IsRead);
CREATE INDEX IX_Notifications_CreatedAt ON Notifications(CreatedAt);
```

---

### 11. AuditLogs

**Purpose**: Store audit trail of all user actions

#### Supabase Schema
```sql
CREATE TABLE audit_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id),
    action TEXT NOT NULL, -- 'Create', 'Update', 'Delete', 'View', 'Login', 'Logout', 'Export', 'Backup', 'Restore'
    entity_type TEXT NOT NULL, -- 'Payment', 'Maintenance', 'User', etc.
    entity_id UUID,
    details JSONB, -- Additional details as JSON
    ip_address INET,
    user_agent TEXT,
    timestamp TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_audit_action CHECK (action IN ('Create', 'Update', 'Delete', 'View', 'Login', 'Logout', 'Export', 'Backup', 'Restore'))
);

CREATE INDEX idx_audit_logs_user_id ON audit_logs(user_id);
CREATE INDEX idx_audit_logs_action ON audit_logs(action);
CREATE INDEX idx_audit_logs_entity_type ON audit_logs(entity_type);
CREATE INDEX idx_audit_logs_entity_id ON audit_logs(entity_id);
CREATE INDEX idx_audit_logs_timestamp ON audit_logs(timestamp);
CREATE INDEX idx_audit_logs_timestamp_desc ON audit_logs(timestamp DESC);
```

#### SQLite Schema
```sql
CREATE TABLE AuditLogs (
    Id TEXT PRIMARY KEY,
    UserId TEXT NOT NULL,
    Action TEXT NOT NULL,
    EntityType TEXT NOT NULL,
    EntityId TEXT,
    Details TEXT, -- JSON string
    IpAddress TEXT,
    UserAgent TEXT,
    Timestamp TEXT NOT NULL,
    LastSyncAt TEXT,
    CloudId TEXT,
    SyncStatus TEXT NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT CHK_AuditAction CHECK (Action IN ('Create', 'Update', 'Delete', 'View', 'Login', 'Logout', 'Export', 'Backup', 'Restore'))
);

CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_Action ON AuditLogs(Action);
CREATE INDEX IX_AuditLogs_EntityType ON AuditLogs(EntityType);
CREATE INDEX IX_AuditLogs_Timestamp ON AuditLogs(Timestamp);
```

---

### 12. Settings

**Purpose**: Store application settings and configuration

#### Supabase Schema
```sql
CREATE TABLE settings (
    key TEXT PRIMARY KEY,
    value TEXT NOT NULL,
    type TEXT NOT NULL, -- 'String', 'Int', 'Bool', 'DateTime', 'Json'
    description TEXT,
    category TEXT, -- 'General', 'Sync', 'Backup', 'UI', etc.
    updated_by UUID REFERENCES users(id),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_setting_type CHECK (type IN ('String', 'Int', 'Bool', 'DateTime', 'Json'))
);

CREATE INDEX idx_settings_category ON settings(category);
```

#### SQLite Schema
```sql
CREATE TABLE Settings (
    Key TEXT PRIMARY KEY,
    Value TEXT NOT NULL,
    Type TEXT NOT NULL,
    Description TEXT,
    Category TEXT,
    UpdatedBy TEXT,
    UpdatedAt TEXT NOT NULL,
    LastSyncAt TEXT,
    CloudId TEXT,
    SyncStatus TEXT NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    CONSTRAINT CHK_SettingType CHECK (Type IN ('String', 'Int', 'Bool', 'DateTime', 'Json'))
);

CREATE INDEX IX_Settings_Category ON Settings(Category);
```

---

### 13. Backups

**Purpose**: Track backup operations and metadata

#### Supabase Schema
```sql
CREATE TABLE backups (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    backup_type TEXT NOT NULL, -- 'Full', 'Database', 'Documents'
    file_path TEXT NOT NULL,
    cloud_storage_path TEXT,
    file_size BIGINT NOT NULL,
    created_by UUID NOT NULL REFERENCES users(id),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    expires_at TIMESTAMPTZ,
    is_automatic BOOLEAN NOT NULL DEFAULT false,
    notes TEXT,
    CONSTRAINT chk_backup_type CHECK (backup_type IN ('Full', 'Database', 'Documents'))
);

CREATE INDEX idx_backups_created_by ON backups(created_by);
CREATE INDEX idx_backups_created_at ON backups(created_at);
CREATE INDEX idx_backups_expires_at ON backups(expires_at);
```

#### SQLite Schema
```sql
CREATE TABLE Backups (
    Id TEXT PRIMARY KEY,
    BackupType TEXT NOT NULL,
    FilePath TEXT NOT NULL,
    CloudStoragePath TEXT,
    FileSize INTEGER NOT NULL,
    CreatedBy TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    ExpiresAt TEXT,
    IsAutomatic INTEGER NOT NULL DEFAULT 0,
    Notes TEXT,
    LastSyncAt TEXT,
    CloudId TEXT,
    SyncStatus TEXT NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT CHK_BackupType CHECK (BackupType IN ('Full', 'Database', 'Documents'))
);

CREATE INDEX IX_Backups_CreatedBy ON Backups(CreatedBy);
CREATE INDEX IX_Backups_CreatedAt ON Backups(CreatedAt);
```

---

### 14. SyncQueue

**Purpose**: Track pending changes for synchronization

#### Supabase Schema
```sql
-- Note: This table is local-only, not synced to cloud
```

#### SQLite Schema
```sql
CREATE TABLE SyncQueue (
    Id TEXT PRIMARY KEY,
    EntityType TEXT NOT NULL,
    EntityId TEXT NOT NULL,
    Operation TEXT NOT NULL, -- 'Create', 'Update', 'Delete'
    Data TEXT NOT NULL, -- JSON serialized entity
    CreatedAt TEXT NOT NULL,
    RetryCount INTEGER NOT NULL DEFAULT 0,
    LastError TEXT,
    LastAttemptAt TEXT,
    Status TEXT NOT NULL DEFAULT 'Pending', -- 'Pending', 'Processing', 'Failed', 'Completed'
    Priority INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT CHK_SyncOperation CHECK (Operation IN ('Create', 'Update', 'Delete')),
    CONSTRAINT CHK_SyncStatus CHECK (Status IN ('Pending', 'Processing', 'Failed', 'Completed'))
);

CREATE INDEX IX_SyncQueue_Status ON SyncQueue(Status);
CREATE INDEX IX_SyncQueue_CreatedAt ON SyncQueue(CreatedAt);
CREATE INDEX IX_SyncQueue_Priority ON SyncQueue(Priority DESC, CreatedAt);
```

---

## Supabase Row Level Security (RLS) Policies

```sql
-- Enable RLS on all tables
ALTER TABLE residences ENABLE ROW LEVEL SECURITY;
ALTER TABLE buildings ENABLE ROW LEVEL SECURITY;
ALTER TABLE houses ENABLE ROW LEVEL SECURITY;
ALTER TABLE users ENABLE ROW LEVEL SECURITY;
ALTER TABLE payments ENABLE ROW LEVEL SECURITY;
ALTER TABLE receipts ENABLE ROW LEVEL SECURITY;
ALTER TABLE expenses ENABLE ROW LEVEL SECURITY;
ALTER TABLE maintenance ENABLE ROW LEVEL SECURITY;
ALTER TABLE maintenance_documents ENABLE ROW LEVEL SECURITY;
ALTER TABLE notifications ENABLE ROW LEVEL SECURITY;
ALTER TABLE audit_logs ENABLE ROW LEVEL SECURITY;
ALTER TABLE settings ENABLE ROW LEVEL SECURITY;
ALTER TABLE backups ENABLE ROW LEVEL SECURITY;

-- Example policies (adjust based on your auth setup)
-- Users can view their own data
CREATE POLICY "Users can view own data" ON users
    FOR SELECT USING (auth.uid() = id);

-- Authenticated users can view payments
CREATE POLICY "Authenticated users can view payments" ON payments
    FOR SELECT USING (auth.role() = 'authenticated');

-- Only admins can manage users
CREATE POLICY "Admins can manage users" ON users
    FOR ALL USING (
        EXISTS (
            SELECT 1 FROM users
            WHERE id = auth.uid() AND role = 'Admin'
        )
    );
```

---

## Seed Data

See `SEED_DATA.md` for complete seed data scripts.

---

## Summary

This database model provides:
- ✅ Complete schema for both SQLite and Supabase
- ✅ All required tables with relationships
- ✅ Indexes for performance
- ✅ Constraints and validations
- ✅ Sync metadata fields
- ✅ RLS policies for Supabase
- ✅ Ready for EF Core implementation

