namespace weatherAPI.Models.Dto;

/// <summary>
/// Current-weather response returned by /weather to the React dashboard.
/// </summary>
public class Weather
{
    public required double Lat { get; set; }
    public required double Lon { get; set; }   
    public required string City { get; set; }
    public string? Country { get; set; }
    public double TemperatureC { get; set; }
    public int Humidity { get; set; }
    public double VisibilityKm { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int WeatherId { get; set; }
    public double WindSpeed { get; set; }
    public string? Sunrise { get; set; }
    public string? Sunset { get; set; }
    public int TimezoneOffsetHours { get; set; }
}
