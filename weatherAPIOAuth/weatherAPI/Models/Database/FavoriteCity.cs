namespace weatherAPI.Models.Database;

/// <summary>
/// User-scoped favorite city. IsDefault marks the startup city after login.
/// </summary>
public class FavoriteCity
{
    public Guid Id { get; set; }

    public Guid UserProfileId { get; set; }

    public UserProfile User { get; set; } = null!;

    public Guid CityId { get; set; }

    public City City { get; set; } = null!;

    public bool IsDefault { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
