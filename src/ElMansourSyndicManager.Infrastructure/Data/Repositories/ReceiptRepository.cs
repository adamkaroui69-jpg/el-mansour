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

public class ReceiptRepository : Repository<Receipt>, IReceiptRepository
{
    public ReceiptRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Receipt?> GetByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        return await _context.Receipts.FirstOrDefaultAsync(r => r.PaymentId == paymentId, cancellationToken);
    }
}
