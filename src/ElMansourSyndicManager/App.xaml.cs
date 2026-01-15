using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.Infrastructure.Data;
using ElMansourSyndicManager.Infrastructure.Services;
using ElMansourSyndicManager.Services.Navigation;
using ElMansourSyndicManager.ViewModels;
using ElMansourSyndicManager.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.IO;
using ElMansourSyndicManager.Core.Configuration;

namespace ElMansourSyndicManager;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    public App()
    {
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
    }

    private bool _isHandlingException;

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        if (_isHandlingException)
        {
            e.Handled = true;
            return;
        }

        _isHandlingException = true;

        try
        {
            string errorMessage = $"Une erreur inattendue s'est produite : {e.Exception.Message}\n\nDétails : {e.Exception.StackTrace}";
            
            // Log to file as fallback
            try 
            {
                System.IO.File.AppendAllText("crash_log.txt", $"{DateTime.Now}: {errorMessage}\n--------------------------------\n");
            }
            catch { /* Ignore file log errors */ }

            MessageBox.Show(errorMessage, "Erreur Critique", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch
        {
            // Ignore errors during error reporting
        }
        finally
        {
            e.Handled = true;
            Shutdown();
        }
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure services
        var services = new ServiceCollection();
        
        // Use Centralized AppConfiguration
        var appConfig = AppConfiguration.Instance;
        var dbPath = appConfig.GetDatabasePath();
        
        // Directories are already ensured by AppConfiguration constructor
        
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseSqlite($"Data Source={dbPath}");
            
            // Enable detailed logging in debug mode
            #if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            #endif
            
            // Configure query tracking behavior
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }, ServiceLifetime.Scoped);

        ConfigureServices(services);

        // Build service provider
        _serviceProvider = services.BuildServiceProvider();
        Services = _serviceProvider;

        // Initialize Application
        using (var scope = _serviceProvider.CreateScope())
        {
            try 
            {
                // 1. Initialize Resources (Directories, etc.)
                var appInitializer = scope.ServiceProvider.GetRequiredService<IAppInitializer>();
                appInitializer.Initialize();

                // 2. Migrate Database
                var migrator = scope.ServiceProvider.GetRequiredService<IDatabaseMigrator>();
                await migrator.MigrateAsync();

                // 3. Seed Data
                var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
                await seeder.SeedAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur critique est survenue au démarrage : {ex.Message}", "Erreur Fatale", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }
        }

        // Show login window first (MainWindow will be shown after login)
        var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
        loginWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // 1. Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        // 2. Infrastructure (Repositories & Domain Services)
        services.AddInfrastructureServices();

        // 3. Navigation
        services.AddSingleton<INavigationService, NavigationService>();

        // 4. ViewModels
        ConfigureViewModels(services);

        // 5. Views
        ConfigureViews(services);
    }

    private void ConfigureViewModels(IServiceCollection services)
    {
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        
        // Features
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<PaymentsViewModel>();
        services.AddTransient<ReceiptsViewModel>();
        services.AddTransient<ExpensesViewModel>();
        services.AddTransient<UsersViewModel>();
        services.AddTransient<DocumentsViewModel>();
        services.AddTransient<ReportsViewModel>();
        services.AddTransient<AuditViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<MaintenanceViewModel>();
    }

    private void ConfigureViews(IServiceCollection services)
    {
        // Windows
        services.AddTransient<LoginWindow>();
        services.AddTransient<MainWindow>();
        
        // Pages
        services.AddTransient<DashboardView>();
        services.AddTransient<PaymentsView>();
        services.AddTransient<ReceiptsView>();
        services.AddTransient<ExpensesView>();
        services.AddTransient<UsersView>();
        services.AddTransient<DocumentsView>();
        services.AddTransient<ReportsView>();
        services.AddTransient<AuditView>();
        services.AddTransient<SettingsView>();
        services.AddTransient<MaintenanceView>();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try 
        {
            if (_serviceProvider != null)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var backupService = scope.ServiceProvider.GetRequiredService<Core.Domain.Interfaces.Services.IBackupService>();
                    // Trigger automatic backup on exit
                    await backupService.RunBackupAsync(isAutomatic: true);
                }
            }
        }
        catch 
        {
            // Ignore backup errors on exit to not crash the shutdown process
        }

        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    public static ServiceProvider? Services { get; private set; }
}
