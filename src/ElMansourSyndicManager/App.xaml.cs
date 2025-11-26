using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services; // Added using for IMaintenanceService
using ElMansourSyndicManager.Infrastructure.Data; // Changed from Data.Local
using ElMansourSyndicManager.Infrastructure.Data.Repositories; // Changed from Data.Local.Repositories
using ElMansourSyndicManager.Infrastructure.Services;
using ElMansourSyndicManager.Services.Navigation;
using ElMansourSyndicManager.ViewModels;
using ElMansourSyndicManager.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Controls; // Added for TextBlock
using System.Windows.Threading;

namespace ElMansourSyndicManager;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    public App()
    {
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"Une erreur inattendue s'est produite : {e.Exception.Message}\n\nDétails : {e.Exception.StackTrace}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
        Shutdown();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Console.WriteLine("Application OnStartup method called.");

        // Configure services
        var services = new ServiceCollection();
        
        // Database Configuration
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var builder = new ConfigurationBuilder()
            .SetBasePath(baseDir)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        var configuration = builder.Build();

        // Database Provider
        var dbProvider = configuration["DatabaseProvider"] ?? "Sqlite";
        
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                var connectionString = configuration.GetConnectionString("SqlServerConnection");
                options.UseSqlServer(connectionString);
            }
            else
            {
                // Default to Sqlite
                var dataDir = System.IO.Path.Combine(baseDir, "data");
                // Directory creation is handled by AppInitializer
                var dbPath = System.IO.Path.Combine(dataDir, "local.db");
                options.UseSqlite($"Data Source={dbPath}");
            }
        });

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
        // Repositories (add your implementations here)
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IHouseRepository, HouseRepository>();
        services.AddScoped<IReceiptRepository, ReceiptRepository>();
        services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
        services.AddScoped<INotificationRepository, ElMansourSyndicManager.Infrastructure.Repositories.NotificationRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        
        // Services registered via AddApplicationServices()
        services.AddApplicationServices();

        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<PaymentsViewModel>();
        services.AddTransient<ReceiptsViewModel>();
        services.AddTransient<ExpensesViewModel>();
        services.AddTransient<UsersViewModel>();
        services.AddTransient<DocumentsViewModel>();
        services.AddTransient<ReportsViewModel>();
        services.AddTransient<AuditViewModel>();
        services.AddTransient<SettingsViewModel>();

        // Views
        services.AddTransient<LoginWindow>();
        services.AddTransient<MainWindow>();
        services.AddTransient<DashboardView>();
        services.AddTransient<PaymentsView>();
        services.AddTransient<ReceiptsView>();
        services.AddTransient<ExpensesView>();
        services.AddTransient<UsersView>();
        services.AddTransient<DocumentsView>();
        services.AddTransient<ReportsView>();
        services.AddTransient<AuditView>();
        services.AddTransient<SettingsView>();

        // Navigation service
        services.AddSingleton<INavigationService, NavigationService>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    public static ServiceProvider? Services { get; private set; }
}
