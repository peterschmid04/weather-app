using System.Text.Json.Serialization;

namespace weatherAPI.Models.External;

/// <summary>
/// City metadata from the forecast response, currently used for timezone
/// conversion when daily forecast labels are calculated.
/// </summary>
public class ForecastCity
{
    [JsonPropertyName("timezone")]
    public int Timezone { get; set; }
}
