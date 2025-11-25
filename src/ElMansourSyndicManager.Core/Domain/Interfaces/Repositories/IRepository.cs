using ElMansourSyndicManager.Core.Domain.Entities;
using System.Linq.Expressions;

namespace ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;

/// <summary>
/// Generic repository interface
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IUserRepository : IRepository<Entities.User>
{
    Task<Entities.User?> GetByHouseCodeAsync(string houseCode, CancellationToken cancellationToken = default);
    Task<List<Entities.User>> GetByRoleAsync(string role, CancellationToken cancellationToken = default);
    Task UpdatePasswordAsync(Guid userId, string hash, string salt, CancellationToken cancellationToken = default);
    Task UpdateLastLoginAsync(Guid userId, DateTime lastLogin, CancellationToken cancellationToken = default);
    Task<int> GetActiveSyndicMemberCountAsync(CancellationToken cancellationToken = default);
    Task CreateAdminUserIfNotExistAsync(string houseCode, string username, string passwordHash, string salt, CancellationToken cancellationToken = default);
    // Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default); // Supprimé pour éviter l'avertissement CS0108
}

public interface IPaymentRepository : IRepository<Entities.Payment>
{
    Task<Entities.Payment?> GetByHouseAndMonthAsync(string houseCode, string month, CancellationToken cancellationToken = default);
    Task<List<Entities.Payment>> GetByHouseCodeAsync(string houseCode, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<List<Entities.Payment>> GetByMonthAsync(string month, CancellationToken cancellationToken = default);
    Task<List<Entities.Payment>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<Entities.Payment?> GetByCodeAsync(string code, CancellationToken cancellationToken = default); // Ajouté pour résoudre les erreurs CS1501
    Task<List<Entities.Payment>> GetAllActiveAsync(CancellationToken cancellationToken = default); // Ajouté pour résoudre les erreurs CS1501
}

public interface IHouseRepository : IRepository<Entities.House>
{
    Task<Entities.House?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<List<Entities.House>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<List<Entities.House>> GetByBuildingAsync(string buildingCode, CancellationToken cancellationToken = default);
}



public interface IReceiptRepository : IRepository<Entities.Receipt>
{
    Task<Entities.Receipt?> GetByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
    // Task<Receipt?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default); // Supprimé pour éviter l'avertissement CS0108
}

public interface IAuditLogRepository : IRepository<Entities.AuditLog>
{
    Task<List<Entities.AuditLog>> GetByDateRangeAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<List<Entities.AuditLog>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<Entities.AuditLog>> GetByEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default);
}


public interface IBackupRepository : IRepository<Entities.Backup> // Spécifier Entities.Backup
{
    Task<List<Entities.Backup>> GetByTypeAsync(string backupType, CancellationToken cancellationToken = default);
    Task<List<Entities.Backup>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<List<Entities.Backup>> GetAutomaticBackupsAsync(CancellationToken cancellationToken = default);
}
