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
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IReceiptService, ReceiptService>();
        // Removed IMaintenanceService, MaintenanceService as implementation is missing
        // Removed IExpenseService, ExpenseService as implementation is missing
        services.AddScoped<IReportingService, ReportingService>();
        // Removed IDocumentService, DocumentService as implementation is missing
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAuditService, AuditService>();
        // Removed ISyncService, SyncService as implementation is missing
        services.AddScoped<IBackupService, BackupService>();

        return services;
    }
}
