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
        Console.WriteLine("Application OnStartup method called."); // Added for debugging

        // Configure services
        var services = new ServiceCollection();
        
        // Database
        // Configuration
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var builder = new ConfigurationBuilder()
            .SetBasePath(baseDir)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        var configuration = builder.Build();

        // Database
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
                if (!System.IO.Directory.Exists(dataDir))
                {
                    System.IO.Directory.CreateDirectory(dataDir);
                }
                var dbPath = System.IO.Path.Combine(dataDir, "local.db");
                options.UseSqlite($"Data Source={dbPath}");
            }
        });

        ConfigureServices(services);

        // Build service provider
        _serviceProvider = services.BuildServiceProvider();
        Services = _serviceProvider;

        // Create admin user if not exists
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Créer la base de données si elle n'existe pas (sans la supprimer !)
            await dbContext.Database.EnsureCreatedAsync();
            
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var houseRepository = scope.ServiceProvider.GetRequiredService<IHouseRepository>();

            // Ensure House D05 exists
            var adminHouse = await houseRepository.GetByCodeAsync("D05");
            if (adminHouse == null)
            {
                adminHouse = new ElMansourSyndicManager.Core.Domain.Entities.House
                {
                    HouseCode = "D05",
                    BuildingCode = "D",
                    OwnerName = "Admin System",
                    ContactNumber = "0000000000",
                    Email = "admin@syndic.com",
                    MonthlyAmount = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await houseRepository.CreateAsync(adminHouse);
            }

            // Create admin user with password "123456"
            var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
            
            // Generate hash for password "123456"
            var password = "123456";
            var (hash, salt) = GeneratePasswordHash(password);
            
            await userRepository.CreateAdminUserIfNotExistAsync("D05", "Admin", hash, salt);

            // --- MIGRATION MANUELLE POUR CORRIGER LE SCHEMA ---
            try 
            {
                // Maintenance: Add CompletedAt if missing
                try { await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE Maintenances ADD COLUMN CompletedAt TEXT NULL;"); } catch { }
                try { await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE Maintenances ADD COLUMN StartedAt TEXT NULL;"); } catch { }
                try { await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE Maintenances ADD COLUMN ScheduledDate TEXT NULL;"); } catch { }
                try { await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE Maintenances ADD COLUMN Type TEXT DEFAULT '';"); } catch { }
                try { await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE Maintenances ADD COLUMN Cost DECIMAL(18,2) DEFAULT 0;"); } catch { }
                try { await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE Maintenances ADD COLUMN Priority TEXT DEFAULT 'Normal';"); } catch { }
                try { await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE Maintenances ADD COLUMN CreatedBy TEXT DEFAULT '';"); } catch { }
                try { await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE Maintenances ADD COLUMN AssignedTo TEXT NULL;"); } catch { }
                try { await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE Maintenances ADD COLUMN ReportedBy TEXT NULL;"); } catch { }
                try { await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE Maintenances ADD COLUMN Notes TEXT NULL;"); } catch { }
                try { await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE Maintenances ADD COLUMN UpdatedAt TEXT NULL;"); } catch { }

                // Payments: Add GeneratedBy, RecordedBy if missing
                try { await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE Payments ADD COLUMN GeneratedBy TEXT DEFAULT '';"); } catch { }
                try { await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE Payments ADD COLUMN RecordedBy TEXT DEFAULT '';"); } catch { }
                try { await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE Payments ADD COLUMN ReferenceNumber TEXT NULL;"); } catch { }
                try { await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE Payments ADD COLUMN UpdatedAt TEXT NULL;"); } catch { }

                // Expenses: Add RecordedBy, MaintenanceId, Notes if missing
                try { await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE Expenses ADD COLUMN RecordedBy TEXT DEFAULT '';"); } catch { }
                try { await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE Expenses ADD COLUMN MaintenanceId TEXT NULL;"); } catch { }
                try { await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE Expenses ADD COLUMN Notes TEXT NULL;"); } catch { }
                try { await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE Expenses ADD COLUMN UpdatedAt TEXT NULL;"); } catch { }
                
                // Houses: Add Email if missing
                try { await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE Houses ADD COLUMN Email TEXT NULL;"); } catch { }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Migration warning: {ex.Message}");
            }
            // --- FIN MIGRATION ---

            // --- DÉBUT SEEDING PERSONNALISÉ ---
            
            // 1. Définir la structure exacte attendue
            var expectedHouses = new List<ElMansourSyndicManager.Core.Domain.Entities.House>();

            // Fonction helper pour créer une maison
            void AddHouse(string code, string block, string owner, decimal amount = 100)
            {
                expectedHouses.Add(new ElMansourSyndicManager.Core.Domain.Entities.House
                {
                    Id = Guid.NewGuid(),
                    HouseCode = code,
                    BuildingCode = block,
                    OwnerName = owner,
                    MonthlyAmount = amount,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // Blocs A, C, D, E (16 appartements : RDC+3 * 4)
            foreach (var block in new[] { "A", "C", "D", "E" })
            {
                for (int i = 1; i <= 16; i++)
                {
                    AddHouse($"{block}{i:00}", block, $"Propriétaire {block}{i:00}");
                }
            }

            // Bloc B (16 appartements standards + 2 spéciaux)
            for (int i = 1; i <= 16; i++)
            {
                AddHouse($"B{i:00}", "B", $"Propriétaire B{i:00}");
            }

            // Bloc B - 4ème étage (Syndic + Concierge) -> Suite séquentielle ou codes spéciaux ?
            // Logique séquentielle : B17 et B18
            AddHouse("B17", "B", "Bureau de Syndicat");
            AddHouse("B18", "B", "Maison de Concierge");

            // Magasins
            // Bloc A : M01
            AddHouse("M01", "A", "Magasin M01 (Bloc A)", 150);
            
            // Bloc B : M02, M03
            AddHouse("M02", "B", "Magasin M02 (Bloc B)", 150);
            AddHouse("M03", "B", "Magasin M03 (Bloc B)", 150);

            // 2. Synchroniser avec la base de données
            var existingHouses = await houseRepository.GetAllActiveAsync();
            var existingCodes = existingHouses.Select(h => h.HouseCode).ToHashSet();

            // Créer les manquantes
            foreach (var house in expectedHouses)
            {
                if (!existingCodes.Contains(house.HouseCode))
                {
                    await houseRepository.CreateAsync(house);
                }
            }

            // 3. Nettoyer les maisons en trop (ex: Blocs F, G, H créés précédemment)
            // On garde D05 (Admin) et celles qu'on vient de définir
            var validCodes = expectedHouses.Select(h => h.HouseCode).ToHashSet();
            validCodes.Add("D05"); // Toujours garder l'admin

            foreach (var existing in existingHouses)
            {
                if (!validCodes.Contains(existing.HouseCode))
                {
                    // Suppression logique (IsActive = false) ou physique ?
                    // Pour nettoyer proprement la liste déroulante, on va essayer de supprimer physiquement.
                    // Si échec (clé étrangère), on désactive.
                    try
                    {
                        await houseRepository.DeleteAsync(existing);
                    }
                    catch
                    {
                        existing.IsActive = false;
                        await houseRepository.UpdateAsync(existing);
                    }
                }
            }
            
            // --- FIN SEEDING PERSONNALISÉ ---
        }

        // Show login window first (MainWindow will be shown after login)
        var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
        loginWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Repositories (add your implementations here)
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>(); // Added
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IHouseRepository, HouseRepository>(); // Uncommented and corrected implementation name
        services.AddScoped<IReceiptRepository, ReceiptRepository>();
        services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
        services.AddScoped<INotificationRepository, ElMansourSyndicManager.Infrastructure.Repositories.NotificationRepository>(); // Added
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        
        // Services
        services.AddScoped<IMaintenanceService, MaintenanceService>();
        services.AddScoped<INotificationService, NotificationService>(); // Added
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IDocumentService, DocumentService>();
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

    private static (string Hash, string Salt) GeneratePasswordHash(string password)
    {
        const int SaltSize = 32;
        const int HashSize = 32;
        const int PBKDF2Iterations = 10000;
        
        var salt = new byte[SaltSize];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        using var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(
            password, 
            salt, 
            PBKDF2Iterations, 
            System.Security.Cryptography.HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(HashSize);
        
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    public static ServiceProvider? Services { get; private set; }
}
