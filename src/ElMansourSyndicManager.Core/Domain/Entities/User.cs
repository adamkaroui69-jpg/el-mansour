using System;

namespace ElMansourSyndicManager.Core.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string PasswordSalt { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // Legacy: Admin, Syndic, Resident. To be replaced by RoleId.
        
        public Guid? RoleId { get; set; } // Nullable pour faciliter la migration
        public virtual Role? AssignedRole { get; set; }

        public Guid? HouseId { get; set; } // Ajouté HouseId
        public string HouseCode { get; set; } = string.Empty; // For residents
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
