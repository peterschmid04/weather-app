using weatherAPI.Models.Dto;
using weatherAPI.Services.Interfaces;

namespace weatherAPI.Services
{
    public class AirQualityService: IAirQualityService
    {
        private readonly IOpenWeatherApiService _airQualityApiClient;
        
        public AirQualityService(IOpenWeatherApiService airQualityApiClient)
        {
            _airQualityApiClient = airQualityApiClient;
        }
        
        public async Task<AirQuality?> GetAirQuality(double lat, double lon)
        {
            var aqiValue = await _airQualityApiClient.GetAirQuality(lat, lon);
            if (aqiValue == null) return null;
            return new AirQuality { Aqi = aqiValue };
        }
    }
}