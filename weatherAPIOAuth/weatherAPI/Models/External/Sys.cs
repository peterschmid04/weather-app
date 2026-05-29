using System.Text.Json.Serialization;

namespace weatherAPI.Models.External;

/// <summary>
/// Country and sun-time metadata from the OpenWeatherMap current-weather
/// response.
/// </summary>
public class Sys
{
    [JsonPropertyName("country")]
    public string? Country { get; set; }
    [JsonPropertyName("sunrise")]
    public int Sunrise { get; set; }
    [JsonPropertyName("sunset")]
    public int Sunset { get; set; }
}
