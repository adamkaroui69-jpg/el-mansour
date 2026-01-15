namespace ElMansourSyndicManager.Core.Domain.Enums;

/// <summary>
/// Types d'actions auditées dans le système
/// </summary>
public static class AuditAction
{
    // Authentification
    public const string Login = "Login";
    public const string Logout = "Logout";
    public const string LoginFailed = "LoginFailed";
    public const string PasswordChanged = "PasswordChanged";
    
    // CRUD Générique
    public const string Create = "Create";
    public const string Read = "Read";
    public const string Update = "Update";
    public const string Delete = "Delete";
    
    // Actions Métier
    public const string PaymentReceived = "PaymentReceived";
    public const string PaymentCancelled = "PaymentCancelled";
    public const string ReceiptGenerated = "ReceiptGenerated";
    public const string ReceiptSent = "ReceiptSent";
    public const string ReportExported = "ReportExported";
    public const string BackupCreated = "BackupCreated";
    public const string BackupRestored = "BackupRestored";
    public const string DocumentUploaded = "DocumentUploaded";
    public const string DocumentDeleted = "DocumentDeleted";
    public const string SettingsChanged = "SettingsChanged";
    
    // Actions Sensibles
    public const string UserCreated = "UserCreated";
    public const string UserDeleted = "UserDeleted";
    public const string UserRoleChanged = "UserRoleChanged";
    public const string DatabaseMigrated = "DatabaseMigrated";
}

/// <summary>
/// Types d'entités auditées
/// </summary>
public static class AuditEntityType
{
    public const string User = "User";
    public const string Payment = "Payment";
    public const string House = "House";
    public const string Receipt = "Receipt";
    public const string Document = "Document";
    public const string Expense = "Expense";
    public const string Maintenance = "Maintenance";
    public const string Report = "Report";
    public const string Backup = "Backup";
    public const string Settings = "Settings";
    public const string System = "System";
}

/// <summary>
/// Niveaux de sévérité des événements d'audit
/// </summary>
public static class AuditSeverity
{
    public const string Info = "Info";         // Actions normales
    public const string Warning = "Warning";   // Actions inhabituelles mais valides
    public const string Error = "Error";       // Erreurs récupérables
    public const string Critical = "Critical"; // Erreurs critiques, violations de sécurité
}
