using weatherAPI.Models.Dto;

namespace weatherAPI.Services.Interfaces;

public interface IForecastService
{
    Task<List<Forecast>?> GetForecast(double lat, double lon);
}