using Microsoft.EntityFrameworkCore;
using weatherAPI.Models.Database;

namespace weatherAPI.Data;

public class WeatherDbContext(DbContextOptions<WeatherDbContext> options) : DbContext(options)
{
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    public DbSet<City> Cities => Set<City>();

    public DbSet<SearchHistory> SearchHistory => Set<SearchHistory>();

    public DbSet<FavoriteCity> FavoriteCities => Set<FavoriteCity>();

    public DbSet<WeatherStation> WeatherStations => Set<WeatherStation>();

    public DbSet<WeatherStationMeasurement> WeatherStationMeasurements => Set<WeatherStationMeasurement>();

    public DbSet<WeatherRequestLog> WeatherRequestLogs => Set<WeatherRequestLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Auth0Subject).HasMaxLength(256).IsRequired();
            entity.Property(user => user.DisplayName).HasMaxLength(160);
            entity.Property(user => user.CreatedAtUtc).HasDefaultValueSql("now()");
            entity.HasIndex(user => user.Auth0Subject).IsUnique();
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(city => city.Id);
            entity.Property(city => city.Name).HasMaxLength(160).IsRequired();
            entity.Property(city => city.NormalizedName).HasMaxLength(160).IsRequired();
            entity.Property(city => city.CountryCode).HasMaxLength(2).IsRequired();
            entity.HasIndex(city => new { city.NormalizedName, city.CountryCode }).IsUnique();
        });

        modelBuilder.Entity<SearchHistory>(entity =>
        {
            entity.HasKey(history => history.Id);
            entity.Property(history => history.QueryText).HasMaxLength(160).IsRequired();
            entity.Property(history => history.SearchedAtUtc).HasDefaultValueSql("now()");
            entity.HasOne(history => history.User)
                .WithMany(user => user.SearchHistory)
                .HasForeignKey(history => history.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(history => history.City)
                .WithMany(city => city.SearchHistory)
                .HasForeignKey(history => history.CityId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(history => new { history.UserProfileId, history.SearchedAtUtc });
        });

        modelBuilder.Entity<FavoriteCity>(entity =>
        {
            entity.HasKey(favorite => favorite.Id);
            entity.Property(favorite => favorite.CreatedAtUtc).HasDefaultValueSql("now()");
            entity.HasOne(favorite => favorite.User)
                .WithMany(user => user.FavoriteCities)
                .HasForeignKey(favorite => favorite.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(favorite => favorite.City)
                .WithMany(city => city.FavoriteCities)
                .HasForeignKey(favorite => favorite.CityId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(favorite => new { favorite.UserProfileId, favorite.CityId }).IsUnique();
        });

        modelBuilder.Entity<WeatherStation>(entity =>
        {
            entity.HasKey(station => station.Id);
            entity.Property(station => station.Name).HasMaxLength(120).IsRequired();
            entity.Property(station => station.Description).HasMaxLength(500);
            entity.Property(station => station.CreatedAtUtc).HasDefaultValueSql("now()");
            entity.HasOne(station => station.User)
                .WithMany(user => user.WeatherStations)
                .HasForeignKey(station => station.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(station => station.City)
                .WithMany(city => city.WeatherStations)
                .HasForeignKey(station => station.CityId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(station => new { station.UserProfileId, station.Name }).IsUnique();
        });

        modelBuilder.Entity<WeatherStationMeasurement>(entity =>
        {
            entity.HasKey(measurement => measurement.Id);
            entity.Property(measurement => measurement.MeasuredAtUtc).HasDefaultValueSql("now()");
            entity.Property(measurement => measurement.Notes).HasMaxLength(500);
            entity.HasOne(measurement => measurement.WeatherStation)
                .WithMany(station => station.Measurements)
                .HasForeignKey(measurement => measurement.WeatherStationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(measurement => new { measurement.WeatherStationId, measurement.MeasuredAtUtc });
        });

        modelBuilder.Entity<WeatherRequestLog>(entity =>
        {
            entity.HasKey(log => log.Id);
            entity.Property(log => log.HttpMethod).HasMaxLength(10).IsRequired();
            entity.Property(log => log.Endpoint).HasMaxLength(80).IsRequired();
            entity.Property(log => log.QueryText).HasMaxLength(160);
            entity.Property(log => log.ErrorMessage).HasMaxLength(500);
            entity.Property(log => log.RequestedAtUtc).HasDefaultValueSql("now()");
            entity.HasOne(log => log.User)
                .WithMany(user => user.WeatherRequestLogs)
                .HasForeignKey(log => log.UserProfileId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(log => log.City)
                .WithMany(city => city.WeatherRequestLogs)
                .HasForeignKey(log => log.CityId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(log => log.RequestedAtUtc);
        });
    }
}
