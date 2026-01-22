using Microsoft.EntityFrameworkCore;
using ElMansourSyndicManager.Core.Domain.Entities;
using ElMansourSyndicManager.Core.Configuration;
using System.IO;

namespace ElMansourSyndicManager.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var config = AppConfiguration.Instance;

                if (string.Equals(config.DatabaseProvider, "SqlServer", StringComparison.OrdinalIgnoreCase))
                {
                    optionsBuilder.UseSqlServer(config.ConnectionString);
                }
                else if (string.Equals(config.DatabaseProvider, "PostgreSQL", StringComparison.OrdinalIgnoreCase))
                {
                    // Utiliser PostgreSQL (Supabase, Neon, etc.)
                    optionsBuilder.UseNpgsql(config.ConnectionString);
                }
                else
                {
                    var dbPath = config.GetDatabasePath();

                    // Ensure directory exists for SQLite
                    var directory = Path.GetDirectoryName(dbPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    optionsBuilder.UseSqlite($"Data Source={dbPath}");
                }

                #if DEBUG
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.EnableDetailedErrors();
                #endif
            }
        }

        // Define your DbSets here, for example:
        public DbSet<User> Users { get; set; } = default!;
        // Add other DbSets for your entities (e.g., Payments, Receipts, etc.)
        public DbSet<AuditLog> AuditLogs { get; set; } = default!;
        public DbSet<Role> Roles { get; set; } = default!;
        public DbSet<RolePermission> RolePermissions { get; set; } = default!;
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
                // Relation User -> Role
                entity.HasOne(d => d.AssignedRole)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Role entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(200);
            });

            // Configure RolePermission entity
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.RoleId, e.PermissionCode }).IsUnique();
                entity.Property(e => e.PermissionCode).IsRequired().HasMaxLength(100);
                
                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Permissions)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
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
