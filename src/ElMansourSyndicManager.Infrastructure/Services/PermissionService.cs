using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.Infrastructure.Data;
using ElMansourSyndicManager.Core.Domain.Entities;

namespace ElMansourSyndicManager.Infrastructure.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthenticationService _authService;
        private readonly ILogger<PermissionService> _logger;
        
        // Cache simple pour éviter trop de requêtes DB
        // Clé: UserId -> Set<Permissions>
        private static readonly Dictionary<string, (DateTime Expiry, HashSet<string> Permissions)> _permissionsCache 
            = new Dictionary<string, (DateTime, HashSet<string>)>();
        
        private const int CACHE_DURATION_MINUTES = 5;

        public PermissionService(
            ApplicationDbContext context,
            IAuthenticationService authService,
            ILogger<PermissionService> logger)
        {
            _context = context;
            _authService = authService;
            _logger = logger;
        }

        public async Task<bool> HasPermissionAsync(string permissionCode)
        {
            var user = _authService.CurrentUser;
            if (user == null) return false;

            return await HasPermissionAsync(user.Id.ToString(), permissionCode);
        }

        public async Task<bool> HasPermissionAsync(string userId, string permissionCode)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(permissionCode)) return false;

            var permissions = await GetUserPermissionsAsync(userId);
            
            // L'administrateur a accès à tout (wildcard)
            if (permissions.Contains("*")) return true;

            return permissions.Contains(permissionCode);
        }

        public async Task EnforcePermissionAsync(string permissionCode)
        {
            if (!await HasPermissionAsync(permissionCode))
            {
                var user = _authService.CurrentUser?.Username ?? "Unknown";
                _logger.LogWarning("Access denied for user {User} on permission {Permission}", user, permissionCode);
                throw new UnauthorizedAccessException($"L'accès à la ressource '{permissionCode}' est refusé.");
            }
        }

        public async Task<HashSet<string>> GetUserPermissionsAsync(string userId)
        {
            // Vérifier le cache
            if (_permissionsCache.TryGetValue(userId, out var cached))
            {
                if (DateTime.UtcNow < cached.Expiry)
                {
                    return cached.Permissions;
                }
            }

            var permissions = new HashSet<string>();

            try
            {
                // 1. Récupérer l'utilisateur et son rôle
                // Note: On regarde aussi l'ancien champ "Role" pour la compatibilité
                var user = await _context.Users
                    .Include(u => u.AssignedRole!)
                    .ThenInclude(r => r.Permissions)
                    .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

                if (user == null) return permissions;

                // 2. Vérification Super Admin (Legacy & RBAC)
                bool isAdmin = user.Role == "Admin" || (user.AssignedRole?.Name == "Administrateur") || (user.AssignedRole?.IsSystem == true && user.AssignedRole.Name == "Admin");
                
                if (isAdmin)
                {
                    permissions.Add("*"); // Wildcard pour admin
                }
                else if (user.AssignedRole != null)
                {
                    // 3. Ajouter les permissions du rôle
                    foreach (var perm in user.AssignedRole.Permissions)
                    {
                        permissions.Add(perm.PermissionCode);
                    }
                }

                // Pour la phase de transition : Si pas de rôle assigné mais Role="Manager" (exemple), on pourrait hardcoder des droits
                // Mais pour l'instant on se base sur la DB.

                // Mettre en cache
                lock (_permissionsCache)
                {
                    if (_permissionsCache.ContainsKey(userId))
                        _permissionsCache[userId] = (DateTime.UtcNow.AddMinutes(CACHE_DURATION_MINUTES), permissions);
                    else
                        _permissionsCache.Add(userId, (DateTime.UtcNow.AddMinutes(CACHE_DURATION_MINUTES), permissions));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des permissions pour {UserId}", userId);
            }

            return permissions;
        }

        public Task RefreshPermissionsAsync()
        {
            lock (_permissionsCache)
            {
                _permissionsCache.Clear();
            }
            return Task.CompletedTask;
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Roles
                .Include(r => r.Permissions)
                .Include(r => r.Users)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<Role?> GetRoleByIdAsync(Guid roleId)
        {
            return await _context.Roles
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == roleId);
        }

        public async Task CreateRoleAsync(Role role, IEnumerable<string> permissionCodes)
        {
            // Vérifier doublon
            if (await _context.Roles.AnyAsync(r => r.Name == role.Name))
                throw new InvalidOperationException($"Le rôle '{role.Name}' existe déjà.");

            _context.Roles.Add(role);
            
            foreach (var code in permissionCodes)
            {
                _context.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionCode = code });
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateRoleAsync(Role role, IEnumerable<string> permissionCodes)
        {
            var existingRole = await _context.Roles.Include(r => r.Permissions).FirstOrDefaultAsync(r => r.Id == role.Id);
            if (existingRole == null) throw new KeyNotFoundException("Rôle introuvable");

            if (existingRole.IsSystem && existingRole.Name != role.Name)
                throw new InvalidOperationException("Impossible de renommer un rôle système.");

            existingRole.Name = role.Name;
            existingRole.Description = role.Description;
            existingRole.UpdatedAt = DateTime.UtcNow;

            // Mettre à jour les permissions
            // 1. Supprimer les anciennes
            _context.RolePermissions.RemoveRange(existingRole.Permissions);
            
            // 2. Ajouter les nouvelles
            foreach (var code in permissionCodes)
            {
                _context.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionCode = code });
            }

            await _context.SaveChangesAsync();
            await RefreshPermissionsAsync(); // Invalider le cache
        }

        public async Task DeleteRoleAsync(Guid roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null) throw new KeyNotFoundException("Rôle introuvable");

            if (role.IsSystem)
                throw new InvalidOperationException("Impossible de supprimer un rôle système.");

            // Vérifier s'il y a des utilisateurs (Optionnel : bloquer ou set null)
            // La FK est en SetNull, donc ça va juste détacher les users.
            
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
        }
    }
}
