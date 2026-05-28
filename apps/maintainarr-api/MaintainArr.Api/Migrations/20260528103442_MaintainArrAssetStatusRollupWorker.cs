using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrAssetStatusRollupWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_asset_status_rollup_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsOfUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CandidatesFound = table.Column<int>(type: "integer", nullable: false),
                    RefreshedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    ScopeRollupsRefreshed = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_status_rollup_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_status_rollups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetTag = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AssetName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LifecycleStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReadinessStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReadinessBasis = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BlockerCount = table.Column<int>(type: "integer", nullable: false),
                    PrimaryBlockerMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    OpenCriticalDefectCount = table.Column<int>(type: "integer", nullable: false),
                    OpenHighDefectCount = table.Column<int>(type: "integer", nullable: false),
                    ActiveWorkOrderCount = table.Column<int>(type: "integer", nullable: false),
                    PmDueCount = table.Column<int>(type: "integer", nullable: false),
                    PmOverdueCount = table.Column<int>(type: "integer", nullable: false),
                    FailedInspectionCount = table.Column<int>(type: "integer", nullable: false),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_status_rollups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_status_scope_rollups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScopeEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeEntityKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ScopeLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TotalAssets = table.Column<int>(type: "integer", nullable: false),
                    ReadyCount = table.Column<int>(type: "integer", nullable: false),
                    NotReadyCount = table.Column<int>(type: "integer", nullable: false),
                    ReadyPercent = table.Column<decimal>(type: "numeric(5,1)", precision: 5, scale: 1, nullable: false),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_status_scope_rollups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_tenant_asset_status_rollup_settings",
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
                    table.PrimaryKey("PK_maintainarr_tenant_asset_status_rollup_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_status_rollup_runs_TenantId_CreatedAt",
                table: "maintainarr_asset_status_rollup_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_status_rollups_TenantId",
                table: "maintainarr_asset_status_rollups",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_status_rollups_TenantId_AssetId",
                table: "maintainarr_asset_status_rollups",
                columns: new[] { "TenantId", "AssetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_status_rollups_TenantId_ComputedAt",
                table: "maintainarr_asset_status_rollups",
                columns: new[] { "TenantId", "ComputedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_status_scope_rollups_TenantId",
                table: "maintainarr_asset_status_scope_rollups",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_status_scope_rollups_TenantId_ScopeType_C~",
                table: "maintainarr_asset_status_scope_rollups",
                columns: new[] { "TenantId", "ScopeType", "ComputedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_status_scope_rollups_TenantId_ScopeType_S~",
                table: "maintainarr_asset_status_scope_rollups",
                columns: new[] { "TenantId", "ScopeType", "ScopeEntityId", "ScopeEntityKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_tenant_asset_status_rollup_settings_TenantId",
                table: "maintainarr_tenant_asset_status_rollup_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_asset_status_rollup_runs");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_status_rollups");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_status_scope_rollups");

            migrationBuilder.DropTable(
                name: "maintainarr_tenant_asset_status_rollup_settings");
        }
    }
}
