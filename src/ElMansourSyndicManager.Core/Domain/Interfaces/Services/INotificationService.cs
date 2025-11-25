using ElMansourSyndicManager.Core.Domain.DTOs;

namespace ElMansourSyndicManager.Core.Domain.Interfaces.Services;

/// <summary>
/// Service for managing notifications and Windows Toast Notifications
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Creates a new notification
    /// </summary>
    Task<NotificationDTO> CreateNotificationAsync(NotificationDTO notification, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets user notifications
    /// </summary>
    Task<List<NotificationDTO>> GetUserNotificationsAsync(string? userId = null, bool unreadOnly = false, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Marks a notification as read
    /// </summary>
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Marks all notifications as read for a user
    /// </summary>
    Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets unread notification count
    /// </summary>
    Task<int> GetUnreadCountAsync(string? userId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generates notifications for unpaid houses
    /// </summary>
    Task GenerateUnpaidHouseNotificationsAsync(string month, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sends a payment-related notification (unpaid house)
    /// </summary>
    Task SendPaymentNotificationAsync(PaymentDto payment, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a notification
    /// </summary>
    Task DeleteNotificationAsync(Guid notificationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets notifications by type
    /// </summary>
    Task<List<NotificationDTO>> GetNotificationsByTypeAsync(string type, string? userId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clears expired notifications
    /// </summary>
    Task ClearExpiredNotificationsAsync(CancellationToken cancellationToken = default);
}
