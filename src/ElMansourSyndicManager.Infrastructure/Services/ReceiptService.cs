using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Entities;
using ElMansourSyndicManager.Core.Domain.Exceptions;
using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using QRCoder;

namespace ElMansourSyndicManager.Infrastructure.Services;

/// <summary>
/// Service for generating and managing PDF receipts with signature overlay
/// </summary>
public class ReceiptService : IReceiptService
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ReceiptService> _logger;
    private readonly string _receiptsBasePath;
    private const string ReceiptsFolder = "Receipts";

    public ReceiptService(
        IReceiptRepository receiptRepository,
        IPaymentRepository paymentRepository,
        IUserRepository userRepository,
        ILogger<ReceiptService> logger)
    {
        _receiptRepository = receiptRepository;
        _paymentRepository = paymentRepository;
        _userRepository = userRepository;
        _logger = logger;
        
        // Initialize QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;
        
        // Setup local receipts directory
        _receiptsBasePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "data",
            ReceiptsFolder);
        
        Directory.CreateDirectory(_receiptsBasePath);
    }

    /// <summary>
    /// Generates a PDF receipt for a payment with signature overlay
    /// </summary>
    public async Task<ReceiptDto> GenerateReceiptAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get payment
            var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
            if (payment == null)
                throw new NotFoundException("Payment", paymentId);

            // Check if receipt already exists
            var existingReceipt = await _receiptRepository.GetByPaymentIdAsync(paymentId);
            if (existingReceipt != null)
            {
                _logger.LogInformation("Receipt already exists for payment {PaymentId}, regenerating", paymentId);
                // Regenerate receipt (update existing)
                return await RegenerateReceiptAsync(existingReceipt.Id, cancellationToken);
            }

            // Get user who recorded the payment (for signature)
            User? recordedByUser = null;
            if (Guid.TryParse(payment.GeneratedBy, out var userId))
            {
                recordedByUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
            }
            
            if (recordedByUser == null)
            {
                _logger.LogWarning("User not found for payment {PaymentId}, GeneratedBy: {GeneratedBy}", paymentId, payment.GeneratedBy);
            }

            // Generate unique trace ID for this receipt
            var traceId = GenerateTraceId();

            // Generate PDF
            var pdfBytes = await GenerateReceiptPdfAsync(payment, recordedByUser, traceId);

            // Save locally
            var localFilePath = CreateLocalFilePath(payment.HouseCode, payment.Month, traceId);
            var directory = Path.GetDirectoryName(localFilePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            await File.WriteAllBytesAsync(localFilePath, pdfBytes, cancellationToken);

            // Create receipt entity
            var receipt = new Receipt
            {
                Id = Guid.NewGuid(),
                ReceiptNumber = traceId,
                PaymentId = paymentId,
                ReceiptDate = DateTime.UtcNow,
                GeneratedDate = DateTime.UtcNow,
                AmountPaid = payment.Amount,
                PaymentMethod = "Cash", // Default
                GeneratedBy = recordedByUser?.Id.ToString() ?? string.Empty,
                FilePath = localFilePath,
                FileName = Path.GetFileName(localFilePath),
                FileSize = pdfBytes.Length,
                MimeType = "application/pdf",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _receiptRepository.CreateAsync(receipt, cancellationToken);

            _logger.LogInformation("Receipt generated for payment {PaymentId}, TraceId: {TraceId}", paymentId, traceId);

            return MapToDto(receipt, payment.Month);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating receipt for payment {PaymentId}", paymentId);
            throw;
        }
    }

    /// <summary>
    /// Gets all receipts
    /// </summary>
    public async Task<List<ReceiptDto>> GetAllReceiptsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var allReceipts = await _receiptRepository.GetAllAsync(cancellationToken);
            
            // Get all payments to map months
            var allPayments = await _paymentRepository.GetAllAsync(cancellationToken);
            var paymentMonths = allPayments.ToDictionary(p => p.Id, p => p.Month);

            return allReceipts.OrderByDescending(r => r.CreatedAt)
                .Select(r => MapToDto(r, paymentMonths.ContainsKey(r.PaymentId) ? paymentMonths[r.PaymentId] : ""))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all receipts");
            throw;
        }
    }

    /// <summary>
    /// Gets receipt history for a specific house
    /// </summary>
    public async Task<List<ReceiptDto>> GetReceiptHistoryAsync(
        string houseCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get all payments for this house
            var payments = await _paymentRepository.GetByHouseCodeAsync(houseCode, null, null, cancellationToken);
            var paymentIds = payments.Select(p => p.Id).ToList();
            var paymentMonths = payments.ToDictionary(p => p.Id, p => p.Month);

            // Get all receipts for these payments
            var allReceipts = await _receiptRepository.GetAllAsync();
            var houseReceipts = allReceipts
                .Where(r => paymentIds.Contains(r.PaymentId))
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            return houseReceipts
                .Select(r => MapToDto(r, paymentMonths.ContainsKey(r.PaymentId) ? paymentMonths[r.PaymentId] : ""))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting receipt history for house {HouseCode}", houseCode);
            throw;
        }
    }

    /// <summary>
    /// Gets a receipt by ID
    /// </summary>
    public async Task<ReceiptDto?> GetReceiptByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var receipt = await _receiptRepository.GetByIdAsync(id, cancellationToken);
        if (receipt == null) return null;
        
        // Get payment to retrieve the month
        var payment = await _paymentRepository.GetByIdAsync(receipt.PaymentId, cancellationToken);
        return MapToDto(receipt, payment?.Month ?? "");
    }

    /// <summary>
    /// Gets receipt file as byte array
    /// </summary>
    public async Task<byte[]?> GetReceiptFileAsync(
        Guid receiptId,
        CancellationToken cancellationToken = default)
    {
        var receipt = await _receiptRepository.GetByIdAsync(receiptId, cancellationToken);
        if (receipt == null)
            return null;

        // Try local file first
        if (File.Exists(receipt.FilePath))
        {
            return await File.ReadAllBytesAsync(receipt.FilePath, cancellationToken);
        }

        // Fallback: try to find file in default directory if absolute path fails
        var fileName = Path.GetFileName(receipt.FilePath);
        if (!string.IsNullOrEmpty(fileName))
        {
             var potentialPath = Directory.GetFiles(_receiptsBasePath, fileName, SearchOption.AllDirectories).FirstOrDefault();
             if (!string.IsNullOrEmpty(potentialPath) && File.Exists(potentialPath))
             {
                 // Update the path in DB for future
                 receipt.FilePath = potentialPath;
                 await _receiptRepository.UpdateAsync(receipt, cancellationToken);
                 return await File.ReadAllBytesAsync(potentialPath, cancellationToken);
             }
        }

        // If file is still missing, recreate it from DB data
        _logger.LogWarning("Receipt file missing for {ReceiptId}, recreating it...", receiptId);
        return await RecreateReceiptPdfAsync(receipt, cancellationToken);
    }

    /// <summary>
    /// Gets receipt file path for viewing
    /// </summary>
    public async Task<string?> GetReceiptFilePathAsync(
        Guid receiptId,
        CancellationToken cancellationToken = default)
    {
        var receipt = await _receiptRepository.GetByIdAsync(receiptId, cancellationToken);
        if (receipt == null)
            return null;

        if (File.Exists(receipt.FilePath))
            return receipt.FilePath;

        // Fallback search
        var fileName = Path.GetFileName(receipt.FilePath);
        if (!string.IsNullOrEmpty(fileName))
        {
             var potentialPath = Directory.GetFiles(_receiptsBasePath, fileName, SearchOption.AllDirectories).FirstOrDefault();
             if (!string.IsNullOrEmpty(potentialPath) && File.Exists(potentialPath))
             {
                 receipt.FilePath = potentialPath;
                 await _receiptRepository.UpdateAsync(receipt, cancellationToken);
                 return potentialPath;
             }
        }

        // If file is still missing, recreate it
        _logger.LogWarning("Receipt file missing for {ReceiptId} (path request), recreating it...", receiptId);
        await RecreateReceiptPdfAsync(receipt, cancellationToken);
        return receipt.FilePath;
    }

    /// <summary>
    /// Regenerates a receipt (if payment was updated)
    /// </summary>
    public async Task<ReceiptDto> RegenerateReceiptAsync(
        Guid receiptId,
        CancellationToken cancellationToken = default)
    {
        var existingReceipt = await _receiptRepository.GetByIdAsync(receiptId, cancellationToken);
        if (existingReceipt == null)
            throw new NotFoundException("Receipt", receiptId);

        // Get payment
        var payment = await _paymentRepository.GetByIdAsync(existingReceipt.PaymentId, cancellationToken);
        if (payment == null)
            throw new NotFoundException("Payment", existingReceipt.PaymentId);

        // Get user
        User? user = null;
        if (Guid.TryParse(existingReceipt.GeneratedBy, out var userId))
        {
            user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        }
        
        if (user == null)
        {
             _logger.LogWarning("User not found for receipt {ReceiptId}, GeneratedBy: {GeneratedBy}", receiptId, existingReceipt.GeneratedBy);
        }

        // Delete old file
        if (File.Exists(existingReceipt.FilePath))
        {
            try
            {
                File.Delete(existingReceipt.FilePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete old receipt file");
            }
        }

        // Generate unique trace ID for this receipt
        var traceId = GenerateTraceId();

        // Generate PDF
        var pdfBytes = await GenerateReceiptPdfAsync(payment, user, traceId);

        // Save locally
        var localFilePath = CreateLocalFilePath(payment.HouseCode, payment.Month, traceId);
        var directory = Path.GetDirectoryName(localFilePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllBytesAsync(localFilePath, pdfBytes, cancellationToken);

        // Update receipt entity
        existingReceipt.FilePath = localFilePath;
        existingReceipt.FileName = Path.GetFileName(localFilePath);
        existingReceipt.FileSize = pdfBytes.Length;
        existingReceipt.UpdatedAt = DateTime.UtcNow;
        existingReceipt.ReceiptNumber = traceId;

        await _receiptRepository.UpdateAsync(existingReceipt, cancellationToken);

        return MapToDto(existingReceipt, payment.Month);
    }

    /// <summary>
    /// Prints a receipt
    /// </summary>
    public async Task PrintReceiptAsync(
        Guid receiptId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pdfBytes = await GetReceiptFileAsync(receiptId, cancellationToken);
            if (pdfBytes == null)
                throw new NotFoundException("Receipt file", receiptId);

            // Save to temporary file for printing
            var tempFile = Path.Combine(Path.GetTempPath(), $"receipt_{receiptId}.pdf");
            await File.WriteAllBytesAsync(tempFile, pdfBytes, cancellationToken);

            // Use default system PDF viewer to print
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = tempFile,
                UseShellExecute = true,
                Verb = "print"
            };

            System.Diagnostics.Process.Start(processStartInfo);

            _logger.LogInformation("Receipt {ReceiptId} sent to printer", receiptId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error printing receipt {ReceiptId}", receiptId);
            throw;
        }
    }

    /// <summary>
    /// Recreates a receipt PDF from existing DB data (for recovery)
    /// </summary>
    private async Task<byte[]> RecreateReceiptPdfAsync(
        Receipt receipt,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(receipt.PaymentId, cancellationToken);
        if (payment == null)
            throw new NotFoundException("Payment", receipt.PaymentId);

        User? user = null;
        // Use the creator of the payment (GeneratedBy)
        if (Guid.TryParse(payment.GeneratedBy, out var userId))
        {
            user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        }

        // Use existing Trace ID (ReceiptNumber)
        var pdfBytes = await GenerateReceiptPdfAsync(payment, user, receipt.ReceiptNumber);

        // Save locally to restore the file
        var localFilePath = receipt.FilePath;
        // If original path is invalid/empty, create a new standard one
        if (string.IsNullOrEmpty(localFilePath) || !Path.IsPathRooted(localFilePath))
        {
            localFilePath = CreateLocalFilePath(payment.HouseCode, payment.Month, receipt.ReceiptNumber);
        }

        var directory = Path.GetDirectoryName(localFilePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllBytesAsync(localFilePath, pdfBytes, cancellationToken);

        // Update DB if path changed
        if (receipt.FilePath != localFilePath)
        {
            receipt.FilePath = localFilePath;
            await _receiptRepository.UpdateAsync(receipt, cancellationToken);
        }

        return pdfBytes;
    }

    #region Private Methods

    /// <summary>
    /// Generates the PDF receipt document
    /// </summary>
    private Task<byte[]> GenerateReceiptPdfAsync(
        Payment payment,
        User? member,
        string traceId)
    {
        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5.Landscape());
                page.Margin(1.2f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                // Header with logo and title
                page.Header()
                    .Column(column =>
                    {
                        // Logo centered at top
                        var logoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "logo.png");
                        if (System.IO.File.Exists(logoPath))
                        {
                            column.Item().AlignCenter().Width(45).Image(logoPath);
                            column.Item().PaddingTop(5);
                        }
                        
                        // Title - smaller and centered
                        column.Item().AlignCenter().Text("Reçu de Paiement - Syndic résidence El Mansour")
                            .SemiBold().FontSize(12).FontColor(Colors.Blue.Darken2);
                        
                        // Decorative line
                        column.Item().PaddingTop(5).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                    });

                // QR Code in top-right corner
                page.Header()
                    .AlignRight()
                    .Width(60)
                    .Column(qrColumn =>
                    {
                        // Generate QR code data
                        var qrData = $"{payment.HouseCode}|{payment.Id}|{payment.Month}|{payment.Amount:F2}";
                        var qrCodeBytes = GenerateQRCode(qrData);
                        
                        if (qrCodeBytes != null)
                        {
                            qrColumn.Item().Image(qrCodeBytes);
                            qrColumn.Item().PaddingTop(2).AlignCenter()
                                .Text("Scan pour vérifier")
                                .FontSize(6)
                                .FontColor(Colors.Grey.Medium);
                        }
                    });

                // Main content
                page.Content()
                    .PaddingVertical(0.5f, Unit.Centimetre)
                    .Column(column =>
                    {
                        column.Spacing(8);
                        
                        // Receipt info section with subtle background
                        column.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(info =>
                        {
                            info.Spacing(5);
                            
                            info.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Date:").FontSize(9).FontColor(Colors.Grey.Darken1);
                                // Use Payment Date if available, otherwise Now
                                var dateStr = payment.PaymentDate.HasValue 
                                    ? payment.PaymentDate.Value.ToString("dd/MM/yyyy") 
                                    : DateTime.Now.ToString("dd/MM/yyyy");
                                row.RelativeItem().AlignRight().Text(dateStr).SemiBold();
                            });
                            
                            info.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Reçu N°:").FontSize(9).FontColor(Colors.Grey.Darken1);
                                row.RelativeItem().AlignRight().Text(traceId).SemiBold().FontSize(9);
                            });
                            
                            info.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Maison:").FontSize(9).FontColor(Colors.Grey.Darken1);
                                row.RelativeItem().AlignRight().Text(payment.HouseCode).SemiBold();
                            });
                            
                            info.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Mois:").FontSize(9).FontColor(Colors.Grey.Darken1);
                                row.RelativeItem().AlignRight().Text(FormatMonth(payment.Month)).SemiBold();
                            });
                        });
                        
                        // Amount section - highlighted
                        column.Item().PaddingTop(6).Border(2).BorderColor(Colors.Blue.Darken2)
                            .Background(Colors.Blue.Lighten5).Padding(12).Column(amount =>
                        {
                            amount.Item().AlignCenter().Text("Montant Payé").FontSize(10).FontColor(Colors.Grey.Darken1);
                            amount.Item().PaddingTop(3).AlignCenter().Text($"{payment.Amount:N2} TND")
                                .Bold().FontSize(18).FontColor(Colors.Blue.Darken3);
                        });
                        
                        // Received by section (Creator Name)
                        if (member != null)
                        {
                            column.Item().PaddingTop(8).AlignCenter().Text($"Créé par: {member.Username}")
                                .FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
                        }
                        
                        // Thank you message
                        column.Item().PaddingTop(8).AlignCenter().Text("Merci pour votre paiement")
                            .FontSize(9).Italic().FontColor(Colors.Grey.Medium);
                    });

                // Footer - simple, no page number
                page.Footer()
                    .AlignCenter()
                    .PaddingTop(5)
                    .Text("Résidence El Mansour")
                    .FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });

        return Task.FromResult(document.GeneratePdf());
    }

    /// <summary>
    /// Creates local file path for receipt storage
    /// </summary>
    private string CreateLocalFilePath(string houseCode, string month, string traceId)
    {
        // Format: Receipts/HouseCode/YYYY-MM/Receipt_HouseCode_YYYY-MM_TraceId.pdf
        var fileName = $"Receipt_{houseCode}_{month}_{traceId}.pdf";
        return Path.Combine(_receiptsBasePath, houseCode, month, fileName);
    }

    /// <summary>
    /// Generates a unique trace ID for receipt tracking
    /// </summary>
    private string GenerateTraceId()
    {
        // Format: YYYYMMDD-HHMMSS-XXXX (last 4 chars of GUID)
        var now = DateTime.UtcNow;
        var guid = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
        return $"{now:yyyyMMdd}-{now:HHmmss}-{guid}";
    }

    /// <summary>
    /// Formats month string (YYYY-MM) to readable format
    /// </summary>
    private string FormatMonth(string month)
    {
        if (DateTime.TryParseExact($"{month}-01", "yyyy-MM-dd", null,
            System.Globalization.DateTimeStyles.None, out var date))
        {
            return date.ToString("MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"));
        }
        return month;
    }

    /// <summary>
    /// Generates QR code as PNG byte array
    /// </summary>
    private byte[]? GenerateQRCode(string data)
    {
        try
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate QR code");
            return null;
        }
    }

    /// <summary>
    /// Maps Receipt entity to DTO
    /// </summary>
    private ReceiptDto MapToDto(Receipt receipt, string paymentMonth = "")
    {
        return new ReceiptDto
        {
            Id = receipt.Id,
            PaymentId = receipt.PaymentId,
            FilePath = receipt.FilePath,
            CloudStoragePath = receipt.CloudStoragePath,
            FileName = receipt.FileName,
            FileSize = receipt.FileSize,
            MimeType = receipt.MimeType,
            GeneratedBy = receipt.GeneratedBy,
            ReceiptDate = (DateTime?)receipt.ReceiptDate,
            GeneratedDate = receipt.GeneratedDate,
            GeneratedAt = receipt.GeneratedDate, // Mapped for UI
            AmountPaid = receipt.AmountPaid,
            PaymentMethod = receipt.PaymentMethod,
            PaymentMonth = paymentMonth, // Set payment month
            CreatedAt = receipt.CreatedAt,
            UpdatedAt = receipt.UpdatedAt ?? receipt.CreatedAt
        };
    }

    /// <summary>
    /// Deletes all receipts associated with a payment (cascade delete)
    /// </summary>
    public async Task DeleteReceiptsByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting receipts for payment {PaymentId}", paymentId);

            // Get all receipts for this payment
            var allReceipts = await _receiptRepository.GetAllAsync(cancellationToken);
            var receiptsToDelete = allReceipts.Where(r => r.PaymentId == paymentId).ToList();

            if (!receiptsToDelete.Any())
            {
                _logger.LogInformation("No receipts found for payment {PaymentId}", paymentId);
                return;
            }

            foreach (var receipt in receiptsToDelete)
            {
                // Delete physical file
                if (File.Exists(receipt.FilePath))
                {
                    try
                    {
                        File.Delete(receipt.FilePath);
                        _logger.LogInformation("Deleted receipt file: {FilePath}", receipt.FilePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete receipt file: {FilePath}", receipt.FilePath);
                    }
                }

                // Delete from database
                await _receiptRepository.DeleteAsync(receipt, cancellationToken);
            }

            _logger.LogInformation("Deleted {Count} receipt(s) for payment {PaymentId}", receiptsToDelete.Count, paymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting receipts for payment {PaymentId}", paymentId);
            throw;
        }
    }

    #endregion
}
