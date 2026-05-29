using System.Text.Json.Serialization;

namespace weatherAPI.Models.External;

public class ForecastApiResponse
{
    [JsonPropertyName("list")]
    public required List<ForecastDay> List { get; set; }

    [JsonPropertyName("city")]
    public ForecastCity? City { get; set; }
}
