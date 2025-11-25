-- =========================================
-- Script d'initialisation pour local.db
-- =========================================

-- Table des utilisateurs
CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    Email TEXT,
    Role TEXT DEFAULT 'User',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    DeletedAt DATETIME
);

-- Table des propriétés / syndics
CREATE TABLE IF NOT EXISTS Properties (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Address TEXT,
    OwnerId INTEGER,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    DeletedAt DATETIME,
    FOREIGN KEY (OwnerId) REFERENCES Users(Id)
);

-- Table des paiements
CREATE TABLE IF NOT EXISTS Payments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PropertyId INTEGER NOT NULL,
    UserId INTEGER NOT NULL,
    Amount REAL NOT NULL,
    PaymentDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    DeletedAt DATETIME,
    FOREIGN KEY (PropertyId) REFERENCES Properties(Id),
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Table des reçus
CREATE TABLE IF NOT EXISTS Receipts (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PaymentId INTEGER NOT NULL,
    ReceiptFile TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    DeletedAt DATETIME,
    FOREIGN KEY (PaymentId) REFERENCES Payments(Id)
);

-- Table des notifications
CREATE TABLE IF NOT EXISTS Notifications (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER NOT NULL,
    Message TEXT NOT NULL,
    IsRead INTEGER DEFAULT 0,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    DeletedAt DATETIME,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Table des paramètres
CREATE TABLE IF NOT EXISTS Settings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Key TEXT NOT NULL UNIQUE,
    Value TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME
);
