namespace weatherAPI.Models.Dto;

public class Forecast
{
    public required string Day { get; set; } 
    public string? Description { get; set; } 
    public string? Icon { get; set; } 
    public double TempMin { get; set; }
    public double TempMax { get; set; }
    public int Id { get; set; }
}