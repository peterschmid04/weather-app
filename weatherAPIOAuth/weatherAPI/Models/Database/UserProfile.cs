namespace weatherAPI.Models.Database;

public class UserProfile
{
    public Guid Id { get; set; }

    public string Auth0Subject { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public string? Email { get; set; }

    public string? NormalizedEmail { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<SearchHistory> SearchHistory { get; set; } = new List<SearchHistory>();

    public ICollection<FavoriteCity> FavoriteCities { get; set; } = new List<FavoriteCity>();

    public ICollection<WeatherStation> WeatherStations { get; set; } = new List<WeatherStation>();

    public ICollection<WeatherStationShare> OwnedWeatherStationShares { get; set; } = new List<WeatherStationShare>();

    public ICollection<WeatherStationShare> ReceivedWeatherStationShares { get; set; } = new List<WeatherStationShare>();

    public ICollection<WeatherRequestLog> WeatherRequestLogs { get; set; } = new List<WeatherRequestLog>();

    public UserThemePreference? ThemePreference { get; set; }
}
