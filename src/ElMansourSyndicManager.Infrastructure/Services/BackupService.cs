using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Entities;
using ElMansourSyndicManager.Core.Domain.Exceptions;
using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

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

        // Setup backup directories
        _backupsBasePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "data",
            "backups");

        _databasePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "data",
            "local.db");

        _receiptsPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "data",
            "Receipts");

        _documentsPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "data",
            "Documents");

        _reportsPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "data",
            "reports");

        // Ensure directories exist
        Directory.CreateDirectory(_backupsBasePath);
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

            // 4. Create encrypted archive
            var archivePath = Path.Combine(_backupsBasePath, $"{backupFolderName}.zip");
            await CreateEncryptedArchiveAsync(backupFolderPath, archivePath, cancellationToken);

            // 5. Calculate file size
            var fileInfo = new FileInfo(archivePath);
            backup.FileSize = fileInfo.Length;
            backup.FilePath = archivePath;
            backup.CloudStoragePath = null; // Cloud storage upload is not supported as IDocumentService was removed.
            _logger.LogInformation("Cloud storage upload skipped as IDocumentService was removed.");

            // 7. Save backup record
            var savedBackup = await _backupRepository.CreateAsync(backup);

            // 8. Log audit
            await _auditService.LogActivityAsync(new AuditLogDto
            {
                UserId = _authService.CurrentUser?.Id.ToString(),
                Action = "Create",
                EntityType = "Backup",
                EntityId = savedBackup.Id.ToString(), // Convertir Guid en string
                Details = $"{{\"type\":\"{backup.BackupType}\",\"size\":{backup.FileSize},\"automatic\":{isAutomatic}}}"
            }, cancellationToken);

            // 9. Send success notification
            await _notificationService.CreateNotificationAsync(new NotificationDTO
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
                await _notificationService.CreateNotificationAsync(new NotificationDTO
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
                await ExtractEncryptedArchiveAsync(backupFilePath, tempExtractPath, cancellationToken);

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
            await Task.Run(() => File.Copy(_databasePath, dbBackupPath, overwrite: true), cancellationToken);

            _logger.LogInformation("Database backed up to {BackupPath}", dbBackupPath);
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
    /// Creates encrypted ZIP archive
    /// </summary>
    private async Task CreateEncryptedArchiveAsync(
        string sourceFolder, 
        string archivePath, 
        CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            using var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create);
            
            foreach (var file in Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(sourceFolder, file);
                archive.CreateEntryFromFile(file, relativePath);
            }
        }, cancellationToken);

        // Encrypt the archive
        await EncryptFileAsync(archivePath, cancellationToken);
    }

    /// <summary>
    /// Encrypts a file using AES
    /// </summary>
    private async Task EncryptFileAsync(string filePath, CancellationToken cancellationToken)
    {
        // Get encryption key from settings or generate one
        var key = GetEncryptionKey();
        
        await Task.Run(() =>
        {
            var encryptedPath = filePath + ".encrypted";
            
            using (var inputFile = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var outputFile = new FileStream(encryptedPath, FileMode.Create, FileAccess.Write))
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();

                // Write IV to beginning of file
                outputFile.Write(aes.IV, 0, aes.IV.Length);

                using (var cryptoStream = new CryptoStream(outputFile, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    inputFile.CopyTo(cryptoStream);
                }
            }

            // Replace original with encrypted
            File.Delete(filePath);
            File.Move(encryptedPath, filePath);
        }, cancellationToken);
    }

    /// <summary>
    /// Extracts encrypted archive
    /// </summary>
    private async Task ExtractEncryptedArchiveAsync(
        string archivePath, 
        string extractPath, 
        CancellationToken cancellationToken)
    {
        // Decrypt first
        var decryptedPath = archivePath + ".decrypted";
        await DecryptFileAsync(archivePath, decryptedPath, cancellationToken);

        try
        {
            await Task.Run(() =>
            {
                ZipFile.ExtractToDirectory(decryptedPath, extractPath);
            }, cancellationToken);
        }
        finally
        {
            // Cleanup decrypted file
            if (File.Exists(decryptedPath))
            {
                File.Delete(decryptedPath);
            }
        }
    }

    /// <summary>
    /// Decrypts a file using AES
    /// </summary>
    private async Task DecryptFileAsync(
        string encryptedPath, 
        string decryptedPath, 
        CancellationToken cancellationToken)
    {
        var key = GetEncryptionKey();
        
        await Task.Run(() =>
        {
            using (var inputFile = new FileStream(encryptedPath, FileMode.Open, FileAccess.Read))
            using (var outputFile = new FileStream(decryptedPath, FileMode.Create, FileAccess.Write))
            using (var aes = Aes.Create())
            {
                aes.Key = key;

                // Read IV from beginning of file
                var iv = new byte[aes.IV.Length];
                inputFile.Read(iv, 0, iv.Length);
                aes.IV = iv;

                using (var cryptoStream = new CryptoStream(inputFile, aes.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    cryptoStream.CopyTo(outputFile);
                }
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Gets encryption key (from settings or generates one)
    /// </summary>
    private byte[] GetEncryptionKey()
    {
        // In production, get from secure settings
        // For now, use a fixed key (should be stored securely)
        var keyString = "ElMansourSyndicManager2024BackupKey!";
        return SHA256.HashData(Encoding.UTF8.GetBytes(keyString));
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
