using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrProcurementExceptionResolutionDepth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LinkedPurchaseOrderId",
                table: "supplyarr_procurement_exceptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LinkedPurchaseRequestId",
                table: "supplyarr_procurement_exceptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResolutionTemplateKey",
                table: "supplyarr_procurement_exceptions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SlaDueAt",
                table: "supplyarr_procurement_exceptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_exceptions_TenantId_SlaDueAt",
                table: "supplyarr_procurement_exceptions",
                columns: new[] { "TenantId", "SlaDueAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_supplyarr_procurement_exceptions_TenantId_SlaDueAt",
                table: "supplyarr_procurement_exceptions");

            migrationBuilder.DropColumn(
                name: "LinkedPurchaseOrderId",
                table: "supplyarr_procurement_exceptions");

            migrationBuilder.DropColumn(
                name: "LinkedPurchaseRequestId",
                table: "supplyarr_procurement_exceptions");

            migrationBuilder.DropColumn(
                name: "ResolutionTemplateKey",
                table: "supplyarr_procurement_exceptions");

            migrationBuilder.DropColumn(
                name: "SlaDueAt",
                table: "supplyarr_procurement_exceptions");
        }
    }
}
