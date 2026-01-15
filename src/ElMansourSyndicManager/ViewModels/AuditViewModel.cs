using ElMansourSyndicManager.Core.Domain.Entities;
using ElMansourSyndicManager.Core.Domain.Enums;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ElMansourSyndicManager.ViewModels;

public class AuditViewModel : ViewModelBase
{
    private readonly IAuditService _auditService;
    
    private DateTime? _filterDateFrom;
    private DateTime? _filterDateTo;
    private string? _filterUser;
    private string? _filterAction;
    private string? _filterEntityType;
    private string? _filterSeverity;
    private bool _isLoading;
    private int _totalRecords;

    public ObservableCollection<AuditLog> AuditLogs { get; } = new();
    public ObservableCollection<string> Actions { get; } = new();
    public ObservableCollection<string> EntityTypes { get; } = new();
    public ObservableCollection<string> Severities { get; } = new();

    public DateTime? FilterDateFrom
    {
        get => _filterDateFrom;
        set => SetProperty(ref _filterDateFrom, value);
    }

    public DateTime? FilterDateTo
    {
        get => _filterDateTo;
        set => SetProperty(ref _filterDateTo, value);
    }

    public string? FilterUser
    {
        get => _filterUser;
        set => SetProperty(ref _filterUser, value);
    }

    public string? FilterAction
    {
        get => _filterAction;
        set => SetProperty(ref _filterAction, value);
    }

    public string? FilterEntityType
    {
        get => _filterEntityType;
        set => SetProperty(ref _filterEntityType, value);
    }

    public string? FilterSeverity
    {
        get => _filterSeverity;
        set => SetProperty(ref _filterSeverity, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public int TotalRecords
    {
        get => _totalRecords;
        set => SetProperty(ref _totalRecords, value);
    }

    public ICommand SearchCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand LoadCommand { get; }

    public AuditViewModel(IAuditService auditService)
    {
        _auditService = auditService;

        // Initialiser les filtres
        InitializeFilters();

        // Commandes
        SearchCommand = new RelayCommand(async () => await SearchAsync());
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        ExportCommand = new RelayCommand(async () => await ExportAsync());
        LoadCommand = new RelayCommand(async () => await LoadAuditLogsAsync());

        // Charger les données initiales (dernières 24h)
        FilterDateFrom = DateTime.Now.AddDays(-1);
        FilterDateTo = DateTime.Now;
        _ = LoadAuditLogsAsync();
    }

    private void InitializeFilters()
    {
        // Actions
        Actions.Add("Tous");
        Actions.Add(AuditAction.Login);
        Actions.Add(AuditAction.Logout);
        Actions.Add(AuditAction.Create);
        Actions.Add(AuditAction.Update);
        Actions.Add(AuditAction.Delete);
        Actions.Add(AuditAction.PaymentReceived);
        Actions.Add(AuditAction.ReceiptGenerated);
        Actions.Add(AuditAction.BackupCreated);
        Actions.Add(AuditAction.BackupRestored);

        // Types d'entités
        EntityTypes.Add("Tous");
        EntityTypes.Add(AuditEntityType.User);
        EntityTypes.Add(AuditEntityType.Payment);
        EntityTypes.Add(AuditEntityType.House);
        EntityTypes.Add(AuditEntityType.Receipt);
        EntityTypes.Add(AuditEntityType.Document);
        EntityTypes.Add(AuditEntityType.Backup);
        EntityTypes.Add(AuditEntityType.Settings);

        // Sévérités
        Severities.Add("Tous");
        Severities.Add(AuditSeverity.Info);
        Severities.Add(AuditSeverity.Warning);
        Severities.Add(AuditSeverity.Error);
        Severities.Add(AuditSeverity.Critical);
    }

    private async Task LoadAuditLogsAsync()
    {
        IsLoading = true;
        try
        {
            var logs = await _auditService.GetAuditLogsAsync(
                from: FilterDateFrom,
                to: FilterDateTo,
                userId: string.IsNullOrEmpty(FilterUser) ? null : FilterUser,
                action: FilterAction == "Tous" ? null : FilterAction,
                entityType: FilterEntityType == "Tous" ? null : FilterEntityType,
                severity: FilterSeverity == "Tous" ? null : FilterSeverity,
                skip: 0,
                take: 500);

            AuditLogs.Clear();
            foreach (var log in logs)
            {
                AuditLogs.Add(log);
            }

            TotalRecords = logs.Count;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Erreur lors du chargement des logs: {ex.Message}", "Erreur", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SearchAsync()
    {
        await LoadAuditLogsAsync();
    }

    private void ClearFilters()
    {
        FilterDateFrom = DateTime.Now.AddDays(-1);
        FilterDateTo = DateTime.Now;
        FilterUser = null;
        FilterAction = "Tous";
        FilterEntityType = "Tous";
        FilterSeverity = "Tous";
        _ = LoadAuditLogsAsync();
    }

    private async Task ExportAsync()
    {
        // TODO: Implémenter l'export CSV/Excel
        System.Windows.MessageBox.Show("Export en cours de développement", "Info", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        await Task.CompletedTask;
    }
}
