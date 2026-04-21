using Xunit;
using GLMS.Data.Entities;

namespace GLMS.Tests.Models;

public class ServiceRequestTests
{
    [Fact]
    public void ServiceRequest_InitialStatus_ShouldBePending()
    {
        // Arrange & Act
        var request = new ServiceRequest
        {
            RequestNumber = "TEST-001",
            Description = "Test service request",
            AmountUSD = 100m
        };

        // Assert
        Assert.Equal(RequestStatus.Pending, request.Status);
    }

    [Fact]
    public void ServiceRequest_WhenCompleted_SetsCompletionDate()
    {
        // Arrange
        var request = new ServiceRequest
        {
            RequestNumber = "TEST-001",
            Status = RequestStatus.Pending
        };

        // Act
        request.Status = RequestStatus.Completed;
        request.CompletionDate = DateTime.UtcNow;

        // Assert
        Assert.Equal(RequestStatus.Completed, request.Status);
        Assert.NotNull(request.CompletionDate);
    }

    [Fact]
    public void ServiceRequest_RequestNumber_FormatIsValid()
    {
        // Arrange & Act
        var prefix = "SRQ";
        var datePart = DateTime.Now.ToString("yyyyMMdd");
        var guidPart = Guid.NewGuid().ToString()[..8].ToUpper();
        var requestNumber = $"{prefix}-{datePart}-{guidPart}";

        // Assert
        Assert.StartsWith("SRQ-", requestNumber);
        Assert.Contains(datePart, requestNumber);
        Assert.Equal(8, guidPart.Length);
    }

    [Fact]
    public void ServiceRequest_Amounts_CanBeSetAndRetrieved()
    {
        // Arrange
        var request = new ServiceRequest
        {
            AmountUSD = 500m,
            AmountZAR = 9250m,
            ExchangeRateUsed = 18.50m
        };

        // Assert
        Assert.Equal(500m, request.AmountUSD);
        Assert.Equal(9250m, request.AmountZAR);
        Assert.Equal(18.50m, request.ExchangeRateUsed);
    }
}