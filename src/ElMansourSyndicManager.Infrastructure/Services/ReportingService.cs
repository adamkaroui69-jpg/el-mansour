using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Exceptions;
using ElMansourSyndicManager.Core.Domain.Entities;
using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ElMansourSyndicManager.Infrastructure.Services;

/// <summary>
/// Service for generating financial reports (monthly and yearly)
/// </summary>
public class ReportingService : IReportingService
{
    private readonly IPaymentService _paymentService;
    private readonly IExpenseService _expenseService;
    private readonly IHouseRepository _houseRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuditService _auditService;
    private readonly IAuthenticationService _authService;
    private readonly ILogger<ReportingService> _logger;
    private readonly string _reportsBasePath;

    public ReportingService(
        IPaymentService paymentService,
        IExpenseService expenseService,
        IHouseRepository houseRepository,
        IUserRepository userRepository,
        IAuditService auditService,
        IAuthenticationService authService,
        ILogger<ReportingService> logger)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        _paymentService = paymentService;
        _expenseService = expenseService;
        _houseRepository = houseRepository;
        _userRepository = userRepository;
        _auditService = auditService;
        _authService = authService;
        _logger = logger;

        _reportsBasePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "data",
            "reports");

        Directory.CreateDirectory(_reportsBasePath);
    }

    /// <summary>
    /// Generates a monthly financial report
    /// </summary>
    public async Task<MonthlyReportDto> GenerateMonthlyReportAsync(
        DateTime month, 
        string format = "PDF",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var monthString = month.ToString("yyyy-MM");
            _logger.LogInformation("Generating monthly report for {Month} in format {Format}", monthString, format);

            var payments = await _paymentService.GetPaymentsByMonthAsync(monthString, cancellationToken);
            var paidPayments = payments.Where(p => p.Status == "Paid").ToList();

            var expenses = await _expenseService.GetExpensesByMonthAsync(month.Year, month.Month, cancellationToken);
            var expensesList = expenses.ToList();

            var unpaidHouses = await _paymentService.GetUnpaidHousesAsync(monthString, cancellationToken);

            var allHouses = await _houseRepository.GetAllActiveAsync(cancellationToken);
            var housesList = allHouses.ToList();

            var totalCollected = paidPayments.Sum(p => p.Amount);
            var totalSpent = expensesList.Sum(e => e.Amount);
            var balance = totalCollected - totalSpent;

            var paidHousesCount = paidPayments.Select(p => p.HouseCode).Distinct().Count();
            var totalHousesCount = housesList.Count;
            var unpaidHousesCount = totalHousesCount - paidHousesCount;
            var collectionRate = totalHousesCount > 0 ? (decimal)paidHousesCount / totalHousesCount * 100 : 0;

            var averagePaymentDelay = CalculateAveragePaymentDelay(paidPayments, month);

            var buildingBreakdown = CalculateBuildingBreakdown(paidPayments, housesList);

            var expenseCategoryBreakdown = expensesList
                .GroupBy(e => e.Category)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

            var report = new MonthlyReportDto
            {
                Id = Guid.NewGuid(),
                Month = month,
                TotalCollected = totalCollected,
                TotalSpent = totalSpent,
                Balance = balance,
                PaidHousesCount = paidHousesCount,
                UnpaidHousesCount = unpaidHousesCount,
                TotalHousesCount = totalHousesCount,
                CollectionRate = collectionRate,
                AveragePaymentDelay = averagePaymentDelay,
                Payments = paidPayments,
                Expenses = expensesList,
                UnpaidHouses = unpaidHouses.ToList(),
                BuildingBreakdown = buildingBreakdown,
                ExpenseCategoryBreakdown = expenseCategoryBreakdown,
                GeneratedAt = DateTime.UtcNow,
                GeneratedBy = _authService.CurrentUser?.Username ?? "System"
            };

            await _auditService.LogActivityAsync(new AuditLogDto
            {
                UserId = _authService.CurrentUser?.Id.ToString(),
                Action = "Generate",
                EntityType = "Report",
                EntityId = report.Id.ToString(),
                Details = $"{{\"type\":\"Monthly\",\"month\":\"{monthString}\"}}"
            }, cancellationToken);

            await SaveReportToDiskAsync(report, "Monthly", monthString, format);

            _logger.LogInformation("Monthly report generated successfully for {Month}", monthString);

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating monthly report for {Month}", month);
            throw;
        }
    }

    /// <summary>
    /// Generates a yearly financial report
    /// </summary>
    public async Task<YearlyReportDto> GenerateYearlyReportAsync(
        int year, 
        string format = "PDF",
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating yearly report for {Year} in format {Format}", year, format);

            var monthlyBreakdown = new Dictionary<string, MonthlyReportDto>();

            for (int month = 1; month <= 12; month++)
            {
                var monthDate = new DateTime(year, month, 1);
                var monthReport = await GenerateMonthlyReportAsync(monthDate, format, cancellationToken);
                monthlyBreakdown[monthDate.ToString("MMMM yyyy", new CultureInfo("fr-FR"))] = monthReport;
            }

            var totalCollected = monthlyBreakdown.Values.Sum(m => m.TotalCollected);
            var totalSpent = monthlyBreakdown.Values.Sum(m => m.TotalSpent);
            var balance = totalCollected - totalSpent;

            var expenseCategoryYearlyBreakdown = monthlyBreakdown.Values
                .SelectMany(m => m.ExpenseCategoryBreakdown)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.Sum(kvp => kvp.Value));

            var report = new YearlyReportDto
            {
                Id = Guid.NewGuid(),
                Year = year,
                TotalCollected = totalCollected,
                TotalSpent = totalSpent,
                Balance = balance,
                MonthlyBreakdown = monthlyBreakdown,
                ExpenseCategoryYearlyBreakdown = expenseCategoryYearlyBreakdown,
                GeneratedAt = DateTime.UtcNow,
                GeneratedBy = _authService.CurrentUser?.Username ?? "System"
            };

            await _auditService.LogActivityAsync(new AuditLogDto
            {
                UserId = _authService.CurrentUser?.Id.ToString(),
                Action = "Generate",
                EntityType = "Report",
                EntityId = report.Id.ToString(),
                Details = $"{{\"type\":\"Yearly\",\"year\":{year}}}"
            }, cancellationToken);

            await SaveReportToDiskAsync(report, "Yearly", year.ToString(), format);

            _logger.LogInformation("Yearly report generated successfully for {Year}", year);

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating yearly report for {Year}", year);
            throw;
        }
    }

    /// <summary>
    /// Gets report history
    /// </summary>
    public async Task<List<ReportHistoryDTO>> GetReportHistoryAsync(
        string? periodType = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var reports = new List<ReportHistoryDTO>();

            if (Directory.Exists(_reportsBasePath))
            {
                var files = Directory.GetFiles(_reportsBasePath, "*.*", SearchOption.AllDirectories)
                    .Where(f => f.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) || 
                               f.EndsWith(".csv", StringComparison.OrdinalIgnoreCase));

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    var fileName = fileInfo.Name;
                    
                    var reportType = fileName.Contains("Monthly") ? "Monthly" : "Yearly";
                    var format = fileInfo.Extension.ToUpper().Replace(".", "");

                    if (periodType == null || reportType == periodType)
                    {
                        var currentUser = await _authService.GetCurrentUserAsync();
                        reports.Add(new ReportHistoryDTO
                        {
                            Id = Guid.NewGuid(),
                            ReportType = reportType,
                            Period = ExtractPeriodFromFileName(fileName),
                            FileName = fileName,
                            FilePath = file,
                            FileSize = fileInfo.Length,
                            Format = format,
                            GeneratedAt = fileInfo.CreationTime,
                            GeneratedBy = currentUser?.Username ?? "System"
                        });
                    }
                }
            }

            return reports.OrderByDescending(r => r.GeneratedAt).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report history");
            throw;
        }
    }

    /// <summary>
    /// Gets report file
    /// </summary>
    public async Task<byte[]?> GetReportFileAsync(
        Guid reportId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var reports = await GetReportHistoryAsync(null, cancellationToken);
            var report = reports.FirstOrDefault(r => r.Id == reportId);

            if (report != null && File.Exists(report.FilePath))
            {
                return await File.ReadAllBytesAsync(report.FilePath, cancellationToken);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report file");
            throw;
        }
    }

    public async Task<byte[]> ExportMonthlyReportToPdfAsync(DateTime month, CancellationToken cancellationToken = default)
    {
        var report = await GenerateMonthlyReportAsync(month, "PDF", cancellationToken);
        return await GenerateMonthlyReportPdfAsync(report);
    }

    public async Task<byte[]> ExportYearlyReportToPdfAsync(int year, CancellationToken cancellationToken = default)
    {
        var report = await GenerateYearlyReportAsync(year, "PDF", cancellationToken);
        return await GenerateYearlyReportPdfAsync(report);
    }

    public async Task<byte[]> ExportMonthlyReportToExcelAsync(DateTime month, CancellationToken cancellationToken = default)
    {
        var report = await GenerateMonthlyReportAsync(month, "Excel", cancellationToken);
        return await GenerateMonthlyReportExcelAsync(report);
    }

    public async Task<byte[]> ExportYearlyReportToExcelAsync(int year, CancellationToken cancellationToken = default)
    {
        var report = await GenerateYearlyReportAsync(year, "Excel", cancellationToken);
        return await GenerateYearlyReportExcelAsync(report);
    }

    public Task DeleteReportAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = Path.GetFullPath(filePath);
            var reportsDir = Path.GetFullPath(_reportsBasePath);
            
            if (!fullPath.StartsWith(reportsDir, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Attempted to delete file outside reports directory: {FilePath}", filePath);
                throw new UnauthorizedAccessException("Cannot delete files outside the reports directory.");
            }

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Deleted report file: {FilePath}", fullPath);
            }
            else
            {
                _logger.LogWarning("Report file not found for deletion: {FilePath}", fullPath);
                throw new FileNotFoundException("Report file not found", fullPath);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting report file: {FilePath}", filePath);
            throw;
        }
    }

    #region Private Methods - PDF Generation

    private Task<byte[]> GenerateMonthlyReportPdfAsync(MonthlyReportDto report)
    {
        try
        {
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
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
                                column.Item().AlignCenter().Width(50).Image(logoPath);
                                column.Item().PaddingTop(8);
                            }
                            
                            // Title - smaller and centered
                            column.Item().AlignCenter().Text($"Rapport Mensuel - {report.Month:MMMM yyyy}")
                                .SemiBold().FontSize(14).FontColor(Colors.Blue.Darken2);
                            
                            // Decorative line
                            column.Item().PaddingTop(8).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                        });

                    page.Content()
                        .PaddingVertical(0.8f, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(15);

                            // Financial Summary - highlighted section
                            x.Item().Column(summary =>
                            {
                                summary.Item().Text("Résumé Financier").FontSize(12).SemiBold().FontColor(Colors.Blue.Darken2);
                                summary.Item().PaddingTop(5).Border(2).BorderColor(Colors.Blue.Darken2)
                                    .Background(Colors.Blue.Lighten5).Padding(12).Column(content =>
                                {
                                    content.Spacing(8);
                                    
                                    content.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text("Total Collecté:").FontSize(10).FontColor(Colors.Grey.Darken1);
                                        row.RelativeItem().AlignRight().Text($"{report.TotalCollected:N2} TND").SemiBold().FontSize(11);
                                    });
                                    
                                    content.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text("Total Dépensé:").FontSize(10).FontColor(Colors.Grey.Darken1);
                                        row.RelativeItem().AlignRight().Text($"{report.TotalSpent:N2} TND").SemiBold().FontSize(11);
                                    });
                                    
                                    content.Item().LineHorizontal(1).LineColor(Colors.Blue.Darken1);
                                    
                                    content.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text("Balance:").Bold().FontSize(11).FontColor(Colors.Blue.Darken3);
                                        row.RelativeItem().AlignRight().Text($"{report.Balance:N2} TND").Bold().FontSize(12).FontColor(Colors.Blue.Darken3);
                                    });
                                });
                            });
                            
                            // Statistics section
                            x.Item().Column(stats =>
                            {
                                stats.Item().Text("Statistiques").FontSize(12).SemiBold().FontColor(Colors.Blue.Darken2);
                                stats.Item().PaddingTop(5).Background(Colors.Grey.Lighten4).Padding(12).Column(content =>
                                {
                                    content.Spacing(6);
                                    
                                    content.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text("Maisons Payées:").FontSize(9).FontColor(Colors.Grey.Darken1);
                                        row.RelativeItem().AlignRight().Text($"{report.PaidHousesCount} / {report.TotalHousesCount}").SemiBold();
                                    });
                                    
                                    content.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text("Taux de Collection:").FontSize(9).FontColor(Colors.Grey.Darken1);
                                        row.RelativeItem().AlignRight().Text($"{report.CollectionRate:N2} %").SemiBold();
                                    });
                                });
                            });

                            if (report.Payments != null && report.Payments.Any())
                            {
                                x.Item().Column(payments =>
                                {
                                    payments.Item().Text("Détail des Paiements").FontSize(12).SemiBold().FontColor(Colors.Blue.Darken2);
                                    payments.Item().PaddingTop(5).Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.ConstantColumn(80);
                                            columns.ConstantColumn(60);
                                            columns.RelativeColumn();
                                            columns.ConstantColumn(80);
                                        });
                                        
                                        table.Header(header =>
                                        {
                                            header.Cell().Element(CellStyle).Text("Date");
                                            header.Cell().Element(CellStyle).Text("Maison");
                                            header.Cell().Element(CellStyle).Text("Statut");
                                            header.Cell().Element(CellStyle).Text("Montant").AlignRight();
                                            
                                            static IContainer CellStyle(IContainer container)
                                            {
                                                return container.Background(Colors.Blue.Lighten4).DefaultTextStyle(x => x.SemiBold().FontSize(9))
                                                    .Padding(5).BorderBottom(2).BorderColor(Colors.Blue.Darken2);
                                            }
                                        });

                                        foreach (var payment in report.Payments)
                                        {
                                            table.Cell().Element(CellStyle).Text($"{payment.PaymentDate:dd/MM/yyyy}");
                                            table.Cell().Element(CellStyle).Text(payment.HouseCode);
                                            table.Cell().Element(CellStyle).Text(payment.Status);
                                            table.Cell().Element(CellStyle).Text($"{payment.Amount:N2}").AlignRight();
                                            
                                            static IContainer CellStyle(IContainer container)
                                            {
                                                return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4);
                                            }
                                        }
                                    });
                                });
                            }

                            if (report.Expenses != null && report.Expenses.Any())
                            {
                                x.Item().Column(expenses =>
                                {
                                    expenses.Item().Text("Détail des Dépenses").FontSize(12).SemiBold().FontColor(Colors.Blue.Darken2);
                                    expenses.Item().PaddingTop(5).Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.ConstantColumn(80);
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                            columns.ConstantColumn(80);
                                        });
                                        
                                        table.Header(header =>
                                        {
                                            header.Cell().Element(CellStyle).Text("Date");
                                            header.Cell().Element(CellStyle).Text("Catégorie");
                                            header.Cell().Element(CellStyle).Text("Description");
                                            header.Cell().Element(CellStyle).Text("Montant").AlignRight();
                                            
                                            static IContainer CellStyle(IContainer container)
                                            {
                                                return container.Background(Colors.Blue.Lighten4).DefaultTextStyle(x => x.SemiBold().FontSize(9))
                                                    .Padding(5).BorderBottom(2).BorderColor(Colors.Blue.Darken2);
                                            }
                                        });

                                        foreach (var expense in report.Expenses)
                                        {
                                            table.Cell().Element(CellStyle).Text($"{expense.ExpenseDate:dd/MM/yyyy}");
                                            table.Cell().Element(CellStyle).Text(expense.Category);
                                            table.Cell().Element(CellStyle).Text(expense.Description);
                                            table.Cell().Element(CellStyle).Text($"{expense.Amount:N2}").AlignRight();
                                            
                                            static IContainer CellStyle(IContainer container)
                                            {
                                                return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4);
                                            }
                                        }
                                    });
                                });
                            }
                        });

                    // Footer - simple, no page number
                    page.Footer()
                        .AlignCenter()
                        .PaddingTop(10)
                        .Text("Résidence El Mansour")
                        .FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });

            return Task.FromResult(document.GeneratePdf());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF report");
            throw;
        }
    }

    private Task<byte[]> GenerateYearlyReportPdfAsync(YearlyReportDto report)
    {
        try
        {
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
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
                                column.Item().AlignCenter().Width(50).Image(logoPath);
                                column.Item().PaddingTop(8);
                            }
                            
                            // Title - smaller and centered
                            column.Item().AlignCenter().Text($"Rapport Annuel - {report.Year}")
                                .SemiBold().FontSize(14).FontColor(Colors.Blue.Darken2);
                            
                            // Decorative line
                            column.Item().PaddingTop(8).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                        });

                    page.Content()
                        .PaddingVertical(0.8f, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(15);

                            // Financial Summary - highlighted section
                            x.Item().Column(summary =>
                            {
                                summary.Item().Text("Résumé Financier").FontSize(12).SemiBold().FontColor(Colors.Blue.Darken2);
                                summary.Item().PaddingTop(5).Border(2).BorderColor(Colors.Blue.Darken2)
                                    .Background(Colors.Blue.Lighten5).Padding(12).Column(content =>
                                {
                                    content.Spacing(8);
                                    
                                    content.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text("Total Collecté:").FontSize(10).FontColor(Colors.Grey.Darken1);
                                        row.RelativeItem().AlignRight().Text($"{report.TotalCollected:N2} TND").SemiBold().FontSize(11);
                                    });
                                    
                                    content.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text("Total Dépensé:").FontSize(10).FontColor(Colors.Grey.Darken1);
                                        row.RelativeItem().AlignRight().Text($"{report.TotalSpent:N2} TND").SemiBold().FontSize(11);
                                    });
                                    
                                    content.Item().LineHorizontal(1).LineColor(Colors.Blue.Darken1);
                                    
                                    content.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text("Balance:").Bold().FontSize(11).FontColor(Colors.Blue.Darken3);
                                        row.RelativeItem().AlignRight().Text($"{report.Balance:N2} TND").Bold().FontSize(12).FontColor(Colors.Blue.Darken3);
                                    });
                                });
                            });
                            
                            if (report.MonthlyBreakdown != null && report.MonthlyBreakdown.Any())
                            {
                                x.Item().Column(monthly =>
                                {
                                    monthly.Item().Text("Détail Mensuel").FontSize(12).SemiBold().FontColor(Colors.Blue.Darken2);
                                    monthly.Item().PaddingTop(5).Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                        });
                                        
                                        table.Header(header =>
                                        {
                                            header.Cell().Element(CellStyle).Text("Mois");
                                            header.Cell().Element(CellStyle).Text("Collecté").AlignRight();
                                            header.Cell().Element(CellStyle).Text("Dépensé").AlignRight();
                                            header.Cell().Element(CellStyle).Text("Balance").AlignRight();
                                            
                                            static IContainer CellStyle(IContainer container)
                                            {
                                                return container.Background(Colors.Blue.Lighten4).DefaultTextStyle(x => x.SemiBold().FontSize(9))
                                                    .Padding(5).BorderBottom(2).BorderColor(Colors.Blue.Darken2);
                                            }
                                        });

                                        foreach (var month in report.MonthlyBreakdown
                                            .OrderBy(kvp => DateTime.ParseExact(kvp.Key, "MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"))))
                                        {
                                            var m = month.Value;
                                            table.Cell().Element(CellStyle).Text(month.Key);
                                            table.Cell().Element(CellStyle).Text($"{m.TotalCollected:N2}").AlignRight();
                                            table.Cell().Element(CellStyle).Text($"{m.TotalSpent:N2}").AlignRight();
                                            table.Cell().Element(CellStyle).Text($"{m.Balance:N2}").AlignRight();
                                            
                                            static IContainer CellStyle(IContainer container)
                                            {
                                                return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4);
                                            }
                                        }
                                    });
                                });
                            }
                        });

                    // Footer - simple, no page number
                    page.Footer()
                        .AlignCenter()
                        .PaddingTop(10)
                        .Text("Résidence El Mansour")
                        .FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });

            return Task.FromResult(document.GeneratePdf());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF report");
            throw;
        }
    }

    #endregion

    #region Private Methods - Excel Generation

    private Task<byte[]> GenerateMonthlyReportExcelAsync(MonthlyReportDto report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Rapport Mensuel;" + report.Month.ToString("MMMM yyyy"));
        sb.AppendLine();
        sb.AppendLine("Résumé Financier");
        sb.AppendLine("Total Collecté;" + report.TotalCollected);
        sb.AppendLine("Total Dépensé;" + report.TotalSpent);
        sb.AppendLine("Balance;" + report.Balance);
        sb.AppendLine();
        sb.AppendLine("Statistiques");
        sb.AppendLine("Maisons Payées;" + report.PaidHousesCount + " / " + report.TotalHousesCount);
        sb.AppendLine("Taux de Collection;" + report.CollectionRate + "%");
        sb.AppendLine("Délai Moyen;" + report.AveragePaymentDelay + " jours");
        sb.AppendLine();
        
        if(report.Payments != null && report.Payments.Any())
        {
            sb.AppendLine("Détail des Paiements");
            sb.AppendLine("Date;Maison;Montant;Statut");
            foreach(var p in report.Payments)
            {
                sb.AppendLine($"{p.PaymentDate:dd/MM/yyyy};{p.HouseCode};{p.Amount};{p.Status}");
            }
            sb.AppendLine();
        }

        if(report.Expenses != null && report.Expenses.Any())
        {
            sb.AppendLine("Détail des Dépenses");
            sb.AppendLine("Date;Catégorie;Montant;Description");
            foreach(var e in report.Expenses)
            {
                sb.AppendLine($"{e.ExpenseDate:dd/MM/yyyy};{e.Category};{e.Amount};{e.Description}");
            }
        }

        return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    private Task<byte[]> GenerateYearlyReportExcelAsync(YearlyReportDto report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Rapport Annuel;" + report.Year);
        sb.AppendLine();
        sb.AppendLine("Résumé Financier");
        sb.AppendLine("Total Collecté;" + report.TotalCollected);
        sb.AppendLine("Total Dépensé;" + report.TotalSpent);
        sb.AppendLine("Balance;" + report.Balance);
        sb.AppendLine();
        
        if(report.MonthlyBreakdown != null && report.MonthlyBreakdown.Any())
        {
            sb.AppendLine("Détail Mensuel");
            sb.AppendLine("Mois;Collecté;Dépensé;Balance");
            foreach(var m in report.MonthlyBreakdown
                .OrderBy(kvp => DateTime.ParseExact(kvp.Key, "MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"))))
            {
                sb.AppendLine($"{m.Key};{m.Value.TotalCollected};{m.Value.TotalSpent};{m.Value.Balance}");
            }
        }

        return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    #endregion

    #region Helper Methods

    private Dictionary<string, decimal> CalculateBuildingBreakdown(
        List<PaymentDto> payments, 
        List<House> houses)
    {
        var breakdown = new Dictionary<string, decimal>();

        foreach (var payment in payments)
        {
            var house = houses.FirstOrDefault(h => h.HouseCode == payment.HouseCode);
            if (house != null)
            {
                var buildingCode = house.BuildingCode;
                if (!breakdown.ContainsKey(buildingCode))
                {
                    breakdown[buildingCode] = 0;
                }
                breakdown[buildingCode] += payment.Amount;
            }
        }

        return breakdown;
    }

    private decimal CalculateAveragePaymentDelay(List<PaymentDto> payments, DateTime month)
    {
        if (!payments.Any())
            return 0;

        var monthStart = new DateTime(month.Year, month.Month, 1);
        var delays = payments
            .Where(p => p.PaymentDate.HasValue && p.PaymentDate.Value >= monthStart)
            .Select(p => (p.PaymentDate!.Value - monthStart).Days)
            .Where(d => d >= 0)
            .ToList();

        return delays.Any() ? (decimal)delays.Average() : 0;
    }

    private string FormatMonth(string month)
    {
        if (DateTime.TryParseExact($"{month}-01", "yyyy-MM-dd", null,
            System.Globalization.DateTimeStyles.None, out var date))
        {
            return date.ToString("MMMM yyyy", new CultureInfo("fr-FR"));
        }
        return month;
    }

    private string ExtractPeriodFromFileName(string fileName)
    {
        var parts = fileName.Split('_');
        if (parts.Length >= 2)
        {
            return parts[1].Split('.')[0];
        }
        return "Unknown";
    }

    private async Task SaveReportToDiskAsync(object report, string reportType, string period, string format = "PDF")
    {
        try
        {
            byte[] fileBytes;
            string extension = format == "Excel" ? ".csv" : ".pdf";
            
            if (reportType == "Monthly" && report is MonthlyReportDto monthlyReport)
            {
                if (format == "Excel")
                    fileBytes = await GenerateMonthlyReportExcelAsync(monthlyReport);
                else
                    fileBytes = await GenerateMonthlyReportPdfAsync(monthlyReport);
            }
            else if (reportType == "Yearly" && report is YearlyReportDto yearlyReport)
            {
                if (format == "Excel")
                    fileBytes = await GenerateYearlyReportExcelAsync(yearlyReport);
                else
                    fileBytes = await GenerateYearlyReportPdfAsync(yearlyReport);
            }
            else
            {
                _logger.LogWarning("Unknown report type or invalid report object");
                return;
            }

            var fileName = $"{reportType}Report_{period}{extension}";
            var filePath = Path.Combine(_reportsBasePath, fileName);
            await File.WriteAllBytesAsync(filePath, fileBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving report to disk");
        }
    }

    #endregion
}
