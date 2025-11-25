namespace ElMansourSyndicManager.Views;

public partial class AuditView : System.Windows.Controls.UserControl
{
    public AuditView()
    {
        InitializeComponent();
        this.Loaded += AuditView_Loaded;
    }

    private void AuditView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.AuditViewModel viewModel)
        {
            viewModel.LoadCommand.Execute(null);
        }
    }
}

