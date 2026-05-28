using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrStaffarrProcurementApprovalAuthority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplyarr_staffarr_procurement_approval_authority_mirrors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CanSubmitPurchaseRequests = table.Column<bool>(type: "boolean", nullable: false),
                    CanApprovePurchaseRequests = table.Column<bool>(type: "boolean", nullable: false),
                    CanIssuePurchaseOrders = table.Column<bool>(type: "boolean", nullable: false),
                    MaxSubmitAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    MaxApproveAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    MaxIssueAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    OrgUnitScopeIdsJson = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    GrantsJson = table.Column<string>(type: "character varying(16384)", maxLength: 16384, nullable: false),
                    SourceComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RefreshedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_staffarr_procurement_approval_authority_mirrors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_procurement_approval_authority_mirrors_~1",
                table: "supplyarr_staffarr_procurement_approval_authority_mirrors",
                columns: new[] { "TenantId", "ExternalUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_procurement_approval_authority_mirrors_~2",
                table: "supplyarr_staffarr_procurement_approval_authority_mirrors",
                columns: new[] { "TenantId", "RefreshedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_procurement_approval_authority_mirrors_~3",
                table: "supplyarr_staffarr_procurement_approval_authority_mirrors",
                columns: new[] { "TenantId", "StaffarrPersonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_procurement_approval_authority_mirrors_T~",
                table: "supplyarr_staffarr_procurement_approval_authority_mirrors",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplyarr_staffarr_procurement_approval_authority_mirrors");
        }
    }
}
