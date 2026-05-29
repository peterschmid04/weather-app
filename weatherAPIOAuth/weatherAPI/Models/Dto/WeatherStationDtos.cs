namespace weatherAPI.Models.Dto;

public record CreateWeatherStationRequest(
    string Name,
    string? CityName,
    string? CountryCode,
    string? Description,
    double? Latitude,
    double? Longitude);

public record UpdateWeatherStationRequest(
    string Name,
    string? CityName,
    string? CountryCode,
    string? Description,
    double? Latitude,
    double? Longitude);

public record CreateWeatherStationMeasurementRequest(
    double? TemperatureC,
    double? HumidityPercent,
    double? PressureHpa,
    double? WindSpeedKmh,
    int? WindDirectionDegrees,
    double? RainfallMm,
    string? Notes,
    DateTime? MeasuredAtUtc);

public record WeatherStationResponse(
    Guid Id,
    string Name,
    string CityName,
    string CountryCode,
    string? Description,
    double? Latitude,
    double? Longitude,
    DateTime CreatedAtUtc,
    bool IsOwner,
    string AccessLevel,
    string? OwnerName,
    WeatherStationMeasurementResponse? LatestMeasurement);

public record WeatherStationMeasurementResponse(
    Guid Id,
    DateTime MeasuredAtUtc,
    double? TemperatureC,
    double? HumidityPercent,
    double? PressureHpa,
    double? WindSpeedKmh,
    int? WindDirectionDegrees,
    double? RainfallMm,
    string? Notes);

public record UserProfileResponse(
    Guid Id,
    string Subject,
    string? Name,
    string? Email);

public record CreateWeatherStationShareRequest(
    Guid WeatherStationId,
    string Email,
    string? Permission);

public record WeatherStationShareOverviewResponse(
    IReadOnlyCollection<OutgoingWeatherStationShareResponse> Outgoing,
    IReadOnlyCollection<IncomingWeatherStationShareResponse> Incoming);

public record OutgoingWeatherStationShareResponse(
    Guid Id,
    Guid WeatherStationId,
    string StationName,
    string SharedWithEmail,
    string Permission,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? AcceptedAtUtc);

public record IncomingWeatherStationShareResponse(
    Guid Id,
    Guid WeatherStationId,
    string StationName,
    string OwnerName,
    string? OwnerEmail,
    string Permission,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? AcceptedAtUtc);
