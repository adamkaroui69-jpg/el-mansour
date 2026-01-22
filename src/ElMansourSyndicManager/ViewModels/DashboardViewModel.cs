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

    private readonly IAuthenticationService _authService;
    private string _userName = string.Empty;

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
        // Add a small delay to avoid DbContext concurrency issues at startup
        await Task.Delay(100);
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
        try
        {
            IsLoading = true;

            var user = await _authService.GetCurrentUserAsync();
            UserName = user?.Username ?? "Utilisateur";

            var currentMonth = DateTime.Now.ToString("yyyy-MM");
            var currentYear = DateTime.Now.Year;
            var yearStart = new DateTime(currentYear, 1, 1);
            var yearEnd = new DateTime(currentYear, 12, 31);
            
            // Load unpaid houses for CURRENT MONTH
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
            UnpaidHousesCount = unpaidHouses.Count();

            // Load recent payments (current month)
            var payments = await _paymentService.GetPaymentsByMonthAsync(currentMonth);
            RecentPayments.Clear();
            foreach (var payment in payments.OrderByDescending(p => p.PaymentDate).Take(10))
            {
                RecentPayments.Add(payment);
            }

            // Load recent expenses (current month for display)
            var allExpenses = await _expenseService.GetAllExpensesAsync();
            var currentMonthExpenses = allExpenses.Where(e => e.ExpenseDate.Year == currentYear && e.ExpenseDate.Month == DateTime.Now.Month);
            RecentExpenses.Clear();
            foreach (var expense in currentMonthExpenses.OrderByDescending(e => e.ExpenseDate).Take(5))
            {
                RecentExpenses.Add(expense);
            }

            // Calculate totals for FULL YEAR
            var stats = await _paymentService.GetPaymentStatisticsAsync(yearStart, yearEnd);
            TotalCollected = stats.TotalCollected;
            
            // Total expenses for the year
            var yearExpenses = allExpenses.Where(e => e.ExpenseDate.Year == currentYear);
            TotalSpent = yearExpenses.Sum(e => e.Amount);
            
            // Caisse (Balance) = Collected - Spent for the year
            Balance = TotalCollected - TotalSpent;
            
            // TotalDue is now "Caisse" (same as Balance)
            TotalDue = Balance;
        }
        catch (Exception)
        {
            // Error loading dashboard data - silently fail
        }
        finally
        {
            IsLoading = false;
        }
    }
}
