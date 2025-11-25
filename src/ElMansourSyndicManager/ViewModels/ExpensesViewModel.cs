using System.Collections.ObjectModel;
using System.Windows.Input;
using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.ViewModels.Base;

namespace ElMansourSyndicManager.ViewModels;

public class ExpensesViewModel : ViewModelBase
{
    private readonly IExpenseService _expenseService;
    private ObservableCollection<ExpenseDto> _expenses;
    private ExpenseDto? _selectedExpense;
    private bool _isLoading;
    private string _errorMessage = string.Empty;

    // Form properties
    private bool _isFormVisible;
    private bool _isEditMode;
    private string _formTitle = string.Empty;
    
    private string _formDescription = string.Empty;
    private string _formCategory = "Autre";
    private decimal _formAmount;
    private DateTime _formExpenseDate = DateTime.Today;
    private string _formNotes = string.Empty;

    private readonly IAuthenticationService _authService;
    private bool _isAdmin;

    public ExpensesViewModel(IExpenseService expenseService, IAuthenticationService authService)
    {
        _expenseService = expenseService;
        _authService = authService;
        _expenses = new ObservableCollection<ExpenseDto>();

        LoadCommand = new RelayCommand(async () => await LoadExpensesAsync());
        CreateCommand = new RelayCommand(ShowCreateForm);
        UpdateCommand = new RelayCommand<ExpenseDto>(ShowUpdateForm);
        DeleteCommand = new RelayCommand<ExpenseDto>(async (e) => await DeleteExpenseAsync(e), (e) => IsAdmin);
        SaveCommand = new RelayCommand(async () => await SaveAsync());
        CancelCommand = new RelayCommand(HideForm);

        CheckAdminStatus();
    }

    private async void CheckAdminStatus()
    {
        var user = await _authService.GetCurrentUserAsync();
        IsAdmin = user?.Role == "Admin";
    }

    public bool IsAdmin
    {
        get => _isAdmin;
        set => SetProperty(ref _isAdmin, value);
    }

    public ICommand LoadCommand { get; }
    public ICommand CreateCommand { get; }
    public ICommand UpdateCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public ObservableCollection<ExpenseDto> Expenses
    {
        get => _expenses;
        set => SetProperty(ref _expenses, value);
    }

    public ExpenseDto? SelectedExpense
    {
        get => _selectedExpense;
        set => SetProperty(ref _selectedExpense, value);
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

    // Form Properties
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

    public string FormDescription
    {
        get => _formDescription;
        set => SetProperty(ref _formDescription, value);
    }

    public string FormCategory
    {
        get => _formCategory;
        set => SetProperty(ref _formCategory, value);
    }

    public decimal FormAmount
    {
        get => _formAmount;
        set => SetProperty(ref _formAmount, value);
    }

    public DateTime FormExpenseDate
    {
        get => _formExpenseDate;
        set => SetProperty(ref _formExpenseDate, value);
    }

    public string FormNotes
    {
        get => _formNotes;
        set => SetProperty(ref _formNotes, value);
    }

    public ObservableCollection<string> Categories { get; } = new ObservableCollection<string> { "Maintenance", "Utilitaires", "Nettoyage", "Administratif", "Autre" };

    public async Task LoadExpensesAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var items = await _expenseService.GetAllExpensesAsync();
            Expenses = new ObservableCollection<ExpenseDto>(items.OrderByDescending(e => e.ExpenseDate));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur lors du chargement des dépenses: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ShowCreateForm()
    {
        IsEditMode = false;
        FormTitle = "Nouvelle Dépense";
        FormDescription = string.Empty;
        FormCategory = "Autre";
        FormAmount = 0;
        FormExpenseDate = DateTime.Today;
        FormNotes = string.Empty;
        IsFormVisible = true;
    }

    private void ShowUpdateForm(ExpenseDto? expense)
    {
        if (expense != null) SelectedExpense = expense;
        if (SelectedExpense == null) return;

        IsEditMode = true;
        FormTitle = "Modifier Dépense";
        FormDescription = SelectedExpense.Description;
        FormCategory = SelectedExpense.Category;
        FormAmount = SelectedExpense.Amount;
        FormExpenseDate = SelectedExpense.ExpenseDate;
        FormNotes = SelectedExpense.Notes ?? string.Empty;
        IsFormVisible = true;
    }

    // ...

    private async Task DeleteExpenseAsync(ExpenseDto? expense)
    {
        if (expense != null) SelectedExpense = expense;
        if (SelectedExpense == null) return;
        
        if (!IsAdmin)
        {
            ErrorMessage = "Seul l'administrateur peut supprimer des dépenses.";
            return;
        }

        var result = System.Windows.MessageBox.Show(
            $"Êtes-vous sûr de vouloir supprimer cette dépense ?\n\nDescription: {SelectedExpense.Description}\nMontant: {SelectedExpense.Amount:N2} TND",
            "Confirmation de suppression",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (result != System.Windows.MessageBoxResult.Yes) return;

        IsLoading = true;
        try
        {
            await _expenseService.DeleteExpenseAsync(SelectedExpense.Id);
            Expenses.Remove(SelectedExpense);
            SelectedExpense = null;
            System.Windows.MessageBox.Show("Dépense supprimée avec succès", "Succès", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
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

    private void HideForm()
    {
        IsFormVisible = false;
        ErrorMessage = string.Empty;
    }

    private async Task SaveAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            if (IsEditMode)
            {
                if (SelectedExpense == null) return;

                var updateDto = new UpdateExpenseDto
                {
                    Description = FormDescription,
                    Category = FormCategory,
                    Amount = FormAmount,
                    ExpenseDate = FormExpenseDate,
                    Notes = FormNotes
                };
                
                var updatedExpense = await _expenseService.UpdateExpenseAsync(SelectedExpense.Id, updateDto);
                
                // Update local collection directly
                var index = Expenses.IndexOf(SelectedExpense);
                if (index >= 0)
                {
                    Expenses[index] = updatedExpense;
                }
            }
            else
            {
                var createDto = new CreateExpenseDto
                {
                    Description = FormDescription,
                    Category = FormCategory,
                    Amount = FormAmount,
                    ExpenseDate = FormExpenseDate,
                    Notes = FormNotes
                };
                
                var newExpense = await _expenseService.CreateExpenseAsync(createDto);
                
                // Add to local collection directly (at the top)
                Expenses.Insert(0, newExpense);
            }
            
            HideForm();
            // await LoadExpensesAsync(); // Removed to prevent reload flicker and potential race conditions
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


}
