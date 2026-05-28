using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrRfqQuoteComparison : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_rfqs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RfqKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AwardedVendorPartyId = table.Column<Guid>(type: "uuid", nullable: true),
                    SelectedVendorQuoteId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    AwardedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AwardedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_rfqs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_rfqs_supplyarr_external_parties_AwardedVendorPart~",
                        column: x => x.AwardedVendorPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_rfq_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RfqId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityRequested = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_rfq_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_rfq_lines_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_rfq_lines_supplyarr_rfqs_RfqId",
                        column: x => x.RfqId,
                        principalTable: "supplyarr_rfqs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_rfq_vendor_invitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RfqId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    InvitedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    InvitedByUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_rfq_vendor_invitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_rfq_vendor_invitations_supplyarr_external_parties~",
                        column: x => x.VendorPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_rfq_vendor_invitations_supplyarr_rfqs_RfqId",
                        column: x => x.RfqId,
                        principalTable: "supplyarr_rfqs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_vendor_quotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RfqId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuoteKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    LeadTimeDays = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_vendor_quotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_quotes_supplyarr_external_parties_VendorPa~",
                        column: x => x.VendorPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_quotes_supplyarr_rfqs_RfqId",
                        column: x => x.RfqId,
                        principalTable: "supplyarr_rfqs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_vendor_quote_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorQuoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    RfqLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    QuantityQuoted = table.Column<decimal>(type: "numeric", nullable: false),
                    LeadTimeDays = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_vendor_quote_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_quote_lines_supplyarr_rfq_lines_RfqLineId",
                        column: x => x.RfqLineId,
                        principalTable: "supplyarr_rfq_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_quote_lines_supplyarr_vendor_quotes_Vendor~",
                        column: x => x.VendorQuoteId,
                        principalTable: "supplyarr_vendor_quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfq_lines_PartId",
                table: "supplyarr_rfq_lines",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfq_lines_RfqId",
                table: "supplyarr_rfq_lines",
                column: "RfqId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfq_lines_TenantId",
                table: "supplyarr_rfq_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfq_lines_TenantId_RfqId_LineNumber",
                table: "supplyarr_rfq_lines",
                columns: new[] { "TenantId", "RfqId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfq_vendor_invitations_RfqId",
                table: "supplyarr_rfq_vendor_invitations",
                column: "RfqId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfq_vendor_invitations_TenantId",
                table: "supplyarr_rfq_vendor_invitations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfq_vendor_invitations_TenantId_RfqId_VendorParty~",
                table: "supplyarr_rfq_vendor_invitations",
                columns: new[] { "TenantId", "RfqId", "VendorPartyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfq_vendor_invitations_VendorPartyId",
                table: "supplyarr_rfq_vendor_invitations",
                column: "VendorPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfqs_AwardedVendorPartyId",
                table: "supplyarr_rfqs",
                column: "AwardedVendorPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfqs_TenantId",
                table: "supplyarr_rfqs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfqs_TenantId_RfqKey",
                table: "supplyarr_rfqs",
                columns: new[] { "TenantId", "RfqKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfqs_TenantId_Status_UpdatedAt",
                table: "supplyarr_rfqs",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_quote_lines_RfqLineId",
                table: "supplyarr_vendor_quote_lines",
                column: "RfqLineId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_quote_lines_TenantId",
                table: "supplyarr_vendor_quote_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_quote_lines_TenantId_VendorQuoteId_RfqLine~",
                table: "supplyarr_vendor_quote_lines",
                columns: new[] { "TenantId", "VendorQuoteId", "RfqLineId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_quote_lines_VendorQuoteId",
                table: "supplyarr_vendor_quote_lines",
                column: "VendorQuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_quotes_RfqId",
                table: "supplyarr_vendor_quotes",
                column: "RfqId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_quotes_TenantId",
                table: "supplyarr_vendor_quotes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_quotes_TenantId_RfqId_QuoteKey",
                table: "supplyarr_vendor_quotes",
                columns: new[] { "TenantId", "RfqId", "QuoteKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_quotes_TenantId_RfqId_VendorPartyId",
                table: "supplyarr_vendor_quotes",
                columns: new[] { "TenantId", "RfqId", "VendorPartyId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_quotes_VendorPartyId",
                table: "supplyarr_vendor_quotes",
                column: "VendorPartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_rfq_vendor_invitations");

            migrationBuilder.DropTable(
                name: "supplyarr_vendor_quote_lines");

            migrationBuilder.DropTable(
                name: "supplyarr_rfq_lines");

            migrationBuilder.DropTable(
                name: "supplyarr_vendor_quotes");

            migrationBuilder.DropTable(
                name: "supplyarr_rfqs");
        }
    }
}
