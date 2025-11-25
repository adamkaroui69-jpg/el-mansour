-- El Mansour Syndic Manager - Seed Data
-- Run this after initial database setup to populate with El Mansour data

-- Insert all houses for El Mansour residence
-- Building A: 3 floors × 4 houses = 12 houses
INSERT INTO houses (id, building_id, code, floor, type, owner_name) VALUES
    -- Floor 1
    ('20000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'A01', 1, 'House', 'Propriétaire A01'),
    ('20000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', 'A02', 1, 'House', 'Propriétaire A02'),
    ('20000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000001', 'A03', 1, 'House', 'Propriétaire A03'),
    ('20000000-0000-0000-0000-000000000004', '10000000-0000-0000-0000-000000000001', 'A04', 1, 'House', 'Propriétaire A04'),
    -- Floor 2
    ('20000000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-000000000001', 'A05', 2, 'House', 'Propriétaire A05'),
    ('20000000-0000-0000-0000-000000000006', '10000000-0000-0000-0000-000000000001', 'A06', 2, 'House', 'Propriétaire A06'),
    ('20000000-0000-0000-0000-000000000007', '10000000-0000-0000-0000-000000000001', 'A07', 2, 'House', 'Propriétaire A07'),
    ('20000000-0000-0000-0000-000000000008', '10000000-0000-0000-0000-000000000001', 'A08', 2, 'House', 'Propriétaire A08'),
    -- Floor 3
    ('20000000-0000-0000-0000-000000000009', '10000000-0000-0000-0000-000000000001', 'A09', 3, 'House', 'Propriétaire A09'),
    ('20000000-0000-0000-0000-000000000010', '10000000-0000-0000-0000-000000000001', 'A10', 3, 'House', 'Propriétaire A10'),
    ('20000000-0000-0000-0000-000000000011', '10000000-0000-0000-0000-000000000001', 'A11', 3, 'House', 'Propriétaire A11'),
    ('20000000-0000-0000-0000-000000000012', '10000000-0000-0000-0000-000000000001', 'A12', 3, 'House', 'Propriétaire A12'),
    -- Shop M01 (Building A ground floor)
    ('20000000-0000-0000-0000-000000000013', '10000000-0000-0000-0000-000000000001', 'M01', 0, 'Shop', 'Propriétaire M01')
ON CONFLICT (code) DO NOTHING;

-- Building B: 4 floors
-- Floor 1-3: 4 houses each = 12 houses
-- Floor 4: Syndic Office + Concierge + Shops M02, M03
INSERT INTO houses (id, building_id, code, floor, type, owner_name) VALUES
    -- Floor 1
    ('20000000-0000-0000-0000-000000000014', '10000000-0000-0000-0000-000000000002', 'B01', 1, 'House', 'Propriétaire B01'),
    ('20000000-0000-0000-0000-000000000015', '10000000-0000-0000-0000-000000000002', 'B02', 1, 'House', 'Propriétaire B02'),
    ('20000000-0000-0000-0000-000000000016', '10000000-0000-0000-0000-000000000002', 'B03', 1, 'House', 'Propriétaire B03'),
    ('20000000-0000-0000-0000-000000000017', '10000000-0000-0000-0000-000000000002', 'B04', 1, 'House', 'Propriétaire B04'),
    -- Floor 2
    ('20000000-0000-0000-0000-000000000018', '10000000-0000-0000-0000-000000000002', 'B05', 2, 'House', 'Propriétaire B05'),
    ('20000000-0000-0000-0000-000000000019', '10000000-0000-0000-0000-000000000002', 'B06', 2, 'House', 'Propriétaire B06'),
    ('20000000-0000-0000-0000-000000000020', '10000000-0000-0000-0000-000000000002', 'B07', 2, 'House', 'Propriétaire B07'),
    ('20000000-0000-0000-0000-000000000021', '10000000-0000-0000-0000-000000000002', 'B08', 2, 'House', 'Propriétaire B08'),
    -- Floor 3
    ('20000000-0000-0000-0000-000000000022', '10000000-0000-0000-0000-000000000002', 'B09', 3, 'House', 'Propriétaire B09'),
    ('20000000-0000-0000-0000-000000000023', '10000000-0000-0000-0000-000000000002', 'B10', 3, 'House', 'Propriétaire B10'),
    ('20000000-0000-0000-0000-000000000024', '10000000-0000-0000-0000-000000000002', 'B11', 3, 'House', 'Propriétaire B11'),
    ('20000000-0000-0000-0000-000000000025', '10000000-0000-0000-0000-000000000002', 'B12', 3, 'House', 'Propriétaire B12'),
    -- Floor 4 (Special units)
    ('20000000-0000-0000-0000-000000000026', '10000000-0000-0000-0000-000000000002', 'B-SYNDIC', 4, 'Office', 'Syndic El Mansour'),
    ('20000000-0000-0000-0000-000000000027', '10000000-0000-0000-0000-000000000002', 'B-CONCIERGE', 4, 'House', 'Concierge'),
    ('20000000-0000-0000-0000-000000000028', '10000000-0000-0000-0000-000000000002', 'M02', 0, 'Shop', 'Propriétaire M02'),
    ('20000000-0000-0000-0000-000000000029', '10000000-0000-0000-0000-000000000002', 'M03', 0, 'Shop', 'Propriétaire M03')
ON CONFLICT (code) DO NOTHING;

-- Building C, D, E: Similar pattern (3 floors × 4 houses = 12 houses each)
-- (Add all houses for buildings C, D, E following the same pattern)

-- Create Admin User
-- Password: Use PBKDF2 hash of 6-digit code (e.g., "123456")
-- Note: Generate actual hash using AuthenticationService
INSERT INTO users (id, house_code, name, surname, password_hash, salt, role, is_active) VALUES
    ('30000000-0000-0000-0000-000000000001', 'B-SYNDIC', 'Admin', 'El Mansour', 
     'R0Mp3qgedXFe+/nRu65dUp/+rveG5h3BNKpLUnqZ2tk=', 'sfCsTJuquMmVMvRnJ7bt41i3DPezJLGSwUtIbL86ZkM=', 'Admin', true)
ON CONFLICT (house_code) DO NOTHING;

-- Create 4 Syndic Members
-- Note: Generate actual password hashes for each member's 6-digit code
INSERT INTO users (id, house_code, name, surname, password_hash, salt, role, is_active) VALUES
    ('30000000-0000-0000-0000-000000000002', 'A01', 'Membre', 'Syndic 1', 'HASH_HERE', 'SALT_HERE', 'SyndicMember', true),
    ('30000000-0000-0000-0000-000000000003', 'B01', 'Membre', 'Syndic 2', 'HASH_HERE', 'SALT_HERE', 'SyndicMember', true),
    ('30000000-0000-0000-0000-000000000004', 'C01', 'Membre', 'Syndic 3', 'HASH_HERE', 'SALT_HERE', 'SyndicMember', true),
    ('30000000-0000-0000-0000-000000000005', 'D01', 'Membre', 'Syndic 4', 'HASH_HERE', 'SALT_HERE', 'SyndicMember', true)
ON CONFLICT (house_code) DO NOTHING;

-- Insert Initial Settings
INSERT INTO settings (key, value, type, description, category) VALUES
    ('BackupSchedule.Enabled', 'true', 'Bool', 'Enable automatic backups', 'Backup'),
    ('BackupSchedule.Time', '02:00:00', 'String', 'Backup time (HH:mm:ss)', 'Backup'),
    ('BackupRetention.Days', '30', 'Int', 'Keep backups for N days', 'Backup'),
    ('Notification.UnpaidHouse.Enabled', 'true', 'Bool', 'Enable unpaid house notifications', 'Notification'),
    ('Report.FiscalYearStart', '01-01', 'String', 'Fiscal year start (MM-DD)', 'Report')
ON CONFLICT (key) DO NOTHING;
