using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreRiskScoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliancecore_risk_score_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PacksEvaluatedCount = table.Column<int>(type: "integer", nullable: false),
                    HighestRiskScore = table.Column<int>(type: "integer", nullable: false),
                    HighestRiskLevel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_risk_score_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_risk_scores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RunId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RiskScoreValue = table.Column<int>(type: "integer", nullable: false),
                    RiskLevel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    RuleOutcome = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    EvaluationResult = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    UnresolvedFactCount = table.Column<int>(type: "integer", nullable: false),
                    FailedRuleCount = table.Column<int>(type: "integer", nullable: false),
                    ResolvedFactCount = table.Column<int>(type: "integer", nullable: false),
                    MirrorFactCount = table.Column<int>(type: "integer", nullable: false),
                    Summary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_risk_scores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_risk_scores_compliancecore_risk_score_runs_R~",
                        column: x => x.RunId,
                        principalTable: "compliancecore_risk_score_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_risk_score_runs_TenantId",
                table: "compliancecore_risk_score_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_risk_score_runs_TenantId_EvaluatedAt",
                table: "compliancecore_risk_score_runs",
                columns: new[] { "TenantId", "EvaluatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_risk_scores_RunId",
                table: "compliancecore_risk_scores",
                column: "RunId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_risk_scores_TenantId",
                table: "compliancecore_risk_scores",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_risk_scores_TenantId_ScopeKey_PackKey_Evalua~",
                table: "compliancecore_risk_scores",
                columns: new[] { "TenantId", "ScopeKey", "PackKey", "EvaluatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_risk_scores");

            migrationBuilder.DropTable(
                name: "compliancecore_risk_score_runs");
        }
    }
}
