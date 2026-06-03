using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class _20260603103000_NexArrMfaProvisioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MfaRecoveryCodeHashesJson",
                table: "user_credentials",
                type: "character varying(4096)",
                maxLength: 4096,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MfaSecret",
                table: "user_credentials",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MfaRecoveryCodeHashesJson",
                table: "user_credentials");

            migrationBuilder.DropColumn(
                name: "MfaSecret",
                table: "user_credentials");
        }
    }
}
