using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrPersonExportScheduledDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staffarr_person_export_delivery_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExportId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IntervalHours = table.Column<int>(type: "integer", nullable: false),
                    EmploymentStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    OrgUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    SkipReason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_person_export_delivery_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_tenant_person_export_schedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IntervalHours = table.Column<int>(type: "integer", nullable: false),
                    LastDeliveredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_tenant_person_export_schedules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_export_delivery_runs_StartedAt",
                table: "staffarr_person_export_delivery_runs",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_person_export_delivery_runs_TenantId",
                table: "staffarr_person_export_delivery_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_tenant_person_export_schedules_TenantId",
                table: "staffarr_tenant_person_export_schedules",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_tenant_person_export_schedules_TenantId_IsEnabled_~",
                table: "staffarr_tenant_person_export_schedules",
                columns: new[] { "TenantId", "IsEnabled", "LastDeliveredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_person_export_delivery_runs");

            migrationBuilder.DropTable(
                name: "staffarr_tenant_person_export_schedules");
        }
    }
}
