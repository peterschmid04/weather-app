namespace weatherAPI.Models.Database;

public class WeatherStation
{
    public Guid Id { get; set; }

    public Guid UserProfileId { get; set; }

    public UserProfile User { get; set; } = null!;

    public Guid CityId { get; set; }

    public City City { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<WeatherStationMeasurement> Measurements { get; set; } = new List<WeatherStationMeasurement>();

    public ICollection<WeatherStationShare> Shares { get; set; } = new List<WeatherStationShare>();
}
