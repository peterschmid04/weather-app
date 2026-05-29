using weatherAPI.Models.Dto;
using weatherAPI.Services.Interfaces;

namespace weatherAPI.Services
{
    /// <summary>
    /// Small mapping service for the UV endpoint. It keeps the endpoint away
    /// from raw OpenWeatherMap JSON details.
    /// </summary>
    public class UvService: IUvService
    {
        private readonly IOpenWeatherApiService _uvApiClient;
        
        public UvService(IOpenWeatherApiService uvApiClient)
        {
          _uvApiClient = uvApiClient;
        }
        
        /// <summary>
        /// Returns the UV DTO for the given coordinates or null when the external
        /// API does not provide a value.
        /// </summary>
        public async Task<Uv?> GetUvIndex(double lat, double lon)
        {
            var uvValue = await _uvApiClient.GetUvIndex(lat, lon);
            if (uvValue == null) return null;
            return new Uv { UvIndex = uvValue };
        }
    }
}
