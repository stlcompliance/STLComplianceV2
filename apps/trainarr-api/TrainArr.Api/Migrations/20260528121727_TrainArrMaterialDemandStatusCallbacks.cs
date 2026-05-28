using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrMaterialDemandStatusCallbacks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_trainarr_training_assignment_material_demand_lines_TenantId~",
                table: "trainarr_training_assignment_material_demand_lines",
                newName: "IX_trainarr_training_assignment_material_demand_lines_TenantI~2");

            migrationBuilder.RenameIndex(
                name: "IX_trainarr_training_assignment_material_demand_lines_TenantI~1",
                table: "trainarr_training_assignment_material_demand_lines",
                newName: "IX_trainarr_training_assignment_material_demand_lines_TenantI~3");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastProcurementStatusAt",
                table: "trainarr_training_assignment_material_demand_lines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcurementStatus",
                table: "trainarr_training_assignment_material_demand_lines",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProcurementStatusMessage",
                table: "trainarr_training_assignment_material_demand_lines",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "QuantityReceived",
                table: "trainarr_training_assignment_material_demand_lines",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "SupplyarrPurchaseOrderId",
                table: "trainarr_training_assignment_material_demand_lines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SupplyarrPurchaseRequestId",
                table: "trainarr_training_assignment_material_demand_lines",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "trainarr_training_assignment_material_demand_status_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainarrPublicationId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_trainarr_training_assignment_material_demand_status_events", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_material_demand_lines_TenantI~1",
                table: "trainarr_training_assignment_material_demand_lines",
                columns: new[] { "TenantId", "TrainarrPublicationId" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_material_demand_lines_TenantId~",
                table: "trainarr_training_assignment_material_demand_lines",
                columns: new[] { "TenantId", "ProcurementStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_material_demand_status_events_~",
                table: "trainarr_training_assignment_material_demand_status_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_material_demand_status_events~1",
                table: "trainarr_training_assignment_material_demand_status_events",
                columns: new[] { "TenantId", "SupplyarrCallbackPublicationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_material_demand_status_events~2",
                table: "trainarr_training_assignment_material_demand_status_events",
                columns: new[] { "TenantId", "TrainarrPublicationId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_training_assignment_material_demand_status_events");

            migrationBuilder.DropIndex(
                name: "IX_trainarr_training_assignment_material_demand_lines_TenantI~1",
                table: "trainarr_training_assignment_material_demand_lines");

            migrationBuilder.DropIndex(
                name: "IX_trainarr_training_assignment_material_demand_lines_TenantId~",
                table: "trainarr_training_assignment_material_demand_lines");

            migrationBuilder.DropColumn(
                name: "LastProcurementStatusAt",
                table: "trainarr_training_assignment_material_demand_lines");

            migrationBuilder.DropColumn(
                name: "ProcurementStatus",
                table: "trainarr_training_assignment_material_demand_lines");

            migrationBuilder.DropColumn(
                name: "ProcurementStatusMessage",
                table: "trainarr_training_assignment_material_demand_lines");

            migrationBuilder.DropColumn(
                name: "QuantityReceived",
                table: "trainarr_training_assignment_material_demand_lines");

            migrationBuilder.DropColumn(
                name: "SupplyarrPurchaseOrderId",
                table: "trainarr_training_assignment_material_demand_lines");

            migrationBuilder.DropColumn(
                name: "SupplyarrPurchaseRequestId",
                table: "trainarr_training_assignment_material_demand_lines");

            migrationBuilder.RenameIndex(
                name: "IX_trainarr_training_assignment_material_demand_lines_TenantI~3",
                table: "trainarr_training_assignment_material_demand_lines",
                newName: "IX_trainarr_training_assignment_material_demand_lines_TenantI~1");

            migrationBuilder.RenameIndex(
                name: "IX_trainarr_training_assignment_material_demand_lines_TenantI~2",
                table: "trainarr_training_assignment_material_demand_lines",
                newName: "IX_trainarr_training_assignment_material_demand_lines_TenantId~");
        }
    }
}
