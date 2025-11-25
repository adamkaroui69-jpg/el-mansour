using System.Collections.ObjectModel;
using System.Windows.Input;
using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.ViewModels.Base;
using Microsoft.Win32;
using System.IO;

namespace ElMansourSyndicManager.ViewModels;

public class AuditViewModel : ViewModelBase
{
    private readonly IAuditService _auditService;
    private ObservableCollection<AuditLogDto> _logs;
    private bool _isLoading;
    private string _errorMessage = string.Empty;
    
    private DateTime _startDate;
    private DateTime _endDate;
    private string _userIdFilter = string.Empty;

    public AuditViewModel(IAuditService auditService)
    {
        _auditService = auditService;
        _logs = new ObservableCollection<AuditLogDto>();
        
        // Default to last 30 days
        StartDate = DateTime.Today.AddDays(-30);
        EndDate = DateTime.Today.AddDays(1).AddSeconds(-1);

        LoadCommand = new RelayCommand(async () => await LoadLogsAsync());
        ExportCommand = new RelayCommand(async () => await ExportLogsAsync());
    }

    public ObservableCollection<AuditLogDto> Logs
    {
        get => _logs;
        set => SetProperty(ref _logs, value);
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

    public DateTime StartDate
    {
        get => _startDate;
        set => SetProperty(ref _startDate, value);
    }

    public DateTime EndDate
    {
        get => _endDate;
        set => SetProperty(ref _endDate, value);
    }

    public string UserIdFilter
    {
        get => _userIdFilter;
        set => SetProperty(ref _userIdFilter, value);
    }

    public ICommand LoadCommand { get; }
    public ICommand ExportCommand { get; }

    public async Task LoadLogsAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var logs = await _auditService.GetAuditLogsAsync(StartDate, EndDate, string.IsNullOrWhiteSpace(UserIdFilter) ? null : UserIdFilter);
            Logs = new ObservableCollection<AuditLogDto>(logs.OrderByDescending(l => l.Timestamp));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur lors du chargement des journaux d'audit: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExportLogsAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var data = await _auditService.ExportAuditLogsAsync(StartDate, EndDate);
            
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                FileName = $"AuditLogs_{DateTime.Now:yyyyMMdd}.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                await File.WriteAllBytesAsync(saveFileDialog.FileName, data);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur lors de l'exportation des journaux: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
