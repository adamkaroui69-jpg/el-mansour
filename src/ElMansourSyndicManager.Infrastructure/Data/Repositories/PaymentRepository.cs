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

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Payment?> GetByHouseAndMonthAsync(string houseCode, string month, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.HouseCode == houseCode && p.Month == month, cancellationToken);
    }

    public async Task<List<Payment>> GetByHouseCodeAsync(string houseCode, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Payments.Where(p => p.HouseCode == houseCode);

        if (from.HasValue)
            query = query.Where(p => p.PaymentDate >= from.Value);

        if (to.HasValue)
            query = query.Where(p => p.PaymentDate <= to.Value);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<Payment>> GetByMonthAsync(string month, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.Month == month)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Payment>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => (p.PaymentDate.HasValue && p.PaymentDate >= from && p.PaymentDate <= to) ||
                       (!p.PaymentDate.HasValue && p.CreatedAt >= from && p.CreatedAt <= to))
            .ToListAsync(cancellationToken);
    }

    public async Task<Payment?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Payments.FirstOrDefaultAsync(p => p.ReferenceNumber == code, cancellationToken);
    }

    public async Task<List<Payment>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Payments.Where(p => p.Status == "Paid").ToListAsync(cancellationToken); // Assuming "Active" means "Paid" for payments
    }
}
