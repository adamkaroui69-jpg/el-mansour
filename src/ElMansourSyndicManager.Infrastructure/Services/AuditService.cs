using ElMansourSyndicManager.Core.Domain.Entities;
using ElMansourSyndicManager.Core.Domain.Enums;
using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ElMansourSyndicManager.Infrastructure.Services;

/// <summary>
/// Implémentation du service d'audit
/// Enregistre automatiquement toutes les actions importantes
/// </summary>
public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditRepository;
    private readonly IAuthenticationService _authService;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IAuditLogRepository auditRepository,
        IAuthenticationService authService,
        ILogger<AuditService> logger)
    {
        _auditRepository = auditRepository;
        _authService = authService;
        _logger = logger;
    }

    public async Task LogAsync(
        string action,
        string entityType,
        string entityId,
        string details,
        string? oldValues = null,
        string? newValues = null,
        string severity = "Info",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUser = _authService.CurrentUser;
            
            var auditLog = new AuditLog
            {
                UserId = currentUser?.Id.ToString() ?? "System",
                Username = currentUser?.Username ?? "System",
                UserRole = currentUser?.Role ?? "System",
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                OldValues = oldValues,
                NewValues = newValues,
                Severity = severity,
                Timestamp = DateTime.UtcNow,
                MachineName = Environment.MachineName,
                IsSuccessful = true
            };

            await _auditRepository.CreateAsync(auditLog, cancellationToken);
            
            // Log également dans Serilog pour les actions critiques
            if (severity == AuditSeverity.Critical || severity == AuditSeverity.Error)
            {
                _logger.LogWarning("Audit [{Severity}]: {Action} on {EntityType}#{EntityId} by {User}", 
                    severity, action, entityType, entityId, auditLog.Username);
            }
        }
        catch (Exception ex)
        {
            // Ne jamais faire échouer l'opération principale à cause de l'audit
            _logger.LogError(ex, "Failed to log audit entry for {Action} on {EntityType}", action, entityType);
        }
    }

    public async Task<T> LogWithTimingAsync<T>(
        string action,
        string entityType,
        string entityId,
        Func<Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        T result = default!;
        bool success = true;
        string? errorMessage = null;

        try
        {
            result = await operation();
            return result;
        }
        catch (Exception ex)
        {
            success = false;
            errorMessage = ex.Message;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            
            var currentUser = _authService.CurrentUser;
            var auditLog = new AuditLog
            {
                UserId = currentUser?.Id.ToString() ?? "System",
                Username = currentUser?.Username ?? "System",
                UserRole = currentUser?.Role ?? "System",
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = $"{action} on {entityType} - Duration: {stopwatch.ElapsedMilliseconds}ms",
                Severity = success ? AuditSeverity.Info : AuditSeverity.Error,
                IsSuccessful = success,
                ErrorMessage = errorMessage,
                DurationMs = (int)stopwatch.ElapsedMilliseconds,
                Timestamp = DateTime.UtcNow,
                MachineName = Environment.MachineName
            };

            try
            {
                await _auditRepository.CreateAsync(auditLog, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log timed audit entry");
            }
        }
    }

    public async Task LogLoginAsync(string userId, string username, bool success, string? errorMessage = null, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            Username = username,
            UserRole = "Unknown", // Sera mis à jour après connexion réussie
            Action = success ? AuditAction.Login : AuditAction.LoginFailed,
            EntityType = AuditEntityType.User,
            EntityId = userId,
            Details = success ? $"Connexion réussie pour {username}" : $"Échec de connexion pour {username}: {errorMessage}",
            Severity = success ? AuditSeverity.Info : AuditSeverity.Warning,
            IsSuccessful = success,
            ErrorMessage = errorMessage,
            Timestamp = DateTime.UtcNow,
            MachineName = Environment.MachineName
        };

        await _auditRepository.CreateAsync(auditLog, cancellationToken);
        
        if (!success)
        {
            _logger.LogWarning("Failed login attempt for user {Username} from {Machine}", username, Environment.MachineName);
        }
    }

    public async Task LogLogoutAsync(string userId, string username, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            Username = username,
            UserRole = _authService.CurrentUser?.Role ?? "Unknown",
            Action = AuditAction.Logout,
            EntityType = AuditEntityType.User,
            EntityId = userId,
            Details = $"Déconnexion de {username}",
            Severity = AuditSeverity.Info,
            IsSuccessful = true,
            Timestamp = DateTime.UtcNow,
            MachineName = Environment.MachineName
        };

        await _auditRepository.CreateAsync(auditLog, cancellationToken);
    }

    public async Task LogCriticalErrorAsync(string action, string details, Exception? exception = null, CancellationToken cancellationToken = default)
    {
        var currentUser = _authService.CurrentUser;
        
        var auditLog = new AuditLog
        {
            UserId = currentUser?.Id.ToString() ?? "System",
            Username = currentUser?.Username ?? "System",
            UserRole = currentUser?.Role ?? "System",
            Action = action,
            EntityType = AuditEntityType.System,
            EntityId = Guid.NewGuid().ToString(),
            Details = details,
            Severity = AuditSeverity.Critical,
            IsSuccessful = false,
            ErrorMessage = exception?.ToString(),
            Timestamp = DateTime.UtcNow,
            MachineName = Environment.MachineName
        };

        await _auditRepository.CreateAsync(auditLog, cancellationToken);
        _logger.LogCritical(exception, "Critical error: {Details}", details);
    }

    public async Task<List<AuditLog>> GetAuditLogsAsync(
        DateTime? from = null,
        DateTime? to = null,
        string? userId = null,
        string? action = null,
        string? entityType = null,
        string? severity = null,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        var logs = await _auditRepository.GetByDateRangeAsync(from, to, cancellationToken);

        // Filtres
        if (!string.IsNullOrEmpty(userId))
            logs = logs.Where(l => l.UserId == userId).ToList();

        if (!string.IsNullOrEmpty(action))
            logs = logs.Where(l => l.Action == action).ToList();

        if (!string.IsNullOrEmpty(entityType))
            logs = logs.Where(l => l.EntityType == entityType).ToList();

        if (!string.IsNullOrEmpty(severity))
            logs = logs.Where(l => l.Severity == severity).ToList();

        return logs
            .OrderByDescending(l => l.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToList();
    }

    public async Task<List<AuditLog>> GetEntityHistoryAsync(string entityType, string entityId, CancellationToken cancellationToken = default)
    {
        var logs = await _auditRepository.GetByEntityAsync(entityType, entityId, cancellationToken);
        return logs.OrderByDescending(l => l.Timestamp).ToList();
    }

    public async Task LogActivityAsync(object auditLogDto, CancellationToken cancellationToken = default)
    {
        // Méthode legacy pour compatibilité avec l'ancien code
        // Convertir l'ancien DTO en nouvel appel
        try
        {
            var dto = auditLogDto as dynamic;
            await LogAsync(
                action: dto?.Action ?? "Unknown",
                entityType: dto?.EntityType ?? "Unknown",
                entityId: dto?.EntityId ?? "",
                details: dto?.Details ?? "",
                severity: "Info",
                cancellationToken: cancellationToken
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log legacy audit activity");
        }
    }

    public async Task CleanOldLogsAsync(int retentionDays = 90, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
        var oldLogs = await _auditRepository.GetByDateRangeAsync(null, cutoffDate, cancellationToken);

        foreach (var log in oldLogs)
        {
            await _auditRepository.DeleteAsync(log, cancellationToken);
        }

        _logger.LogInformation("Cleaned {Count} audit logs older than {Days} days", oldLogs.Count, retentionDays);
    }
}
