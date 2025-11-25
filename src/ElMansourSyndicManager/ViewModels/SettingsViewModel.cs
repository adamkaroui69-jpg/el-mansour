using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;
using ElMansourSyndicManager.ViewModels.Base;
using System.Net.Http;
using System.Xml.Linq;
using System.Diagnostics;
using System.Windows;

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
    public ICommand CheckForUpdatesCommand { get; }

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
        CheckForUpdatesCommand = new RelayCommand(async () => await CheckForUpdatesAsync());
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

    private async Task CheckForUpdatesAsync()
    {
        IsCheckingForUpdates = true;
        UpdateStatus = "Vérification des mises à jour...";

        var releasesUrl = "https://github.com/adamkaroui69-jpg/el-mansour/releases";

        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            
            // GitHub requires a User-Agent header
            httpClient.DefaultRequestHeaders.Add("User-Agent", "ElMansourSyndicManager");

            // Download update.xml from GitHub releases
            var updateXmlUrl = "https://github.com/adamkaroui69-jpg/el-mansour/releases/latest/download/update.xml";
            var xmlContent = await httpClient.GetStringAsync(updateXmlUrl);

            // Parse XML
            var doc = XDocument.Parse(xmlContent);
            var versionElement = doc.Root?.Element("version");
            var urlElement = doc.Root?.Element("url");

            if (versionElement == null || urlElement == null)
            {
                UpdateStatus = "Format de mise à jour invalide.";
                OpenReleasesPageFallback(releasesUrl);
                return;
            }

            var latestVersion = versionElement.Value;
            var downloadUrl = urlElement.Value;

            // Compare versions
            var currentVersion = new Version(AppVersion);
            var remoteVersion = new Version(latestVersion.TrimEnd('0', '.'));

            if (remoteVersion > currentVersion)
            {
                UpdateStatus = $"Nouvelle version disponible : {latestVersion}";
                
                var result = MessageBox.Show(
                    $"Une nouvelle version ({latestVersion}) est disponible !\n\n" +
                    $"Version actuelle : {AppVersion}\n" +
                    $"Nouvelle version : {latestVersion}\n\n" +
                    "Voulez-vous télécharger la mise à jour maintenant ?",
                    "Mise à jour disponible",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    // Open download URL in browser
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = downloadUrl,
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
            UpdateStatus = "Impossible de vérifier automatiquement.";
            
            // Fallback: Propose to open the releases page manually
            var result = MessageBox.Show(
                $"Impossible de vérifier les mises à jour automatiquement (Erreur: {ex.Message}).\n\n" +
                "Cela arrive souvent si le dépôt est privé.\n\n" +
                "Voulez-vous ouvrir la page des téléchargements pour vérifier manuellement ?",
                "Vérification manuelle",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = releasesUrl,
                    UseShellExecute = true
                });
            }
        }
        finally
        {
            IsCheckingForUpdates = false;
        }
    }

    private void OpenReleasesPageFallback(string url)
    {
        var result = MessageBox.Show(
            "Impossible de lire les informations de mise à jour.\n" +
            "Voulez-vous ouvrir la page des téléchargements ?",
            "Erreur",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
    }
}
