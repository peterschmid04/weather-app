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
