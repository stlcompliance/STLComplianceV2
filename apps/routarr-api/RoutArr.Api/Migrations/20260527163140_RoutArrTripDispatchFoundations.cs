using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class RoutArrTripDispatchFoundations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "routarr_audit_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TargetType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TargetId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_audit_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_trips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    DispatchStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AssignedDriverPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VehicleRefKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ScheduledStartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ScheduledEndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DispatchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_trips", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_trip_loads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoadKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    LoadType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    OriginLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DestinationLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_trip_loads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_trip_loads_routarr_trips_TripId",
                        column: x => x.TripId,
                        principalTable: "routarr_trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_audit_events_TenantId",
                table: "routarr_audit_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_audit_events_TenantId_OccurredAt",
                table: "routarr_audit_events",
                columns: new[] { "TenantId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_loads_TenantId",
                table: "routarr_trip_loads",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_loads_TenantId_TripId",
                table: "routarr_trip_loads",
                columns: new[] { "TenantId", "TripId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_loads_TenantId_TripId_LoadKey",
                table: "routarr_trip_loads",
                columns: new[] { "TenantId", "TripId", "LoadKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_loads_TripId",
                table: "routarr_trip_loads",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trips_TenantId",
                table: "routarr_trips",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trips_TenantId_AssignedDriverPersonId",
                table: "routarr_trips",
                columns: new[] { "TenantId", "AssignedDriverPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trips_TenantId_DispatchStatus_UpdatedAt",
                table: "routarr_trips",
                columns: new[] { "TenantId", "DispatchStatus", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trips_TenantId_TripNumber",
                table: "routarr_trips",
                columns: new[] { "TenantId", "TripNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "routarr_audit_events");

            migrationBuilder.DropTable(
                name: "routarr_trip_loads");

            migrationBuilder.DropTable(
                name: "routarr_trips");
        }
    }
}
