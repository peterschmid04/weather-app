namespace weatherAPI.Models.Database;

public class City
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    public string CountryCode { get; set; } = string.Empty;

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public ICollection<SearchHistory> SearchHistory { get; set; } = new List<SearchHistory>();

    public ICollection<FavoriteCity> FavoriteCities { get; set; } = new List<FavoriteCity>();

    public ICollection<WeatherStation> WeatherStations { get; set; } = new List<WeatherStation>();

    public ICollection<WeatherRequestLog> WeatherRequestLogs { get; set; } = new List<WeatherRequestLog>();
}
