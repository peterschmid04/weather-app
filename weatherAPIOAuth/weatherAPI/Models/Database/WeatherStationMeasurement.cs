namespace weatherAPI.Models.Database;

public class WeatherStationMeasurement
{
    public Guid Id { get; set; }

    public Guid WeatherStationId { get; set; }

    public WeatherStation WeatherStation { get; set; } = null!;

    public DateTime MeasuredAtUtc { get; set; } = DateTime.UtcNow;

    public double? TemperatureC { get; set; }

    public double? HumidityPercent { get; set; }

    public double? PressureHpa { get; set; }

    public double? WindSpeedKmh { get; set; }

    public int? WindDirectionDegrees { get; set; }

    public double? RainfallMm { get; set; }

    public string? Notes { get; set; }
}
