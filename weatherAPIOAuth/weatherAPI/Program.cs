using Microsoft.OpenApi.Models; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using weatherAPI.Services;
using weatherAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.RateLimiting;
using weatherAPI.Data;
using weatherAPI.Models.Database;
using weatherAPI.Models.Dto;
using weatherAPI.Security;
using weatherAPI.Logging;

var builder = WebApplication.CreateBuilder(args);   
static bool IsTrue(string? value) => string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);

var detailedLogsEnabled = IsTrue(builder.Configuration["LOGS"]) || IsTrue(builder.Configuration["ENABLE_DETAILED_LOGS"]);
if (detailedLogsEnabled)
{
    var logDirectory = builder.Configuration["LOG_DIRECTORY"];
    if (string.IsNullOrWhiteSpace(logDirectory))
    {
        logDirectory = Path.Combine(builder.Environment.ContentRootPath, "logs");
    }

    var logFile = Path.Combine(logDirectory, $"weather-api-{DateTime.UtcNow:yyyyMMdd-HHmmss}.log");
    builder.Logging.SetMinimumLevel(LogLevel.Trace);
    builder.Logging.AddProvider(new DetailedFileLoggerProvider(logFile));
}

// Add services
var databaseConnectionString = builder.Configuration.GetConnectionString("WeatherDatabase")
    ?? throw new InvalidOperationException("Connection string 'WeatherDatabase' is missing. Configure ConnectionStrings__WeatherDatabase.");

var openWeatherApiKey = builder.Configuration["OpenWeatherMap:ApiKey"];
if (string.IsNullOrWhiteSpace(openWeatherApiKey) ||
    openWeatherApiKey.Contains("your-openweathermap", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("OPENWEATHERMAP_API_KEY is missing. Add a real OpenWeatherMap API key to the local .env file before starting the backend.");
}

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

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
        PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                GetRateLimitPartitionKey(context),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                })),
        PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                GetRateLimitPartitionKey(context),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 1000,
                    Window = TimeSpan.FromHours(1),
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                })));
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
app.UseRateLimiter();
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

app.MapGet("/weather", async (
    HttpContext http,
    string? city,
    [FromServices] IWeatherService weatherService,
    [FromServices] WeatherDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(http, db);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    if (string.IsNullOrWhiteSpace(city) || city.Trim().Length > 160)
    {
        return Results.BadRequest(new { message = "City name is required and must be 160 characters or less." });
    }

    var queryText = city.Trim();
    var resultWeather = await weatherService.GetWeather(queryText);
    if (resultWeather is null)
    {
        await AddWeatherRequestLogAsync(db, user.Id, null, http.Request.Method, "/weather", queryText, StatusCodes.Status404NotFound, false, "City not found.");
        return Results.NotFound(new { message = "City not found." });
    }

    if (!RegionAuthorization.IsCountryAllowed(http.User, resultWeather.Country!)) 
    {
        await AddWeatherRequestLogAsync(db, user.Id, null, http.Request.Method, "/weather", queryText, StatusCodes.Status403Forbidden, false, "Region not allowed.");
        return Results.StatusCode(StatusCodes.Status403Forbidden);
    }

    var cityEntity = await FindOrCreateCityAsync(
        db,
        resultWeather.City,
        resultWeather.Country,
        resultWeather.Lat,
        resultWeather.Lon);

    db.SearchHistory.Add(new SearchHistory
    {
        Id = Guid.NewGuid(),
        UserProfileId = user.Id,
        City = cityEntity,
        QueryText = queryText
    });
    db.WeatherRequestLogs.Add(new WeatherRequestLog
    {
        Id = Guid.NewGuid(),
        UserProfileId = user.Id,
        City = cityEntity,
        HttpMethod = http.Request.Method,
        Endpoint = "/weather",
        QueryText = queryText,
        StatusCode = StatusCodes.Status200OK,
        WasSuccessful = true
    });
    await SaveChangesAndTrimSearchHistoryAsync(db, user.Id);

    return Results.Ok(resultWeather);
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
            user.Email));
});

app.MapGet("/access", async (HttpContext http, [FromServices] WeatherDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(http, db);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new AccessOverviewResponse(
        user.Id,
        user.DisplayName,
        user.Email,
        RegionAuthorization.GetRegionScope(http.User),
        RegionAuthorization.GetAllowedRegionLabels(http.User),
        RegionAuthorization.GetPermissions(http.User),
        GetAuth0Roles(http.User)));
});

var history = app.MapGroup("/history");

history.MapGet("/", async (HttpContext http, [FromServices] WeatherDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(http, db);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    await TrimSearchHistoryAsync(db, user.Id);

    var result = await db.SearchHistory
        .AsNoTracking()
        .Where(item => item.UserProfileId == user.Id)
        .OrderByDescending(item => item.SearchedAtUtc)
        .Take(3)
        .Select(item => new SearchHistoryResponse(
            item.Id,
            item.QueryText,
            item.City.Name,
            item.City.CountryCode,
            item.City.Latitude,
            item.City.Longitude,
            item.SearchedAtUtc))
        .ToListAsync();

    return Results.Ok(result);
});

history.MapPost("/", async (
    HttpContext http,
    [FromBody] CityRequest request,
    [FromServices] WeatherDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(http, db);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    var validationError = ValidateCityRequest(request);
    if (validationError is not null)
    {
        return Results.BadRequest(new { message = validationError });
    }

    var city = await FindOrCreateCityAsync(db, request.CityName, request.CountryCode, request.Latitude, request.Longitude);
    var item = new SearchHistory
    {
        Id = Guid.NewGuid(),
        UserProfileId = user.Id,
        City = city,
        QueryText = request.CityName.Trim()
    };

    db.SearchHistory.Add(item);
    await SaveChangesAndTrimSearchHistoryAsync(db, user.Id);

    return Results.Created($"/history/{item.Id}", new SearchHistoryResponse(
        item.Id,
        item.QueryText,
        city.Name,
        city.CountryCode,
        city.Latitude,
        city.Longitude,
        item.SearchedAtUtc));
});

history.MapDelete("/{historyId:guid}", async (
    HttpContext http,
    Guid historyId,
    [FromServices] WeatherDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(http, db);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    var item = await db.SearchHistory
        .SingleOrDefaultAsync(existing => existing.Id == historyId && existing.UserProfileId == user.Id);

    if (item is null)
    {
        return Results.NotFound(new { message = "History item not found." });
    }

    db.SearchHistory.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

var favorites = app.MapGroup("/favorites");

favorites.MapGet("/", async (HttpContext http, [FromServices] WeatherDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(http, db);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    var result = await db.FavoriteCities
        .AsNoTracking()
        .Where(favorite => favorite.UserProfileId == user.Id)
        .OrderBy(favorite => favorite.City.Name)
        .Select(favorite => new FavoriteCityResponse(
            favorite.Id,
            favorite.City.Name,
            favorite.City.CountryCode,
            favorite.City.Latitude,
            favorite.City.Longitude,
            favorite.CreatedAtUtc))
        .ToListAsync();

    return Results.Ok(result);
});

favorites.MapPost("/", async (
    HttpContext http,
    [FromBody] CityRequest request,
    [FromServices] WeatherDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(http, db);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    var validationError = ValidateCityRequest(request);
    if (validationError is not null)
    {
        return Results.BadRequest(new { message = validationError });
    }

    var city = await FindOrCreateCityAsync(db, request.CityName, request.CountryCode, request.Latitude, request.Longitude);
    var duplicateExists = await db.FavoriteCities
        .AnyAsync(favorite => favorite.UserProfileId == user.Id && favorite.CityId == city.Id);

    if (duplicateExists)
    {
        return Results.Conflict(new { message = "City is already in favorites." });
    }

    var favorite = new FavoriteCity
    {
        Id = Guid.NewGuid(),
        UserProfileId = user.Id,
        City = city
    };

    db.FavoriteCities.Add(favorite);
    await db.SaveChangesAsync();

    return Results.Created($"/favorites/{favorite.Id}", new FavoriteCityResponse(
        favorite.Id,
        city.Name,
        city.CountryCode,
        city.Latitude,
        city.Longitude,
        favorite.CreatedAtUtc));
});

favorites.MapPut("/{favoriteId:guid}", async (
    HttpContext http,
    Guid favoriteId,
    [FromBody] CityRequest request,
    [FromServices] WeatherDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(http, db);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    var validationError = ValidateCityRequest(request);
    if (validationError is not null)
    {
        return Results.BadRequest(new { message = validationError });
    }

    var favorite = await db.FavoriteCities
        .SingleOrDefaultAsync(existing => existing.Id == favoriteId && existing.UserProfileId == user.Id);

    if (favorite is null)
    {
        return Results.NotFound(new { message = "Favorite not found." });
    }

    var city = await FindOrCreateCityAsync(db, request.CityName, request.CountryCode, request.Latitude, request.Longitude);
    var duplicateExists = await db.FavoriteCities
        .AnyAsync(existing =>
            existing.Id != favorite.Id &&
            existing.UserProfileId == user.Id &&
            existing.CityId == city.Id);

    if (duplicateExists)
    {
        return Results.Conflict(new { message = "City is already in favorites." });
    }

    favorite.City = city;
    await db.SaveChangesAsync();

    return Results.Ok(new FavoriteCityResponse(
        favorite.Id,
        city.Name,
        city.CountryCode,
        city.Latitude,
        city.Longitude,
        favorite.CreatedAtUtc));
});

favorites.MapDelete("/{favoriteId:guid}", async (
    HttpContext http,
    Guid favoriteId,
    [FromServices] WeatherDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(http, db);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    var favorite = await db.FavoriteCities
        .SingleOrDefaultAsync(existing => existing.Id == favoriteId && existing.UserProfileId == user.Id);

    if (favorite is null)
    {
        return Results.NotFound(new { message = "Favorite not found." });
    }

    db.FavoriteCities.Remove(favorite);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

var theme = app.MapGroup("/theme");

theme.MapGet("/", async (HttpContext http, [FromServices] WeatherDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(http, db);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    var themeName = await db.UserThemePreferences
        .AsNoTracking()
        .Where(preference => preference.UserProfileId == user.Id)
        .Select(preference => preference.ThemeName)
        .SingleOrDefaultAsync() ?? "graphite";

    return Results.Ok(new ThemePreferenceResponse(themeName));
});

theme.MapPut("/", async (
    HttpContext http,
    [FromBody] UpdateThemePreferenceRequest request,
    [FromServices] WeatherDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(http, db);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    var themeName = NormalizeThemeName(request.ThemeName);
    if (themeName is null)
    {
        return Results.BadRequest(new { message = "Unknown theme." });
    }

    var preference = await db.UserThemePreferences
        .SingleOrDefaultAsync(existing => existing.UserProfileId == user.Id);

    if (preference is null)
    {
        preference = new UserThemePreference
        {
            Id = Guid.NewGuid(),
            UserProfileId = user.Id,
            ThemeName = themeName
        };
        db.UserThemePreferences.Add(preference);
    }
    else
    {
        preference.ThemeName = themeName;
        preference.UpdatedAtUtc = DateTime.UtcNow;
    }

    await db.SaveChangesAsync();
    return Results.Ok(new ThemePreferenceResponse(themeName));
});

var stations = app.MapGroup("/stations");

stations.MapGet("/", async (HttpContext http, [FromServices] WeatherDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(http, db);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    var normalizedEmail = user.NormalizedEmail;

    var result = await db.WeatherStations
        .AsNoTracking()
        .Where(station =>
            station.UserProfileId == user.Id ||
            station.Shares.Any(share =>
                share.Status == "accepted" &&
                (share.SharedWithUserProfileId == user.Id ||
                 (normalizedEmail != null && share.NormalizedSharedWithEmail == normalizedEmail))))
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
            station.UserProfileId == user.Id,
            station.UserProfileId == user.Id
                ? "owner"
                : station.Shares
                    .Where(share =>
                        share.Status == "accepted" &&
                        (share.SharedWithUserProfileId == user.Id ||
                         (normalizedEmail != null && share.NormalizedSharedWithEmail == normalizedEmail)))
                    .Select(share => share.Permission)
                    .FirstOrDefault() ?? "read",
            station.User.DisplayName ?? station.User.Email,
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

    var validationError = ValidateCreateStationRequest(request);
    if (validationError is not null)
    {
        return Results.BadRequest(new { message = validationError });
    }

    var stationName = request.Name.Trim();
    var cityName = string.IsNullOrWhiteSpace(request.CityName) ? stationName : request.CityName.Trim();
    var countryCode = NormalizeCountryCode(request.CountryCode);
    var normalizedCityName = NormalizeCityName(cityName);

    var duplicateExists = await db.WeatherStations
        .AnyAsync(station => station.UserProfileId == user.Id && station.Name == stationName);

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
        UserProfileId = user.Id,
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
        true,
        "owner",
        user.DisplayName ?? user.Email,
        null));
});

stations.MapPut("/{stationId:guid}", async (
    HttpContext http,
    Guid stationId,
    [FromBody] UpdateWeatherStationRequest request,
    [FromServices] WeatherDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(http, db);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    var validationError = ValidateUpdateStationRequest(request);
    if (validationError is not null)
    {
        return Results.BadRequest(new { message = validationError });
    }

    var station = await db.WeatherStations
        .Include(existing => existing.City)
        .SingleOrDefaultAsync(existing => existing.Id == stationId && existing.UserProfileId == user.Id);

    if (station is null)
    {
        return Results.NotFound(new { message = "Station not found." });
    }

    var stationName = request.Name.Trim();
    var cityName = string.IsNullOrWhiteSpace(request.CityName) ? stationName : request.CityName.Trim();
    var countryCode = NormalizeCountryCode(request.CountryCode);
    var normalizedCityName = NormalizeCityName(cityName);

    var duplicateExists = await db.WeatherStations
        .AnyAsync(existing =>
            existing.Id != station.Id &&
            existing.UserProfileId == user.Id &&
            existing.Name == stationName);

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

    station.Name = stationName;
    station.City = city;
    station.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
    station.Latitude = request.Latitude;
    station.Longitude = request.Longitude;

    await db.SaveChangesAsync();

    return Results.Ok(new WeatherStationResponse(
        station.Id,
        station.Name,
        city.Name,
        city.CountryCode,
        station.Description,
        station.Latitude,
        station.Longitude,
        station.CreatedAtUtc,
        true,
        "owner",
        user.DisplayName ?? user.Email,
        null));
});

stations.MapDelete("/{stationId:guid}", async (
    HttpContext http,
    Guid stationId,
    [FromServices] WeatherDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(http, db);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    var station = await db.WeatherStations
        .SingleOrDefaultAsync(existing => existing.Id == stationId && existing.UserProfileId == user.Id);

    if (station is null)
    {
        return Results.NotFound(new { message = "Station not found." });
    }

    db.WeatherStations.Remove(station);
    await db.SaveChangesAsync();
    return Results.NoContent();
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

    if (!await CanReadStationAsync(db, stationId, user))
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

    if (!await CanReadStationAsync(db, stationId, user))
    {
        return Results.NotFound(new { message = "Station not found." });
    }

    if (!await CanWriteStationMeasurementsAsync(db, stationId, user))
    {
        return Results.StatusCode(StatusCodes.Status403Forbidden);
    }

    var measurement = new WeatherStationMeasurement
    {
        Id = Guid.NewGuid(),
        WeatherStationId = stationId,
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

    return Results.Created($"/stations/{stationId}/measurements/{measurement.Id}", ToMeasurementResponse(measurement));
});

var stationShares = app.MapGroup("/station-shares");

stationShares.MapGet("/", async (HttpContext http, [FromServices] WeatherDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(http, db);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    var normalizedEmail = user.NormalizedEmail;

    var outgoing = await db.WeatherStationShares
        .AsNoTracking()
        .Where(share => share.OwnerUserProfileId == user.Id)
        .OrderByDescending(share => share.CreatedAtUtc)
        .Select(share => new OutgoingWeatherStationShareResponse(
            share.Id,
            share.WeatherStationId,
            share.WeatherStation.Name,
            share.SharedWithEmail,
            share.Permission,
            share.Status,
            share.CreatedAtUtc,
            share.AcceptedAtUtc))
        .ToListAsync();

    var incoming = await db.WeatherStationShares
        .AsNoTracking()
        .Where(share =>
            share.OwnerUserProfileId != user.Id &&
            (share.SharedWithUserProfileId == user.Id ||
             (normalizedEmail != null && share.NormalizedSharedWithEmail == normalizedEmail)))
        .OrderByDescending(share => share.CreatedAtUtc)
        .Select(share => new IncomingWeatherStationShareResponse(
            share.Id,
            share.WeatherStationId,
            share.WeatherStation.Name,
            share.Owner.DisplayName ?? share.Owner.Email ?? "Unbekannter Nutzer",
            share.Owner.Email,
            share.Permission,
            share.Status,
            share.CreatedAtUtc,
            share.AcceptedAtUtc))
        .ToListAsync();

    return Results.Ok(new WeatherStationShareOverviewResponse(outgoing, incoming));
});

stationShares.MapPost("/", async (
    HttpContext http,
    [FromBody] CreateWeatherStationShareRequest request,
    [FromServices] WeatherDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(http, db);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    var validationError = ValidateShareRequest(request);
    if (validationError is not null)
    {
        return Results.BadRequest(new { message = validationError });
    }

    var normalizedEmail = NormalizeEmail(request.Email);
    if (string.Equals(user.NormalizedEmail, normalizedEmail, StringComparison.OrdinalIgnoreCase))
    {
        return Results.BadRequest(new { message = "You cannot share a station with yourself." });
    }

    var station = await db.WeatherStations
        .SingleOrDefaultAsync(existing => existing.Id == request.WeatherStationId && existing.UserProfileId == user.Id);

    if (station is null)
    {
        return Results.NotFound(new { message = "Station not found." });
    }

    var duplicateExists = await db.WeatherStationShares
        .AnyAsync(share =>
            share.WeatherStationId == station.Id &&
            share.NormalizedSharedWithEmail == normalizedEmail);

    if (duplicateExists)
    {
        return Results.Conflict(new { message = "Station is already shared with this email." });
    }

    var targetUser = await db.UserProfiles.SingleOrDefaultAsync(existing => existing.NormalizedEmail == normalizedEmail);
    var permission = NormalizeSharePermission(request.Permission) ?? "write_measurements";
    var share = new WeatherStationShare
    {
        Id = Guid.NewGuid(),
        WeatherStationId = station.Id,
        OwnerUserProfileId = user.Id,
        SharedWithUserProfileId = targetUser?.Id,
        SharedWithEmail = request.Email.Trim(),
        NormalizedSharedWithEmail = normalizedEmail,
        Permission = permission,
        Status = "pending"
    };

    db.WeatherStationShares.Add(share);
    await db.SaveChangesAsync();

    return Results.Created($"/station-shares/{share.Id}", new OutgoingWeatherStationShareResponse(
        share.Id,
        station.Id,
        station.Name,
        share.SharedWithEmail,
        share.Permission,
        share.Status,
        share.CreatedAtUtc,
        share.AcceptedAtUtc));
});

stationShares.MapPost("/{shareId:guid}/accept", async (
    HttpContext http,
    Guid shareId,
    [FromServices] WeatherDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(http, db);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    if (string.IsNullOrWhiteSpace(user.NormalizedEmail))
    {
        return Results.BadRequest(new { message = "Your Auth0 profile needs an email address to accept shares." });
    }

    var share = await db.WeatherStationShares
        .Include(existing => existing.WeatherStation)
        .Include(existing => existing.Owner)
        .SingleOrDefaultAsync(existing =>
            existing.Id == shareId &&
            existing.OwnerUserProfileId != user.Id &&
            (existing.SharedWithUserProfileId == user.Id ||
             existing.NormalizedSharedWithEmail == user.NormalizedEmail));

    if (share is null)
    {
        return Results.NotFound(new { message = "Share not found." });
    }

    share.SharedWithUserProfileId = user.Id;
    share.Status = "accepted";
    share.AcceptedAtUtc ??= DateTime.UtcNow;
    await db.SaveChangesAsync();

    return Results.Ok(new IncomingWeatherStationShareResponse(
        share.Id,
        share.WeatherStationId,
        share.WeatherStation.Name,
        share.Owner.DisplayName ?? share.Owner.Email ?? "Unbekannter Nutzer",
        share.Owner.Email,
        share.Permission,
        share.Status,
        share.CreatedAtUtc,
        share.AcceptedAtUtc));
});

stationShares.MapDelete("/{shareId:guid}", async (
    HttpContext http,
    Guid shareId,
    [FromServices] WeatherDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(http, db);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    var normalizedEmail = user.NormalizedEmail;
    var share = await db.WeatherStationShares
        .SingleOrDefaultAsync(existing =>
            existing.Id == shareId &&
            (existing.OwnerUserProfileId == user.Id ||
             existing.SharedWithUserProfileId == user.Id ||
             (normalizedEmail != null && existing.NormalizedSharedWithEmail == normalizedEmail)));

    if (share is null)
    {
        return Results.NotFound(new { message = "Share not found." });
    }

    db.WeatherStationShares.Remove(share);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();  

static async Task<UserProfile?> GetOrCreateCurrentUserAsync(HttpContext http, WeatherDbContext db)
{
    var subject = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? http.User.FindFirst("sub")?.Value;
    if (string.IsNullOrWhiteSpace(subject))
    {
        return null;
    }

    var user = await db.UserProfiles.SingleOrDefaultAsync(existingUser => existingUser.Auth0Subject == subject);
    var email = GetEmailFromClaims(http);
    var displayName = http.User.FindFirst("name")?.Value ?? email;
    var normalizedEmail = NormalizeEmailOrNull(email);

    if (user is not null)
    {
        var changed = false;
        if (!string.Equals(user.DisplayName, displayName, StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(displayName))
        {
            user.DisplayName = displayName;
            changed = true;
        }

        if (!string.IsNullOrWhiteSpace(email) && !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            user.Email = email.Trim();
            user.NormalizedEmail = normalizedEmail;
            changed = true;
        }

        changed = await LinkPendingSharesToUserAsync(db, user) || changed;

        if (changed)
        {
            await db.SaveChangesAsync();
        }

        return user;
    }

    user = new UserProfile
    {
        Id = Guid.NewGuid(),
        Auth0Subject = subject,
        DisplayName = displayName,
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
        NormalizedEmail = normalizedEmail
    };

    db.UserProfiles.Add(user);
    await LinkPendingSharesToUserAsync(db, user);
    await db.SaveChangesAsync();
    return user;
}

static string? GetEmailFromClaims(HttpContext http)
{
    var principal = http.User;
    var directEmail = principal.FindFirst(ClaimTypes.Email)?.Value ?? principal.FindFirst("email")?.Value;
    if (LooksLikeEmail(directEmail))
    {
        return directEmail!.Trim();
    }

    var namespacedEmail = principal.Claims
        .FirstOrDefault(claim =>
            claim.Type.EndsWith("/email", StringComparison.OrdinalIgnoreCase) ||
            claim.Type.EndsWith(":email", StringComparison.OrdinalIgnoreCase))
        ?.Value
        ?.Trim();

    if (LooksLikeEmail(namespacedEmail))
    {
        return namespacedEmail;
    }

    var usernameEmail =
        principal.FindFirst("preferred_username")?.Value ??
        principal.FindFirst("upn")?.Value ??
        principal.FindFirst("unique_name")?.Value ??
        principal.FindFirst("nickname")?.Value ??
        principal.FindFirst("name")?.Value;

    if (LooksLikeEmail(usernameEmail))
    {
        return usernameEmail!.Trim();
    }

    var environment = http.RequestServices.GetRequiredService<IHostEnvironment>();
    if (environment.IsDevelopment() &&
        http.Request.Headers.TryGetValue("X-Weather-App-Profile-Email", out var profileEmail) &&
        LooksLikeEmail(profileEmail.FirstOrDefault()))
    {
        return profileEmail.FirstOrDefault()!.Trim();
    }

    return null;
}

static async Task<bool> LinkPendingSharesToUserAsync(WeatherDbContext db, UserProfile user)
{
    if (string.IsNullOrWhiteSpace(user.NormalizedEmail))
    {
        return false;
    }

    var pendingShares = await db.WeatherStationShares
        .Where(share =>
            share.SharedWithUserProfileId == null &&
            share.NormalizedSharedWithEmail == user.NormalizedEmail)
        .ToListAsync();

    foreach (var share in pendingShares)
    {
        share.SharedWithUserProfileId = user.Id;
    }

    return pendingShares.Count > 0;
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

static string? ValidateCityRequest(CityRequest request)
{
    if (string.IsNullOrWhiteSpace(request.CityName))
    {
        return "City name is required.";
    }

    if (request.CityName.Trim().Length > 160)
    {
        return "City name must be 160 characters or less.";
    }

    if (!string.IsNullOrWhiteSpace(request.CountryCode) && request.CountryCode.Trim().Length != 2)
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

static string? ValidateCreateStationRequest(CreateWeatherStationRequest request) =>
    ValidateStationFields(request.Name, request.CityName, request.CountryCode, request.Latitude, request.Longitude);

static string? ValidateUpdateStationRequest(UpdateWeatherStationRequest request) =>
    ValidateStationFields(request.Name, request.CityName, request.CountryCode, request.Latitude, request.Longitude);

static string? ValidateStationFields(string name, string? cityName, string? countryCode, double? latitude, double? longitude)
{
    if (string.IsNullOrWhiteSpace(name))
    {
        return "Station name is required.";
    }

    if (name.Trim().Length > 120)
    {
        return "Station name must be 120 characters or less.";
    }

    if (!string.IsNullOrWhiteSpace(cityName) && cityName.Trim().Length > 160)
    {
        return "City name must be 160 characters or less.";
    }

    if (!string.IsNullOrWhiteSpace(countryCode) && countryCode.Trim().Length != 2)
    {
        return "Country code must contain exactly two letters.";
    }

    if (latitude is < -90 or > 90)
    {
        return "Latitude must be between -90 and 90.";
    }

    if (longitude is < -180 or > 180)
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
        return "Windrichtung muss N, NO, O, SO, S, SW, W oder NW sein.";
    }

    if (request.RainfallMm is < 0)
    {
        return "Rainfall cannot be negative.";
    }

    return null;
}

static string? ValidateShareRequest(CreateWeatherStationShareRequest request)
{
    if (request.WeatherStationId == Guid.Empty)
    {
        return "Weather station is required.";
    }

    if (string.IsNullOrWhiteSpace(request.Email))
    {
        return "Email is required.";
    }

    var email = request.Email.Trim();
    if (email.Length > 320 || !email.Contains('@') || email.StartsWith('@') || email.EndsWith('@'))
    {
        return "Email must be a valid email address.";
    }

    if (NormalizeSharePermission(request.Permission) is null)
    {
        return "Permission must be read or write_measurements.";
    }

    return null;
}

static string? NormalizeSharePermission(string? value) =>
    value?.Trim().ToLowerInvariant() switch
    {
        null or "" => "write_measurements",
        "read" => "read",
        "write" => "write_measurements",
        "write_measurements" => "write_measurements",
        _ => null
    };

static async Task<bool> CanReadStationAsync(WeatherDbContext db, Guid stationId, UserProfile user)
{
    var normalizedEmail = user.NormalizedEmail;
    return await db.WeatherStations.AnyAsync(station =>
        station.Id == stationId &&
        (station.UserProfileId == user.Id ||
         station.Shares.Any(share =>
             share.Status == "accepted" &&
             (share.SharedWithUserProfileId == user.Id ||
              (normalizedEmail != null && share.NormalizedSharedWithEmail == normalizedEmail)))));
}

static async Task<bool> CanWriteStationMeasurementsAsync(WeatherDbContext db, Guid stationId, UserProfile user)
{
    var normalizedEmail = user.NormalizedEmail;
    return await db.WeatherStations.AnyAsync(station =>
        station.Id == stationId &&
        (station.UserProfileId == user.Id ||
         station.Shares.Any(share =>
             share.Status == "accepted" &&
             share.Permission == "write_measurements" &&
             (share.SharedWithUserProfileId == user.Id ||
              (normalizedEmail != null && share.NormalizedSharedWithEmail == normalizedEmail)))));
}

static async Task<City> FindOrCreateCityAsync(
    WeatherDbContext db,
    string cityName,
    string? countryCode,
    double? latitude,
    double? longitude)
{
    var name = cityName.Trim();
    var normalizedCityName = NormalizeCityName(name);
    var normalizedCountryCode = NormalizeCountryCode(countryCode);
    var city = await db.Cities.SingleOrDefaultAsync(existingCity =>
        existingCity.NormalizedName == normalizedCityName &&
        existingCity.CountryCode == normalizedCountryCode);

    if (city is not null)
    {
        if (latitude is not null)
        {
            city.Latitude = latitude;
        }

        if (longitude is not null)
        {
            city.Longitude = longitude;
        }

        return city;
    }

    city = new City
    {
        Id = Guid.NewGuid(),
        Name = name,
        NormalizedName = normalizedCityName,
        CountryCode = normalizedCountryCode,
        Latitude = latitude,
        Longitude = longitude
    };
    db.Cities.Add(city);
    return city;
}

static async Task AddWeatherRequestLogAsync(
    WeatherDbContext db,
    Guid? userProfileId,
    Guid? cityId,
    string httpMethod,
    string endpoint,
    string? queryText,
    int statusCode,
    bool wasSuccessful,
    string? errorMessage)
{
    db.WeatherRequestLogs.Add(new WeatherRequestLog
    {
        Id = Guid.NewGuid(),
        UserProfileId = userProfileId,
        CityId = cityId,
        HttpMethod = httpMethod,
        Endpoint = endpoint,
        QueryText = string.IsNullOrWhiteSpace(queryText) ? null : queryText.Trim(),
        StatusCode = statusCode,
        WasSuccessful = wasSuccessful,
        ErrorMessage = errorMessage
    });
    await db.SaveChangesAsync();
}

static async Task SaveChangesAndTrimSearchHistoryAsync(WeatherDbContext db, Guid userProfileId)
{
    await db.SaveChangesAsync();
    await TrimSearchHistoryAsync(db, userProfileId);
}

static async Task TrimSearchHistoryAsync(WeatherDbContext db, Guid userProfileId)
{
    var oldItems = await db.SearchHistory
        .Where(item => item.UserProfileId == userProfileId)
        .OrderByDescending(item => item.SearchedAtUtc)
        .Skip(3)
        .ToListAsync();

    if (oldItems.Count == 0)
    {
        return;
    }

    db.SearchHistory.RemoveRange(oldItems);
    await db.SaveChangesAsync();
}

static string? NormalizeThemeName(string? value)
{
    return value?.Trim().ToLowerInvariant() switch
    {
        "graphite" => "graphite",
        "sky" => "sky",
        "forest" => "forest",
        "sunset" => "sunset",
        _ => null
    };
}

static string GetRateLimitPartitionKey(HttpContext context) =>
    context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
    ?? context.User.FindFirst("sub")?.Value
    ?? context.Connection.RemoteIpAddress?.ToString()
    ?? "anonymous";

static IReadOnlyCollection<string> GetAuth0Roles(ClaimsPrincipal user) =>
    user.Claims
        .Where(claim =>
            claim.Type.Equals("roles", StringComparison.OrdinalIgnoreCase) ||
            claim.Type.EndsWith("/roles", StringComparison.OrdinalIgnoreCase))
        .Select(claim => claim.Value)
        .Where(value => !string.IsNullOrWhiteSpace(value))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(value => value)
        .ToList();

static string NormalizeCountryCode(string? value) =>
    string.IsNullOrWhiteSpace(value) ? "DE" : value.Trim().ToUpperInvariant();

static string NormalizeCityName(string value) => value.Trim().ToUpperInvariant();

static string NormalizeEmail(string value) => value.Trim().ToUpperInvariant();

static string? NormalizeEmailOrNull(string? value) =>
    string.IsNullOrWhiteSpace(value) ? null : NormalizeEmail(value);

static bool LooksLikeEmail(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return false;
    }

    var trimmed = value.Trim();
    var atIndex = trimmed.IndexOf('@');
    return atIndex > 0 && atIndex < trimmed.Length - 1;
}
