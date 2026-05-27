using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrVendorReturns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_vendor_returns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReturnKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    VendorPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    InventoryBinId = table.Column<Guid>(type: "uuid", nullable: false),
                    RmaNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PostedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_vendor_returns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_returns_supplyarr_external_parties_VendorP~",
                        column: x => x.VendorPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_returns_supplyarr_inventory_bins_Inventory~",
                        column: x => x.InventoryBinId,
                        principalTable: "supplyarr_inventory_bins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_returns_supplyarr_purchase_orders_Purchase~",
                        column: x => x.PurchaseOrderId,
                        principalTable: "supplyarr_purchase_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_vendor_return_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorReturnId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseOrderLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_vendor_return_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_return_lines_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_return_lines_supplyarr_purchase_order_line~",
                        column: x => x.PurchaseOrderLineId,
                        principalTable: "supplyarr_purchase_order_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_return_lines_supplyarr_vendor_returns_Vend~",
                        column: x => x.VendorReturnId,
                        principalTable: "supplyarr_vendor_returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_return_lines_PartId",
                table: "supplyarr_vendor_return_lines",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_return_lines_PurchaseOrderLineId",
                table: "supplyarr_vendor_return_lines",
                column: "PurchaseOrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_return_lines_TenantId",
                table: "supplyarr_vendor_return_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_return_lines_TenantId_PartId",
                table: "supplyarr_vendor_return_lines",
                columns: new[] { "TenantId", "PartId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_return_lines_TenantId_VendorReturnId",
                table: "supplyarr_vendor_return_lines",
                columns: new[] { "TenantId", "VendorReturnId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_return_lines_TenantId_VendorReturnId_LineN~",
                table: "supplyarr_vendor_return_lines",
                columns: new[] { "TenantId", "VendorReturnId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_return_lines_VendorReturnId",
                table: "supplyarr_vendor_return_lines",
                column: "VendorReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_returns_InventoryBinId",
                table: "supplyarr_vendor_returns",
                column: "InventoryBinId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_returns_PurchaseOrderId",
                table: "supplyarr_vendor_returns",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_returns_TenantId",
                table: "supplyarr_vendor_returns",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_returns_TenantId_PurchaseOrderId",
                table: "supplyarr_vendor_returns",
                columns: new[] { "TenantId", "PurchaseOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_returns_TenantId_ReturnKey",
                table: "supplyarr_vendor_returns",
                columns: new[] { "TenantId", "ReturnKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_returns_TenantId_Status_UpdatedAt",
                table: "supplyarr_vendor_returns",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_returns_TenantId_VendorPartyId",
                table: "supplyarr_vendor_returns",
                columns: new[] { "TenantId", "VendorPartyId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_returns_VendorPartyId",
                table: "supplyarr_vendor_returns",
                column: "VendorPartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_vendor_return_lines");

            migrationBuilder.DropTable(
                name: "supplyarr_vendor_returns");
        }
    }
}
