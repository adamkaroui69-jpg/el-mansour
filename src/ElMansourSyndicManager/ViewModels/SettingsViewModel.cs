using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;
using ElMansourSyndicManager.ViewModels.Base;
using System.Net.Http;
using System.Xml.Linq;
using System.Diagnostics;
using System.Windows;
using MaterialDesignThemes.Wpf;

using MaterialDesignThemes.Wpf;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;

namespace ElMansourSyndicManager.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private string _selectedTheme = "Clair";
    private string _selectedLanguage = "Français";
    private bool _notificationsEnabled = true;
    private bool _autoBackupEnabled = true;
    private string _backupFrequency = "Quotidien";
    private bool _isCheckingForUpdates;
    private string _updateStatus = "";

    public string AppVersion
    {
        get
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "1.0.0";
        }
    }

    public bool IsCheckingForUpdates
    {
        get => _isCheckingForUpdates;
        set => SetProperty(ref _isCheckingForUpdates, value);
    }

    public string UpdateStatus
    {
        get => _updateStatus;
        set => SetProperty(ref _updateStatus, value);
    }

    private readonly IUpdateService _updateService;
    private readonly IBackupService _backupService;

    public SettingsViewModel(IUpdateService updateService, IBackupService backupService)
    {
        _updateService = updateService;
        _backupService = backupService;

        // Load settings (mocked for now or from Properties.Settings)
        SelectedTheme = "Clair";
        SelectedLanguage = "Français";
        NotificationsEnabled = true;
        AutoBackupEnabled = true;
        BackupFrequency = "Quotidien";
        EnableEmailNotifications = false;
        EnablePaymentReminders = true;

        SaveCommand = new RelayCommand(SaveSettings);
        BackupNowCommand = new RelayCommand(async () => await BackupNowAsync());
        CheckForUpdatesCommand = new RelayCommand(async () => await CheckForUpdatesAsync());
        
        // Apply initial theme
        ApplyTheme(SelectedTheme);
    }
    
    // Default constructor for design time (if needed) fallback
    public SettingsViewModel() : this(new DesignTimeUpdateService(), new DesignTimeBackupService()) { }

    public string SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (SetProperty(ref _selectedTheme, value))
            {
                ApplyTheme(value);
            }
        }
    }

    private void ApplyTheme(string theme)
    {
        var paletteHelper = new PaletteHelper();
        var themeObj = paletteHelper.GetTheme();

        if (theme == "Sombre")
        {
            themeObj.SetBaseTheme(MaterialDesignThemes.Wpf.Theme.Dark);
        }
        else
        {
            themeObj.SetBaseTheme(MaterialDesignThemes.Wpf.Theme.Light);
        }

        paletteHelper.SetTheme(themeObj);
    }

    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set => SetProperty(ref _selectedLanguage, value);
    }

    public bool NotificationsEnabled
    {
        get => _notificationsEnabled;
        set => SetProperty(ref _notificationsEnabled, value);
    }

    public bool AutoBackupEnabled
    {
        get => _autoBackupEnabled;
        set => SetProperty(ref _autoBackupEnabled, value);
    }

    public string BackupFrequency
    {
        get => _backupFrequency;
        set => SetProperty(ref _backupFrequency, value);
    }

    public ObservableCollection<string> Themes { get; } = new ObservableCollection<string> { "Clair", "Sombre" };
    public ObservableCollection<string> Languages { get; } = new ObservableCollection<string> { "Français", "English", "العربية" };
    public ObservableCollection<string> BackupFrequencies { get; } = new ObservableCollection<string> { "Quotidien", "Hebdomadaire", "Mensuel" };

    private bool _enableEmailNotifications;
    private bool _enablePaymentReminders;

    public bool EnableEmailNotifications
    {
        get => _enableEmailNotifications;
        set => SetProperty(ref _enableEmailNotifications, value);
    }

    public bool EnablePaymentReminders
    {
        get => _enablePaymentReminders;
        set => SetProperty(ref _enablePaymentReminders, value);
    }

    public ICommand SaveCommand { get; }
    public ICommand BackupNowCommand { get; }
    public ICommand CheckForUpdatesCommand { get; }

    private void SaveSettings()
    {
        // Save settings logic here
        System.Windows.MessageBox.Show("Paramètres enregistrés avec succès.", "Succès", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }

    private async Task BackupNowAsync()
    {
        try 
        {
            await _backupService.RunBackupAsync();
            System.Windows.MessageBox.Show("Sauvegarde effectuée avec succès.", "Succès", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Erreur lors de la sauvegarde : {ex.Message}", "Erreur", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private async Task CheckForUpdatesAsync()
    {
        IsCheckingForUpdates = true;
        UpdateStatus = "Vérification des mises à jour...";

        try
        {
            var updateInfo = await _updateService.CheckForUpdatesAsync();

            if (updateInfo != null)
            {
                UpdateStatus = $"Nouvelle version disponible : {updateInfo.Version}";
                
                var result = MessageBox.Show(
                    $"Une nouvelle version ({updateInfo.Version}) est disponible !\n\n" +
                    $"Notes : {updateInfo.ReleaseNotes}\n\n" +
                    "Voulez-vous télécharger la mise à jour maintenant ?",
                    "Mise à jour disponible",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = updateInfo.DownloadUrl,
                        UseShellExecute = true
                    });
                }
            }
            else
            {
                UpdateStatus = "Vous utilisez la dernière version.";
                MessageBox.Show(
                    $"Vous utilisez déjà la dernière version ({AppVersion}).",
                    "Aucune mise à jour",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            UpdateStatus = "Erreur de vérification.";
            MessageBox.Show($"Impossible de vérifier les mises à jour : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsCheckingForUpdates = false;
        }
    }
}

// Dummy classes for design time
public class DesignTimeUpdateService : IUpdateService
{
    public Task<UpdateInfo?> CheckForUpdatesAsync() => Task.FromResult<UpdateInfo?>(null);
    public string GetCurrentVersion() => "1.0.0";
}

public class DesignTimeBackupService : IBackupService
{
    public Task<ElMansourSyndicManager.Core.Domain.DTOs.BackupHistoryDTO> RunBackupAsync(bool isAutomatic = false, System.Threading.CancellationToken cancellationToken = default) => Task.FromResult(new ElMansourSyndicManager.Core.Domain.DTOs.BackupHistoryDTO());
    public Task<List<ElMansourSyndicManager.Core.Domain.DTOs.BackupHistoryDTO>> GetBackupHistoryAsync(System.Threading.CancellationToken cancellationToken = default) => Task.FromResult(new List<ElMansourSyndicManager.Core.Domain.DTOs.BackupHistoryDTO>());
    public Task DeleteOldBackupsAsync(int keepLastN, System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task TriggerScheduledBackupAsync(System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<bool> RestoreBackupAsync(string backupFilePath, System.Threading.CancellationToken cancellationToken = default) => Task.FromResult(true);
    public Task<bool> DeleteBackupAsync(string backupId, System.Threading.CancellationToken cancellationToken = default) => Task.FromResult(true);
    public Task<string?> GetBackupFilePathAsync(string backupId, System.Threading.CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
    public Task ScheduleBackupsAsync(bool enabled, TimeSpan? time = null, System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;
}
