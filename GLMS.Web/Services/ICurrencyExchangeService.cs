namespace GLMS.Web.Services;

public interface ICurrencyExchangeService
{
    Task<decimal> GetUSDtoZARRateAsync();
    Task<decimal> ConvertUSDtoZARAsync(decimal usdAmount);
}
