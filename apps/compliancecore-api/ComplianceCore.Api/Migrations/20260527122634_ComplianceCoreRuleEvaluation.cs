using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreRuleEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RuleContentJson",
                table: "compliancecore_rule_packs",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "compliancecore_rule_evaluation_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OverallResult = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    FactInputsJson = table.Column<string>(type: "jsonb", nullable: false),
                    RuleResultsJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_rule_evaluation_runs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_rule_evaluation_runs_compliancecore_rule_pac~",
                        column: x => x.RulePackId,
                        principalTable: "compliancecore_rule_packs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_evaluation_runs_CreatedAt",
                table: "compliancecore_rule_evaluation_runs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_evaluation_runs_RulePackId",
                table: "compliancecore_rule_evaluation_runs",
                column: "RulePackId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_evaluation_runs_TenantId",
                table: "compliancecore_rule_evaluation_runs",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_rule_evaluation_runs");

            migrationBuilder.DropColumn(
                name: "RuleContentJson",
                table: "compliancecore_rule_packs");
        }
    }
}
