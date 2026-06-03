using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class NexArrPlatformSessionSettingsRequireMfa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequirePlatformAdminMfa",
                table: "nexarr_platform_session_settings",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequirePlatformAdminMfa",
                table: "nexarr_platform_session_settings");
        }
    }
}
