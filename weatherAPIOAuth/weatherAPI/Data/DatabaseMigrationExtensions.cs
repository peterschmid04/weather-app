using Microsoft.EntityFrameworkCore;

namespace weatherAPI.Data;

public static class DatabaseMigrationExtensions
{
    public static async Task ApplyDatabaseMigrationsAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
