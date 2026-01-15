using System;
using System.Threading.Tasks;

namespace ElMansourSyndicManager.Core.Domain.Interfaces.Services;

public class UpdateInfo
{
    public string Version { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string ReleaseNotes { get; set; } = string.Empty;
    public bool IsCritical { get; set; }
}

public interface IUpdateService
{
    Task<UpdateInfo?> CheckForUpdatesAsync();
    string GetCurrentVersion();
}
