using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrPricingLeadTimeSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_part_vendor_lead_time_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartVendorLinkId = table.Column<Guid>(type: "uuid", nullable: false),
                    SnapshotKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LeadTimeDays = table.Column<int>(type: "integer", nullable: false),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_part_vendor_lead_time_snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_vendor_lead_time_snapshots_supplyarr_part_ve~",
                        column: x => x.PartVendorLinkId,
                        principalTable: "supplyarr_part_vendor_links",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_part_vendor_pricing_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartVendorLinkId = table.Column<Guid>(type: "uuid", nullable: false),
                    SnapshotKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    MinimumOrderQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_part_vendor_pricing_snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_vendor_pricing_snapshots_supplyarr_part_vend~",
                        column: x => x.PartVendorLinkId,
                        principalTable: "supplyarr_part_vendor_links",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_lead_time_snapshots_PartVendorLinkId",
                table: "supplyarr_part_vendor_lead_time_snapshots",
                column: "PartVendorLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_lead_time_snapshots_TenantId",
                table: "supplyarr_part_vendor_lead_time_snapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_lead_time_snapshots_TenantId_PartVen~1",
                table: "supplyarr_part_vendor_lead_time_snapshots",
                columns: new[] { "TenantId", "PartVendorLinkId", "EffectiveTo" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_lead_time_snapshots_TenantId_PartVend~",
                table: "supplyarr_part_vendor_lead_time_snapshots",
                columns: new[] { "TenantId", "PartVendorLinkId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_lead_time_snapshots_TenantId_Snapshot~",
                table: "supplyarr_part_vendor_lead_time_snapshots",
                columns: new[] { "TenantId", "SnapshotKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_pricing_snapshots_PartVendorLinkId",
                table: "supplyarr_part_vendor_pricing_snapshots",
                column: "PartVendorLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_pricing_snapshots_TenantId",
                table: "supplyarr_part_vendor_pricing_snapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_pricing_snapshots_TenantId_PartVendo~1",
                table: "supplyarr_part_vendor_pricing_snapshots",
                columns: new[] { "TenantId", "PartVendorLinkId", "EffectiveTo" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_pricing_snapshots_TenantId_PartVendor~",
                table: "supplyarr_part_vendor_pricing_snapshots",
                columns: new[] { "TenantId", "PartVendorLinkId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_pricing_snapshots_TenantId_SnapshotKey",
                table: "supplyarr_part_vendor_pricing_snapshots",
                columns: new[] { "TenantId", "SnapshotKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_part_vendor_lead_time_snapshots");

            migrationBuilder.DropTable(
                name: "supplyarr_part_vendor_pricing_snapshots");
        }
    }
}
