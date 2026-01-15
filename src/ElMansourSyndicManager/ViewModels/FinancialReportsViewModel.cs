using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ElMansourSyndicManager.ViewModels.Base;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.Core.Domain.DTOs.Financial;

namespace ElMansourSyndicManager.ViewModels
{
    public class FinancialReportsViewModel : ViewModelBase
    {
        private readonly IFinancialService _financialService;
        private readonly IExportService _exportService;
        private readonly IDialogService _dialogService;

        private ObservableCollection<ResidentFinancialStateDto> _residentsFinancialState;
        private ResidentFinancialStateDto? _selectedResident;
        private decimal _totalArrears;
        private int _residentsInArrears;
        private int _residentsUpToDate;
        private int _residentsInAdvance;
        private bool _isLoading;
        private string _filterText;

        public FinancialReportsViewModel(
            IFinancialService financialService,
            IExportService exportService,
            IDialogService dialogService)
        {
            _financialService = financialService;
            _exportService = exportService;
            _dialogService = dialogService;

            _residentsFinancialState = new ObservableCollection<ResidentFinancialStateDto>();
            _filterText = string.Empty;

            // Commands
            LoadDataCommand = new RelayCommand(async () => await LoadDataAsync());
            ExportExcelCommand = new RelayCommand(async () => await ExportToExcelAsync());
            ExportCsvCommand = new RelayCommand(async () => await ExportToCsvAsync());
            RefreshCommand = new RelayCommand(async () => await LoadDataAsync());

            // Load data on initialization
            _ = LoadDataAsync();
        }

        #region Properties

        public ObservableCollection<ResidentFinancialStateDto> ResidentsFinancialState
        {
            get => _residentsFinancialState;
            set => SetProperty(ref _residentsFinancialState, value);
        }

        public ResidentFinancialStateDto? SelectedResident
        {
            get => _selectedResident;
            set => SetProperty(ref _selectedResident, value);
        }

        public decimal TotalArrears
        {
            get => _totalArrears;
            set => SetProperty(ref _totalArrears, value);
        }

        public int ResidentsInArrears
        {
            get => _residentsInArrears;
            set => SetProperty(ref _residentsInArrears, value);
        }

        public int ResidentsUpToDate
        {
            get => _residentsUpToDate;
            set => SetProperty(ref _residentsUpToDate, value);
        }

        public int ResidentsInAdvance
        {
            get => _residentsInAdvance;
            set => SetProperty(ref _residentsInAdvance, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (SetProperty(ref _filterText, value))
                {
                    ApplyFilter();
                }
            }
        }

        #endregion

        #region Commands

        public ICommand LoadDataCommand { get; }
        public ICommand ExportExcelCommand { get; }
        public ICommand ExportCsvCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        #region Methods

        private async Task LoadDataAsync()
        {
            IsLoading = true;

            try
            {
                var allResidents = await _financialService.GetAllResidentsFinancialStateAsync();
                
                ResidentsFinancialState.Clear();
                foreach (var resident in allResidents)
                {
                    ResidentsFinancialState.Add(resident);
                }

                // Calculate statistics
                TotalArrears = await _financialService.GetTotalArrearsAsync();
                ResidentsInArrears = ResidentsFinancialState.Count(r => r.Balance < 0);
                ResidentsUpToDate = ResidentsFinancialState.Count(r => r.Balance == 0);
                ResidentsInAdvance = ResidentsFinancialState.Count(r => r.Balance > 0);

                _dialogService.ShowMessage($"Données chargées : {ResidentsFinancialState.Count} résidents");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync(
                    $"Erreur lors du chargement des données : {ex.Message}",
                    "Erreur"
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExportToExcelAsync()
        {
            try
            {
                var data = ResidentsFinancialState.ToList();
                var excelBytes = _exportService.ExportToExcel(data, "État Financier");

                // Save file dialog
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Fichiers Excel (*.xlsx)|*.xlsx",
                    FileName = $"Rapport_Financier_{DateTime.Now:yyyy-MM-dd}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    await System.IO.File.WriteAllBytesAsync(saveFileDialog.FileName, excelBytes);
                    _dialogService.ShowMessage(
                        "Export Excel réussi !",
                        "OUVRIR",
                        () => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveFileDialog.FileName,
                            UseShellExecute = true
                        })
                    );
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync(
                    $"Erreur lors de l'export Excel : {ex.Message}",
                    "Erreur"
                );
            }
        }

        private async Task ExportToCsvAsync()
        {
            try
            {
                var data = ResidentsFinancialState.ToList();
                var csvBytes = _exportService.ExportToCsv(data);

                // Save file dialog
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Fichiers CSV (*.csv)|*.csv",
                    FileName = $"Rapport_Financier_{DateTime.Now:yyyy-MM-dd}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    await System.IO.File.WriteAllBytesAsync(saveFileDialog.FileName, csvBytes);
                    _dialogService.ShowMessage(
                        "Export CSV réussi !",
                        "OUVRIR",
                        () => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveFileDialog.FileName,
                            UseShellExecute = true
                        })
                    );
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync(
                    $"Erreur lors de l'export CSV : {ex.Message}",
                    "Erreur"
                );
            }
        }

        private void ApplyFilter()
        {
            // TODO: Implement filtering logic
            // For now, reload data
            _ = LoadDataAsync();
        }

        #endregion
    }
}
