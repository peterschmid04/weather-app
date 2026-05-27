using System.Text.Json.Serialization;

namespace weatherAPI.Models.External;

public class Temperature
{
    [JsonPropertyName("min")]
    public double Min { get; set; }

    [JsonPropertyName("max")]
    public double Max { get; set; }
}