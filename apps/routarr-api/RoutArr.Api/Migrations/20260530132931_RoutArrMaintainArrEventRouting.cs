using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class RoutArrMaintainArrEventRouting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MaintainarrDefectId",
                table: "routarr_trip_dvir_inspections",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaintainarrEventRouteStatus",
                table: "routarr_trip_dvir_inspections",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MaintainarrEventRoutedAt",
                table: "routarr_trip_dvir_inspections",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MaintainarrInboundEventId",
                table: "routarr_trip_dvir_inspections",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MaintainarrDefectId",
                table: "routarr_dispatch_exceptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MaintainarrInboundEventId",
                table: "routarr_dispatch_exceptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaintainarrIncidentRouteStatus",
                table: "routarr_dispatch_exceptions",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MaintainarrIncidentRoutedAt",
                table: "routarr_dispatch_exceptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_dvir_inspections_maintainarr_defect",
                table: "routarr_trip_dvir_inspections",
                columns: new[] { "TenantId", "MaintainarrDefectId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_exceptions_maintainarr_defect",
                table: "routarr_dispatch_exceptions",
                columns: new[] { "TenantId", "MaintainarrDefectId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_routarr_trip_dvir_inspections_maintainarr_defect",
                table: "routarr_trip_dvir_inspections");

            migrationBuilder.DropIndex(
                name: "IX_routarr_dispatch_exceptions_maintainarr_defect",
                table: "routarr_dispatch_exceptions");

            migrationBuilder.DropColumn(
                name: "MaintainarrDefectId",
                table: "routarr_trip_dvir_inspections");

            migrationBuilder.DropColumn(
                name: "MaintainarrEventRouteStatus",
                table: "routarr_trip_dvir_inspections");

            migrationBuilder.DropColumn(
                name: "MaintainarrEventRoutedAt",
                table: "routarr_trip_dvir_inspections");

            migrationBuilder.DropColumn(
                name: "MaintainarrInboundEventId",
                table: "routarr_trip_dvir_inspections");

            migrationBuilder.DropColumn(
                name: "MaintainarrDefectId",
                table: "routarr_dispatch_exceptions");

            migrationBuilder.DropColumn(
                name: "MaintainarrInboundEventId",
                table: "routarr_dispatch_exceptions");

            migrationBuilder.DropColumn(
                name: "MaintainarrIncidentRouteStatus",
                table: "routarr_dispatch_exceptions");

            migrationBuilder.DropColumn(
                name: "MaintainarrIncidentRoutedAt",
                table: "routarr_dispatch_exceptions");
        }
    }
}
