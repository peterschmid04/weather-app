using weatherAPI.Models.Dto;

namespace weatherAPI.Services.Interfaces;

public interface IUvService
{
    Task<Uv?> GetUvIndex(double lat, double lon);
}