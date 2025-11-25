namespace ElMansourSyndicManager.Views;

public partial class UsersView : System.Windows.Controls.UserControl
{
    public UsersView()
    {
        InitializeComponent();
        this.Loaded += UsersView_Loaded;
    }

    private void UsersView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.UsersViewModel viewModel)
        {
            viewModel.LoadUsersCommand.Execute(null);
        }
    }
}

