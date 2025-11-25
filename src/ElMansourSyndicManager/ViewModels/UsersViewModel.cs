using System.Collections.ObjectModel;
using System.Windows.Input;
using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.ViewModels.Base;

namespace ElMansourSyndicManager.ViewModels;

public class UsersViewModel : ViewModelBase
{
    private readonly IUserService _userService;
    private readonly IAuthenticationService _authService;
    private ObservableCollection<UserDto> _users;
    private bool _isLoading;
    private string _errorMessage = string.Empty;
    private UserDto? _selectedUser;

    public ObservableCollection<UserDto> Users
    {
        get => _users;
        set => SetProperty(ref _users, value);
    }

    public UserDto? SelectedUser
    {
        get => _selectedUser;
        set => SetProperty(ref _selectedUser, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private bool _isFormVisible;
    private bool _isEditMode;
    private string _formTitle = string.Empty;
    
    // Form properties
    private string _formName = string.Empty;
    private string _formSurname = string.Empty;
    private string _formHouseCode = string.Empty;
    private string _formCode = string.Empty;
    private string _formRole = "SyndicMember";
    private bool _formIsActive;

    public bool IsFormVisible
    {
        get => _isFormVisible;
        set => SetProperty(ref _isFormVisible, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public string FormTitle
    {
        get => _formTitle;
        set => SetProperty(ref _formTitle, value);
    }

    public string FormName
    {
        get => _formName;
        set => SetProperty(ref _formName, value);
    }

    public string FormSurname
    {
        get => _formSurname;
        set => SetProperty(ref _formSurname, value);
    }

    public string FormHouseCode
    {
        get => _formHouseCode;
        set => SetProperty(ref _formHouseCode, value);
    }

    public string FormCode
    {
        get => _formCode;
        set => SetProperty(ref _formCode, value);
    }

    public string FormRole
    {
        get => _formRole;
        set => SetProperty(ref _formRole, value);
    }

    public bool FormIsActive
    {
        get => _formIsActive;
        set => SetProperty(ref _formIsActive, value);
    }
    
    public ObservableCollection<string> Roles { get; } = new ObservableCollection<string> { "Admin", "SyndicMember", "Trésorier" };

    public ICommand LoadUsersCommand { get; }
    public ICommand CreateUserCommand { get; }
    public ICommand UpdateUserCommand { get; }
    public ICommand DeleteUserCommand { get; }
    public ICommand ResetPasswordCommand { get; }
    public ICommand SaveUserCommand { get; }
    public ICommand CancelUserCommand { get; }

    // Constructor update
    public UsersViewModel(IUserService userService, IAuthenticationService authService)
    {
        _userService = userService;
        _authService = authService;
        _users = new ObservableCollection<UserDto>();
        
        LoadUsersCommand = new RelayCommand(async () => await LoadUsersAsync());
        CreateUserCommand = new RelayCommand(ShowCreateUserForm);
        UpdateUserCommand = new RelayCommand<UserDto>(ShowUpdateUserForm);
        DeleteUserCommand = new RelayCommand<UserDto>(async (u) => await DeleteUserAsync(u), (u) => IsAdmin);
        ResetPasswordCommand = new RelayCommand(async () => await ResetPasswordAsync(), () => SelectedUser != null);
        SaveUserCommand = new RelayCommand(async () => await SaveUserAsync());
        CancelUserCommand = new RelayCommand(HideForm);

        CheckAdminStatus();
    }

    private async void CheckAdminStatus()
    {
        var user = await _authService.GetCurrentUserAsync();
        IsAdmin = user?.Role == "Admin";
    }

    private bool _isAdmin;
    public bool IsAdmin
    {
        get => _isAdmin;
        set => SetProperty(ref _isAdmin, value);
    }

    public async Task LoadUsersAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var users = await _userService.GetAllUsersAsync();
            Users = new ObservableCollection<UserDto>(users);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur lors du chargement des utilisateurs: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ShowCreateUserForm()
    {
        IsEditMode = false;
        FormTitle = "Nouvel Utilisateur";
        FormName = string.Empty;
        FormSurname = string.Empty;
        FormHouseCode = string.Empty;
        FormCode = string.Empty;
        FormRole = "SyndicMember";
        FormIsActive = true;
        IsFormVisible = true;
        ErrorMessage = string.Empty;
    }

    private void ShowUpdateUserForm(UserDto? user)
    {
        if (user != null) SelectedUser = user;
        if (SelectedUser == null) return;

        IsEditMode = true;
        FormTitle = "Modifier Utilisateur";
        
        // Split username into Name and Surname (approximation)
        var parts = SelectedUser.Username.Split(' ');
        FormName = parts.Length > 0 ? parts[0] : string.Empty;
        FormSurname = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : string.Empty;
        
        FormHouseCode = string.Empty; // Limitation
        
        FormCode = string.Empty; // Password not shown
        FormRole = SelectedUser.Role;
        FormIsActive = SelectedUser.IsActive;
        IsFormVisible = true;
        ErrorMessage = string.Empty;
    }

    private void HideForm()
    {
        IsFormVisible = false;
        ErrorMessage = string.Empty;
    }

    private async Task SaveUserAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            if (IsEditMode)
            {
                if (SelectedUser == null) return;

                var updateDto = new UpdateUserDto
                {
                    Name = FormName,
                    Surname = FormSurname,
                    IsActive = FormIsActive,
                    // Role cannot be updated in UpdateUserDto? Let's check CommonDTOs.cs
                    // UpdateUserDto has: Name, Surname, SignaturePath, IsActive. NO Role.
                };
                
                await _userService.UpdateUserAsync(SelectedUser.Id, updateDto);
            }
            else
            {
                var createDto = new CreateUserDto
                {
                    Name = FormName,
                    Surname = FormSurname,
                    HouseCode = FormHouseCode,
                    Code = FormCode,
                    Role = FormRole
                };
                
                await _userService.CreateUserAsync(createDto);
            }
            
            HideForm();
            await LoadUsersAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task DeleteUserAsync(UserDto? user)
    {
        if (user != null) SelectedUser = user;
        if (SelectedUser == null) return;
        
        if (!IsAdmin)
        {
            ErrorMessage = "Seul l'administrateur peut supprimer des utilisateurs.";
            return;
        }

        // Prevent self-deletion
        var currentUser = await _authService.GetCurrentUserAsync();
        if (currentUser != null && currentUser.Id == SelectedUser.Id)
        {
            System.Windows.MessageBox.Show("Vous ne pouvez pas supprimer votre propre compte.", "Action interdite", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        var result = System.Windows.MessageBox.Show(
            $"Êtes-vous sûr de vouloir supprimer cet utilisateur ?\n\nNom: {SelectedUser.Username}\nRôle: {SelectedUser.Role}",
            "Confirmation de suppression",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (result != System.Windows.MessageBoxResult.Yes) return;
        
        IsLoading = true;
        try
        {
            await _userService.DeleteUserAsync(SelectedUser.Id);
            Users.Remove(SelectedUser);
            SelectedUser = null;
            System.Windows.MessageBox.Show("Utilisateur supprimé avec succès", "Succès", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur lors de la suppression: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ResetPasswordAsync()
    {
        if (SelectedUser == null) return;

        // Logic to show reset password dialog (enter new code)
         ErrorMessage = "La fonctionnalité de réinitialisation du mot de passe nécessite l'implémentation d'une boîte de dialogue.";
    }
}
