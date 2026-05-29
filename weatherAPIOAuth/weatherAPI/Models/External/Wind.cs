using System.Text.Json.Serialization;

namespace weatherAPI.Models.External;

/// <summary>
/// Wind data returned by OpenWeatherMap for the current weather response.
/// </summary>
public class Wind
{
    [JsonPropertyName("speed")]
    public double Speed { get; set; }
}
