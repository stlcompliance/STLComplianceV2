using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreFindingsWorkflowGates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliancecore_findings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleEvaluationRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    FindingKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Severity = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RuleKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FactKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Message = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_findings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_findings_compliancecore_rule_evaluation_runs~",
                        column: x => x.RuleEvaluationRunId,
                        principalTable: "compliancecore_rule_evaluation_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_compliancecore_findings_compliancecore_rule_packs_RulePackId",
                        column: x => x.RulePackId,
                        principalTable: "compliancecore_rule_packs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_workflow_gate_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: false),
                    GateKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_workflow_gate_definitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_workflow_gate_definitions_compliancecore_rul~",
                        column: x => x.RulePackId,
                        principalTable: "compliancecore_rule_packs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_workflow_gate_check_results",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowGateDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleEvaluationRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    GateKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Outcome = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Message = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ReasonsJson = table.Column<string>(type: "jsonb", nullable: false),
                    ContextJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_workflow_gate_check_results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_workflow_gate_check_results_compliancecore_r~",
                        column: x => x.RuleEvaluationRunId,
                        principalTable: "compliancecore_rule_evaluation_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_compliancecore_workflow_gate_check_results_compliancecore_w~",
                        column: x => x.WorkflowGateDefinitionId,
                        principalTable: "compliancecore_workflow_gate_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_findings_CreatedAt",
                table: "compliancecore_findings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_findings_RuleEvaluationRunId",
                table: "compliancecore_findings",
                column: "RuleEvaluationRunId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_findings_RulePackId",
                table: "compliancecore_findings",
                column: "RulePackId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_findings_TenantId",
                table: "compliancecore_findings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_findings_TenantId_FindingKey",
                table: "compliancecore_findings",
                columns: new[] { "TenantId", "FindingKey" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_workflow_gate_check_results_CreatedAt",
                table: "compliancecore_workflow_gate_check_results",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_workflow_gate_check_results_RuleEvaluationRu~",
                table: "compliancecore_workflow_gate_check_results",
                column: "RuleEvaluationRunId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_workflow_gate_check_results_TenantId",
                table: "compliancecore_workflow_gate_check_results",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_workflow_gate_check_results_WorkflowGateDefi~",
                table: "compliancecore_workflow_gate_check_results",
                column: "WorkflowGateDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_workflow_gate_definitions_RulePackId",
                table: "compliancecore_workflow_gate_definitions",
                column: "RulePackId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_workflow_gate_definitions_TenantId",
                table: "compliancecore_workflow_gate_definitions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_workflow_gate_definitions_TenantId_GateKey",
                table: "compliancecore_workflow_gate_definitions",
                columns: new[] { "TenantId", "GateKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_findings");

            migrationBuilder.DropTable(
                name: "compliancecore_workflow_gate_check_results");

            migrationBuilder.DropTable(
                name: "compliancecore_workflow_gate_definitions");
        }
    }
}
