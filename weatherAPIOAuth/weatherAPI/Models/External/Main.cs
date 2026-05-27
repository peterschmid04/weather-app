using System.Text.Json.Serialization;

namespace weatherAPI.Models.External;

public class Main
{
    [JsonPropertyName("temp")]
    public double Temp { get; set; }
    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }
}