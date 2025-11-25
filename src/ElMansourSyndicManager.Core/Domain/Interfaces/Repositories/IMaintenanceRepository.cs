using ElMansourSyndicManager.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElMansourSyndicManager.Core.Domain.Interfaces.Repositories
{
    public interface IMaintenanceRepository : IRepository<Maintenance>
    {
        Task<IEnumerable<Maintenance>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    }
}
