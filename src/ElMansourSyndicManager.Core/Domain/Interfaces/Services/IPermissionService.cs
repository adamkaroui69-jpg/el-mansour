using System.Collections.Generic;
using System.Threading.Tasks;
using ElMansourSyndicManager.Core.Domain.Entities;

namespace ElMansourSyndicManager.Core.Domain.Interfaces.Services
{
    public interface IPermissionService
    {
        /// <summary>
        /// Vérifie si l'utilisateur courant possède la permission requise.
        /// </summary>
        Task<bool> HasPermissionAsync(string permissionCode);

        /// <summary>
        /// Vérifie si un utilisateur spécifique possède la permission.
        /// </summary>
        Task<bool> HasPermissionAsync(string userId, string permissionCode);

        /// <summary>
        /// Vérifie la permission pour l'utilisateur courant et lève une exception UnauthorizedAccessException si refusé.
        /// </summary>
        Task EnforcePermissionAsync(string permissionCode);

        /// <summary>
        /// Récupère toutes les permissions assignées à un utilisateur.
        /// </summary>
        Task<HashSet<string>> GetUserPermissionsAsync(string userId);

        /// <summary>
        /// Rafraîchit le cache des permissions (si implémenté).
        /// </summary>
        Task RefreshPermissionsAsync();

        // Gestion des Rôles
        Task<List<Role>> GetAllRolesAsync();
        Task<Role?> GetRoleByIdAsync(Guid roleId);
        Task CreateRoleAsync(Role role, IEnumerable<string> permissionCodes);
        Task UpdateRoleAsync(Role role, IEnumerable<string> permissionCodes);
        Task DeleteRoleAsync(Guid roleId);
    }
}
