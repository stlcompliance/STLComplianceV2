using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class RoutArrTripDriverClose : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ClosedAt",
                table: "routarr_trips",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "routarr_trips");
        }
    }
}
