-- El Mansour Syndic Manager - Supabase Storage Buckets Setup
-- Run this in Supabase SQL Editor

-- Create storage buckets
INSERT INTO storage.buckets (id, name, public, file_size_limit, allowed_mime_types)
VALUES
    ('receipts', 'receipts', false, 10485760, ARRAY['application/pdf']),
    ('documents', 'documents', false, 52428800, ARRAY['application/pdf', 'image/png', 'image/jpeg']),
    ('backups', 'backups', false, 1073741824, ARRAY['application/zip']),
    ('reports', 'reports', false, 104857600, ARRAY['application/pdf', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet']),
    ('signatures', 'signatures', false, 5242880, ARRAY['image/png'])
ON CONFLICT (id) DO NOTHING;

-- Storage Policies
-- Allow authenticated users to upload receipts
CREATE POLICY "Users can upload receipts" ON storage.objects
    FOR INSERT WITH CHECK (
        bucket_id = 'receipts' AND
        auth.role() = 'authenticated'
    );

-- Allow users to read receipts
CREATE POLICY "Users can read receipts" ON storage.objects
    FOR SELECT USING (
        bucket_id = 'receipts' AND
        auth.role() = 'authenticated'
    );

-- Similar policies for other buckets...
-- (Add comprehensive storage policies)

