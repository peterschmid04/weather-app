namespace weatherAPI.Models.Database;

/// <summary>
/// Invitation or accepted permission that grants another Auth0 user access to a station.
/// </summary>
public class WeatherStationShare
{
    public Guid Id { get; set; }

    public Guid WeatherStationId { get; set; }

    public WeatherStation WeatherStation { get; set; } = null!;

    public Guid OwnerUserProfileId { get; set; }

    public UserProfile Owner { get; set; } = null!;

    public Guid? SharedWithUserProfileId { get; set; }

    public UserProfile? SharedWithUser { get; set; }

    public string SharedWithEmail { get; set; } = string.Empty;

    public string NormalizedSharedWithEmail { get; set; } = string.Empty;

    public string Permission { get; set; } = "write_measurements";

    public string Status { get; set; } = "pending";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? AcceptedAtUtc { get; set; }
}
