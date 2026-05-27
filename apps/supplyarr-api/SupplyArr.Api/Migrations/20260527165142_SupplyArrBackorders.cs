using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrBackorders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_backorders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BackorderKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseOrderLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseRequestLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivingReceiptId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivingReceiptLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityBackordered = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityFulfilled = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ExpectedBy = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FulfilledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    FulfilledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_backorders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_backorders_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_backorders_supplyarr_purchase_order_lines_Purchas~",
                        column: x => x.PurchaseOrderLineId,
                        principalTable: "supplyarr_purchase_order_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_backorders_supplyarr_purchase_orders_PurchaseOrde~",
                        column: x => x.PurchaseOrderId,
                        principalTable: "supplyarr_purchase_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_backorders_PartId",
                table: "supplyarr_backorders",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_backorders_PurchaseOrderId",
                table: "supplyarr_backorders",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_backorders_PurchaseOrderLineId",
                table: "supplyarr_backorders",
                column: "PurchaseOrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_backorders_TenantId",
                table: "supplyarr_backorders",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_backorders_TenantId_BackorderKey",
                table: "supplyarr_backorders",
                columns: new[] { "TenantId", "BackorderKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_backorders_TenantId_PartId_Status",
                table: "supplyarr_backorders",
                columns: new[] { "TenantId", "PartId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_backorders_TenantId_PurchaseOrderId",
                table: "supplyarr_backorders",
                columns: new[] { "TenantId", "PurchaseOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_backorders_TenantId_PurchaseOrderLineId_Status",
                table: "supplyarr_backorders",
                columns: new[] { "TenantId", "PurchaseOrderLineId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_backorders");
        }
    }
}
