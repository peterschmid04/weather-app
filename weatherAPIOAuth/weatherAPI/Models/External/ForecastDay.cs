using System.Text.Json.Serialization;

namespace weatherAPI.Models.External;

public class ForecastDay
{
    [JsonPropertyName("dt")] 
    public long Dt { get; set; }

    [JsonPropertyName("temp")] 
    public required Temperature Temp { get; set; } 

    [JsonPropertyName("weather")]
    public required List<WeatherInfo> Weather { get; set; } 
}