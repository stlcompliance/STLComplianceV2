using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenancePermitRefsAndReturnToService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "maintainarr_maintenance_permit_refs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermitType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RecordRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    StatusSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ApprovedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ValidFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ValidTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_maintenance_permit_refs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_maintenance_permit_refs_maintainarr_work_orders~",
                        column: x => x.WorkOrderId,
                        principalTable: "maintainarr_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintainarr_return_to_services",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequiredChecksJson = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    CompletedChecksJson = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    FinalInspectionRef = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    FinalReadinessStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    RecordRefsJson = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintainarr_return_to_services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_maintainarr_return_to_services_maintainarr_work_orders_Work~",
                        column: x => x.WorkOrderId,
                        principalTable: "maintainarr_work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_permit_refs_TenantId",
                table: "maintainarr_maintenance_permit_refs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_permit_refs_TenantId_WorkOrderId_Re~",
                table: "maintainarr_maintenance_permit_refs",
                columns: new[] { "TenantId", "WorkOrderId", "RecordRef" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_maintenance_permit_refs_WorkOrderId",
                table: "maintainarr_maintenance_permit_refs",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_return_to_services_TenantId",
                table: "maintainarr_return_to_services",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_return_to_services_TenantId_WorkOrderId",
                table: "maintainarr_return_to_services",
                columns: new[] { "TenantId", "WorkOrderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_maintainarr_return_to_services_WorkOrderId",
                table: "maintainarr_return_to_services",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "maintainarr_maintenance_permit_refs");

            migrationBuilder.DropTable(
                name: "maintainarr_return_to_services");
        }
    }
}
