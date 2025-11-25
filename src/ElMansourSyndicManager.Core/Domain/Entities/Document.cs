using System;

namespace ElMansourSyndicManager.Core.Domain.Entities
{
    public class Document : BaseEntity
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty; // Local path
        public string? CloudPath { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string Category { get; set; } = "General"; // General, Legal, Financial, etc.
        public string UploadedBy { get; set; } = string.Empty; // User ID
        public string? Description { get; set; }
    }
}
