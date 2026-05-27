using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace weatherAPI.Data;

public class WeatherDbContextFactory : IDesignTimeDbContextFactory<WeatherDbContext>
{
    public WeatherDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__WeatherDatabase")
            ?? "Host=localhost;Port=5432;Database=weather_app;Username=weather_app;Password=change-me";

        var optionsBuilder = new DbContextOptionsBuilder<WeatherDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new WeatherDbContext(optionsBuilder.Options);
    }
}
