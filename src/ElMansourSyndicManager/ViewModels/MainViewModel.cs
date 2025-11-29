using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.Services;
using ElMansourSyndicManager.Services.Navigation;
using ElMansourSyndicManager.ViewModels.Base;
using ElMansourSyndicManager.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using AutoUpdaterDotNET;
using ElMansourSyndicManager.Models;
using Microsoft.Extensions.Logging;

namespace ElMansourSyndicManager.ViewModels;

/// <summary>
/// Main ViewModel for the application shell
/// </summary>
/// <summary>
/// Main ViewModel for the application shell
/// </summary>
public class MainViewModel : ViewModelBase, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MainViewModel> _logger;
    private IServiceScope? _currentScope;
    
    private object _currentView;
    private UserDto? _currentUser;
    private NavigationItem? _selectedNavigationItem;

    public MainViewModel(
        IServiceScopeFactory scopeFactory,
        ILogger<MainViewModel> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        
        // Navigation items
        NavigationItems = new ObservableCollection<NavigationItem>
        {
            new() { Title = "Tableau de bord", Icon = "ViewDashboard", ViewModelType = typeof(DashboardViewModel) },
            new() { Title = "Paiements", Icon = "Cash", ViewModelType = typeof(PaymentsViewModel) },
            new() { Title = "Reçus", Icon = "Receipt", ViewModelType = typeof(ReceiptsViewModel) },
            new() { Title = "Dépenses", Icon = "CurrencyUsd", ViewModelType = typeof(ExpensesViewModel) },
            new() { Title = "Utilisateurs", Icon = "Account", ViewModelType = typeof(UsersViewModel), RequiresAdmin = true },
            new() { Title = "Documents", Icon = "FileDocument", ViewModelType = typeof(DocumentsViewModel) },
            new() { Title = "Rapports", Icon = "ChartBar", ViewModelType = typeof(ReportsViewModel) },
            new() { Title = "Audit", Icon = "History", ViewModelType = typeof(AuditViewModel), RequiresAdmin = true },
            new() { Title = "Paramètres", Icon = "Cog", ViewModelType = typeof(SettingsViewModel), RequiresAdmin = true }
        };

        LogoutCommand = new RelayCommand(Logout);
        MarkAllAsReadCommand = new RelayCommand(MarkAllAsRead);

        // Set default selection (will trigger NavigateTo)
        SelectedNavigationItem = NavigationItems.FirstOrDefault();
    }

    public async Task InitializeAsync()
    {
        await LoadCurrentUser();
        
        // Initialize the current view model if needed
        // Note: NavigateTo might have already set CurrentView, but we ensure it's initialized here too if needed
        if (_currentView is FrameworkElement view && view.DataContext is IInitializable initializableViewModel)
        {
            await initializableViewModel.InitializeAsync();
        }
    }

    public ObservableCollection<NavigationItem> NavigationItems { get; }

    public NavigationItem? SelectedNavigationItem
    {
        get => _selectedNavigationItem;
        set
        {
            if (SetProperty(ref _selectedNavigationItem, value) && value != null)
            {
                NavigateTo(value.ViewModelType);
            }
        }
    }

    public object CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public UserDto? CurrentUser
    {
        get => _currentUser;
        set => SetProperty(ref _currentUser, value);
    }

    public ICommand LogoutCommand { get; }

    public ObservableCollection<Notification> Notifications { get; } = new ObservableCollection<Notification>();

    public bool HasNotifications => Notifications.Any(n => !n.IsRead);

    public int NotificationCount => Notifications.Count(n => !n.IsRead);

    public ICommand MarkAllAsReadCommand { get; }

    private async Task InitializeNotifications()
    {
        // Subscribe to AutoUpdater events
        AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
        
        // Add welcome notification
        AddNotification(new Notification
        {
            Title = "Bienvenue",
            Message = $"Bienvenue dans El Mansour Syndic Manager, {CurrentUser?.Username} !",
            Icon = "HandWave",
            TypeColor = "#4CAF50"
        });

        // Load notifications from database using a dedicated scope
        await ExecuteSafelyAsync(async () =>
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                // Generate notifications for unpaid houses first
                var currentMonth = DateTime.Now.ToString("yyyy-MM");
                await notificationService.GenerateUnpaidHouseNotificationsAsync(currentMonth);

                var notifications = await notificationService.GetUserNotificationsAsync(CurrentUser?.Id.ToString());
                
                foreach (var notifDto in notifications)
                {
                    // Check if already added (to avoid duplicates with welcome message)
                    if (!Notifications.Any(n => n.Title == notifDto.Title && n.Message == notifDto.Message))
                    {
                        AddNotification(new Notification
                        {
                            Title = notifDto.Title,
                            Message = notifDto.Message,
                            Icon = GetIconForType(notifDto.Type),
                            TypeColor = GetColorForType(notifDto.Type),
                            IsRead = notifDto.IsRead
                        });
                    }
                }
            }
        }, "Erreur lors du chargement des notifications", _logger);
    }

    private string GetIconForType(string type)
    {
        return type switch
        {
            "UnpaidHouse" => "AlertCircle",
            "PaymentDue" => "CashRemove",
            "MaintenanceDue" => "Tools",
            "Info" => "Information",
            _ => "Bell"
        };
    }

    private string GetColorForType(string type)
    {
        return type switch
        {
            "UnpaidHouse" => "#F44336", // Red
            "PaymentDue" => "#FF9800", // Orange
            "MaintenanceDue" => "#2196F3", // Blue
            "Info" => "#4CAF50", // Green
            _ => "#757575" // Grey
        };
    }

    private async Task LoadCurrentUser()
    {
        await ExecuteSafelyAsync(async () =>
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
                CurrentUser = await authService.GetCurrentUserAsync();
            }

            // Filter navigation items based on role
            if (CurrentUser?.Role != "Admin")
            {
                var itemsToRemove = NavigationItems.Where(item => item.RequiresAdmin).ToList();
                foreach (var item in itemsToRemove)
                {
                    NavigationItems.Remove(item);
                }
            }
            
            // Initialize notifications
            await InitializeNotifications();
        }, "Erreur lors du chargement de l'utilisateur", _logger);
    }


    private void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
    {
        if (args.Error == null)
        {
            if (args.IsUpdateAvailable)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AddNotification(new Notification
                    {
                        Title = "Mise à jour disponible",
                        Message = $"Une nouvelle version ({args.CurrentVersion}) est disponible. Cliquez pour mettre à jour.",
                        Icon = "CloudDownload",
                        TypeColor = "#2196F3"
                    });
                });
            }
        }
    }

    public void AddNotification(Notification notification)
    {
        Notifications.Insert(0, notification);
        OnPropertyChanged(nameof(HasNotifications));
        OnPropertyChanged(nameof(NotificationCount));
    }

    private void MarkAllAsRead()
    {
        foreach (var notification in Notifications)
        {
            notification.IsRead = true;
        }
        // Refresh list to update UI
        var temp = Notifications.ToList();
        Notifications.Clear();
        foreach (var item in temp) Notifications.Add(item);
        
        OnPropertyChanged(nameof(HasNotifications));
        OnPropertyChanged(nameof(NotificationCount));
    }

    private async void NavigateTo(Type viewModelType)
    {
        // Dispose previous scope
        _currentScope?.Dispose();
        _currentScope = null;

        // Create new scope for the new view
        _currentScope = _scopeFactory.CreateScope();
        var scopedProvider = _currentScope.ServiceProvider;

        // Create view based on ViewModel type
        // We resolve the View from the scoped provider so it (and its VM) gets the scoped services
        object? view = viewModelType.Name switch
        {
            nameof(DashboardViewModel) => scopedProvider.GetRequiredService<DashboardView>(),
            nameof(PaymentsViewModel) => scopedProvider.GetRequiredService<PaymentsView>(),
            nameof(ReceiptsViewModel) => scopedProvider.GetRequiredService<ReceiptsView>(),
            nameof(ExpensesViewModel) => scopedProvider.GetRequiredService<ExpensesView>(),
            nameof(UsersViewModel) => scopedProvider.GetRequiredService<UsersView>(),
            nameof(DocumentsViewModel) => scopedProvider.GetRequiredService<DocumentsView>(),
            nameof(ReportsViewModel) => scopedProvider.GetRequiredService<ReportsView>(),
            nameof(AuditViewModel) => scopedProvider.GetRequiredService<AuditView>(),
            nameof(SettingsViewModel) => scopedProvider.GetRequiredService<SettingsView>(),
            _ => null
        };

        if (view != null)
        {
            // Set ViewModel as DataContext
            // We also resolve the ViewModel from the SAME scoped provider
            var viewModel = scopedProvider.GetRequiredService(viewModelType);
            
            if (view is FrameworkElement frameworkElement)
            {
                frameworkElement.DataContext = viewModel;
            }
            CurrentView = view;

            // Call InitializeAsync if ViewModel implements IInitializable
            if (viewModel is IInitializable initializableViewModel)
            {
                await initializableViewModel.InitializeAsync();
            }
        }
    }

    private async void Logout()
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
            await authService.LogoutAsync();
        }

        // Show login window
        // We need to resolve LoginWindow from a fresh scope or root, but LoginWindow is transient.
        // We can use the root services for the LoginWindow as it will create its own VM.
        // Accessing App.Services is a bit dirty but works for top-level window switching.
        var loginWindow = new LoginWindow(App.Services?.GetRequiredService<LoginViewModel>() ?? throw new InvalidOperationException("Service provider not initialized"));
        loginWindow.Show();
        
        // Close main window
        Application.Current.Windows.OfType<MainWindow>().FirstOrDefault()?.Close();
    }

    public void Dispose()
    {
        _currentScope?.Dispose();
    }
}

public class NavigationItem
{
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public Type ViewModelType { get; set; } = null!;
    public bool RequiresAdmin { get; set; }
}
