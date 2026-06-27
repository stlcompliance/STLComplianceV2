using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_asset_reservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetTag = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AssetName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ReservationNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Purpose = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RequestedStartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RequestedEndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PickupLocationRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PickupLocationNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ReturnLocationRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReturnLocationNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CapacityNotes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    EquipmentNotes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    OperatorPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OperatorDisplayNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DriverPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DriverDisplayNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RequestedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RequestedByDisplayNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CheckOutMeterReading = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ReturnMeterReading = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReservedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CheckedOutAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    InUseAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReturnedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    InspectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CanceledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NoShowAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    NoShowReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    InspectionNotes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    DamageNotes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_asset_reservations_maintainarr_assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_asset_reservation_status_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetReservationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ToStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Message = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ActorPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ActorDisplayNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    MeterReading = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_asset_reservation_status_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_asset_reservation_status_events_maintainarr_ass~",
                        column: x => x.AssetReservationId,
                        principalTable: "maintainarr_asset_reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_reservation_status_events_AssetReservatio~",
                table: "maintainarr_asset_reservation_status_events",
                column: "AssetReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_reservation_status_events_TenantId",
                table: "maintainarr_asset_reservation_status_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_reservation_status_events_TenantId_AssetR~",
                table: "maintainarr_asset_reservation_status_events",
                columns: new[] { "TenantId", "AssetReservationId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_reservations_AssetId",
                table: "maintainarr_asset_reservations",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_reservations_TenantId",
                table: "maintainarr_asset_reservations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_reservations_TenantId_AssetId",
                table: "maintainarr_asset_reservations",
                columns: new[] { "TenantId", "AssetId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_reservations_TenantId_RequestedStartAt",
                table: "maintainarr_asset_reservations",
                columns: new[] { "TenantId", "RequestedStartAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_reservations_TenantId_ReservationNumber",
                table: "maintainarr_asset_reservations",
                columns: new[] { "TenantId", "ReservationNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_asset_reservations_TenantId_Status",
                table: "maintainarr_asset_reservations",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_asset_reservation_status_events");

            migrationBuilder.DropTable(
                name: "maintainarr_asset_reservations");
        }
    }
}
