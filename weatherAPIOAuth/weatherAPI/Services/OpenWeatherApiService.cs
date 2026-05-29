using System.Text.Json;
using System.Globalization;
using System.Text;
using weatherAPI.Models.External;
using weatherAPI.Services.Interfaces;


namespace weatherAPI.Services;

public class OpenWeatherApiService : IOpenWeatherApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private static readonly Dictionary<string, string> WeatherQueryAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["freiburg"] = "Freiburg,DE",
        ["freiburg im breisgau"] = "Freiburg,DE",
        ["freiburg breisgau"] = "Freiburg,DE",
        ["frankfurt"] = "Frankfurt am Main,DE",
        ["frankfurt am main"] = "Frankfurt am Main,DE",
        ["frankfurt oder"] = "Frankfurt (Oder),DE",
        ["frankfurt (oder)"] = "Frankfurt (Oder),DE",
        ["munchen"] = "Munich,DE",
        ["muenchen"] = "Munich,DE",
        ["munich"] = "Munich,DE",
        ["peking"] = "Beijing,CN",
        ["wien"] = "Vienna,AT",
        ["vienna"] = "Vienna,AT",
    };
    
    public OpenWeatherApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }
    
    public async Task<OpenWeatherResponse?> GetWeather(string city)
    {
        var apiKey = GetRequiredApiKey();
        var client = _httpClientFactory.CreateClient();  
        var weatherQuery = BuildWeatherQuery(city);
        var apiUrl = $"https://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeDataString(weatherQuery)}&appid={Uri.EscapeDataString(apiKey)}";
        var response = await client.GetAsync(apiUrl);
    
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OpenWeatherResponse>(json);
    }

    private static string BuildWeatherQuery(string city)
    {
        var normalizedCity = NormalizeWhitespace(city);
        var lookupKey = NormalizeLookupKey(normalizedCity);

        if (WeatherQueryAliases.TryGetValue(lookupKey, out var aliasedQuery))
        {
            return aliasedQuery;
        }

        return normalizedCity;
    }

    private static string NormalizeWhitespace(string value)
    {
        return string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string NormalizeLookupKey(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }
    
    public async Task<double?> GetAirQuality(double lat, double lon)
    {
        var apiKey = GetRequiredApiKey();
        var client = _httpClientFactory.CreateClient();
        var apiUrl =
            $"https://api.openweathermap.org/data/2.5/air_pollution?lat={lat.ToString("F2", CultureInfo.InvariantCulture)}&lon={lon.ToString("F2", CultureInfo.InvariantCulture)}&appid={Uri.EscapeDataString(apiKey)}&units=metric";
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
        var apiKey = GetRequiredApiKey();
        var client = _httpClientFactory.CreateClient();
        var apiUrl =
            $"https://api.openweathermap.org/data/2.5/forecast?lat={lat.ToString("F2", CultureInfo.InvariantCulture)}&lon={lon.ToString("F2", CultureInfo.InvariantCulture)}&appid={Uri.EscapeDataString(apiKey)}&units=metric";
        var response = await client.GetAsync(apiUrl);

        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        var forecastData = JsonSerializer.Deserialize<ForecastApiResponse>(json);
        return forecastData;
    }
    
    public async Task<double?> GetUvIndex(double lat, double lon)
    {
        var apiKey = GetRequiredApiKey();
        var client = _httpClientFactory.CreateClient();
        var apiUrl =
            $"https://api.openweathermap.org/data/3.0/onecall?lat={lat.ToString("F2", CultureInfo.InvariantCulture)}&lon={lon.ToString("F2", CultureInfo.InvariantCulture)}&exclude=minutely,hourly,daily,alerts&appid={Uri.EscapeDataString(apiKey)}&units=metric";
        var response = await client.GetAsync(apiUrl);

        if (!response.IsSuccessStatusCode) return null;
        var jsonString = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(jsonString);
        var uvValue = jsonDoc.RootElement.GetProperty("current").GetProperty("uvi").GetDouble();

        return uvValue;
    }

    private string GetRequiredApiKey()
    {
        var apiKey = _configuration["OpenWeatherMap:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("OPENWEATHERMAP_API_KEY is missing. Add it to the local .env file before requesting weather data.");
        }

        return apiKey.Trim();
    }
}
