using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexArr.Api.Migrations;

public partial class NexArrPlatformSessionPasswordPolicy : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "PasswordMinLength",
            table: "nexarr_platform_session_settings",
            type: "integer",
            nullable: false,
            defaultValue: 12);

        migrationBuilder.AddColumn<bool>(
            name: "RequirePasswordComplexity",
            table: "nexarr_platform_session_settings",
            type: "boolean",
            nullable: false,
            defaultValue: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PasswordMinLength",
            table: "nexarr_platform_session_settings");

        migrationBuilder.DropColumn(
            name: "RequirePasswordComplexity",
            table: "nexarr_platform_session_settings");
    }
}
