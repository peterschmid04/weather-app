using System.Text.Json.Serialization;

namespace weatherAPI.Models.External;

public class ForecastDay
{
    [JsonPropertyName("dt")] 
    public long Dt { get; set; }

    [JsonPropertyName("main")]
    public required ForecastMain Main { get; set; }

    [JsonPropertyName("weather")]
    public required List<WeatherInfo> Weather { get; set; } 
}
