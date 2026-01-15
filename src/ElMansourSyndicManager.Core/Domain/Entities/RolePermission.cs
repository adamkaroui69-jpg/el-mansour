using System;

namespace ElMansourSyndicManager.Core.Domain.Entities
{
    public class RolePermission : BaseEntity
    {
        public Guid RoleId { get; set; }
        public virtual Role Role { get; set; } = null!;
        
        public string PermissionCode { get; set; } = string.Empty;
    }
}
