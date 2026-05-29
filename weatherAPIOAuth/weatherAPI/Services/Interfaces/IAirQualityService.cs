using weatherAPI.Models.Dto;

namespace weatherAPI.Services.Interfaces;

/// <summary>
/// Public contract for the /airquality endpoint.
/// </summary>
public interface IAirQualityService
{
    Task<AirQuality?> GetAirQuality(double lat, double lon);
}
