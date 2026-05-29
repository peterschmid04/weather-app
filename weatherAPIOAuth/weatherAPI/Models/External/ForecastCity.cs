using System.Text.Json.Serialization;

namespace weatherAPI.Models.External;

public class ForecastCity
{
    [JsonPropertyName("timezone")]
    public int Timezone { get; set; }
}
