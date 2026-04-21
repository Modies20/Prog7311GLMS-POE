using Xunit;
using GLMS.Data.Entities;
using GLMS.Web.Services;

namespace GLMS.Tests.Services;

public class ContractValidationTests
{
    private readonly ContractValidationService _validationService;

    public ContractValidationTests()
    {
        _validationService = new ContractValidationService();
    }

    [Fact]
    public void CanCreateServiceRequest_WithActiveContract_ReturnsTrue()
    {
        // Arrange
        var contract = new Contract
        {
            Status = ContractStatus.Active,
            StartDate = DateTime.Today.AddDays(-30),
            EndDate = DateTime.Today.AddDays(30)
        };

        // Act
        var result = _validationService.CanCreateServiceRequest(contract);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanCreateServiceRequest_WithExpiredContract_ReturnsFalse()
    {
        // Arrange
        var contract = new Contract
        {
            Status = ContractStatus.Expired,
            StartDate = DateTime.Today.AddDays(-60),
            EndDate = DateTime.Today.AddDays(-1)
        };

        // Act
        var result = _validationService.CanCreateServiceRequest(contract);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanCreateServiceRequest_WithDraftContract_ReturnsFalse()
    {
        // Arrange
        var contract = new Contract
        {
            Status = ContractStatus.Draft,
            StartDate = DateTime.Today.AddDays(7),
            EndDate = DateTime.Today.AddDays(37)
        };

        // Act
        var result = _validationService.CanCreateServiceRequest(contract);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanCreateServiceRequest_WithOnHoldContract_ReturnsFalse()
    {
        // Arrange
        var contract = new Contract
        {
            Status = ContractStatus.OnHold,
            StartDate = DateTime.Today.AddDays(-30),
            EndDate = DateTime.Today.AddDays(30)
        };

        // Act
        var result = _validationService.CanCreateServiceRequest(contract);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanCreateServiceRequest_WithContractNotStarted_ReturnsFalse()
    {
        // Arrange
        var contract = new Contract
        {
            Status = ContractStatus.Active,
            StartDate = DateTime.Today.AddDays(7),
            EndDate = DateTime.Today.AddDays(37)
        };

        // Act
        var result = _validationService.CanCreateServiceRequest(contract);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanCreateServiceRequest_WithNullContract_ReturnsFalse()
    {
        // Act
        var result = _validationService.CanCreateServiceRequest(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetValidationErrorMessage_WithExpiredContract_ReturnsCorrectMessage()
    {
        // Arrange
        var contract = new Contract
        {
            ContractNumber = "CT-001",
            Status = ContractStatus.Expired,
            EndDate = DateTime.Today.AddDays(-1)
        };

        // Act
        var message = _validationService.GetValidationErrorMessage(contract);

        // Assert
        Assert.Contains("expired", message.ToLower());
        Assert.Contains("CT-001", message);
    }

    [Fact]
    public void GetValidationErrorMessage_WithNullContract_ReturnsCorrectMessage()
    {
        // Act
        var message = _validationService.GetValidationErrorMessage(null);

        // Assert
        Assert.Contains("does not exist", message);
    }
}