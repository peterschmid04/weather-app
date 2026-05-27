using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace weatherAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalWeatherStations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeatherStations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherStations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeatherStations_AppUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WeatherStations_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WeatherStationMeasurements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WeatherStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    MeasuredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    TemperatureC = table.Column<double>(type: "double precision", nullable: true),
                    HumidityPercent = table.Column<double>(type: "double precision", nullable: true),
                    PressureHpa = table.Column<double>(type: "double precision", nullable: true),
                    WindSpeedKmh = table.Column<double>(type: "double precision", nullable: true),
                    WindDirectionDegrees = table.Column<int>(type: "integer", nullable: true),
                    RainfallMm = table.Column<double>(type: "double precision", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherStationMeasurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeatherStationMeasurements_WeatherStations_WeatherStationId",
                        column: x => x.WeatherStationId,
                        principalTable: "WeatherStations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeatherStationMeasurements_WeatherStationId_MeasuredAtUtc",
                table: "WeatherStationMeasurements",
                columns: new[] { "WeatherStationId", "MeasuredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_WeatherStations_AppUserId_Name",
                table: "WeatherStations",
                columns: new[] { "AppUserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeatherStations_CityId",
                table: "WeatherStations",
                column: "CityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeatherStationMeasurements");

            migrationBuilder.DropTable(
                name: "WeatherStations");
        }
    }
}
