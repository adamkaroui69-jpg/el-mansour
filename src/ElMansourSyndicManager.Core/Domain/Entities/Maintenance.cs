using System;
using System.Collections.Generic; // Ajouté pour List

namespace ElMansourSyndicManager.Core.Domain.Entities
{
    public class Maintenance : BaseEntity
    {
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Ajouté Type
        public decimal Cost { get; set; } // Ajouté Cost
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed
        public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent
        public string CreatedBy { get; set; } = string.Empty; // Ajouté CreatedBy
        public string? AssignedTo { get; set; } // User ID
        public DateTime? ScheduledDate { get; set; }
        public DateTime? StartedAt { get; set; } // Ajouté StartedAt
        public DateTime? CompletedAt { get; set; } // Changé CompletionDate en CompletedAt
        public string? ReportedBy { get; set; } // User ID
        public string? Notes { get; set; } // Ajouté Notes
        public List<MaintenanceDocument> Documents { get; set; } = new(); // Ajouté Documents
    }
}
