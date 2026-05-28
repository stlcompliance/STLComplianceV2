using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreControlEffectiveness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliancecore_control_effectiveness_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PacksEvaluatedCount = table.Column<int>(type: "integer", nullable: false),
                    LowestEffectivenessScore = table.Column<int>(type: "integer", nullable: false),
                    LowestEffectivenessLevel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AverageEffectivenessScore = table.Column<int>(type: "integer", nullable: false),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_control_effectiveness_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_control_effectiveness_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RunId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EffectivenessScore = table.Column<int>(type: "integer", nullable: false),
                    EffectivenessLevel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ControlStatus = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    RuleOutcome = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    EvaluationResult = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    TotalRuleCount = table.Column<int>(type: "integer", nullable: false),
                    PassedRuleCount = table.Column<int>(type: "integer", nullable: false),
                    FailedRuleCount = table.Column<int>(type: "integer", nullable: false),
                    UnresolvedFactCount = table.Column<int>(type: "integer", nullable: false),
                    ResolvedFactCount = table.Column<int>(type: "integer", nullable: false),
                    Summary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_control_effectiveness_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_control_effectiveness_records_compliancecore~",
                        column: x => x.RunId,
                        principalTable: "compliancecore_control_effectiveness_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_control_effectiveness_records_RunId",
                table: "compliancecore_control_effectiveness_records",
                column: "RunId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_control_effectiveness_records_TenantId",
                table: "compliancecore_control_effectiveness_records",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_control_effectiveness_records_TenantId_Scope~",
                table: "compliancecore_control_effectiveness_records",
                columns: new[] { "TenantId", "ScopeKey", "PackKey", "EvaluatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_control_effectiveness_runs_TenantId",
                table: "compliancecore_control_effectiveness_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_control_effectiveness_runs_TenantId_Evaluate~",
                table: "compliancecore_control_effectiveness_runs",
                columns: new[] { "TenantId", "EvaluatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_control_effectiveness_records");

            migrationBuilder.DropTable(
                name: "compliancecore_control_effectiveness_runs");
        }
    }
}
