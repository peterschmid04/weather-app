using System.Globalization;
using weatherAPI.Models.Dto;
using weatherAPI.Services.Interfaces;
namespace weatherAPI.Services;

public class ForecastService : IForecastService
{
    private readonly IOpenWeatherApiService _forecastApiClient;
    public ForecastService(IOpenWeatherApiService forecastApiClient)
    {
        _forecastApiClient  = forecastApiClient;
    }
    
    public async Task<List<Forecast>?> GetForecast(double lat, double lon)
    {
        var forecastData = await _forecastApiClient.GetForecast(lat, lon);
        if (forecastData == null) return null;
        
        var forecastList = forecastData.List
            .Skip(1).Take(6)
            .Select(day => new Forecast
            {
                Day = DateTimeOffset.FromUnixTimeSeconds(day.Dt).DateTime
                    .ToString("dddd", CultureInfo.InvariantCulture),
                TempMin = day.Temp.Min,
                TempMax = day.Temp.Max,
                Id = day.Weather[0].Id,
                Description = day.Weather[0].Description,
                Icon = day.Weather[0].Icon
            }).ToList();

        return forecastList;
    }
}
