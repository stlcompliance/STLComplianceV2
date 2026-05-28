using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrSupplierOnboarding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_party_supplier_onboarding",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    OnboardingStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_party_supplier_onboarding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_party_supplier_onboarding_supplyarr_external_part~",
                        column: x => x.ExternalPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_tenant_supplier_onboarding_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequiredDocumentTypeKeysJson = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_tenant_supplier_onboarding_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_supplier_onboarding_ExternalPartyId",
                table: "supplyarr_party_supplier_onboarding",
                column: "ExternalPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_supplier_onboarding_TenantId",
                table: "supplyarr_party_supplier_onboarding",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_supplier_onboarding_TenantId_ExternalPartyId",
                table: "supplyarr_party_supplier_onboarding",
                columns: new[] { "TenantId", "ExternalPartyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_supplier_onboarding_TenantId_OnboardingStat~",
                table: "supplyarr_party_supplier_onboarding",
                columns: new[] { "TenantId", "OnboardingStatus", "SubmittedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_tenant_supplier_onboarding_settings_TenantId",
                table: "supplyarr_tenant_supplier_onboarding_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_party_supplier_onboarding");

            migrationBuilder.DropTable(
                name: "supplyarr_tenant_supplier_onboarding_settings");
        }
    }
}
