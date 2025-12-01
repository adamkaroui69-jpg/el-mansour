using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Entities;
using ElMansourSyndicManager.Core.Domain.Exceptions;
using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Cryptography;

namespace ElMansourSyndicManager.Tests.Unit.Services;

public class AuthenticationServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<ILogger<AuthenticationService>> _loggerMock;
    private readonly AuthenticationService _authService;

    public AuthenticationServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _auditServiceMock = new Mock<IAuditService>();
        _loggerMock = new Mock<ILogger<AuthenticationService>>();
        
        _authService = new AuthenticationService(
            _userRepositoryMock.Object,
            _auditServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var houseCode = "B40";
        var code = "123456";
        
        // Generate valid hash/salt for "123456"
        var (hash, salt) = _authService.HashPassword(code);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            HouseCode = houseCode,
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = "Admin",
            IsActive = true,
            Username = "Admin User"
        };

        _userRepositoryMock.Setup(x => x.GetByHouseCodeAsync(houseCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
            
        _userRepositoryMock.Setup(x => x.UpdateLastLoginAsync(user.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.AuthenticateAsync(houseCode, code);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.User.Should().NotBeNull();
        result.User!.HouseCode.Should().Be(houseCode);
        
        _authService.IsAuthenticated.Should().BeTrue();
        _authService.CurrentUser.Should().NotBeNull();
        _authService.CurrentUser!.Id.Should().Be(user.Id);
        
        _auditServiceMock.Verify(x => x.LogActivityAsync(
            It.Is<AuditLogDto>(l => l.Action == "Login" && l.Details.Contains("success\":true")), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidHouseCode_ShouldReturnFailure()
    {
        // Arrange
        var houseCode = "INVALID";
        var code = "123456";

        _userRepositoryMock.Setup(x => x.GetByHouseCodeAsync(houseCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.AuthenticateAsync(houseCode, code);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("incorrect");
        _authService.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidCode_ShouldReturnFailure()
    {
        // Arrange
        var houseCode = "B40";
        var code = "000000"; // Wrong code
        var correctCode = "123456";
        
        var (hash, salt) = _authService.HashPassword(correctCode);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            HouseCode = houseCode,
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = "Admin",
            IsActive = true
        };

        _userRepositoryMock.Setup(x => x.GetByHouseCodeAsync(houseCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.AuthenticateAsync(houseCode, code);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("incorrect");
        _authService.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task AuthenticateAsync_WithInactiveUser_ShouldReturnFailure()
    {
        // Arrange
        var houseCode = "B40";
        var code = "123456";
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            HouseCode = houseCode,
            IsActive = false // Inactive
        };

        _userRepositoryMock.Setup(x => x.GetByHouseCodeAsync(houseCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.AuthenticateAsync(houseCode, code);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("désactivé");
        _authService.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task LogoutAsync_ShouldClearCurrentUser()
    {
        // Arrange
        // First login to set current user
        var houseCode = "B40";
        var code = "123456";
        var (hash, salt) = _authService.HashPassword(code);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            HouseCode = houseCode,
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = "Admin",
            IsActive = true
        };

        _userRepositoryMock.Setup(x => x.GetByHouseCodeAsync(houseCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _authService.AuthenticateAsync(houseCode, code);
        _authService.IsAuthenticated.Should().BeTrue();

        // Act
        await _authService.LogoutAsync();

        // Assert
        _authService.IsAuthenticated.Should().BeFalse();
        _authService.CurrentUser.Should().BeNull();
    }

    [Fact]
    public async Task ChangePasswordAsync_WithValidData_ShouldUpdatePassword()
    {
        // Arrange
        // First login
        var houseCode = "B40";
        var oldCode = "123456";
        var newCode = "654321";
        var (hash, salt) = _authService.HashPassword(oldCode);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            HouseCode = houseCode,
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = "Admin",
            IsActive = true
        };

        _userRepositoryMock.Setup(x => x.GetByHouseCodeAsync(houseCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.UpdatePasswordAsync(user.Id, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _authService.AuthenticateAsync(houseCode, oldCode);

        // Act
        var result = await _authService.ChangePasswordAsync(oldCode, newCode);

        // Assert
        result.Should().BeTrue();
        _userRepositoryMock.Verify(x => x.UpdatePasswordAsync(user.Id, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
