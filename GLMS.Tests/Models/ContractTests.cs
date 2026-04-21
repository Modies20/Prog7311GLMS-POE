using Xunit;
using GLMS.Data.Entities;

namespace GLMS.Tests.Models;

public class ContractTests
{
    [Fact]
    public void Contract_IsActive_WithActiveStatusAndCurrentDate_ReturnsTrue()
    {
        // Arrange
        var contract = new Contract
        {
            Status = ContractStatus.Active,
            StartDate = DateTime.Today.AddDays(-10),
            EndDate = DateTime.Today.AddDays(10)
        };

        // Act & Assert
        Assert.True(contract.IsActive);
    }

    [Fact]
    public void Contract_IsActive_WithExpiredDate_ReturnsFalse()
    {
        // Arrange
        var contract = new Contract
        {
            Status = ContractStatus.Active,
            StartDate = DateTime.Today.AddDays(-20),
            EndDate = DateTime.Today.AddDays(-1)
        };

        // Act & Assert
        Assert.False(contract.IsActive);
    }

    [Fact]
    public void Contract_IsActive_WithFutureStartDate_ReturnsFalse()
    {
        // Arrange
        var contract = new Contract
        {
            Status = ContractStatus.Active,
            StartDate = DateTime.Today.AddDays(5),
            EndDate = DateTime.Today.AddDays(35)
        };

        // Act & Assert
        Assert.False(contract.IsActive);
    }

    [Fact]
    public void Contract_IsActive_WithNonActiveStatus_ReturnsFalse()
    {
        // Arrange
        var contract = new Contract
        {
            Status = ContractStatus.Draft,
            StartDate = DateTime.Today.AddDays(-10),
            EndDate = DateTime.Today.AddDays(10)
        };

        // Act & Assert
        Assert.False(contract.IsActive);
    }

    [Fact]
    public void Contract_DaysRemaining_CalculatesCorrectly()
    {
        // Arrange
        var contract = new Contract
        {
            EndDate = DateTime.Today.AddDays(15)
        };

        // Act
        var daysRemaining = contract.DaysRemaining;

        // Assert
        Assert.Equal(15, daysRemaining);
    }

    [Fact]
    public void Contract_DaysRemaining_WithExpiredContract_ReturnsZero()
    {
        // Arrange
        var contract = new Contract
        {
            EndDate = DateTime.Today.AddDays(-5)
        };

        // Act
        var daysRemaining = contract.DaysRemaining;

        // Assert
        Assert.Equal(0, daysRemaining);
    }
}