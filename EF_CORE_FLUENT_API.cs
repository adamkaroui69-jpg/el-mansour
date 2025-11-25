// El Mansour Syndic Manager - EF Core Fluent API Configuration
// File: src/ElMansourSyndicManager.Infrastructure/Data/Local/Configurations/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ElMansourSyndicManager.Infrastructure.Data.Local.Entities;

namespace ElMansourSyndicManager.Infrastructure.Data.Local.Configurations;

#region Residence Configuration

public class ResidenceConfiguration : IEntityTypeConfiguration<Residence>
{
    public void Configure(EntityTypeBuilder<Residence> builder)
    {
        builder.ToTable("Residences");
        
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.Id)
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(r => r.Name)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.HasIndex(r => r.Name)
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");
        
        builder.Property(r => r.Country)
            .HasMaxLength(50)
            .HasDefaultValue("Morocco");
        
        // Relationships
        builder.HasMany(r => r.Buildings)
            .WithOne(b => b.Residence)
            .HasForeignKey(b => b.ResidenceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

#endregion

#region Building Configuration

public class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.ToTable("Buildings");
        
        builder.HasKey(b => b.Id);
        
        builder.Property(b => b.Code)
            .HasMaxLength(1)
            .IsRequired();
        
        builder.HasIndex(b => new { b.ResidenceId, b.Code })
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");
        
        builder.HasCheckConstraint("CHK_BuildingCode", 
            "[Code] IN ('A', 'B', 'C', 'D', 'E')");
        
        // Relationships
        builder.HasMany(b => b.Houses)
            .WithOne(h => h.Building)
            .HasForeignKey(h => h.BuildingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

#endregion

#region House Configuration

public class HouseConfiguration : IEntityTypeConfiguration<House>
{
    public void Configure(EntityTypeBuilder<House> builder)
    {
        builder.ToTable("Houses");
        
        builder.HasKey(h => h.Id);
        
        builder.Property(h => h.Code)
            .HasMaxLength(20)
            .IsRequired();
        
        builder.HasIndex(h => h.Code)
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");
        
        builder.Property(h => h.MonthlyAmount)
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0.00m);
        
        builder.HasCheckConstraint("CHK_HouseType", 
            "[Type] IN ('House', 'Shop', 'Office', 'Concierge')");
        
        builder.HasCheckConstraint("CHK_HouseFloor", 
            "[Floor] >= 0");
        
        // Relationships
        builder.HasMany(h => h.Users)
            .WithOne(u => u.House)
            .HasForeignKey(u => u.HouseId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasMany(h => h.Payments)
            .WithOne(p => p.House)
            .HasForeignKey(p => p.HouseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

#endregion

#region User Configuration

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.HouseCode)
            .HasMaxLength(20)
            .IsRequired();
        
        builder.HasIndex(u => u.HouseCode)
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");
        
        builder.Property(u => u.PasswordHash)
            .HasMaxLength(256)
            .IsRequired();
        
        builder.Property(u => u.Salt)
            .HasMaxLength(64)
            .IsRequired();
        
        builder.HasCheckConstraint("CHK_UserRole", 
            "[Role] IN ('Admin', 'SyndicMember')");
        
        // Relationships
        builder.HasMany(u => u.Payments)
            .WithOne(p => p.RecordedByUser)
            .HasForeignKey(p => p.RecordedBy)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(u => u.CreatedMaintenance)
            .WithOne(m => m.CreatedByUser)
            .HasForeignKey(m => m.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(u => u.AssignedMaintenance)
            .WithOne(m => m.AssignedToUser)
            .HasForeignKey(m => m.AssignedTo)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

#endregion

#region Payment Configuration

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Amount)
            .HasColumnType("decimal(10,2)")
            .IsRequired();
        
        builder.Property(p => p.Month)
            .HasMaxLength(7)
            .IsRequired();
        
        builder.HasIndex(p => new { p.HouseCode, p.Month })
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");
        
        builder.HasCheckConstraint("CHK_PaymentMonthFormat", 
            "[Month] GLOB '[0-9][0-9][0-9][0-9]-[0-9][0-9]'");
        
        builder.HasCheckConstraint("CHK_PaymentStatus", 
            "[Status] IN ('Paid', 'Unpaid', 'Overdue', 'Cancelled')");
        
        // Relationships
        builder.HasOne(p => p.Receipt)
            .WithOne(r => r.Payment)
            .HasForeignKey<Receipt>(r => r.PaymentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

#endregion

#region Receipt Configuration

public class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
{
    public void Configure(EntityTypeBuilder<Receipt> builder)
    {
        builder.ToTable("Receipts");
        
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.MimeType)
            .HasMaxLength(100)
            .HasDefaultValue("application/pdf");
        
        // Relationships
        builder.HasOne(r => r.SignatureUser)
            .WithMany()
            .HasForeignKey(r => r.SignatureUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

#endregion

#region Expense Configuration

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Amount)
            .HasColumnType("decimal(10,2)")
            .IsRequired();
        
        builder.Property(e => e.Month)
            .HasMaxLength(7)
            .IsRequired();
        
        builder.HasCheckConstraint("CHK_ExpenseMonthFormat", 
            "[Month] GLOB '[0-9][0-9][0-9][0-9]-[0-9][0-9]'");
        
        builder.HasCheckConstraint("CHK_ExpenseCategory", 
            "[Category] IN ('Maintenance', 'Utilities', 'Insurance', 'Other')");
        
        // Relationships
        builder.HasOne(e => e.Maintenance)
            .WithMany(m => m.Expenses)
            .HasForeignKey(e => e.MaintenanceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

#endregion

#region Maintenance Configuration

public class MaintenanceConfiguration : IEntityTypeConfiguration<Maintenance>
{
    public void Configure(EntityTypeBuilder<Maintenance> builder)
    {
        builder.ToTable("Maintenance");
        
        builder.HasKey(m => m.Id);
        
        builder.Property(m => m.Cost)
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0.00m);
        
        builder.Property(m => m.Status)
            .HasMaxLength(20)
            .HasDefaultValue("Pending");
        
        builder.HasCheckConstraint("CHK_MaintenanceType", 
            "[Type] IN ('Repair', 'Cleaning', 'Security', 'Other')");
        
        builder.HasCheckConstraint("CHK_MaintenanceStatus", 
            "[Status] IN ('Pending', 'InProgress', 'Completed', 'Cancelled')");
        
        builder.HasCheckConstraint("CHK_MaintenancePriority", 
            "[Priority] IN ('Low', 'Normal', 'High', 'Urgent')");
        
        // Relationships
        builder.HasMany(m => m.Documents)
            .WithOne(d => d.Maintenance)
            .HasForeignKey(d => d.MaintenanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

#endregion

#region MaintenanceDocument Configuration

public class MaintenanceDocumentConfiguration : IEntityTypeConfiguration<MaintenanceDocument>
{
    public void Configure(EntityTypeBuilder<MaintenanceDocument> builder)
    {
        builder.ToTable("MaintenanceDocuments");
        
        builder.HasKey(d => d.Id);
        
        builder.HasCheckConstraint("CHK_DocumentType", 
            "[DocumentType] IN ('Justificative', 'Invoice', 'Photo', 'Other')");
    }
}

#endregion

#region Notification Configuration

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        
        builder.HasKey(n => n.Id);
        
        builder.Property(n => n.IsRead)
            .HasDefaultValue(false);
        
        builder.HasCheckConstraint("CHK_NotificationType", 
            "[Type] IN ('UnpaidHouse', 'MaintenanceDue', 'System', 'Info')");
        
        builder.HasCheckConstraint("CHK_NotificationPriority", 
            "[Priority] IN ('Low', 'Normal', 'High', 'Urgent')");
        
        builder.HasIndex(n => n.IsRead);
        builder.HasIndex(n => n.CreatedAt);
    }
}

#endregion

#region AuditLog Configuration

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        
        builder.HasKey(a => a.Id);
        
        builder.HasCheckConstraint("CHK_AuditAction", 
            "[Action] IN ('Create', 'Update', 'Delete', 'View', 'Login', 'Logout', 'Export', 'Backup', 'Restore')");
        
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.Action);
        builder.HasIndex(a => a.EntityType);
        builder.HasIndex(a => a.Timestamp);
    }
}

#endregion

#region Setting Configuration

public class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable("Settings");
        
        builder.HasKey(s => s.Key);
        
        builder.Property(s => s.Key)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.HasCheckConstraint("CHK_SettingType", 
            "[Type] IN ('String', 'Int', 'Bool', 'DateTime', 'Json')");
        
        builder.HasIndex(s => s.Category);
    }
}

#endregion

#region Backup Configuration

public class BackupConfiguration : IEntityTypeConfiguration<Backup>
{
    public void Configure(EntityTypeBuilder<Backup> builder)
    {
        builder.ToTable("Backups");
        
        builder.HasKey(b => b.Id);
        
        builder.Property(b => b.IsAutomatic)
            .HasDefaultValue(false);
        
        builder.HasCheckConstraint("CHK_BackupType", 
            "[BackupType] IN ('Full', 'Database', 'Documents')");
        
        builder.HasIndex(b => b.CreatedBy);
        builder.HasIndex(b => b.CreatedAt);
    }
}

#endregion

#region SyncQueue Configuration

public class SyncQueueConfiguration : IEntityTypeConfiguration<SyncQueue>
{
    public void Configure(EntityTypeBuilder<SyncQueue> builder)
    {
        builder.ToTable("SyncQueue");
        
        builder.HasKey(s => s.Id);
        
        builder.HasCheckConstraint("CHK_SyncOperation", 
            "[Operation] IN ('Create', 'Update', 'Delete')");
        
        builder.HasCheckConstraint("CHK_SyncStatus", 
            "[Status] IN ('Pending', 'Processing', 'Failed', 'Completed')");
        
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => new { s.Priority, s.CreatedAt })
            .IsDescending(true, true);
    }
}

#endregion

