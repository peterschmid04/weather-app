using weatherAPI.Models.Dto;

namespace weatherAPI.Services.Interfaces;

public interface IAirQualityService
{
    Task<AirQuality?> GetAirQuality(double lat, double lon);
}