using ElMansourSyndicManager.Core.Domain.DTOs;

namespace ElMansourSyndicManager.Core.Domain.Interfaces.Services;

/// <summary>
/// Service for managing payments
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Creates a new payment record
    /// </summary>
    Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto payment, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a payment by ID
    /// </summary>
    Task<PaymentDto?> GetPaymentByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all payments for a specific house
    /// </summary>
    Task<List<PaymentDto>> GetPaymentsByHouseAsync(string houseCode, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all payments for a specific month
    /// </summary>
    Task<List<PaymentDto>> GetPaymentsByMonthAsync(string month, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all unpaid houses for a specific month
    /// </summary>
    Task<List<UnpaidHouseDto>> GetUnpaidHousesAsync(string month, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Marks a payment as paid
    /// </summary>
    Task<PaymentDto> MarkAsPaidAsync(Guid paymentId, DateTime paymentDate, string? notes = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Marks a payment as unpaid
    /// </summary>
    Task<PaymentDto> MarkAsUnpaidAsync(Guid paymentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Detects and marks overdue payments
    /// </summary>
    Task<int> DetectOverduePaymentsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets payment statistics for a date range
    /// </summary>
    Task<PaymentStatisticsDto> GetPaymentStatisticsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a payment (soft delete, admin only)
    /// </summary>
    Task<bool> DeletePaymentAsync(Guid id, CancellationToken cancellationToken = default);
}

