using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using weatherAPI.Data;

#nullable disable

namespace weatherAPI.Migrations
{
    [DbContext(typeof(WeatherDbContext))]
    [Migration("20260527143000_InitialWeatherPersistence")]
    public partial class InitialWeatherPersistence : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Auth0Subject = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", user => user.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", city => city.Id);
                });

            migrationBuilder.CreateTable(
                name: "FavoriteCities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteCities", favorite => favorite.Id);
                    table.ForeignKey(
                        name: "FK_FavoriteCities_AppUsers_AppUserId",
                        column: favorite => favorite.AppUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoriteCities_Cities_CityId",
                        column: favorite => favorite.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SearchHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CityId = table.Column<Guid>(type: "uuid", nullable: false),
                    QueryText = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    SearchedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchHistory", history => history.Id);
                    table.ForeignKey(
                        name: "FK_SearchHistory_AppUsers_AppUserId",
                        column: history => history.AppUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SearchHistory_Cities_CityId",
                        column: history => history.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WeatherRequestLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CityId = table.Column<Guid>(type: "uuid", nullable: true),
                    HttpMethod = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    QueryText = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    StatusCode = table.Column<int>(type: "integer", nullable: true),
                    WasSuccessful = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RequestedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherRequestLogs", log => log.Id);
                    table.ForeignKey(
                        name: "FK_WeatherRequestLogs_AppUsers_AppUserId",
                        column: log => log.AppUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WeatherRequestLogs_Cities_CityId",
                        column: log => log.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_Auth0Subject",
                table: "AppUsers",
                column: "Auth0Subject",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cities_NormalizedName_CountryCode",
                table: "Cities",
                columns: new[] { "NormalizedName", "CountryCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteCities_AppUserId_CityId",
                table: "FavoriteCities",
                columns: new[] { "AppUserId", "CityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteCities_CityId",
                table: "FavoriteCities",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchHistory_AppUserId_SearchedAtUtc",
                table: "SearchHistory",
                columns: new[] { "AppUserId", "SearchedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SearchHistory_CityId",
                table: "SearchHistory",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherRequestLogs_AppUserId",
                table: "WeatherRequestLogs",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherRequestLogs_CityId",
                table: "WeatherRequestLogs",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherRequestLogs_RequestedAtUtc",
                table: "WeatherRequestLogs",
                column: "RequestedAtUtc");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "FavoriteCities");
            migrationBuilder.DropTable(name: "SearchHistory");
            migrationBuilder.DropTable(name: "WeatherRequestLogs");
            migrationBuilder.DropTable(name: "AppUsers");
            migrationBuilder.DropTable(name: "Cities");
        }
    }
}
