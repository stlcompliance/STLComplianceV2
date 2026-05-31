using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrPendingModelSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InvoiceFileName",
                table: "supplyarr_receiving_receipts",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InvoiceReference",
                table: "supplyarr_receiving_receipts",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PackingSlipFileName",
                table: "supplyarr_receiving_receipts",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PackingSlipReference",
                table: "supplyarr_receiving_receipts",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Condition",
                table: "supplyarr_receiving_receipt_lines",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SerialLotNumbersJson",
                table: "supplyarr_receiving_receipt_lines",
                type: "character varying(4096)",
                maxLength: 4096,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "RequiresSerialLotTracking",
                table: "supplyarr_parts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceFileName",
                table: "supplyarr_receiving_receipts");

            migrationBuilder.DropColumn(
                name: "InvoiceReference",
                table: "supplyarr_receiving_receipts");

            migrationBuilder.DropColumn(
                name: "PackingSlipFileName",
                table: "supplyarr_receiving_receipts");

            migrationBuilder.DropColumn(
                name: "PackingSlipReference",
                table: "supplyarr_receiving_receipts");

            migrationBuilder.DropColumn(
                name: "Condition",
                table: "supplyarr_receiving_receipt_lines");

            migrationBuilder.DropColumn(
                name: "SerialLotNumbersJson",
                table: "supplyarr_receiving_receipt_lines");

            migrationBuilder.DropColumn(
                name: "RequiresSerialLotTracking",
                table: "supplyarr_parts");
        }
    }
}
