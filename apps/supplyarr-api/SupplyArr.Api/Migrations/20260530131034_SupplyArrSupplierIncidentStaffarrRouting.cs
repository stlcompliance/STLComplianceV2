using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrSupplierIncidentStaffarrRouting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InvolvedStaffarrPersonId",
                table: "supplyarr_supplier_incidents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StaffarrIncidentRouteStatus",
                table: "supplyarr_supplier_incidents",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StaffarrIncidentRoutedAt",
                table: "supplyarr_supplier_incidents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StaffarrPersonnelIncidentId",
                table: "supplyarr_supplier_incidents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_supplier_incidents_staffarr_incident",
                table: "supplyarr_supplier_incidents",
                columns: new[] { "TenantId", "StaffarrPersonnelIncidentId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_supplier_incidents_staffarr_person",
                table: "supplyarr_supplier_incidents",
                columns: new[] { "TenantId", "InvolvedStaffarrPersonId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_supplyarr_supplier_incidents_staffarr_incident",
                table: "supplyarr_supplier_incidents");

            migrationBuilder.DropIndex(
                name: "IX_supplyarr_supplier_incidents_staffarr_person",
                table: "supplyarr_supplier_incidents");

            migrationBuilder.DropColumn(
                name: "InvolvedStaffarrPersonId",
                table: "supplyarr_supplier_incidents");

            migrationBuilder.DropColumn(
                name: "StaffarrIncidentRouteStatus",
                table: "supplyarr_supplier_incidents");

            migrationBuilder.DropColumn(
                name: "StaffarrIncidentRoutedAt",
                table: "supplyarr_supplier_incidents");

            migrationBuilder.DropColumn(
                name: "StaffarrPersonnelIncidentId",
                table: "supplyarr_supplier_incidents");
        }
    }
}
