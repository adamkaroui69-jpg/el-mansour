using ElMansourSyndicManager.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElMansourSyndicManager.Core.Domain.Interfaces.Repositories
{
    public interface IExpenseRepository : IRepository<Expense>
    {
        Task<IEnumerable<Expense>> GetByMonthAsync(int year, int month, CancellationToken cancellationToken = default);
        Task<IEnumerable<Expense>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    }
}
