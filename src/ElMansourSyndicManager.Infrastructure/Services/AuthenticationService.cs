using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Exceptions;
using ElMansourSyndicManager.Core.Domain.Entities; // Utiliser les entités User
using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace ElMansourSyndicManager.Infrastructure.Services;

/// <summary>
/// Service for user authentication and authorization
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<AuthenticationService> _logger;
    private UserDto? _currentUser;
    private const int PBKDF2Iterations = 10000;
    private const int SaltSize = 32;
    private const int HashSize = 32;

    public AuthenticationService(
        IUserRepository userRepository,
        IAuditService auditService,
        ILogger<AuthenticationService> logger)
    {
        _userRepository = userRepository;
        _auditService = auditService;
        _logger = logger;
    }

    public bool IsAuthenticated => _currentUser != null;
    public bool IsAdmin => _currentUser?.Role == "Admin";
    public UserDto? CurrentUser => _currentUser;

    public async Task<AuthenticationResultDto> AuthenticateAsync(
        string houseCode, 
        string code, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(houseCode))
                throw new ValidationException("House code is required");

            if (string.IsNullOrWhiteSpace(code) || code.Length != 6 || !code.All(char.IsDigit))
                throw new ValidationException("Code must be exactly 6 digits");

            // Find user by house code
            var user = await _userRepository.GetByHouseCodeAsync(houseCode); // Supprimé cancellationToken
            if (user == null)
            {
                _logger.LogWarning("Authentication failed: User not found for house code {HouseCode}", houseCode);
                await _auditService.LogActivityAsync(new AuditLogDto
                {
                    Action = "Login",
                    EntityType = "User",
                    Details = $"{{\"houseCode\":\"{houseCode}\",\"success\":false,\"reason\":\"User not found\"}}"
                }, cancellationToken);
                
                return new AuthenticationResultDto
                {
                    Success = false,
                    ErrorMessage = "Code maison ou code d'authentification incorrect"
                };
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Authentication failed: User {UserId} is inactive", user.Id);
                return new AuthenticationResultDto
                {
                    Success = false,
                    ErrorMessage = "Ce compte utilisateur est désactivé"
                };
            }

            // Verify password
            var isValid = VerifyPassword(code, user.PasswordHash, user.PasswordSalt); // Utiliser user.PasswordSalt
            if (!isValid)
            {
                _logger.LogWarning("Authentication failed: Invalid code for user {UserId}", user.Id);
                await _auditService.LogActivityAsync(new AuditLogDto
                {
                    UserId = user.Id.ToString(),
                    Action = "Login",
                    EntityType = "User",
                    Details = $"{{\"houseCode\":\"{houseCode}\",\"success\":false,\"reason\":\"Invalid code\"}}"
                }, cancellationToken);
                
                return new AuthenticationResultDto
                {
                    Success = false,
                    ErrorMessage = "Code maison ou code d'authentification incorrect"
                };
            }

            // Update last login
            await _userRepository.UpdateLastLoginAsync(user.Id, DateTime.UtcNow, cancellationToken);

            // Set current user
            _currentUser = MapToDto(user);

            // Log successful authentication
            await _auditService.LogActivityAsync(new AuditLogDto
            {
                UserId = user.Id.ToString(),
                Action = "Login",
                EntityType = "User",
                Details = $"{{\"houseCode\":\"{houseCode}\",\"success\":true}}"
            }, cancellationToken);

            _logger.LogInformation("User {UserId} authenticated successfully", user.Id);

            return new AuthenticationResultDto
            {
                Success = true,
                User = MapToDto(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication");
            throw;
        }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser != null)
        {
            await _auditService.LogActivityAsync(new AuditLogDto
            {
                UserId = _currentUser.Id.ToString(),
                Action = "Logout",
                EntityType = "User"
            }, cancellationToken);
            
            _logger.LogInformation("User {UserId} logged out", _currentUser.Id);
            _currentUser = null;
        }
    }

    public async Task<UserDto?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_currentUser);
    }

    public async Task<bool> ChangePasswordAsync(
        string currentCode, 
        string newCode, 
        CancellationToken cancellationToken = default)
    {
        if (_currentUser == null)
            throw new UnauthorizedException("No user is currently authenticated");

        // Validate new code
        if (string.IsNullOrWhiteSpace(newCode) || newCode.Length != 6 || !newCode.All(char.IsDigit))
            throw new ValidationException("New code must be exactly 6 digits");

        // Verify current password
        var user = await _userRepository.GetByIdAsync(_currentUser.Id, cancellationToken);
        if (user == null)
            throw new NotFoundException("User", _currentUser.Id.ToString()); // Converti Guid en string pour l'exception

        var isValid = VerifyPassword(currentCode, user.PasswordHash, user.PasswordSalt); // Utiliser user.PasswordSalt
        if (!isValid)
            throw new ValidationException("Current code is incorrect");

        // Hash new password
        var (hash, salt) = HashPassword(newCode);

        // Update password
        await _userRepository.UpdatePasswordAsync(_currentUser.Id, hash, salt, cancellationToken);

        // Log activity
        await _auditService.LogActivityAsync(new AuditLogDto
        {
            UserId = _currentUser?.Id.ToString(),
            Action = "Update",
            EntityType = "User",
            EntityId = _currentUser?.Id.ToString(),
            Details = "{\"field\":\"password\"}"
        }, cancellationToken);

        _logger.LogInformation("User {UserId} changed password", _currentUser!.Id);

        return true;
    }

    public async Task<bool> ValidateSessionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser == null)
            return false;

        // Check if user still exists and is active
        var user = await _userRepository.GetByIdAsync(_currentUser.Id, cancellationToken);
        if (user == null || !user.IsActive)
        {
            _currentUser = null;
            return false;
        }

        return true;
    }

    public bool IsInRole(string role)
    {
        return _currentUser?.Role == role;
    }

    #region Private Methods

    private UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            HouseId = user.HouseId,
            Username = user.Username,
            Role = user.Role,
            IsActive = user.IsActive,
            LastLoginAt = (DateTime?)user.LastLogin,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt ?? user.CreatedAt
        };
    }

    private (string Hash, string Salt) HashPassword(string password)
    {
        var salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        var hash = PBKDF2(password, salt, PBKDF2Iterations, HashSize);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    private bool VerifyPassword(string password, string hash, string salt)
    {
        try
        {
            var saltBytes = Convert.FromBase64String(salt);
            var hashBytes = Convert.FromBase64String(hash);
            var computedHash = PBKDF2(password, saltBytes, PBKDF2Iterations, HashSize);
            return SlowEquals(hashBytes, computedHash);
        }
        catch
        {
            return false;
        }
    }

    private byte[] PBKDF2(string password, byte[] salt, int iterations, int outputBytes)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(outputBytes);
    }

    private bool SlowEquals(byte[] a, byte[] b)
    {
        if (a.Length != b.Length) return false;
        var diff = 0;
        for (var i = 0; i < a.Length; i++)
            diff |= a[i] ^ b[i];
        return diff == 0;
    }

    #endregion
}
