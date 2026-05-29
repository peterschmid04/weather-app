using weatherAPI.Models.Dto;

namespace weatherAPI.Services.Interfaces
{
    /// <summary>
    /// Public contract for current-weather data used by /weather.
    /// </summary>
    public interface IWeatherService
    {
        Task<Weather?> GetWeather(string city);
    }
}
