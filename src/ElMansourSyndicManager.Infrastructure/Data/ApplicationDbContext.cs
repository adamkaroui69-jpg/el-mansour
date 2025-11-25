using Microsoft.EntityFrameworkCore;
using ElMansourSyndicManager.Core.Domain.Entities;

namespace ElMansourSyndicManager.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Define your DbSets here, for example:
        public DbSet<User> Users { get; set; } = default!;
        // Add other DbSets for your entities (e.g., Payments, Receipts, etc.)
        public DbSet<AuditLog> AuditLogs { get; set; } = default!;
        public DbSet<Backup> Backups { get; set; } = default!;
        public DbSet<Building> Buildings { get; set; } = default!;
        public DbSet<Expense> Expenses { get; set; } = default!;
        public DbSet<House> Houses { get; set; } = default!;
        public DbSet<Maintenance> Maintenances { get; set; } = default!;
        public DbSet<Notification> Notifications { get; set; } = default!;
        public DbSet<Payment> Payments { get; set; } = default!;
        public DbSet<Receipt> Receipts { get; set; } = default!;
        public DbSet<Document> Documents { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.PasswordSalt).IsRequired();
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
                entity.Property(e => e.HouseCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IsActive).IsRequired();
            });

            // Configure House entity
            modelBuilder.Entity<House>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.HouseCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.BuildingCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.OwnerName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ContactNumber).HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.MonthlyAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.IsActive).IsRequired();
            });

            // Configure Payment entity
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.HouseCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Month).IsRequired().HasMaxLength(7);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            });

            // Configure other entities as needed
        }
    }
}
