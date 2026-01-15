using ElMansourSyndicManager.Core.Configuration;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.Infrastructure.Data;
using ElMansourSyndicManager.Infrastructure.Services;
using ElMansourSyndicManager.Services;
using ElMansourSyndicManager.Services.Navigation;
using ElMansourSyndicManager.ViewModels;
using ElMansourSyndicManager.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;

namespace ElMansourSyndicManager;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    public App()
    {
        // Initialiser le logger global dès le démarrage
        ConfigureGlobalLogging();
        
        // S'abonner aux gestionnaires d'exceptions globaux
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    private void ConfigureGlobalLogging()
    {
        var config = AppConfiguration.Instance;
        var logPath = Path.Combine(config.LogsDirectory, "log-.txt");

        // Configuration Serilog robuste
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            // Log dans la console pour le debug
            .WriteTo.Console()
            // Log dans les fichiers avec rotation journalière
            .WriteTo.File(logPath, 
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information("Application Démarrée. Version: {Version}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        HandleFatalException(e.Exception, "Erreur UI Non Gérée");
        e.Handled = true; // Empêcher le crash brutal si possible
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            HandleFatalException(ex, "Erreur Critique AppDomain");
        }
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        HandleFatalException(e.Exception, "Erreur Tâche de Fond Non Observée");
        e.SetObserved();
    }

    private void HandleFatalException(Exception ex, string context)
    {
        Log.Fatal(ex, "Une erreur critique est survenue: {Context}", context);

        string userMessage = "Une erreur inattendue est survenue.\n\n" +
                             "L'application a généré un rapport d'erreur dans le dossier 'logs'.\n" +
                             "Veuillez contacter le support si le problème persiste.\n\n" +
                             $"Détails: {ex.Message}";

        MessageBox.Show(userMessage, "Erreur de l'Application", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure services
        var services = new ServiceCollection();
        
        // Use Centralized AppConfiguration
        var appConfig = AppConfiguration.Instance;
        var dbPath = appConfig.GetDatabasePath();
        
        // logging with Serilog
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddSerilog(dispose: true);
        });

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseSqlite($"Data Source={dbPath}");
            
            #if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            #endif
            
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
                Log.Fatal(ex, "Erreur fatale lors de l'initialisation de l'application");
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
        // 2. Infrastructure (Repositories & Domain Services)
        services.AddInfrastructureServices();

        // 3. Navigation & UI Services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<ISnackbarMessageQueue, SnackbarMessageQueue>();
        services.AddSingleton<IDialogService, DialogService>();

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
        services.AddTransient<FinancialReportsViewModel>();
        services.AddTransient<AuditViewModel>();
        services.AddTransient<RolesViewModel>();
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
        services.AddTransient<FinancialReportsView>();
        services.AddTransient<AuditView>();
        services.AddTransient<SettingsView>();
        services.AddTransient<MaintenanceView>();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        Log.Information("Arrêt de l'application...");

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
        catch (Exception ex)
        {
            Log.Error(ex, "Erreur lors de la sauvegarde automatique à la fermeture");
        }

        _serviceProvider?.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }

    public static ServiceProvider? Services { get; private set; }
}
