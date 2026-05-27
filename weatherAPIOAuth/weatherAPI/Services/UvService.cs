using weatherAPI.Models.Dto;
using weatherAPI.Services.Interfaces;

namespace weatherAPI.Services
{
    public class UvService: IUvService
    {
        private readonly IOpenWeatherApiService _uvApiClient;
        
        public UvService(IOpenWeatherApiService uvApiClient)
        {
          _uvApiClient = uvApiClient;
        }
        
        public async Task<Uv?> GetUvIndex(double lat, double lon)
        {
            var uvValue = await _uvApiClient.GetUvIndex(lat, lon);
            if (uvValue == null) return null;
            return new Uv { UvIndex = uvValue };
        }
    }
}