using System;

namespace ElMansourSyndicManager.Core.Domain.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } // Changed to nullable to match LastModifiedDate behavior
    }
}
