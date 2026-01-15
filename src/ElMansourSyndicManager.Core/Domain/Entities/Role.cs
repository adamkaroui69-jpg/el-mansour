using System;
using System.Collections.Generic;

namespace ElMansourSyndicManager.Core.Domain.Entities
{
    public class Role : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Indique si le rôle est un rôle système (ex: Admin) qui ne peut pas être supprimé.
        /// </summary>
        public bool IsSystem { get; set; } = false;

        public virtual ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
