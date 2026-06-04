using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreRuleTestCases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliancecore_rule_test_cases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TestKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ExpectedResult = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    FactsJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_rule_test_cases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_rule_test_cases_compliancecore_rule_packs_Ru~",
                        column: x => x.RulePackId,
                        principalTable: "compliancecore_rule_packs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_test_cases_RulePackId",
                table: "compliancecore_rule_test_cases",
                column: "RulePackId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_test_cases_TenantId",
                table: "compliancecore_rule_test_cases",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_test_cases_TenantId_RulePackId_RuleKey",
                table: "compliancecore_rule_test_cases",
                columns: new[] { "TenantId", "RulePackId", "RuleKey" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_test_cases_TenantId_RulePackId_TestKey",
                table: "compliancecore_rule_test_cases",
                columns: new[] { "TenantId", "RulePackId", "TestKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_rule_test_cases");
        }
    }
}
