namespace weatherAPI.Models.Database;

public class WeatherRequestLog
{
    public Guid Id { get; set; }

    public Guid? AppUserId { get; set; }

    public AppUser? User { get; set; }

    public Guid? CityId { get; set; }

    public City? City { get; set; }

    public string HttpMethod { get; set; } = string.Empty;

    public string Endpoint { get; set; } = string.Empty;

    public string? QueryText { get; set; }

    public int? StatusCode { get; set; }

    public bool WasSuccessful { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;
}
