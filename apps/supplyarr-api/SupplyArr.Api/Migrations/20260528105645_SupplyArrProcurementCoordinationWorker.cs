using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrProcurementCoordinationWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_procurement_coordination_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CoordinationStage = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NextActionRequired = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    VendorPartyId = table.Column<Guid>(type: "uuid", nullable: true),
                    VendorDisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DocumentStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LineCount = table.Column<int>(type: "integer", nullable: false),
                    QuantityOrdered = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityReceived = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ReceiptProgressPercent = table.Column<int>(type: "integer", nullable: true),
                    IsTerminal = table.Column<bool>(type: "boolean", nullable: false),
                    SourceUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_procurement_coordination_records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_procurement_coordination_runs",
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
                    table.PrimaryKey("PK_supplyarr_procurement_coordination_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_tenant_procurement_coordination_settings",
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
                    table.PrimaryKey("PK_supplyarr_tenant_procurement_coordination_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_procurement_coordination_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoordinationRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Detail = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    SourceEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceEntityId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_procurement_coordination_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_procurement_coordination_events_supplyarr_procure~",
                        column: x => x.CoordinationRecordId,
                        principalTable: "supplyarr_procurement_coordination_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_coordination_events_CoordinationRecor~",
                table: "supplyarr_procurement_coordination_events",
                column: "CoordinationRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_coordination_events_TenantId",
                table: "supplyarr_procurement_coordination_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_coordination_events_TenantId_Coordina~",
                table: "supplyarr_procurement_coordination_events",
                columns: new[] { "TenantId", "CoordinationRecordId", "SequenceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_coordination_records_TenantId",
                table: "supplyarr_procurement_coordination_records",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_coordination_records_TenantId_Coordin~",
                table: "supplyarr_procurement_coordination_records",
                columns: new[] { "TenantId", "CoordinationStage", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_coordination_records_TenantId_IsTermi~",
                table: "supplyarr_procurement_coordination_records",
                columns: new[] { "TenantId", "IsTerminal", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_coordination_records_TenantId_Subject~",
                table: "supplyarr_procurement_coordination_records",
                columns: new[] { "TenantId", "SubjectType", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_coordination_runs_TenantId",
                table: "supplyarr_procurement_coordination_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_coordination_runs_TenantId_CreatedAt",
                table: "supplyarr_procurement_coordination_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_tenant_procurement_coordination_settings_TenantId",
                table: "supplyarr_tenant_procurement_coordination_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_procurement_coordination_events");

            migrationBuilder.DropTable(
                name: "supplyarr_procurement_coordination_runs");

            migrationBuilder.DropTable(
                name: "supplyarr_tenant_procurement_coordination_settings");

            migrationBuilder.DropTable(
                name: "supplyarr_procurement_coordination_records");
        }
    }
}
