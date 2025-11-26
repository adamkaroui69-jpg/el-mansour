using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ElMansourSyndicManager.Infrastructure.Services;

public class AppInitializer : IAppInitializer
{
    private readonly ILogger<AppInitializer> _logger;

    public AppInitializer(ILogger<AppInitializer> logger)
    {
        _logger = logger;
    }

    public void Initialize()
    {
        try
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dataDir = Path.Combine(baseDir, "data");
            
            if (!Directory.Exists(dataDir))
            {
                _logger.LogInformation("Creating data directory at {Path}", dataDir);
                Directory.CreateDirectory(dataDir);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing application resources");
            throw;
        }
    }
}
