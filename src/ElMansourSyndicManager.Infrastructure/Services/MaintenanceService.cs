using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.Core.Domain.Entities; // Utiliser les entités Maintenance
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElMansourSyndicManager.Infrastructure.Services;

public class MaintenanceService : IMaintenanceService
{
    private readonly IMaintenanceRepository _maintenanceRepository;
    private readonly ILogger<MaintenanceService> _logger;

    public MaintenanceService(IMaintenanceRepository maintenanceRepository, ILogger<MaintenanceService> logger)
    {
        _maintenanceRepository = maintenanceRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<MaintenanceDTO>> GetAllMaintenanceAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all maintenance records.");
        var maintenanceEntities = await _maintenanceRepository.GetAllAsync(cancellationToken);
        return maintenanceEntities.Select(MapToDto);
    }

    public async Task<IEnumerable<MaintenanceDTO>> GetMaintenanceByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching maintenance by status: {Status}", status);
        
        var maintenanceEntities = await _maintenanceRepository.GetByStatusAsync(status, cancellationToken);
        return maintenanceEntities.Select(MapToDto);
    }

    public async Task<MaintenanceDTO> CreateMaintenanceAsync(CreateMaintenanceDTO dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new maintenance record.");

        var maintenance = new Maintenance
        {
            Id = Guid.NewGuid(),
            Description = dto.Description,
            Type = dto.Type,
            Cost = dto.Cost,
            Status = "Pending", // Default status
            Priority = dto.Priority,
            CreatedBy = "System", // Or get from current user context
            AssignedTo = dto.AssignedTo,
            ScheduledDate = dto.ScheduledDate,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _maintenanceRepository.CreateAsync(maintenance, cancellationToken);
        return MapToDto(maintenance);
    }

    public async Task<MaintenanceDTO> UpdateMaintenanceAsync(Guid id, UpdateMaintenanceDTO dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating maintenance record with ID: {MaintenanceId}", id);

        var maintenance = await _maintenanceRepository.GetByIdAsync(id, cancellationToken);
        if (maintenance == null)
        {
            _logger.LogWarning("Maintenance record with ID {MaintenanceId} not found.", id);
            throw new KeyNotFoundException($"Maintenance with ID {id} not found.");
        }

        maintenance.Description = dto.Description;
        maintenance.Type = dto.Type;
        maintenance.Cost = dto.Cost;
        maintenance.Status = dto.Status;
        maintenance.Priority = dto.Priority;
        maintenance.AssignedTo = dto.AssignedTo;
        maintenance.ScheduledDate = dto.ScheduledDate;
        maintenance.StartedAt = dto.StartedAt;
        maintenance.CompletedAt = dto.CompletedAt;
        maintenance.ReportedBy = dto.ReportedBy;
        maintenance.Notes = dto.Notes;
        maintenance.UpdatedAt = DateTime.UtcNow;

        await _maintenanceRepository.UpdateAsync(maintenance, cancellationToken);
        return MapToDto(maintenance);
    }

    public async Task<bool> DeleteMaintenanceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting maintenance record with ID: {MaintenanceId}", id);

        var maintenance = await _maintenanceRepository.GetByIdAsync(id, cancellationToken);
        if (maintenance == null)
        {
            _logger.LogWarning("Maintenance record with ID {MaintenanceId} not found.", id);
            throw new KeyNotFoundException($"Maintenance with ID {id} not found.");
        }

        await _maintenanceRepository.DeleteAsync(maintenance, cancellationToken);
        return true;
    }

    private MaintenanceDTO MapToDto(Maintenance maintenance)
    {
        return new MaintenanceDTO
        {
            Id = maintenance.Id,
            Description = maintenance.Description,
            Type = maintenance.Type,
            Cost = maintenance.Cost,
            Status = maintenance.Status,
            Priority = maintenance.Priority,
            CreatedBy = maintenance.CreatedBy,
            AssignedTo = maintenance.AssignedTo,
            ScheduledDate = (DateTime?)maintenance.ScheduledDate,
            StartedAt = maintenance.StartedAt,
            CompletedAt = maintenance.CompletedAt,
            ReportedBy = maintenance.ReportedBy,
            Notes = maintenance.Notes,
            CreatedAt = maintenance.CreatedAt,
            UpdatedAt = maintenance.UpdatedAt ?? maintenance.CreatedAt
            // Documents n'est pas mappé ici, car il s'agit d'une liste d'objets complexes
        };
    }
}
