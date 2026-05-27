using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace weatherAPI.Migrations
{
    /// <inheritdoc />
    public partial class RenameAppUsersToUserProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteCities_AppUsers_AppUserId",
                table: "FavoriteCities");

            migrationBuilder.DropForeignKey(
                name: "FK_SearchHistory_AppUsers_AppUserId",
                table: "SearchHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_WeatherRequestLogs_AppUsers_AppUserId",
                table: "WeatherRequestLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_WeatherStations_AppUsers_AppUserId",
                table: "WeatherStations");

            migrationBuilder.RenameTable(
                name: "AppUsers",
                newName: "UserProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppUsers",
                table: "UserProfiles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserProfiles",
                table: "UserProfiles",
                column: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_AppUsers_Auth0Subject",
                table: "UserProfiles",
                newName: "IX_UserProfiles_Auth0Subject");

            migrationBuilder.RenameColumn(
                name: "AppUserId",
                table: "WeatherStations",
                newName: "UserProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_WeatherStations_AppUserId_Name",
                table: "WeatherStations",
                newName: "IX_WeatherStations_UserProfileId_Name");

            migrationBuilder.RenameColumn(
                name: "AppUserId",
                table: "WeatherRequestLogs",
                newName: "UserProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_WeatherRequestLogs_AppUserId",
                table: "WeatherRequestLogs",
                newName: "IX_WeatherRequestLogs_UserProfileId");

            migrationBuilder.RenameColumn(
                name: "AppUserId",
                table: "SearchHistory",
                newName: "UserProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_SearchHistory_AppUserId_SearchedAtUtc",
                table: "SearchHistory",
                newName: "IX_SearchHistory_UserProfileId_SearchedAtUtc");

            migrationBuilder.RenameColumn(
                name: "AppUserId",
                table: "FavoriteCities",
                newName: "UserProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_FavoriteCities_AppUserId_CityId",
                table: "FavoriteCities",
                newName: "IX_FavoriteCities_UserProfileId_CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteCities_UserProfiles_UserProfileId",
                table: "FavoriteCities",
                column: "UserProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SearchHistory_UserProfiles_UserProfileId",
                table: "SearchHistory",
                column: "UserProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WeatherRequestLogs_UserProfiles_UserProfileId",
                table: "WeatherRequestLogs",
                column: "UserProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WeatherStations_UserProfiles_UserProfileId",
                table: "WeatherStations",
                column: "UserProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteCities_UserProfiles_UserProfileId",
                table: "FavoriteCities");

            migrationBuilder.DropForeignKey(
                name: "FK_SearchHistory_UserProfiles_UserProfileId",
                table: "SearchHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_WeatherRequestLogs_UserProfiles_UserProfileId",
                table: "WeatherRequestLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_WeatherStations_UserProfiles_UserProfileId",
                table: "WeatherStations");

            migrationBuilder.RenameTable(
                name: "UserProfiles",
                newName: "AppUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserProfiles",
                table: "AppUsers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppUsers",
                table: "AppUsers",
                column: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_UserProfiles_Auth0Subject",
                table: "AppUsers",
                newName: "IX_AppUsers_Auth0Subject");

            migrationBuilder.RenameColumn(
                name: "UserProfileId",
                table: "WeatherStations",
                newName: "AppUserId");

            migrationBuilder.RenameIndex(
                name: "IX_WeatherStations_UserProfileId_Name",
                table: "WeatherStations",
                newName: "IX_WeatherStations_AppUserId_Name");

            migrationBuilder.RenameColumn(
                name: "UserProfileId",
                table: "WeatherRequestLogs",
                newName: "AppUserId");

            migrationBuilder.RenameIndex(
                name: "IX_WeatherRequestLogs_UserProfileId",
                table: "WeatherRequestLogs",
                newName: "IX_WeatherRequestLogs_AppUserId");

            migrationBuilder.RenameColumn(
                name: "UserProfileId",
                table: "SearchHistory",
                newName: "AppUserId");

            migrationBuilder.RenameIndex(
                name: "IX_SearchHistory_UserProfileId_SearchedAtUtc",
                table: "SearchHistory",
                newName: "IX_SearchHistory_AppUserId_SearchedAtUtc");

            migrationBuilder.RenameColumn(
                name: "UserProfileId",
                table: "FavoriteCities",
                newName: "AppUserId");

            migrationBuilder.RenameIndex(
                name: "IX_FavoriteCities_UserProfileId_CityId",
                table: "FavoriteCities",
                newName: "IX_FavoriteCities_AppUserId_CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteCities_AppUsers_AppUserId",
                table: "FavoriteCities",
                column: "AppUserId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SearchHistory_AppUsers_AppUserId",
                table: "SearchHistory",
                column: "AppUserId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WeatherRequestLogs_AppUsers_AppUserId",
                table: "WeatherRequestLogs",
                column: "AppUserId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WeatherStations_AppUsers_AppUserId",
                table: "WeatherStations",
                column: "AppUserId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
