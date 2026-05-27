using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace weatherAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUserThemePreferencesAndCrudEndpoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserThemePreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThemeName = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserThemePreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserThemePreferences_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserThemePreferences_UserProfileId",
                table: "UserThemePreferences",
                column: "UserProfileId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserThemePreferences");
        }
    }
}
