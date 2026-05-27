using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrMeterTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssetMeterId",
                table: "maintainarr_pm_schedules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "IntervalUsage",
                table: "maintainarr_pm_schedules",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LastCompletedUsage",
                table: "maintainarr_pm_schedules",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NextDueAtUsage",
                table: "maintainarr_pm_schedules",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScheduleMode",
                table: "maintainarr_pm_schedules",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_meters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    MeterKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Unit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BaselineReading = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CurrentReading = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    LastReadingAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_meters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_asset_meters_maintainarr_assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_meter_readings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetMeterId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReadingValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    DeltaFromPrevious = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    IsCorrection = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_meter_readings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_meter_readings_maintainarr_asset_meters_AssetMe~",
                        column: x => x.AssetMeterId,
                        principalTable: "maintainarr_asset_meters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_maintainarr_meter_readings_maintainarr_assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_schedules_AssetMeterId",
                table: "maintainarr_pm_schedules",
                column: "AssetMeterId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_pm_schedules_TenantId_AssetMeterId_Status",
                table: "maintainarr_pm_schedules",
                columns: new[] { "TenantId", "AssetMeterId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_meters_AssetId",
                table: "maintainarr_asset_meters",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_meters_TenantId",
                table: "maintainarr_asset_meters",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_meters_TenantId_AssetId_MeterKey",
                table: "maintainarr_asset_meters",
                columns: new[] { "TenantId", "AssetId", "MeterKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_meters_TenantId_AssetId_Status",
                table: "maintainarr_asset_meters",
                columns: new[] { "TenantId", "AssetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_meter_readings_AssetId",
                table: "maintainarr_meter_readings",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_meter_readings_AssetMeterId",
                table: "maintainarr_meter_readings",
                column: "AssetMeterId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_meter_readings_TenantId",
                table: "maintainarr_meter_readings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_meter_readings_TenantId_AssetId_ReadAt",
                table: "maintainarr_meter_readings",
                columns: new[] { "TenantId", "AssetId", "ReadAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_meter_readings_TenantId_AssetMeterId_ReadAt",
                table: "maintainarr_meter_readings",
                columns: new[] { "TenantId", "AssetMeterId", "ReadAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_maintainarr_pm_schedules_maintainarr_asset_meters_AssetMete~",
                table: "maintainarr_pm_schedules",
                column: "AssetMeterId",
                principalTable: "maintainarr_asset_meters",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_maintainarr_pm_schedules_maintainarr_asset_meters_AssetMete~",
                table: "maintainarr_pm_schedules");

            migrationBuilder.DropTable(
                name: "maintainarr_meter_readings");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_meters");

            migrationBuilder.DropIndex(
                name: "IX_maintainarr_pm_schedules_AssetMeterId",
                table: "maintainarr_pm_schedules");

            migrationBuilder.DropIndex(
                name: "IX_maintainarr_pm_schedules_TenantId_AssetMeterId_Status",
                table: "maintainarr_pm_schedules");

            migrationBuilder.DropColumn(
                name: "AssetMeterId",
                table: "maintainarr_pm_schedules");

            migrationBuilder.DropColumn(
                name: "IntervalUsage",
                table: "maintainarr_pm_schedules");

            migrationBuilder.DropColumn(
                name: "LastCompletedUsage",
                table: "maintainarr_pm_schedules");

            migrationBuilder.DropColumn(
                name: "NextDueAtUsage",
                table: "maintainarr_pm_schedules");

            migrationBuilder.DropColumn(
                name: "ScheduleMode",
                table: "maintainarr_pm_schedules");
        }
    }
}
