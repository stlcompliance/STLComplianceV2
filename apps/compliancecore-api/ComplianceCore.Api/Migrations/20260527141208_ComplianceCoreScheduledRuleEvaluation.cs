using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreScheduledRuleEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastScheduledEvaluationAt",
                table: "compliancecore_rule_packs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "compliancecore_scheduled_rule_evaluation_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IntervalHours = table.Column<int>(type: "integer", nullable: false),
                    PacksDueCount = table.Column<int>(type: "integer", nullable: false),
                    PacksProcessedCount = table.Column<int>(type: "integer", nullable: false),
                    EvaluatedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    AllowCount = table.Column<int>(type: "integer", nullable: false),
                    WarnCount = table.Column<int>(type: "integer", nullable: false),
                    BlockCount = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_scheduled_rule_evaluation_runs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_packs_TenantId_Status_LastScheduledEval~",
                table: "compliancecore_rule_packs",
                columns: new[] { "TenantId", "Status", "LastScheduledEvaluationAt" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_scheduled_rule_evaluation_runs_StartedAt",
                table: "compliancecore_scheduled_rule_evaluation_runs",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_scheduled_rule_evaluation_runs_TenantId",
                table: "compliancecore_scheduled_rule_evaluation_runs",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_scheduled_rule_evaluation_runs");

            migrationBuilder.DropIndex(
                name: "IX_compliancecore_rule_packs_TenantId_Status_LastScheduledEval~",
                table: "compliancecore_rule_packs");

            migrationBuilder.DropColumn(
                name: "LastScheduledEvaluationAt",
                table: "compliancecore_rule_packs");
        }
    }
}
