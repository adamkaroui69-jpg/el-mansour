# El Mansour Syndic Manager - Project Structure

```
ElMansourSyndicManager/
│
├── ElMansourSyndicManager.sln
│
├── src/
│   ├── ElMansourSyndicManager/
│   │   ├── App.xaml
│   │   ├── App.xaml.cs
│   │   ├── App.config
│   │   └── ElMansourSyndicManager.csproj
│   │
│   ├── ElMansourSyndicManager.Core/
│   │   ├── Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── User.cs
│   │   │   │   ├── Payment.cs
│   │   │   │   ├── Maintenance.cs
│   │   │   │   ├── MaintenanceDocument.cs
│   │   │   │   ├── House.cs
│   │   │   │   ├── AuditLog.cs
│   │   │   │   └── SyncQueueItem.cs
│   │   │   │
│   │   │   ├── Enums/
│   │   │   │   ├── UserRole.cs
│   │   │   │   ├── MaintenanceType.cs
│   │   │   │   ├── MaintenanceStatus.cs
│   │   │   │   ├── PaymentStatus.cs
│   │   │   │   ├── DocumentType.cs
│   │   │   │   ├── SyncStatus.cs
│   │   │   │   └── ActionType.cs
│   │   │   │
│   │   │   ├── ValueObjects/
│   │   │   │   ├── Money.cs
│   │   │   │   ├── HouseCode.cs
│   │   │   │   └── Signature.cs
│   │   │   │
│   │   │   └── Exceptions/
│   │   │       ├── DomainException.cs
│   │   │       ├── ValidationException.cs
│   │   │       └── BusinessRuleException.cs
│   │   │
│   │   ├── Interfaces/
│   │   │   ├── Services/
│   │   │   │   ├── IAuthenticationService.cs
│   │   │   │   ├── IPaymentService.cs
│   │   │   │   ├── IMaintenanceService.cs
│   │   │   │   ├── IReportService.cs
│   │   │   │   ├── IUserService.cs
│   │   │   │   ├── IDocumentService.cs
│   │   │   │   ├── IPdfService.cs
│   │   │   │   ├── ISyncService.cs
│   │   │   │   ├── IAuditService.cs
│   │   │   │   └── IStorageService.cs
│   │   │   │
│   │   │   ├── Repositories/
│   │   │   │   ├── IUserRepository.cs
│   │   │   │   ├── IPaymentRepository.cs
│   │   │   │   ├── IMaintenanceRepository.cs
│   │   │   │   ├── IHouseRepository.cs
│   │   │   │   ├── IAuditLogRepository.cs
│   │   │   │   └── ISyncQueueRepository.cs
│   │   │   │
│   │   │   └── Infrastructure/
│   │   │       ├── IDatabaseContext.cs
│   │   │       ├── ICloudClient.cs
│   │   │       └── IFileStorage.cs
│   │   │
│   │   └── DTOs/
│   │       ├── PaymentDto.cs
│   │       ├── MaintenanceDto.cs
│   │       ├── UserDto.cs
│   │       ├── ReportDto.cs
│   │       ├── ReceiptData.cs
│   │       ├── SyncStatusDto.cs
│   │       └── ConflictDto.cs
│   │
│   ├── ElMansourSyndicManager.Infrastructure/
│   │   ├── Data/
│   │   │   ├── Local/
│   │   │   │   ├── SqliteDbContext.cs
│   │   │   │   ├── Repositories/
│   │   │   │   │   ├── UserRepository.cs
│   │   │   │   │   ├── PaymentRepository.cs
│   │   │   │   │   ├── MaintenanceRepository.cs
│   │   │   │   │   ├── HouseRepository.cs
│   │   │   │   │   ├── AuditLogRepository.cs
│   │   │   │   │   └── SyncQueueRepository.cs
│   │   │   │   │
│   │   │   │   └── Migrations/
│   │   │   │       └── (EF Core migrations or manual SQL)
│   │   │   │
│   │   │   └── Cloud/
│   │   │       ├── SupabaseClient.cs
│   │   │       ├── CloudRepository.cs
│   │   │       └── RealtimeSubscriptions.cs
│   │   │
│   │   ├── Services/
│   │   │   ├── AuthenticationService.cs
│   │   │   ├── PaymentService.cs
│   │   │   ├── MaintenanceService.cs
│   │   │   ├── ReportService.cs
│   │   │   ├── UserService.cs
│   │   │   ├── DocumentService.cs
│   │   │   ├── PdfService.cs
│   │   │   ├── SyncService.cs
│   │   │   ├── AuditService.cs
│   │   │   └── StorageService.cs
│   │   │
│   │   ├── Security/
│   │   │   ├── PasswordHasher.cs
│   │   │   ├── TokenManager.cs
│   │   │   └── EncryptionHelper.cs
│   │   │
│   │   ├── Sync/
│   │   │   ├── SyncEngine.cs
│   │   │   ├── ConflictResolver.cs
│   │   │   └── OfflineQueueManager.cs
│   │   │
│   │   ├── Storage/
│   │   │   ├── LocalFileStorage.cs
│   │   │   └── CloudFileStorage.cs
│   │   │
│   │   └── Logging/
│   │       ├── LoggerConfiguration.cs
│   │       └── AuditLogger.cs
│   │
│   ├── ElMansourSyndicManager.ViewModels/
│   │   ├── Base/
│   │   │   ├── ViewModelBase.cs
│   │   │   └── RelayCommand.cs
│   │   │
│   │   ├── DashboardViewModel.cs
│   │   ├── LoginViewModel.cs
│   │   ├── PaymentViewModel.cs
│   │   ├── PaymentListViewModel.cs
│   │   ├── MaintenanceViewModel.cs
│   │   ├── MaintenanceListViewModel.cs
│   │   ├── ReportViewModel.cs
│   │   ├── UserManagementViewModel.cs
│   │   ├── SettingsViewModel.cs
│   │   └── SyncStatusViewModel.cs
│   │
│   ├── ElMansourSyndicManager.Views/
│   │   ├── Windows/
│   │   │   ├── MainWindow.xaml
│   │   │   ├── MainWindow.xaml.cs
│   │   │   └── LoginWindow.xaml
│   │   │   └── LoginWindow.xaml.cs
│   │   │
│   │   ├── Pages/
│   │   │   ├── DashboardPage.xaml
│   │   │   ├── DashboardPage.xaml.cs
│   │   │   ├── PaymentPage.xaml
│   │   │   ├── PaymentPage.xaml.cs
│   │   │   ├── PaymentListPage.xaml
│   │   │   ├── PaymentListPage.xaml.cs
│   │   │   ├── MaintenancePage.xaml
│   │   │   ├── MaintenancePage.xaml.cs
│   │   │   ├── MaintenanceListPage.xaml
│   │   │   ├── MaintenanceListPage.xaml.cs
│   │   │   ├── ReportPage.xaml
│   │   │   ├── ReportPage.xaml.cs
│   │   │   ├── UserManagementPage.xaml
│   │   │   ├── UserManagementPage.xaml.cs
│   │   │   ├── SettingsPage.xaml
│   │   │   └── SettingsPage.xaml.cs
│   │   │
│   │   ├── UserControls/
│   │   │   ├── PaymentCard.xaml
│   │   │   ├── MaintenanceCard.xaml
│   │   │   ├── HouseSelector.xaml
│   │   │   ├── SignatureViewer.xaml
│   │   │   ├── DocumentViewer.xaml
│   │   │   └── StatisticsChart.xaml
│   │   │
│   │   └── Dialogs/
│   │       ├── ConfirmDialog.xaml
│   │       ├── ReceiptPreviewDialog.xaml
│   │       └── ConflictResolutionDialog.xaml
│   │
│   └── ElMansourSyndicManager.Utilities/
│       ├── Helpers/
│       │   ├── DateTimeHelper.cs
│       │   ├── ValidationHelper.cs
│       │   ├── FileHelper.cs
│       │   └── StringHelper.cs
│       │
│       ├── Converters/
│       │   ├── BooleanToVisibilityConverter.cs
│       │   ├── DateTimeToStringConverter.cs
│       │   ├── CurrencyConverter.cs
│       │   └── RoleToVisibilityConverter.cs
│       │
│       ├── Extensions/
│       │   ├── DateTimeExtensions.cs
│       │   ├── StringExtensions.cs
│       │   └── CollectionExtensions.cs
│       │
│       └── Constants/
│           ├── AppConstants.cs
│           ├── DatabaseConstants.cs
│           └── UiConstants.cs
│
├── tests/
│   ├── ElMansourSyndicManager.Tests.Unit/
│   │   ├── Services/
│   │   ├── Repositories/
│   │   └── ViewModels/
│   │
│   └── ElMansourSyndicManager.Tests.Integration/
│       ├── Database/
│       └── Sync/
│
├── docs/
│   ├── ARCHITECTURE.md
│   ├── PROJECT_STRUCTURE.md
│   ├── MODULES.md
│   ├── API.md
│   └── DEPLOYMENT.md
│
├── scripts/
│   ├── setup-database.sql
│   ├── seed-data.sql
│   └── backup.ps1
│
├── resources/
│   ├── Images/
│   │   ├── logo.png
│   │   └── icons/
│   │
│   ├── Templates/
│   │   ├── ReceiptTemplate.html
│   │   └── ReportTemplate.html
│   │
│   └── Localization/
│       └── fr-FR.resx
│
├── data/
│   ├── database/
│   │   └── (SQLite database files)
│   │
│   ├── documents/
│   │   ├── receipts/
│   │   ├── maintenance/
│   │   └── reports/
│   │
│   └── backups/
│       └── (backup files)
│
├── .gitignore
├── README.md
└── LICENSE

```

## Project Structure Explanation

### Core Projects

1. **ElMansourSyndicManager** (Main WPF Application)
   - Entry point
   - Application configuration
   - Resource dictionaries
   - Material Design theme setup

2. **ElMansourSyndicManager.Core**
   - Domain entities and business logic
   - Interfaces (services, repositories)
   - DTOs for data transfer
   - Value objects and enums

3. **ElMansourSyndicManager.Infrastructure**
   - Data access implementations
   - Service implementations
   - External integrations (Supabase, file storage)
   - Security and encryption
   - Sync engine

4. **ElMansourSyndicManager.ViewModels**
   - MVVM ViewModels
   - Command implementations
   - View state management

5. **ElMansourSyndicManager.Views**
   - XAML views
   - Code-behind (minimal)
   - User controls
   - Dialogs

6. **ElMansourSyndicManager.Utilities**
   - Helper classes
   - Value converters
   - Extensions
   - Constants

### Key Directories

- **Domain/Entities**: Core business entities
- **Domain/Enums**: Type-safe enumerations
- **Domain/ValueObjects**: Immutable value objects
- **Interfaces**: Contracts for dependency injection
- **Data/Local**: SQLite database and repositories
- **Data/Cloud**: Supabase integration
- **Services**: Business logic implementations
- **Security**: Authentication and encryption
- **Sync**: Synchronization engine
- **Storage**: File storage abstractions

### Data Organization

- **data/database/**: SQLite database files
- **data/documents/**: Local document storage
- **data/backups/**: Backup files

### Resources

- **Images**: Application images and icons
- **Templates**: PDF/HTML templates
- **Localization**: French language resources

