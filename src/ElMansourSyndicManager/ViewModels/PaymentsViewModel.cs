using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Entities;
using ElMansourSyndicManager.Core.Domain.Exceptions;
using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Logging;

namespace ElMansourSyndicManager.ViewModels;

/// <summary>
/// ViewModel for the payments page
/// </summary>
public class PaymentsViewModel : ViewModelBase, IInitializable
{
    private readonly IPaymentService _paymentService;
    private readonly IReceiptService _receiptService;
    private readonly IHouseRepository _houseRepository;
    private readonly IAuthenticationService _authService;
    private readonly ILogger<PaymentsViewModel> _logger;
    private PaymentDto? _selectedPayment;
    private string _selectedMonth = DateTime.Now.ToString("yyyy-MM");
    private bool _isLoading;
    private bool _isAdmin;
    private bool _canValidate;

    public PaymentsViewModel(
        IPaymentService paymentService,
        IReceiptService receiptService,
        IHouseRepository houseRepository,
        IAuthenticationService authService,
        ILogger<PaymentsViewModel> logger)
    {
        _paymentService = paymentService;
        _receiptService = receiptService;
        _houseRepository = houseRepository;
        _authService = authService;
        _logger = logger;

        Payments = new ObservableCollection<PaymentDto>();
        Houses = new ObservableCollection<HouseDto>();

        ShowCreateFormCommand = new RelayCommand(ShowCreateForm);
        CancelFormCommand = new RelayCommand(CancelForm);
        CreatePaymentCommand = new RelayCommand(async () => await CreatePaymentAsync());
        MarkAsPaidCommand = new RelayCommand<PaymentDto>(async p => await MarkAsPaidAsync(p), p => p != null && CanValidate);
        GenerateReceiptCommand = new RelayCommand<PaymentDto>(async p => await GenerateReceiptAsync(p), p => p != null && CanValidate);
        DeletePaymentCommand = new RelayCommand<PaymentDto>(async p => await DeletePaymentAsync(p), p => p != null && _isAdmin);
        RefreshCommand = new RelayCommand(async () => await LoadPaymentsAsync());

    }

    public async Task InitializeAsync()
    {
        // Check if user is admin or treasurer
        var currentUser = await _authService.GetCurrentUserAsync();
        IsAdmin = currentUser?.Role == "Admin";
        CanValidate = IsAdmin || currentUser?.Role == "Trésorier";
        
        await LoadPaymentsAsync();
        await LoadHousesAsync();
    }

    public ObservableCollection<PaymentDto> Payments { get; }
    public ObservableCollection<HouseDto> Houses { get; }

    public PaymentDto? SelectedPayment
    {
        get => _selectedPayment;
        set => SetProperty(ref _selectedPayment, value);
    }

    public string SelectedMonth
    {
        get => _selectedMonth;
        set
        {
            if (SetProperty(ref _selectedMonth, value))
            {
                _ = LoadPaymentsAsync();
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsAdmin
    {
        get => _isAdmin;
        set => SetProperty(ref _isAdmin, value);
    }

    public bool CanValidate
    {
        get => _canValidate;
        set => SetProperty(ref _canValidate, value);
    }

    // Form visibility
    private bool _isFormVisible;
    public bool IsFormVisible
    {
        get => _isFormVisible;
        set => SetProperty(ref _isFormVisible, value);
    }

    // Create Payment Properties
    private string _selectedHouseCode = string.Empty;
    public string SelectedHouseCode
    {
        get => _selectedHouseCode;
        set
        {
            if (SetProperty(ref _selectedHouseCode, value))
            {
                var selectedHouse = Houses.FirstOrDefault(h => h.Code == value);
                if (selectedHouse != null)
                {
                    PaymentAmount = selectedHouse.MonthlyAmount;
                }
            }
        }
    }

    private decimal _paymentAmount;
    public decimal PaymentAmount
    {
        get => _paymentAmount;
        set => SetProperty(ref _paymentAmount, value);
    }

    private DateTime _paymentDate = DateTime.Now;
    public DateTime PaymentDate
    {
        get => _paymentDate;
        set => SetProperty(ref _paymentDate, value);
    }

    public ICommand ShowCreateFormCommand { get; }
    public ICommand CancelFormCommand { get; }
    public ICommand CreatePaymentCommand { get; }
    public ICommand MarkAsPaidCommand { get; }
    public ICommand GenerateReceiptCommand { get; }
    public ICommand DeletePaymentCommand { get; }
    public ICommand RefreshCommand { get; }

    private async Task LoadPaymentsAsync()
    {
        await ExecuteSafelyAsync(async () =>
        {
            IsLoading = true;
            try
            {
                var payments = await _paymentService.GetPaymentsByMonthAsync(SelectedMonth);
                Payments.Clear();
                foreach (var payment in payments)
                {
                    Payments.Add(payment);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }, "Erreur lors du chargement des paiements", _logger);
    }

    private async Task LoadHousesAsync()
    {
        await ExecuteSafelyAsync(async () =>
        {
            var houses = await _houseRepository.GetAllActiveAsync();
            
            // Tri alphabétique puis numérique
            var sortedHouses = houses.OrderBy(h => 
            {
                // Extraire la partie alphabétique (premier caractère ou premiers caractères non-numériques)
                var code = h.HouseCode;
                var letterPart = new string(code.TakeWhile(c => !char.IsDigit(c)).ToArray());
                return letterPart;
            })
            .ThenBy(h => 
            {
                // Extraire la partie numérique
                var code = h.HouseCode;
                var numberPart = new string(code.SkipWhile(c => !char.IsDigit(c)).ToArray());
                return int.TryParse(numberPart, out var num) ? num : 0;
            });
            
            Houses.Clear();
            foreach (var house in sortedHouses)
            {
                Houses.Add(new HouseDto
                {
                    Code = house.HouseCode,
                    MonthlyAmount = house.MonthlyAmount
                });
            }
        }, "Erreur lors du chargement des maisons", _logger);
    }

    private void ShowCreateForm()
    {
        // Reset form
        SelectedHouseCode = string.Empty;
        PaymentAmount = 0;
        PaymentDate = DateTime.Now;
        IsFormVisible = true;
    }

    private void CancelForm()
    {
        IsFormVisible = false;
    }

    private async Task CreatePaymentAsync()
    {
        await ExecuteSafelyAsync(async () =>
        {
            try
            {
                // Validation côté client
                if (string.IsNullOrWhiteSpace(SelectedHouseCode))
                {
                    MessageBox.Show("Veuillez sélectionner une maison", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (PaymentAmount <= 0)
                {
                    MessageBox.Show("Le montant doit être supérieur à zéro", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (PaymentDate == default(DateTime))
                {
                    MessageBox.Show("Veuillez sélectionner une date de paiement", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var payment = new CreatePaymentDto
                {
                    HouseCode = SelectedHouseCode,
                    Amount = PaymentAmount,
                    PaymentDate = PaymentDate,
                    Month = PaymentDate.ToString("yyyy-MM", CultureInfo.InvariantCulture)
                };

                await _paymentService.CreatePaymentAsync(payment);
                await LoadPaymentsAsync();
                
                // Close form and show success message
                IsFormVisible = false;
                MessageBox.Show("Paiement enregistré avec succès", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (ValidationException ex)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Erreur de validation :");
                foreach (var error in ex.Errors)
                {
                    sb.AppendLine($"- {string.Join(", ", error.Value)}");
                }
                MessageBox.Show(sb.ToString(), "Erreur de Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }, "Erreur lors de la création du paiement", _logger);
    }

    private async Task MarkAsPaidAsync(PaymentDto payment)
    {
        if (!CanValidate)
        {
            MessageBox.Show("Vous n'avez pas les droits pour valider un paiement.", "Accès refusé", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await ExecuteSafelyAsync(async () =>
        {
            await _paymentService.MarkAsPaidAsync(payment.Id, DateTime.Now);
            await LoadPaymentsAsync();
        }, "Erreur lors de la validation du paiement", _logger);
    }

    private async Task GenerateReceiptAsync(PaymentDto payment)
    {
        if (!CanValidate)
        {
            MessageBox.Show("Vous n'avez pas les droits pour générer un reçu.", "Accès refusé", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await ExecuteSafelyAsync(async () =>
        {
            await _receiptService.GenerateReceiptAsync(payment.Id);
            MessageBox.Show("Reçu généré avec succès", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
        }, "Erreur lors de la génération du reçu", _logger);
    }

    private async Task DeletePaymentAsync(PaymentDto payment)
    {
        await ExecuteSafelyAsync(async () =>
        {
            var result = MessageBox.Show(
                $"Êtes-vous sûr de vouloir supprimer ce paiement ?\n\nMaison: {payment.HouseCode}\nMontant: {payment.Amount:N0} TND\nDate: {payment.PaymentDate:dd/MM/yyyy}\n\nNote: Le reçu associé sera également supprimé.",
                "Confirmation de suppression",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                await _paymentService.DeletePaymentAsync(payment.Id);
                Payments.Remove(payment);
                MessageBox.Show("Paiement supprimé avec succès", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }, "Erreur lors de la suppression du paiement", _logger);
    }
}

public class HouseDto
{
    public string Code { get; set; } = string.Empty;
    public decimal MonthlyAmount { get; set; }
}
