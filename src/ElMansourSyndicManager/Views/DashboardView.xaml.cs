using ElMansourSyndicManager.ViewModels;
using ElMansourSyndicManager.ViewModels.Base;

namespace ElMansourSyndicManager.Views;

public partial class DashboardView : System.Windows.Controls.UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        Loaded += DashboardView_Loaded;
    }

    private async void DashboardView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is IInitializable viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }
}

