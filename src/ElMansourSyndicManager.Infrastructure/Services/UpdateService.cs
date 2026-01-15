using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using System.Reflection;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System;

namespace ElMansourSyndicManager.Infrastructure.Services;

public class UpdateService : IUpdateService
{
    private const string UPDATE_XML_URL = "https://github.com/adamkaroui69-jpg/el-mansour/releases/latest/download/update.xml";

    public string GetCurrentVersion()
    {
        var version = Assembly.GetEntryAssembly()?.GetName().Version;
        return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "1.0.0";
    }

    public async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "ElMansourSyndicManager");

            var xmlContent = await httpClient.GetStringAsync(UPDATE_XML_URL);
            var doc = XDocument.Parse(xmlContent);

            var versionElement = doc.Root?.Element("version");
            var urlElement = doc.Root?.Element("url");
            var notesElement = doc.Root?.Element("notes");
            var criticalElement = doc.Root?.Element("critical");

            if (versionElement == null || urlElement == null)
                return null;

            var remoteVersionStr = versionElement.Value;
            var currentVersion = new Version(GetCurrentVersion());
            var remoteVersion = new Version(remoteVersionStr);

            if (remoteVersion > currentVersion)
            {
                return new UpdateInfo
                {
                    Version = remoteVersionStr,
                    DownloadUrl = urlElement.Value,
                    ReleaseNotes = notesElement?.Value ?? "Mise à jour standard",
                    IsCritical = criticalElement != null && bool.TryParse(criticalElement.Value, out var crit) && crit
                };
            }

            return null; // Up to date
        }
        catch
        {
            return null; // Error checking (no internet, or invalid xml)
        }
    }
}
