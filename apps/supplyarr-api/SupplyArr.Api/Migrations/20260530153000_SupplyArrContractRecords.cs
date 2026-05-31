using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrContractRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ContractType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    VendorPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RenewalAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PaymentTerms = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FreightTerms = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    WarrantyTerms = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    MinimumSpend = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ServiceLevelAgreement = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ApprovalStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_contracts_supplyarr_external_parties_VendorParty~",
                        column: x => x.VendorPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_contracts_TenantId",
                table: "supplyarr_contracts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_contracts_TenantId_ContractKey",
                table: "supplyarr_contracts",
                columns: new[] { "TenantId", "ContractKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_contracts_TenantId_Status_ExpiresAt",
                table: "supplyarr_contracts",
                columns: new[] { "TenantId", "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_contracts_TenantId_VendorPartyId",
                table: "supplyarr_contracts",
                columns: new[] { "TenantId", "VendorPartyId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_contracts_VendorPartyId",
                table: "supplyarr_contracts",
                column: "VendorPartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "supplyarr_contracts");
        }
    }
}
