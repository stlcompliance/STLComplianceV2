using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrMaintenanceHistoryRollupWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_maintenance_history_rollup_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsOfUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CandidatesFound = table.Column<int>(type: "integer", nullable: false),
                    RefreshedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_maintenance_history_rollup_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_maintenance_history_rollups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetTag = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AssetName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    EventCount = table.Column<int>(type: "integer", nullable: false),
                    InspectionCount = table.Column<int>(type: "integer", nullable: false),
                    DefectCount = table.Column<int>(type: "integer", nullable: false),
                    WorkOrderCount = table.Column<int>(type: "integer", nullable: false),
                    PmCount = table.Column<int>(type: "integer", nullable: false),
                    LastEventAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_maintenance_history_rollups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_tenant_maintenance_history_rollup_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    StalenessHours = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_tenant_maintenance_history_rollup_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_maintenance_history_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    RollupId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EventType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Detail = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceEntityId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelatedEntityId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_maintenance_history_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_maintenance_history_events_maintainarr_maintena~",
                        column: x => x.RollupId,
                        principalTable: "maintainarr_maintenance_history_rollups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_history_events_RollupId",
                table: "maintainarr_maintenance_history_events",
                column: "RollupId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_history_events_TenantId_AssetId_Occ~",
                table: "maintainarr_maintenance_history_events",
                columns: new[] { "TenantId", "AssetId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_history_events_TenantId_RollupId",
                table: "maintainarr_maintenance_history_events",
                columns: new[] { "TenantId", "RollupId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_history_rollup_runs_TenantId_Create~",
                table: "maintainarr_maintenance_history_rollup_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_history_rollups_TenantId",
                table: "maintainarr_maintenance_history_rollups",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_history_rollups_TenantId_AssetId",
                table: "maintainarr_maintenance_history_rollups",
                columns: new[] { "TenantId", "AssetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_history_rollups_TenantId_ComputedAt",
                table: "maintainarr_maintenance_history_rollups",
                columns: new[] { "TenantId", "ComputedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_tenant_maintenance_history_rollup_settings_Tena~",
                table: "maintainarr_tenant_maintenance_history_rollup_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_maintenance_history_events");

            migrationBuilder.DropTable(
                name: "maintainarr_maintenance_history_rollup_runs");

            migrationBuilder.DropTable(
                name: "maintainarr_tenant_maintenance_history_rollup_settings");

            migrationBuilder.DropTable(
                name: "maintainarr_maintenance_history_rollups");
        }
    }
}
