using ElMansourSyndicManager.Core.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ElMansourSyndicManager.Core.Domain.Interfaces.Services;

/// <summary>
/// Service d'audit pour enregistrer toutes les actions importantes
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Enregistre une action d'audit
    /// </summary>
    Task LogAsync(
        string action,
        string entityType,
        string entityId,
        string details,
        string? oldValues = null,
        string? newValues = null,
        string severity = "Info",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enregistre une action d'audit avec mesure de performance
    /// </summary>
    Task<T> LogWithTimingAsync<T>(
        string action,
        string entityType,
        string entityId,
        Func<Task<T>> operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enregistre une connexion réussie
    /// </summary>
    Task LogLoginAsync(string userId, string username, bool success, string? errorMessage = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enregistre une déconnexion
    /// </summary>
    Task LogLogoutAsync(string userId, string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enregistre une erreur critique
    /// </summary>
    Task LogCriticalErrorAsync(string action, string details, Exception? exception = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère les logs d'audit avec filtres
    /// </summary>
    Task<List<AuditLog>> GetAuditLogsAsync(
        DateTime? from = null,
        DateTime? to = null,
        string? userId = null,
        string? action = null,
        string? entityType = null,
        string? severity = null,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère les logs pour une entité spécifique
    /// </summary>
    Task<List<AuditLog>> GetEntityHistoryAsync(string entityType, string entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enregistre une activité (méthode legacy pour compatibilité)
    /// </summary>
    Task LogActivityAsync(object auditLogDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Nettoie les anciens logs (rétention)
    /// </summary>
    Task CleanOldLogsAsync(int retentionDays = 90, CancellationToken cancellationToken = default);
}
