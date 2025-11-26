namespace ElMansourSyndicManager.Core.Domain.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }
    public string? UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? RelatedEntityType { get; set; }
    public string? RelatedEntityId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string Priority { get; set; } = "Normal";
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
