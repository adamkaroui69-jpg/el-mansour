using System.Windows.Controls;

namespace ElMansourSyndicManager.Services.Navigation
{
    public interface INavigationService
    {
        UserControl CurrentView { get; }
        void NavigateTo<T>() where T : UserControl;
        void GoBack();
    }
}
