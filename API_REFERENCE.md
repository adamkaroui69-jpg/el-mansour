# El Mansour Syndic Manager - API Reference

## Service Interfaces

### IAuthenticationService

```csharp
public interface IAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(string houseCode, string code);
    Task LogoutAsync();
    Task<UserDto> GetCurrentUserAsync();
    Task<bool> ChangePasswordAsync(string newCode);
    Task<bool> ValidateSessionAsync();
    bool IsAuthenticated { get; }
    UserDto CurrentUser { get; }
}
```

**Methods**:
- `AuthenticateAsync`: Authenticate user with house code and 6-digit code
- `LogoutAsync`: Log out current user
- `GetCurrentUserAsync`: Get currently authenticated user
- `ChangePasswordAsync`: Change user's authentication code
- `ValidateSessionAsync`: Validate current session token

---

### IPaymentService

```csharp
public interface IPaymentService
{
    Task<PaymentDto> RecordPaymentAsync(PaymentDto payment);
    Task<PaymentDto> GetPaymentAsync(Guid id);
    Task<List<PaymentDto>> GetPaymentsByHouseAsync(string houseCode, DateTime? from = null, DateTime? to = null);
    Task<List<PaymentDto>> GetPaymentsByMonthAsync(string month); // Format: "YYYY-MM"
    Task<List<UnpaidHouseDto>> GetUnpaidHousesAsync(DateTime month);
    Task<byte[]> GenerateReceiptAsync(Guid paymentId);
    Task<PaymentStatisticsDto> GetPaymentStatisticsAsync(DateTime from, DateTime to);
    Task<bool> DeletePaymentAsync(Guid id); // Soft delete, Admin only
}
```

**Methods**:
- `RecordPaymentAsync`: Record a new payment
- `GetPaymentAsync`: Get payment by ID
- `GetPaymentsByHouseAsync`: Get all payments for a house
- `GetPaymentsByMonthAsync`: Get all payments for a month
- `GetUnpaidHousesAsync`: Get list of unpaid houses for a month
- `GenerateReceiptAsync`: Generate PDF receipt
- `GetPaymentStatisticsAsync`: Get payment statistics
- `DeletePaymentAsync`: Soft delete a payment (Admin only)

---

### IMaintenanceService

```csharp
public interface IMaintenanceService
{
    Task<MaintenanceDto> CreateMaintenanceAsync(MaintenanceDto maintenance);
    Task<MaintenanceDto> UpdateMaintenanceAsync(Guid id, MaintenanceDto maintenance);
    Task<MaintenanceDto> GetMaintenanceAsync(Guid id);
    Task<List<MaintenanceDto>> GetAllMaintenanceAsync(DateTime? from = null, DateTime? to = null);
    Task<List<MaintenanceDto>> GetMaintenanceByTypeAsync(MaintenanceType type);
    Task<List<MaintenanceDto>> GetMaintenanceByStatusAsync(MaintenanceStatus status);
    Task AttachDocumentAsync(Guid maintenanceId, string filePath, DocumentType documentType);
    Task RemoveDocumentAsync(Guid documentId);
    Task<bool> DeleteMaintenanceAsync(Guid id);
}
```

**Methods**:
- `CreateMaintenanceAsync`: Create new maintenance request
- `UpdateMaintenanceAsync`: Update existing maintenance
- `GetMaintenanceAsync`: Get maintenance by ID
- `GetAllMaintenanceAsync`: Get all maintenance with optional date filter
- `GetMaintenanceByTypeAsync`: Filter by type
- `GetMaintenanceByStatusAsync`: Filter by status
- `AttachDocumentAsync`: Attach document to maintenance
- `RemoveDocumentAsync`: Remove document
- `DeleteMaintenanceAsync`: Delete maintenance

---

### IReportService

```csharp
public interface IReportService
{
    Task<MonthlyReportDto> GenerateMonthlyReportAsync(DateTime month);
    Task<YearlyReportDto> GenerateYearlyReportAsync(int year);
    Task<FinancialSummaryDto> GetFinancialSummaryAsync(DateTime from, DateTime to);
    Task<List<UnpaidHouseDto>> GetUnpaidHousesReportAsync(DateTime month);
    Task<StatisticsDto> GetStatisticsAsync(DateTime from, DateTime to);
    Task<byte[]> ExportReportAsync(ReportDto report, ExportFormat format);
}
```

**Methods**:
- `GenerateMonthlyReportAsync`: Generate monthly financial report
- `GenerateYearlyReportAsync`: Generate yearly financial report
- `GetFinancialSummaryAsync`: Get financial summary for period
- `GetUnpaidHousesReportAsync`: Get unpaid houses for report
- `GetStatisticsAsync`: Get statistics and analytics
- `ExportReportAsync`: Export report to PDF or Excel

---

### IUserService

```csharp
public interface IUserService
{
    Task<UserDto> CreateUserAsync(UserDto user);
    Task<UserDto> UpdateUserAsync(Guid id, UserDto user);
    Task<bool> DeleteUserAsync(Guid id); // Admin only
    Task<UserDto> GetUserAsync(Guid id);
    Task<UserDto> GetUserByHouseCodeAsync(string houseCode);
    Task<List<UserDto>> GetAllUsersAsync();
    Task<bool> UpdateSignatureAsync(Guid userId, string signaturePath);
    Task<bool> ResetPasswordAsync(Guid userId, string newCode);
}
```

**Methods**:
- `CreateUserAsync`: Create new user (Admin only)
- `UpdateUserAsync`: Update user information
- `DeleteUserAsync`: Delete user (Admin only, soft delete)
- `GetUserAsync`: Get user by ID
- `GetUserByHouseCodeAsync`: Get user by house code
- `GetAllUsersAsync`: Get all users
- `UpdateSignatureAsync`: Update user signature
- `ResetPasswordAsync`: Reset user password (Admin only)

---

### IDocumentService

```csharp
public interface IDocumentService
{
    Task<DocumentDto> UploadDocumentAsync(string filePath, DocumentType type, Guid? relatedEntityId = null);
    Task<Stream> DownloadDocumentAsync(Guid documentId);
    Task<bool> DeleteDocumentAsync(Guid documentId);
    Task<List<DocumentDto>> GetDocumentsByTypeAsync(DocumentType type);
    Task<List<DocumentDto>> GetDocumentsByEntityAsync(string entityType, Guid entityId);
    Task<string> GetDocumentUrlAsync(Guid documentId);
}
```

**Methods**:
- `UploadDocumentAsync`: Upload document to storage
- `DownloadDocumentAsync`: Download document stream
- `DeleteDocumentAsync`: Delete document
- `GetDocumentsByTypeAsync`: Get documents by type
- `GetDocumentsByEntityAsync`: Get documents related to entity
- `GetDocumentUrlAsync`: Get document URL for viewing

---

### IPdfService

```csharp
public interface IPdfService
{
    Task<byte[]> GenerateReceiptAsync(ReceiptData data);
    Task<byte[]> GenerateReportAsync(ReportData data);
    Task<byte[]> MergePdfAsync(List<byte[]> pdfs);
    Task SavePdfAsync(byte[] pdfData, string filePath);
}
```

**Methods**:
- `GenerateReceiptAsync`: Generate payment receipt PDF
- `GenerateReportAsync`: Generate financial report PDF
- `MergePdfAsync`: Merge multiple PDFs
- `SavePdfAsync`: Save PDF to file system

---

### ISyncService

```csharp
public interface ISyncService
{
    Task<SyncResult> SyncAsync();
    Task<SyncStatusDto> GetSyncStatusAsync();
    Task<List<ConflictDto>> GetConflictsAsync();
    Task<bool> ResolveConflictAsync(ConflictDto conflict, ConflictResolution resolution);
    Task<List<SyncQueueItemDto>> GetPendingChangesAsync();
    Task<bool> ForceSyncAsync();
    event EventHandler<SyncStatusChangedEventArgs> SyncStatusChanged;
}
```

**Methods**:
- `SyncAsync`: Perform synchronization
- `GetSyncStatusAsync`: Get current sync status
- `GetConflictsAsync`: Get list of conflicts
- `ResolveConflictAsync`: Resolve a conflict manually
- `GetPendingChangesAsync`: Get pending changes in queue
- `ForceSyncAsync`: Force immediate sync
- `SyncStatusChanged`: Event fired when sync status changes

---

### IAuditService

```csharp
public interface IAuditService
{
    Task LogActivityAsync(ActivityLogDto log);
    Task<List<AuditLogDto>> GetAuditLogAsync(DateTime? from = null, DateTime? to = null, Guid? userId = null);
    Task<List<AuditLogDto>> GetAuditLogByEntityAsync(string entityType, Guid entityId);
    Task<byte[]> ExportAuditLogAsync(DateTime from, DateTime to);
}
```

**Methods**:
- `LogActivityAsync`: Log user activity
- `GetAuditLogAsync`: Get audit logs with filters
- `GetAuditLogByEntityAsync`: Get audit logs for specific entity
- `ExportAuditLogAsync`: Export audit log to file

---

### IStorageService

```csharp
public interface IStorageService
{
    Task<string> UploadFileAsync(string localPath, string remotePath);
    Task<Stream> DownloadFileAsync(string remotePath);
    Task<bool> DeleteFileAsync(string remotePath);
    Task<string> GetFileUrlAsync(string remotePath, int expirationMinutes = 60);
    Task<bool> FileExistsAsync(string remotePath);
}
```

**Methods**:
- `UploadFileAsync`: Upload file to cloud storage
- `DownloadFileAsync`: Download file from cloud
- `DeleteFileAsync`: Delete file from cloud
- `GetFileUrlAsync`: Get signed URL for file access
- `FileExistsAsync`: Check if file exists

---

## Data Transfer Objects (DTOs)

### PaymentDto

```csharp
public class PaymentDto
{
    public Guid Id { get; set; }
    public string HouseCode { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Month { get; set; } // "YYYY-MM"
    public int Year { get; set; }
    public string ReceiptPath { get; set; }
    public Guid RecordedBy { get; set; }
    public PaymentStatus Status { get; set; }
    public string Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### MaintenanceDto

```csharp
public class MaintenanceDto
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public MaintenanceType Type { get; set; }
    public decimal Cost { get; set; }
    public MaintenanceStatus Status { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Notes { get; set; }
    public List<DocumentDto> Documents { get; set; }
}
```

### UserDto

```csharp
public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string HouseCode { get; set; }
    public UserRole Role { get; set; }
    public string SignaturePath { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### MonthlyReportDto

```csharp
public class MonthlyReportDto
{
    public DateTime Month { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal Balance { get; set; }
    public int PaidHousesCount { get; set; }
    public int UnpaidHousesCount { get; set; }
    public List<PaymentDto> Payments { get; set; }
    public List<MaintenanceDto> Maintenance { get; set; }
    public List<UnpaidHouseDto> UnpaidHouses { get; set; }
}
```

### SyncResult

```csharp
public class SyncResult
{
    public SyncStatus Status { get; set; }
    public int PushedCount { get; set; }
    public int PulledCount { get; set; }
    public int ConflictsCount { get; set; }
    public string Error { get; set; }
    public DateTime CompletedAt { get; set; }
}
```

---

## Enums

### UserRole

```csharp
public enum UserRole
{
    Admin,
    SyndicMember
}
```

### MaintenanceType

```csharp
public enum MaintenanceType
{
    Repair,
    Cleaning,
    Security,
    Other
}
```

### MaintenanceStatus

```csharp
public enum MaintenanceStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled
}
```

### PaymentStatus

```csharp
public enum PaymentStatus
{
    Paid,
    Unpaid,
    Overdue,
    Cancelled
}
```

### DocumentType

```csharp
public enum DocumentType
{
    Justificative,
    Invoice,
    Receipt,
    Other
}
```

### SyncStatus

```csharp
public enum SyncStatus
{
    Pending,
    Syncing,
    Synced,
    Conflict,
    Failed,
    Offline
}
```

---

## Error Handling

### Custom Exceptions

```csharp
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class SyncException : Exception
{
    public SyncException(string message) : base(message) { }
}
```

---

## Usage Examples

### Record Payment

```csharp
var paymentService = serviceProvider.GetService<IPaymentService>();

var payment = new PaymentDto
{
    HouseCode = "A01",
    Amount = 1500.00m,
    PaymentDate = DateTime.Now,
    Month = "2024-01",
    Year = 2024,
    RecordedBy = currentUser.Id
};

var result = await paymentService.RecordPaymentAsync(payment);
var receipt = await paymentService.GenerateReceiptAsync(result.Id);
```

### Create Maintenance

```csharp
var maintenanceService = serviceProvider.GetService<IMaintenanceService>();

var maintenance = new MaintenanceDto
{
    Description = "Réparation de l'ascenseur",
    Type = MaintenanceType.Repair,
    Cost = 5000.00m,
    Status = MaintenanceStatus.Pending,
    CreatedBy = currentUser.Id
};

var result = await maintenanceService.CreateMaintenanceAsync(maintenance);
await maintenanceService.AttachDocumentAsync(result.Id, "invoice.pdf", DocumentType.Invoice);
```

### Generate Report

```csharp
var reportService = serviceProvider.GetService<IReportService>();

var monthlyReport = await reportService.GenerateMonthlyReportAsync(new DateTime(2024, 1, 1));
var pdfBytes = await reportService.ExportReportAsync(monthlyReport, ExportFormat.Pdf);
```

---

## Summary

This API reference provides:
- ✅ Complete service interfaces
- ✅ Data transfer objects
- ✅ Enumerations
- ✅ Error handling
- ✅ Usage examples

All services follow async/await pattern and return DTOs for data transfer.

