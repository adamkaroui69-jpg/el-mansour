using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Controls;

namespace ElMansourSyndicManager.Services.Navigation
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private UserControl? _currentView;

        public UserControl CurrentView
        {
            get => _currentView ?? throw new InvalidOperationException("CurrentView is not set.");
            private set
            {
                _currentView = value;
                // Potentially add an event here to notify subscribers of view changes
            }
        }

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void NavigateTo<T>() where T : UserControl
        {
            CurrentView = _serviceProvider.GetRequiredService<T>();
        }

        public void GoBack()
        {
            // This is a basic implementation. For a more robust back navigation,
            // you would need to maintain a history stack of views.
            // For now, we'll just throw an exception or do nothing.
            throw new NotImplementedException("GoBack is not fully implemented yet. A history stack is required.");
        }
    }
}
