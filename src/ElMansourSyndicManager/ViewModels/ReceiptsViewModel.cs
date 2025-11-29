using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.ViewModels.Base;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Logging;

namespace ElMansourSyndicManager.ViewModels;

/// <summary>
/// ViewModel for the receipts page
/// </summary>
public class ReceiptsViewModel : ViewModelBase, IInitializable
{
    private readonly IReceiptService _receiptService;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<ReceiptsViewModel> _logger;
    private ReceiptDto? _selectedReceipt;
    private string _selectedHouseCode = string.Empty;
    private bool _isLoading;

    public ReceiptsViewModel(
        IReceiptService receiptService,
        IPaymentService paymentService,
        ILogger<ReceiptsViewModel> logger)
    {
        _receiptService = receiptService;
        _paymentService = paymentService;
        _logger = logger;

        Receipts = new ObservableCollection<ReceiptDto>();

        RefreshCommand = new RelayCommand(async () => await LoadReceiptsAsync());
        ViewReceiptCommand = new RelayCommand<ReceiptDto>(async r => await ViewReceiptAsync(r), r => r != null);
        PrintReceiptCommand = new RelayCommand<ReceiptDto>(async r => await PrintReceiptAsync(r), r => r != null);
        DownloadReceiptCommand = new RelayCommand<ReceiptDto>(async r => await DownloadReceiptAsync(r), r => r != null);
        EmailReceiptCommand = new RelayCommand<ReceiptDto>(async r => await EmailReceiptAsync(r), r => r != null);

    }

    public async Task InitializeAsync()
    {
        await LoadReceiptsAsync();
    }

    public ObservableCollection<ReceiptDto> Receipts { get; }

    public ReceiptDto? SelectedReceipt
    {
        get => _selectedReceipt;
        set => SetProperty(ref _selectedReceipt, value);
    }

    public string SelectedMonth
    {
        get => _selectedMonth;
        set
        {
            if (SetProperty(ref _selectedMonth, value))
            {
                // Update Date property without triggering loop
                if (DateTime.TryParseExact(value + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var date))
                {
                    if (_selectedMonthDate != date)
                    {
                        _selectedMonthDate = date;
                        OnPropertyChanged(nameof(SelectedMonthDate));
                    }
                }
                _ = LoadReceiptsAsync();
            }
        }
    }

    private string _selectedMonth = DateTime.Now.ToString("yyyy-MM");
    private DateTime _selectedMonthDate = DateTime.Now;
    public DateTime SelectedMonthDate
    {
        get => _selectedMonthDate;
        set
        {
            if (SetProperty(ref _selectedMonthDate, value))
            {
                // Update String property
                SelectedMonth = value.ToString("yyyy-MM");
            }
        }
    }

    public string SelectedHouseCode
    {
        get => _selectedHouseCode;
        set
        {
            if (SetProperty(ref _selectedHouseCode, value))
            {
                _ = LoadReceiptsAsync();
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand ViewReceiptCommand { get; }
    public ICommand PrintReceiptCommand { get; }
    public ICommand DownloadReceiptCommand { get; }
    public ICommand EmailReceiptCommand { get; }

    private async Task LoadReceiptsAsync()
    {
        await ExecuteSafelyAsync(async () =>
        {
            IsLoading = true;
            try
            {
                List<ReceiptDto> receipts;
                if (string.IsNullOrWhiteSpace(SelectedHouseCode))
                {
                    receipts = await _receiptService.GetAllReceiptsAsync();
                }
                else
                {
                    receipts = await _receiptService.GetReceiptHistoryAsync(SelectedHouseCode);
                }

                // Filter by payment month (not receipt generation date)
                receipts = receipts.Where(r => !string.IsNullOrEmpty(r.PaymentMonth) && r.PaymentMonth == SelectedMonth).ToList();

                Receipts.Clear();
                foreach (var receipt in receipts)
                {
                    Receipts.Add(receipt);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }, "Erreur lors du chargement des reçus", _logger);
    }

    private async Task ViewReceiptAsync(ReceiptDto receipt)
    {
        await ExecuteSafelyAsync(async () =>
        {
            var filePath = await _receiptService.GetReceiptFilePathAsync(receipt.Id);
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                // Try to download from cloud
                var pdfBytes = await _receiptService.GetReceiptFileAsync(receipt.Id);
                if (pdfBytes == null)
                {
                    MessageBox.Show(
                        "Le fichier du reçu n'a pas pu être trouvé.",
                        "Erreur",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Save to temp and open
                var tempFile = Path.Combine(Path.GetTempPath(), receipt.FileName);
                await File.WriteAllBytesAsync(tempFile, pdfBytes);
                filePath = tempFile;
            }

            // Open PDF with default viewer
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }, "Erreur lors de l'ouverture du reçu", _logger);
    }

    private async Task PrintReceiptAsync(ReceiptDto receipt)
    {
        await ExecuteSafelyAsync(async () =>
        {
            await _receiptService.PrintReceiptAsync(receipt.Id);
            MessageBox.Show(
                "Le reçu a été envoyé à l'imprimante.",
                "Succès",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }, "Erreur lors de l'impression", _logger);
    }

    private async Task DownloadReceiptAsync(ReceiptDto receipt)
    {
        await ExecuteSafelyAsync(async () =>
        {
            var saveDialog = new SaveFileDialog
            {
                FileName = receipt.FileName,
                Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*",
                DefaultExt = "pdf"
            };

            if (saveDialog.ShowDialog() == true)
            {
                var pdfBytes = await _receiptService.GetReceiptFileAsync(receipt.Id);
                if (pdfBytes == null)
                {
                    MessageBox.Show(
                        "Le fichier du reçu n'a pas pu être téléchargé.",
                        "Erreur",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                await File.WriteAllBytesAsync(saveDialog.FileName, pdfBytes);
                MessageBox.Show(
                    "Reçu téléchargé avec succès.",
                    "Succès",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }, "Erreur lors du téléchargement", _logger);
    }

    private async Task EmailReceiptAsync(ReceiptDto receipt)
    {
        await ExecuteSafelyAsync(async () =>
        {
            var pdfBytes = await _receiptService.GetReceiptFileAsync(receipt.Id);
            if (pdfBytes == null)
            {
                MessageBox.Show(
                    "Le fichier du reçu n'a pas pu être récupéré.",
                    "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Save to temp file
            var tempFile = Path.Combine(Path.GetTempPath(), receipt.FileName);
            await File.WriteAllBytesAsync(tempFile, pdfBytes);

            // Create mailto link with attachment (requires email client support)
            var mailtoLink = $"mailto:?subject=Reçu de Paiement - {receipt.FileName}&body=Veuillez trouver ci-joint le reçu de paiement.";
            
            Process.Start(new ProcessStartInfo
            {
                FileName = mailtoLink,
                UseShellExecute = true
            });

            // Note: Direct attachment via mailto is limited
            // For full email functionality, consider using a library like MailKit
            MessageBox.Show(
                "Votre client de messagerie va s'ouvrir. Veuillez joindre le fichier manuellement depuis: " + tempFile,
                "Information",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }, "Erreur lors de l'envoi par email", _logger);
    }
}
