using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkOrderTechnicianAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_work_order_technician_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AssignmentRole = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AssignedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AcceptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RequiredQualificationRefsJson = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    QualificationCheckSnapshotJson = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_work_order_technician_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_work_order_technician_assignments_maintainarr_w~",
                        column: x => x.WorkOrderId,
                        principalTable: "maintainarr_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_technician_assignments_TenantId",
                table: "maintainarr_work_order_technician_assignments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_technician_assignments_TenantId_Wor~1",
                table: "maintainarr_work_order_technician_assignments",
                columns: new[] { "TenantId", "WorkOrderId", "PersonId", "AssignmentRole" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_technician_assignments_TenantId_Work~",
                table: "maintainarr_work_order_technician_assignments",
                columns: new[] { "TenantId", "WorkOrderId", "AssignedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_work_order_technician_assignments_WorkOrderId",
                table: "maintainarr_work_order_technician_assignments",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_work_order_technician_assignments");
        }
    }
}
