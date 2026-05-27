using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrPmSchedulesAndDueScan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_pm_schedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    IntervalDays = table.Column<int>(type: "integer", nullable: false),
                    NextDueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastCompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DueStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LastDueScanAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_pm_schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_pm_schedules_maintainarr_assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_schedules_AssetId",
                table: "maintainarr_pm_schedules",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_schedules_TenantId",
                table: "maintainarr_pm_schedules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_schedules_TenantId_AssetId_ScheduleKey",
                table: "maintainarr_pm_schedules",
                columns: new[] { "TenantId", "AssetId", "ScheduleKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_schedules_TenantId_Status_DueStatus_NextDueAt",
                table: "maintainarr_pm_schedules",
                columns: new[] { "TenantId", "Status", "DueStatus", "NextDueAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_pm_schedules");
        }
    }
}
