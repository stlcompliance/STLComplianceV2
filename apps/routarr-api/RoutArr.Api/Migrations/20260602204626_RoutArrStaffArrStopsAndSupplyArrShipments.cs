using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class RoutArrStaffArrStopsAndSupplyArrShipments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StaffarrSiteNameSnapshot",
                table: "routarr_route_stops",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "StaffarrSiteOrgUnitId",
                table: "routarr_route_stops",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "routarr_supplyarr_shipment_intents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyarrShipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipmentKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DestinationName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DestinationAddressSnapshot = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RouteId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_supplyarr_shipment_intents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_supplyarr_shipment_intent_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipmentIntentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyarrShipmentLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartDisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_supplyarr_shipment_intent_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_supplyarr_shipment_intent_lines_routarr_supplyarr_s~",
                        column: x => x.ShipmentIntentId,
                        principalTable: "routarr_supplyarr_shipment_intents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_route_stops_TenantId_StaffarrSiteOrgUnitId",
                table: "routarr_route_stops",
                columns: new[] { "TenantId", "StaffarrSiteOrgUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_supplyarr_shipment_intent_lines_ShipmentIntentId",
                table: "routarr_supplyarr_shipment_intent_lines",
                column: "ShipmentIntentId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_supplyarr_shipment_intent_lines_TenantId",
                table: "routarr_supplyarr_shipment_intent_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_supplyarr_shipment_intent_lines_TenantId_ShipmentIn~",
                table: "routarr_supplyarr_shipment_intent_lines",
                columns: new[] { "TenantId", "ShipmentIntentId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_supplyarr_shipment_intents_TenantId",
                table: "routarr_supplyarr_shipment_intents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_supplyarr_shipment_intents_TenantId_Status_UpdatedAt",
                table: "routarr_supplyarr_shipment_intents",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_supplyarr_shipment_intents_TenantId_SupplyarrShipme~",
                table: "routarr_supplyarr_shipment_intents",
                columns: new[] { "TenantId", "SupplyarrShipmentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "routarr_supplyarr_shipment_intent_lines");

            migrationBuilder.DropTable(
                name: "routarr_supplyarr_shipment_intents");

            migrationBuilder.DropIndex(
                name: "IX_routarr_route_stops_TenantId_StaffarrSiteOrgUnitId",
                table: "routarr_route_stops");

            migrationBuilder.DropColumn(
                name: "StaffarrSiteNameSnapshot",
                table: "routarr_route_stops");

            migrationBuilder.DropColumn(
                name: "StaffarrSiteOrgUnitId",
                table: "routarr_route_stops");
        }
    }
}
