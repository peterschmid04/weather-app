using System.Text.Json.Serialization;

namespace weatherAPI.Models.External;

/// <summary>
/// Temperature block from one forecast entry.
/// </summary>
public class ForecastMain
{
    [JsonPropertyName("temp")]
    public double Temp { get; set; }

    [JsonPropertyName("temp_min")]
    public double TempMin { get; set; }

    [JsonPropertyName("temp_max")]
    public double TempMax { get; set; }
}
