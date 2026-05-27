using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrPurchaseOrderFoundations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_purchase_orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IssuedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IssuedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_purchase_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_purchase_orders_supplyarr_external_parties_Vendor~",
                        column: x => x.VendorPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_purchase_orders_supplyarr_purchase_requests_Purch~",
                        column: x => x.PurchaseRequestId,
                        principalTable: "supplyarr_purchase_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_purchase_order_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseRequestLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityOrdered = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_purchase_order_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_purchase_order_lines_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_purchase_order_lines_supplyarr_purchase_orders_Pu~",
                        column: x => x.PurchaseOrderId,
                        principalTable: "supplyarr_purchase_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_supplyarr_purchase_order_lines_supplyarr_purchase_request_l~",
                        column: x => x.PurchaseRequestLineId,
                        principalTable: "supplyarr_purchase_request_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_order_lines_PartId",
                table: "supplyarr_purchase_order_lines",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_order_lines_PurchaseOrderId",
                table: "supplyarr_purchase_order_lines",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_order_lines_PurchaseRequestLineId",
                table: "supplyarr_purchase_order_lines",
                column: "PurchaseRequestLineId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_order_lines_TenantId",
                table: "supplyarr_purchase_order_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_order_lines_TenantId_PurchaseOrderId",
                table: "supplyarr_purchase_order_lines",
                columns: new[] { "TenantId", "PurchaseOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_order_lines_TenantId_PurchaseOrderId_Lin~",
                table: "supplyarr_purchase_order_lines",
                columns: new[] { "TenantId", "PurchaseOrderId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_orders_PurchaseRequestId",
                table: "supplyarr_purchase_orders",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_orders_TenantId",
                table: "supplyarr_purchase_orders",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_orders_TenantId_OrderKey",
                table: "supplyarr_purchase_orders",
                columns: new[] { "TenantId", "OrderKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_orders_TenantId_PurchaseRequestId",
                table: "supplyarr_purchase_orders",
                columns: new[] { "TenantId", "PurchaseRequestId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_orders_TenantId_Status_UpdatedAt",
                table: "supplyarr_purchase_orders",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_orders_VendorPartyId",
                table: "supplyarr_purchase_orders",
                column: "VendorPartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_purchase_order_lines");

            migrationBuilder.DropTable(
                name: "supplyarr_purchase_orders");
        }
    }
}
