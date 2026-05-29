using Microsoft.EntityFrameworkCore;
using weatherAPI.Models.Database;

namespace weatherAPI.Data;

/// <summary>
/// EF-Core database context for all persistent Weather App data.
/// Auth0 remains the identity provider; this context stores only app data such
/// as profiles, cities, favorites, history, stations, shares and theme choices.
/// </summary>
public class WeatherDbContext(DbContextOptions<WeatherDbContext> options) : DbContext(options)
{
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    public DbSet<City> Cities => Set<City>();

    public DbSet<SearchHistory> SearchHistory => Set<SearchHistory>();

    public DbSet<FavoriteCity> FavoriteCities => Set<FavoriteCity>();

    public DbSet<WeatherStation> WeatherStations => Set<WeatherStation>();

    public DbSet<WeatherStationMeasurement> WeatherStationMeasurements => Set<WeatherStationMeasurement>();

    public DbSet<WeatherStationShare> WeatherStationShares => Set<WeatherStationShare>();

    public DbSet<WeatherRequestLog> WeatherRequestLogs => Set<WeatherRequestLog>();

    public DbSet<UserThemePreference> UserThemePreferences => Set<UserThemePreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // UserProfiles mirror Auth0 users by subject and optional email so app
        // data can be separated per logged-in user without storing passwords.
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Auth0Subject).HasMaxLength(256).IsRequired();
            entity.Property(user => user.DisplayName).HasMaxLength(160);
            entity.Property(user => user.Email).HasMaxLength(320);
            entity.Property(user => user.NormalizedEmail).HasMaxLength(320);
            entity.Property(user => user.CreatedAtUtc).HasDefaultValueSql("now()");
            entity.HasIndex(user => user.Auth0Subject).IsUnique();
            entity.HasIndex(user => user.NormalizedEmail);
        });

        // Cities are centralized and referenced by favorites, history, logs and
        // weather stations to avoid duplicated city records.
        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(city => city.Id);
            entity.Property(city => city.Name).HasMaxLength(160).IsRequired();
            entity.Property(city => city.NormalizedName).HasMaxLength(160).IsRequired();
            entity.Property(city => city.CountryCode).HasMaxLength(2).IsRequired();
            entity.HasIndex(city => new { city.NormalizedName, city.CountryCode }).IsUnique();
        });

        // SearchHistory stores recent searches per user and points to Cities.
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

        // FavoriteCities stores each user's saved cities; IsDefault marks the
        // single city that should load after login.
        modelBuilder.Entity<FavoriteCity>(entity =>
        {
            entity.HasKey(favorite => favorite.Id);
            entity.Property(favorite => favorite.IsDefault).HasDefaultValue(false);
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
            entity.HasIndex(favorite => new { favorite.UserProfileId, favorite.IsDefault });
        });

        // WeatherStations are user-owned custom places/stations with optional
        // coordinates and a normalized City relation.
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

        // Measurements are time series entries attached to one station.
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

        // Shares model invitations and accepted access from one owner to
        // another Auth0 user identified by email/profile.
        modelBuilder.Entity<WeatherStationShare>(entity =>
        {
            entity.HasKey(share => share.Id);
            entity.Property(share => share.SharedWithEmail).HasMaxLength(320).IsRequired();
            entity.Property(share => share.NormalizedSharedWithEmail).HasMaxLength(320).IsRequired();
            entity.Property(share => share.Permission).HasMaxLength(40).IsRequired();
            entity.Property(share => share.Status).HasMaxLength(20).IsRequired();
            entity.Property(share => share.CreatedAtUtc).HasDefaultValueSql("now()");
            entity.HasOne(share => share.WeatherStation)
                .WithMany(station => station.Shares)
                .HasForeignKey(share => share.WeatherStationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(share => share.Owner)
                .WithMany(user => user.OwnedWeatherStationShares)
                .HasForeignKey(share => share.OwnerUserProfileId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(share => share.SharedWithUser)
                .WithMany(user => user.ReceivedWeatherStationShares)
                .HasForeignKey(share => share.SharedWithUserProfileId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(share => new { share.WeatherStationId, share.NormalizedSharedWithEmail }).IsUnique();
            entity.HasIndex(share => share.OwnerUserProfileId);
            entity.HasIndex(share => share.SharedWithUserProfileId);
            entity.HasIndex(share => share.NormalizedSharedWithEmail);
        });

        // Request logs record weather lookup attempts for debugging/audit.
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

        // One theme preference per user controls the saved UI color theme.
        modelBuilder.Entity<UserThemePreference>(entity =>
        {
            entity.HasKey(preference => preference.Id);
            entity.Property(preference => preference.ThemeName).HasMaxLength(40).IsRequired();
            entity.Property(preference => preference.UpdatedAtUtc).HasDefaultValueSql("now()");
            entity.HasOne(preference => preference.User)
                .WithOne(user => user.ThemePreference)
                .HasForeignKey<UserThemePreference>(preference => preference.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(preference => preference.UserProfileId).IsUnique();
        });
    }
}
