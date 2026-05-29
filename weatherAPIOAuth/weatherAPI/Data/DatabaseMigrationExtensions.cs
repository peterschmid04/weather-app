using Microsoft.EntityFrameworkCore;

namespace weatherAPI.Data;

/// <summary>
/// Startup helper that applies pending EF-Core migrations when the backend
/// container starts. This keeps a fresh PostgreSQL volume usable with a single
/// docker compose up command.
/// </summary>
public static class DatabaseMigrationExtensions
{
    /// <summary>
    /// Creates a scoped WeatherDbContext and runs Database.MigrateAsync().
    /// </summary>
    public static async Task ApplyDatabaseMigrationsAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
