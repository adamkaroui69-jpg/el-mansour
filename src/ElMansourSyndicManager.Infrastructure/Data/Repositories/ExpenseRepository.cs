using ElMansourSyndicManager.Core.Domain.Entities;
using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ElMansourSyndicManager.Infrastructure.Data.Repositories
{
    public class ExpenseRepository : Repository<Expense>, IExpenseRepository
    {
        public ExpenseRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Expense>> GetByMonthAsync(int year, int month, CancellationToken cancellationToken = default)
        {
            return await _context.Expenses
                .Where(e => e.ExpenseDate.Year == year && e.ExpenseDate.Month == month)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Expense>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
        {
            return await _context.Expenses
                .Where(e => e.Category == category)
                .ToListAsync(cancellationToken);
        }
    }
}
