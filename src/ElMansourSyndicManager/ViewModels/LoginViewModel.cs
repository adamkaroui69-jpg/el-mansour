using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.ViewModels.Base;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;

namespace ElMansourSyndicManager.ViewModels;

/// <summary>
/// ViewModel for the login page
/// </summary>
public class LoginViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authService;
    private string _houseCode = string.Empty;
    private string _code = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading;

    public LoginViewModel(IAuthenticationService authService)
    {
        _authService = authService;
        LoginCommand = new RelayCommand(async () => await LoginAsync(), () => !IsLoading && CanLogin);
    }

    public string HouseCode
    {
        get => _houseCode;
        set
        {
            Debug.WriteLine($"HouseCode set to: {value}");
            if (SetProperty(ref _houseCode, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public string Code
    {
        get => _code;
        set
        {
            Debug.WriteLine($"Code set to: {value}");
            if (SetProperty(ref _code, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            SetProperty(ref _isLoading, value);
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public bool CanLogin => !string.IsNullOrWhiteSpace(HouseCode) &&
                           !string.IsNullOrWhiteSpace(Code) &&
                           Code.Length == 6;

    public ICommand LoginCommand { get; }

    public event EventHandler? LoginSuccessful;

    private async Task LoginAsync()
    {
        Debug.WriteLine($"LoginAsync called. HouseCode: {HouseCode}, Code: {Code}");
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var task = _authService.AuthenticateAsync(HouseCode, Code);
            if (await Task.WhenAny(task, Task.Delay(5000)) == task)
            {
                if (task.Result.Success && task.Result.User != null)
                {
                    LoginSuccessful?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ErrorMessage = task.Result.ErrorMessage ?? "Code maison ou mot de passe incorrect.";
                }
            }
            else
            {
                ErrorMessage = "La connexion a expiré.";
            }
        }
        catch (Exception ex)
        {
            var message = ex.Message;
            var inner = ex.InnerException;
            while (inner != null)
            {
                message += $" -> {inner.Message}";
                inner = inner.InnerException;
            }
            ErrorMessage = $"Erreur lors de la connexion: {message}";
            Debug.WriteLine($"Login error: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
