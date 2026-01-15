using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace ElMansourSyndicManager.Core.Configuration;

/// <summary>
/// Gestionnaire de configuration centralisé pour l'application
/// </summary>
public class AppConfiguration
{
    private readonly IConfiguration _configuration;
    private static AppConfiguration? _instance;
    private static readonly object _lock = new object();

    private AppConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        _configuration = builder.Build();
        EnsureDirectoriesExist();
    }

    public static AppConfiguration Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new AppConfiguration();
                    }
                }
            }
            return _instance;
        }
    }

    // Database Settings
    public string DatabaseProvider => _configuration["DatabaseProvider"] ?? "Sqlite";
    public string ConnectionString => _configuration.GetConnectionString("DefaultConnection") ?? "Data Source=data/local.db";

    // Application Directories (Absolute Paths)
    public string BaseDirectory => AppDomain.CurrentDomain.BaseDirectory;
    public string DataDirectory => GetAbsolutePath(_configuration["ApplicationSettings:DataDirectory"] ?? "data");
    public string DocumentsDirectory => GetAbsolutePath(_configuration["ApplicationSettings:DocumentsDirectory"] ?? "data/documents");
    public string BackupsDirectory => GetAbsolutePath(_configuration["ApplicationSettings:BackupsDirectory"] ?? "data/backups");
    public string LogsDirectory => GetAbsolutePath(_configuration["ApplicationSettings:LogsDirectory"] ?? "data/logs");

    // Backup Settings
    public int MaxBackupCount => int.TryParse(_configuration["ApplicationSettings:MaxBackupCount"], out var count) ? count : 10;
    public bool AutoBackupEnabled => bool.TryParse(_configuration["ApplicationSettings:AutoBackupEnabled"], out var enabled) && enabled;
    public int AutoBackupIntervalHours => int.TryParse(_configuration["ApplicationSettings:AutoBackupIntervalHours"], out var hours) ? hours : 24;

    // Company Info
    public string CompanyName => _configuration["CompanyInfo:Name"] ?? "El Mansour Syndic";
    public string CompanyAddress => _configuration["CompanyInfo:Address"] ?? "";
    public string CompanyPhone => _configuration["CompanyInfo:Phone"] ?? "";
    public string CompanyEmail => _configuration["CompanyInfo:Email"] ?? "";

    /// <summary>
    /// Convertit un chemin relatif en chemin absolu
    /// </summary>
    private string GetAbsolutePath(string relativePath)
    {
        if (Path.IsPathRooted(relativePath))
            return relativePath;

        return Path.Combine(BaseDirectory, relativePath);
    }

    /// <summary>
    /// Crée tous les répertoires nécessaires s'ils n'existent pas
    /// </summary>
    private void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(DataDirectory);
        Directory.CreateDirectory(DocumentsDirectory);
        Directory.CreateDirectory(BackupsDirectory);
        Directory.CreateDirectory(LogsDirectory);
    }

    /// <summary>
    /// Obtient le chemin complet de la base de données
    /// </summary>
    public string GetDatabasePath()
    {
        // Extract path from connection string
        var connStr = ConnectionString;
        var dataSourcePrefix = "Data Source=";
        var startIndex = connStr.IndexOf(dataSourcePrefix);
        
        if (startIndex >= 0)
        {
            var path = connStr.Substring(startIndex + dataSourcePrefix.Length).Trim();
            // Remove any trailing semicolons or parameters
            var semicolonIndex = path.IndexOf(';');
            if (semicolonIndex >= 0)
                path = path.Substring(0, semicolonIndex);

            return GetAbsolutePath(path);
        }

        return GetAbsolutePath("data/local.db");
    }

    /// <summary>
    /// Vérifie si c'est le premier lancement (base de données n'existe pas)
    /// </summary>
    public bool IsFirstRun()
    {
        return !File.Exists(GetDatabasePath());
    }
}
