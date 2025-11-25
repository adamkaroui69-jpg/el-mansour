using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Entities; // Utiliser les entités
using ElMansourSyndicManager.Infrastructure.Data;
using ElMansourSyndicManager.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace ElMansourSyndicManager.Infrastructure.Data.Repositories;

public class HouseRepository : Repository<House>, IHouseRepository
{
    public HouseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<House?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Houses.FirstOrDefaultAsync(h => h.HouseCode == code, cancellationToken);
    }

    public async Task<List<House>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Houses.ToListAsync(cancellationToken);
    }

    public async Task<List<House>> GetByBuildingAsync(string buildingCode, CancellationToken cancellationToken = default)
    {
        return await _context.Houses.Where(h => h.BuildingCode == buildingCode).ToListAsync(cancellationToken);
    }
}
