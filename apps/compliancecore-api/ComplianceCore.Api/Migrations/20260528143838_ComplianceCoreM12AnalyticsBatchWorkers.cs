using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreM12AnalyticsBatchWorkers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliancecore_m12_analytics_batch_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IntervalHours = table.Column<int>(type: "integer", nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RiskScoringRan = table.Column<bool>(type: "boolean", nullable: false),
                    MissingEvidenceRan = table.Column<bool>(type: "boolean", nullable: false),
                    ControlEffectivenessRan = table.Column<bool>(type: "boolean", nullable: false),
                    ReadinessForecastRan = table.Column<bool>(type: "boolean", nullable: false),
                    AuditDeliveryQueued = table.Column<bool>(type: "boolean", nullable: false),
                    RiskScoreRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    MissingEvidenceWarningRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    ControlEffectivenessRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReadinessForecastRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuditPackageJobId = table.Column<Guid>(type: "uuid", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_m12_analytics_batch_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_tenant_m12_analytics_worker_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IntervalHours = table.Column<int>(type: "integer", nullable: false),
                    RiskScoringEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    MissingEvidenceEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ControlEffectivenessEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ReadinessForecastEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AuditDeliveryEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LastBatchRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastRiskScoringRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastMissingEvidenceRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastControlEffectivenessRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastReadinessForecastRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastAuditDeliveryRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_tenant_m12_analytics_worker_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_m12_analytics_batch_runs_TenantId_StartedAt",
                table: "compliancecore_m12_analytics_batch_runs",
                columns: new[] { "TenantId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_tenant_m12_analytics_worker_settings_TenantId",
                table: "compliancecore_tenant_m12_analytics_worker_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_m12_analytics_batch_runs");

            migrationBuilder.DropTable(
                name: "compliancecore_tenant_m12_analytics_worker_settings");
        }
    }
}
