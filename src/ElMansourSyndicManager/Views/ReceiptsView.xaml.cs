namespace ElMansourSyndicManager.Views;

public partial class ReceiptsView : System.Windows.Controls.UserControl
{
    public ReceiptsView()
    {
        InitializeComponent();
    }

    private void ReceiptsDataGrid_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.DataGrid grid)
        {
            // Run after layout is complete to avoid InvalidOperationException
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle, new Action(() =>
            {
                grid.UpdateLayout();
            }));
        }
    }

    private void ActionButtons_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.StackPanel panel)
        {
            // Run after layout is complete to avoid InvalidOperationException
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle, new Action(() =>
            {
                panel.UpdateLayout();
                panel.InvalidateVisual();
                
                // Force redraw of children
                foreach (var child in panel.Children)
                {
                    if (child is System.Windows.UIElement element)
                    {
                        element.InvalidateVisual();
                    }
                }
            }));
        }
    }
}

