using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrDemandProcessingWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_demand_processing_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsOfUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CandidatesFound = table.Column<int>(type: "integer", nullable: false),
                    ProcessedCount = table.Column<int>(type: "integer", nullable: false),
                    PrDraftsCreatedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_demand_processing_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_demand_processing_states",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DemandRefId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaintainarrWorkOrderNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ProcessingOutcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RecommendedAction = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LinesTotalCount = table.Column<int>(type: "integer", nullable: false),
                    LinesCatalogCount = table.Column<int>(type: "integer", nullable: false),
                    LinesShortCount = table.Column<int>(type: "integer", nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastProcessingMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    DemandReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_demand_processing_states", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_demand_processing_states_supplyarr_maintainarr_de~",
                        column: x => x.DemandRefId,
                        principalTable: "supplyarr_maintainarr_demand_refs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_tenant_demand_processing_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AutoCreatePrDraftWhenShort = table.Column<bool>(type: "boolean", nullable: false),
                    MinHoursBeforeProcessing = table.Column<int>(type: "integer", nullable: false),
                    StalenessHours = table.Column<int>(type: "integer", nullable: false),
                    NotifyOnPrDraftCreated = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_tenant_demand_processing_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_demand_processing_runs_TenantId",
                table: "supplyarr_demand_processing_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_demand_processing_runs_TenantId_CreatedAt",
                table: "supplyarr_demand_processing_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_demand_processing_states_DemandRefId",
                table: "supplyarr_demand_processing_states",
                column: "DemandRefId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_demand_processing_states_TenantId",
                table: "supplyarr_demand_processing_states",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_demand_processing_states_TenantId_DemandRefId",
                table: "supplyarr_demand_processing_states",
                columns: new[] { "TenantId", "DemandRefId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_demand_processing_states_TenantId_LastProcessedAt",
                table: "supplyarr_demand_processing_states",
                columns: new[] { "TenantId", "LastProcessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_tenant_demand_processing_settings_TenantId",
                table: "supplyarr_tenant_demand_processing_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_demand_processing_runs");

            migrationBuilder.DropTable(
                name: "supplyarr_demand_processing_states");

            migrationBuilder.DropTable(
                name: "supplyarr_tenant_demand_processing_settings");
        }
    }
}
