using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Entities;
using ElMansourSyndicManager.Core.Domain.Exceptions;
using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using ElMansourSyndicManager.Core.Configuration;

namespace ElMansourSyndicManager.Infrastructure.Services;

/// <summary>
/// Service for managing backups with encryption and cloud upload
/// </summary>
public class BackupService : IBackupService
{
    private readonly IBackupRepository _backupRepository;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;
    private readonly IAuthenticationService _authService;
    private readonly ILogger<BackupService> _logger;
    private readonly string _backupsBasePath;
    private readonly string _databasePath;
    private readonly string _receiptsPath;
    private readonly string _documentsPath;
    private readonly string _reportsPath;
    private Timer? _scheduledBackupTimer;
    private bool _scheduledBackupsEnabled;
    private TimeSpan _scheduledBackupTime = new TimeSpan(2, 0, 0); // Default: 2 AM


    public BackupService(
        IBackupRepository backupRepository,
        INotificationService notificationService,
        IAuditService auditService,
        IAuthenticationService authService,
        ILogger<BackupService> logger)
    {
        _backupRepository = backupRepository;
        _notificationService = notificationService;
        _auditService = auditService;
        _authService = authService;
        _logger = logger;

        var config = AppConfiguration.Instance;

        // Setup backup directories from AppConfiguration
        _backupsBasePath = config.BackupsDirectory;
        _databasePath = config.GetDatabasePath();
        
        // Construct other paths relative to DataDirectory
        // Assuming Receipts/Documents/Reports are subfolders of DataDirectory
        // NOTE: DocumentService uses config.DocumentsDirectory for Documents.
        
        _receiptsPath = Path.Combine(config.DataDirectory, "Receipts");
        _documentsPath = config.DocumentsDirectory;
        _reportsPath = Path.Combine(config.DataDirectory, "reports");

        // Ensure directories exist
        Directory.CreateDirectory(_backupsBasePath);
        Directory.CreateDirectory(_receiptsPath);
        Directory.CreateDirectory(_documentsPath);
        Directory.CreateDirectory(_reportsPath);
    }

    /// <summary>
    /// Runs a full backup immediately
    /// </summary>
    public async Task<BackupHistoryDTO> RunBackupAsync(
        bool isAutomatic = false,
        CancellationToken cancellationToken = default)
    {
        var backupId = Guid.NewGuid();
        var backupTimestamp = DateTime.UtcNow;
        var backupFolderName = backupTimestamp.ToString("yyyy-MM-dd_HHmmss");
        var backupFolderPath = Path.Combine(_backupsBasePath, backupFolderName);

        try
        {
            _logger.LogInformation("Starting backup {BackupId} (Automatic: {IsAutomatic})", backupId, isAutomatic);

            Directory.CreateDirectory(backupFolderPath);

            // Create backup entity
            var backup = new Backup
            {
                Id = backupId, // Id est maintenant un Guid
                BackupType = "Full",
                FilePath = backupFolderPath,
                CreatedBy = _authService.CurrentUser?.Id.ToString() ?? "System",
                IsAutomatic = isAutomatic,
                CreatedAt = backupTimestamp,
                UpdatedAt = backupTimestamp
            };

            // 1. Backup database
            await BackupDatabaseAsync(backupFolderPath, cancellationToken);

            // 2. Backup files (receipts, documents, reports)
            await BackupFilesAsync(backupFolderPath, cancellationToken);

            // 3. Create metadata file
            var metadataPath = Path.Combine(backupFolderPath, "metadata.json");
            await CreateMetadataFileAsync(metadataPath, backup, cancellationToken);

            // 4. Create archive (Standard ZIP)
            var archivePath = Path.Combine(_backupsBasePath, $"{backupFolderName}.zip");
            await CreateArchiveAsync(backupFolderPath, archivePath, cancellationToken);

            // 5. Calculate file size
            var fileInfo = new FileInfo(archivePath);
            backup.FileSize = fileInfo.Length;
            backup.FilePath = archivePath;
            backup.CloudStoragePath = null;
            _logger.LogInformation("Cloud storage upload skipped as IDocumentService was removed.");

            // 7. Save backup record
            var savedBackup = await _backupRepository.CreateAsync(backup);

            // 8. Log audit
            await _auditService.LogActivityAsync(new AuditLogDto
            {
                UserId = _authService.CurrentUser?.Id.ToString(),
                Action = "Create",
                EntityType = "Backup",
                EntityId = savedBackup.Id.ToString(), 
                Details = $"{{\"type\":\"{backup.BackupType}\",\"size\":{backup.FileSize},\"automatic\":{isAutomatic}}}"
            }, cancellationToken);

            // 9. Send success notification
            await _notificationService.CreateNotificationAsync(new NotificationDto
            {
                UserId = _authService.CurrentUser?.Id.ToString(),
                Type = "System",
                Title = "Sauvegarde Réussie",
                Message = $"La sauvegarde a été créée avec succès. Taille: {FormatFileSize(backup.FileSize)}",
                Priority = "Normal"
            });

            _logger.LogInformation("Backup {BackupId} completed successfully. Size: {Size} bytes", backupId, backup.FileSize);

            return MapToDto(savedBackup);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating backup {BackupId}", backupId);

            // Send failure notification
            try
            {
                await _notificationService.CreateNotificationAsync(new NotificationDto
                {
                    UserId = _authService.CurrentUser?.Id.ToString(),
                    Type = "System",
                    Title = "Échec de la Sauvegarde",
                    Message = $"La sauvegarde a échoué: {ex.Message}",
                    Priority = "High"
                });
            }
            catch
            {
                // Ignore notification errors
            }

            // Cleanup on failure
            if (Directory.Exists(backupFolderPath))
            {
                try
                {
                    Directory.Delete(backupFolderPath, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }

            throw;
        }
    }

    /// <summary>
    /// Gets backup history
    /// </summary>
    public async Task<List<BackupHistoryDTO>> GetBackupHistoryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var backups = await _backupRepository.GetAllAsync();
            return backups
                .OrderByDescending(b => b.CreatedAt)
                .Select(MapToDto)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting backup history");
            throw;
        }
    }

    /// <summary>
    /// Deletes old backups, keeping only the last N backups
    /// </summary>
    public async Task DeleteOldBackupsAsync(
        int keepLastN, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var backups = await _backupRepository.GetAllAsync();
            var sortedBackups = backups
                .OrderByDescending(b => b.CreatedAt)
                .ToList();

            var backupsToDelete = sortedBackups.Skip(keepLastN).ToList();

            foreach (var backup in backupsToDelete)
            {
                // Delete local file
                if (File.Exists(backup.FilePath))
                {
                    try
                    {
                        File.Delete(backup.FilePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete backup file: {FilePath}", backup.FilePath);
                    }
            }

            // Delete from database
            await _backupRepository.DeleteAsync(backup);
            }

            _logger.LogInformation("Deleted {Count} old backups, kept {KeepCount}", backupsToDelete.Count, keepLastN);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting old backups");
            throw;
        }
    }

    /// <summary>
    /// Triggers scheduled backup (called by cron/scheduler)
    /// </summary>
    public async Task TriggerScheduledBackupAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Triggering scheduled backup");
            await RunBackupAsync(isAutomatic: true, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in scheduled backup");
            throw;
        }
    }

    /// <summary>
    /// Restores a backup from file
    /// </summary>
    public async Task<bool> RestoreBackupAsync(
        string backupFilePath, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(backupFilePath))
                throw new FileNotFoundException("Backup file not found", backupFilePath);

            _logger.LogInformation("Starting restore from {BackupPath}", backupFilePath);

            // Extract encrypted archive
            var tempExtractPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempExtractPath);

            try
            {
                await ExtractArchiveAsync(backupFilePath, tempExtractPath, cancellationToken);

                // Restore database
                var dbBackupPath = Path.Combine(tempExtractPath, "database.db");
                if (File.Exists(dbBackupPath))
                {
                    // Close current database connection
                    // Then copy backup over current database
                    File.Copy(dbBackupPath, _databasePath, overwrite: true);
                }

                // Restore files
                var filesBackupPath = Path.Combine(tempExtractPath, "files");
                if (Directory.Exists(filesBackupPath))
                {
                    // Restore receipts, documents, reports
                    await RestoreFilesAsync(filesBackupPath, cancellationToken);
                }

                _logger.LogInformation("Restore completed successfully");
                return true;
            }
            finally
            {
                // Cleanup temp directory
                if (Directory.Exists(tempExtractPath))
                {
                    Directory.Delete(tempExtractPath, true);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring backup");
            throw;
        }
    }

    /// <summary>
    /// Deletes a specific backup
    /// </summary>
    public async Task<bool> DeleteBackupAsync(
        string backupId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var backup = await _backupRepository.GetByIdAsync(Guid.Parse(backupId));
            if (backup == null)
                return false;

            // Delete local file
            if (File.Exists(backup.FilePath))
            {
                File.Delete(backup.FilePath);
            }

            // Delete from database
            await _backupRepository.DeleteAsync(backup); // Correction: Passer l'entité, pas l'ID et cancellationToken

            _logger.LogInformation("Backup {BackupId} deleted", backupId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting backup");
            throw;
        }
    }

    /// <summary>
    /// Gets backup file path for download
    /// </summary>
    public async Task<string?> GetBackupFilePathAsync(
        string backupId, 
        CancellationToken cancellationToken = default)
    {
        var backup = await _backupRepository.GetByIdAsync(Guid.Parse(backupId));
        if (backup == null)
            return null;

        if (File.Exists(backup.FilePath))
            return backup.FilePath;

        return null;
    }

    /// <summary>
    /// Configures scheduled backups
    /// </summary>
    public async Task ScheduleBackupsAsync(
        bool enabled, 
        TimeSpan? time = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _scheduledBackupsEnabled = enabled;
            if (time.HasValue)
            {
                _scheduledBackupTime = time.Value;
            }

            // Stop existing timer
            _scheduledBackupTimer?.Dispose();
            _scheduledBackupTimer = null;

            if (enabled)
            {
                // Calculate next backup time
                var now = DateTime.Now;
                var nextBackup = now.Date.Add(_scheduledBackupTime);
                if (nextBackup <= now)
                {
                    nextBackup = nextBackup.AddDays(1);
                }

                var delay = nextBackup - now;

                // Create timer for scheduled backups
                _scheduledBackupTimer = new Timer(async _ =>
                {
                    try
                    {
                        await TriggerScheduledBackupAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in scheduled backup timer");
                    }
                }, null, delay, TimeSpan.FromDays(1)); // Run daily

                _logger.LogInformation("Scheduled backups enabled. Next backup at {NextBackup}", nextBackup);
            }
            else
            {
                _logger.LogInformation("Scheduled backups disabled");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring scheduled backups");
            throw;
        }
    }

    #region Private Methods

    /// <summary>
    /// Backs up the SQLite database
    /// </summary>
    /// <summary>
    /// Backs up the SQLite database ensuring safeguards for WAL mode
    /// </summary>
    private async Task BackupDatabaseAsync(string backupFolderPath, CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(_databasePath))
            {
                _logger.LogWarning("Database file not found at {DatabasePath}", _databasePath);
                return;
            }

            var dbBackupPath = Path.Combine(backupFolderPath, "database.db");

            // Pour SQLite en mode WAL, il est préférable de copier aussi les fichiers wal et shm s'ils existent
            // Ou de forcer un checkpoint. Ici, nous copions simplement les fichiers auxiliaires si présents.
            
            await Task.Run(() => 
            {
                File.Copy(_databasePath, dbBackupPath, overwrite: true);
                
                string walPath = _databasePath + "-wal";
                string shmPath = _databasePath + "-shm";

                if (File.Exists(walPath)) File.Copy(walPath, dbBackupPath + "-wal", overwrite: true);
                if (File.Exists(shmPath)) File.Copy(shmPath, dbBackupPath + "-shm", overwrite: true);

            }, cancellationToken);

            _logger.LogInformation("Database backed up to {BackupPath} (including WAL/SHM if present)", dbBackupPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error backing up database");
            throw;
        }
    }

    /// <summary>
    /// Backs up files (receipts, documents, reports)
    /// </summary>
    private async Task BackupFilesAsync(string backupFolderPath, CancellationToken cancellationToken)
    {
        try
        {
            var filesBackupPath = Path.Combine(backupFolderPath, "files");
            Directory.CreateDirectory(filesBackupPath);

            // Backup receipts
            if (Directory.Exists(_receiptsPath))
            {
                var receiptsBackup = Path.Combine(filesBackupPath, "Receipts");
                await CopyDirectoryAsync(_receiptsPath, receiptsBackup, cancellationToken);
            }

            // Backup documents
            if (Directory.Exists(_documentsPath))
            {
                var documentsBackup = Path.Combine(filesBackupPath, "Documents");
                await CopyDirectoryAsync(_documentsPath, documentsBackup, cancellationToken);
            }

            // Backup reports
            if (Directory.Exists(_reportsPath))
            {
                var reportsBackup = Path.Combine(filesBackupPath, "reports");
                await CopyDirectoryAsync(_reportsPath, reportsBackup, cancellationToken);
            }

            _logger.LogInformation("Files backed up to {BackupPath}", filesBackupPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error backing up files");
            throw;
        }
    }

    /// <summary>
    /// Creates metadata file for backup
    /// </summary>
    private async Task CreateMetadataFileAsync(
        string metadataPath, 
        Backup backup, 
        CancellationToken cancellationToken)
    {
        var metadata = new
        {
            BackupId = backup.Id.ToString(),
            BackupType = backup.BackupType,
            CreatedAt = backup.CreatedAt,
            CreatedBy = backup.CreatedBy,
            IsAutomatic = backup.IsAutomatic,
            Version = "1.0",
            Application = "El Mansour Syndic Manager"
        };

        var json = System.Text.Json.JsonSerializer.Serialize(metadata, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(metadataPath, json, cancellationToken);
    }

    /// <summary>
    /// Creates standard ZIP archive
    /// </summary>
    private async Task CreateArchiveAsync(
        string sourceFolder, 
        string archivePath, 
        CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            if (File.Exists(archivePath)) File.Delete(archivePath);
            ZipFile.CreateFromDirectory(sourceFolder, archivePath);
        }, cancellationToken);
    }

    /// <summary>
    /// Extracts standard ZIP archive
    /// </summary>
    private async Task ExtractArchiveAsync(
        string archivePath, 
        string extractPath, 
        CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            ZipFile.ExtractToDirectory(archivePath, extractPath);
        }, cancellationToken);
    }


    /// <summary>
    /// Copies directory recursively
    /// </summary>
    private async Task CopyDirectoryAsync(
        string sourceDir, 
        string destDir, 
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var fileName = Path.GetFileName(file);
            var destFile = Path.Combine(destDir, fileName);
            await Task.Run(() => File.Copy(file, destFile, overwrite: true), cancellationToken);
        }

        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            var dirName = Path.GetFileName(subDir);
            var destSubDir = Path.Combine(destDir, dirName);
            await CopyDirectoryAsync(subDir, destSubDir, cancellationToken);
        }
    }

    /// <summary>
    /// Restores files from backup
    /// </summary>
    private async Task RestoreFilesAsync(string filesBackupPath, CancellationToken cancellationToken)
    {
        // Restore receipts
        var receiptsBackup = Path.Combine(filesBackupPath, "Receipts");
        if (Directory.Exists(receiptsBackup))
        {
            await CopyDirectoryAsync(receiptsBackup, _receiptsPath, cancellationToken);
        }

        // Restore documents
        var documentsBackup = Path.Combine(filesBackupPath, "Documents");
        if (Directory.Exists(documentsBackup))
        {
            await CopyDirectoryAsync(documentsBackup, _documentsPath, cancellationToken);
        }

        // Restore reports
        var reportsBackup = Path.Combine(filesBackupPath, "reports");
        if (Directory.Exists(reportsBackup))
        {
            await CopyDirectoryAsync(reportsBackup, _reportsPath, cancellationToken);
        }
    }

    /// <summary>
    /// Maps Backup entity to DTO
    /// </summary>
    private BackupHistoryDTO MapToDto(Backup backup)
    {
        return new BackupHistoryDTO
        {
            Id = backup.Id,
            BackupType = backup.BackupType,
            FilePath = backup.FilePath,
            CloudStoragePath = backup.CloudStoragePath,
            FileSize = backup.FileSize,
            CreatedBy = backup.CreatedBy,
            CreatedAt = backup.CreatedAt,
            ExpiresAt = backup.ExpiresAt,
            IsAutomatic = backup.IsAutomatic,
            Notes = backup.Notes,
            Status = "Success"
        };
    }

    /// <summary>
    /// Formats file size to human-readable string
    /// </summary>
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

    #endregion
}
