namespace weatherAPI.Models.Database;

public class SearchHistory
{
    public Guid Id { get; set; }

    public Guid AppUserId { get; set; }

    public AppUser User { get; set; } = null!;

    public Guid CityId { get; set; }

    public City City { get; set; } = null!;

    public string QueryText { get; set; } = string.Empty;

    public DateTime SearchedAtUtc { get; set; } = DateTime.UtcNow;
}
