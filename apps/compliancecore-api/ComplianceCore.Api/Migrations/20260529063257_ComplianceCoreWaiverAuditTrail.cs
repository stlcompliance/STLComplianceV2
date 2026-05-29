using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreWaiverAuditTrail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AppliedWaiverId",
                table: "compliancecore_workflow_gate_check_results",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppliedWaiverKey",
                table: "compliancecore_workflow_gate_check_results",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AppliedWaiverId",
                table: "compliancecore_rule_evaluation_runs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppliedWaiverKey",
                table: "compliancecore_rule_evaluation_runs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_workflow_gate_check_results_AppliedWaiverId",
                table: "compliancecore_workflow_gate_check_results",
                column: "AppliedWaiverId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_evaluation_runs_AppliedWaiverId",
                table: "compliancecore_rule_evaluation_runs",
                column: "AppliedWaiverId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_compliancecore_workflow_gate_check_results_AppliedWaiverId",
                table: "compliancecore_workflow_gate_check_results");

            migrationBuilder.DropIndex(
                name: "IX_compliancecore_rule_evaluation_runs_AppliedWaiverId",
                table: "compliancecore_rule_evaluation_runs");

            migrationBuilder.DropColumn(
                name: "AppliedWaiverId",
                table: "compliancecore_workflow_gate_check_results");

            migrationBuilder.DropColumn(
                name: "AppliedWaiverKey",
                table: "compliancecore_workflow_gate_check_results");

            migrationBuilder.DropColumn(
                name: "AppliedWaiverId",
                table: "compliancecore_rule_evaluation_runs");

            migrationBuilder.DropColumn(
                name: "AppliedWaiverKey",
                table: "compliancecore_rule_evaluation_runs");
        }
    }
}
