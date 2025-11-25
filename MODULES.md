# El Mansour Syndic Manager - Modules Documentation

## Module Overview

The application is organized into 8 core modules, each handling a specific domain of functionality.

---

## 1. Authentication Module

### Purpose
Handles user authentication, authorization, and session management.

### Components
- **AuthenticationService**: Core authentication logic
- **PasswordHasher**: Secure password hashing (PBKDF2)
- **TokenManager**: JWT token generation and validation
- **LoginViewModel/View**: Login UI

### Features
- 6-digit code authentication
- House code validation
- Role-based access control
- Session management (24-hour expiration)
- Automatic token refresh
- Password change functionality

### Security
- Passwords hashed with PBKDF2 (10,000 iterations)
- Salt stored per user
- JWT tokens with expiration
- Secure session storage

### Dependencies
- UserRepository
- AuditService (for login logging)

---

## 2. Payment Module

### Purpose
Manages monthly payment tracking, receipt generation, and payment status.

### Components
- **PaymentService**: Payment business logic
- **PaymentRepository**: Data access
- **PdfService**: Receipt generation
- **PaymentViewModel/View**: Payment UI
- **PaymentListViewModel/View**: Payment listing UI

### Features
- Record monthly payments
- Generate PDF receipts with signature
- Track payment status (Paid, Unpaid, Overdue)
- View payment history
- Filter by house, date range
- Export payment data

### Business Rules
- Each house must have exactly one payment per month
- Payment amount is fixed per house
- Receipt must include syndic member signature
- Payments cannot be deleted (only marked as cancelled)

### Data Model
```csharp
Payment {
    Id: Guid
    HouseCode: string
    Amount: decimal
    PaymentDate: DateTime
    Month: string (YYYY-MM)
    Year: int
    ReceiptPath: string
    RecordedBy: Guid (UserId)
    CreatedAt: DateTime
    UpdatedAt: DateTime
}
```

### Dependencies
- HouseRepository
- UserRepository
- PdfService
- DocumentService
- AuditService

---

## 3. Maintenance Module

### Purpose
Manages maintenance requests, costs, and justificative documents.

### Components
- **MaintenanceService**: Maintenance business logic
- **MaintenanceRepository**: Data access
- **DocumentService**: Document attachment handling
- **MaintenanceViewModel/View**: Maintenance creation/editing UI
- **MaintenanceListViewModel/View**: Maintenance listing UI

### Features
- Create maintenance requests
- Update maintenance status
- Attach justificative documents (PDF, images)
- Track costs
- Filter by type, status, date range
- View maintenance history

### Business Rules
- Maintenance must have description and type
- Cost is required
- Justificative document recommended for completed maintenance
- Status workflow: Pending → InProgress → Completed

### Data Model
```csharp
Maintenance {
    Id: Guid
    Description: string
    Type: MaintenanceType (Repair, Cleaning, Security, Other)
    Cost: decimal
    Status: MaintenanceStatus (Pending, InProgress, Completed, Cancelled)
    CreatedBy: Guid (UserId)
    CreatedAt: DateTime
    UpdatedAt: DateTime
    CompletedAt: DateTime?
    Documents: List<MaintenanceDocument>
}

MaintenanceDocument {
    Id: Guid
    MaintenanceId: Guid
    DocumentPath: string
    DocumentType: DocumentType (Justificative, Invoice, Other)
    UploadedAt: DateTime
}
```

### Dependencies
- DocumentService
- UserRepository
- AuditService

---

## 4. Reporting Module

### Purpose
Generates financial reports, statistics, and analytics.

### Components
- **ReportService**: Report generation logic
- **ReportViewModel/View**: Report UI
- **StatisticsCalculator**: Statistical calculations
- **ChartGenerator**: Chart/graph generation

### Features
- Monthly financial reports
- Yearly financial reports
- Detailed income/expense breakdown
- Unpaid houses listing
- Statistics and graphs:
  - Collection rate
  - Monthly trends
  - Payment distribution
  - Maintenance costs
- Export to PDF/Excel

### Report Types

#### Monthly Report
- Total collected
- Total spent
- Balance
- List of paid houses
- List of unpaid houses
- Maintenance summary
- Attachments list

#### Yearly Report
- Annual summary
- Monthly breakdown
- Year-over-year comparison
- Statistics and trends

### Dependencies
- PaymentService
- MaintenanceService
- PdfService
- HouseRepository

---

## 5. User Management Module

### Purpose
Manages users (Admin only), including creation, modification, and deletion.

### Components
- **UserService**: User business logic
- **UserRepository**: Data access
- **UserManagementViewModel/View**: User management UI

### Features
- Create new users (Syndic Members)
- Update user information
- Delete users (soft delete)
- Assign house codes
- Manage signatures (PNG upload)
- Set authentication codes
- View user list

### Business Rules
- Only Admin can manage users
- Maximum 4 Syndic Members
- House code must be unique
- Signature is required for Syndic Members
- Cannot delete the Admin user
- Cannot delete users with recorded payments

### Data Model
```csharp
User {
    Id: Guid
    Name: string
    Surname: string
    HouseCode: string (unique)
    PasswordHash: string
    Role: UserRole (Admin, SyndicMember)
    SignaturePath: string
    IsActive: bool
    CreatedAt: DateTime
    UpdatedAt: DateTime
}
```

### Dependencies
- PasswordHasher
- DocumentService
- AuditService

---

## 6. Document Management Module

### Purpose
Handles document upload, storage, viewing, and synchronization.

### Components
- **DocumentService**: Document business logic
- **StorageService**: File storage abstraction
- **LocalFileStorage**: Local file storage
- **CloudFileStorage**: Cloud file storage
- **DocumentViewer**: Document viewing UI

### Features
- Upload documents (PDF, PNG, JPG)
- Store documents locally and in cloud
- View documents
- Delete documents
- Document versioning
- Document access control

### Supported Formats
- PDF (receipts, justificatives)
- PNG (signatures, images)
- JPG (images)

### Storage Strategy
- Local: `data/documents/{type}/{id}/`
- Cloud: Supabase Storage buckets
- Automatic sync on upload

### Dependencies
- StorageService
- SyncService

---

## 7. Audit Module

### Purpose
Tracks all user activities and system changes for compliance and debugging.

### Components
- **AuditService**: Audit logging logic
- **AuditLogRepository**: Data access
- **AuditLogger**: Logging implementation

### Features
- Log all user actions
- Track data changes (Create, Update, Delete)
- Record login/logout events
- Store IP addresses
- Export audit logs
- Filter by user, date, action type

### Logged Actions
- Login/Logout
- Payment creation/update
- Maintenance creation/update
- User management
- Report generation
- Document upload/download
- Settings changes

### Data Model
```csharp
AuditLog {
    Id: Guid
    UserId: Guid
    Action: ActionType (Create, Update, Delete, View, Login, Logout)
    EntityType: string (Payment, Maintenance, User, etc.)
    EntityId: Guid?
    Details: string (JSON)
    IpAddress: string
    Timestamp: DateTime
}
```

### Dependencies
- None (used by all other modules)

---

## 8. Sync Module

### Purpose
Handles bidirectional synchronization between local SQLite and cloud (Supabase).

### Components
- **SyncService**: Sync orchestration
- **SyncEngine**: Core sync logic
- **ConflictResolver**: Conflict resolution
- **OfflineQueueManager**: Offline change queue
- **SyncStatusViewModel/View**: Sync status UI

### Features
- Real-time synchronization
- Automatic sync every 5 minutes
- Manual sync trigger
- Conflict detection and resolution
- Offline queue management
- Sync status monitoring
- Last-Write-Wins (LWW) strategy
- Manual conflict resolution

### Sync Process

1. **Push Phase**:
   - Get local changes from SyncQueue
   - Push to cloud
   - Update sync status

2. **Pull Phase**:
   - Get cloud changes
   - Detect conflicts
   - Resolve conflicts
   - Apply changes to local

3. **Conflict Resolution**:
   - Compare timestamps
   - Apply LWW by default
   - Flag conflicts for manual review
   - Log all conflicts

### Sync Entities
- Users
- Payments
- Maintenance
- MaintenanceDocuments
- AuditLogs

### Dependencies
- All repositories
- CloudClient
- AuditService

---

## Module Dependencies Graph

```
Authentication Module
    ↓
Payment Module ──→ Document Module
    ↓                    ↓
Maintenance Module ──────┘
    ↓
Reporting Module
    ↓
User Management Module
    ↓
All Modules ──→ Audit Module
    ↓
Sync Module ──→ Cloud Backend
```

---

## Module Communication

### Direct Dependencies
- Modules call services directly through dependency injection
- Services use repositories for data access
- Services use other services for cross-cutting concerns

### Event-Driven Communication
- Sync events trigger UI updates
- Payment events trigger notifications
- Audit events are logged automatically

### Dependency Injection
- All services registered in DI container
- Repositories registered as scoped
- Services registered as singletons (where appropriate)

---

## Module Testing Strategy

### Unit Tests
- Each service has corresponding unit tests
- Mock repositories and dependencies
- Test business logic in isolation

### Integration Tests
- Test service interactions
- Test database operations
- Test sync functionality

### UI Tests
- Test ViewModels
- Test navigation
- Test user interactions (optional)

