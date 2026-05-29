using System.Text.Json.Serialization;

namespace weatherAPI.Models.External;

/// <summary>
/// Weather condition details from OpenWeatherMap, including the condition id
/// used by the frontend icon mapping.
/// </summary>
public class WeatherInfo
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }
    [JsonPropertyName("id")]
    public int Id { get; set; }
}
