using System.Text.Json;
using System.Globalization;
using weatherAPI.Models.External;
using weatherAPI.Services.Interfaces;


namespace weatherAPI.Services;

public class OpenWeatherApiService : IOpenWeatherApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    
    public OpenWeatherApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }
    
    public async Task<OpenWeatherResponse?> GetWeather(string city)
    {
        var apiKey = _configuration["OpenWeatherMap:ApiKey"];    
        var client = _httpClientFactory.CreateClient();  
        var apiUrl = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}";
        var response = await client.GetAsync(apiUrl);
    
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OpenWeatherResponse>(json);
    }
    
    public async Task<double?> GetAirQuality(double lat, double lon)
    {
        var apiKey = _configuration["OpenWeatherMap:ApiKey"];
        var client = _httpClientFactory.CreateClient();
        var apiUrl =
            $"https://api.openweathermap.org/data/2.5/air_pollution?lat={lat}&lon={lon}&appid={apiKey}&units=metric";
        var response = await client.GetAsync(apiUrl);

        if (!response.IsSuccessStatusCode) return null;
        var jsonString = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(jsonString);
        var aqiValue = jsonDoc.RootElement.GetProperty("list").EnumerateArray().First().GetProperty("main")
            .GetProperty("aqi").GetDouble();
        return aqiValue;
    }
    
    public async Task<ForecastApiResponse?> GetForecast(double lat, double lon)
    {
        var apiKey = _configuration["OpenWeatherMap:ApiKey"];
        var client = _httpClientFactory.CreateClient();
        var apiUrl =
            $"https://api.openweathermap.org/data/2.5/forecast/daily?lat={lat}&lon={lon}&cnt={7}&appid={apiKey}&units=metric";
        var response = await client.GetAsync(apiUrl);

        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        var forecastData = JsonSerializer.Deserialize<ForecastApiResponse>(json);
        return forecastData;
    }
    
    public async Task<double?> GetUvIndex(double lat, double lon)
    {
        var apiKey = _configuration["OpenWeatherMap:ApiKey"];
        var client = _httpClientFactory.CreateClient();
        var apiUrl =
            $"https://api.openweathermap.org/data/2.5/uvi?lat={lat.ToString("F2", CultureInfo.InvariantCulture)}&lon={lon.ToString("F2", CultureInfo.InvariantCulture)}&appid={apiKey}";
        var response = await client.GetAsync(apiUrl);

        if (!response.IsSuccessStatusCode) return null;
        var jsonString = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(jsonString);
        var uvValue = jsonDoc.RootElement.GetProperty("value").GetDouble();

        return uvValue;
    }
}