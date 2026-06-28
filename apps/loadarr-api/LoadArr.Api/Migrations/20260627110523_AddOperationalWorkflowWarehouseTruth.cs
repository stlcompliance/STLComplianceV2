using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoadArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationalWorkflowWarehouseTruth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "loadarr_inventory_balances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BalanceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SupplyarrItemId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LocationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LotCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SerialCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    State = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    QuantityOnHand = table.Column<decimal>(type: "numeric", nullable: false),
                    QuantityReserved = table.Column<decimal>(type: "numeric", nullable: false),
                    QuantityAllocated = table.Column<decimal>(type: "numeric", nullable: false),
                    QuantityBlocked = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loadarr_inventory_balances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "loadarr_inventory_movements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MovementId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MovementType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FromLocationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ToLocationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SupplyarrItemId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RelatedObjectType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelatedObjectId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    InventoryOriginEventId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loadarr_inventory_movements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "loadarr_inventory_origin_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginEventId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OriginType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OriginProductKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OriginObjectType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OriginObjectId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    WarehouseLocationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SupplyarrItemId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loadarr_inventory_origin_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "loadarr_warehouse_tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TaskType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SupplyarrItemId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceObjectType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceObjectId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DueAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loadarr_warehouse_tasks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_loadarr_inventory_balances_TenantId_BalanceId",
                table: "loadarr_inventory_balances",
                columns: new[] { "TenantId", "BalanceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_loadarr_inventory_balances_TenantId_SupplyarrItemId_Locatio~",
                table: "loadarr_inventory_balances",
                columns: new[] { "TenantId", "SupplyarrItemId", "LocationId", "LotCode", "SerialCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_loadarr_inventory_movements_TenantId_MovementId",
                table: "loadarr_inventory_movements",
                columns: new[] { "TenantId", "MovementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_loadarr_inventory_movements_TenantId_RelatedObjectType_Rela~",
                table: "loadarr_inventory_movements",
                columns: new[] { "TenantId", "RelatedObjectType", "RelatedObjectId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_loadarr_inventory_origin_events_TenantId_OriginEventId",
                table: "loadarr_inventory_origin_events",
                columns: new[] { "TenantId", "OriginEventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_loadarr_inventory_origin_events_TenantId_OriginObjectType_O~",
                table: "loadarr_inventory_origin_events",
                columns: new[] { "TenantId", "OriginObjectType", "OriginObjectId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_loadarr_warehouse_tasks_TenantId_SourceObjectType_SourceObj~",
                table: "loadarr_warehouse_tasks",
                columns: new[] { "TenantId", "SourceObjectType", "SourceObjectId", "TaskType" });

            migrationBuilder.CreateIndex(
                name: "IX_loadarr_warehouse_tasks_TenantId_TaskId",
                table: "loadarr_warehouse_tasks",
                columns: new[] { "TenantId", "TaskId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "loadarr_inventory_balances");

            migrationBuilder.DropTable(
                name: "loadarr_inventory_movements");

            migrationBuilder.DropTable(
                name: "loadarr_inventory_origin_events");

            migrationBuilder.DropTable(
                name: "loadarr_warehouse_tasks");
        }
    }
}
