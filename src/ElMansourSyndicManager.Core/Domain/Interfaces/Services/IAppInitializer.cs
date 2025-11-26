namespace ElMansourSyndicManager.Core.Domain.Interfaces.Services;

/// <summary>
/// Service responsible for application initialization tasks
/// </summary>
public interface IAppInitializer
{
    /// <summary>
    /// Initializes application resources (directories, logs, etc.)
    /// </summary>
    void Initialize();
}
