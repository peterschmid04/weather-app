using Microsoft.OpenApi.Models; 
using Microsoft.AspNetCore.Mvc;
using weatherAPI.Services;
using weatherAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using weatherAPI.Security;

var builder = WebApplication.CreateBuilder(args);   
// Add services
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

app.Run();  