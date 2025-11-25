namespace ElMansourSyndicManager.Views;

public partial class DocumentsView : System.Windows.Controls.UserControl
{
    public DocumentsView()
    {
        InitializeComponent();
        this.Loaded += DocumentsView_Loaded;
    }

    private void DocumentsView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.DocumentsViewModel viewModel)
        {
            viewModel.LoadCommand.Execute(null);
        }
    }
}

