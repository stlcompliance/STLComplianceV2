using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrDowntimeTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_asset_availability_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetTag = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AssetName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PeriodStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TotalHours = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DowntimeHours = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AvailabilityPercent = table.Column<decimal>(type: "numeric(5,1)", precision: 5, scale: 1, nullable: false),
                    PlannedDowntimeHours = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnplannedDowntimeHours = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    HasActiveDowntime = table.Column<bool>(type: "boolean", nullable: false),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_availability_snapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_downtime_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetTag = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AssetName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Reason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsPlanned = table.Column<bool>(type: "boolean", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StatusTrigger = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    DefectId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_downtime_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_downtime_sync_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsOfUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AssetsScanned = table.Column<int>(type: "integer", nullable: false),
                    EventsOpened = table.Column<int>(type: "integer", nullable: false),
                    EventsClosed = table.Column<int>(type: "integer", nullable: false),
                    SnapshotsRefreshed = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_downtime_sync_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_fleet_availability_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AssetCount = table.Column<int>(type: "integer", nullable: false),
                    TotalHours = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DowntimeHours = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AvailabilityPercent = table.Column<decimal>(type: "numeric(5,1)", precision: 5, scale: 1, nullable: false),
                    PlannedDowntimeHours = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnplannedDowntimeHours = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ActiveDowntimeEventCount = table.Column<int>(type: "integer", nullable: false),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_fleet_availability_snapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_tenant_downtime_tracking_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AutoTrackOutOfService = table.Column<bool>(type: "boolean", nullable: false),
                    AutoTrackNotReady = table.Column<bool>(type: "boolean", nullable: false),
                    AvailabilityPeriodDays = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_tenant_downtime_tracking_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_availability_snapshots_TenantId",
                table: "maintainarr_asset_availability_snapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_availability_snapshots_TenantId_AssetId",
                table: "maintainarr_asset_availability_snapshots",
                columns: new[] { "TenantId", "AssetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_downtime_events_TenantId",
                table: "maintainarr_asset_downtime_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_downtime_events_TenantId_AssetId_EndedAt",
                table: "maintainarr_asset_downtime_events",
                columns: new[] { "TenantId", "AssetId", "EndedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_downtime_events_TenantId_AssetId_StartedAt",
                table: "maintainarr_asset_downtime_events",
                columns: new[] { "TenantId", "AssetId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_downtime_events_TenantId_Source_EndedAt",
                table: "maintainarr_asset_downtime_events",
                columns: new[] { "TenantId", "Source", "EndedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_downtime_sync_runs_TenantId_CreatedAt",
                table: "maintainarr_asset_downtime_sync_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_fleet_availability_snapshots_TenantId",
                table: "maintainarr_fleet_availability_snapshots",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_tenant_downtime_tracking_settings_TenantId",
                table: "maintainarr_tenant_downtime_tracking_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_asset_availability_snapshots");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_downtime_events");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_downtime_sync_runs");

            migrationBuilder.DropTable(
                name: "maintainarr_fleet_availability_snapshots");

            migrationBuilder.DropTable(
                name: "maintainarr_tenant_downtime_tracking_settings");
        }
    }
}
