using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ElMansourSyndicManager.Infrastructure.Services;

/// <summary>
/// Extension methods for dependency injection
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Services
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
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IBackupService, BackupService>();
        
        // Startup Services
        services.AddScoped<IAppInitializer, AppInitializer>();
        services.AddScoped<IDatabaseMigrator, DatabaseMigrator>();
        services.AddScoped<IDataSeeder, DataSeeder>();

        return services;
    }
}
