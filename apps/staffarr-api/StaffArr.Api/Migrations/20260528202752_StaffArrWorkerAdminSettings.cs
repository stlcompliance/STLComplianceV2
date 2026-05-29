using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class StaffArrWorkerAdminSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staffarr_tenant_worker_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ScanIntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    BatchSize = table.Column<int>(type: "integer", nullable: false),
                    StalenessHours = table.Column<int>(type: "integer", nullable: true),
                    LastRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_tenant_worker_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staffarr_worker_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CandidatesFound = table.Column<int>(type: "integer", nullable: false),
                    ProcessedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    Summary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staffarr_worker_runs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_tenant_worker_settings_TenantId",
                table: "staffarr_tenant_worker_settings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_tenant_worker_settings_TenantId_WorkerKey",
                table: "staffarr_tenant_worker_settings",
                columns: new[] { "TenantId", "WorkerKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_worker_runs_TenantId",
                table: "staffarr_worker_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_staffarr_worker_runs_TenantId_WorkerKey_StartedAt",
                table: "staffarr_worker_runs",
                columns: new[] { "TenantId", "WorkerKey", "StartedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staffarr_tenant_worker_settings");

            migrationBuilder.DropTable(
                name: "staffarr_worker_runs");
        }
    }
}
