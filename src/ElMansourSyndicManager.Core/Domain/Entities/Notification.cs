namespace ElMansourSyndicManager.Core.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public string? UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public required string Message { get; set; }
        public string? RelatedEntityType { get; set; }
        public string? RelatedEntityId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public string Priority { get; set; } = "Normal";
        public DateTime? ExpiresAt { get; set; }
        // CreatedDate et LastModifiedDate sont héritées de BaseEntity
    }
}
