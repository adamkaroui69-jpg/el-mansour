namespace ElMansourSyndicManager.Views;

public partial class ReportsView : System.Windows.Controls.UserControl
{
    public ReportsView()
    {
        InitializeComponent();
        this.Loaded += ReportsView_Loaded;
    }

    private void ReportsView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.ReportsViewModel viewModel)
        {
            viewModel.LoadHistoryCommand.Execute(null);
        }
    }
}
