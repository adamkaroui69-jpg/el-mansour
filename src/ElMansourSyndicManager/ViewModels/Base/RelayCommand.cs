using System.Windows.Input;

namespace ElMansourSyndicManager.ViewModels.Base;

/// <summary>
/// Relay command implementation for ICommand
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action? _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute?.Invoke();
}

public class RelayCommand<T> : ICommand
{
    private readonly Action<T>? _execute;
    private readonly Func<T, bool>? _canExecute;

    public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object? parameter)
    {
        if (_canExecute == null) return true;
        if (parameter == null) return _canExecute.Invoke(default!);
        
        if (parameter is T t)
        {
            return _canExecute.Invoke(t);
        }
        
        return false;
    }

    public void Execute(object? parameter)
    {
        if (_execute == null) return;
        if (parameter == null) 
        {
            _execute.Invoke(default!);
            return;
        }

        if (parameter is T t)
        {
            _execute.Invoke(t);
        }
    }
}
