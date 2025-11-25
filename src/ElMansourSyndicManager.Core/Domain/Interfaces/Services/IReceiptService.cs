using ElMansourSyndicManager.Core.Domain.DTOs;

namespace ElMansourSyndicManager.Core.Domain.Interfaces.Services;

/// <summary>
/// Service for generating and managing receipts
/// </summary>
public interface IReceiptService
{
    /// <summary>
    /// Generates a PDF receipt for a payment with signature overlay
    /// </summary>
    Task<ReceiptDto> GenerateReceiptAsync(Guid paymentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a receipt by ID
    /// </summary>
    Task<ReceiptDto?> GetReceiptByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets receipt file as byte array
    /// </summary>
    Task<byte[]?> GetReceiptFileAsync(Guid receiptId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets receipt file path for viewing
    /// </summary>
    Task<string?> GetReceiptFilePathAsync(Guid receiptId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Regenerates a receipt (if payment was updated)
    /// </summary>
    Task<ReceiptDto> RegenerateReceiptAsync(Guid receiptId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all receipts
    /// </summary>
    Task<List<ReceiptDto>> GetAllReceiptsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets receipt history for a specific house
    /// </summary>
    Task<List<ReceiptDto>> GetReceiptHistoryAsync(string houseCode, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Prints a receipt
    /// </summary>
    Task PrintReceiptAsync(Guid receiptId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes all receipts associated with a payment (cascade delete)
    /// </summary>
    Task DeleteReceiptsByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
}

