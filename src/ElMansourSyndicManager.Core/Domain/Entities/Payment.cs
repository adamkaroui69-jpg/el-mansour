using System;

namespace ElMansourSyndicManager.Core.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public string HouseCode { get; set; } = string.Empty;
        public string Month { get; set; } = string.Empty; // YYYY-MM
        public decimal Amount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? ReferenceNumber { get; set; }
        public string Status { get; set; } = "Pending"; // Paid, Unpaid, Overdue
        public string GeneratedBy { get; set; } = string.Empty;
        public string RecordedBy { get; set; } = string.Empty;
    }
}
