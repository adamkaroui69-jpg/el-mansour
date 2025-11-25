using ElMansourSyndicManager.Core.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ElMansourSyndicManager.Core.Domain.Interfaces.Services
{
    public interface IExpenseService
    {
        Task<IEnumerable<ExpenseDto>> GetAllExpensesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<ExpenseDto>> GetExpensesByMonthAsync(int year, int month, CancellationToken cancellationToken = default);
        Task<ExpenseDto> CreateExpenseAsync(CreateExpenseDto dto, CancellationToken cancellationToken = default);
        Task<ExpenseDto> UpdateExpenseAsync(Guid id, UpdateExpenseDto dto, CancellationToken cancellationToken = default);
        Task DeleteExpenseAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Dictionary<string, decimal>> GetExpenseCategoryBreakdownAsync(int year, int month, CancellationToken cancellationToken = default);
    }
}
