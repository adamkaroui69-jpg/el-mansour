namespace ElMansourSyndicManager.Core.Domain.Constants;

/// <summary>
/// Définit toutes les permissions disponibles dans l'application.
/// Ces chaînes sont stockées en base de données pour associer des droits aux rôles.
/// </summary>
public static class AppPermissions
{
    // Module : Paiements
    public static class Payments
    {
        public const string View = "Payments.View";
        public const string Create = "Payments.Create";
        public const string Edit = "Payments.Edit";
        public const string Delete = "Payments.Delete";
        public const string Validate = "Payments.Validate"; // Pour valider un paiement en attente
    }

    // Module : Dépenses
    public static class Expenses
    {
        public const string View = "Expenses.View";
        public const string Create = "Expenses.Create";
        public const string Edit = "Expenses.Edit";
        public const string Delete = "Expenses.Delete";
    }

    // Module : Utilisateurs / Résidents / Propriétaires
    public static class Users
    {
        public const string View = "Users.View";
        public const string Create = "Users.Create";
        public const string Edit = "Users.Edit";
        public const string Delete = "Users.Delete";
        public const string ManageRoles = "Users.ManageRoles"; // Droit critique : changer les rôles
    }

    // Module : Rapports
    public static class Reports
    {
        public const string View = "Reports.View";
        public const string Export = "Reports.Export";
    }

    // Module : Documents
    public static class Documents
    {
        public const string View = "Documents.View";
        public const string Upload = "Documents.Upload";
        public const string Delete = "Documents.Delete";
    }

    // Module : Système
    public static class System
    {
        public const string ViewAuditLogs = "System.ViewAuditLogs";
        public const string ManageSettings = "System.ManageSettings";
        public const string ManageBackups = "System.ManageBackups";
    }

    /// <summary>
    /// Retourne la liste complète de toutes les permissions pour l'initialisation.
    /// </summary>
    public static IEnumerable<string> GetAll()
    {
        var permissions = new List<string>();
        foreach (var type in typeof(AppPermissions).GetNestedTypes())
        {
            foreach (var field in type.GetFields())
            {
                if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                {
                    permissions.Add((string)field.GetValue(null)!);
                }
            }
        }
        return permissions;
    }
}
