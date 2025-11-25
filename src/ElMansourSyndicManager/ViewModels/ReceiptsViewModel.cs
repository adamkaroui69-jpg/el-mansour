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
/// ViewModel for the receipts page
/// </summary>
public class ReceiptsViewModel : ViewModelBase, IInitializable
{
    private readonly IReceiptService _receiptService;
    private readonly IPaymentService _paymentService;
    private ReceiptDto? _selectedReceipt;
    private string _selectedHouseCode = string.Empty;
    private bool _isLoading;

    public ReceiptsViewModel(
        IReceiptService receiptService,
        IPaymentService paymentService)
    {
        _receiptService = receiptService;
        _paymentService = paymentService;

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
        try
        {
            IsLoading = true;
            
            List<ReceiptDto> receipts;
            if (string.IsNullOrWhiteSpace(SelectedHouseCode))
            {
                receipts = await _receiptService.GetAllReceiptsAsync();
            }
            else
            {
                receipts = await _receiptService.GetReceiptHistoryAsync(SelectedHouseCode);
            }
            Receipts.Clear();
            foreach (var receipt in receipts)
            {
                Receipts.Add(receipt);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur lors du chargement des reçus: {ex.Message}",
                "Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ViewReceiptAsync(ReceiptDto receipt)
    {
        try
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
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur lors de l'ouverture du reçu: {ex.Message}",
                "Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async Task PrintReceiptAsync(ReceiptDto receipt)
    {
        try
        {
            await _receiptService.PrintReceiptAsync(receipt.Id);
            MessageBox.Show(
                "Le reçu a été envoyé à l'imprimante.",
                "Succès",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur lors de l'impression: {ex.Message}",
                "Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async Task DownloadReceiptAsync(ReceiptDto receipt)
    {
        try
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
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur lors du téléchargement: {ex.Message}",
                "Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async Task EmailReceiptAsync(ReceiptDto receipt)
    {
        try
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
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur lors de l'envoi par email: {ex.Message}",
                "Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
