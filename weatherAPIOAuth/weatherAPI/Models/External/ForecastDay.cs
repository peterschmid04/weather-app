using System.Text.Json.Serialization;

namespace weatherAPI.Models.External;

/// <summary>
/// One 3-hour forecast entry returned by OpenWeatherMap.
/// The backend groups these entries into daily cards for the frontend.
/// </summary>
public class ForecastDay
{
    [JsonPropertyName("dt")] 
    public long Dt { get; set; }

    [JsonPropertyName("main")]
    public required ForecastMain Main { get; set; }

    [JsonPropertyName("weather")]
    public required List<WeatherInfo> Weather { get; set; } 
}
