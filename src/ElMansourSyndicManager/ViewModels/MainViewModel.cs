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

namespace ElMansourSyndicManager.ViewModels;

/// <summary>
/// Main ViewModel for the application shell
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authService;
    private readonly INavigationService _navigationService;
    private readonly IServiceProvider _serviceProvider;
    private object _currentView;
    private UserDto? _currentUser;
    private NavigationItem? _selectedNavigationItem;

    public MainViewModel(
        IAuthenticationService authService,
        INavigationService navigationService,
        IServiceProvider serviceProvider,
        DashboardViewModel dashboardViewModel)
    {
        _authService = authService;
        _navigationService = navigationService;
        _serviceProvider = serviceProvider;
        
        // Initialize with dashboard
        var dashboardView = _serviceProvider.GetRequiredService<DashboardView>();
        dashboardView.DataContext = dashboardViewModel;
        _currentView = dashboardView;
        
        // Call InitializeAsync for the initial DashboardViewModel
        if (dashboardViewModel is IInitializable initializableDashboardViewModel)
        {
            initializableDashboardViewModel.InitializeAsync();
        }
        
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

        // Set default selection
        SelectedNavigationItem = NavigationItems.FirstOrDefault();

        LogoutCommand = new RelayCommand(Logout);
        MarkAllAsReadCommand = new RelayCommand(MarkAllAsRead);
        
        // Load current user
        LoadCurrentUser();
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

    private async void LoadCurrentUser()
    {
        CurrentUser = await _authService.GetCurrentUserAsync();
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
        InitializeNotifications();
    }

    private void InitializeNotifications()
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
        // Create view based on ViewModel type
        object? view = viewModelType.Name switch
        {
            nameof(DashboardViewModel) => _serviceProvider.GetRequiredService<DashboardView>(),
            nameof(PaymentsViewModel) => _serviceProvider.GetRequiredService<PaymentsView>(),
            nameof(ReceiptsViewModel) => _serviceProvider.GetRequiredService<ReceiptsView>(),
            nameof(ExpensesViewModel) => _serviceProvider.GetRequiredService<ExpensesView>(),
            nameof(UsersViewModel) => _serviceProvider.GetRequiredService<UsersView>(),
            nameof(DocumentsViewModel) => _serviceProvider.GetRequiredService<DocumentsView>(),
            nameof(ReportsViewModel) => _serviceProvider.GetRequiredService<ReportsView>(),
            nameof(AuditViewModel) => _serviceProvider.GetRequiredService<AuditView>(),
            nameof(SettingsViewModel) => _serviceProvider.GetRequiredService<SettingsView>(),
            _ => null
        };

        if (view != null)
        {
            // Set ViewModel as DataContext
            var viewModel = _serviceProvider.GetRequiredService(viewModelType);
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
        await _authService.LogoutAsync();
        // Show login window
        var loginWindow = new LoginWindow(App.Services?.GetRequiredService<LoginViewModel>() ?? throw new InvalidOperationException("Service provider not initialized"));
        loginWindow.Show();
        
        // Close main window
        Application.Current.Windows.OfType<MainWindow>().FirstOrDefault()?.Close();
    }
}

public class NavigationItem
{
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public Type ViewModelType { get; set; } = null!;
    public bool RequiresAdmin { get; set; }
}
