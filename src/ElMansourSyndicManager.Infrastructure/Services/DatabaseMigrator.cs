using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ElMansourSyndicManager.Infrastructure.Services;

public class DatabaseMigrator : IDatabaseMigrator
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseMigrator> _logger;

    public DatabaseMigrator(ApplicationDbContext context, ILogger<DatabaseMigrator> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting database migration...");
            await _context.Database.MigrateAsync(cancellationToken);
            _logger.LogInformation("Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            // Ignorer l'erreur si la table existe déjà (transition depuis EnsureCreated vers Migrations)
            _logger.LogWarning(ex, "Migration warning (safe to ignore on existing DB): {Message}", ex.Message);
        }
    }
}
