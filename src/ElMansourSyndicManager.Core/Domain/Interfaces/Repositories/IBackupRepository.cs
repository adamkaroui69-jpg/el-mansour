using ElMansourSyndicManager.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;

public interface IBackupRepository : IRepository<Backup>
{
    Task<List<Backup>> GetByTypeAsync(string backupType, CancellationToken cancellationToken = default);
    Task<List<Backup>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<List<Backup>> GetAutomaticBackupsAsync(CancellationToken cancellationToken = default);
}
