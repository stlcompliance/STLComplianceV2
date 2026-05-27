using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrReceivingFoundations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "QuantityReceived",
                table: "supplyarr_purchase_order_lines",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "supplyarr_receiving_receipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiptKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryBinId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PostedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_receiving_receipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_receiving_receipts_supplyarr_inventory_bins_Inven~",
                        column: x => x.InventoryBinId,
                        principalTable: "supplyarr_inventory_bins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_receiving_receipts_supplyarr_purchase_orders_Purc~",
                        column: x => x.PurchaseOrderId,
                        principalTable: "supplyarr_purchase_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_receiving_receipt_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivingReceiptId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseOrderLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    QuantityReceived = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_receiving_receipt_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_receiving_receipt_lines_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_receiving_receipt_lines_supplyarr_purchase_order_~",
                        column: x => x.PurchaseOrderLineId,
                        principalTable: "supplyarr_purchase_order_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_receiving_receipt_lines_supplyarr_receiving_recei~",
                        column: x => x.ReceivingReceiptId,
                        principalTable: "supplyarr_receiving_receipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_receipt_lines_PartId",
                table: "supplyarr_receiving_receipt_lines",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_receipt_lines_PurchaseOrderLineId",
                table: "supplyarr_receiving_receipt_lines",
                column: "PurchaseOrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_receipt_lines_ReceivingReceiptId",
                table: "supplyarr_receiving_receipt_lines",
                column: "ReceivingReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_receipt_lines_TenantId",
                table: "supplyarr_receiving_receipt_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_receipt_lines_TenantId_ReceivingReceip~1",
                table: "supplyarr_receiving_receipt_lines",
                columns: new[] { "TenantId", "ReceivingReceiptId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_receipt_lines_TenantId_ReceivingReceipt~",
                table: "supplyarr_receiving_receipt_lines",
                columns: new[] { "TenantId", "ReceivingReceiptId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_receipts_InventoryBinId",
                table: "supplyarr_receiving_receipts",
                column: "InventoryBinId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_receipts_PurchaseOrderId",
                table: "supplyarr_receiving_receipts",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_receipts_TenantId",
                table: "supplyarr_receiving_receipts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_receipts_TenantId_PurchaseOrderId",
                table: "supplyarr_receiving_receipts",
                columns: new[] { "TenantId", "PurchaseOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_receipts_TenantId_ReceiptKey",
                table: "supplyarr_receiving_receipts",
                columns: new[] { "TenantId", "ReceiptKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_receipts_TenantId_Status_UpdatedAt",
                table: "supplyarr_receiving_receipts",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_receiving_receipt_lines");

            migrationBuilder.DropTable(
                name: "supplyarr_receiving_receipts");

            migrationBuilder.DropColumn(
                name: "QuantityReceived",
                table: "supplyarr_purchase_order_lines");
        }
    }
}
