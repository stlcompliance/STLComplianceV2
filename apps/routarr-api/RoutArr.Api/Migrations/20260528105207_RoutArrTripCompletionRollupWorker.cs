using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class RoutArrTripCompletionRollupWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "routarr_tenant_trip_completion_rollup_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    StalenessHours = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_tenant_trip_completion_rollup_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_trip_completion_rollup_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsOfUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CandidatesFound = table.Column<int>(type: "integer", nullable: false),
                    RefreshedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_trip_completion_rollup_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_trip_completion_rollups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DispatchStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AssignedDriverPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VehicleRefKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ScheduledStartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ScheduledEndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    RouteCount = table.Column<int>(type: "integer", nullable: false),
                    CompletedRouteCount = table.Column<int>(type: "integer", nullable: false),
                    StopCount = table.Column<int>(type: "integer", nullable: false),
                    CompletedStopCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedStopCount = table.Column<int>(type: "integer", nullable: false),
                    PendingStopCount = table.Column<int>(type: "integer", nullable: false),
                    LoadCount = table.Column<int>(type: "integer", nullable: false),
                    DeliveredLoadCount = table.Column<int>(type: "integer", nullable: false),
                    PendingLoadCount = table.Column<int>(type: "integer", nullable: false),
                    SourceUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_trip_completion_rollups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_trip_completion_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    RollupId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Detail = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    SourceEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceEntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_trip_completion_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_trip_completion_events_routarr_trip_completion_roll~",
                        column: x => x.RollupId,
                        principalTable: "routarr_trip_completion_rollups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_trip_completion_rollup_settings_TenantId",
                table: "routarr_tenant_trip_completion_rollup_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_completion_events_RollupId",
                table: "routarr_trip_completion_events",
                column: "RollupId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_completion_events_TenantId",
                table: "routarr_trip_completion_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_completion_events_TenantId_TripId_SequenceNumb~",
                table: "routarr_trip_completion_events",
                columns: new[] { "TenantId", "TripId", "SequenceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_completion_rollup_runs_TenantId",
                table: "routarr_trip_completion_rollup_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_completion_rollup_runs_TenantId_CreatedAt",
                table: "routarr_trip_completion_rollup_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_completion_rollups_TenantId",
                table: "routarr_trip_completion_rollups",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_completion_rollups_TenantId_DispatchStatus_Com~",
                table: "routarr_trip_completion_rollups",
                columns: new[] { "TenantId", "DispatchStatus", "CompletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_completion_rollups_TenantId_TripId",
                table: "routarr_trip_completion_rollups",
                columns: new[] { "TenantId", "TripId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "routarr_tenant_trip_completion_rollup_settings");

            migrationBuilder.DropTable(
                name: "routarr_trip_completion_events");

            migrationBuilder.DropTable(
                name: "routarr_trip_completion_rollup_runs");

            migrationBuilder.DropTable(
                name: "routarr_trip_completion_rollups");
        }
    }
}
