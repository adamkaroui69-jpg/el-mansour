using ElMansourSyndicManager.Core.Domain.Entities;
using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace ElMansourSyndicManager.Infrastructure.Repositories;

public class BackupRepository : Repository<Backup>, IBackupRepository
{
    public BackupRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<Backup>> GetByTypeAsync(string backupType, CancellationToken cancellationToken = default)
    {
        return await _context.Backups
            .Where(b => b.BackupType == backupType)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Backup>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.Backups
            .Where(b => b.CreatedAt >= from && b.CreatedAt <= to)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Backup>> GetAutomaticBackupsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Backups
            .Where(b => b.IsAutomatic)
            .ToListAsync(cancellationToken);
    }
}
