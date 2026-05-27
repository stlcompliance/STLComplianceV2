using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrPurchaseRequestFoundations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_purchase_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    VendorPartyId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_purchase_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_purchase_requests_supplyarr_external_parties_Vend~",
                        column: x => x.VendorPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_purchase_request_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityRequested = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_purchase_request_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_purchase_request_lines_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_purchase_request_lines_supplyarr_purchase_request~",
                        column: x => x.PurchaseRequestId,
                        principalTable: "supplyarr_purchase_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_request_lines_PartId",
                table: "supplyarr_purchase_request_lines",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_request_lines_PurchaseRequestId",
                table: "supplyarr_purchase_request_lines",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_request_lines_TenantId",
                table: "supplyarr_purchase_request_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_request_lines_TenantId_PurchaseRequestId",
                table: "supplyarr_purchase_request_lines",
                columns: new[] { "TenantId", "PurchaseRequestId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_request_lines_TenantId_PurchaseRequestId~",
                table: "supplyarr_purchase_request_lines",
                columns: new[] { "TenantId", "PurchaseRequestId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_requests_TenantId",
                table: "supplyarr_purchase_requests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_requests_TenantId_RequestKey",
                table: "supplyarr_purchase_requests",
                columns: new[] { "TenantId", "RequestKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_requests_TenantId_Status_UpdatedAt",
                table: "supplyarr_purchase_requests",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_requests_VendorPartyId",
                table: "supplyarr_purchase_requests",
                column: "VendorPartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_purchase_request_lines");

            migrationBuilder.DropTable(
                name: "supplyarr_purchase_requests");
        }
    }
}
