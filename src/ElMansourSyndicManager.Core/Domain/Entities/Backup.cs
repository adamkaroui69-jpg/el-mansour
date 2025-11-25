using System;

namespace ElMansourSyndicManager.Core.Domain.Entities
{
    public class Backup : BaseEntity
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime BackupDate { get; set; } = DateTime.UtcNow;
        public string BackupType { get; set; } = "Manual"; // Full, Database, Documents
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public bool IsAutomatic { get; set; }
        public long FileSize { get; set; }
        public string? CloudStoragePath { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = "Success"; // Success, Failed, InProgress
    }
}
