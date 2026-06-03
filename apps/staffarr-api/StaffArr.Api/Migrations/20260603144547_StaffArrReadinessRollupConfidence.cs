using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrReadinessRollupConfidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfidenceLevel",
                table: "staffarr_readiness_rollups",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "low");

            migrationBuilder.AddColumn<int>(
                name: "ConfidenceScore",
                table: "staffarr_readiness_rollups",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfidenceLevel",
                table: "staffarr_readiness_rollups");

            migrationBuilder.DropColumn(
                name: "ConfidenceScore",
                table: "staffarr_readiness_rollups");
        }
    }
}
