using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrWorkOrderPartsDemand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_work_order_parts_demand_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    SupplyarrPartId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    QuantityRequested = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MaintainarrPublicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplyarrDemandRefId = table.Column<Guid>(type: "uuid", nullable: true),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_work_order_parts_demand_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_order_parts_demand_lines_maintainarr_work_~",
                        column: x => x.WorkOrderId,
                        principalTable: "maintainarr_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_parts_demand_lines_TenantId",
                table: "maintainarr_work_order_parts_demand_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_parts_demand_lines_TenantId_Maintain~",
                table: "maintainarr_work_order_parts_demand_lines",
                columns: new[] { "TenantId", "MaintainarrPublicationId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_parts_demand_lines_TenantId_WorkOrd~1",
                table: "maintainarr_work_order_parts_demand_lines",
                columns: new[] { "TenantId", "WorkOrderId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_parts_demand_lines_TenantId_WorkOrde~",
                table: "maintainarr_work_order_parts_demand_lines",
                columns: new[] { "TenantId", "WorkOrderId", "LineNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_parts_demand_lines_WorkOrderId",
                table: "maintainarr_work_order_parts_demand_lines",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_work_order_parts_demand_lines");
        }
    }
}
