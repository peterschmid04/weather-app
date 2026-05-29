using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using weatherAPI.Data;

#nullable disable

namespace weatherAPI.Migrations
{
    [DbContext(typeof(WeatherDbContext))]
    [Migration("20260529024500_AddWeatherStationShares")]
    public partial class AddWeatherStationShares : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "UserProfiles",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "UserProfiles",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WeatherStationShares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WeatherStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerUserProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    SharedWithUserProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    SharedWithEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    NormalizedSharedWithEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Permission = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    AcceptedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherStationShares", share => share.Id);
                    table.ForeignKey(
                        name: "FK_WeatherStationShares_UserProfiles_OwnerUserProfileId",
                        column: share => share.OwnerUserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeatherStationShares_UserProfiles_SharedWithUserProfileId",
                        column: share => share.SharedWithUserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WeatherStationShares_WeatherStations_WeatherStationId",
                        column: share => share.WeatherStationId,
                        principalTable: "WeatherStations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_NormalizedEmail",
                table: "UserProfiles",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherStationShares_NormalizedSharedWithEmail",
                table: "WeatherStationShares",
                column: "NormalizedSharedWithEmail");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherStationShares_OwnerUserProfileId",
                table: "WeatherStationShares",
                column: "OwnerUserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherStationShares_SharedWithUserProfileId",
                table: "WeatherStationShares",
                column: "SharedWithUserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherStationShares_WeatherStationId_NormalizedSharedWithEmail",
                table: "WeatherStationShares",
                columns: new[] { "WeatherStationId", "NormalizedSharedWithEmail" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeatherStationShares");

            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_NormalizedEmail",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "UserProfiles");
        }
    }
}
