# El Mansour Syndic Manager - Architecture Document

## 1. Global Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        CLIENT APPLICATION                        │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                    PRESENTATION LAYER                     │   │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌─────────┐ │   │
│  │  │ Dashboard│  │ Payments │  │Maintenance│  │ Reports │ │   │
│  │  │   View   │  │   View   │  │   View    │  │  View   │ │   │
│  │  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘ │   │
│  └───────┼─────────────┼─────────────┼─────────────┼────────┘   │
│          │             │             │             │            │
│  ┌───────▼─────────────▼─────────────▼─────────────▼────────┐  │
│  │                    VIEWMODEL LAYER                        │  │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌─────────┐  │  │
│  │  │Dashboard │  │ Payment  │  │Maintenance│  │ Report  │  │  │
│  │  │ViewModel │  │ViewModel │  │ViewModel  │  │ViewModel│  │  │
│  │  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘  │  │
│  └───────┼─────────────┼─────────────┼─────────────┼─────────┘  │
│          │             │             │             │            │
│  ┌───────▼─────────────▼─────────────▼─────────────▼────────┐  │
│  │                      SERVICE LAYER                        │  │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌─────────┐  │  │
│  │  │ Payment  │  │Maintenance│  │  Report  │  │   Auth │  │  │
│  │  │ Service  │  │  Service  │  │  Service │  │ Service │  │  │
│  │  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘  │  │
│  │  ┌────┴─────┐  ┌────┴─────┐  ┌────┴─────┐  ┌────┴─────┐  │  │
│  │  │   PDF   │  │  Storage  │  │   Sync   │  │  Audit   │  │  │
│  │  │ Service │  │  Service  │  │  Service │  │  Service │  │  │
│  │  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘  │  │
│  └───────┼─────────────┼─────────────┼─────────────┼─────────┘  │
│          │             │             │             │            │
│  ┌───────▼─────────────▼─────────────▼─────────────▼────────┐  │
│  │                    DATA ACCESS LAYER                      │  │
│  │  ┌──────────────────┐         ┌──────────────────┐       │  │
│  │  │  Local SQLite    │◄───────►│  Cloud Backend   │       │  │
│  │  │   Repository     │  Sync   │   (Supabase)     │       │  │
│  │  └──────────────────┘         └──────────────────┘       │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

## 2. Architecture Layers

### 2.1 Presentation Layer (Views)
- **Technology**: WPF with Material Design in XAML Toolkit
- **Pattern**: MVVM (Model-View-ViewModel)
- **Responsibilities**:
  - User interface rendering
  - User input handling
  - Data binding to ViewModels
  - Navigation

### 2.2 ViewModel Layer
- **Pattern**: MVVM with INotifyPropertyChanged
- **Responsibilities**:
  - Business logic coordination
  - Command handling
  - Data transformation for UI
  - Validation
  - Navigation state management

### 2.3 Service Layer
- **Responsibilities**:
  - Business logic implementation
  - Cross-cutting concerns (logging, validation)
  - External service integration
  - Data transformation

### 2.4 Data Access Layer
- **Components**:
  - **Local Repository**: SQLite database operations
  - **Cloud Repository**: Supabase/Firestore operations
  - **Sync Engine**: Bidirectional synchronization
- **Responsibilities**:
  - Data persistence
  - Data synchronization
  - Conflict resolution
  - Offline support

## 3. Core Modules

### 3.1 Authentication Module
- User authentication (6-digit code)
- Role-based access control (Admin, Syndic Member)
- Session management
- Password hashing (BCrypt/PBKDF2)

### 3.2 Payment Module
- Monthly payment tracking
- Payment receipt generation (PDF)
- Payment status management
- Unpaid house notifications

### 3.3 Maintenance Module
- Maintenance request creation
- Cost tracking
- Justificative document attachment
- Status workflow (Pending, In Progress, Completed)

### 3.4 Reporting Module
- Monthly financial reports
- Yearly financial reports
- Statistics and graphs
- Unpaid houses listing
- Export capabilities (PDF, Excel)

### 3.5 User Management Module
- User CRUD operations (Admin only)
- Signature management
- House assignment
- Authentication code management

### 3.6 Document Management Module
- Justificative document upload
- Document storage (local + cloud)
- Document viewing
- Document versioning

### 3.7 Audit Module
- Activity logging
- Change tracking
- Audit trail generation
- Compliance reporting

### 3.8 Sync Module
- Real-time synchronization
- Conflict resolution
- Offline queue management
- Sync status monitoring

## 4. Services

### 4.1 AuthenticationService
- **Methods**:
  - `AuthenticateAsync(string houseCode, string code)`
  - `LogoutAsync()`
  - `GetCurrentUserAsync()`
  - `ChangePasswordAsync(string newCode)`
  - `ValidateSessionAsync()`

### 4.2 PaymentService
- **Methods**:
  - `RecordPaymentAsync(PaymentDto payment)`
  - `GetPaymentsByHouseAsync(string houseCode, DateTime? from, DateTime? to)`
  - `GetUnpaidHousesAsync(DateTime month)`
  - `GenerateReceiptAsync(PaymentDto payment)`
  - `GetPaymentStatisticsAsync(DateTime from, DateTime to)`

### 4.3 MaintenanceService
- **Methods**:
  - `CreateMaintenanceAsync(MaintenanceDto maintenance)`
  - `UpdateMaintenanceAsync(Guid id, MaintenanceDto maintenance)`
  - `GetMaintenanceAsync(Guid id)`
  - `GetAllMaintenanceAsync(DateTime? from, DateTime? to)`
  - `AttachDocumentAsync(Guid maintenanceId, string filePath)`

### 4.4 ReportService
- **Methods**:
  - `GenerateMonthlyReportAsync(DateTime month)`
  - `GenerateYearlyReportAsync(int year)`
  - `GetFinancialSummaryAsync(DateTime from, DateTime to)`
  - `GetUnpaidHousesReportAsync(DateTime month)`
  - `ExportReportAsync(ReportDto report, ExportFormat format)`

### 4.5 UserService
- **Methods**:
  - `CreateUserAsync(UserDto user)`
  - `UpdateUserAsync(Guid id, UserDto user)`
  - `DeleteUserAsync(Guid id)`
  - `GetAllUsersAsync()`
  - `GetUserByHouseCodeAsync(string houseCode)`

### 4.6 DocumentService
- **Methods**:
  - `UploadDocumentAsync(string filePath, DocumentType type)`
  - `DownloadDocumentAsync(Guid documentId)`
  - `DeleteDocumentAsync(Guid documentId)`
  - `GetDocumentsByTypeAsync(DocumentType type)`

### 4.7 PDFService
- **Methods**:
  - `GenerateReceiptAsync(ReceiptData data)`
  - `GenerateReportAsync(ReportData data)`
  - `MergePdfAsync(List<byte[]> pdfs)`

### 4.8 SyncService
- **Methods**:
  - `SyncAsync()`
  - `GetSyncStatusAsync()`
  - `ResolveConflictAsync(ConflictDto conflict)`
  - `GetPendingChangesAsync()`

### 4.9 AuditService
- **Methods**:
  - `LogActivityAsync(ActivityLogDto log)`
  - `GetAuditLogAsync(DateTime? from, DateTime? to, string userId)`
  - `ExportAuditLogAsync(DateTime from, DateTime to)`

### 4.10 StorageService
- **Methods**:
  - `UploadFileAsync(string localPath, string remotePath)`
  - `DownloadFileAsync(string remotePath, string localPath)`
  - `DeleteFileAsync(string remotePath)`
  - `GetFileUrlAsync(string remotePath)`

## 5. Database Schema

### 5.1 Local SQLite Schema

#### Users Table
```sql
CREATE TABLE Users (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Surname TEXT NOT NULL,
    HouseCode TEXT UNIQUE NOT NULL,
    PasswordHash TEXT NOT NULL,
    Role TEXT NOT NULL, -- 'Admin' or 'SyndicMember'
    SignaturePath TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    LastSyncAt TEXT,
    CloudId TEXT
);
```

#### Payments Table
```sql
CREATE TABLE Payments (
    Id TEXT PRIMARY KEY,
    HouseCode TEXT NOT NULL,
    Amount REAL NOT NULL,
    PaymentDate TEXT NOT NULL,
    Month TEXT NOT NULL, -- Format: YYYY-MM
    Year INTEGER NOT NULL,
    ReceiptPath TEXT,
    RecordedBy TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    LastSyncAt TEXT,
    CloudId TEXT,
    SyncStatus TEXT NOT NULL DEFAULT 'Pending', -- Pending, Synced, Conflict
    FOREIGN KEY (HouseCode) REFERENCES Houses(Code)
);
```

#### Maintenance Table
```sql
CREATE TABLE Maintenance (
    Id TEXT PRIMARY KEY,
    Description TEXT NOT NULL,
    Type TEXT NOT NULL, -- 'Repair', 'Cleaning', 'Security', 'Other'
    Cost REAL NOT NULL,
    Status TEXT NOT NULL, -- 'Pending', 'InProgress', 'Completed', 'Cancelled'
    CreatedBy TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    CompletedAt TEXT,
    LastSyncAt TEXT,
    CloudId TEXT,
    SyncStatus TEXT NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
);
```

#### MaintenanceDocuments Table
```sql
CREATE TABLE MaintenanceDocuments (
    Id TEXT PRIMARY KEY,
    MaintenanceId TEXT NOT NULL,
    DocumentPath TEXT NOT NULL,
    DocumentType TEXT NOT NULL, -- 'Justificative', 'Invoice', 'Other'
    UploadedAt TEXT NOT NULL,
    LastSyncAt TEXT,
    CloudId TEXT,
    SyncStatus TEXT NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (MaintenanceId) REFERENCES Maintenance(Id)
);
```

#### Houses Table
```sql
CREATE TABLE Houses (
    Code TEXT PRIMARY KEY,
    Building TEXT NOT NULL, -- 'A', 'B', 'C', 'D', 'E'
    Floor INTEGER NOT NULL,
    UnitNumber INTEGER NOT NULL,
    Type TEXT NOT NULL, -- 'House', 'Shop', 'Office', 'Concierge'
    MonthlyAmount REAL NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL
);
```

#### AuditLogs Table
```sql
CREATE TABLE AuditLogs (
    Id TEXT PRIMARY KEY,
    UserId TEXT NOT NULL,
    Action TEXT NOT NULL, -- 'Create', 'Update', 'Delete', 'View', 'Login', 'Logout'
    EntityType TEXT NOT NULL, -- 'Payment', 'Maintenance', 'User', etc.
    EntityId TEXT,
    Details TEXT,
    IpAddress TEXT,
    Timestamp TEXT NOT NULL,
    LastSyncAt TEXT,
    CloudId TEXT,
    SyncStatus TEXT NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
```

#### SyncQueue Table
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
    Status TEXT NOT NULL DEFAULT 'Pending' -- Pending, Processing, Failed, Completed
);
```

### 5.2 Cloud Schema (Supabase)

Similar structure with additional fields:
- `created_at` (timestamp)
- `updated_at` (timestamp)
- `deleted_at` (soft delete)
- Row Level Security (RLS) policies
- Real-time subscriptions

## 6. Security Model

### 6.1 Authentication
- **6-digit code**: Hashed using PBKDF2 with 10,000 iterations
- **Session management**: JWT tokens with 24-hour expiration
- **House code validation**: Must match existing house codes

### 6.2 Authorization
- **Admin**: Full access to all features
- **Syndic Member**: 
  - Can record payments
  - Can view reports
  - Can create maintenance requests
  - Cannot manage users
  - Cannot delete critical data

### 6.3 Data Protection
- **Sensitive data encryption**: SQLite database encrypted using SQLCipher
- **Document access**: Role-based document access control
- **Audit trail**: All critical operations logged

### 6.4 Network Security
- **HTTPS only**: All cloud communications over HTTPS
- **API keys**: Stored in secure configuration (encrypted)
- **Token refresh**: Automatic token refresh before expiration

## 7. Sync Strategy

### 7.1 Sync Architecture
```
┌─────────────────┐
│  Local SQLite   │
│   (Primary)     │
└────────┬────────┘
         │
         │ Sync Engine
         │
┌────────▼────────┐
│  Conflict       │
│  Resolution     │
└────────┬────────┘
         │
    ┌────┴────┐
    │         │
┌───▼───┐ ┌──▼────┐
│Cloud  │ │Offline│
│Sync   │ │Queue  │
└───────┘ └───────┘
```

### 7.2 Sync Process

1. **Initial Sync**:
   - Download all data from cloud
   - Merge with local data
   - Resolve conflicts (Last-Write-Wins with manual override option)

2. **Incremental Sync**:
   - Check for changes every 5 minutes
   - Push local changes to cloud
   - Pull cloud changes to local
   - Handle conflicts

3. **Conflict Resolution**:
   - **Strategy**: Last-Write-Wins (LWW) by default
   - **Manual override**: Admin can manually resolve conflicts
   - **Conflict log**: All conflicts logged for review

4. **Offline Mode**:
   - All operations work offline
   - Changes queued in SyncQueue table
   - Automatic sync when connection restored

### 7.3 State Merging Algorithm

```csharp
// Pseudo-code for state merging
async Task MergeStatesAsync()
{
    var localChanges = await GetLocalChangesAsync();
    var cloudChanges = await GetCloudChangesAsync();
    
    foreach (var conflict in DetectConflicts(localChanges, cloudChanges))
    {
        if (conflict.LocalTimestamp > conflict.CloudTimestamp)
        {
            await PushToCloudAsync(conflict.LocalEntity);
        }
        else
        {
            await UpdateLocalAsync(conflict.CloudEntity);
        }
    }
    
    await ApplyNonConflictingChangesAsync(localChanges, cloudChanges);
}
```

## 8. Navigation Flow

### 8.1 Application Flow

```
Login Screen
    │
    ├─► [Invalid Credentials] ──► Error Message
    │
    └─► [Valid Credentials] ──► Dashboard
            │
            ├─► Payments Module
            │       ├─► Record Payment
            │       ├─► View Payments
            │       └─► Generate Receipt
            │
            ├─► Maintenance Module
            │       ├─► Create Maintenance
            │       ├─► View Maintenance
            │       └─► Attach Documents
            │
            ├─► Reports Module
            │       ├─► Monthly Report
            │       ├─► Yearly Report
            │       └─► Statistics
            │
            ├─► User Management (Admin Only)
            │       ├─► Add User
            │       ├─► Edit User
            │       └─► Remove User
            │
            └─► Settings
                    ├─► Sync Status
                    ├─► Backup/Restore
                    └─► Logout
```

### 8.2 Dashboard Wireframe (Text)

```
┌─────────────────────────────────────────────────────────────────┐
│ El Mansour Syndic Manager                    [User] [Settings] [X]│
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │  Total Collected │  │   Total Spent   │  │     Balance     │ │
│  │   This Month     │  │   This Month    │  │   This Month    │ │
│  │                  │  │                  │  │                  │ │
│  │   45,000 MAD     │  │   12,500 MAD     │  │   32,500 MAD    │ │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘ │
│                                                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                    Quick Actions                          │  │
│  │  [Record Payment]  [Create Maintenance]  [Generate Report] │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │              Unpaid Houses (This Month)                   │  │
│  │  ┌──────┬──────────────┬──────────┬──────────────────┐  │  │
│  │  │ Code │    Owner     │  Amount  │    Status        │  │  │
│  │  ├──────┼──────────────┼──────────┼──────────────────┤  │  │
│  │  │ A02  │ Ahmed Benali │ 1,500 MAD│ ⚠️ Overdue       │  │  │
│  │  │ C03  │ Fatima Alami │ 1,500 MAD│ ⚠️ Overdue       │  │  │
│  │  └──────┴──────────────┴──────────┴──────────────────┘  │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │              Recent Payments                               │  │
│  │  [List of last 10 payments with date, house, amount]      │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │              Pending Maintenance                           │  │
│  │  [List of maintenance requests with status]                │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │              Monthly Collection Chart                      │  │
│  │  [Bar chart showing collection over last 12 months]       │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

## 9. Backup Mechanism

### 9.1 Backup Strategy
- **Automatic backups**: Daily at 2 AM
- **Manual backups**: On-demand via Settings
- **Backup location**: Local folder + optional cloud storage
- **Backup content**: 
  - SQLite database file
  - Document attachments
  - Configuration files

### 9.2 Backup Format
- **Database**: SQLite dump (.sql) + binary backup (.db)
- **Documents**: Compressed archive (.zip)
- **Metadata**: JSON file with backup information

### 9.3 Restore Process
1. Select backup file
2. Validate backup integrity
3. Close current database connection
4. Restore database
5. Restore documents
6. Verify data integrity
7. Trigger full sync with cloud

## 10. Technology Stack

### 10.1 Core Technologies
- **.NET 8**: Application framework
- **WPF**: UI framework
- **Material Design in XAML Toolkit**: UI components
- **SQLite**: Local database
- **Supabase .NET Client**: Cloud backend
- **Entity Framework Core**: ORM (optional, or Dapper)
- **iTextSharp/QuestPDF**: PDF generation
- **CommunityToolkit.Mvvm**: MVVM helpers
- **Serilog**: Logging
- **AutoMapper**: Object mapping

### 10.2 NuGet Packages
```
- MaterialDesignThemes
- MaterialDesignColors
- Microsoft.EntityFrameworkCore.Sqlite
- Supabase
- QuestPDF
- CommunityToolkit.Mvvm
- Serilog
- Serilog.Sinks.File
- AutoMapper
- BCrypt.Net-Next
- Newtonsoft.Json
- System.Data.SQLite
```

## 11. Project Structure

See PROJECT_STRUCTURE.md for detailed folder organization.

## 12. Implementation Phases

### Phase 1: Foundation
- Project setup
- Database schema
- Authentication
- Basic UI shell

### Phase 2: Core Features
- Payment management
- Receipt generation
- User management

### Phase 3: Advanced Features
- Maintenance management
- Reporting
- Document management

### Phase 4: Integration
- Cloud sync
- Backup/restore
- Audit logging

### Phase 5: Polish
- UI/UX improvements
- Performance optimization
- Testing
- Documentation

