using ElMansourSyndicManager.Core.Domain.Constants;
using ElMansourSyndicManager.Core.Domain.Entities;
using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ElMansourSyndicManager.Infrastructure.Services;

public class DataSeeder : IDataSeeder
{
    private readonly IUserRepository _userRepository;
    private readonly IHouseRepository _houseRepository;
    private readonly IAuthenticationService _authService;
    private readonly ILogger<DataSeeder> _logger;
    private readonly ApplicationDbContext _dbContext;

    public DataSeeder(
        IUserRepository userRepository,
        IHouseRepository houseRepository,
        IAuthenticationService authService,
        ILogger<DataSeeder> logger,
        ApplicationDbContext dbContext)
    {
        _userRepository = userRepository;
        _houseRepository = houseRepository;
        _authService = authService;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting data seeding...");

            // 1. Seed Roles & Permissions (Prioritaire)
            await SeedRolesAsync(cancellationToken);

            // 2. Ensure House D05 exists (Admin House)
            var adminHouse = await _houseRepository.GetByCodeAsync("D05", cancellationToken);
            if (adminHouse == null)
            {
                _logger.LogInformation("Creating admin house D05");
                adminHouse = new House
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
                await _houseRepository.CreateAsync(adminHouse, cancellationToken);
            }

            // 3. Create admin user
            var password = "123456";
            var (hash, salt) = _authService.HashPassword(password);
            
            await _userRepository.CreateAdminUserIfNotExistAsync("D05", "Admin", hash, salt, cancellationToken);
            
            // Link Admin user to Admin Role if not done
            var adminUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == "D05" || u.Username == "Admin", cancellationToken);
            var adminRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Administrateur", cancellationToken);
            
            if (adminUser != null && adminRole != null && adminUser.RoleId == null)
            {
                adminUser.RoleId = adminRole.Id;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            // 4. Seed other houses
            await SeedHousesAsync(cancellationToken);

            _logger.LogInformation("Data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during data seeding");
            throw;
        }
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        // 1. Définir les rôles par défaut
        var adminRoleName = "Administrateur";
        var userRoleName = "Résident";

        // Vérifier Admin
        var adminRole = await _dbContext.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Name == adminRoleName, cancellationToken);
            
        if (adminRole == null)
        {
            adminRole = new Role 
            { 
                Name = adminRoleName, 
                Description = "Accès complet au système", 
                IsSystem = true 
            };
            _dbContext.Roles.Add(adminRole);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        
        // Assigner TOUTES les permissions à Admin
        var allPermissions = AppPermissions.GetAll();
        
        // Note: Lors de la création initiale, adminRole.Permissions est vide mais non null car initialisé dans le constructeur de l'entité Role.
        // Si récupéré de la BDD, Include l'a chargé.
        var existingPerms = adminRole.Permissions?.Select(p => p.PermissionCode).ToHashSet() ?? new HashSet<string>();
        
        foreach (var perm in allPermissions)
        {
            if (!existingPerms.Contains(perm))
            {
                _dbContext.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionCode = perm });
            }
        }

        // Vérifier Résident
        var residentRole = await _dbContext.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Name == userRoleName, cancellationToken);
            
        if (residentRole == null)
        {
            residentRole = new Role
            {
                Name = userRoleName,
                Description = "Accès standard résident",
                IsSystem = true
            };
            _dbContext.Roles.Add(residentRole);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        
        // Assigner permissions limitées (Lecture)
        var residentPerms = new[] 
        { 
            AppPermissions.Payments.View,
            AppPermissions.Reports.View,
            AppPermissions.Payments.Create, // Peut déclarer un paiement
            AppPermissions.Documents.View
        };
        
        var existingResPerms = residentRole.Permissions?.Select(p => p.PermissionCode).ToHashSet() ?? new HashSet<string>();
        foreach (var perm in residentPerms)
        {
            if (!existingResPerms.Contains(perm))
            {
                _dbContext.RolePermissions.Add(new RolePermission { RoleId = residentRole.Id, PermissionCode = perm });
            }
        }
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedHousesAsync(CancellationToken cancellationToken)
    {
        var expectedHouses = new List<House>();

        // Fonction helper pour créer une maison
        void AddHouse(string code, string block, string owner, decimal amount = 100)
        {
            expectedHouses.Add(new House
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

        // Bloc B - 4ème étage (Syndic + Concierge)
        AddHouse("B17", "B", "Bureau de Syndicat");
        AddHouse("B18", "B", "Maison de Concierge");

        // Magasins
        // Bloc A : M01
        AddHouse("M01", "A", "Magasin M01 (Bloc A)", 150);
        
        // Bloc B : M02, M03
        AddHouse("M02", "B", "Magasin M02 (Bloc B)", 150);
        AddHouse("M03", "B", "Magasin M03 (Bloc B)", 150);

        // Synchroniser avec la base de données
        var existingHouses = await _houseRepository.GetAllActiveAsync(cancellationToken);
        var existingCodes = existingHouses.Select(h => h.HouseCode).ToHashSet();

        // Créer les manquantes
        foreach (var house in expectedHouses)
        {
            if (!existingCodes.Contains(house.HouseCode))
            {
                await _houseRepository.CreateAsync(house, cancellationToken);
            }
        }

        // Nettoyer les maisons en trop
        var validCodes = expectedHouses.Select(h => h.HouseCode).ToHashSet();
        validCodes.Add("D05"); // Toujours garder l'admin

        foreach (var existing in existingHouses)
        {
            if (!validCodes.Contains(existing.HouseCode))
            {
                try
                {
                    await _houseRepository.DeleteAsync(existing, cancellationToken);
                }
                catch
                {
                    existing.IsActive = false;
                    await _houseRepository.UpdateAsync(existing, cancellationToken);
                }
            }
        }
    }
}
