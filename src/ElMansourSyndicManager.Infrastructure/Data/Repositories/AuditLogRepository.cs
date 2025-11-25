using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Entities; // Utiliser les entités
using ElMansourSyndicManager.Infrastructure.Data;
using ElMansourSyndicManager.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElMansourSyndicManager.Infrastructure.Data.Repositories;

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<AuditLog>> GetByDateRangeAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(l => l.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(l => l.CreatedAt <= to.Value);
        }

        return await query.OrderByDescending(l => l.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<List<AuditLog>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs.Where(l => l.UserId == userId).OrderByDescending(l => l.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<List<AuditLog>> GetByEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs.Where(l => l.EntityType == entityType && l.EntityId == entityId).OrderByDescending(l => l.CreatedAt).ToListAsync(cancellationToken);
    }
}
