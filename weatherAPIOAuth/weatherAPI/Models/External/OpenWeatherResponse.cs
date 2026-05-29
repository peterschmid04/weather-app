using System.Text.Json.Serialization;

namespace weatherAPI.Models.External;

/// <summary>
/// Raw JSON shape for OpenWeatherMap current-weather responses.
/// </summary>
public class OpenWeatherResponse
{      
    [JsonPropertyName("coord")]
    public required Coord Coord { get; set; }
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("main")]
    public required Main Main { get; set; }
    [JsonPropertyName("visibility")]
    public double VisibilityMeters { get; set; }
    [JsonPropertyName("wind")]
    public required Wind Wind { get; set; }
    [JsonPropertyName("sys")]
    public required Sys Sys { get; set; }
    [JsonPropertyName("timezone")]
    public int Timezone { get; set; }
    [JsonPropertyName("weather")]
    public required WeatherInfo[] Weather { get; set; }
}
