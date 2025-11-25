using ElMansourSyndicManager.Core.Domain.DTOs;

namespace ElMansourSyndicManager.Core.Domain.Interfaces.Services;

public interface IAuditService
{
    Task LogActivityAsync(AuditLogDto log, CancellationToken cancellationToken = default);
    Task<List<AuditLogDto>> GetAuditLogsAsync(DateTime? from = null, DateTime? to = null, string? userId = null, CancellationToken cancellationToken = default);
    Task<List<AuditLogDto>> GetAuditLogsByEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default);
    Task<byte[]> ExportAuditLogsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
}

