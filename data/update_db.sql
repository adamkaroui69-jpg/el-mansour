-- Vérification et création de la table Users
CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY,
    Username TEXT,
    PasswordHash TEXT,
    Email TEXT,
    Role TEXT,
    CreatedAt DATETIME,
    UpdatedAt DATETIME,
    DeletedAt DATETIME,
    HouseCode TEXT,
    FirstName TEXT,
    LastName TEXT,
    Phone TEXT
);

-- Vérification et création de la table Properties
CREATE TABLE IF NOT EXISTS Properties (
    Id INTEGER PRIMARY KEY,
    Name TEXT,
    Address TEXT,
    OwnerId INTEGER,
    CreatedAt DATETIME,
    UpdatedAt DATETIME,
    DeletedAt DATETIME
);

-- Vérification et création de la table Payments
CREATE TABLE IF NOT EXISTS Payments (
    Id INTEGER PRIMARY KEY,
    PropertyId INTEGER,
    UserId INTEGER,
    Amount REAL,
    PaymentDate DATETIME,
    CreatedAt DATETIME,
    UpdatedAt DATETIME,
    DeletedAt DATETIME
);

-- Vérification et création de la table Receipts
CREATE TABLE IF NOT EXISTS Receipts (
    Id INTEGER PRIMARY KEY,
    PaymentId INTEGER,
    ReceiptFile TEXT,
    CreatedAt DATETIME,
    UpdatedAt DATETIME,
    DeletedAt DATETIME
);

-- Vérification et création de la table Notifications
CREATE TABLE IF NOT EXISTS Notifications (
    Id INTEGER PRIMARY KEY,
    UserId INTEGER,
    Message TEXT,
    IsRead INTEGER,
    CreatedAt DATETIME,
    DeletedAt DATETIME
);

-- Vérification et création de la table Settings
CREATE TABLE IF NOT EXISTS Settings (
    Id INTEGER PRIMARY KEY,
    Key TEXT,
    Value TEXT,
    CreatedAt DATETIME,
    UpdatedAt DATETIME
);

-- Ajout des colonnes manquantes à la table Users
ALTER TABLE Users ADD COLUMN Username TEXT;
ALTER TABLE Users ADD COLUMN PasswordHash TEXT;
ALTER TABLE Users ADD COLUMN Email TEXT;
ALTER TABLE Users ADD COLUMN Role TEXT;
ALTER TABLE Users ADD COLUMN CreatedAt DATETIME;
ALTER TABLE Users ADD COLUMN UpdatedAt DATETIME;
ALTER TABLE Users ADD COLUMN DeletedAt DATETIME;
ALTER TABLE Users ADD COLUMN HouseCode TEXT;
ALTER TABLE Users ADD COLUMN FirstName TEXT;
ALTER TABLE Users ADD COLUMN LastName TEXT;
ALTER TABLE Users ADD COLUMN Phone TEXT;

-- Ajout des colonnes manquantes à la table Properties
ALTER TABLE Properties ADD COLUMN Name TEXT;
ALTER TABLE Properties ADD COLUMN Address TEXT;
ALTER TABLE Properties ADD COLUMN OwnerId INTEGER;
ALTER TABLE Properties ADD COLUMN CreatedAt DATETIME;
ALTER TABLE Properties ADD COLUMN UpdatedAt DATETIME;
ALTER TABLE Properties ADD COLUMN DeletedAt DATETIME;

-- Ajout des colonnes manquantes à la table Payments
ALTER TABLE Payments ADD COLUMN PropertyId INTEGER;
ALTER TABLE Payments ADD COLUMN UserId INTEGER;
ALTER TABLE Payments ADD COLUMN Amount REAL;
ALTER TABLE Payments ADD COLUMN PaymentDate DATETIME;
ALTER TABLE Payments ADD COLUMN CreatedAt DATETIME;
ALTER TABLE Payments ADD COLUMN UpdatedAt DATETIME;
ALTER TABLE Payments ADD COLUMN DeletedAt DATETIME;

-- Ajout des colonnes manquantes à la table Receipts
ALTER TABLE Receipts ADD COLUMN PaymentId INTEGER;
ALTER TABLE Receipts ADD COLUMN ReceiptFile TEXT;
ALTER TABLE Receipts ADD COLUMN CreatedAt DATETIME;
ALTER TABLE Receipts ADD COLUMN UpdatedAt DATETIME;
ALTER TABLE Receipts ADD COLUMN DeletedAt DATETIME;

-- Ajout des colonnes manquantes à la table Notifications
ALTER TABLE Notifications ADD COLUMN UserId INTEGER;
ALTER TABLE Notifications ADD COLUMN Message TEXT;
ALTER TABLE Notifications ADD COLUMN IsRead INTEGER;
ALTER TABLE Notifications ADD COLUMN CreatedAt DATETIME;
ALTER TABLE Notifications ADD COLUMN DeletedAt DATETIME;

-- Ajout des colonnes manquantes à la table Settings
ALTER TABLE Settings ADD COLUMN Key TEXT;
ALTER TABLE Settings ADD COLUMN Value TEXT;
ALTER TABLE Settings ADD COLUMN CreatedAt DATETIME;
ALTER TABLE Settings ADD COLUMN UpdatedAt DATETIME;

-- Rapport des tables et colonnes
SELECT 'Tables mises à jour :';
SELECT name FROM sqlite_master WHERE type='table';
SELECT 'Colonnes mises à jour :';
PRAGMA table_info(Users);
PRAGMA table_info(Properties);
PRAGMA table_info(Payments);
PRAGMA table_info(Receipts);
PRAGMA table_info(Notifications);
PRAGMA table_info(Settings);
