using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPmOccurrences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_pm_occurrences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PmScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurrenceNumber = table.Column<int>(type: "integer", nullable: false),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DueMeterType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DueMeterValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    GeneratedWorkOrderRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    GeneratedInspectionRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedByWorkOrderRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SkippedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    SkippedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SkippedReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_pm_occurrences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_pm_occurrences_maintainarr_assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_maintainarr_pm_occurrences_maintainarr_pm_schedules_PmSched~",
                        column: x => x.PmScheduleId,
                        principalTable: "maintainarr_pm_schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_occurrences_AssetId",
                table: "maintainarr_pm_occurrences",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_occurrences_PmScheduleId",
                table: "maintainarr_pm_occurrences",
                column: "PmScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_occurrences_TenantId",
                table: "maintainarr_pm_occurrences",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_occurrences_TenantId_PmScheduleId_DueAt",
                table: "maintainarr_pm_occurrences",
                columns: new[] { "TenantId", "PmScheduleId", "DueAt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_occurrences_TenantId_PmScheduleId_Occurrence~",
                table: "maintainarr_pm_occurrences",
                columns: new[] { "TenantId", "PmScheduleId", "OccurrenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_occurrences_TenantId_Status_DueAt",
                table: "maintainarr_pm_occurrences",
                columns: new[] { "TenantId", "Status", "DueAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_pm_occurrences");
        }
    }
}
