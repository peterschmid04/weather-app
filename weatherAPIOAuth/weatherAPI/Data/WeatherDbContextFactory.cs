using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace weatherAPI.Data;

/// <summary>
/// Creates the EF Core design-time database context used by commands such as
/// <c>dotnet ef migrations add</c>. Runtime containers use the same connection
/// string key, while the local fallback only exists for developer tooling.
/// </summary>
public class WeatherDbContextFactory : IDesignTimeDbContextFactory<WeatherDbContext>
{
    /// <summary>
    /// Builds a context with the Npgsql provider so EF Core can inspect the
    /// model and generate PostgreSQL migrations without starting the web API.
    /// </summary>
    public WeatherDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__WeatherDatabase")
            ?? "Host=localhost;Port=5432;Database=weather_app;Username=weather_app;Password=change-me";

        var optionsBuilder = new DbContextOptionsBuilder<WeatherDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new WeatherDbContext(optionsBuilder.Options);
    }
}
