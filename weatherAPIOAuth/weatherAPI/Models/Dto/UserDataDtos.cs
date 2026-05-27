namespace weatherAPI.Models.Dto;

public record CityRequest(
    string CityName,
    string? CountryCode,
    double? Latitude,
    double? Longitude);

public record SearchHistoryResponse(
    Guid Id,
    string QueryText,
    string CityName,
    string CountryCode,
    double? Latitude,
    double? Longitude,
    DateTime SearchedAtUtc);

public record FavoriteCityResponse(
    Guid Id,
    string CityName,
    string CountryCode,
    double? Latitude,
    double? Longitude,
    DateTime CreatedAtUtc);

public record ThemePreferenceResponse(string ThemeName);

public record UpdateThemePreferenceRequest(string ThemeName);
