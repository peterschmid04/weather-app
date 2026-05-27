using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using weatherAPI.Data;

#nullable disable

namespace weatherAPI.Migrations
{
    [DbContext(typeof(WeatherDbContext))]
    partial class WeatherDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("weatherAPI.Models.Database.AppUser", entity =>
            {
                entity.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid");

                entity.Property<string>("Auth0Subject")
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnType("character varying(256)");

                entity.Property<DateTime>("CreatedAtUtc")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property<string>("DisplayName")
                    .HasMaxLength(160)
                    .HasColumnType("character varying(160)");

                entity.HasKey("Id");
                entity.HasIndex("Auth0Subject").IsUnique();
                entity.ToTable("AppUsers");
            });

            modelBuilder.Entity("weatherAPI.Models.Database.City", entity =>
            {
                entity.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid");

                entity.Property<string>("CountryCode")
                    .IsRequired()
                    .HasMaxLength(2)
                    .HasColumnType("character varying(2)");

                entity.Property<double?>("Latitude")
                    .HasColumnType("double precision");

                entity.Property<double?>("Longitude")
                    .HasColumnType("double precision");

                entity.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(160)
                    .HasColumnType("character varying(160)");

                entity.Property<string>("NormalizedName")
                    .IsRequired()
                    .HasMaxLength(160)
                    .HasColumnType("character varying(160)");

                entity.HasKey("Id");
                entity.HasIndex("NormalizedName", "CountryCode").IsUnique();
                entity.ToTable("Cities");
            });

            modelBuilder.Entity("weatherAPI.Models.Database.FavoriteCity", entity =>
            {
                entity.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid");

                entity.Property<Guid>("AppUserId")
                    .HasColumnType("uuid");

                entity.Property<Guid>("CityId")
                    .HasColumnType("uuid");

                entity.Property<DateTime>("CreatedAtUtc")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.HasKey("Id");
                entity.HasIndex("CityId");
                entity.HasIndex("AppUserId", "CityId").IsUnique();
                entity.ToTable("FavoriteCities");
            });

            modelBuilder.Entity("weatherAPI.Models.Database.SearchHistory", entity =>
            {
                entity.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid");

                entity.Property<Guid>("AppUserId")
                    .HasColumnType("uuid");

                entity.Property<Guid>("CityId")
                    .HasColumnType("uuid");

                entity.Property<string>("QueryText")
                    .IsRequired()
                    .HasMaxLength(160)
                    .HasColumnType("character varying(160)");

                entity.Property<DateTime>("SearchedAtUtc")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.HasKey("Id");
                entity.HasIndex("CityId");
                entity.HasIndex("AppUserId", "SearchedAtUtc");
                entity.ToTable("SearchHistory");
            });

            modelBuilder.Entity("weatherAPI.Models.Database.WeatherRequestLog", entity =>
            {
                entity.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid");

                entity.Property<Guid?>("AppUserId")
                    .HasColumnType("uuid");

                entity.Property<Guid?>("CityId")
                    .HasColumnType("uuid");

                entity.Property<string>("Endpoint")
                    .IsRequired()
                    .HasMaxLength(80)
                    .HasColumnType("character varying(80)");

                entity.Property<string>("ErrorMessage")
                    .HasMaxLength(500)
                    .HasColumnType("character varying(500)");

                entity.Property<string>("HttpMethod")
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnType("character varying(10)");

                entity.Property<string>("QueryText")
                    .HasMaxLength(160)
                    .HasColumnType("character varying(160)");

                entity.Property<DateTime>("RequestedAtUtc")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property<int?>("StatusCode")
                    .HasColumnType("integer");

                entity.Property<bool>("WasSuccessful")
                    .HasColumnType("boolean");

                entity.HasKey("Id");
                entity.HasIndex("AppUserId");
                entity.HasIndex("CityId");
                entity.HasIndex("RequestedAtUtc");
                entity.ToTable("WeatherRequestLogs");
            });

            modelBuilder.Entity("weatherAPI.Models.Database.FavoriteCity", entity =>
            {
                entity.HasOne("weatherAPI.Models.Database.AppUser", "User")
                    .WithMany("FavoriteCities")
                    .HasForeignKey("AppUserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                entity.HasOne("weatherAPI.Models.Database.City", "City")
                    .WithMany("FavoriteCities")
                    .HasForeignKey("CityId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                entity.Navigation("City");
                entity.Navigation("User");
            });

            modelBuilder.Entity("weatherAPI.Models.Database.SearchHistory", entity =>
            {
                entity.HasOne("weatherAPI.Models.Database.AppUser", "User")
                    .WithMany("SearchHistory")
                    .HasForeignKey("AppUserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                entity.HasOne("weatherAPI.Models.Database.City", "City")
                    .WithMany("SearchHistory")
                    .HasForeignKey("CityId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                entity.Navigation("City");
                entity.Navigation("User");
            });

            modelBuilder.Entity("weatherAPI.Models.Database.WeatherRequestLog", entity =>
            {
                entity.HasOne("weatherAPI.Models.Database.AppUser", "User")
                    .WithMany("WeatherRequestLogs")
                    .HasForeignKey("AppUserId")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne("weatherAPI.Models.Database.City", "City")
                    .WithMany("WeatherRequestLogs")
                    .HasForeignKey("CityId")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.Navigation("City");
                entity.Navigation("User");
            });

            modelBuilder.Entity("weatherAPI.Models.Database.AppUser", entity =>
            {
                entity.Navigation("FavoriteCities");
                entity.Navigation("SearchHistory");
                entity.Navigation("WeatherRequestLogs");
            });

            modelBuilder.Entity("weatherAPI.Models.Database.City", entity =>
            {
                entity.Navigation("FavoriteCities");
                entity.Navigation("SearchHistory");
                entity.Navigation("WeatherRequestLogs");
            });
        }
    }
}
