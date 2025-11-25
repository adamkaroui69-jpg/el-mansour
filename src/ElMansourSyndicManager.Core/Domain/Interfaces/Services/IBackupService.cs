using ElMansourSyndicManager.Core.Domain.DTOs;

namespace ElMansourSyndicManager.Core.Domain.Interfaces.Services;

/// <summary>
/// Service for managing backups (database, files, cloud sync)
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Runs a full backup immediately (database + files)
    /// </summary>
    Task<BackupHistoryDTO> RunBackupAsync(bool isAutomatic = false, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets backup history
    /// </summary>
    Task<List<BackupHistoryDTO>> GetBackupHistoryAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes old backups, keeping only the last N backups
    /// </summary>
    Task DeleteOldBackupsAsync(int keepLastN, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Triggers scheduled backup (called by cron/scheduler)
    /// </summary>
    Task TriggerScheduledBackupAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Restores a backup from file
    /// </summary>
    Task<bool> RestoreBackupAsync(string backupFilePath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a specific backup
    /// </summary>
    Task<bool> DeleteBackupAsync(string backupId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets backup file path for download
    /// </summary>
    Task<string?> GetBackupFilePathAsync(string backupId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Configures scheduled backups
    /// </summary>
    Task ScheduleBackupsAsync(bool enabled, TimeSpan? time = null, CancellationToken cancellationToken = default);
}
