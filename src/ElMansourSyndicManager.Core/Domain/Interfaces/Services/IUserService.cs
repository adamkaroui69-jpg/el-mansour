using ElMansourSyndicManager.Core.Domain.DTOs;

namespace ElMansourSyndicManager.Core.Domain.Interfaces.Services;

public interface IUserService
{
    Task<UserDto> CreateUserAsync(CreateUserDto user, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto user, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserByHouseCodeAsync(string houseCode, CancellationToken cancellationToken = default);
    Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<bool> ResetPasswordAsync(Guid userId, string newCode, CancellationToken cancellationToken = default);
}
