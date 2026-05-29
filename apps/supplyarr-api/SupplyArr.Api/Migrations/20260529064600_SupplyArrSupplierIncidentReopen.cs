using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrSupplierIncidentReopen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastReopenReason",
                table: "supplyarr_supplier_incidents",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ReopenCount",
                table: "supplyarr_supplier_incidents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReopenedAt",
                table: "supplyarr_supplier_incidents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReopenedByUserId",
                table: "supplyarr_supplier_incidents",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastReopenReason",
                table: "supplyarr_supplier_incidents");

            migrationBuilder.DropColumn(
                name: "ReopenCount",
                table: "supplyarr_supplier_incidents");

            migrationBuilder.DropColumn(
                name: "ReopenedAt",
                table: "supplyarr_supplier_incidents");

            migrationBuilder.DropColumn(
                name: "ReopenedByUserId",
                table: "supplyarr_supplier_incidents");
        }
    }
}
