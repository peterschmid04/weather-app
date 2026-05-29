namespace weatherAPI.Models.Database;

/// <summary>
/// Stored search query for a user and city, trimmed by the backend to the UI limit.
/// </summary>
public class SearchHistory
{
    public Guid Id { get; set; }

    public Guid UserProfileId { get; set; }

    public UserProfile User { get; set; } = null!;

    public Guid CityId { get; set; }

    public City City { get; set; } = null!;

    public string QueryText { get; set; } = string.Empty;

    public DateTime SearchedAtUtc { get; set; } = DateTime.UtcNow;
}
