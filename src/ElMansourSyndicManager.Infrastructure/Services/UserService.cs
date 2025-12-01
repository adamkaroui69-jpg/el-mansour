using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Exceptions;
using ElMansourSyndicManager.Core.Domain.Entities; // Utiliser les entités User
using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Threading;

namespace ElMansourSyndicManager.Infrastructure.Services;

/// <summary>
/// Service for managing users
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IHouseRepository _houseRepository;
    private readonly IAuthenticationService _authService;
    private readonly IAuditService _auditService;
    // Removed IDocumentService as its interface was removed
    private readonly ILogger<UserService> _logger;
    private const int PBKDF2Iterations = 10000;
    private const int SaltSize = 32;
    private const int HashSize = 32;

    public UserService(
        IUserRepository userRepository,
        IHouseRepository houseRepository,
        IAuthenticationService authService,
        IAuditService auditService,
        // Removed IDocumentService as its interface was removed
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _houseRepository = houseRepository;
        _authService = authService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<UserDto> CreateUserAsync(
        CreateUserDto user,
        CancellationToken cancellationToken = default)
    {
        // Only admins can create users
        if (!_authService.IsAuthenticated || !_authService.IsAdmin)
            throw new UnauthorizedException("Only admins can create users");

        // Validate input
        ValidateCreateUserInput(user);

        // Check if house code exists
        var house = await _houseRepository.GetByCodeAsync(user.HouseCode, cancellationToken);
        if (house == null)
            throw new NotFoundException("House", user.HouseCode);

        if (!house.IsActive)
            throw new BusinessRuleException($"House {user.HouseCode} is not active");

        // Check for duplicate user (same house code)
        var existingUser = await _userRepository.GetByHouseCodeAsync(user.HouseCode, cancellationToken);

        if (existingUser != null)
            throw new BusinessRuleException($"User for house code {user.HouseCode} already exists");

        // Check role limit (max 4 Syndic Members)
        if (user.Role == "SyndicMember")
        {
        var syndicCount = await _userRepository.GetActiveSyndicMemberCountAsync(cancellationToken);
            if (syndicCount >= 4)
                throw new BusinessRuleException("Maximum of 4 Syndic Members allowed");
        }

        // Hash password
        var (hash, salt) = HashPassword(user.Code);

        // Create user entity
        var userEntity = new User
        {
            // Id est généré par BaseEntity
            HouseId = house.Id, // HouseId est un Guid
            Username = $"{user.Name} {user.Surname}", // Concatenate Name and Surname for Username
            HouseCode = user.HouseCode,
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = user.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(userEntity, cancellationToken);

        // Log activity
        await _auditService.LogActivityAsync(new AuditLogDto
        {
            UserId = _authService.CurrentUser!.Id.ToString(),
            Action = "Create",
            EntityType = "User",
            EntityId = userEntity.Id.ToString(), // Utilise l'ID de l'entité créée
            Details = $"{{\"name\":\"{user.Name}\",\"surname\":\"{user.Surname}\",\"houseCode\":\"{user.HouseCode}\",\"role\":\"{user.Role}\"}}"
        }, cancellationToken);

        _logger.LogInformation("User {UserId} created", userEntity.Id);

        return MapToDto(userEntity);
    }

    public async Task<UserDto> UpdateUserAsync(
        Guid id,
        UpdateUserDto user,
        CancellationToken cancellationToken = default)
    {
        // Only admins can update users
        if (!_authService.IsAuthenticated || !_authService.IsAdmin)
            throw new UnauthorizedException("Only admins can update users");

        var existingUser = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (existingUser == null)
            throw new NotFoundException("User", id.ToString());

        // Update fields
        existingUser.Username = $"{user.Name} {user.Surname}"; // Concatenate Name and Surname for Username
        existingUser.IsActive = user.IsActive;
        existingUser.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(existingUser, cancellationToken);

        await _auditService.LogActivityAsync(new AuditLogDto
        {
            UserId = _authService.CurrentUser?.Id.ToString(), // Gérer la nullabilité
            Action = "Update",
            EntityType = "User",
            EntityId = id.ToString(),
            Details = "{{\"field\":\"password\"}}"
        }, cancellationToken);

        return MapToDto(existingUser);
    }

    public async Task<bool> DeleteUserAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // Only admins can delete users
        if (!_authService.IsAuthenticated || !_authService.IsAdmin)
            throw new UnauthorizedException("Only admins can delete users");

        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
            throw new NotFoundException("User", id.ToString());

        // Cannot delete admin user
        if (user.Role == "Admin")
            throw new BusinessRuleException("Cannot delete admin user");

        // Soft delete
        // user.DeletedAt = DateTime.UtcNow; // La propriété DeletedAt n'existe pas dans le modèle User
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);

        await _auditService.LogActivityAsync(new AuditLogDto
        {
            UserId = _authService.CurrentUser?.Id.ToString(), // Gérer la nullabilité
            Action = "Delete",
            EntityType = "User",
            EntityId = id.ToString()
        }, cancellationToken);

        return true;
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<UserDto?> GetUserByHouseCodeAsync(string houseCode, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByHouseCodeAsync(houseCode, cancellationToken);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(MapToDto).ToList();
    }

    // Removed UpdateSignatureAsync as User entity does not have signature properties
    /*
    public async Task<bool> UpdateSignatureAsync(
        Guid userId, 
        string signaturePath, 
        CancellationToken cancellationToken = default)
    {
        if (!_authService.IsAuthenticated)
            throw new UnauthorizedException("User must be authenticated");

        // Users can only update their own signature, or admin can update any
        if (!_authService.IsAdmin && _authService.CurrentUser!.Id != userId)
            throw new UnauthorizedException("You can only update your own signature");

        var user = await _userRepository.GetByIdAsync(userId.ToString(), cancellationToken);
        if (user == null)
            throw new NotFoundException("User", userId);

        if (!File.Exists(signaturePath))
            throw new NotFoundException("Signature file", signaturePath);

        user.SignaturePath = signaturePath;
        user.SignatureCloudPath = await _documentService.UploadDocumentAsync(
            signaturePath, 
            "Signature", 
            null, 
            cancellationToken);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);

        return true;
    }
    */

    public async Task<bool> ResetPasswordAsync(
        Guid userId,
        string newCode,
        CancellationToken cancellationToken = default)
    {
        // Only admins can reset passwords
        if (!_authService.IsAuthenticated || !_authService.IsAdmin)
            throw new UnauthorizedException("Only admins can reset passwords");

        if (string.IsNullOrWhiteSpace(newCode) || newCode.Length != 6 || !newCode.All(char.IsDigit))
            throw new ValidationException("New code must be exactly 6 digits");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new NotFoundException("User", userId.ToString());

        var (hash, salt) = HashPassword(newCode);
        await _userRepository.UpdatePasswordAsync(userId, hash, salt, cancellationToken);

        await _auditService.LogActivityAsync(new AuditLogDto
        {
            UserId = _authService.CurrentUser?.Id.ToString(), // Correction: Gérer la nullabilité
            Action = "Update",
            EntityType = "User",
            EntityId = userId.ToString(),
            Details = "{{\"field\":\"password\"}}"
        }, cancellationToken); // Ajout de cancellationToken

        return true;
    }

    #region Private Methods

    private void ValidateCreateUserInput(CreateUserDto user)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(user.Name))
            errors["Name"] = new[] { "Name is required" };

        if (string.IsNullOrWhiteSpace(user.Surname))
            errors["Surname"] = new[] { "Surname is required" };

        if (string.IsNullOrWhiteSpace(user.HouseCode))
            errors["HouseCode"] = new[] { "House code is required" }; // Correction: Ajout de la validation manquante

        if (string.IsNullOrWhiteSpace(user.Code) || user.Code.Length != 6 || !user.Code.All(char.IsDigit))
            errors["Code"] = new[] { "Code must be exactly 6 digits" };

        if (user.Role != "Admin" && user.Role != "SyndicMember" && user.Role != "Trésorier")
            errors["Role"] = new[] { "Role must be Admin, SyndicMember or Trésorier" };

        if (errors.Any())
            throw new ValidationException("User validation failed", errors);
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

    private byte[] PBKDF2(string password, byte[] salt, int iterations, int outputBytes)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password, salt, iterations, System.Security.Cryptography.HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(outputBytes);
    }

    private UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            HouseId = user.HouseId,
            HouseCode = user.HouseCode,
            Username = user.Username,
            Role = user.Role,
            IsActive = user.IsActive,
            LastLoginAt = (DateTime?)user.LastLogin,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt ?? user.CreatedAt
        };
    }

    #endregion
}
