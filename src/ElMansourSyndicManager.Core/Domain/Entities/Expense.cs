using System;

namespace ElMansourSyndicManager.Core.Domain.Entities
{
    public class Expense : BaseEntity
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string Category { get; set; } = string.Empty;
        public string RecordedBy { get; set; } = string.Empty; // User ID
        public Guid? MaintenanceId { get; set; }
        public string? Notes { get; set; }
    }
}
