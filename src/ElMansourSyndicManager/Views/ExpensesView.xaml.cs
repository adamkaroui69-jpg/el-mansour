using ElMansourSyndicManager.ViewModels;

namespace ElMansourSyndicManager.Views;

public partial class ExpensesView : System.Windows.Controls.UserControl
{
    public ExpensesView()
    {
        InitializeComponent();
        Loaded += ExpensesView_Loaded;
    }

    private async void ExpensesView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ExpensesViewModel viewModel)
        {
            await viewModel.LoadExpensesAsync();
        }
    }
}

