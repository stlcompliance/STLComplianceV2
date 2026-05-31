using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class RoutArrStaffArrIncidentRouting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StaffarrIncidentRouteStatus",
                table: "routarr_dispatch_exceptions",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StaffarrIncidentRoutedAt",
                table: "routarr_dispatch_exceptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StaffarrPersonnelIncidentId",
                table: "routarr_dispatch_exceptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_exceptions_staffarr_incident",
                table: "routarr_dispatch_exceptions",
                columns: new[] { "TenantId", "StaffarrPersonnelIncidentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_routarr_dispatch_exceptions_staffarr_incident",
                table: "routarr_dispatch_exceptions");

            migrationBuilder.DropColumn(
                name: "StaffarrIncidentRouteStatus",
                table: "routarr_dispatch_exceptions");

            migrationBuilder.DropColumn(
                name: "StaffarrIncidentRoutedAt",
                table: "routarr_dispatch_exceptions");

            migrationBuilder.DropColumn(
                name: "StaffarrPersonnelIncidentId",
                table: "routarr_dispatch_exceptions");
        }
    }
}
