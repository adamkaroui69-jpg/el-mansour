using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.Infrastructure.Data.Repositories;
using ElMansourSyndicManager.Infrastructure.Repositories;
using ElMansourSyndicManager.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ElMansourSyndicManager.Infrastructure.Services;

/// <summary>
/// Centralized Dependency Injection for the Infrastructure and Core layers.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // 1. Repositories (Data Access)
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IHouseRepository, HouseRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IReceiptRepository, ReceiptRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IBackupRepository, BackupRepository>();

        // 2. Domain Services (Business Logic)
        // AuthenticationService must be Singleton to maintain user session across scopes
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IReceiptService, ReceiptService>();
        services.AddScoped<IMaintenanceService, MaintenanceService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IReportingService, ReportingService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAuditService, AuditService>(); // Service d'audit
        services.AddScoped<IPermissionService, PermissionService>(); // Service de permissions
        services.AddScoped<IBackupService, BackupService>();
        services.AddScoped<IFinancialService, FinancialService>(); // Gestion financière (Arriérés, Soldes)
        services.AddScoped<IExportService, ExportService>(); // Export Excel/CSV
        
        // 3. System Services (Startup & Maintenance)
        services.AddScoped<IAppInitializer, AppInitializer>();
        services.AddScoped<IDatabaseMigrator, DatabaseMigrator>();
        services.AddScoped<IDataSeeder, DataSeeder>();
        services.AddScoped<IUpdateService, UpdateService>();

        return services;
    }
}
