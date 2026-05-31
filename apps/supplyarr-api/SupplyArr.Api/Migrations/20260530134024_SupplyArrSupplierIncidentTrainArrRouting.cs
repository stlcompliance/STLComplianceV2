using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrSupplierIncidentTrainArrRouting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TrainarrIncidentRemediationId",
                table: "supplyarr_supplier_incidents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrainarrIncidentRouteStatus",
                table: "supplyarr_supplier_incidents",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TrainarrIncidentRoutedAt",
                table: "supplyarr_supplier_incidents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_supplier_incidents_trainarr_remediation",
                table: "supplyarr_supplier_incidents",
                columns: new[] { "TenantId", "TrainarrIncidentRemediationId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_supplyarr_supplier_incidents_trainarr_remediation",
                table: "supplyarr_supplier_incidents");

            migrationBuilder.DropColumn(
                name: "TrainarrIncidentRemediationId",
                table: "supplyarr_supplier_incidents");

            migrationBuilder.DropColumn(
                name: "TrainarrIncidentRouteStatus",
                table: "supplyarr_supplier_incidents");

            migrationBuilder.DropColumn(
                name: "TrainarrIncidentRoutedAt",
                table: "supplyarr_supplier_incidents");
        }
    }
}
