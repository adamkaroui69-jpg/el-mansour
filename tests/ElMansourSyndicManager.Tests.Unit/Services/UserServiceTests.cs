using ElMansourSyndicManager.Core.Domain.DTOs;
using ElMansourSyndicManager.Core.Domain.Entities;
using ElMansourSyndicManager.Core.Domain.Exceptions;
using ElMansourSyndicManager.Core.Domain.Interfaces.Repositories;
using ElMansourSyndicManager.Core.Domain.Interfaces.Services;
using ElMansourSyndicManager.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ElMansourSyndicManager.Tests.Unit.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IHouseRepository> _houseRepositoryMock;
    private readonly Mock<IAuthenticationService> _authServiceMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _houseRepositoryMock = new Mock<IHouseRepository>();
        _authServiceMock = new Mock<IAuthenticationService>();
        _auditServiceMock = new Mock<IAuditService>();
        _loggerMock = new Mock<ILogger<UserService>>();

        _userService = new UserService(
            _userRepositoryMock.Object,
            _houseRepositoryMock.Object,
            _authServiceMock.Object,
            _auditServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task CreateUserAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            Name = "John",
            Surname = "Doe",
            HouseCode = "A01",
            Code = "123456",
            Role = "SyndicMember"
        };

        var currentUser = new UserDto { Id = Guid.NewGuid(), Role = "Admin" };
        var house = new House { Id = Guid.NewGuid(), HouseCode = "A01", IsActive = true };

        _authServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authServiceMock.Setup(x => x.IsAdmin).Returns(true);
        _authServiceMock.Setup(x => x.CurrentUser).Returns(currentUser);

        _houseRepositoryMock.Setup(x => x.GetByCodeAsync("A01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(house);
            
        _userRepositoryMock.Setup(x => x.GetByHouseCodeAsync("A01", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
            
        _userRepositoryMock.Setup(x => x.GetActiveSyndicMemberCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken ct) => u);

        // Act
        var result = await _userService.CreateUserAsync(createUserDto);

        // Assert
        result.Should().NotBeNull();
        result.HouseCode.Should().Be("A01");
        result.Username.Should().Be("John Doe");
        result.Role.Should().Be("SyndicMember");
        result.IsActive.Should().BeTrue();

        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WhenNotAdmin_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var createUserDto = new CreateUserDto();
        
        _authServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authServiceMock.Setup(x => x.IsAdmin).Returns(false); // Not Admin

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _userService.CreateUserAsync(createUserDto)
        );
    }

    [Fact]
    public async Task CreateUserAsync_WithDuplicateHouseCode_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            Name = "John",
            Surname = "Doe",
            HouseCode = "A01",
            Code = "123456",
            Role = "SyndicMember"
        };

        var currentUser = new UserDto { Id = Guid.NewGuid(), Role = "Admin" };
        var house = new House { Id = Guid.NewGuid(), HouseCode = "A01", IsActive = true };
        var existingUser = new User { Id = Guid.NewGuid(), HouseCode = "A01" };

        _authServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authServiceMock.Setup(x => x.IsAdmin).Returns(true);
        _authServiceMock.Setup(x => x.CurrentUser).Returns(currentUser);

        _houseRepositoryMock.Setup(x => x.GetByCodeAsync("A01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(house);
            
        _userRepositoryMock.Setup(x => x.GetByHouseCodeAsync("A01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser); // User already exists

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(
            () => _userService.CreateUserAsync(createUserDto)
        );
    }

    [Fact]
    public async Task UpdateUserAsync_WithValidData_ShouldUpdateUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateUserDto = new UpdateUserDto
        {
            Name = "Jane",
            Surname = "Doe",
            IsActive = false
        };

        var currentUser = new UserDto { Id = Guid.NewGuid(), Role = "Admin" };
        var existingUser = new User 
        { 
            Id = userId, 
            Username = "John Doe",
            IsActive = true 
        };

        _authServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authServiceMock.Setup(x => x.IsAdmin).Returns(true);
        _authServiceMock.Setup(x => x.CurrentUser).Returns(currentUser);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);
            
        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userService.UpdateUserAsync(userId, updateUserDto);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("Jane Doe");
        result.IsActive.Should().BeFalse();

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<User>(u => 
            u.Username == "Jane Doe" && u.IsActive == false), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WithValidId_ShouldSoftDeleteUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentUser = new UserDto { Id = Guid.NewGuid(), Role = "Admin" };
        var userToDelete = new User 
        { 
            Id = userId, 
            Role = "SyndicMember",
            IsActive = true 
        };

        _authServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authServiceMock.Setup(x => x.IsAdmin).Returns(true);
        _authServiceMock.Setup(x => x.CurrentUser).Returns(currentUser);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userToDelete);
            
        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        result.Should().BeTrue();
        userToDelete.IsActive.Should().BeFalse(); // Check soft delete

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<User>(u => u.IsActive == false), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenTryingToDeleteAdmin_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentUser = new UserDto { Id = Guid.NewGuid(), Role = "Admin" };
        var adminUser = new User 
        { 
            Id = userId, 
            Role = "Admin", // Trying to delete an Admin
            IsActive = true 
        };

        _authServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authServiceMock.Setup(x => x.IsAdmin).Returns(true);
        _authServiceMock.Setup(x => x.CurrentUser).Returns(currentUser);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(
            () => _userService.DeleteUserAsync(userId)
        );
    }
}
