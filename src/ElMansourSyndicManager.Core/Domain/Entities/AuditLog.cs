using System;

namespace ElMansourSyndicManager.Core.Domain.Entities
{
    /// <summary>
    /// Enregistre toutes les actions importantes des utilisateurs
    /// Principe : Qui a fait Quoi, Quand, Où et Pourquoi
    /// </summary>
    public class AuditLog : BaseEntity
    {
        // QUI (Who)
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        
        // QUOI (What)
        public string Action { get; set; } = string.Empty; // Create, Update, Delete, View, Export, Login, Logout
        public string EntityType { get; set; } = string.Empty; // Payment, House, User, Document, etc.
        public string EntityId { get; set; } = string.Empty;
        public string? OldValues { get; set; } // JSON des anciennes valeurs (pour Update)
        public string? NewValues { get; set; } // JSON des nouvelles valeurs
        public string Details { get; set; } = string.Empty; // Description lisible
        
        // QUAND (When)
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        // OÙ (Where)
        public string? IpAddress { get; set; }
        public string? MachineName { get; set; }
        public string? UserAgent { get; set; }
        
        // CONTEXTE
        public string Severity { get; set; } = "Info"; // Info, Warning, Error, Critical
        public bool IsSuccessful { get; set; } = true;
        public string? ErrorMessage { get; set; }
        
        // MÉTADONNÉES
        public string? RequestId { get; set; } // Pour tracer une transaction complète
        public int DurationMs { get; set; } // Durée de l'opération
    }
}
