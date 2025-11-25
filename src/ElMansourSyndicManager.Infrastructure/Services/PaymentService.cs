using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Entities;
using ElMansourSyndicManager.Core.Domain.Exceptions;
using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ElMansourSyndicManager.Infrastructure.Services;

/// <summary>
/// Service for managing payments
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IHouseRepository _houseRepository;
    private readonly IUserRepository _userRepository;
    private readonly IReceiptService _receiptService;
    private readonly IAuditService _auditService;
    private readonly IAuthenticationService _authService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IHouseRepository houseRepository,
        IUserRepository userRepository,
        IReceiptService receiptService,
        IAuditService auditService,
        IAuthenticationService authService,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _houseRepository = houseRepository;
        _userRepository = userRepository;
        _receiptService = receiptService;
        _auditService = auditService;
        _authService = authService;
        _logger = logger;
    }

    public async Task<PaymentDto> CreatePaymentAsync(
        CreatePaymentDto payment, 
        CancellationToken cancellationToken = default)
    {
        // Validate current user
        if (!_authService.IsAuthenticated)
            throw new UnauthorizedException("User must be authenticated");

        var currentUser = _authService.CurrentUser!;

        // Validate input
        ValidatePaymentInput(payment);

        // Check if house exists
        var house = await _houseRepository.GetByCodeAsync(payment.HouseCode, cancellationToken);
        if (house == null)
            throw new NotFoundException("House", payment.HouseCode);

        if (!house.IsActive)
            throw new BusinessRuleException($"House {payment.HouseCode} is not active");

        // Validate amount matches house monthly amount
        // if (payment.Amount != house.MonthlyAmount)
        //    throw new BusinessRuleException($"Payment amount {payment.Amount} does not match house monthly amount {house.MonthlyAmount}");

        // Check for duplicate payment (same house, same month)
        var existingPayment = await _paymentRepository.GetByHouseAndMonthAsync(
            payment.HouseCode, 
            payment.Month, 
            cancellationToken);
        
        if (existingPayment != null)
            throw new BusinessRuleException($"Payment for house {payment.HouseCode} in month {payment.Month} already exists");

        // Create payment entity
        var paymentEntity = new Payment
        {
            Id = Guid.NewGuid(),
            HouseCode = payment.HouseCode,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            Month = payment.Month,
            Status = "Paid",
            ReferenceNumber = payment.ReferenceNumber,
            GeneratedBy = currentUser.Id.ToString(),
            RecordedBy = currentUser.Id.ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Save payment
        var savedPayment = await _paymentRepository.CreateAsync(paymentEntity, cancellationToken);

        // Generate receipt automatically
        try
        {
            await _receiptService.GenerateReceiptAsync(savedPayment.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate receipt for payment {PaymentId}", savedPayment.Id);
            // Don't fail the payment creation if receipt generation fails
        }

        // Log activity
        await _auditService.LogActivityAsync(new AuditLogDto
        {
            UserId = currentUser.Id.ToString(),
            Action = "Create",
            EntityType = "Payment",
            EntityId = savedPayment.Id.ToString(),
            Details = $"{{\"houseCode\":\"{payment.HouseCode}\",\"amount\":{payment.Amount},\"month\":\"{payment.Month}\"}}"
        }, cancellationToken);

        _logger.LogInformation("Payment {PaymentId} created for house {HouseCode}", savedPayment.Id, payment.HouseCode);

        return MapToDto(savedPayment);
    }

    public async Task<PaymentDto?> GetPaymentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        return payment != null ? MapToDto(payment) : null;
    }

    public async Task<List<PaymentDto>> GetPaymentsByHouseAsync(
        string houseCode, 
        DateTime? from = null, 
        DateTime? to = null, 
        CancellationToken cancellationToken = default)
    {
        var payments = await _paymentRepository.GetByHouseCodeAsync(houseCode, from, to, cancellationToken);
        return payments.Select(MapToDto).ToList();
    }

    public async Task<List<PaymentDto>> GetPaymentsByMonthAsync(
        string month, 
        CancellationToken cancellationToken = default)
    {
        ValidateMonthFormat(month);
        var payments = await _paymentRepository.GetByMonthAsync(month, cancellationToken);
        return payments.Select(MapToDto).ToList();
    }

    public async Task<List<UnpaidHouseDto>> GetUnpaidHousesAsync(
        string month, 
        CancellationToken cancellationToken = default)
    {
        ValidateMonthFormat(month);

        // Get all active houses
        var houses = await _houseRepository.GetAllActiveAsync(cancellationToken);
        
        // Get all paid houses for the month
        var allPayments = await _paymentRepository.GetByMonthAsync(month, cancellationToken);
        var paidPayments = allPayments.Where(p => 
            string.Equals(p.Status, "Paid", StringComparison.OrdinalIgnoreCase) || 
            string.Equals(p.Status, "Payé", StringComparison.OrdinalIgnoreCase));
            
        var paidHouseCodes = paidPayments.Select(p => p.HouseCode).ToHashSet();

        // Calculate unpaid houses
        var unpaidHouses = new List<UnpaidHouseDto>();
        var monthDate = DateTime.ParseExact($"{month}-01", "yyyy-MM-dd", null);
        var today = DateTime.Today;

        foreach (var house in houses.Where(h => !paidHouseCodes.Contains(h.HouseCode)))
        {
            var daysOverdue = (today - monthDate).Days;
            if (daysOverdue > 0)
            {
                unpaidHouses.Add(new UnpaidHouseDto
                {
                    HouseCode = house.HouseCode,
                    Building = house.BuildingCode,
                    OwnerName = house.OwnerName,
                    OwnerPhone = house.ContactNumber,
                    MonthlyAmount = house.MonthlyAmount,
                    Month = month,
                    DaysOverdue = daysOverdue
                });
            }
        }

        return unpaidHouses.OrderBy(h => h.DaysOverdue).ToList();
    }

    public async Task<PaymentDto> MarkAsPaidAsync(
        Guid paymentId, 
        DateTime paymentDate, 
        string? notes = null, 
        CancellationToken cancellationToken = default)
    {
        if (!_authService.IsAuthenticated)
            throw new UnauthorizedException("User must be authenticated");

        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
            throw new NotFoundException("Payment", paymentId);

        payment.Status = "Paid";
        payment.PaymentDate = paymentDate;
        payment.UpdatedAt = DateTime.UtcNow;
        
        // S'assurer que GeneratedBy et RecordedBy ne sont pas vides
        if (string.IsNullOrEmpty(payment.GeneratedBy))
        {
            payment.GeneratedBy = _authService.CurrentUser!.Id.ToString();
        }
        if (string.IsNullOrEmpty(payment.RecordedBy))
        {
            payment.RecordedBy = _authService.CurrentUser!.Id.ToString();
        }

        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        await _auditService.LogActivityAsync(new AuditLogDto
        {
            UserId = _authService.CurrentUser!.Id.ToString(),
            Action = "Update",
            EntityType = "Payment",
            EntityId = paymentId.ToString(),
            Details = "{\"status\":\"Paid\"}"
        }, cancellationToken);

        return MapToDto(payment);
    }

    public async Task<PaymentDto> MarkAsUnpaidAsync(
        Guid paymentId, 
        CancellationToken cancellationToken = default)
    {
        if (!_authService.IsAuthenticated)
            throw new UnauthorizedException("User must be authenticated");

        if (!_authService.IsAdmin)
            throw new UnauthorizedException("Only admins can mark payments as unpaid");

        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
            throw new NotFoundException("Payment", paymentId);

        payment.Status = "Unpaid";
        payment.UpdatedAt = DateTime.UtcNow;

        await _paymentRepository.UpdateAsync(payment, cancellationToken);

        await _auditService.LogActivityAsync(new AuditLogDto
        {
            UserId = _authService.CurrentUser!.Id.ToString(),
            Action = "Update",
            EntityType = "Payment",
            EntityId = paymentId.ToString(),
            Details = "{\"status\":\"Unpaid\"}"
        }, cancellationToken);

        return MapToDto(payment);
    }

    public async Task<int> DetectOverduePaymentsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var currentMonth = today.ToString("yyyy-MM");
        var lastMonth = today.AddMonths(-1).ToString("yyyy-MM");

        // Get unpaid payments from last month
        var lastMonthPayments = await _paymentRepository.GetByMonthAsync(lastMonth, cancellationToken);
        var overduePayments = lastMonthPayments
            .Where(p => p.Status == "Unpaid" && p.PaymentDate < today.AddDays(-30))
            .ToList();

        int updatedCount = 0;
        foreach (var payment in overduePayments)
        {
            payment.Status = "Overdue";
            payment.UpdatedAt = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            updatedCount++;
        }

        if (updatedCount > 0)
        {
            _logger.LogInformation("Marked {Count} payments as overdue", updatedCount);
        }

        return updatedCount;
    }

    public async Task<PaymentStatisticsDto> GetPaymentStatisticsAsync(
        DateTime from, 
        DateTime to, 
        CancellationToken cancellationToken = default)
    {
        try 
        {
            var logPath = @"c:\Users\adamk\Desktop\raisidance application\debug_log.txt";
            File.AppendAllText(logPath, $"\n[{DateTime.Now}] GetPaymentStatisticsAsync called. Range: {from} to {to}\n");

            // Use GetAllAsync to avoid potential EF Core date comparison issues and ensure we get everything
            var allPayments = await _paymentRepository.GetAllAsync(cancellationToken);
            File.AppendAllText(logPath, $"[{DateTime.Now}] Total payments in DB: {allPayments.Count()}\n");

            foreach(var p in allPayments)
            {
                File.AppendAllText(logPath, $"Payment: Id={p.Id}, Status='{p.Status}', Amount={p.Amount}, Date={p.PaymentDate}, Created={p.CreatedAt}\n");
            }
            
            // Filter in memory
            var payments = allPayments.Where(p => 
                (p.PaymentDate.HasValue && p.PaymentDate >= from && p.PaymentDate <= to) ||
                (!p.PaymentDate.HasValue && p.CreatedAt >= from && p.CreatedAt <= to))
                .ToList();

            File.AppendAllText(logPath, $"[{DateTime.Now}] Payments after date filter: {payments.Count}\n");

            var activeHouses = await _houseRepository.GetAllActiveAsync(cancellationToken);
            var houseCount = activeHouses.Count();

            // Log for debugging
            var paidPayments = payments.Where(p => 
                string.Equals(p.Status?.Trim(), "Paid", StringComparison.OrdinalIgnoreCase) || 
                string.Equals(p.Status?.Trim(), "Payé", StringComparison.OrdinalIgnoreCase)).ToList();
                
            File.AppendAllText(logPath, $"[{DateTime.Now}] Paid payments count: {paidPayments.Count}. Sum: {paidPayments.Sum(p => p.Amount)}\n");

            _logger.LogInformation("GetPaymentStatisticsAsync: Found {Total} payments in range. {Paid} are paid. Total Amount: {Amount}", 
                payments.Count, paidPayments.Count, paidPayments.Sum(p => p.Amount));

            var totalCollected = paidPayments.Sum(p => p.Amount);
                
            var totalExpected = houseCount * activeHouses.FirstOrDefault()?.MonthlyAmount ?? 0;
            var collectionRate = totalExpected > 0 ? (totalCollected / totalExpected) * 100 : 0;

            var monthlyBreakdown = payments
                .GroupBy(p => p.Month)
                .ToDictionary(g => g.Key, g => g.Where(p => 
                    string.Equals(p.Status?.Trim(), "Paid", StringComparison.OrdinalIgnoreCase) || 
                    string.Equals(p.Status?.Trim(), "Payé", StringComparison.OrdinalIgnoreCase))
                    .Sum(p => p.Amount));

            return new PaymentStatisticsDto
            {
                TotalCollected = totalCollected,
                TotalExpected = totalExpected,
                CollectionRate = collectionRate,
                PaidCount = paidPayments.Count,
                UnpaidCount = payments.Count(p => p.Status == "Unpaid"),
                OverdueCount = payments.Count(p => p.Status == "Overdue"),
                MonthlyBreakdown = monthlyBreakdown
            };
        }
        catch (Exception ex)
        {
            var logPath = @"c:\Users\adamk\Desktop\raisidance application\debug_log.txt";
            File.AppendAllText(logPath, $"[{DateTime.Now}] ERROR in GetPaymentStatisticsAsync: {ex.Message}\n{ex.StackTrace}\n");
            throw;
        }
    }

    #region Private Methods

    private void ValidatePaymentInput(CreatePaymentDto payment)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(payment.HouseCode))
            errors["HouseCode"] = new[] { "House code is required" };

        if (payment.Amount <= 0)
            errors["Amount"] = new[] { "Amount must be greater than zero" };

        if (!ValidateMonthFormat(payment.Month))
            errors["Month"] = new[] { "Month must be in format YYYY-MM" };

        if (errors.Any())
            throw new ValidationException("Payment validation failed", errors);
    }

    private bool ValidateMonthFormat(string month)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(month, @"^\d{4}-\d{2}$");
    }

    private PaymentDto MapToDto(Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            HouseCode = payment.HouseCode,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            Month = payment.Month,
            Status = payment.Status,
            ReferenceNumber = payment.ReferenceNumber,
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.UpdatedAt ?? payment.CreatedAt
        };
    }

    public async Task<bool> DeletePaymentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting payment with ID: {PaymentId}", id);

            var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
            if (payment == null)
            {
                _logger.LogWarning("Payment with ID {PaymentId} not found", id);
                return false;
            }

            // Delete associated receipts first (cascade delete)
            try
            {
                await _receiptService.DeleteReceiptsByPaymentIdAsync(id, cancellationToken);
                _logger.LogInformation("Deleted receipts for payment {PaymentId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("No receipts found or error deleting receipts for payment {PaymentId}: {Error}", id, ex.Message);
                // Continue with payment deletion even if receipt deletion fails
            }

            // Delete the payment
            await _paymentRepository.DeleteAsync(payment, cancellationToken);

            _logger.LogInformation("Successfully deleted payment {PaymentId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment {PaymentId}", id);
            throw;
        }
    }

    #endregion
}
