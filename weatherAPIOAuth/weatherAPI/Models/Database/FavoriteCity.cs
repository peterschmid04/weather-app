namespace weatherAPI.Models.Database;

public class FavoriteCity
{
    public Guid Id { get; set; }

    public Guid AppUserId { get; set; }

    public AppUser User { get; set; } = null!;

    public Guid CityId { get; set; }

    public City City { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
