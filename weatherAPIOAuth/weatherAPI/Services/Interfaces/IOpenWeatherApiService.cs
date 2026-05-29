using weatherAPI.Models.External;

namespace weatherAPI.Services.Interfaces;

/// <summary>
/// Boundary around OpenWeatherMap calls so higher services can be tested
/// without depending directly on HTTP or external API URLs.
/// </summary>
public interface IOpenWeatherApiService
{
    Task<OpenWeatherResponse?> GetWeather(string city);
    Task<double?> GetUvIndex(double lat, double lon);  
    Task<double?> GetAirQuality(double lat, double lon);
    Task<ForecastApiResponse?> GetForecast(double lat, double lon);
}
