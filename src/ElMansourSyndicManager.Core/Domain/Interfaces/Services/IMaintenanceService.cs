using ElMansourSyndicManager.Core.Domain.DTOs;

namespace ElMansourSyndicManager.Core.Domain.Interfaces.Services
{
    public interface IMaintenanceService
    {
        Task<IEnumerable<MaintenanceDTO>> GetAllMaintenanceAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<MaintenanceDTO>> GetMaintenanceByStatusAsync(string status, CancellationToken cancellationToken = default);
        Task<MaintenanceDTO> CreateMaintenanceAsync(CreateMaintenanceDTO dto, CancellationToken cancellationToken = default);
        Task<MaintenanceDTO> UpdateMaintenanceAsync(Guid id, UpdateMaintenanceDTO dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteMaintenanceAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
