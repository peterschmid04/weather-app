using weatherAPI.Models.Dto;
using weatherAPI.Services.Interfaces;

namespace weatherAPI.Services
{
    /// <summary>
    /// Maps the OpenWeatherMap air-quality value into the DTO returned by the API.
    /// </summary>
    public class AirQualityService: IAirQualityService
    {
        private readonly IOpenWeatherApiService _airQualityApiClient;
        
        public AirQualityService(IOpenWeatherApiService airQualityApiClient)
        {
            _airQualityApiClient = airQualityApiClient;
        }
        
        /// <summary>
        /// Returns AQI for coordinates or null when the external API call fails.
        /// </summary>
        public async Task<AirQuality?> GetAirQuality(double lat, double lon)
        {
            var aqiValue = await _airQualityApiClient.GetAirQuality(lat, lon);
            if (aqiValue == null) return null;
            return new AirQuality { Aqi = aqiValue };
        }
    }
}
