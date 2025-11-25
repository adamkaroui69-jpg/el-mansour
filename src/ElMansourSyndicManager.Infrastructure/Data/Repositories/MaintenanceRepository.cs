using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Entities;
using ElMansourSyndicManager.Infrastructure.Data;
using ElMansourSyndicManager.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System;

namespace ElMansourSyndicManager.Infrastructure.Data.Repositories;

public class MaintenanceRepository : Repository<Maintenance>, IMaintenanceRepository
{
    public MaintenanceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Maintenance>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Maintenance>().Where(m => m.Status == status).ToListAsync(cancellationToken);
    }

    public override async Task<Maintenance?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Maintenance>().FindAsync(new object[] { id }, cancellationToken);
    }

    public override async Task<IEnumerable<Maintenance>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Maintenance>().ToListAsync(cancellationToken);
    }

    public override async Task<IEnumerable<Maintenance>> FindAsync(Expression<Func<Maintenance, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Maintenance>().Where(predicate).ToListAsync(cancellationToken);
    }

    public override async Task<Maintenance> CreateAsync(Maintenance entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<Maintenance>().AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public override async Task UpdateAsync(Maintenance entity, CancellationToken cancellationToken = default)
    {
        _context.Set<Maintenance>().Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public override async Task DeleteAsync(Maintenance entity, CancellationToken cancellationToken = default)
    {
        _context.Set<Maintenance>().Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
