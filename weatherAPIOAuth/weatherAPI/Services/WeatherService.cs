using weatherAPI.Services.Interfaces;
using weatherAPI.Models.Dto;

namespace weatherAPI.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly IOpenWeatherApiService _weatherApiClient;
        
        public WeatherService(IOpenWeatherApiService weatherApiClient)
        {
            _weatherApiClient = weatherApiClient;
        }
        
        public async Task<Weather?> GetWeather(string city)
        {
            var openWeather = await _weatherApiClient.GetWeather(city);
            if (openWeather == null) return null;
            
            return new Weather
            {   
                Lat = openWeather.Coord.Lat,
                Lon = openWeather.Coord.Lon,
                City = openWeather.Name,
                Country = openWeather.Sys.Country,
                TemperatureC = Math.Round(openWeather.Main.Temp - 273.15, 1),
                Humidity = openWeather.Main.Humidity,
                VisibilityKm = openWeather.VisibilityMeters / 1000.0,
                Description = openWeather.Weather[0].Description,
                Icon = openWeather.Weather[0].Icon,
                WeatherId = openWeather.Weather[0].Id,
                WindSpeed = openWeather.Wind.Speed,
                Sunrise = DateTimeOffset.FromUnixTimeSeconds(openWeather.Sys.Sunrise)
                    .ToOffset(TimeSpan.FromSeconds(openWeather.Timezone))
                    .ToString("HH:mm"),

                Sunset = DateTimeOffset.FromUnixTimeSeconds(openWeather.Sys.Sunset)
                    .ToOffset(TimeSpan.FromSeconds(openWeather.Timezone))
                    .ToString("HH:mm"),
                TimezoneOffsetHours = openWeather.Timezone / 3600,
            };
        }
    }
}
