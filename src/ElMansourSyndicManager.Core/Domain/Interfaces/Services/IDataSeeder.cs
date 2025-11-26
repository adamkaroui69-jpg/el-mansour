namespace ElMansourSyndicManager.Core.Domain.Interfaces.Services;

/// <summary>
/// Service responsible for seeding initial data into the database
/// </summary>
public interface IDataSeeder
{
    /// <summary>
    /// Seeds the database with initial data
    /// </summary>
    Task SeedAsync(CancellationToken cancellationToken = default);
}
