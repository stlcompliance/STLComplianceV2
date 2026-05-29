using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrPartStockReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_part_stock_reservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReservationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryBinId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartStockLevelId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityReserved = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FulfilledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    FulfilledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReleasedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReleasedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReleaseReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_part_stock_reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_stock_reservations_supplyarr_inventory_bins_~",
                        column: x => x.InventoryBinId,
                        principalTable: "supplyarr_inventory_bins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_stock_reservations_supplyarr_part_stock_leve~",
                        column: x => x.PartStockLevelId,
                        principalTable: "supplyarr_part_stock_levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_stock_reservations_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_reservations_InventoryBinId",
                table: "supplyarr_part_stock_reservations",
                column: "InventoryBinId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_reservations_PartId",
                table: "supplyarr_part_stock_reservations",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_reservations_PartStockLevelId",
                table: "supplyarr_part_stock_reservations",
                column: "PartStockLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_reservations_TenantId",
                table: "supplyarr_part_stock_reservations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_reservations_TenantId_InventoryBinId_S~",
                table: "supplyarr_part_stock_reservations",
                columns: new[] { "TenantId", "InventoryBinId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_reservations_TenantId_PartId_Status",
                table: "supplyarr_part_stock_reservations",
                columns: new[] { "TenantId", "PartId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_reservations_TenantId_ReservationKey",
                table: "supplyarr_part_stock_reservations",
                columns: new[] { "TenantId", "ReservationKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_part_stock_reservations");
        }
    }
}
