using ElMansourSyndicManager.Core.Domain.DTOs;

namespace ElMansourSyndicManager.Core.Domain.Interfaces.Services;

/// <summary>
/// Service for user authentication and authorization
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user with house code and 6-digit code
    /// </summary>
    Task<AuthenticationResultDto> AuthenticateAsync(string houseCode, string code, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Logs out the current user
    /// </summary>
    Task LogoutAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the currently authenticated user
    /// </summary>
    Task<UserDto?> GetCurrentUserAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Changes the authentication code for the current user
    /// </summary>
    Task<bool> ChangePasswordAsync(string currentCode, string newCode, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates the current session
    /// </summary>
    Task<bool> ValidateSessionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if the current user has a specific role
    /// </summary>
    bool IsInRole(string role);
    
    /// <summary>
    /// Checks if the current user is an admin
    /// </summary>
    bool IsAdmin { get; }
    
    /// <summary>
    /// Indicates if a user is currently authenticated
    /// </summary>
    bool IsAuthenticated { get; }
    
    /// <summary>
    /// Current authenticated user
    /// </summary>
    UserDto? CurrentUser { get; }

    /// <summary>
    /// Hashes a password for storage
    /// </summary>
    (string Hash, string Salt) HashPassword(string password);
}

