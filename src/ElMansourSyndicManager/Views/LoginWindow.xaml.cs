using ElMansourSyndicManager.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection; // Add this using directive

namespace ElMansourSyndicManager.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;

    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        
        _viewModel.LoginSuccessful += ViewModel_LoginSuccessful;
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            _viewModel.Code = passwordBox.Password;
        }
    }

    private void ViewModel_LoginSuccessful(object? sender, EventArgs e)
    {
        // Open main window
        var mainWindow = App.Services?.GetRequiredService<MainWindow>() ?? throw new InvalidOperationException("Service provider not initialized");
        mainWindow.Show();
        
        // Close login window
        this.Close();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (DataContext is LoginViewModel vm && vm.LoginCommand.CanExecute(null))
                vm.LoginCommand.Execute(null);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}
