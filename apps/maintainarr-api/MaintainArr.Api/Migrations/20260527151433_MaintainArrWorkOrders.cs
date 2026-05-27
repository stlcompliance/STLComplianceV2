using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrWorkOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_work_orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefectId = table.Column<Guid>(type: "uuid", nullable: true),
                    PmScheduleId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkOrderNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Priority = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AssignedTechnicianPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_work_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_orders_maintainarr_assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "maintainarr_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_orders_maintainarr_defects_DefectId",
                        column: x => x.DefectId,
                        principalTable: "maintainarr_defects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_orders_maintainarr_pm_schedules_PmSchedule~",
                        column: x => x.PmScheduleId,
                        principalTable: "maintainarr_pm_schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_orders_AssetId",
                table: "maintainarr_work_orders",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_orders_DefectId",
                table: "maintainarr_work_orders",
                column: "DefectId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_orders_PmScheduleId",
                table: "maintainarr_work_orders",
                column: "PmScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_orders_TenantId",
                table: "maintainarr_work_orders",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_orders_TenantId_AssetId_Status",
                table: "maintainarr_work_orders",
                columns: new[] { "TenantId", "AssetId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_orders_TenantId_AssignedTechnicianPersonId~",
                table: "maintainarr_work_orders",
                columns: new[] { "TenantId", "AssignedTechnicianPersonId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_orders_TenantId_CreatedByUserId_CreatedAt",
                table: "maintainarr_work_orders",
                columns: new[] { "TenantId", "CreatedByUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_orders_TenantId_DefectId",
                table: "maintainarr_work_orders",
                columns: new[] { "TenantId", "DefectId" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_orders_TenantId_WorkOrderNumber",
                table: "maintainarr_work_orders",
                columns: new[] { "TenantId", "WorkOrderNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_work_orders");
        }
    }
}
