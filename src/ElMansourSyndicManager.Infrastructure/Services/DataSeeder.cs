using ElMansourSyndicManager.Core.Domain.Entities;
using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ElMansourSyndicManager.Infrastructure.Services;

public class DataSeeder : IDataSeeder
{
    private readonly IUserRepository _userRepository;
    private readonly IHouseRepository _houseRepository;
    private readonly IAuthenticationService _authService;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(
        IUserRepository userRepository,
        IHouseRepository houseRepository,
        IAuthenticationService authService,
        ILogger<DataSeeder> logger)
    {
        _userRepository = userRepository;
        _houseRepository = houseRepository;
        _authService = authService;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting data seeding...");

            // 1. Ensure House D05 exists (Admin House)
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

            // 2. Create admin user
            var password = "123456";
            var (hash, salt) = _authService.HashPassword(password);
            
            await _userRepository.CreateAdminUserIfNotExistAsync("D05", "Admin", hash, salt, cancellationToken);

            // 3. Seed other houses
            await SeedHousesAsync(cancellationToken);

            _logger.LogInformation("Data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during data seeding");
            throw;
        }
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

        // Nettoyer les maisons en trop (ex: Blocs F, G, H créés précédemment)
        // On garde D05 (Admin) et celles qu'on vient de définir
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
