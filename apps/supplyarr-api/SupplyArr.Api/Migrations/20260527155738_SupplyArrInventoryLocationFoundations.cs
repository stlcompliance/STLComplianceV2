using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrInventoryLocationFoundations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_inventory_locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LocationType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AddressLine = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_inventory_locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_inventory_bins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    BinKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_inventory_bins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_inventory_bins_supplyarr_inventory_locations_Inve~",
                        column: x => x.InventoryLocationId,
                        principalTable: "supplyarr_inventory_locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_part_stock_levels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryBinId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityOnHand = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityReserved = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_part_stock_levels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_stock_levels_supplyarr_inventory_bins_Invent~",
                        column: x => x.InventoryBinId,
                        principalTable: "supplyarr_inventory_bins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_stock_levels_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_inventory_bins_InventoryLocationId",
                table: "supplyarr_inventory_bins",
                column: "InventoryLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_inventory_bins_TenantId",
                table: "supplyarr_inventory_bins",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_inventory_bins_TenantId_InventoryLocationId",
                table: "supplyarr_inventory_bins",
                columns: new[] { "TenantId", "InventoryLocationId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_inventory_bins_TenantId_InventoryLocationId_BinKey",
                table: "supplyarr_inventory_bins",
                columns: new[] { "TenantId", "InventoryLocationId", "BinKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_inventory_locations_TenantId",
                table: "supplyarr_inventory_locations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_inventory_locations_TenantId_LocationKey",
                table: "supplyarr_inventory_locations",
                columns: new[] { "TenantId", "LocationKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_inventory_locations_TenantId_LocationType_Status",
                table: "supplyarr_inventory_locations",
                columns: new[] { "TenantId", "LocationType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_levels_InventoryBinId",
                table: "supplyarr_part_stock_levels",
                column: "InventoryBinId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_levels_PartId",
                table: "supplyarr_part_stock_levels",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_levels_TenantId",
                table: "supplyarr_part_stock_levels",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_levels_TenantId_InventoryBinId",
                table: "supplyarr_part_stock_levels",
                columns: new[] { "TenantId", "InventoryBinId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_levels_TenantId_PartId",
                table: "supplyarr_part_stock_levels",
                columns: new[] { "TenantId", "PartId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_levels_TenantId_PartId_InventoryBinId",
                table: "supplyarr_part_stock_levels",
                columns: new[] { "TenantId", "PartId", "InventoryBinId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_part_stock_levels");

            migrationBuilder.DropTable(
                name: "supplyarr_inventory_bins");

            migrationBuilder.DropTable(
                name: "supplyarr_inventory_locations");
        }
    }
}
