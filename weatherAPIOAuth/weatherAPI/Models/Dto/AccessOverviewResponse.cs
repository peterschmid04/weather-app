namespace weatherAPI.Models.Dto;

/// <summary>
/// Summarizes the current Auth0 user profile and the region permissions that
/// the backend derives from the JWT claims.
/// </summary>
public record AccessOverviewResponse(
    Guid UserId,
    string? Name,
    string? Email,
    string RegionScope,
    IReadOnlyCollection<string> AllowedRegions,
    IReadOnlyCollection<string> Permissions,
    IReadOnlyCollection<string> Roles);
