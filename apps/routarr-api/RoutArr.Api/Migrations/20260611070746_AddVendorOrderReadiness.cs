using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorOrderReadiness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BrokerOrderId",
                table: "routarr_trips",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DispatchBlockReason",
                table: "routarr_trips",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DispatchOverrideAt",
                table: "routarr_trips",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DispatchOverrideByPersonId",
                table: "routarr_trips",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DispatchOverrideReason",
                table: "routarr_trips",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReleasedForDispatchAt",
                table: "routarr_trips",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReleasedForDispatchByEventId",
                table: "routarr_trips",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "VendorConfirmedReadyAtSnapshot",
                table: "routarr_trips",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "VendorExpectedReadyAtSnapshot",
                table: "routarr_trips",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VendorOrderId",
                table: "routarr_trips",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VendorOrderedQuantitySnapshot",
                table: "routarr_trips",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VendorQuantityReadySnapshot",
                table: "routarr_trips",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VendorReadinessStatusSnapshot",
                table: "routarr_trips",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "routarr_dispatch_blocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BlockReason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BlockingEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BlockingEntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResolvedByEventId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OverrideReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_dispatch_blocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_dispatch_blocks_routarr_trips_TripId",
                        column: x => x.TripId,
                        principalTable: "routarr_trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "routarr_supplyarr_vendor_order_event_receipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    VendorOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_supplyarr_vendor_order_event_receipts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trips_TenantId_BrokerOrderId",
                table: "routarr_trips",
                columns: new[] { "TenantId", "BrokerOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trips_TenantId_VendorOrderId",
                table: "routarr_trips",
                columns: new[] { "TenantId", "VendorOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_blocks_TenantId",
                table: "routarr_dispatch_blocks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_blocks_TenantId_TripId_BlockType_Status",
                table: "routarr_dispatch_blocks",
                columns: new[] { "TenantId", "TripId", "BlockType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_blocks_TripId",
                table: "routarr_dispatch_blocks",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_supplyarr_vendor_order_event_receipts_TenantId",
                table: "routarr_supplyarr_vendor_order_event_receipts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_supplyarr_vendor_order_event_receipts_TenantId_Even~",
                table: "routarr_supplyarr_vendor_order_event_receipts",
                columns: new[] { "TenantId", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_supplyarr_vendor_order_event_receipts_TenantId_Vend~",
                table: "routarr_supplyarr_vendor_order_event_receipts",
                columns: new[] { "TenantId", "VendorOrderId", "ProcessedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "routarr_dispatch_blocks");

            migrationBuilder.DropTable(
                name: "routarr_supplyarr_vendor_order_event_receipts");

            migrationBuilder.DropIndex(
                name: "IX_routarr_trips_TenantId_BrokerOrderId",
                table: "routarr_trips");

            migrationBuilder.DropIndex(
                name: "IX_routarr_trips_TenantId_VendorOrderId",
                table: "routarr_trips");

            migrationBuilder.DropColumn(
                name: "BrokerOrderId",
                table: "routarr_trips");

            migrationBuilder.DropColumn(
                name: "DispatchBlockReason",
                table: "routarr_trips");

            migrationBuilder.DropColumn(
                name: "DispatchOverrideAt",
                table: "routarr_trips");

            migrationBuilder.DropColumn(
                name: "DispatchOverrideByPersonId",
                table: "routarr_trips");

            migrationBuilder.DropColumn(
                name: "DispatchOverrideReason",
                table: "routarr_trips");

            migrationBuilder.DropColumn(
                name: "ReleasedForDispatchAt",
                table: "routarr_trips");

            migrationBuilder.DropColumn(
                name: "ReleasedForDispatchByEventId",
                table: "routarr_trips");

            migrationBuilder.DropColumn(
                name: "VendorConfirmedReadyAtSnapshot",
                table: "routarr_trips");

            migrationBuilder.DropColumn(
                name: "VendorExpectedReadyAtSnapshot",
                table: "routarr_trips");

            migrationBuilder.DropColumn(
                name: "VendorOrderId",
                table: "routarr_trips");

            migrationBuilder.DropColumn(
                name: "VendorOrderedQuantitySnapshot",
                table: "routarr_trips");

            migrationBuilder.DropColumn(
                name: "VendorQuantityReadySnapshot",
                table: "routarr_trips");

            migrationBuilder.DropColumn(
                name: "VendorReadinessStatusSnapshot",
                table: "routarr_trips");
        }
    }
}
