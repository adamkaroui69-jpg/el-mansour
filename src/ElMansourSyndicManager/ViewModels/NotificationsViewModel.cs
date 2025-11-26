using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ElMansourSyndicManager.ViewModels;

/// <summary>
/// ViewModel for the notifications page
/// </summary>
public class NotificationsViewModel : ViewModelBase
{
    private readonly INotificationService _notificationService;
    private readonly IAuthenticationService _authService;
    private NotificationDto? _selectedNotification;
    private string _selectedFilter = "All";
    private string _selectedTypeFilter = "All";
    private bool _isLoading;
    private int _unreadCount;

    public NotificationsViewModel(
        INotificationService notificationService,
        IAuthenticationService authService)
    {
        _notificationService = notificationService;
        _authService = authService;

        Notifications = new ObservableCollection<NotificationDto>();
        FilteredNotifications = new ObservableCollection<NotificationDto>();

        RefreshCommand = new RelayCommand(async () => await LoadNotificationsAsync());
        MarkReadCommand = new RelayCommand<NotificationDto>(async n => await MarkAsReadAsync(n), n => n != null && !n.IsRead);
        MarkAllReadCommand = new RelayCommand(async () => await MarkAllAsReadAsync(), () => UnreadCount > 0);
        DeleteCommand = new RelayCommand<NotificationDto>(async n => await DeleteNotificationAsync(n), n => n != null);
        OpenRelatedItemCommand = new RelayCommand<NotificationDto>(async n => await OpenRelatedItemAsync(n), n => n != null && !string.IsNullOrEmpty(n.RelatedEntityId));

        // Load initial data
        _ = LoadNotificationsAsync();
    }

    public ObservableCollection<NotificationDto> Notifications { get; }
    public ObservableCollection<NotificationDto> FilteredNotifications { get; }

    public NotificationDto? SelectedNotification
    {
        get => _selectedNotification;
        set => SetProperty(ref _selectedNotification, value);
    }

    public string SelectedFilter
    {
        get => _selectedFilter;
        set
        {
            if (SetProperty(ref _selectedFilter, value))
            {
                ApplyFilters();
            }
        }
    }

    public string SelectedTypeFilter
    {
        get => _selectedTypeFilter;
        set
        {
            if (SetProperty(ref _selectedTypeFilter, value))
            {
                ApplyFilters();
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public int UnreadCount
    {
        get => _unreadCount;
        set
        {
            if (SetProperty(ref _unreadCount, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public ICommand RefreshCommand { get; }
    public ICommand MarkReadCommand { get; }
    public ICommand MarkAllReadCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand OpenRelatedItemCommand { get; }

    private async Task LoadNotificationsAsync()
    {
        try
        {
            IsLoading = true;
            var notifications = await _notificationService.GetUserNotificationsAsync();
            
            Notifications.Clear();
            foreach (var notification in notifications)
            {
                Notifications.Add(notification);
            }

            UnreadCount = await _notificationService.GetUnreadCountAsync();
            ApplyFilters();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur lors du chargement des notifications: {ex.Message}",
                "Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task MarkAsReadAsync(NotificationDto notification)
    {
        try
        {
            await _notificationService.MarkAsReadAsync(notification.Id);
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            UnreadCount = await _notificationService.GetUnreadCountAsync();
            ApplyFilters();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur lors du marquage de la notification: {ex.Message}",
                "Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async Task MarkAllAsReadAsync()
    {
        try
        {
            var userId = _authService.CurrentUser?.Id.ToString();
            if (userId != null)
            {
                await _notificationService.MarkAllAsReadAsync(userId);
                await LoadNotificationsAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur lors du marquage de toutes les notifications: {ex.Message}",
                "Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async Task DeleteNotificationAsync(NotificationDto notification)
    {
        try
        {
            var result = MessageBox.Show(
                "Êtes-vous sûr de vouloir supprimer cette notification?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _notificationService.DeleteNotificationAsync(notification.Id);
                Notifications.Remove(notification);
                ApplyFilters();
                UnreadCount = await _notificationService.GetUnreadCountAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur lors de la suppression: {ex.Message}",
                "Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async Task OpenRelatedItemAsync(NotificationDto notification)
    {
        try
        {
            // Mark as read when opening
            if (!notification.IsRead)
            {
                await MarkAsReadAsync(notification);
            }

            // Navigate to related item based on type
            // This would typically use a navigation service
            // For now, we'll just show a message
            var message = notification.RelatedEntityType switch
            {
                "Payment" => $"Ouvrir le paiement {notification.RelatedEntityId}",
                "Maintenance" => $"Ouvrir la tâche de maintenance {notification.RelatedEntityId}",
                _ => $"Ouvrir {notification.RelatedEntityType} {notification.RelatedEntityId}"
            };

            MessageBox.Show(
                message,
                "Navigation",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // TODO: Implement actual navigation using INavigationService
            // _navigationService.NavigateTo(notification.RelatedEntityType, notification.RelatedEntityId);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur lors de l'ouverture: {ex.Message}",
                "Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ApplyFilters()
    {
        FilteredNotifications.Clear();

        var filtered = Notifications.AsEnumerable();

        // Filter by read/unread
        if (SelectedFilter == "Unread")
        {
            filtered = filtered.Where(n => !n.IsRead);
        }
        else if (SelectedFilter == "Read")
        {
            filtered = filtered.Where(n => n.IsRead);
        }

        // Filter by type
        if (SelectedTypeFilter != "All")
        {
            filtered = filtered.Where(n => n.Type == SelectedTypeFilter);
        }

        foreach (var notification in filtered.OrderByDescending(n => n.CreatedAt))
        {
            FilteredNotifications.Add(notification);
        }
    }

    public string GetNotificationIcon(NotificationDto notification)
    {
        return notification.Type switch
        {
            "UnpaidHouse" => "CashAlert",
            "MaintenanceDue" => "Tools",
            "System" => "Information",
            "Info" => "Information",
            _ => "Bell"
        };
    }

    public string GetPriorityColor(NotificationDto notification)
    {
        return notification.Priority switch
        {
            "Urgent" => "#FF0000",
            "High" => "#FF8800",
            "Normal" => "#2196F3",
            "Low" => "#4CAF50",
            _ => "#757575"
        };
    }
}
