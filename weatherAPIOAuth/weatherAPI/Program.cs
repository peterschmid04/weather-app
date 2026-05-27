using Microsoft.OpenApi.Models; 
using Microsoft.AspNetCore.Mvc;
using weatherAPI.Services;
using weatherAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using weatherAPI.Data;
using weatherAPI.Models.Database;
using weatherAPI.Models.Dto;
using weatherAPI.Security;

var builder = WebApplication.CreateBuilder(args);   
// Add services
var databaseConnectionString = builder.Configuration.GetConnectionString("WeatherDatabase")
    ?? throw new InvalidOperationException("Connection string 'WeatherDatabase' is missing. Configure ConnectionStrings__WeatherDatabase.");

builder.Services.AddDbContext<WeatherDbContext>(options =>
    options.UseNpgsql(databaseConnectionString, npgsqlOptions =>
        npgsqlOptions.EnableRetryOnFailure()));

builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IUvService, UvService>();
builder.Services.AddScoped<IAirQualityService, AirQualityService>();
builder.Services.AddScoped<IForecastService, ForecastService>();
builder.Services.AddScoped<IOpenWeatherApiService, OpenWeatherApiService>();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthorization(options => options.FallbackPolicy = options.DefaultPolicy);
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo {Title = "Weather API", Version = "v1"});
    var domain = builder.Configuration["Auth0:Domain"];
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Description = "Auth0 Authorization Code Flow (PKCE)",
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"https://{domain}/authorize"),
                TokenUrl         = new Uri($"https://{domain}/oauth/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "read:weather", "Read access to the Weather API" }
                }
            }
        }
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            },
            ["read:weather"]
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", p => p
        .WithOrigins("http://localhost:3000", "http://localhost:5122")
        .AllowAnyMethod()
        .AllowAnyHeader());
});

// Auth0 JWT
var domain   = builder.Configuration["Auth0:Domain"];
var audience = builder.Configuration["Auth0:Audience"];
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.Authority = $"https://{domain}/";
        o.Audience  = audience;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer   = true,
            ValidIssuer      = $"https://{domain}/",
            ValidateAudience = true,
            ValidAudience    = audience,
            ValidateLifetime = true,
            ClockSkew        = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();
await app.Services.ApplyDatabaseMigrationsAsync();

app.UseCors("AllowFrontend");
app.UseSwagger();
app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("/swagger/v1/swagger.json", "Weather API V1");
    o.OAuthClientId(builder.Configuration["Auth0:ClientId"]);
    o.OAuthUsePkce();
    o.OAuthScopeSeparator(" ");
    o.OAuthAdditionalQueryStringParams(new() {["audience"] = builder.Configuration["Auth0:Audience"]!});
});
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

var rnd = new Random();
string[] funTxt =
[
    "“Partly cloudy, mostly coding”",
    "fueled by coffee",
    "100% chance of code",
    "naming is a feature",
    "bug or feature?",
    "cache it if you can",
    "High pressure, high uptime",
    "Storm warning: merge conflicts",
    "Sunny with a chance of deploys"
];

app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers.Append("Fun-Text",funTxt[rnd.Next(0, funTxt.Length)]);
        return Task.CompletedTask;
    });
    await next();
});

app.MapGet("/weather", async (HttpContext http, string city, [FromServices] IWeatherService weatherService) =>
{
    var resultWeather = await weatherService.GetWeather(city); 
    if (resultWeather is null)
       return Results.Problem("Weather data not available."); 
    if (!RegionAuthorization.IsCountryAllowed(http.User, resultWeather.Country!)) 
       return Results.StatusCode(StatusCodes.Status403Forbidden); 
    return Results.Json(resultWeather);
});

app.MapGet("/uv", async (double lat, double lon, [FromServices] IUvService uvService) =>
{
    var resultUv = await uvService.GetUvIndex(lat, lon);
    return resultUv is null ? Results.Problem("Failed to retrieve UV index.") : Results.Json(resultUv);
});

app.MapGet("/airquality", async (double lat, double lon, [FromServices] IAirQualityService airQualityService) =>
{
    var resultAqi = await airQualityService.GetAirQuality(lat, lon);
    return resultAqi is null ? Results.Problem("Failed to retrieve air quality data.") : Results.Json(resultAqi);
});

app.MapGet("/forecast", async (double lat, double lon, [FromServices] IForecastService forecastService) =>
{
    var resultForecast = await forecastService.GetForecast(lat, lon);
    return resultForecast != null ? Results.Json(resultForecast) : Results.Problem("Failed to retrieve forecast data.");
});

app.MapGet("/my-profile", async (HttpContext http, [FromServices] WeatherDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(http, db);
    return user is null
        ? Results.Unauthorized()
        : Results.Ok(new UserProfileResponse(
            user.Id,
            user.Auth0Subject,
            http.User.FindFirst("name")?.Value,
            http.User.FindFirst(ClaimTypes.Email)?.Value ?? http.User.FindFirst("email")?.Value));
});

var stations = app.MapGroup("/stations");

stations.MapGet("/", async (HttpContext http, [FromServices] WeatherDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(http, db);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    var result = await db.WeatherStations
        .AsNoTracking()
        .Where(station => station.AppUserId == user.Id)
        .OrderBy(station => station.Name)
        .Select(station => new WeatherStationResponse(
            station.Id,
            station.Name,
            station.City.Name,
            station.City.CountryCode,
            station.Description,
            station.Latitude,
            station.Longitude,
            station.CreatedAtUtc,
            station.Measurements
                .OrderByDescending(measurement => measurement.MeasuredAtUtc)
                .Select(measurement => new WeatherStationMeasurementResponse(
                    measurement.Id,
                    measurement.MeasuredAtUtc,
                    measurement.TemperatureC,
                    measurement.HumidityPercent,
                    measurement.PressureHpa,
                    measurement.WindSpeedKmh,
                    measurement.WindDirectionDegrees,
                    measurement.RainfallMm,
                    measurement.Notes))
                .FirstOrDefault()))
        .ToListAsync();

    return Results.Ok(result);
});

stations.MapPost("/", async (
    HttpContext http,
    [FromBody] CreateWeatherStationRequest request,
    [FromServices] WeatherDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(http, db);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    var validationError = ValidateStationRequest(request);
    if (validationError is not null)
    {
        return Results.BadRequest(new { message = validationError });
    }

    var stationName = request.Name.Trim();
    var cityName = request.CityName.Trim();
    var countryCode = request.CountryCode.Trim().ToUpperInvariant();
    var normalizedCityName = NormalizeCityName(cityName);

    var duplicateExists = await db.WeatherStations
        .AnyAsync(station => station.AppUserId == user.Id && station.Name == stationName);

    if (duplicateExists)
    {
        return Results.Conflict(new { message = "A station with this name already exists." });
    }

    var city = await db.Cities.SingleOrDefaultAsync(existingCity =>
        existingCity.NormalizedName == normalizedCityName &&
        existingCity.CountryCode == countryCode);

    if (city is null)
    {
        city = new City
        {
            Id = Guid.NewGuid(),
            Name = cityName,
            NormalizedName = normalizedCityName,
            CountryCode = countryCode,
            Latitude = request.Latitude,
            Longitude = request.Longitude
        };
        db.Cities.Add(city);
    }

    var station = new WeatherStation
    {
        Id = Guid.NewGuid(),
        AppUserId = user.Id,
        City = city,
        Name = stationName,
        Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
        Latitude = request.Latitude,
        Longitude = request.Longitude
    };

    db.WeatherStations.Add(station);
    await db.SaveChangesAsync();

    return Results.Created($"/stations/{station.Id}", new WeatherStationResponse(
        station.Id,
        station.Name,
        city.Name,
        city.CountryCode,
        station.Description,
        station.Latitude,
        station.Longitude,
        station.CreatedAtUtc,
        null));
});

stations.MapGet("/{stationId:guid}/measurements", async (
    HttpContext http,
    Guid stationId,
    [FromServices] WeatherDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(http, db);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    var stationExists = await db.WeatherStations
        .AnyAsync(station => station.Id == stationId && station.AppUserId == user.Id);

    if (!stationExists)
    {
        return Results.NotFound(new { message = "Station not found." });
    }

    var measurements = await db.WeatherStationMeasurements
        .AsNoTracking()
        .Where(measurement => measurement.WeatherStationId == stationId)
        .OrderByDescending(measurement => measurement.MeasuredAtUtc)
        .Take(50)
        .Select(measurement => new WeatherStationMeasurementResponse(
            measurement.Id,
            measurement.MeasuredAtUtc,
            measurement.TemperatureC,
            measurement.HumidityPercent,
            measurement.PressureHpa,
            measurement.WindSpeedKmh,
            measurement.WindDirectionDegrees,
            measurement.RainfallMm,
            measurement.Notes))
        .ToListAsync();

    return Results.Ok(measurements);
});

stations.MapPost("/{stationId:guid}/measurements", async (
    HttpContext http,
    Guid stationId,
    [FromBody] CreateWeatherStationMeasurementRequest request,
    [FromServices] WeatherDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(http, db);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    var validationError = ValidateMeasurementRequest(request);
    if (validationError is not null)
    {
        return Results.BadRequest(new { message = validationError });
    }

    var station = await db.WeatherStations
        .SingleOrDefaultAsync(existingStation =>
            existingStation.Id == stationId &&
            existingStation.AppUserId == user.Id);

    if (station is null)
    {
        return Results.NotFound(new { message = "Station not found." });
    }

    var measurement = new WeatherStationMeasurement
    {
        Id = Guid.NewGuid(),
        WeatherStationId = station.Id,
        MeasuredAtUtc = request.MeasuredAtUtc ?? DateTime.UtcNow,
        TemperatureC = request.TemperatureC,
        HumidityPercent = request.HumidityPercent,
        PressureHpa = request.PressureHpa,
        WindSpeedKmh = request.WindSpeedKmh,
        WindDirectionDegrees = request.WindDirectionDegrees,
        RainfallMm = request.RainfallMm,
        Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
    };

    db.WeatherStationMeasurements.Add(measurement);
    await db.SaveChangesAsync();

    return Results.Created($"/stations/{station.Id}/measurements/{measurement.Id}", ToMeasurementResponse(measurement));
});

app.Run();  

static async Task<AppUser?> GetOrCreateCurrentUserAsync(HttpContext http, WeatherDbContext db)
{
    var subject = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? http.User.FindFirst("sub")?.Value;
    if (string.IsNullOrWhiteSpace(subject))
    {
        return null;
    }

    var user = await db.AppUsers.SingleOrDefaultAsync(existingUser => existingUser.Auth0Subject == subject);
    if (user is not null)
    {
        return user;
    }

    user = new AppUser
    {
        Id = Guid.NewGuid(),
        Auth0Subject = subject,
        DisplayName = http.User.FindFirst("name")?.Value ?? http.User.FindFirst(ClaimTypes.Email)?.Value
    };

    db.AppUsers.Add(user);
    await db.SaveChangesAsync();
    return user;
}

static WeatherStationMeasurementResponse ToMeasurementResponse(WeatherStationMeasurement measurement) =>
    new(
        measurement.Id,
        measurement.MeasuredAtUtc,
        measurement.TemperatureC,
        measurement.HumidityPercent,
        measurement.PressureHpa,
        measurement.WindSpeedKmh,
        measurement.WindDirectionDegrees,
        measurement.RainfallMm,
        measurement.Notes);

static string? ValidateStationRequest(CreateWeatherStationRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return "Station name is required.";
    }

    if (request.Name.Trim().Length > 120)
    {
        return "Station name must be 120 characters or less.";
    }

    if (string.IsNullOrWhiteSpace(request.CityName))
    {
        return "City name is required.";
    }

    if (string.IsNullOrWhiteSpace(request.CountryCode) || request.CountryCode.Trim().Length != 2)
    {
        return "Country code must contain exactly two letters.";
    }

    if (request.Latitude is < -90 or > 90)
    {
        return "Latitude must be between -90 and 90.";
    }

    if (request.Longitude is < -180 or > 180)
    {
        return "Longitude must be between -180 and 180.";
    }

    return null;
}

static string? ValidateMeasurementRequest(CreateWeatherStationMeasurementRequest request)
{
    if (request.HumidityPercent is < 0 or > 100)
    {
        return "Humidity must be between 0 and 100 percent.";
    }

    if (request.WindDirectionDegrees is < 0 or > 360)
    {
        return "Wind direction must be between 0 and 360 degrees.";
    }

    if (request.RainfallMm is < 0)
    {
        return "Rainfall cannot be negative.";
    }

    return null;
}

static string NormalizeCityName(string value) => value.Trim().ToUpperInvariant();
