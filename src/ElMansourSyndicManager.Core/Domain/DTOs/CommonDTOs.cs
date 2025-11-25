namespace ElMansourSyndicManager.Core.Domain.DTOs;

#region Authentication DTOs

public class AuthenticationResultDto
{
    public bool Success { get; set; }
    public UserDto? User { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Token { get; set; }
}

#endregion

#region Payment DTOs

public class CreatePaymentDto
{
    public string HouseCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string Month { get; set; } = string.Empty; // YYYY-MM
    public string? PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

public class PaymentDto
{
    public Guid Id { get; set; }
    public string HouseCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string Month { get; set; } = string.Empty;
    public string Status { get; set; } = "Paid";
    public string? ReferenceNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public CancellationToken CancellationToken { get; set; }
}

public class UnpaidHouseDto
{
    public string HouseCode { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;
    public int Floor { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string? OwnerPhone { get; set; }
    public decimal MonthlyAmount { get; set; }
    public string Month { get; set; } = string.Empty;
    public int DaysOverdue { get; set; }
}

public class PaymentStatisticsDto
{
    public decimal TotalCollected { get; set; }
    public decimal TotalExpected { get; set; }
    public decimal CollectionRate { get; set; }
    public int PaidCount { get; set; }
    public int UnpaidCount { get; set; }
    public int OverdueCount { get; set; }
    public Dictionary<string, decimal> MonthlyBreakdown { get; set; } = new();
}

#endregion

#region Receipt DTOs

public class ReceiptDto
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string? CloudStoragePath { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string MimeType { get; set; } = "application/pdf";
    public string GeneratedBy { get; set; } = string.Empty;
    public DateTime? ReceiptDate { get; set; } // Ajouté
    public DateTime GeneratedDate { get; set; } // Ajouté pour correspondre à l'entité
    public decimal AmountPaid { get; set; } // Ajouté
    public string PaymentMethod { get; set; } = string.Empty; // Ajouté
    public DateTime GeneratedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

#endregion

#region User DTOs

public class UserDto
{
    public Guid Id { get; set; }
    public Guid? HouseId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string HouseCode { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // 6-digit code
    public string Role { get; set; } = "SyndicMember";
    public string? SignaturePath { get; set; }
}

public class UpdateUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string? SignaturePath { get; set; }
    public bool IsActive { get; set; }
}

#endregion

#region Maintenance DTOs

public class CreateMaintenanceDTO
{
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string Priority { get; set; } = "Normal";
    public string? AssignedTo { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateMaintenanceDTO
{
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string Status { get; set; } = "Pending";
    public string Priority { get; set; } = "Normal";
    public string? AssignedTo { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ReportedBy { get; set; }
    public string? Notes { get; set; }
}

public class MaintenanceDTO
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string Status { get; set; } = "Pending";
    public string Priority { get; set; } = "Normal";
    public string CreatedBy { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ReportedBy { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class MaintenanceDocumentDTO
{
    public Guid Id { get; set; }
    public Guid MaintenanceId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? CloudStoragePath { get; set; }
    public long FileSize { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

#endregion

#region Expense DTOs

public class CreateExpenseDto
{
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? MaintenanceId { get; set; }
    public string? Notes { get; set; }
}

public class UpdateExpenseDto
{
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? MaintenanceId { get; set; }
    public string? Notes { get; set; }
}

public class ExpenseDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string Month { get; set; } = string.Empty;
    public int Year { get; set; }
    public string RecordedBy { get; set; } = string.Empty;
    public string? MaintenanceId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

#endregion

#region Report DTOs

public class MonthlyReportDto
{
    public Guid Id { get; set; }
    public DateTime Month { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal Balance { get; set; }
    public int PaidHousesCount { get; set; }
    public int UnpaidHousesCount { get; set; }
    public int TotalHousesCount { get; set; }
    public decimal CollectionRate { get; set; }
    public decimal AveragePaymentDelay { get; set; }
    public List<PaymentDto> Payments { get; set; } = new();
    public List<ExpenseDto> Expenses { get; set; } = new();
    public List<UnpaidHouseDto> UnpaidHouses { get; set; } = new();
    public Dictionary<string, decimal> BuildingBreakdown { get; set; } = new();
    public Dictionary<string, decimal> ExpenseCategoryBreakdown { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
}

public class YearlyReportDto
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal Balance { get; set; }
    public Dictionary<string, MonthlyReportDto> MonthlyBreakdown { get; set; } = new();
    public PaymentStatisticsDto Statistics { get; set; } = new();
    public Dictionary<string, decimal> BuildingYearlyBreakdown { get; set; } = new();
    public Dictionary<string, decimal> ExpenseCategoryYearlyBreakdown { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
}

public class ReportHistoryDTO
{
    public Guid Id { get; set; }
    public string ReportType { get; set; } = string.Empty; // "Monthly" or "Yearly"
    public string Period { get; set; } = string.Empty; // "2024-01" or "2024"
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? CloudStoragePath { get; set; }
    public long FileSize { get; set; }
    public string Format { get; set; } = string.Empty; // "PDF" or "Excel"
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
}

#endregion

#region Notification DTOs

public class NotificationDTO
{
    public Guid Id { get; set; }
    public string? UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? RelatedEntityType { get; set; }
    public string? RelatedEntityId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string Priority { get; set; } = "Normal";
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

#endregion

#region Backup DTOs

public class BackupHistoryDTO
{
    public Guid Id { get; set; }
    public string BackupType { get; set; } = string.Empty; // Full, Database, Documents
    public string FilePath { get; set; } = string.Empty;
    public string? CloudStoragePath { get; set; }
    public long FileSize { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsAutomatic { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "Success"; // Success, Failed, InProgress
    public string? ErrorMessage { get; set; }
}

#endregion

#region Sync DTOs

public class SyncResultDto
{
    public bool Success { get; set; }
    public int PushedCount { get; set; }
    public int PulledCount { get; set; }
    public int ConflictsCount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CompletedAt { get; set; }
}

public class ConflictDto
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public object LocalEntity { get; set; } = new();
    public object CloudEntity { get; set; } = new();
    public DateTime LocalUpdatedAt { get; set; }
    public DateTime CloudUpdatedAt { get; set; }
}

#endregion

#region Document DTOs

public class DocumentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

#endregion
