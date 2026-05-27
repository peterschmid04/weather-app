using weatherAPI.Models.External;

namespace weatherAPI.Services.Interfaces;

public interface IOpenWeatherApiService
{
    Task<OpenWeatherResponse?> GetWeather(string city);
    Task<double?> GetUvIndex(double lat, double lon);  
    Task<double?> GetAirQuality(double lat, double lon);
    Task<ForecastApiResponse?> GetForecast(double lat, double lon);
}
