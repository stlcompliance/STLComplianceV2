using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class RoutArrRouteStopFoundations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "routarr_routes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    RouteStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActivatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_routes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_routes_routarr_trips_TripId",
                        column: x => x.TripId,
                        principalTable: "routarr_trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "routarr_route_stops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    StopKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AddressLabel = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    StopType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StopStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    ScheduledArrivalAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ArrivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_route_stops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_route_stops_routarr_routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "routarr_routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_route_stops_RouteId",
                table: "routarr_route_stops",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_route_stops_TenantId",
                table: "routarr_route_stops",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_route_stops_TenantId_RouteId",
                table: "routarr_route_stops",
                columns: new[] { "TenantId", "RouteId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_route_stops_TenantId_RouteId_SequenceNumber",
                table: "routarr_route_stops",
                columns: new[] { "TenantId", "RouteId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_route_stops_TenantId_RouteId_StopKey",
                table: "routarr_route_stops",
                columns: new[] { "TenantId", "RouteId", "StopKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_routes_TenantId",
                table: "routarr_routes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_routes_TenantId_RouteNumber",
                table: "routarr_routes",
                columns: new[] { "TenantId", "RouteNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_routes_TenantId_RouteStatus_UpdatedAt",
                table: "routarr_routes",
                columns: new[] { "TenantId", "RouteStatus", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_routes_TenantId_TripId",
                table: "routarr_routes",
                columns: new[] { "TenantId", "TripId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_routes_TripId",
                table: "routarr_routes",
                column: "TripId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "routarr_route_stops");

            migrationBuilder.DropTable(
                name: "routarr_routes");
        }
    }
}
