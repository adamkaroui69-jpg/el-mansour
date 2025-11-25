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
        // TODO: Remplacez l'URL ci-dessous par l'URL réelle de votre fichier update.xml
        // Exemple: https://mon-site.com/updates/update.xml
        // Ou un chemin réseau: \\SERVER\Updates\update.xml
        AutoUpdater.Start("https://raw.githubusercontent.com/adamkaroui69-jpg/el-mansour/main/update.xml");
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

