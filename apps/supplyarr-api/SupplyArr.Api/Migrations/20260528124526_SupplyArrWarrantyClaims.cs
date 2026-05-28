using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrWarrantyClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_warranty_claims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ClaimType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    VendorPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseOrderLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivingReceiptId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivingReceiptLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuantityClaimed = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ProblemDescription = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    VendorRmaNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    VendorDisposition = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    VendorResponseNotes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ClosureNotes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    DenialReason = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VendorRespondedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VendorRespondedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeniedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeniedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_warranty_claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_warranty_claims_supplyarr_external_parties_Vendor~",
                        column: x => x.VendorPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_warranty_claims_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_warranty_claims_supplyarr_purchase_order_lines_Pu~",
                        column: x => x.PurchaseOrderLineId,
                        principalTable: "supplyarr_purchase_order_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_supplyarr_warranty_claims_supplyarr_purchase_orders_Purchas~",
                        column: x => x.PurchaseOrderId,
                        principalTable: "supplyarr_purchase_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_supplyarr_warranty_claims_supplyarr_receiving_receipt_lines~",
                        column: x => x.ReceivingReceiptLineId,
                        principalTable: "supplyarr_receiving_receipt_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_supplyarr_warranty_claims_supplyarr_receiving_receipts_Rece~",
                        column: x => x.ReceivingReceiptId,
                        principalTable: "supplyarr_receiving_receipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_warranty_claims_PartId",
                table: "supplyarr_warranty_claims",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_warranty_claims_PurchaseOrderId",
                table: "supplyarr_warranty_claims",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_warranty_claims_PurchaseOrderLineId",
                table: "supplyarr_warranty_claims",
                column: "PurchaseOrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_warranty_claims_ReceivingReceiptId",
                table: "supplyarr_warranty_claims",
                column: "ReceivingReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_warranty_claims_ReceivingReceiptLineId",
                table: "supplyarr_warranty_claims",
                column: "ReceivingReceiptLineId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_warranty_claims_TenantId",
                table: "supplyarr_warranty_claims",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_warranty_claims_TenantId_ClaimKey",
                table: "supplyarr_warranty_claims",
                columns: new[] { "TenantId", "ClaimKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_warranty_claims_TenantId_PartId",
                table: "supplyarr_warranty_claims",
                columns: new[] { "TenantId", "PartId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_warranty_claims_TenantId_PurchaseOrderId",
                table: "supplyarr_warranty_claims",
                columns: new[] { "TenantId", "PurchaseOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_warranty_claims_TenantId_Status_UpdatedAt",
                table: "supplyarr_warranty_claims",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_warranty_claims_TenantId_VendorPartyId",
                table: "supplyarr_warranty_claims",
                columns: new[] { "TenantId", "VendorPartyId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_warranty_claims_VendorPartyId",
                table: "supplyarr_warranty_claims",
                column: "VendorPartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_warranty_claims");
        }
    }
}
