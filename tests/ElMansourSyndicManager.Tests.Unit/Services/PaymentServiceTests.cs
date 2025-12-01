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

/// <summary>
/// Tests unitaires pour PaymentService
/// </summary>
public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IHouseRepository> _houseRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IReceiptService> _receiptServiceMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<IAuthenticationService> _authServiceMock;
    private readonly Mock<ILogger<PaymentService>> _loggerMock;
    private readonly PaymentService _paymentService;

    public PaymentServiceTests()
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _houseRepositoryMock = new Mock<IHouseRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _receiptServiceMock = new Mock<IReceiptService>();
        _auditServiceMock = new Mock<IAuditService>();
        _authServiceMock = new Mock<IAuthenticationService>();
        _loggerMock = new Mock<ILogger<PaymentService>>();

        _paymentService = new PaymentService(
            _paymentRepositoryMock.Object,
            _houseRepositoryMock.Object,
            _userRepositoryMock.Object,
            _receiptServiceMock.Object,
            _auditServiceMock.Object,
            _authServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task CreatePaymentAsync_WithValidData_ShouldCreatePayment()
    {
        // Arrange
        var createPaymentDto = new CreatePaymentDto
        {
            HouseCode = "A01",
            Amount = 100.00m,
            Month = "2025-11",
            PaymentDate = DateTime.Now,
            ReferenceNumber = "REF001"
        };

        var currentUser = new UserDto
        {
            Id = Guid.NewGuid(),
            HouseCode = "B40",
            Role = "Admin"
        };

        var house = new House
        {
            Id = Guid.NewGuid(),
            HouseCode = "A01",
            IsActive = true,
            MonthlyAmount = 100.00m
        };

        _authServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authServiceMock.Setup(x => x.CurrentUser).Returns(currentUser);
        _houseRepositoryMock.Setup(x => x.GetByCodeAsync("A01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(house);
        _paymentRepositoryMock.Setup(x => x.GetByHouseAndMonthAsync("A01", "2025-11", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);
        _paymentRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment p, CancellationToken ct) => p);
        _auditServiceMock.Setup(x => x.LogActivityAsync(It.IsAny<AuditLogDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _paymentService.CreatePaymentAsync(createPaymentDto);

        // Assert
        result.Should().NotBeNull();
        result.HouseCode.Should().Be("A01");
        result.Amount.Should().Be(100.00m);
        result.Month.Should().Be("2025-11");
        result.Status.Should().Be("Pending");

        _paymentRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
        _auditServiceMock.Verify(x => x.LogActivityAsync(It.IsAny<AuditLogDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePaymentAsync_WhenNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var createPaymentDto = new CreatePaymentDto
        {
            HouseCode = "A01",
            Amount = 100.00m,
            Month = "2025-11",
            PaymentDate = DateTime.Now
        };

        _authServiceMock.Setup(x => x.IsAuthenticated).Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _paymentService.CreatePaymentAsync(createPaymentDto)
        );
    }

    [Fact]
    public async Task CreatePaymentAsync_WithInvalidHouseCode_ShouldThrowNotFoundException()
    {
        // Arrange
        var createPaymentDto = new CreatePaymentDto
        {
            HouseCode = "INVALID",
            Amount = 100.00m,
            Month = "2025-11",
            PaymentDate = DateTime.Now
        };

        var currentUser = new UserDto
        {
            Id = Guid.NewGuid(),
            HouseCode = "B40",
            Role = "Admin"
        };

        _authServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authServiceMock.Setup(x => x.CurrentUser).Returns(currentUser);
        _houseRepositoryMock.Setup(x => x.GetByCodeAsync("INVALID", It.IsAny<CancellationToken>()))
            .ReturnsAsync((House?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _paymentService.CreatePaymentAsync(createPaymentDto)
        );
    }

    [Fact]
    public async Task GetUnpaidHousesAsync_ShouldReturnUnpaidHouses()
    {
        // Arrange
        var month = "2025-11";
        var houses = new List<House>
        {
            new House { Id = Guid.NewGuid(), HouseCode = "A01", IsActive = true, MonthlyAmount = 100.00m, OwnerName = "Owner 1" },
            new House { Id = Guid.NewGuid(), HouseCode = "A02", IsActive = true, MonthlyAmount = 100.00m, OwnerName = "Owner 2" },
            new House { Id = Guid.NewGuid(), HouseCode = "A03", IsActive = true, MonthlyAmount = 100.00m, OwnerName = "Owner 3" }
        };

        var payments = new List<Payment>
        {
            new Payment { Id = Guid.NewGuid(), HouseCode = "A01", Month = month, Status = "Paid", Amount = 100.00m }
        };

        _houseRepositoryMock.Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(houses);
        _paymentRepositoryMock.Setup(x => x.GetByMonthAsync(month, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payments);

        // Act
        var result = await _paymentService.GetUnpaidHousesAsync(month);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(h => h.HouseCode == "A02");
        result.Should().Contain(h => h.HouseCode == "A03");
        result.Should().NotContain(h => h.HouseCode == "A01");
    }

    [Fact]
    public async Task DeletePaymentAsync_WithValidId_ShouldDeletePaymentAndReceipts()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            HouseCode = "A01",
            Amount = 100.00m,
            Month = "2025-11"
        };

        _paymentRepositoryMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);
        _paymentRepositoryMock.Setup(x => x.DeleteAsync(payment, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _receiptServiceMock.Setup(x => x.DeleteReceiptsByPaymentIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _paymentService.DeletePaymentAsync(paymentId);

        // Assert
        result.Should().BeTrue();
        _receiptServiceMock.Verify(x => x.DeleteReceiptsByPaymentIdAsync(paymentId, It.IsAny<CancellationToken>()), Times.Once);
        _paymentRepositoryMock.Verify(x => x.DeleteAsync(payment, It.IsAny<CancellationToken>()), Times.Once);
    }
}
