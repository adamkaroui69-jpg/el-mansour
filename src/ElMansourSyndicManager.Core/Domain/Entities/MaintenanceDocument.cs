using System;

namespace ElMansourSyndicManager.Core.Domain.Entities
{
    public class MaintenanceDocument : BaseEntity
    {
        public Guid MaintenanceId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? CloudStoragePath { get; set; }
        public long FileSize { get; set; }
        public string MimeType { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }
}
