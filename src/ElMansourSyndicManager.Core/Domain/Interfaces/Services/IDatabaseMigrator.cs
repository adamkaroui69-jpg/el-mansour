namespace ElMansourSyndicManager.Core.Domain.Interfaces.Services;

/// <summary>
/// Service responsible for applying database migrations
/// </summary>
public interface IDatabaseMigrator
{
    /// <summary>
    /// Applies pending migrations to the database
    /// </summary>
    Task MigrateAsync(CancellationToken cancellationToken = default);
}
