using System.Globalization;
using weatherAPI.Models.Dto;
using weatherAPI.Services.Interfaces;
namespace weatherAPI.Services;

/// <summary>
/// Converts OpenWeatherMap 3-hour forecast entries into day cards for the UI.
/// </summary>
public class ForecastService : IForecastService
{
    private readonly IOpenWeatherApiService _forecastApiClient;
    public ForecastService(IOpenWeatherApiService forecastApiClient)
    {
        _forecastApiClient  = forecastApiClient;
    }
    
    /// <summary>
    /// Groups forecast entries by local day, skips the current day and uses the
    /// entry closest to noon for icon/description.
    /// </summary>
    public async Task<List<Forecast>?> GetForecast(double lat, double lon)
    {
        var forecastData = await _forecastApiClient.GetForecast(lat, lon);
        if (forecastData == null) return null;

        var timezone = TimeSpan.FromSeconds(forecastData.City?.Timezone ?? 0);
        var germanCulture = CultureInfo.GetCultureInfo("de-DE");

        var forecastList = forecastData.List
            .Select(item => new
            {
                Item = item,
                LocalTime = DateTimeOffset.FromUnixTimeSeconds(item.Dt).ToOffset(timezone)
            })
            .GroupBy(entry => entry.LocalTime.Date)
            .OrderBy(group => group.Key)
            .Skip(1)
            .Take(6)
            .Select(group =>
            {
                var entries = group.ToList();
                var representative = entries
                    .OrderBy(entry => Math.Abs(entry.LocalTime.Hour - 12))
                    .First()
                    .Item;
                var representativeWeather = representative.Weather.FirstOrDefault();

                return new Forecast
                {
                    Day = group.Key.ToString("dddd", germanCulture),
                    TempMin = Math.Round(entries.Min(entry => entry.Item.Main.TempMin), 1),
                    TempMax = Math.Round(entries.Max(entry => entry.Item.Main.TempMax), 1),
                    Id = representativeWeather?.Id ?? 0,
                    Description = representativeWeather?.Description,
                    Icon = representativeWeather?.Icon
                };
            })
            .ToList();

        return forecastList;
    }
}
