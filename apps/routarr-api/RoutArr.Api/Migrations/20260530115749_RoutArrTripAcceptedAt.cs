using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class RoutArrTripAcceptedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AcceptedAt",
                table: "routarr_trips",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trips_TenantId_AcceptedAt",
                table: "routarr_trips",
                columns: new[] { "TenantId", "AcceptedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_routarr_trips_TenantId_AcceptedAt",
                table: "routarr_trips");

            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                table: "routarr_trips");
        }
    }
}
