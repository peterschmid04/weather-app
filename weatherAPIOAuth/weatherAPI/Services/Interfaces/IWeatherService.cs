using weatherAPI.Models.Dto;

namespace weatherAPI.Services.Interfaces
{
    public interface IWeatherService
    {
        Task<Weather?> GetWeather(string city);
    }
}