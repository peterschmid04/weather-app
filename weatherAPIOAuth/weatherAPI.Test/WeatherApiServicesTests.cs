using FluentAssertions;

using Moq;

using weatherAPI.Models.External;
using weatherAPI.Services;
using weatherAPI.Services.Interfaces;

namespace weatherAPI.Test;

[TestClass]
public class WeatherApiServicesTests
{
    private const double validLat = 48.77;
    private const double validLon = 9.18;
    private static Mock<IOpenWeatherApiService> _apiMock = null!;
    private static AirQualityService _airQualityService = null!;
    private static UvService _uvService = null!;
    private static ForecastService _forecastService = null!;
    private static WeatherService _weatherService = null!;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        _apiMock = new Mock<IOpenWeatherApiService>();

        _apiMock
            .Setup(api => api.GetAirQuality(validLat, validLon))
            .ReturnsAsync(2.0);

        _apiMock.Setup(api => api.GetUvIndex(validLat, validLon)).ReturnsAsync(6.78);

        _airQualityService = new AirQualityService(_apiMock.Object);
        _uvService = new UvService(_apiMock.Object);

        var expectedWeather = GetOpenWeatherResponseTestData();
        _apiMock.Setup(api => api.GetWeather("London")).ReturnsAsync(expectedWeather);
        _weatherService = new WeatherService(_apiMock.Object);

        const string expectedDescription = "overcast clouds";
        const double expectedTempMin = 11.48;
        const double expectedTempMax = 24.49;
        var expectedForecast = GetForecastApiResponseTestData(expectedTempMin, expectedTempMax, expectedDescription);
        _apiMock.Setup(api => api.GetForecast(validLat, validLon)).ReturnsAsync(expectedForecast);
        _forecastService = new ForecastService(_apiMock.Object);
    }

    [TestInitialize]
    public void TestInit()
    {
        _apiMock.Invocations.Clear();
    }

    [TestMethod]
    public async Task AirQualityServiceTest()
    {
        var result = await _airQualityService.GetAirQuality(validLat, validLon);

        result.Should().NotBeNull();
        result.Aqi.Should().Be(2.0);

        _apiMock.Verify(api => api.GetAirQuality(validLat, validLon), Times.Once);
        _apiMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task AirQualityService_ReturnsNull_ForInvalidCoordinates()
    {
        var result = await _airQualityService.GetAirQuality(999, 999);

        result.Should().BeNull();
        _apiMock.Verify(api => api.GetAirQuality(999, 999), Times.Once);
        _apiMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task UvService_Returns_Value()
    {
        var result = await _uvService.GetUvIndex(validLat, validLon);

        result.Should().NotBeNull();
        result.UvIndex.Should().Be(6.78);

        _apiMock.Verify(api => api.GetUvIndex(validLat, validLon), Times.Once);
        _apiMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task ForecastServiceTest()
    {
        var result = await _forecastService.GetForecast(validLat, validLon);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);


        result[0].Day.Should().Be("Freitag");
        result[0].Description.Should().Be("overcast clouds");
        result[0].Icon.Should().Be("04d");
        result[0].Id.Should().Be(804);
        result[0].TempMin.Should().Be(11.5);
        result[0].TempMax.Should().Be(24.5);

        _apiMock.Verify(api => api.GetForecast(validLat, validLon), Times.Once);
        _apiMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task WeatherServiceTest()
    {
        const string city = "London";

        var result = await _weatherService.GetWeather(city);

        result.Should().NotBeNull();
        result.City.Should().Be("London");
        result.Humidity.Should().Be(70);
        result.WeatherId.Should().Be(804);
        result.Description.Should().Be("overcast clouds");

        _apiMock.Verify(api => api.GetWeather(city), Times.Once);
        _apiMock.VerifyNoOtherCalls();
    }
    
    private static ForecastApiResponse GetForecastApiResponseTestData(double expectedTempMin, double expectedTempMax,
        string expectedDescription)
    {
        var expectedForecast = new ForecastApiResponse
        {
            City = new ForecastCity { Timezone = 0 },
            List =
            [
                new ForecastDay
                {
                    Dt = new DateTimeOffset(2025, 8, 21, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds(),
                    Main = new ForecastMain { Temp = 15.0, TempMin = 10.0, TempMax = 20.0 },
                    Weather = [new WeatherInfo { Id = 800, Description = "clear sky", Icon = "01d" }]
                },

                new ForecastDay
                {
                    Dt = new DateTimeOffset(2025, 8, 22, 12, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds(),
                    Main = new ForecastMain
                    {
                        Temp = 18.0,
                        TempMin = expectedTempMin,
                        TempMax = expectedTempMax
                    },
                    Weather =
                    [
                        new WeatherInfo
                        {
                            Id = 804,
                            Description = expectedDescription,
                            Icon = "04d"
                        }
                    ]
                }
            ]
        };
        return expectedForecast;
    }

    private static OpenWeatherResponse GetOpenWeatherResponseTestData()
    {
        var expectedWeather = new OpenWeatherResponse
        {
            Coord = new Coord { Lat = 51.5085, Lon = -0.1257 },
            Name = "London",
            Main = new Main { Humidity = 70 },
            Weather = [new WeatherInfo { Id = 804, Description = "overcast clouds", Icon = "04d" }],
            Wind = new Wind { Speed = 4.12 },
            Timezone = 3600,
            Sys = new Sys { Country = "GB", Sunrise = 1691374800, Sunset = 1691427600 },
            VisibilityMeters = 10000
        };
        return expectedWeather;
    }
}
