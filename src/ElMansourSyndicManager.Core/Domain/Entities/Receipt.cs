using System;

namespace ElMansourSyndicManager.Core.Domain.Entities
{
    public class Receipt : BaseEntity
    {
        public Guid PaymentId { get; set; } // Foreign key to Payment
        public string ReceiptNumber { get; set; } = string.Empty;
        public DateTime? ReceiptDate { get; set; }
        public DateTime GeneratedDate { get; set; } // Requis par la DB
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string GeneratedBy { get; set; } = string.Empty; // User ID
        public string FilePath { get; set; } = string.Empty;
        public string? CloudStoragePath { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string MimeType { get; set; } = "application/pdf";
    }
}
