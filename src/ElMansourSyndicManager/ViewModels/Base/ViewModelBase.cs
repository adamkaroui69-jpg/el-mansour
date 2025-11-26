using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Logging;

namespace ElMansourSyndicManager.ViewModels.Base;

/// <summary>
/// Base class for all ViewModels implementing INotifyPropertyChanged
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        return true;
    }

    protected virtual void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        OnPropertyChanged(propertyName);
    }

    /// <summary>
    /// Executes an async action safely with error handling
    /// </summary>
    protected async Task ExecuteSafelyAsync(Func<Task> action, string errorMessage = "Une erreur est survenue", ILogger? logger = null)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, errorMessage);
            MessageBox.Show($"{errorMessage}\n\nDétails: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
