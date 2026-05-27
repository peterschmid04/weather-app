using System.Text.Json.Serialization;

namespace weatherAPI.Models.External;

public class WeatherInfo
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }
    [JsonPropertyName("id")]
    public int Id { get; set; }
}