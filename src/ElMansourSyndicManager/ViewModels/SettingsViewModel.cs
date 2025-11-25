using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;
using ElMansourSyndicManager.ViewModels.Base;

namespace ElMansourSyndicManager.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private string _selectedTheme = "Clair";
    private string _selectedLanguage = "Français";
    private bool _notificationsEnabled = true;
    private bool _autoBackupEnabled = true;
    private string _backupFrequency = "Quotidien";

    public string AppVersion
    {
        get
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "1.0.0";
        }
    }


    public string SelectedTheme
    {
        get => _selectedTheme;
        set => SetProperty(ref _selectedTheme, value);
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

    public SettingsViewModel()
    {
        // Load settings (mocked for now or from Properties.Settings)
        SelectedTheme = "Clair";
        SelectedLanguage = "Français";
        NotificationsEnabled = true;
        AutoBackupEnabled = true;
        BackupFrequency = "Quotidien";
        EnableEmailNotifications = false;
        EnablePaymentReminders = true;

        SaveCommand = new RelayCommand(SaveSettings);
        BackupNowCommand = new RelayCommand(BackupNow);
    }

    private void SaveSettings()
    {
        // Save settings logic here
        // For example, save to Properties.Settings.Default or a JSON config file
        System.Windows.MessageBox.Show("Paramètres enregistrés avec succès.", "Succès", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }

    private void BackupNow()
    {
        System.Windows.MessageBox.Show("Sauvegarde effectuée avec succès.", "Succès", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }
}
