CREATE TABLE IF NOT EXISTS AuditLogs (
    Id TEXT PRIMARY KEY,
    UserId TEXT NOT NULL,
    Action TEXT NOT NULL,
    EntityType TEXT NOT NULL,
    EntityId TEXT NOT NULL,
    Details TEXT NOT NULL,
    IpAddress TEXT NOT NULL,
    UserAgent TEXT NOT NULL,
    Timestamp DATETIME NOT NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL
);

CREATE TABLE IF NOT EXISTS Backups (
    Id TEXT PRIMARY KEY,
    BackupType TEXT NOT NULL,
    FilePath TEXT NOT NULL,
    CloudStoragePath TEXT,
    FileSize INTEGER NOT NULL,
    CreatedBy TEXT NOT NULL,
    CreatedAt DATETIME NOT NULL,
    ExpiresAt DATETIME,
    IsAutomatic INTEGER NOT NULL,
    Notes TEXT,
    UpdatedAt DATETIME NOT NULL
);

CREATE TABLE IF NOT EXISTS Buildings (
    Id TEXT PRIMARY KEY,
    Code TEXT NOT NULL,
    Name TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Expenses (
    Id INTEGER PRIMARY KEY
);

CREATE TABLE IF NOT EXISTS Houses (
    Id TEXT PRIMARY KEY,
    Code TEXT NOT NULL,
    Type TEXT NOT NULL,
    BuildingId TEXT,
    BuildingCode TEXT,
    Floor INTEGER NOT NULL,
    OwnerName TEXT,
    OwnerPhone TEXT,
    OwnerEmail TEXT,
    MonthlyAmount REAL NOT NULL,
    IsActive INTEGER NOT NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL,
    DeletedAt DATETIME
);

-- Add a house with Code "D05" for testing purposes
INSERT INTO Houses (Id, Code, Type, BuildingId, BuildingCode, Floor, OwnerName, OwnerPhone, OwnerEmail, MonthlyAmount, IsActive, CreatedAt, UpdatedAt)
VALUES (
    'a1b2c3d4-e5f6-7890-1234-567890abcdef', -- Example House ID
    'D05',
    'Apartment',
    NULL,
    NULL,
    1,
    'John Doe',
    '123-456-7890',
    'john.doe@example.com',
    100.0,
    1,
    DATETIME('now'),
    DATETIME('now')
);

CREATE TABLE IF NOT EXISTS Maintenances (
    Id INTEGER PRIMARY KEY
);

CREATE TABLE IF NOT EXISTS Notifications (
    Id TEXT PRIMARY KEY,
    UserId TEXT,
    Type TEXT NOT NULL,
    Title TEXT NOT NULL,
    Message TEXT NOT NULL,
    RelatedEntityType TEXT,
    RelatedEntityId TEXT,
    IsRead INTEGER NOT NULL,
    ReadAt DATETIME,
    Priority TEXT NOT NULL,
    ExpiresAt DATETIME,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL
);

CREATE TABLE IF NOT EXISTS Payments (
    Id TEXT PRIMARY KEY,
    HouseId TEXT NOT NULL,
    HouseCode TEXT NOT NULL,
    Amount REAL NOT NULL,
    PaymentDate DATETIME NOT NULL,
    Month TEXT NOT NULL,
    Year INTEGER NOT NULL,
    ReceiptId TEXT,
    RecordedBy TEXT NOT NULL,
    Status TEXT NOT NULL,
    PaymentMethod TEXT NOT NULL,
    ReferenceNumber TEXT,
    Notes TEXT,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL,
    DeletedAt DATETIME
);

CREATE TABLE IF NOT EXISTS Receipts (
    Id TEXT PRIMARY KEY,
    PaymentId TEXT NOT NULL,
    FilePath TEXT NOT NULL,
    CloudStoragePath TEXT,
    FileName TEXT NOT NULL,
    FileSize INTEGER NOT NULL,
    MimeType TEXT NOT NULL,
    GeneratedBy TEXT NOT NULL,
    SignatureUserId TEXT,
    GeneratedAt DATETIME NOT NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL
);

CREATE TABLE IF NOT EXISTS Users (
    Id TEXT PRIMARY KEY,
    HouseId TEXT NOT NULL,
    HouseCode TEXT NOT NULL,
    Username TEXT NOT NULL,
    PasswordHash TEXT NOT NULL,
    Salt TEXT NOT NULL,
    Role TEXT NOT NULL,
    IsActive INTEGER NOT NULL,
    LastLogin DATETIME,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL,
    DeletedAt DATETIME
);
