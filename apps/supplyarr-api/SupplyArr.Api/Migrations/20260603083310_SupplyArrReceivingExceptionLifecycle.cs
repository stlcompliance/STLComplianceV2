using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrReceivingExceptionLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "supplyarr_receiving_exceptions",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledAt",
                table: "supplyarr_receiving_exceptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CancelledByUserId",
                table: "supplyarr_receiving_exceptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastReopenReason",
                table: "supplyarr_receiving_exceptions",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ReopenCount",
                table: "supplyarr_receiving_exceptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReopenedAt",
                table: "supplyarr_receiving_exceptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReopenedByUserId",
                table: "supplyarr_receiving_exceptions",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "supplyarr_receiving_exceptions");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "supplyarr_receiving_exceptions");

            migrationBuilder.DropColumn(
                name: "CancelledByUserId",
                table: "supplyarr_receiving_exceptions");

            migrationBuilder.DropColumn(
                name: "LastReopenReason",
                table: "supplyarr_receiving_exceptions");

            migrationBuilder.DropColumn(
                name: "ReopenCount",
                table: "supplyarr_receiving_exceptions");

            migrationBuilder.DropColumn(
                name: "ReopenedAt",
                table: "supplyarr_receiving_exceptions");

            migrationBuilder.DropColumn(
                name: "ReopenedByUserId",
                table: "supplyarr_receiving_exceptions");
        }
    }
}
