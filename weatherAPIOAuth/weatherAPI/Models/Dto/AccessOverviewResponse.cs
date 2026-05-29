namespace weatherAPI.Models.Dto;

public record AccessOverviewResponse(
    Guid UserId,
    string? Name,
    string? Email,
    string RegionScope,
    IReadOnlyCollection<string> AllowedRegions,
    IReadOnlyCollection<string> Permissions,
    IReadOnlyCollection<string> Roles);
