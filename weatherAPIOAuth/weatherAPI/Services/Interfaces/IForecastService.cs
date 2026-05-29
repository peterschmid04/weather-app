using weatherAPI.Models.Dto;

namespace weatherAPI.Services.Interfaces;

/// <summary>
/// Public contract for forecast day-card data.
/// </summary>
public interface IForecastService
{
    Task<List<Forecast>?> GetForecast(double lat, double lon);
}
