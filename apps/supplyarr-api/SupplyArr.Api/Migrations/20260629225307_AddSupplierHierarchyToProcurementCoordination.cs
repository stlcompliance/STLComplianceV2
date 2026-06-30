using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierHierarchyToProcurementCoordination : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParentSupplierDisplayName",
                table: "supplyarr_procurement_coordination_records",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentSupplierId",
                table: "supplyarr_procurement_coordination_records",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierDisplayName",
                table: "supplyarr_procurement_coordination_records",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SupplierId",
                table: "supplyarr_procurement_coordination_records",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierKey",
                table: "supplyarr_procurement_coordination_records",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierServiceTypesJson",
                table: "supplyarr_procurement_coordination_records",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "SupplierUnitKind",
                table: "supplyarr_procurement_coordination_records",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentSupplierDisplayName",
                table: "supplyarr_procurement_coordination_records");

            migrationBuilder.DropColumn(
                name: "ParentSupplierId",
                table: "supplyarr_procurement_coordination_records");

            migrationBuilder.DropColumn(
                name: "SupplierDisplayName",
                table: "supplyarr_procurement_coordination_records");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "supplyarr_procurement_coordination_records");

            migrationBuilder.DropColumn(
                name: "SupplierKey",
                table: "supplyarr_procurement_coordination_records");

            migrationBuilder.DropColumn(
                name: "SupplierServiceTypesJson",
                table: "supplyarr_procurement_coordination_records");

            migrationBuilder.DropColumn(
                name: "SupplierUnitKind",
                table: "supplyarr_procurement_coordination_records");
        }
    }
}
