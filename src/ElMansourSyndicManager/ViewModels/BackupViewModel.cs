using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.ViewModels.Base;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ElMansourSyndicManager.ViewModels;

/// <summary>
/// ViewModel for the backup page
/// </summary>
public class BackupViewModel : ViewModelBase
{
    private readonly IBackupService _backupService;
    private readonly IAuthenticationService _authService;
    private BackupHistoryDTO? _selectedBackup;
    private bool _isLoading;
    private bool _isBackingUp;
    private string _statusMessage = string.Empty;
    private int _keepLastNBackups = 10;

    public BackupViewModel(
        IBackupService backupService,
        IAuthenticationService authService)
    {
        _backupService = backupService;
        _authService = authService;

        Backups = new ObservableCollection<BackupHistoryDTO>();

        RunBackupCommand = new RelayCommand(async () => await RunBackupAsync(), () => !IsBackingUp);
        DeleteOldBackupsCommand = new RelayCommand(async () => await DeleteOldBackupsAsync(), () => !IsBackingUp);
        DeleteBackupCommand = new RelayCommand<BackupHistoryDTO>(async b => await DeleteBackupAsync(b), b => b != null);
        DownloadBackupCommand = new RelayCommand<BackupHistoryDTO>(async b => await DownloadBackupAsync(b), b => b != null);
        RestoreBackupCommand = new RelayCommand(async () => await RestoreBackupAsync());
        RefreshCommand = new RelayCommand(async () => await LoadBackupsAsync());

        // Load initial data
        _ = LoadBackupsAsync();
    }

    public ObservableCollection<BackupHistoryDTO> Backups { get; }

    public BackupHistoryDTO? SelectedBackup
    {
        get => _selectedBackup;
        set => SetProperty(ref _selectedBackup, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsBackingUp
    {
        get => _isBackingUp;
        set
        {
            if (SetProperty(ref _isBackingUp, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public int KeepLastNBackups
    {
        get => _keepLastNBackups;
        set => SetProperty(ref _keepLastNBackups, value);
    }

    public ICommand RunBackupCommand { get; }
    public ICommand DeleteOldBackupsCommand { get; }
    public ICommand DeleteBackupCommand { get; }
    public ICommand DownloadBackupCommand { get; }
    public ICommand RestoreBackupCommand { get; }
    public ICommand RefreshCommand { get; }

    private async Task RunBackupAsync()
    {
        try
        {
            IsBackingUp = true;
            StatusMessage = "Création de la sauvegarde en cours...";

            var backup = await _backupService.RunBackupAsync(isAutomatic: false);

            StatusMessage = $"Sauvegarde créée avec succès. Taille: {FormatFileSize(backup.FileSize)}";
            await LoadBackupsAsync();

            MessageBox.Show(
                "La sauvegarde a été créée avec succès.",
                "Succès",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur lors de la sauvegarde: {ex.Message}";
            MessageBox.Show(
                $"Erreur lors de la création de la sauvegarde: {ex.Message}",
                "Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsBackingUp = false;
        }
    }

    private async Task DeleteOldBackupsAsync()
    {
        try
        {
            var result = MessageBox.Show(
                $"Êtes-vous sûr de vouloir supprimer les anciennes sauvegardes (garder les {KeepLastNBackups} dernières)?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                IsBackingUp = true;
                StatusMessage = "Suppression des anciennes sauvegardes...";

                await _backupService.DeleteOldBackupsAsync(KeepLastNBackups);
                await LoadBackupsAsync();

                StatusMessage = "Anciennes sauvegardes supprimées avec succès";
                MessageBox.Show(
                    "Les anciennes sauvegardes ont été supprimées avec succès.",
                    "Succès",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur lors de la suppression: {ex.Message}";
            MessageBox.Show(
                $"Erreur lors de la suppression des anciennes sauvegardes: {ex.Message}",
                "Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsBackingUp = false;
        }
    }

    private async Task DeleteBackupAsync(BackupHistoryDTO backup)
    {
        try
        {
            var result = MessageBox.Show(
                "Êtes-vous sûr de vouloir supprimer cette sauvegarde?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _backupService.DeleteBackupAsync(backup.Id.ToString());
                Backups.Remove(backup);
                StatusMessage = "Sauvegarde supprimée avec succès";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur lors de la suppression: {ex.Message}",
                "Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async Task DownloadBackupAsync(BackupHistoryDTO backup)
    {
        try
        {
            var filePath = await _backupService.GetBackupFilePathAsync(backup.Id.ToString());
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                MessageBox.Show(
                    "Le fichier de sauvegarde n'a pas pu être trouvé.",
                    "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                FileName = Path.GetFileName(filePath),
                Filter = "ZIP Files (*.zip)|*.zip|All Files (*.*)|*.*",
                DefaultExt = "zip"
            };

            if (saveDialog.ShowDialog() == true)
            {
                File.Copy(filePath, saveDialog.FileName, overwrite: true);
                MessageBox.Show(
                    "Sauvegarde téléchargée avec succès.",
                    "Succès",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur lors du téléchargement: {ex.Message}",
                "Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async Task RestoreBackupAsync()
    {
        try
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "ZIP Files (*.zip)|*.zip|All Files (*.*)|*.*",
                DefaultExt = "zip"
            };

            if (openDialog.ShowDialog() == true)
            {
                var result = MessageBox.Show(
                    "La restauration va remplacer toutes les données actuelles. Êtes-vous sûr de vouloir continuer?",
                    "Attention",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    IsBackingUp = true;
                    StatusMessage = "Restauration de la sauvegarde en cours...";

                    var success = await _backupService.RestoreBackupAsync(openDialog.FileName);

                    if (success)
                    {
                        StatusMessage = "Restauration terminée avec succès";
                        MessageBox.Show(
                            "La sauvegarde a été restaurée avec succès. L'application va redémarrer.",
                            "Succès",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        // Restart application
                        System.Windows.Application.Current.Shutdown();
                        System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur lors de la restauration: {ex.Message}";
            MessageBox.Show(
                $"Erreur lors de la restauration: {ex.Message}",
                "Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsBackingUp = false;
        }
    }

    private async Task LoadBackupsAsync()
    {
        try
        {
            IsLoading = true;
            var backups = await _backupService.GetBackupHistoryAsync();
            
            Backups.Clear();
            foreach (var backup in backups)
            {
                Backups.Add(backup);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur lors du chargement des sauvegardes: {ex.Message}",
                "Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
