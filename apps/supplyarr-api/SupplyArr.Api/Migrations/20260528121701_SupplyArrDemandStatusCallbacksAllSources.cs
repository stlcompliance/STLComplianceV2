using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupplyArrDemandStatusCallbacksAllSources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastStatusCallbackAt",
                table: "supplyarr_trainarr_demand_refs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastStatusCallbackAt",
                table: "supplyarr_staffarr_demand_refs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastStatusCallbackAt",
                table: "supplyarr_routarr_demand_refs",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastStatusCallbackAt",
                table: "supplyarr_trainarr_demand_refs");

            migrationBuilder.DropColumn(
                name: "LastStatusCallbackAt",
                table: "supplyarr_staffarr_demand_refs");

            migrationBuilder.DropColumn(
                name: "LastStatusCallbackAt",
                table: "supplyarr_routarr_demand_refs");
        }
    }
}
