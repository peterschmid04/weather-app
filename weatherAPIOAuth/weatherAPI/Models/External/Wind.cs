using System.Text.Json.Serialization;

namespace weatherAPI.Models.External;

public class Wind
{
    [JsonPropertyName("speed")]
    public double Speed { get; set; }
}