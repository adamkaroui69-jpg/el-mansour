-- El Mansour Syndic Manager - Supabase Database Schema
-- Run this script in Supabase SQL Editor to create all tables

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 1. Residences
CREATE TABLE IF NOT EXISTS residences (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT NOT NULL,
    address TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 2. Buildings
CREATE TABLE IF NOT EXISTS buildings (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    residence_id UUID REFERENCES residences(id) ON DELETE CASCADE,
    code TEXT NOT NULL,
    name TEXT,
    floors INTEGER,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(residence_id, code)
);

-- 3. Houses
CREATE TABLE IF NOT EXISTS houses (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    building_id UUID REFERENCES buildings(id) ON DELETE CASCADE,
    code TEXT NOT NULL UNIQUE,
    floor INTEGER,
    type TEXT NOT NULL DEFAULT 'House',
    owner_name TEXT,
    owner_phone TEXT,
    owner_email TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 4. Users
CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    house_id UUID REFERENCES houses(id) ON DELETE SET NULL,
    name TEXT NOT NULL,
    surname TEXT NOT NULL,
    house_code TEXT NOT NULL,
    password_hash TEXT NOT NULL,
    salt TEXT NOT NULL,
    role TEXT NOT NULL DEFAULT 'SyndicMember',
    signature_path TEXT,
    signature_cloud_path TEXT,
    is_active BOOLEAN NOT NULL DEFAULT true,
    last_login_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_user_role CHECK (role IN ('Admin', 'SyndicMember')),
    UNIQUE(house_code)
);

-- 5. Payments
CREATE TABLE IF NOT EXISTS payments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    house_id UUID REFERENCES houses(id) ON DELETE CASCADE,
    house_code TEXT NOT NULL,
    amount DECIMAL(10,2) NOT NULL,
    payment_date DATE NOT NULL,
    month TEXT NOT NULL,
    year INTEGER NOT NULL,
    receipt_id UUID,
    recorded_by UUID REFERENCES users(id) ON DELETE RESTRICT,
    status TEXT NOT NULL DEFAULT 'Paid',
    payment_method TEXT,
    reference_number TEXT,
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_payment_month_format CHECK (month ~ '^[0-9]{4}-[0-9]{2}$'),
    CONSTRAINT chk_payment_status CHECK (status IN ('Paid', 'Unpaid', 'Overdue', 'Cancelled')),
    UNIQUE(house_code, month)
);

-- 6. Receipts
CREATE TABLE IF NOT EXISTS receipts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    payment_id UUID REFERENCES payments(id) ON DELETE SET NULL,
    file_path TEXT NOT NULL,
    cloud_storage_path TEXT,
    file_name TEXT NOT NULL,
    file_size BIGINT NOT NULL,
    mime_type TEXT NOT NULL DEFAULT 'application/pdf',
    generated_by UUID REFERENCES users(id) ON DELETE RESTRICT,
    signature_user_id UUID REFERENCES users(id) ON DELETE SET NULL,
    generated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 7. Expenses
CREATE TABLE IF NOT EXISTS expenses (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    description TEXT NOT NULL,
    category TEXT NOT NULL,
    amount DECIMAL(10,2) NOT NULL,
    expense_date DATE NOT NULL,
    month TEXT NOT NULL,
    year INTEGER NOT NULL,
    recorded_by UUID REFERENCES users(id) ON DELETE RESTRICT,
    maintenance_id UUID,
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 8. Maintenance
CREATE TABLE IF NOT EXISTS maintenance (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    description TEXT NOT NULL,
    type TEXT NOT NULL,
    status TEXT NOT NULL DEFAULT 'Pending',
    priority TEXT NOT NULL DEFAULT 'Normal',
    cost DECIMAL(10,2) DEFAULT 0,
    created_by UUID REFERENCES users(id) ON DELETE RESTRICT,
    assigned_to UUID REFERENCES users(id) ON DELETE SET NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    completed_at TIMESTAMPTZ,
    notes TEXT,
    CONSTRAINT chk_maintenance_type CHECK (type IN ('Repair', 'Cleaning', 'Security', 'Other')),
    CONSTRAINT chk_maintenance_status CHECK (status IN ('Pending', 'InProgress', 'Completed', 'Cancelled')),
    CONSTRAINT chk_maintenance_priority CHECK (priority IN ('Low', 'Normal', 'High', 'Urgent'))
);

-- 9. Documents
CREATE TABLE IF NOT EXISTS documents (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    file_name TEXT NOT NULL,
    file_path TEXT NOT NULL,
    cloud_storage_path TEXT,
    file_size BIGINT NOT NULL,
    mime_type TEXT NOT NULL,
    document_type TEXT NOT NULL,
    uploaded_by UUID REFERENCES users(id) ON DELETE RESTRICT,
    related_entity_type TEXT,
    related_entity_id UUID,
    uploaded_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 10. Notifications
CREATE TABLE IF NOT EXISTS notifications (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    type TEXT NOT NULL,
    title TEXT NOT NULL,
    message TEXT NOT NULL,
    related_entity_type TEXT,
    related_entity_id UUID,
    is_read BOOLEAN NOT NULL DEFAULT false,
    read_at TIMESTAMPTZ,
    priority TEXT DEFAULT 'Normal',
    expires_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_notification_type CHECK (type IN ('UnpaidHouse', 'MaintenanceDue', 'System', 'Info')),
    CONSTRAINT chk_notification_priority CHECK (priority IN ('Low', 'Normal', 'High', 'Urgent'))
);

-- 11. Audit Logs
CREATE TABLE IF NOT EXISTS audit_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    action TEXT NOT NULL,
    entity_type TEXT NOT NULL,
    entity_id UUID,
    details JSONB,
    ip_address INET,
    user_agent TEXT,
    timestamp TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_audit_action CHECK (action IN ('Create', 'Update', 'Delete', 'View', 'Login', 'Logout', 'Export', 'Backup', 'Restore'))
);

-- 12. Backups
CREATE TABLE IF NOT EXISTS backups (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    backup_type TEXT NOT NULL,
    file_path TEXT NOT NULL,
    cloud_storage_path TEXT,
    file_size BIGINT NOT NULL,
    created_by UUID NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    expires_at TIMESTAMPTZ,
    is_automatic BOOLEAN NOT NULL DEFAULT false,
    notes TEXT,
    CONSTRAINT chk_backup_type CHECK (backup_type IN ('Full', 'Database', 'Documents'))
);

-- 13. Settings
CREATE TABLE IF NOT EXISTS settings (
    key TEXT PRIMARY KEY,
    value TEXT NOT NULL,
    type TEXT NOT NULL,
    description TEXT,
    category TEXT,
    updated_by UUID REFERENCES users(id) ON DELETE SET NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_setting_type CHECK (type IN ('String', 'Int', 'Bool', 'DateTime', 'Json'))
);

-- Create Indexes
CREATE INDEX IF NOT EXISTS idx_buildings_residence_id ON buildings(residence_id);
CREATE INDEX IF NOT EXISTS idx_houses_building_id ON houses(building_id);
CREATE INDEX IF NOT EXISTS idx_houses_code ON houses(code);
CREATE INDEX IF NOT EXISTS idx_users_house_code ON users(house_code);
CREATE INDEX IF NOT EXISTS idx_payments_house_code ON payments(house_code);
CREATE INDEX IF NOT EXISTS idx_payments_month ON payments(month);
CREATE INDEX IF NOT EXISTS idx_payments_status ON payments(status);
CREATE INDEX IF NOT EXISTS idx_receipts_payment_id ON receipts(payment_id);
CREATE INDEX IF NOT EXISTS idx_expenses_month ON expenses(month);
CREATE INDEX IF NOT EXISTS idx_notifications_user_id ON notifications(user_id);
CREATE INDEX IF NOT EXISTS idx_notifications_is_read ON notifications(is_read);
CREATE INDEX IF NOT EXISTS idx_audit_logs_user_id ON audit_logs(user_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_timestamp ON audit_logs(timestamp);
CREATE INDEX IF NOT EXISTS idx_backups_created_at ON backups(created_at);

-- Enable Row Level Security (RLS)
ALTER TABLE users ENABLE ROW LEVEL SECURITY;
ALTER TABLE payments ENABLE ROW LEVEL SECURITY;
ALTER TABLE receipts ENABLE ROW LEVEL SECURITY;
ALTER TABLE expenses ENABLE ROW LEVEL SECURITY;
ALTER TABLE maintenance ENABLE ROW LEVEL SECURITY;
ALTER TABLE documents ENABLE ROW LEVEL SECURITY;
ALTER TABLE notifications ENABLE ROW LEVEL SECURITY;
ALTER TABLE audit_logs ENABLE ROW LEVEL SECURITY;
ALTER TABLE backups ENABLE ROW LEVEL SECURITY;
ALTER TABLE settings ENABLE ROW LEVEL SECURITY;

-- RLS Policies (users can only see their own data, admins see all)
-- Note: Adjust these policies based on your security requirements

-- Users can see their own data
CREATE POLICY "Users can view own data" ON users
    FOR SELECT USING (auth.uid() = id);

-- Admins can see all users
CREATE POLICY "Admins can view all users" ON users
    FOR SELECT USING (
        EXISTS (
            SELECT 1 FROM users u
            WHERE u.id = auth.uid() AND u.role = 'Admin'
        )
    );

-- Similar policies for other tables...
-- (Add comprehensive RLS policies based on your requirements)

-- Insert Seed Data
INSERT INTO residences (id, name, address) VALUES
    ('00000000-0000-0000-0000-000000000001', 'El Mansour', 'Casablanca, Morocco')
ON CONFLICT DO NOTHING;

-- Insert Buildings
INSERT INTO buildings (id, residence_id, code, name, floors) VALUES
    ('10000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', 'A', 'Bâtiment A', 3),
    ('10000000-0000-0000-0000-000000000002', '00000000-0000-0000-0000-000000000001', 'B', 'Bâtiment B', 4),
    ('10000000-0000-0000-0000-000000000003', '00000000-0000-0000-0000-000000000001', 'C', 'Bâtiment C', 3),
    ('10000000-0000-0000-0000-000000000004', '00000000-0000-0000-0000-000000000001', 'D', 'Bâtiment D', 3),
    ('10000000-0000-0000-0000-000000000005', '00000000-0000-0000-0000-000000000001', 'E', 'Bâtiment E', 3)
ON CONFLICT DO NOTHING;

-- Insert Houses (example for Building A - 3 floors × 4 houses)
-- Note: Generate all houses programmatically or use a script
-- Building A: A01, A02, A03, A04 (floor 1), A05-A08 (floor 2), A09-A12 (floor 3)
-- Building B: B01-B04 (floor 1), B05-B08 (floor 2), B09-B12 (floor 3), B-SYNDIC, B-CONCIERGE, M02, M03 (floor 4)
-- Building C, D, E: Similar pattern
-- Shop M01: Building A ground floor

-- Example: Insert first few houses
-- (Complete this with all houses based on your structure)

-- Create Storage Buckets
-- Note: Run these in Supabase Dashboard > Storage

-- receipts bucket
-- documents bucket  
-- backups bucket
-- reports bucket

