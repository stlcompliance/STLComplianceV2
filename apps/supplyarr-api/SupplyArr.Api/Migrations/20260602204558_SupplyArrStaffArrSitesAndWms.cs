using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrStaffArrSitesAndWms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StaffarrSiteNameSnapshot",
                table: "supplyarr_inventory_locations",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "StaffarrSiteOrgUnitId",
                table: "supplyarr_inventory_locations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StaffarrSiteResolutionStatus",
                table: "supplyarr_inventory_locations",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "unassigned");

            migrationBuilder.Sql(
                "UPDATE supplyarr_inventory_locations SET \"LocationType\" = 'parts_room' WHERE \"LocationType\" = 'site';");

            migrationBuilder.CreateTable(
                name: "supplyarr_wms_outbound_shipments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipmentKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ShipVia = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DestinationName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DestinationAddressSnapshot = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    RoutarrShipmentIntentId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoutarrRouteId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoutarrStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_wms_outbound_shipments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_wms_stock_ledger",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MovementGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MovementType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryBinId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelatedInventoryBinId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuantityOnHandDelta = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityReservedDelta = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityOnHandAfter = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityReservedAfter = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_wms_stock_ledger", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_wms_stock_ledger_supplyarr_inventory_bins_Invento~",
                        column: x => x.InventoryBinId,
                        principalTable: "supplyarr_inventory_bins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_wms_stock_ledger_supplyarr_inventory_bins_Related~",
                        column: x => x.RelatedInventoryBinId,
                        principalTable: "supplyarr_inventory_bins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_wms_stock_ledger_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_wms_outbound_shipment_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OutboundShipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromInventoryBinId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityRequested = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityReserved = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityPicked = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityShipped = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_wms_outbound_shipment_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_wms_outbound_shipment_lines_supplyarr_inventory_b~",
                        column: x => x.FromInventoryBinId,
                        principalTable: "supplyarr_inventory_bins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_wms_outbound_shipment_lines_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_wms_outbound_shipment_lines_supplyarr_wms_outboun~",
                        column: x => x.OutboundShipmentId,
                        principalTable: "supplyarr_wms_outbound_shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_inventory_locations_TenantId_StaffarrSiteOrgUnitId",
                table: "supplyarr_inventory_locations",
                columns: new[] { "TenantId", "StaffarrSiteOrgUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_outbound_shipment_lines_FromInventoryBinId",
                table: "supplyarr_wms_outbound_shipment_lines",
                column: "FromInventoryBinId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_outbound_shipment_lines_OutboundShipmentId",
                table: "supplyarr_wms_outbound_shipment_lines",
                column: "OutboundShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_outbound_shipment_lines_PartId",
                table: "supplyarr_wms_outbound_shipment_lines",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_outbound_shipment_lines_TenantId",
                table: "supplyarr_wms_outbound_shipment_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_outbound_shipment_lines_TenantId_OutboundShip~",
                table: "supplyarr_wms_outbound_shipment_lines",
                columns: new[] { "TenantId", "OutboundShipmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_outbound_shipments_TenantId",
                table: "supplyarr_wms_outbound_shipments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_outbound_shipments_TenantId_IdempotencyKey",
                table: "supplyarr_wms_outbound_shipments",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_outbound_shipments_TenantId_ShipmentKey",
                table: "supplyarr_wms_outbound_shipments",
                columns: new[] { "TenantId", "ShipmentKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_outbound_shipments_TenantId_Status_UpdatedAt",
                table: "supplyarr_wms_outbound_shipments",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_stock_ledger_InventoryBinId",
                table: "supplyarr_wms_stock_ledger",
                column: "InventoryBinId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_stock_ledger_PartId",
                table: "supplyarr_wms_stock_ledger",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_stock_ledger_RelatedInventoryBinId",
                table: "supplyarr_wms_stock_ledger",
                column: "RelatedInventoryBinId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_stock_ledger_TenantId",
                table: "supplyarr_wms_stock_ledger",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_stock_ledger_TenantId_IdempotencyKey",
                table: "supplyarr_wms_stock_ledger",
                columns: new[] { "TenantId", "IdempotencyKey" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_stock_ledger_TenantId_MovementGroupId",
                table: "supplyarr_wms_stock_ledger",
                columns: new[] { "TenantId", "MovementGroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_stock_ledger_TenantId_PartId_InventoryBinId_C~",
                table: "supplyarr_wms_stock_ledger",
                columns: new[] { "TenantId", "PartId", "InventoryBinId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_wms_outbound_shipment_lines");

            migrationBuilder.DropTable(
                name: "supplyarr_wms_stock_ledger");

            migrationBuilder.DropTable(
                name: "supplyarr_wms_outbound_shipments");

            migrationBuilder.DropIndex(
                name: "IX_supplyarr_inventory_locations_TenantId_StaffarrSiteOrgUnitId",
                table: "supplyarr_inventory_locations");

            migrationBuilder.DropColumn(
                name: "StaffarrSiteNameSnapshot",
                table: "supplyarr_inventory_locations");

            migrationBuilder.DropColumn(
                name: "StaffarrSiteOrgUnitId",
                table: "supplyarr_inventory_locations");

            migrationBuilder.DropColumn(
                name: "StaffarrSiteResolutionStatus",
                table: "supplyarr_inventory_locations");
        }
    }
}
