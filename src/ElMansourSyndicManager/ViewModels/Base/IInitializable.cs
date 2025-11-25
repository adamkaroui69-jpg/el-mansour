using System.Threading.Tasks;

namespace ElMansourSyndicManager.ViewModels.Base;

public interface IInitializable
{
    Task InitializeAsync();
}
