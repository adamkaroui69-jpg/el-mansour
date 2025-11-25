using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Entities;
using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ElMansourSyndicManager.Infrastructure.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IAuthenticationService _authService;
        private readonly ILogger<ExpenseService> _logger;

        public ExpenseService(IExpenseRepository expenseRepository, IAuthenticationService authService, ILogger<ExpenseService> logger)
        {
            _expenseRepository = expenseRepository;
            _authService = authService;
            _logger = logger;
        }

        public async Task<IEnumerable<ExpenseDto>> GetAllExpensesAsync(CancellationToken cancellationToken = default)
        {
            var expenses = await _expenseRepository.GetAllAsync(cancellationToken);
            return expenses.Select(MapToDto);
        }

        public async Task<IEnumerable<ExpenseDto>> GetExpensesByMonthAsync(int year, int month, CancellationToken cancellationToken = default)
        {
            var expenses = await _expenseRepository.GetByMonthAsync(year, month, cancellationToken);
            return expenses.Select(MapToDto);
        }

        public async Task<ExpenseDto> CreateExpenseAsync(CreateExpenseDto dto, CancellationToken cancellationToken = default)
        {
            var expense = new Expense
            {
                Description = dto.Description,
                Category = dto.Category,
                Amount = dto.Amount,
                ExpenseDate = dto.ExpenseDate,
                RecordedBy = _authService.CurrentUser?.Username ?? "System",
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (Guid.TryParse(dto.MaintenanceId, out var mId))
            {
                expense.MaintenanceId = mId;
            }

            await _expenseRepository.CreateAsync(expense, cancellationToken);
            return MapToDto(expense);
        }

        public async Task<ExpenseDto> UpdateExpenseAsync(Guid id, UpdateExpenseDto dto, CancellationToken cancellationToken = default)
        {
            var expense = await _expenseRepository.GetByIdAsync(id, cancellationToken);
            if (expense == null) throw new KeyNotFoundException($"Expense {id} not found");

            expense.Description = dto.Description;
            expense.Category = dto.Category;
            expense.Amount = dto.Amount;
            expense.ExpenseDate = dto.ExpenseDate;
            expense.Notes = dto.Notes;
            expense.UpdatedAt = DateTime.UtcNow;

             if (Guid.TryParse(dto.MaintenanceId, out var mId))
            {
                expense.MaintenanceId = mId;
            }
            else
            {
                expense.MaintenanceId = null;
            }

            await _expenseRepository.UpdateAsync(expense, cancellationToken);
            return MapToDto(expense);
        }

        public async Task DeleteExpenseAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var expense = await _expenseRepository.GetByIdAsync(id, cancellationToken);
            if (expense != null)
            {
                await _expenseRepository.DeleteAsync(expense, cancellationToken);
            }
        }

        public async Task<Dictionary<string, decimal>> GetExpenseCategoryBreakdownAsync(int year, int month, CancellationToken cancellationToken = default)
        {
            var expenses = await _expenseRepository.GetByMonthAsync(year, month, cancellationToken);
            return expenses.GroupBy(e => e.Category)
                           .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
        }

        private ExpenseDto MapToDto(Expense expense)
        {
            return new ExpenseDto
            {
                Id = expense.Id,
                Description = expense.Description,
                Category = expense.Category,
                Amount = expense.Amount,
                ExpenseDate = expense.ExpenseDate,
                Month = expense.ExpenseDate.ToString("yyyy-MM"),
                Year = expense.ExpenseDate.Year,
                RecordedBy = expense.RecordedBy,
                MaintenanceId = expense.MaintenanceId?.ToString(),
                Notes = expense.Notes,
                CreatedAt = expense.CreatedAt
            };
        }
    }
}
