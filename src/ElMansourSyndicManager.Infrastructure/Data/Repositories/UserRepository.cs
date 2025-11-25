using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Entities; // Utiliser les entités
using ElMansourSyndicManager.Infrastructure.Data;
using ElMansourSyndicManager.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElMansourSyndicManager.Infrastructure.Data.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    private readonly IHouseRepository _houseRepository;

    public UserRepository(ApplicationDbContext context, IHouseRepository houseRepository) : base(context)
    {
        _houseRepository = houseRepository;
    }

    public async Task<User?> GetByHouseCodeAsync(string houseCode, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.HouseCode == houseCode, cancellationToken);
    }

    public async Task<List<User>> GetByRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        return await _context.Users.Where(u => u.Role == role).ToListAsync(cancellationToken);
    }

    public new async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) // Ajout du mot-clé new
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task UpdatePasswordAsync(Guid userId, string hash, string salt, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(userId, cancellationToken);
        if (user != null)
        {
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(user, cancellationToken); // Utilisation de la méthode UpdateAsync du dépôt générique
            await SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateLastLoginAsync(Guid userId, DateTime lastLogin, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(userId, cancellationToken);
        if (user != null)
        {
            user.LastLogin = lastLogin;
            user.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(user, cancellationToken); // Utilisation de la méthode UpdateAsync du dépôt générique
            await SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> GetActiveSyndicMemberCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users.CountAsync(u => u.Role == "Syndic" && u.IsActive, cancellationToken);
    }

    public async Task CreateAdminUserIfNotExistAsync(string houseCode, string username, string passwordHash, string salt, CancellationToken cancellationToken = default)
    {
        var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.HouseCode == houseCode && u.Role == "Admin", cancellationToken);

        if (adminUser == null)
        {
            var house = await _houseRepository.GetByCodeAsync(houseCode, cancellationToken);
            if (house == null)
            {
                throw new Exception($"House with code '{houseCode}' not found.");
            }

            var newUser = new User
            {
                HouseCode = houseCode,
                Username = username,
                PasswordHash = passwordHash,
                PasswordSalt = salt,
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await CreateAsync(newUser, cancellationToken); // Utilisation de la méthode CreateAsync du dépôt générique
            await SaveChangesAsync(cancellationToken);
        }
    }
}
