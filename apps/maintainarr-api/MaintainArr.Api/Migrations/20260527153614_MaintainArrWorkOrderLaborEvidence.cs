using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class MaintainArrWorkOrderLaborEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_work_order_evidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidenceTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_work_order_evidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_order_evidence_maintainarr_work_orders_Wor~",
                        column: x => x.WorkOrderId,
                        principalTable: "maintainarr_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_work_order_task_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_work_order_task_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_order_task_lines_maintainarr_work_orders_W~",
                        column: x => x.WorkOrderId,
                        principalTable: "maintainarr_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_work_order_labor_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderTaskLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    PersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    HoursWorked = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    LaborTypeKey = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    LoggedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoggedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_work_order_labor_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_order_labor_entries_maintainarr_work_order~",
                        column: x => x.WorkOrderId,
                        principalTable: "maintainarr_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_order_labor_entries_maintainarr_work_orde~1",
                        column: x => x.WorkOrderTaskLineId,
                        principalTable: "maintainarr_work_order_task_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_evidence_TenantId",
                table: "maintainarr_work_order_evidence",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_evidence_TenantId_WorkOrderId_Create~",
                table: "maintainarr_work_order_evidence",
                columns: new[] { "TenantId", "WorkOrderId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_evidence_WorkOrderId",
                table: "maintainarr_work_order_evidence",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_labor_entries_TenantId",
                table: "maintainarr_work_order_labor_entries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_labor_entries_TenantId_PersonId_Logg~",
                table: "maintainarr_work_order_labor_entries",
                columns: new[] { "TenantId", "PersonId", "LoggedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_labor_entries_TenantId_WorkOrderId_L~",
                table: "maintainarr_work_order_labor_entries",
                columns: new[] { "TenantId", "WorkOrderId", "LoggedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_labor_entries_WorkOrderId",
                table: "maintainarr_work_order_labor_entries",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_labor_entries_WorkOrderTaskLineId",
                table: "maintainarr_work_order_labor_entries",
                column: "WorkOrderTaskLineId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_task_lines_TenantId",
                table: "maintainarr_work_order_task_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_task_lines_TenantId_WorkOrderId_Sort~",
                table: "maintainarr_work_order_task_lines",
                columns: new[] { "TenantId", "WorkOrderId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_task_lines_WorkOrderId",
                table: "maintainarr_work_order_task_lines",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_work_order_evidence");

            migrationBuilder.DropTable(
                name: "maintainarr_work_order_labor_entries");

            migrationBuilder.DropTable(
                name: "maintainarr_work_order_task_lines");
        }
    }
}
