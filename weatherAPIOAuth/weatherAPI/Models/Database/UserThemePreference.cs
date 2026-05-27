namespace weatherAPI.Models.Database;

public class UserThemePreference
{
    public Guid Id { get; set; }

    public Guid UserProfileId { get; set; }

    public UserProfile User { get; set; } = null!;

    public string ThemeName { get; set; } = "graphite";

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
