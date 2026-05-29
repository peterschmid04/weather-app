using System.Text.Json.Serialization;

namespace weatherAPI.Models.External;

/// <summary>
/// Main current-weather values from OpenWeatherMap.
/// </summary>
public class Main
{
    [JsonPropertyName("temp")]
    public double Temp { get; set; }
    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }
}
