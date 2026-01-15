using System;
using System.Collections.Generic;

namespace ElMansourSyndicManager.Core.Domain.DTOs
{
    public class RoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsSystem { get; set; }
        public int UserCount { get; set; }
        
        // Liste des permissions actives pour ce rôle (codes)
        public List<string> Permissions { get; set; } = new List<string>();
    }

    public class PermissionGroupDto
    {
        public string GroupName { get; set; } = string.Empty;
        public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
    }

    public class PermissionDto
    {
        public string Code { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsAssigned { get; set; }
    }
}
