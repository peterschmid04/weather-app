using weatherAPI.Models.Dto;

namespace weatherAPI.Services.Interfaces;

/// <summary>
/// Public contract for the /uv endpoint.
/// </summary>
public interface IUvService
{
    Task<Uv?> GetUvIndex(double lat, double lon);
}
