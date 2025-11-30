using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.IO;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace ElMansourSyndicManager.ViewModels;

/// <summary>
/// ViewModel for the dashboard page
/// </summary>
public class DashboardViewModel : ViewModelBase, IInitializable
{
    private readonly IPaymentService _paymentService;
    private readonly INotificationService _notificationService;
    private readonly IExpenseService _expenseService;
    private decimal _totalCollected;
    private decimal _totalSpent;
    private decimal _balance;
    private int _unpaidHousesCount;
    private bool _isLoading;

    private readonly IAuthenticationService _authService;
    private string _userName;

    public DashboardViewModel(
        IPaymentService paymentService,
        INotificationService notificationService,
        IExpenseService expenseService,
        IAuthenticationService authService)
    {
        _paymentService = paymentService;
        _notificationService = notificationService;
        _expenseService = expenseService;
        _authService = authService;

        UnpaidHouses = new ObservableCollection<UnpaidHouseDto>();
        RecentPayments = new ObservableCollection<PaymentDto>();
        RecentExpenses = new ObservableCollection<ExpenseDto>();

        RefreshCommand = new RelayCommand(async () => await LoadDataAsync());
        NavigateToPaymentsCommand = new RelayCommand(() => { /* Navigate */ });
        NavigateToExpensesCommand = new RelayCommand(() => { /* Navigate */ });

        // Load data on initialization
    }

    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    public async Task InitializeAsync()
    {
        await LoadDataAsync();
    }

    public decimal TotalCollected
    {
        get => _totalCollected;
        set => SetProperty(ref _totalCollected, value);
    }

    public decimal TotalSpent
    {
        get => _totalSpent;
        set => SetProperty(ref _totalSpent, value);
    }

    public decimal Balance
    {
        get => _balance;
        set => SetProperty(ref _balance, value);
    }

    private decimal _totalDue;
    public decimal TotalDue
    {
        get => _totalDue;
        set => SetProperty(ref _totalDue, value);
    }

    public int UnpaidHousesCount
    {
        get => _unpaidHousesCount;
        set => SetProperty(ref _unpaidHousesCount, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ObservableCollection<UnpaidHouseDto> UnpaidHouses { get; }
    public ObservableCollection<PaymentDto> RecentPayments { get; }
    public ObservableCollection<ExpenseDto> RecentExpenses { get; }

    public ICommand RefreshCommand { get; }
    public ICommand NavigateToPaymentsCommand { get; }
    public ICommand NavigateToExpensesCommand { get; }

    public ISeries[] Series { get; set; }
    public Axis[] XAxes { get; set; }

    private async Task LoadDataAsync()
    {
        // Use relative path for logging
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var logDir = Path.Combine(baseDir, "data", "logs");
        if (!Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }
        var logPath = Path.Combine(logDir, "dashboard_debug.txt");
        
        try
        {
            File.AppendAllText(logPath, $"\n[{DateTime.Now}] DashboardViewModel.LoadDataAsync started.\n");
            IsLoading = true;

            var user = await _authService.GetCurrentUserAsync();
            UserName = user?.Username ?? "Utilisateur";

            var currentMonth = DateTime.Now.ToString("yyyy-MM");
            
            // Load unpaid houses
            var unpaidHouses = await _paymentService.GetUnpaidHousesAsync(currentMonth);
            
            // Tri alphabétique puis numérique
            var sortedUnpaidHouses = unpaidHouses.OrderBy(h => 
            {
                var code = h.HouseCode;
                var letterPart = new string(code.TakeWhile(c => !char.IsDigit(c)).ToArray());
                return letterPart;
            })
            .ThenBy(h => 
            {
                var code = h.HouseCode;
                var numberPart = new string(code.SkipWhile(c => !char.IsDigit(c)).ToArray());
                return int.TryParse(numberPart, out var num) ? num : 0;
            });
            
            UnpaidHouses.Clear();
            foreach (var house in sortedUnpaidHouses)
            {
                UnpaidHouses.Add(house);
            }
            UnpaidHousesCount = unpaidHouses.Count;
            TotalDue = unpaidHouses.Sum(h => h.MonthlyAmount);

            // Load recent payments
            var payments = await _paymentService.GetPaymentsByMonthAsync(currentMonth);
            RecentPayments.Clear();
            foreach (var payment in payments.OrderByDescending(p => p.PaymentDate).Take(10))
            {
                RecentPayments.Add(payment);
            }

            // Load recent expenses
            var allExpenses = await _expenseService.GetAllExpensesAsync();
            RecentExpenses.Clear();
            foreach (var expense in allExpenses.OrderByDescending(e => e.ExpenseDate).Take(5))
            {
                RecentExpenses.Add(expense);
            }

            // Calculate totals
            var stats = await _paymentService.GetPaymentStatisticsAsync(DateTime.MinValue, DateTime.MaxValue);
            TotalCollected = stats.TotalCollected;
            TotalSpent = allExpenses.Sum(e => e.Amount);
            Balance = TotalCollected - TotalSpent;

            // --- CHART DATA PREPARATION ---
            var incomeValues = new List<double>();
            var expenseValues = new List<double>();
            var labels = new List<string>();

            var now = DateTime.Now;
            for (int i = 5; i >= 0; i--)
            {
                var date = now.AddMonths(-i);
                var monthStr = date.ToString("yyyy-MM");
                labels.Add(date.ToString("MMM", System.Globalization.CultureInfo.GetCultureInfo("fr-FR")));

                // Monthly Income
                var monthPayments = await _paymentService.GetPaymentsByMonthAsync(monthStr);
                incomeValues.Add((double)monthPayments.Sum(p => p.Amount));

                // Monthly Expenses
                var monthExpenses = await _expenseService.GetExpensesByMonthAsync(date.Year, date.Month);
                expenseValues.Add((double)monthExpenses.Sum(e => e.Amount));
            }

            Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Name = "Revenus",
                    Values = incomeValues.ToArray(),
                    Fill = new SolidColorPaint(SKColors.CornflowerBlue)
                },
                new ColumnSeries<double>
                {
                    Name = "Dépenses",
                    Values = expenseValues.ToArray(),
                    Fill = new SolidColorPaint(SKColors.IndianRed)
                }
            };

            XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = labels.ToArray(),
                    LabelsPaint = new SolidColorPaint(SKColors.Gray)
                }
            };

            OnPropertyChanged(nameof(Series));
            OnPropertyChanged(nameof(XAxes));

            File.AppendAllText(logPath, $"[{DateTime.Now}] DashboardViewModel.LoadDataAsync completed successfully.\n");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading dashboard data: {ex.Message}");
            try { File.AppendAllText(logPath, $"[{DateTime.Now}] ERROR: {ex.Message}\n"); } catch {}
        }
        finally
        {
            IsLoading = false;
        }
    }
}
