using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrVendorPortalAccessCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PortalAccessCode",
                table: "supplyarr_rfq_vendor_invitations",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PortalAccessCodeIssuedAt",
                table: "supplyarr_rfq_vendor_invitations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: DateTimeOffset.UnixEpoch);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PortalAccessCodeExpiresAt",
                table: "supplyarr_rfq_vendor_invitations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: DateTimeOffset.UnixEpoch);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfq_vendor_invitations_TenantId_RfqId_PortalAccess",
                table: "supplyarr_rfq_vendor_invitations",
                columns: new[] { "TenantId", "RfqId", "PortalAccessCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_supplyarr_rfq_vendor_invitations_TenantId_RfqId_PortalAccess",
                table: "supplyarr_rfq_vendor_invitations");

            migrationBuilder.DropColumn(
                name: "PortalAccessCode",
                table: "supplyarr_rfq_vendor_invitations");

            migrationBuilder.DropColumn(
                name: "PortalAccessCodeIssuedAt",
                table: "supplyarr_rfq_vendor_invitations");

            migrationBuilder.DropColumn(
                name: "PortalAccessCodeExpiresAt",
                table: "supplyarr_rfq_vendor_invitations");
        }
    }
}
