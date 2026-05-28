using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrAvailabilitySnapshotWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CatalogAvailabilityStatus",
                table: "supplyarr_part_vendor_links",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CatalogQuantityAvailable",
                table: "supplyarr_part_vendor_links",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "supplyarr_availability_snapshot_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsOfUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CandidatesFound = table.Column<int>(type: "integer", nullable: false),
                    CapturedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_availability_snapshot_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_part_vendor_availability_capture_states",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartVendorLinkId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastCapturedQuantityAvailable = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    LastCapturedAvailabilityStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    LastAvailabilitySnapshotId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastCapturedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_part_vendor_availability_capture_states", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_vendor_availability_capture_states_supplyarr~",
                        column: x => x.PartVendorLinkId,
                        principalTable: "supplyarr_part_vendor_links",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_tenant_availability_snapshot_settings",
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
                    table.PrimaryKey("PK_supplyarr_tenant_availability_snapshot_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_availability_snapshot_runs_TenantId",
                table: "supplyarr_availability_snapshot_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_availability_snapshot_runs_TenantId_CreatedAt",
                table: "supplyarr_availability_snapshot_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_availability_capture_states_PartVendo~",
                table: "supplyarr_part_vendor_availability_capture_states",
                column: "PartVendorLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_availability_capture_states_TenantId",
                table: "supplyarr_part_vendor_availability_capture_states",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_availability_capture_states_TenantId_~",
                table: "supplyarr_part_vendor_availability_capture_states",
                columns: new[] { "TenantId", "PartVendorLinkId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_tenant_availability_snapshot_settings_TenantId",
                table: "supplyarr_tenant_availability_snapshot_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_availability_snapshot_runs");

            migrationBuilder.DropTable(
                name: "supplyarr_part_vendor_availability_capture_states");

            migrationBuilder.DropTable(
                name: "supplyarr_tenant_availability_snapshot_settings");

            migrationBuilder.DropColumn(
                name: "CatalogAvailabilityStatus",
                table: "supplyarr_part_vendor_links");

            migrationBuilder.DropColumn(
                name: "CatalogQuantityAvailable",
                table: "supplyarr_part_vendor_links");
        }
    }
}
