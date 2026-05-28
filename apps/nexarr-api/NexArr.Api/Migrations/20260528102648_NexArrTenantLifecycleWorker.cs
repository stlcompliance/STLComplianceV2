using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class NexArrTenantLifecycleWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nexarr_platform_tenant_lifecycle_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AutoSuspendWhenNoValidLicense = table.Column<bool>(type: "boolean", nullable: false),
                    SuspendGraceDaysAfterLastLicenseExpiry = table.Column<int>(type: "integer", nullable: false),
                    AutoReactivateWhenValidLicense = table.Column<bool>(type: "boolean", nullable: false),
                    RevokeSessionsOnSuspend = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_platform_tenant_lifecycle_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nexarr_tenant_lifecycle_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PendingCount = table.Column<int>(type: "integer", nullable: false),
                    SuspendedCount = table.Column<int>(type: "integer", nullable: false),
                    ReactivatedCount = table.Column<int>(type: "integer", nullable: false),
                    SessionsRevokedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    SkipReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nexarr_tenant_lifecycle_runs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nexarr_tenant_lifecycle_runs_ProcessedAt",
                table: "nexarr_tenant_lifecycle_runs",
                column: "ProcessedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nexarr_platform_tenant_lifecycle_settings");

            migrationBuilder.DropTable(
                name: "nexarr_tenant_lifecycle_runs");
        }
    }
}
