using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Entities; // Utiliser les entités
using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ElMansourSyndicManager.Infrastructure.Services;

/// <summary>
/// Service for audit logging
/// </summary>
public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IAuditLogRepository auditLogRepository,
        ILogger<AuditService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    public async Task LogActivityAsync(
        AuditLogDto logDto, // Changé log en logDto
        CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = new AuditLog
            {
                // Id est généré par BaseEntity
                UserId = logDto.UserId ?? "System",
                Action = logDto.Action,
                EntityType = logDto.EntityType,
                EntityId = logDto.EntityId ?? string.Empty,
                Details = logDto.Details ?? string.Empty,
                IpAddress = logDto.IpAddress ?? "Unknown",
                UserAgent = logDto.UserAgent ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _auditLogRepository.CreateAsync(auditLog);
        }
        catch (Exception ex)
        {
            // Don't fail the operation if audit logging fails
            _logger.LogError(ex, "Failed to log audit activity: {Action}", logDto.Action);
        }
    }

    public async Task<List<AuditLogDto>> GetAuditLogsAsync(
        DateTime? from = null, 
        DateTime? to = null, 
        string? userId = null, 
        CancellationToken cancellationToken = default)
    {
        var logs = await _auditLogRepository.GetByDateRangeAsync(from, to);
        
        if (!string.IsNullOrEmpty(userId))
        {
            logs = logs.Where(l => l.UserId == userId).ToList();
        }

        return logs.Select(MapToDto).ToList(); // Le type de logs est maintenant List<Entities.AuditLog>
    }

    public async Task<List<AuditLogDto>> GetAuditLogsByEntityAsync(
        string entityType, 
        string entityId, 
        CancellationToken cancellationToken = default)
    {
        var logs = await _auditLogRepository.GetByEntityAsync(entityType, entityId);
        return logs.Select(MapToDto).ToList(); // Le type de logs est maintenant List<Entities.AuditLog>
    }

    public async Task<byte[]> ExportAuditLogsAsync(
        DateTime from, 
        DateTime to, 
        CancellationToken cancellationToken = default)
    {
        var logs = await _auditLogRepository.GetByDateRangeAsync(from, to);
        var json = JsonSerializer.Serialize(logs.Select(MapToDto), new JsonSerializerOptions { WriteIndented = true }); // Le type de logs est maintenant List<Entities.AuditLog>
        return Encoding.UTF8.GetBytes(json);
    }

    #region Private Methods

    private string GetLocalIpAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
        }
        catch { }
        return "Unknown";
    }

    private AuditLogDto MapToDto(AuditLog log)
    {
        return new AuditLogDto
        {
            Id = log.Id,
            UserId = log.UserId ?? "System",
            Action = log.Action,
            EntityType = log.EntityType,
            EntityId = log.EntityId,
            Details = log.Details ?? string.Empty,
            IpAddress = log.IpAddress ?? "Unknown",
            UserAgent = log.UserAgent ?? string.Empty,
            Timestamp = log.CreatedAt
        };
    }

    #endregion
}
