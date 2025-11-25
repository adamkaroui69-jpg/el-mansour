using ElMansourSyndicManager.ViewModels;
using System.Windows;
using Microsoft.Extensions.DependencyInjection; // Add this using directive

using AutoUpdaterDotNET;

namespace ElMansourSyndicManager;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        // Configuration du système de mise à jour automatique
        ConfigureAutoUpdater();
    }

    private void ConfigureAutoUpdater()
    {
        // Configuration pour rendre les mises à jour obligatoires
        AutoUpdater.Mandatory = true;
        AutoUpdater.UpdateMode = Mode.Forced;
        
        // Fermer l'application si l'utilisateur refuse la mise à jour
        AutoUpdater.ApplicationExitEvent += AutoUpdater_ApplicationExitEvent;
        
        // Personnalisation des messages (optionnel)
        AutoUpdater.DownloadPath = System.IO.Path.GetTempPath();
        
        // Démarrer la vérification de mise à jour
        // L'URL pointe vers le fichier update.xml dans la dernière release GitHub
        AutoUpdater.Start("https://github.com/adamkaroui69-jpg/el-mansour/releases/latest/download/update.xml");
    }

    private void AutoUpdater_ApplicationExitEvent()
    {
        // Fermer l'application si l'utilisateur refuse la mise à jour obligatoire
        Application.Current.Shutdown();
    }
}

// Extension method for getting services
public static class ServiceProviderExtensions
{
    public static T GetRequiredService<T>(this Application app) where T : class
    {
        return App.Services?.GetRequiredService<T>() ?? throw new InvalidOperationException("Service provider not initialized");
    }
}

