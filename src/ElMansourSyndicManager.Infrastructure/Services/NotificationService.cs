using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Exceptions;
using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.Core.Domain.Entities;
using Microsoft.Extensions.Logging;
// Removed Microsoft.Toolkit.Uwp.Notifications and System.Runtime.InteropServices as toast notifications are disabled
// using Microsoft.Toolkit.Uwp.Notifications;
// using System.Runtime.InteropServices;

namespace ElMansourSyndicManager.Infrastructure.Services;

/// <summary>
/// Service for managing notifications with Windows Toast Notifications support
/// </summary>
public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IPaymentService _paymentService;
    // Removed IMaintenanceService as its interface was removed
    private readonly IUserRepository _userRepository;
    private readonly IAuditService _auditService;
    private readonly IAuthenticationService _authService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        IPaymentService paymentService,
        // Removed IMaintenanceService as its interface was removed
        IUserRepository userRepository,
        IAuditService auditService,
        IAuthenticationService authService,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _paymentService = paymentService;
        // Removed _maintenanceService = maintenanceService;
        _userRepository = userRepository;
        _auditService = auditService;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new notification
    /// </summary>
        public async Task<NotificationDTO> CreateNotificationAsync(
            NotificationDTO notificationDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = new Notification
                {
                    UserId = notificationDto.UserId,
                    Type = notificationDto.Type,
                    Title = notificationDto.Title,
                    Message = notificationDto.Message,
                    RelatedEntityType = notificationDto.RelatedEntityType,
                    RelatedEntityId = notificationDto.RelatedEntityId,
                    IsRead = notificationDto.IsRead,
                    ReadAt = notificationDto.ReadAt,
                    Priority = notificationDto.Priority,
                    ExpiresAt = notificationDto.ExpiresAt,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _notificationRepository.CreateAsync(entity, cancellationToken);
                await _notificationRepository.SaveChangesAsync(cancellationToken);

                // Log audit
                await _auditService.LogActivityAsync(new AuditLogDto
                {
                    UserId = _authService.CurrentUser?.Id.ToString(),
                    Action = "Create",
                    EntityType = "Notification",
                    EntityId = entity.Id.ToString(), // Utilise l'ID de l'entité créée
                    Details = $"{{\"type\":\"{notificationDto.Type}\",\"title\":\"{notificationDto.Title}\"}}"
                }, cancellationToken);

                _logger.LogInformation("Notification {NotificationId} created for user {UserId}", entity.Id, notificationDto.UserId);

                return MapToDto(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                throw;
            }
        }

    /// <summary>
    /// Gets user notifications
    /// </summary>
        public async Task<List<NotificationDTO>> GetUserNotificationsAsync(
            string? userId = null,
            bool unreadOnly = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // The repository methods now return IEnumerable<T> and do not take CancellationToken
                var allNotifications = await _notificationRepository.GetAllAsync(cancellationToken);
                
                var notifications = allNotifications
                    .Where(n => (!unreadOnly || !n.IsRead) && (userId == null || n.UserId == userId))
                .OrderByDescending(n => n.CreatedAt)
                    .ToList();

                return notifications.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user notifications");
                throw;
            }
        }

    /// <summary>
    /// Marks a notification as read
    /// </summary>
        public async Task MarkAsReadAsync(
            Guid notificationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
                if (notification == null)
                    throw new NotFoundException("Notification", notificationId.ToString());

                if (!notification.IsRead)
                {
                    notification.IsRead = true;
                    notification.UpdatedAt = DateTime.UtcNow;
                    await _notificationRepository.UpdateAsync(notification, cancellationToken);
                    await _notificationRepository.SaveChangesAsync(cancellationToken);

                    // Log audit
                    await _auditService.LogActivityAsync(new AuditLogDto
                    {
                        UserId = _authService.CurrentUser?.Id.ToString(),
                        Action = "Update",
                        EntityType = "Notification",
                        EntityId = notification.Id.ToString(),
                    Details = "{\"action\":\"MarkAsRead\"}"
                }, cancellationToken);

                _logger.LogInformation("Notification {NotificationId} marked as read", notificationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read");
            throw;
        }
    }

    /// <summary>
    /// Marks all notifications as read for a user
    /// </summary>
        public async Task MarkAllAsReadAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Assuming GetByUserIdAsync is still needed, but it's not in INotificationRepository
                // For now, I'll use GetAllAsync and filter. This needs to be refined if GetByUserIdAsync is added back.
                var notifications = await _notificationRepository.GetAllAsync(cancellationToken);
                var unreadNotifications = notifications.Where(n => !n.IsRead && (userId == null || n.UserId == userId)).ToList();

                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.UpdatedAt = DateTime.UtcNow;
                    await _notificationRepository.UpdateAsync(notification, cancellationToken);
                }
                await _notificationRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Marked {Count} notifications as read for user {UserId}", unreadNotifications.Count, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            throw;
        }
    }

    /// <summary>
    /// Gets unread notification count
    /// </summary>
        public async Task<int> GetUnreadCountAsync(
            string? userId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Assuming GetByUserIdAsync is still needed, but it's not in INotificationRepository
                // For now, I'll use GetAllAsync and filter. This needs to be refined if GetByUserIdAsync is added back.
                var notifications = await _notificationRepository.GetAllAsync(cancellationToken);
                return notifications.Count(n => !n.IsRead && (userId == null || n.UserId == userId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                throw;
            }
        }

    /// <summary>
    /// Generates notifications for unpaid houses
    /// </summary>
        public async Task GenerateUnpaidHouseNotificationsAsync(
            string month,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var unpaidHouses = await _paymentService.GetUnpaidHousesAsync(month, cancellationToken);
                
                foreach (var house in unpaidHouses)
                {
                    var notification = new NotificationDTO
                    {
                        UserId = null, // Ou un ID d'utilisateur spécifique si applicable
                        Type = "UnpaidHouse",
                        Title = "Maison Non Payée",
                        Message = $"La maison {house.HouseCode} n'a pas encore payé pour {FormatMonth(month)}. Montant: {house.MonthlyAmount:N2} TND. Jours de retard: {house.DaysOverdue}",
                        RelatedEntityType = "House",
                        RelatedEntityId = house.HouseCode,
                        Priority = house.DaysOverdue > 30 ? "Urgent" : house.DaysOverdue > 15 ? "High" : "Normal"
                    };

                    await CreateNotificationAsync(notification, cancellationToken);
                }

            _logger.LogInformation("Generated {Count} unpaid house notifications for {Month}", unpaidHouses.Count, month);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating unpaid house notifications");
            throw;
        }
    }

    /// <summary>
    /// Sends a payment-related notification
    /// </summary>
        public async Task SendPaymentNotificationAsync(
            PaymentDto payment,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var notification = new NotificationDTO
                {
                    UserId = null, // Ou un ID d'utilisateur spécifique si applicable
                    Type = "PaymentDue",
                    Title = "Paiement en Retard",
                    Message = $"Le paiement pour la maison {payment.HouseCode} est en retard. Montant: {payment.Amount:N2} TND",
                    RelatedEntityType = "Payment",
                    RelatedEntityId = payment.Id.ToString(),
                    Priority = "High"
                };

                await CreateNotificationAsync(notification, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment notification");
                throw;
            }
        }

    /// <summary>
    /// Sends a maintenance task notification
    /// </summary>
    // Removed SendMaintenanceNotificationAsync as IMaintenanceService was removed
    /*
    public async Task SendMaintenanceNotificationAsync(
        MaintenanceDTO maintenance,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string priority = maintenance.Priority;
            string title = maintenance.Status switch
            {
                "Pending" => "Nouvelle Tâche de Maintenance",
                "InProgress" => "Maintenance en Cours",
                "Completed" => "Maintenance Terminée",
                _ => "Notification de Maintenance"
            };

            var notification = new NotificationDTO
            {
                UserId = maintenance.AssignedTo,
                Type = "MaintenanceDue",
                Title = title,
                Message = $"{maintenance.Description} - Statut: {maintenance.Status}",
                RelatedEntityType = "Maintenance",
                RelatedEntityId = maintenance.Id.ToString(),
                Priority = priority
            };

            await CreateNotificationAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending maintenance notification");
            throw;
        }
    }
    */

    /// <summary>
    /// Sends a document notification
    /// </summary>
    // Removed SendDocumentNotificationAsync as IDocumentService was removed
    /*
    public async Task SendDocumentNotificationAsync(
        string documentType, 
        string documentName, 
        string? relatedEntityId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = new NotificationDTO
            {
                UserId = null, // Send to all users
                Type = "Info",
                Title = "Nouveau Document",
                Message = $"Un nouveau document a été ajouté: {documentName}",
                RelatedEntityType = documentType,
                RelatedEntityId = relatedEntityId,
                Priority = "Normal"
            };

            await CreateNotificationAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending document notification");
            throw;
        }
    }
    */

    /// <summary>
    /// Deletes a notification
    /// </summary>
        public async Task DeleteNotificationAsync(
            Guid notificationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
                if (notification == null)
                    throw new NotFoundException("Notification", notificationId.ToString());

                await _notificationRepository.DeleteAsync(notification, cancellationToken);
                await _notificationRepository.SaveChangesAsync(cancellationToken);

                // Log audit
                await _auditService.LogActivityAsync(new AuditLogDto
                {
                    UserId = _authService.CurrentUser?.Id.ToString(),
                    Action = "Delete",
                    EntityType = "Notification",
                    EntityId = notification.Id.ToString(),
                    Details = "{\"action\":\"DeleteNotification\"}"
                }, cancellationToken);

            _logger.LogInformation("Notification {NotificationId} deleted", notificationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification");
            throw;
        }
    }

    /// <summary>
    /// Shows a Windows Toast Notification
    /// </summary>
    // Temporarily commented out due to API changes in Microsoft.Toolkit.Uwp.Notifications
    /*
    public async Task ShowToastNotificationAsync(
        NotificationDTO notification,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if running on Windows
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _logger.LogWarning("Toast notifications are only supported on Windows");
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    var toastBuilder = new ToastContentBuilder();

                    // Set title and message
                    toastBuilder.AddText(notification.Title)
                               .AddText(notification.Message);

                    // Set priority/urgency
                    if (notification.Priority == "Urgent" || notification.Priority == "High")
                    {
                        toastBuilder.SetToastScenario(ToastScenario.Reminder);
                    }

                    // Add action buttons based on notification type
                    if (!string.IsNullOrEmpty(notification.RelatedEntityId))
                    {
                        var action = notification.Type switch
                        {
                            "UnpaidHouse" => $"view-payment-{notification.RelatedEntityId}",
                            "MaintenanceDue" => $"view-maintenance-{notification.RelatedEntityId}",
                            _ => $"view-{notification.RelatedEntityType?.ToLower()}-{notification.RelatedEntityId}"
                        };

                        toastBuilder.AddButton(new ToastButton()
                            .SetContent("Ouvrir")
                            .AddArgument("action", action)
                            .SetActivationType(ToastActivationType.Foreground));
                    }

                    // Show the toast
                    toastBuilder.Show();

                    _logger.LogInformation("Toast notification shown: {Title}", notification.Title);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to show toast notification");
                    // Don't throw - toast failure shouldn't break the app
                }
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing toast notification");
            // Don't throw - toast failure shouldn't break the app
        }
    }
    */

    /// <summary>
    /// Gets notifications by type
    /// </summary>
        public async Task<List<NotificationDTO>> GetNotificationsByTypeAsync(
            string type,
            string? userId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Assuming GetByUserIdAsync and filtering by type is still needed, but it's not in INotificationRepository
                // For now, I'll use GetAllAsync and filter. This needs to be refined if these methods are added back.
                var notifications = await _notificationRepository.GetAllAsync(cancellationToken);
                
                return notifications
                    .Where(n => n.Type == type && (userId == null || n.UserId == userId))
                    .OrderByDescending(n => n.CreatedAt)
                    .Select(MapToDto)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications by type");
                throw;
            }
        }

    /// <summary>
    /// Clears expired notifications
    /// </summary>
        public async Task ClearExpiredNotificationsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var allNotifications = await _notificationRepository.GetAllAsync(cancellationToken);
                var expired = allNotifications
                    .Where(n => n.ExpiresAt.HasValue && n.ExpiresAt < DateTime.UtcNow)
                    .ToList();

                foreach (var notification in expired)
                {
                    await _notificationRepository.DeleteAsync(notification, cancellationToken);
                }
                await _notificationRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Cleared {Count} expired notifications", expired.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing expired notifications");
            throw;
        }
    }

    #region Private Methods

    /// <summary>
    /// Maps Notification entity to DTO
    /// </summary>
    private NotificationDTO MapToDto(Notification notification)
    {
        return new NotificationDTO
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            RelatedEntityType = notification.RelatedEntityType,
            RelatedEntityId = notification.RelatedEntityId,
            IsRead = notification.IsRead,
            ReadAt = (DateTime?)notification.ReadAt,
            Priority = notification.Priority,
            ExpiresAt = notification.ExpiresAt,
            CreatedAt = notification.CreatedAt,
            UpdatedAt = notification.UpdatedAt ?? notification.CreatedAt
        };
    }

    /// <summary>
    /// Formats month string (YYYY-MM) to readable format
    /// </summary>
    private string FormatMonth(string month)
    {
        if (DateTime.TryParseExact($"{month}-01", "yyyy-MM-dd", null,
            System.Globalization.DateTimeStyles.None, out var date))
        {
            return date.ToString("MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"));
        }
        return month;
    }

    #endregion
}
