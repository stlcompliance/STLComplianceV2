using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class RoutArrDriverTimeTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "GeofenceAnchorLatitude",
                table: "routarr_route_stops",
                type: "numeric(10,6)",
                precision: 10,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GeofenceAnchorLongitude",
                table: "routarr_route_stops",
                type: "numeric(10,6)",
                precision: 10,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GeofenceRadiusMeters",
                table: "routarr_route_stops",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastGeofenceCheckAt",
                table: "routarr_route_stops",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LastGeofenceDistanceMeters",
                table: "routarr_route_stops",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LastGeofenceReportedLatitude",
                table: "routarr_route_stops",
                type: "numeric(10,6)",
                precision: 10,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LastGeofenceReportedLongitude",
                table: "routarr_route_stops",
                type: "numeric(10,6)",
                precision: 10,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastGeofenceResult",
                table: "routarr_route_stops",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "routarr_driver_time_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EntryType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    EditReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_driver_time_entries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_driver_time_entries_TenantId",
                table: "routarr_driver_time_entries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_driver_time_entries_TenantId_PersonId_EntryType_Sta~",
                table: "routarr_driver_time_entries",
                columns: new[] { "TenantId", "PersonId", "EntryType", "StartsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_driver_time_entries_TenantId_PersonId_StartsAt",
                table: "routarr_driver_time_entries",
                columns: new[] { "TenantId", "PersonId", "StartsAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "routarr_driver_time_entries");

            migrationBuilder.DropColumn(
                name: "GeofenceAnchorLatitude",
                table: "routarr_route_stops");

            migrationBuilder.DropColumn(
                name: "GeofenceAnchorLongitude",
                table: "routarr_route_stops");

            migrationBuilder.DropColumn(
                name: "GeofenceRadiusMeters",
                table: "routarr_route_stops");

            migrationBuilder.DropColumn(
                name: "LastGeofenceCheckAt",
                table: "routarr_route_stops");

            migrationBuilder.DropColumn(
                name: "LastGeofenceDistanceMeters",
                table: "routarr_route_stops");

            migrationBuilder.DropColumn(
                name: "LastGeofenceReportedLatitude",
                table: "routarr_route_stops");

            migrationBuilder.DropColumn(
                name: "LastGeofenceReportedLongitude",
                table: "routarr_route_stops");

            migrationBuilder.DropColumn(
                name: "LastGeofenceResult",
                table: "routarr_route_stops");
        }
    }
}
