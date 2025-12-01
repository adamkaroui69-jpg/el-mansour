using ElMansourSyndicManager.Core.Domain.Entities;
using FluentAssertions;

namespace ElMansourSyndicManager.Tests.Unit.Domain;

/// <summary>
/// Tests unitaires pour l'entité House
/// </summary>
public class HouseTests
{
    [Fact]
    public void House_ShouldInitializeWithDefaultValues()
    {
        // Act
        var house = new House();

        // Assert
        house.HouseCode.Should().Be(string.Empty);
        house.BuildingCode.Should().Be(string.Empty);
        house.OwnerName.Should().Be(string.Empty);
        house.MonthlyAmount.Should().Be(0);
        house.IsActive.Should().BeTrue();
    }

    [Fact]
    public void House_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var houseId = Guid.NewGuid();

        // Act
        var house = new House
        {
            Id = houseId,
            HouseCode = "A01",
            BuildingCode = "A",
            OwnerName = "John Doe",
            ContactNumber = "12345678",
            Email = "john@example.com",
            MonthlyAmount = 150.00m,
            IsActive = true
        };

        // Assert
        house.Id.Should().Be(houseId);
        house.HouseCode.Should().Be("A01");
        house.BuildingCode.Should().Be("A");
        house.OwnerName.Should().Be("John Doe");
        house.ContactNumber.Should().Be("12345678");
        house.Email.Should().Be("john@example.com");
        house.MonthlyAmount.Should().Be(150.00m);
        house.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("A", "A01")]
    [InlineData("A", "A12")]
    [InlineData("B", "B23")]
    [InlineData("C", "C34")]
    public void House_HouseCode_ShouldFollowNamingConvention(string building, string expectedCode)
    {
        // Arrange & Act
        var house = new House
        {
            BuildingCode = building,
            HouseCode = expectedCode
        };

        // Assert
        house.HouseCode.Should().Be(expectedCode);
        house.HouseCode.Should().StartWith(building);
    }

    [Fact]
    public void House_MonthlyAmount_ShouldBePositive()
    {
        // Arrange & Act
        var house = new House
        {
            MonthlyAmount = 200.00m
        };

        // Assert
        house.MonthlyAmount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void House_IsActive_ShouldBeToggleable()
    {
        // Arrange
        var house = new House
        {
            IsActive = true
        };

        // Act
        house.IsActive = false;

        // Assert
        house.IsActive.Should().BeFalse();
    }

    [Fact]
    public void House_ShouldInheritFromBaseEntity()
    {
        // Arrange & Act
        var house = new House();

        // Assert
        house.Should().BeAssignableTo<BaseEntity>();
        house.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("A")]
    [InlineData("B")]
    [InlineData("C")]
    [InlineData("D")]
    [InlineData("E")]
    public void House_BuildingCode_ShouldBeValidBuilding(string buildingCode)
    {
        // Arrange & Act
        var house = new House
        {
            BuildingCode = buildingCode
        };

        // Assert
        house.BuildingCode.Should().Be(buildingCode);
        house.BuildingCode.Should().MatchRegex(@"^[A-E]$");
    }

    [Fact]
    public void House_Email_ShouldAcceptValidEmail()
    {
        // Arrange & Act
        var house = new House
        {
            Email = "owner@example.com"
        };

        // Assert
        house.Email.Should().Be("owner@example.com");
        house.Email.Should().Contain("@");
    }
}
