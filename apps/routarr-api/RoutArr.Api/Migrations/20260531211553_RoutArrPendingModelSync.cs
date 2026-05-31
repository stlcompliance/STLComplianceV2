using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class RoutArrPendingModelSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "routarr_trip_dispatch_release_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReleasedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReleasedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DriverCanAssign = table.Column<bool>(type: "boolean", nullable: false),
                    VehicleCanAssign = table.Column<bool>(type: "boolean", nullable: false),
                    HasMissingExternalData = table.Column<bool>(type: "boolean", nullable: false),
                    HasStaleExternalData = table.Column<bool>(type: "boolean", nullable: false),
                    Summary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    SnapshotJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_trip_dispatch_release_snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_trip_dispatch_release_snapshots_routarr_trips_TripId",
                        column: x => x.TripId,
                        principalTable: "routarr_trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_dispatch_release_snapshots_TenantId",
                table: "routarr_trip_dispatch_release_snapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_dispatch_release_snapshots_TenantId_ReleasedAt",
                table: "routarr_trip_dispatch_release_snapshots",
                columns: new[] { "TenantId", "ReleasedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_dispatch_release_snapshots_TenantId_TripId",
                table: "routarr_trip_dispatch_release_snapshots",
                columns: new[] { "TenantId", "TripId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_dispatch_release_snapshots_TripId",
                table: "routarr_trip_dispatch_release_snapshots",
                column: "TripId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "routarr_trip_dispatch_release_snapshots");
        }
    }
}
