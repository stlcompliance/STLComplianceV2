using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreReadinessForecasting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliancecore_readiness_forecast_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PacksForecastCount = table.Column<int>(type: "integer", nullable: false),
                    ReadinessScore = table.Column<int>(type: "integer", nullable: false),
                    ReadinessLevel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    LowestReadinessScore = table.Column<int>(type: "integer", nullable: false),
                    AverageReadinessScore = table.Column<int>(type: "integer", nullable: false),
                    HighestRiskScore = table.Column<int>(type: "integer", nullable: false),
                    MissingEvidenceWarningCount = table.Column<int>(type: "integer", nullable: false),
                    AverageEffectivenessScore = table.Column<int>(type: "integer", nullable: false),
                    RiskScoreRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    MissingEvidenceWarningRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlEffectivenessRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    ForecastedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_readiness_forecast_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_readiness_forecasts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RunId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReadinessScore = table.Column<int>(type: "integer", nullable: false),
                    ReadinessLevel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    RiskScore = table.Column<int>(type: "integer", nullable: false),
                    RiskLevel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    EffectivenessScore = table.Column<int>(type: "integer", nullable: false),
                    EffectivenessLevel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MissingEvidenceWarningCount = table.Column<int>(type: "integer", nullable: false),
                    HighestMissingEvidenceSeverity = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Summary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ForecastedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_readiness_forecasts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_readiness_forecasts_compliancecore_readiness~",
                        column: x => x.RunId,
                        principalTable: "compliancecore_readiness_forecast_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_readiness_forecast_runs_TenantId",
                table: "compliancecore_readiness_forecast_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_readiness_forecast_runs_TenantId_ForecastedAt",
                table: "compliancecore_readiness_forecast_runs",
                columns: new[] { "TenantId", "ForecastedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_readiness_forecasts_RunId",
                table: "compliancecore_readiness_forecasts",
                column: "RunId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_readiness_forecasts_TenantId",
                table: "compliancecore_readiness_forecasts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_readiness_forecasts_TenantId_ScopeKey_PackKe~",
                table: "compliancecore_readiness_forecasts",
                columns: new[] { "TenantId", "ScopeKey", "PackKey", "ForecastedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_readiness_forecasts");

            migrationBuilder.DropTable(
                name: "compliancecore_readiness_forecast_runs");
        }
    }
}
