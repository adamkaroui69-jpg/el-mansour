using ElMansourSyndicManager.Core.Domain.DTOs;

namespace ElMansourSyndicManager.Core.Domain.Interfaces.Services;

/// <summary>
/// Service for generating financial reports
/// </summary>
public interface IReportingService
{
    /// <summary>
    /// Generates a monthly financial report
    /// </summary>
    Task<MonthlyReportDto> GenerateMonthlyReportAsync(DateTime month, string format = "PDF", CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generates a yearly financial report
    /// </summary>
    Task<YearlyReportDto> GenerateYearlyReportAsync(int year, string format = "PDF", CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Exports monthly report to PDF
    /// </summary>
    Task<byte[]> ExportMonthlyReportToPdfAsync(DateTime month, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Exports yearly report to PDF
    /// </summary>
    Task<byte[]> ExportYearlyReportToPdfAsync(int year, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Exports monthly report to Excel
    /// </summary>
    Task<byte[]> ExportMonthlyReportToExcelAsync(DateTime month, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Exports yearly report to Excel
    /// </summary>
    Task<byte[]> ExportYearlyReportToExcelAsync(int year, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets report history
    /// </summary>
Task<List<ReportHistoryDTO>> GetReportHistoryAsync(string? periodType = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets report file (PDF or Excel)
    /// </summary>
    Task<byte[]?> GetReportFileAsync(Guid reportId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a report file
    /// </summary>
    Task DeleteReportAsync(string filePath, CancellationToken cancellationToken = default);
}
