using System.Text.Json.Serialization;

namespace weatherAPI.Models.External;

/// <summary>
/// Raw JSON shape for OpenWeatherMap 5-day/3-hour forecast responses.
/// </summary>
public class ForecastApiResponse
{
    [JsonPropertyName("list")]
    public required List<ForecastDay> List { get; set; }

    [JsonPropertyName("city")]
    public ForecastCity? City { get; set; }
}
