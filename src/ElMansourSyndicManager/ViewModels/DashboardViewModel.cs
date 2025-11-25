using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.IO;

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

    public DashboardViewModel(
        IPaymentService paymentService,
        INotificationService notificationService,
        IExpenseService expenseService)
    {
        _paymentService = paymentService;
        _notificationService = notificationService;
        _expenseService = expenseService;

        UnpaidHouses = new ObservableCollection<UnpaidHouseDto>();
        RecentPayments = new ObservableCollection<PaymentDto>();
        RecentExpenses = new ObservableCollection<ExpenseDto>();

        RefreshCommand = new RelayCommand(async () => await LoadDataAsync());
        NavigateToPaymentsCommand = new RelayCommand(() => { /* Navigate */ });
        NavigateToExpensesCommand = new RelayCommand(() => { /* Navigate */ });

        // Load data on initialization
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

            var currentMonth = DateTime.Now.ToString("yyyy-MM");
            
            // Load unpaid houses
            File.AppendAllText(logPath, $"[{DateTime.Now}] Calling GetUnpaidHousesAsync...\n");
            var unpaidHouses = await _paymentService.GetUnpaidHousesAsync(currentMonth);
            File.AppendAllText(logPath, $"[{DateTime.Now}] GetUnpaidHousesAsync returned {unpaidHouses.Count} items.\n");
            
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
            
            // Calculate Total Due (sum of all unpaid amounts)
            TotalDue = unpaidHouses.Sum(h => h.MonthlyAmount);
            File.AppendAllText(logPath, $"[{DateTime.Now}] Total Due calculated: {TotalDue} TND from {unpaidHouses.Count} unpaid houses.\n");

            // Load recent payments du mois en cours
            File.AppendAllText(logPath, $"[{DateTime.Now}] Calling GetPaymentsByMonthAsync...\n");
            var payments = await _paymentService.GetPaymentsByMonthAsync(currentMonth);
            File.AppendAllText(logPath, $"[{DateTime.Now}] GetPaymentsByMonthAsync returned {payments.Count} items.\n");

            RecentPayments.Clear();
            foreach (var payment in payments.OrderByDescending(p => p.PaymentDate).Take(10))
            {
                RecentPayments.Add(payment);
            }

            // Load recent expenses
            try 
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] Calling GetAllExpensesAsync for recent expenses...\n");
                var allExpenses = await _expenseService.GetAllExpensesAsync();
                File.AppendAllText(logPath, $"[{DateTime.Now}] GetAllExpensesAsync returned {allExpenses.Count()} items.\n");

                RecentExpenses.Clear();
                foreach (var expense in allExpenses.OrderByDescending(e => e.ExpenseDate).Take(5))
                {
                    RecentExpenses.Add(expense);
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] ERROR loading recent expenses: {ex.Message}\n");
                System.Diagnostics.Debug.WriteLine($"Error loading recent expenses: {ex.Message}");
            }

            // Calculate totals using PaymentService statistics
            // Use a wide range to ensure we catch all payments
            var startOfTime = DateTime.MinValue;
            var endOfTime = DateTime.MaxValue;
            
            File.AppendAllText(logPath, $"[{DateTime.Now}] Calling GetPaymentStatisticsAsync...\n");
            var stats = await _paymentService.GetPaymentStatisticsAsync(startOfTime, endOfTime);
            File.AppendAllText(logPath, $"[{DateTime.Now}] GetPaymentStatisticsAsync returned. TotalCollected: {stats.TotalCollected}\n");
            
            TotalCollected = stats.TotalCollected;
            
            // Calculate Total Spent from Expenses
            try
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] Calling GetAllExpensesAsync...\n");
                var expenses = await _expenseService.GetAllExpensesAsync();
                
                // FALLBACK for Expenses: If GetAll returns 0, try fetching by month
                if (!expenses.Any())
                {
                     File.AppendAllText(logPath, $"[{DateTime.Now}] GetAllExpensesAsync returned 0. Trying fallback (GetExpensesByMonthAsync)...\n");
                     var fallbackExpenses = new List<ExpenseDto>();
                     var current = DateTime.Now;
                     // Check last 24 months + next 1 month
                     for (int i = -1; i < 24; i++)
                     {
                         var d = current.AddMonths(-i);
                         var monthExpenses = await _expenseService.GetExpensesByMonthAsync(d.Year, d.Month);
                         if (monthExpenses.Any())
                         {
                             fallbackExpenses.AddRange(monthExpenses);
                             File.AppendAllText(logPath, $"  Found {monthExpenses.Count()} expenses in {d:yyyy-MM}\n");
                         }
                     }
                     expenses = fallbackExpenses;
                     File.AppendAllText(logPath, $"[{DateTime.Now}] Fallback found total {expenses.Count()} expenses.\n");
                }

                TotalSpent = expenses.Sum(e => e.Amount);
                File.AppendAllText(logPath, $"[{DateTime.Now}] TotalSpent calculated: {TotalSpent}\n");
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] ERROR loading expenses: {ex.Message}\n");
                TotalSpent = 0;
            }

            // Debug
            System.Diagnostics.Debug.WriteLine($"DASHBOARD - Total collecté: {stats.TotalCollected} TND, Nombre de paiements: {stats.PaidCount}, Dépenses: {TotalSpent}");
            
            Balance = TotalCollected - TotalSpent;
            File.AppendAllText(logPath, $"[{DateTime.Now}] DashboardViewModel.LoadDataAsync completed successfully.\n");
        }
        catch (Exception ex)
        {
            // Handle error
            System.Diagnostics.Debug.WriteLine($"Error loading dashboard data: {ex.Message}");
            try
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] ERROR in DashboardViewModel.LoadDataAsync: {ex.Message}\n{ex.StackTrace}\n");
            }
            catch
            {
                // Ignore logging errors
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}
