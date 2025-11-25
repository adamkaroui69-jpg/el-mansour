using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.ViewModels.Base;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ElMansourSyndicManager.ViewModels;

/// <summary>
/// ViewModel for the reports page
/// </summary>
public class ReportsViewModel : ViewModelBase, IInitializable
{
    private readonly IReportingService _reportingService;
    private readonly IAuthenticationService _authService;
    private MonthlyReportDto? _currentMonthlyReport;
    private YearlyReportDto? _currentYearlyReport;
    private string _selectedReportType = "Monthly";
    private ReportHistoryDTO? _selectedReport;
    private DateTime _selectedMonth = DateTime.Now;
    private int _selectedYear = DateTime.Now.Year;
    private bool _isLoading;
    private bool _isGenerating;

    public ReportsViewModel(
        IReportingService reportingService,
        IAuthenticationService authService)
    {
        _reportingService = reportingService;
        _authService = authService;

        ReportHistory = new ObservableCollection<ReportHistoryDTO>();
        MonthlyChartData = new ObservableCollection<ChartDataPoint>();
        ExpenseChartData = new ObservableCollection<ChartDataPoint>();

        GenerateReportCommand = new RelayCommand(async () => await GenerateReportAsync(), () => !IsGenerating);
        ExportPdfCommand = new RelayCommand(async () => await ExportPdfAsync(), () => CurrentReportAvailable);
        ExportExcelCommand = new RelayCommand(async () => await ExportExcelAsync(), () => CurrentReportAvailable);
        ViewReportCommand = new RelayCommand<ReportHistoryDTO>(async r => await ViewReportAsync(r), r => r != null);
        DeleteReportCommand = new RelayCommand<ReportHistoryDTO>(async r => await DeleteReportAsync(r), r => r != null && IsAdmin);
        DownloadReportCommand = new RelayCommand<ReportHistoryDTO>(async r => await DownloadReportAsync(r), r => r != null);
        LoadHistoryCommand = new RelayCommand(async () => await LoadHistoryAsync(), () => !IsLoading);
        SetReportTypeCommand = new RelayCommand<string>(type => SelectedReportType = type);
        SetExportFormatCommand = new RelayCommand<string>(format => SelectedExportFormat = format);

        // Load initial data
    }

    public async Task InitializeAsync()
    {
        await LoadHistoryAsync();
    }

    public ObservableCollection<ReportHistoryDTO> ReportHistory { get; }
    public ObservableCollection<ChartDataPoint> MonthlyChartData { get; }
    public ObservableCollection<ChartDataPoint> ExpenseChartData { get; }

    public MonthlyReportDto? CurrentMonthlyReport
    {
        get => _currentMonthlyReport;
        set
        {
            if (SetProperty(ref _currentMonthlyReport, value))
            {
                UpdateChartData();
                RaisePropertyChanged(nameof(CurrentReportAvailable));
            }
        }
    }

    public YearlyReportDto? CurrentYearlyReport
    {
        get => _currentYearlyReport;
        set
        {
            if (SetProperty(ref _currentYearlyReport, value))
            {
                UpdateChartData();
                RaisePropertyChanged(nameof(CurrentReportAvailable));
            }
        }
    }

    public string SelectedReportType
    {
        get => _selectedReportType;
        set
        {
            if (SetProperty(ref _selectedReportType, value))
            {
                RaisePropertyChanged(nameof(CurrentReportAvailable));
                RaisePropertyChanged(nameof(IsMonthlyReport));
                RaisePropertyChanged(nameof(IsYearlyReport));
            }
        }
    }

    public bool IsMonthlyReport => SelectedReportType == "Monthly";
    public bool IsYearlyReport => SelectedReportType == "Yearly";

    public DateTime SelectedMonth
    {
        get => _selectedMonth;
        set => SetProperty(ref _selectedMonth, value);
    }

    public int SelectedYear
    {
        get => _selectedYear;
        set
        {
            if (SetProperty(ref _selectedYear, value))
            {
                // Update SelectedMonth when year changes to keep consistency
                // Ensure month is valid (though unlikely to fail with 1-12)
                try 
                {
                    SelectedMonth = new DateTime(value, SelectedMonthNumber, 1);
                }
                catch
                {
                    // Fallback if something goes wrong (e.g. leap year edge case if we were tracking days)
                    SelectedMonth = new DateTime(value, 1, 1);
                    SelectedMonthNumber = 1;
                }
            }
        }
    }

    // Month and Year selection for dropdowns
    private int _selectedMonthNumber = DateTime.Now.Month;
    public int SelectedMonthNumber
    {
        get => _selectedMonthNumber;
        set
        {
            if (SetProperty(ref _selectedMonthNumber, value))
            {
                // Update SelectedMonth when month number changes
                SelectedMonth = new DateTime(SelectedYear, value, 1);
            }
        }
    }

    public List<MonthItem> Months { get; } = new List<MonthItem>
    {
        new MonthItem { Number = 1, Name = "Janvier" },
        new MonthItem { Number = 2, Name = "Février" },
        new MonthItem { Number = 3, Name = "Mars" },
        new MonthItem { Number = 4, Name = "Avril" },
        new MonthItem { Number = 5, Name = "Mai" },
        new MonthItem { Number = 6, Name = "Juin" },
        new MonthItem { Number = 7, Name = "Juillet" },
        new MonthItem { Number = 8, Name = "Août" },
        new MonthItem { Number = 9, Name = "Septembre" },
        new MonthItem { Number = 10, Name = "Octobre" },
        new MonthItem { Number = 11, Name = "Novembre" },
        new MonthItem { Number = 12, Name = "Décembre" }
    };

    public bool IsAdmin => _authService.CurrentUser?.Role == "Admin";
    
    private string _selectedExportFormat = "PDF";
    public string SelectedExportFormat
    {
        get => _selectedExportFormat;
        set => SetProperty(ref _selectedExportFormat, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsGenerating
    {
        get => _isGenerating;
        set
        {
            SetProperty(ref _isGenerating, value);
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public bool CurrentReportAvailable => 
        (SelectedReportType == "Monthly" && CurrentMonthlyReport != null) ||
        (SelectedReportType == "Yearly" && CurrentYearlyReport != null);

    public ICommand GenerateReportCommand { get; }
    public ICommand ExportPdfCommand { get; }
    public ICommand ExportExcelCommand { get; }
    public ICommand ViewReportCommand { get; }
    public ICommand DeleteReportCommand { get; }
    public ICommand DownloadReportCommand { get; }
    public ICommand LoadHistoryCommand { get; }
    public ICommand SetReportTypeCommand { get; }
    public ICommand SetExportFormatCommand { get; }

    public ReportHistoryDTO? SelectedReport
    {
        get => _selectedReport;
        set => SetProperty(ref _selectedReport, value);
    }

    private async Task GenerateReportAsync()
    {
        try
        {
            IsGenerating = true;
            IsLoading = true;

            if (SelectedReportType == "Monthly")
            {
                CurrentMonthlyReport = await _reportingService.GenerateMonthlyReportAsync(SelectedMonth, SelectedExportFormat);
                CurrentYearlyReport = null;
            }
            else
            {
                CurrentYearlyReport = await _reportingService.GenerateYearlyReportAsync(SelectedYear, SelectedExportFormat);
                CurrentMonthlyReport = null;
            }

            await LoadHistoryAsync();

            MessageBox.Show(
                "Rapport généré avec succès",
                "Succès",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur lors de la génération du rapport: {ex.Message}",
                "Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsGenerating = false;
            IsLoading = false;
        }
    }

    private async Task ExportPdfAsync()
    {
        try
        {
            IsLoading = true;

            byte[] pdfBytes;
            string fileName;

            if (SelectedReportType == "Monthly" && CurrentMonthlyReport != null)
            {
                pdfBytes = await _reportingService.ExportMonthlyReportToPdfAsync(SelectedMonth);
                fileName = $"RapportMensuel_{SelectedMonth:yyyy-MM}.pdf";
            }
            else if (SelectedReportType == "Yearly" && CurrentYearlyReport != null)
            {
                pdfBytes = await _reportingService.ExportYearlyReportToPdfAsync(SelectedYear);
                fileName = $"RapportAnnuel_{SelectedYear}.pdf";
            }
            else
            {
                MessageBox.Show("Aucun rapport disponible pour l'export", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                FileName = fileName,
                Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*",
                DefaultExt = "pdf"
            };

            if (saveDialog.ShowDialog() == true)
            {
                await File.WriteAllBytesAsync(saveDialog.FileName, pdfBytes);
                MessageBox.Show(
                    "Rapport exporté en PDF avec succès",
                    "Succès",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur lors de l'export PDF: {ex.Message}",
                "Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExportExcelAsync()
    {
        try
        {
            IsLoading = true;

            byte[] excelBytes;
            string fileName;

            if (SelectedReportType == "Monthly" && CurrentMonthlyReport != null)
            {
                excelBytes = await _reportingService.ExportMonthlyReportToExcelAsync(SelectedMonth);
                fileName = $"RapportMensuel_{SelectedMonth:yyyy-MM}.xlsx";
            }
            else if (SelectedReportType == "Yearly" && CurrentYearlyReport != null)
            {
                excelBytes = await _reportingService.ExportYearlyReportToExcelAsync(SelectedYear);
                fileName = $"RapportAnnuel_{SelectedYear}.xlsx";
            }
            else
            {
                MessageBox.Show("Aucun rapport disponible pour l'export", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                FileName = fileName,
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                DefaultExt = "xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                await File.WriteAllBytesAsync(saveDialog.FileName, excelBytes);
                MessageBox.Show(
                    "Rapport exporté en Excel avec succès",
                    "Succès",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur lors de l'export Excel: {ex.Message}",
                "Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ViewReportAsync(ReportHistoryDTO report)
    {
        try
        {
            if (File.Exists(report.FilePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = report.FilePath,
                    UseShellExecute = true
                });
            }
            else
            {
                // Try to download from cloud
                var fileBytes = await _reportingService.GetReportFileAsync(report.Id);
                if (fileBytes != null)
                {
                    var tempFile = Path.Combine(Path.GetTempPath(), report.FileName);
                    await File.WriteAllBytesAsync(tempFile, fileBytes);
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = tempFile,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show(
                        "Le fichier du rapport n'a pas pu être trouvé.",
                        "Erreur",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur lors de l'ouverture du rapport: {ex.Message}",
                "Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async Task DownloadReportAsync(ReportHistoryDTO report)
    {
        if (report == null) return;

        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                FileName = report.FileName,
                Filter = report.FileName.EndsWith(".pdf") ? "PDF Files (*.pdf)|*.pdf" : "CSV Files (*.csv)|*.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                File.Copy(report.FilePath, saveFileDialog.FileName, true);
                MessageBox.Show("Rapport téléchargé avec succès !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du téléchargement : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task DeleteReportAsync(ReportHistoryDTO report)
    {
        if (report == null) return;

        var result = MessageBox.Show(
            $"Êtes-vous sûr de vouloir supprimer le rapport {report.ReportType} de {report.Period} ?",
            "Confirmation de suppression",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                IsLoading = true;
                await _reportingService.DeleteReportAsync(report.FilePath);
                
                // Remove from list
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ReportHistory.Remove(report);
                });
                
                MessageBox.Show("Rapport supprimé avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la suppression du rapport : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    private async Task LoadHistoryAsync()
    {
        try
        {
            IsLoading = true;
            var history = await _reportingService.GetReportHistoryAsync();
            
            // Update on UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                ReportHistory.Clear();
                foreach (var report in history)
                {
                    ReportHistory.Add(report);
                }
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur lors du chargement de l'historique: {ex.Message}",
                "Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateChartData()
    {
        MonthlyChartData.Clear();
        ExpenseChartData.Clear();

        if (CurrentMonthlyReport != null)
        {
            // Building breakdown chart
            foreach (var building in CurrentMonthlyReport.BuildingBreakdown)
            {
                MonthlyChartData.Add(new ChartDataPoint
                {
                    Label = $"Bâtiment {building.Key}",
                    Value = building.Value
                });
            }

            // Expense category breakdown chart
            foreach (var category in CurrentMonthlyReport.ExpenseCategoryBreakdown)
            {
                ExpenseChartData.Add(new ChartDataPoint
                {
                    Label = category.Key,
                    Value = category.Value
                });
            }
        }
        else if (CurrentYearlyReport != null)
        {
            // Monthly breakdown for yearly report
            foreach (var monthly in CurrentYearlyReport.MonthlyBreakdown.OrderBy(m => m.Key))
            {
                MonthlyChartData.Add(new ChartDataPoint
                {
                    Label = FormatMonth(monthly.Key),
                    Value = monthly.Value.TotalCollected
                });
            }

            // Yearly expense breakdown
            foreach (var category in CurrentYearlyReport.ExpenseCategoryYearlyBreakdown)
            {
                ExpenseChartData.Add(new ChartDataPoint
                {
                    Label = category.Key,
                    Value = category.Value
                });
            }
        }
    }

    private string FormatMonth(string month)
    {
        if (DateTime.TryParseExact($"{month}-01", "yyyy-MM-dd", null,
            System.Globalization.DateTimeStyles.None, out var date))
        {
            return date.ToString("MMM yyyy", new System.Globalization.CultureInfo("fr-FR"));
        }
        return month;
    }
}

public class MonthItem
{
    public int Number { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ChartDataPoint
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}
