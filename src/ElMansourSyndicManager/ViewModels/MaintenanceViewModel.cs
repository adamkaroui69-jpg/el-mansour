using System.Collections.ObjectModel;
using System.Windows.Input;
using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.ViewModels.Base;

namespace ElMansourSyndicManager.ViewModels;

public class MaintenanceViewModel : ViewModelBase
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly IAuthenticationService _authService;
    private readonly IExpenseService _expenseService; // Injected
    private ObservableCollection<MaintenanceDTO> _maintenanceList;
    private MaintenanceDTO? _selectedMaintenance;
    private bool _isLoading;
    private string _errorMessage = string.Empty;

    // Form properties
    private bool _isFormVisible;
    private bool _isEditMode;
    private string _formTitle = string.Empty;
    
    private string _formDescription = string.Empty;
    private string _formType = "Maintenance";
    private decimal _formCost;
    private string _formStatus = "Pending";
    private string _formPriority = "Normal";
    private DateTime _formScheduledDate = DateTime.Today;
    private string _formNotes = string.Empty;
    private string _formAssignedTo = string.Empty;
    private bool _createExpense; // New property

    private bool _isAdmin;

    public MaintenanceViewModel(IMaintenanceService maintenanceService, IAuthenticationService authService, IExpenseService expenseService)
    {
        _maintenanceService = maintenanceService;
        _authService = authService;
        _expenseService = expenseService;
        _maintenanceList = new ObservableCollection<MaintenanceDTO>();

        LoadCommand = new RelayCommand(async () => await LoadMaintenanceAsync());
        CreateCommand = new RelayCommand(ShowCreateForm);
        UpdateCommand = new RelayCommand<MaintenanceDTO>(ShowUpdateForm);
        DeleteCommand = new RelayCommand<MaintenanceDTO>(async (m) => await DeleteMaintenanceAsync(m), (m) => IsAdmin);
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

    public ObservableCollection<MaintenanceDTO> MaintenanceList
    {
        get => _maintenanceList;
        set => SetProperty(ref _maintenanceList, value);
    }

    public MaintenanceDTO? SelectedMaintenance
    {
        get => _selectedMaintenance;
        set => SetProperty(ref _selectedMaintenance, value);
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

    public string FormType
    {
        get => _formType;
        set => SetProperty(ref _formType, value);
    }

    public decimal FormCost
    {
        get => _formCost;
        set => SetProperty(ref _formCost, value);
    }

    public string FormStatus
    {
        get => _formStatus;
        set => SetProperty(ref _formStatus, value);
    }

    public string FormPriority
    {
        get => _formPriority;
        set => SetProperty(ref _formPriority, value);
    }

    public DateTime FormScheduledDate
    {
        get => _formScheduledDate;
        set => SetProperty(ref _formScheduledDate, value);
    }

    public string FormNotes
    {
        get => _formNotes;
        set => SetProperty(ref _formNotes, value);
    }

    public string FormAssignedTo
    {
        get => _formAssignedTo;
        set => SetProperty(ref _formAssignedTo, value);
    }

    public bool CreateExpense
    {
        get => _createExpense;
        set => SetProperty(ref _createExpense, value);
    }

    public ObservableCollection<string> Types { get; } = new ObservableCollection<string> { "Maintenance", "Réparation", "Amélioration", "Urgence", "Autre" };
    public ObservableCollection<string> Statuses { get; } = new ObservableCollection<string> { "Pending", "InProgress", "Completed" };
    public ObservableCollection<string> Priorities { get; } = new ObservableCollection<string> { "Low", "Normal", "High", "Urgent" };

    public async Task LoadMaintenanceAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var items = await _maintenanceService.GetAllMaintenanceAsync();
            MaintenanceList = new ObservableCollection<MaintenanceDTO>(items.OrderByDescending(m => m.ScheduledDate));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur lors du chargement de la maintenance: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ShowCreateForm()
    {
        IsEditMode = false;
        FormTitle = "Nouvelle Maintenance";
        FormDescription = string.Empty;
        FormType = "Maintenance";
        FormCost = 0;
        FormStatus = "Pending";
        FormPriority = "Normal";
        FormScheduledDate = DateTime.Today;
        FormNotes = string.Empty;
        FormAssignedTo = string.Empty;
        CreateExpense = false; // Reset to false
        IsFormVisible = true;
    }

    private void ShowUpdateForm(MaintenanceDTO? maintenance)
    {
        if (maintenance != null) SelectedMaintenance = maintenance;
        if (SelectedMaintenance == null) return;

        IsEditMode = true;
        FormTitle = "Modifier Maintenance";
        FormDescription = SelectedMaintenance.Description;
        FormType = SelectedMaintenance.Type;
        FormCost = SelectedMaintenance.Cost;
        FormStatus = SelectedMaintenance.Status;
        FormPriority = SelectedMaintenance.Priority;
        FormScheduledDate = SelectedMaintenance.ScheduledDate ?? DateTime.Today;
        FormNotes = SelectedMaintenance.Notes ?? string.Empty;
        FormAssignedTo = SelectedMaintenance.AssignedTo ?? string.Empty;
        CreateExpense = false; // Hide or reset for edit mode, or handle differently
        IsFormVisible = true;
    }

    private async Task DeleteMaintenanceAsync(MaintenanceDTO? maintenance)
    {
        if (maintenance != null) SelectedMaintenance = maintenance;
        if (SelectedMaintenance == null) return;
        
        if (!IsAdmin)
        {
            ErrorMessage = "Seul l'administrateur peut supprimer des tâches de maintenance.";
            return;
        }

        var result = System.Windows.MessageBox.Show(
            $"Êtes-vous sûr de vouloir supprimer cette tâche ?\n\nDescription: {SelectedMaintenance.Description}",
            "Confirmation de suppression",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (result != System.Windows.MessageBoxResult.Yes) return;

        IsLoading = true;
        try
        {
            await _maintenanceService.DeleteMaintenanceAsync(SelectedMaintenance.Id);
            MaintenanceList.Remove(SelectedMaintenance);
            SelectedMaintenance = null;
            System.Windows.MessageBox.Show("Tâche supprimée avec succès", "Succès", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
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
                if (SelectedMaintenance == null) return;

                var updateDto = new UpdateMaintenanceDTO
                {
                    Description = FormDescription,
                    Type = FormType,
                    Cost = FormCost,
                    Status = FormStatus,
                    Priority = FormPriority,
                    ScheduledDate = FormScheduledDate,
                    Notes = FormNotes,
                    AssignedTo = FormAssignedTo
                };
                
                var updatedMaintenance = await _maintenanceService.UpdateMaintenanceAsync(SelectedMaintenance.Id, updateDto);
                
                // Update local collection directly
                var index = MaintenanceList.IndexOf(SelectedMaintenance);
                if (index >= 0)
                {
                    MaintenanceList[index] = updatedMaintenance;
                }
            }
            else
            {
                var createDto = new CreateMaintenanceDTO
                {
                    Description = FormDescription,
                    Type = FormType,
                    Cost = FormCost,
                    Priority = FormPriority,
                    ScheduledDate = FormScheduledDate,
                    Notes = FormNotes,
                    AssignedTo = FormAssignedTo
                };
                
                var newMaintenance = await _maintenanceService.CreateMaintenanceAsync(createDto);
                
                // Add to local collection directly (at the top)
                MaintenanceList.Insert(0, newMaintenance);

                // Create Expense if requested
                if (CreateExpense && FormCost > 0)
                {
                    try
                    {
                        var createExpenseDto = new CreateExpenseDto
                        {
                            Description = $"Maintenance: {FormDescription}",
                            Category = "Maintenance & Réparations",
                            Amount = FormCost,
                            ExpenseDate = FormScheduledDate,
                            MaintenanceId = newMaintenance.Id.ToString(),
                            Notes = "Généré automatiquement depuis le module Maintenance"
                        };
                        await _expenseService.CreateExpenseAsync(createExpenseDto);
                        System.Windows.MessageBox.Show("Dépense créée automatiquement avec succès.", "Info", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                    catch (Exception exExpense)
                    {
                         System.Windows.MessageBox.Show($"La maintenance a été créée, mais la création de la dépense a échoué: {exExpense.Message}", "Avertissement", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    }
                }
            }
            
            HideForm();
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
