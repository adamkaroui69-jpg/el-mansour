using System.Collections.ObjectModel;
using System.Windows.Input;
using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.ViewModels.Base;
using Microsoft.Win32;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace ElMansourSyndicManager.ViewModels;

public class DocumentsViewModel : ViewModelBase
{
    private readonly IDocumentService _documentService;
    private readonly IAuthenticationService _authService;
    private ObservableCollection<DocumentDto> _documents;
    private DocumentDto? _selectedDocument;
    private bool _isLoading;
    private string _errorMessage = string.Empty;
    private string _searchQuery = string.Empty;
    private string _selectedCategoryFilter;

    // Upload Form
    private bool _isUploadFormVisible;
    private string _uploadFilePath = string.Empty;
    private string _uploadCategory = "General";
    private string _uploadDescription = string.Empty;

    public DocumentsViewModel(IDocumentService documentService)
    {
        _documentService = documentService;
        _authService = App.Services?.GetRequiredService<IAuthenticationService>()!;
        _documents = new ObservableCollection<DocumentDto>();
        _selectedCategoryFilter = "All";

        LoadCommand = new RelayCommand(async () => await LoadDocumentsAsync());
        UploadCommand = new RelayCommand(ShowUploadForm);
        DeleteCommand = new RelayCommand<DocumentDto>(async (doc) => await DeleteDocumentAsync(doc));
        OpenCommand = new RelayCommand<DocumentDto>(async (doc) => await OpenDocumentAsync(doc));
        BrowseFileCommand = new RelayCommand(BrowseFile);
        SaveUploadCommand = new RelayCommand(async () => await UploadDocumentAsync());
        CancelUploadCommand = new RelayCommand(HideUploadForm);
        SearchCommand = new RelayCommand(async () => await SearchDocumentsAsync());
    }

    public ObservableCollection<DocumentDto> Documents
    {
        get => _documents;
        set => SetProperty(ref _documents, value);
    }

    public DocumentDto? SelectedDocument
    {
        get => _selectedDocument;
        set => SetProperty(ref _selectedDocument, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }

    public string SelectedCategoryFilter
    {
        get => _selectedCategoryFilter;
        set 
        {
            if (SetProperty(ref _selectedCategoryFilter, value))
            {
                _ = LoadDocumentsAsync(); // Auto reload on filter change
            }
        }
    }

    // Upload Form Properties
    public bool IsUploadFormVisible
    {
        get => _isUploadFormVisible;
        set => SetProperty(ref _isUploadFormVisible, value);
    }

    public string UploadFilePath
    {
        get => _uploadFilePath;
        set => SetProperty(ref _uploadFilePath, value);
    }

    public string UploadCategory
    {
        get => _uploadCategory;
        set => SetProperty(ref _uploadCategory, value);
    }

    public string UploadDescription
    {
        get => _uploadDescription;
        set => SetProperty(ref _uploadDescription, value);
    }

    public ObservableCollection<string> Categories { get; } = new ObservableCollection<string> { "Général", "Juridique", "Financier", "Maintenance", "Autre" };
    public ObservableCollection<string> FilterCategories { get; } = new ObservableCollection<string> { "Tout", "Général", "Juridique", "Financier", "Maintenance", "Autre" };

    public bool IsAdmin => _authService.CurrentUser?.Role == "Admin";

    public ICommand LoadCommand { get; }
    public ICommand UploadCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand OpenCommand { get; }
    public ICommand BrowseFileCommand { get; }
    public ICommand SaveUploadCommand { get; }
    public ICommand CancelUploadCommand { get; }
    public ICommand SearchCommand { get; }

    public async Task LoadDocumentsAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            IEnumerable<DocumentDto> docs;
            if (SelectedCategoryFilter == "Tout" || SelectedCategoryFilter == "All") // Handle legacy "All"
            {
                docs = await _documentService.GetAllDocumentsAsync();
            }
            else
            {
                docs = await _documentService.GetDocumentsByCategoryAsync(SelectedCategoryFilter);
            }
            Documents = new ObservableCollection<DocumentDto>(docs.OrderByDescending(d => d.CreatedAt));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur lors du chargement des documents: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SearchDocumentsAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            await LoadDocumentsAsync();
            return;
        }

        IsLoading = true;
        try
        {
            var docs = await _documentService.SearchDocumentsAsync(SearchQuery);
            Documents = new ObservableCollection<DocumentDto>(docs);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur lors de la recherche: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ShowUploadForm()
    {
        UploadFilePath = string.Empty;
        UploadCategory = "Général";
        UploadDescription = string.Empty;
        IsUploadFormVisible = true;
    }

    private void HideUploadForm()
    {
        IsUploadFormVisible = false;
        ErrorMessage = string.Empty;
    }

    private void BrowseFile()
    {
        var openFileDialog = new OpenFileDialog();
        if (openFileDialog.ShowDialog() == true)
        {
            UploadFilePath = openFileDialog.FileName;
        }
    }

    private async Task UploadDocumentAsync()
    {
        if (string.IsNullOrWhiteSpace(UploadFilePath))
        {
            ErrorMessage = "Veuillez sélectionner un fichier.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            await _documentService.UploadDocumentAsync(UploadFilePath, UploadCategory, UploadDescription);
            HideUploadForm();
            await LoadDocumentsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur lors de l'envoi du document: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task DeleteDocumentAsync(DocumentDto? document)
    {
        var docToDelete = document ?? SelectedDocument;
        if (docToDelete == null) return;
        
        IsLoading = true;
        try
        {
            await _documentService.DeleteDocumentAsync(docToDelete.Id);
            Documents.Remove(docToDelete);
            if (SelectedDocument == docToDelete) SelectedDocument = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur lors de la suppression du document: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task OpenDocumentAsync(DocumentDto? document)
    {
        var docToOpen = document ?? SelectedDocument;
        if (docToOpen == null) return;

        try
        {
            var path = await _documentService.GetDocumentPathAsync(docToOpen.Id);
            new Process
            {
                StartInfo = new ProcessStartInfo(path)
                {
                    UseShellExecute = true
                }
            }.Start();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur lors de l'ouverture du document: {ex.Message}";
        }
    }
}
