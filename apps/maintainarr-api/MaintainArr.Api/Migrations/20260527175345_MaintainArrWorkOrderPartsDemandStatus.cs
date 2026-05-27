using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrWorkOrderPartsDemandStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastProcurementStatusAt",
                table: "maintainarr_work_order_parts_demand_lines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcurementStatus",
                table: "maintainarr_work_order_parts_demand_lines",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProcurementStatusMessage",
                table: "maintainarr_work_order_parts_demand_lines",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "QuantityReceived",
                table: "maintainarr_work_order_parts_demand_lines",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "SupplyarrPurchaseOrderId",
                table: "maintainarr_work_order_parts_demand_lines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SupplyarrPurchaseRequestId",
                table: "maintainarr_work_order_parts_demand_lines",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "maintainarr_work_order_parts_demand_status_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaintainarrPublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyarrDemandRefId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyarrCallbackPublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProcurementStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SupplyarrPurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplyarrPurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplyarrReceivingReceiptId = table.Column<Guid>(type: "uuid", nullable: true),
                    Message = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_work_order_parts_demand_status_events", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_parts_demand_lines_TenantId_Procurem~",
                table: "maintainarr_work_order_parts_demand_lines",
                columns: new[] { "TenantId", "ProcurementStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_parts_demand_status_events_TenantId",
                table: "maintainarr_work_order_parts_demand_status_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_parts_demand_status_events_TenantId_~",
                table: "maintainarr_work_order_parts_demand_status_events",
                columns: new[] { "TenantId", "SupplyarrCallbackPublicationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_parts_demand_status_events_TenantId~1",
                table: "maintainarr_work_order_parts_demand_status_events",
                columns: new[] { "TenantId", "MaintainarrPublicationId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_work_order_parts_demand_status_events");

            migrationBuilder.DropIndex(
                name: "IX_maintainarr_work_order_parts_demand_lines_TenantId_Procurem~",
                table: "maintainarr_work_order_parts_demand_lines");

            migrationBuilder.DropColumn(
                name: "LastProcurementStatusAt",
                table: "maintainarr_work_order_parts_demand_lines");

            migrationBuilder.DropColumn(
                name: "ProcurementStatus",
                table: "maintainarr_work_order_parts_demand_lines");

            migrationBuilder.DropColumn(
                name: "ProcurementStatusMessage",
                table: "maintainarr_work_order_parts_demand_lines");

            migrationBuilder.DropColumn(
                name: "QuantityReceived",
                table: "maintainarr_work_order_parts_demand_lines");

            migrationBuilder.DropColumn(
                name: "SupplyarrPurchaseOrderId",
                table: "maintainarr_work_order_parts_demand_lines");

            migrationBuilder.DropColumn(
                name: "SupplyarrPurchaseRequestId",
                table: "maintainarr_work_order_parts_demand_lines");
        }
    }
}
