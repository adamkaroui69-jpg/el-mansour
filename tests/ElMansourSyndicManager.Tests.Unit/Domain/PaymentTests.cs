using ElMansourSyndicManager.Core.Domain.Entities;
using FluentAssertions;

namespace ElMansourSyndicManager.Tests.Unit.Domain;

/// <summary>
/// Tests unitaires pour l'entité Payment
/// </summary>
public class PaymentTests
{
    [Fact]
    public void Payment_ShouldInitializeWithDefaultValues()
    {
        // Act
        var payment = new Payment();

        // Assert
        payment.HouseCode.Should().Be(string.Empty);
        payment.Month.Should().Be(string.Empty);
        payment.Amount.Should().Be(0);
        payment.PaymentDate.Should().BeNull();
        payment.Status.Should().Be("Pending");
        payment.GeneratedBy.Should().Be(string.Empty);
        payment.RecordedBy.Should().Be(string.Empty);
    }

    [Fact]
    public void Payment_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var paymentDate = DateTime.Now;

        // Act
        var payment = new Payment
        {
            Id = paymentId,
            HouseCode = "A01",
            Month = "2025-11",
            Amount = 100.50m,
            PaymentDate = paymentDate,
            Status = "Paid",
            ReferenceNumber = "REF001",
            GeneratedBy = "user1",
            RecordedBy = "admin1"
        };

        // Assert
        payment.Id.Should().Be(paymentId);
        payment.HouseCode.Should().Be("A01");
        payment.Month.Should().Be("2025-11");
        payment.Amount.Should().Be(100.50m);
        payment.PaymentDate.Should().Be(paymentDate);
        payment.Status.Should().Be("Paid");
        payment.ReferenceNumber.Should().Be("REF001");
        payment.GeneratedBy.Should().Be("user1");
        payment.RecordedBy.Should().Be("admin1");
    }

    [Fact]
    public void Payment_MonthFormat_ShouldBeYearMonthFormat()
    {
        // Arrange
        var payment = new Payment
        {
            Month = "2025-11"
        };

        // Assert
        payment.Month.Should().MatchRegex(@"^\d{4}-\d{2}$");
    }

    [Fact]
    public void Payment_Amount_ShouldAcceptDecimalValues()
    {
        // Arrange & Act
        var payment = new Payment
        {
            Amount = 123.45m
        };

        // Assert
        payment.Amount.Should().Be(123.45m);
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Paid")]
    [InlineData("Unpaid")]
    [InlineData("Overdue")]
    public void Payment_Status_ShouldAcceptValidStatuses(string status)
    {
        // Arrange & Act
        var payment = new Payment
        {
            Status = status
        };

        // Assert
        payment.Status.Should().Be(status);
    }

    [Fact]
    public void Payment_ShouldInheritFromBaseEntity()
    {
        // Arrange & Act
        var payment = new Payment();

        // Assert
        payment.Should().BeAssignableTo<BaseEntity>();
        payment.Id.Should().NotBeEmpty();
    }
}
