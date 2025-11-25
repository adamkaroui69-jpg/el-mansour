// El Mansour Syndic Manager - EF Core Models
// File: src/ElMansourSyndicManager.Infrastructure/Data/Local/Entities/

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElMansourSyndicManager.Infrastructure.Data.Local.Entities;

#region Base Entity

public abstract class BaseEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
    
    // Sync metadata
    public DateTime? LastSyncAt { get; set; }
    public string? CloudId { get; set; }
    public string SyncStatus { get; set; } = "Pending"; // Pending, Synced, Conflict
}

#endregion

#region Residence

public class Residence : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Address { get; set; }
    
    [MaxLength(100)]
    public string? City { get; set; }
    
    [MaxLength(20)]
    public string? PostalCode { get; set; }
    
    [MaxLength(50)]
    public string Country { get; set; } = "Morocco";
    
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    [MaxLength(100)]
    public string? Email { get; set; }
    
    // Navigation
    public virtual ICollection<Building> Buildings { get; set; } = new List<Building>();
}

#endregion

#region Building

public class Building : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string ResidenceId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1)]
    public string Code { get; set; } = string.Empty; // A, B, C, D, E
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public int Floors { get; set; }
    
    public string? Description { get; set; }
    
    // Navigation
    [ForeignKey(nameof(ResidenceId))]
    public virtual Residence? Residence { get; set; }
    
    public virtual ICollection<House> Houses { get; set; } = new List<House>();
}

#endregion

#region House

public class House : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string BuildingId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty; // A01, B02, M01, B-SYNDIC, B-CONCIERGE
    
    [Required]
    public int Floor { get; set; }
    
    [Required]
    public int UnitNumber { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Type { get; set; } = string.Empty; // House, Shop, Office, Concierge
    
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal MonthlyAmount { get; set; } = 0.00m;
    
    [MaxLength(100)]
    public string? OwnerName { get; set; }
    
    [MaxLength(20)]
    public string? OwnerPhone { get; set; }
    
    [MaxLength(100)]
    public string? OwnerEmail { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    public string? Notes { get; set; }
    
    // Navigation
    [ForeignKey(nameof(BuildingId))]
    public virtual Building? Building { get; set; }
    
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

#endregion

#region User

public class User : BaseEntity
{
    [MaxLength(50)]
    public string? HouseId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Surname { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string HouseCode { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(64)]
    public string Salt { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = string.Empty; // Admin, SyndicMember
    
    [MaxLength(500)]
    public string? SignaturePath { get; set; }
    
    [MaxLength(500)]
    public string? SignatureCloudPath { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    public DateTime? LastLoginAt { get; set; }
    
    // Navigation
    [ForeignKey(nameof(HouseId))]
    public virtual House? House { get; set; }
    
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<Maintenance> CreatedMaintenance { get; set; } = new List<Maintenance>();
    public virtual ICollection<Maintenance> AssignedMaintenance { get; set; } = new List<Maintenance>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}

#endregion

#region Payment

public class Payment : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string HouseId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string HouseCode { get; set; } = string.Empty;
    
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }
    
    [Required]
    public DateTime PaymentDate { get; set; }
    
    [Required]
    [MaxLength(7)]
    public string Month { get; set; } = string.Empty; // YYYY-MM
    
    [Required]
    public int Year { get; set; }
    
    [MaxLength(50)]
    public string? ReceiptId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string RecordedBy { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Paid"; // Paid, Unpaid, Overdue, Cancelled
    
    [MaxLength(50)]
    public string? PaymentMethod { get; set; }
    
    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }
    
    public string? Notes { get; set; }
    
    // Navigation
    [ForeignKey(nameof(HouseId))]
    public virtual House? House { get; set; }
    
    [ForeignKey(nameof(ReceiptId))]
    public virtual Receipt? Receipt { get; set; }
    
    [ForeignKey(nameof(RecordedBy))]
    public virtual User? RecordedByUser { get; set; }
}

#endregion

#region Receipt

public class Receipt : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string PaymentId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? CloudStoragePath { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    public long FileSize { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string MimeType { get; set; } = "application/pdf";
    
    [Required]
    [MaxLength(50)]
    public string GeneratedBy { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? SignatureUserId { get; set; }
    
    [Required]
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    [ForeignKey(nameof(PaymentId))]
    public virtual Payment? Payment { get; set; }
    
    [ForeignKey(nameof(GeneratedBy))]
    public virtual User? GeneratedByUser { get; set; }
    
    [ForeignKey(nameof(SignatureUserId))]
    public virtual User? SignatureUser { get; set; }
}

#endregion

#region Expense

public class Expense : BaseEntity
{
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty; // Maintenance, Utilities, Insurance, Other
    
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }
    
    [Required]
    public DateTime ExpenseDate { get; set; }
    
    [Required]
    [MaxLength(7)]
    public string Month { get; set; } = string.Empty; // YYYY-MM
    
    [Required]
    public int Year { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string RecordedBy { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? MaintenanceId { get; set; }
    
    public string? Notes { get; set; }
    
    // Navigation
    [ForeignKey(nameof(RecordedBy))]
    public virtual User? RecordedByUser { get; set; }
    
    [ForeignKey(nameof(MaintenanceId))]
    public virtual Maintenance? Maintenance { get; set; }
}

#endregion

#region Maintenance

public class Maintenance : BaseEntity
{
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty; // Repair, Cleaning, Security, Other
    
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Cost { get; set; } = 0.00m;
    
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Cancelled
    
    [MaxLength(20)]
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent
    
    [Required]
    [MaxLength(50)]
    public string CreatedBy { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? AssignedTo { get; set; }
    
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    public string? Notes { get; set; }
    
    // Navigation
    [ForeignKey(nameof(CreatedBy))]
    public virtual User? CreatedByUser { get; set; }
    
    [ForeignKey(nameof(AssignedTo))]
    public virtual User? AssignedToUser { get; set; }
    
    public virtual ICollection<MaintenanceDocument> Documents { get; set; } = new List<MaintenanceDocument>();
    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}

#endregion

#region MaintenanceDocument

public class MaintenanceDocument : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string MaintenanceId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? CloudStoragePath { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    public long FileSize { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string MimeType { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string DocumentType { get; set; } = string.Empty; // Justificative, Invoice, Photo, Other
    
    [Required]
    [MaxLength(50)]
    public string UploadedBy { get; set; } = string.Empty;
    
    [Required]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    [ForeignKey(nameof(MaintenanceId))]
    public virtual Maintenance? Maintenance { get; set; }
    
    [ForeignKey(nameof(UploadedBy))]
    public virtual User? UploadedByUser { get; set; }
}

#endregion

#region Notification

public class Notification : BaseEntity
{
    [MaxLength(50)]
    public string? UserId { get; set; } // NULL = all users
    
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty; // UnpaidHouse, MaintenanceDue, System, Info
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? RelatedEntityType { get; set; }
    
    [MaxLength(50)]
    public string? RelatedEntityId { get; set; }
    
    [Required]
    public bool IsRead { get; set; } = false;
    
    public DateTime? ReadAt { get; set; }
    
    [MaxLength(20)]
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent
    
    public DateTime? ExpiresAt { get; set; }
    
    // Navigation
    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
}

#endregion

#region AuditLog

public class AuditLog : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty; // Create, Update, Delete, View, Login, Logout, etc.
    
    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? EntityId { get; set; }
    
    public string? Details { get; set; } // JSON string
    
    [MaxLength(50)]
    public string? IpAddress { get; set; }
    
    [MaxLength(500)]
    public string? UserAgent { get; set; }
    
    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation
    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
}

#endregion

#region Setting

public class Setting : BaseEntity
{
    [Key]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;
    
    [Required]
    public string Value { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string Type { get; set; } = string.Empty; // String, Int, Bool, DateTime, Json
    
    public string? Description { get; set; }
    
    [MaxLength(50)]
    public string? Category { get; set; }
    
    [MaxLength(50)]
    public string? UpdatedBy { get; set; }
    
    // Navigation
    [ForeignKey(nameof(UpdatedBy))]
    public virtual User? UpdatedByUser { get; set; }
}

#endregion

#region Backup

public class Backup : BaseEntity
{
    [Required]
    [MaxLength(20)]
    public string BackupType { get; set; } = string.Empty; // Full, Database, Documents
    
    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? CloudStoragePath { get; set; }
    
    [Required]
    public long FileSize { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string CreatedBy { get; set; } = string.Empty;
    
    public DateTime? ExpiresAt { get; set; }
    
    [Required]
    public bool IsAutomatic { get; set; } = false;
    
    public string? Notes { get; set; }
    
    // Navigation
    [ForeignKey(nameof(CreatedBy))]
    public virtual User? CreatedByUser { get; set; }
}

#endregion

#region SyncQueue

public class SyncQueue : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string EntityId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string Operation { get; set; } = string.Empty; // Create, Update, Delete
    
    [Required]
    public string Data { get; set; } = string.Empty; // JSON serialized entity
    
    [Required]
    public int RetryCount { get; set; } = 0;
    
    public string? LastError { get; set; }
    
    public DateTime? LastAttemptAt { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Processing, Failed, Completed
    
    [Required]
    public int Priority { get; set; } = 0;
}

#endregion

