using GLMS.Web.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using Xunit;

namespace GLMS.Tests.Services;

public class CurrencyExchangeTests
{
    [Fact]
    public async Task ConvertUSDtoZAR_WithValidRate_ReturnsCorrectAmount()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var responseContent = "{\"base\":\"USD\",\"date\":\"2024-01-01\",\"rates\":{\"ZAR\":18.50}}";

        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var mockLogger = new Mock<ILogger<CurrencyExchangeService>>();

        var service = new CurrencyExchangeService(httpClient, mockLogger.Object);
        var usdAmount = 100m;
        var expectedZar = 1850m;

        // Act
        var result = await service.ConvertUSDtoZARAsync(usdAmount);

        // Assert
        Assert.Equal(expectedZar, result);
    }

    [Fact]
    public async Task GetUSDtoZARRateAsync_ReturnsValidRate()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var responseContent = "{\"base\":\"USD\",\"date\":\"2024-01-01\",\"rates\":{\"ZAR\":18.50}}";

        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var mockLogger = new Mock<ILogger<CurrencyExchangeService>>();
        var service = new CurrencyExchangeService(httpClient, mockLogger.Object);

        // Act
        var rate = await service.GetUSDtoZARRateAsync();

        // Assert
        Assert.Equal(18.50m, rate);
    }

    [Theory]
    [InlineData(10, 18.50, 185)]
    [InlineData(25.50, 18.50, 471.75)]
    [InlineData(100, 19.20, 1920)]
    public async Task CurrencyConversion_MultipleScenarios_ReturnsCorrectAmount(
        decimal usdAmount, decimal rate, decimal expectedZar)
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var responseContent = $"{{\"base\":\"USD\",\"date\":\"2024-01-01\",\"rates\":{{\"ZAR\":{rate.ToString(System.Globalization.CultureInfo.InvariantCulture)}}}}}";

        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var mockLogger = new Mock<ILogger<CurrencyExchangeService>>();
        var service = new CurrencyExchangeService(httpClient, mockLogger.Object);

        // Act
        var result = await service.ConvertUSDtoZARAsync(usdAmount);

        // Assert
        Assert.Equal(expectedZar, Math.Round(result, 2));
    }
}
