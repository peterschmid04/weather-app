using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using weatherAPI.Data;

#nullable disable

namespace weatherAPI.Migrations
{
    [DbContext(typeof(WeatherDbContext))]
    [Migration("20260529173000_AddDefaultFavoriteCity")]
    public partial class AddDefaultFavoriteCity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "FavoriteCities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteCities_UserProfileId_IsDefault",
                table: "FavoriteCities",
                columns: new[] { "UserProfileId", "IsDefault" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FavoriteCities_UserProfileId_IsDefault",
                table: "FavoriteCities");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "FavoriteCities");
        }
    }
}
