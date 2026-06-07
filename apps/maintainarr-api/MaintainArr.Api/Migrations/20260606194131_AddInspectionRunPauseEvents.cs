using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddInspectionRunPauseEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "maintainarr_pm_programs",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "submitted",
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "maintainarr_asset_classes",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldDefaultValue: "submitted");

            migrationBuilder.CreateTable(
                name: "maintainarr_inspection_run_pause_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    PausedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResumedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    Reason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    PausedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResumedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_inspection_run_pause_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_inspection_run_pause_events_maintainarr_inspect~",
                        column: x => x.InspectionRunId,
                        principalTable: "maintainarr_inspection_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_run_pause_events_InspectionRunId",
                table: "maintainarr_inspection_run_pause_events",
                column: "InspectionRunId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_run_pause_events_TenantId",
                table: "maintainarr_inspection_run_pause_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_inspection_run_pause_events_TenantId_Inspection~",
                table: "maintainarr_inspection_run_pause_events",
                columns: new[] { "TenantId", "InspectionRunId", "PausedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_inspection_run_pause_events");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "maintainarr_pm_programs",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldDefaultValue: "submitted");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "maintainarr_asset_classes",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "submitted",
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);
        }
    }
}
