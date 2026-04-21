using System.Text.Json;

namespace GLMS.Web.Services;

public class CurrencyExchangeService : ICurrencyExchangeService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CurrencyExchangeService> _logger;
    private const string BASE_URL = "https://api.exchangerate-api.com/v4/latest/USD";

    public CurrencyExchangeService(HttpClient httpClient, ILogger<CurrencyExchangeService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<decimal> GetUSDtoZARRateAsync()
    {
        try
        {
            _logger.LogInformation("Fetching current USD to ZAR exchange rate");

            var response = await _httpClient.GetAsync(BASE_URL);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            // .NET 9.0 improved JSON deserialization
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };

            var exchangeData = JsonSerializer.Deserialize<ExchangeRateResponse>(json, options);

            if (exchangeData?.Rates != null && exchangeData.Rates.TryGetValue("ZAR", out var rate))
            {
                _logger.LogInformation("Exchange rate retrieved: 1 USD = {Rate} ZAR", rate);
                return rate;
            }

            _logger.LogWarning("ZAR rate not found in API response, using fallback rate");
            return 18.50m;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching exchange rate");
            return 18.50m;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error fetching exchange rate");
            return 18.50m;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching exchange rate");
            return 18.50m;
        }
    }

    public async Task<decimal> ConvertUSDtoZARAsync(decimal usdAmount)
    {
        var rate = await GetUSDtoZARRateAsync();
        var zarAmount = usdAmount * rate;

        _logger.LogInformation("Converted {USD} USD to {ZAR} ZAR at rate {Rate}",
            usdAmount, zarAmount, rate);

        return zarAmount;
    }
}

public class ExchangeRateResponse
{
    public string? Base { get; set; }
    public DateTime Date { get; set; }
    public Dictionary<string, decimal>? Rates { get; set; }
}