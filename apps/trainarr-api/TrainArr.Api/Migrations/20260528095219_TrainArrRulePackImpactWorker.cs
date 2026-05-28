using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class TrainArrRulePackImpactWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trainarr_rule_pack_impact_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RulePackKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Outcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequiresAttention = table.Column<bool>(type: "boolean", nullable: false),
                    SkipReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_rule_pack_impact_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_rule_pack_impact_states",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RulePackKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RequiresAttention = table.Column<bool>(type: "boolean", nullable: false),
                    HasDrift = table.Column<bool>(type: "boolean", nullable: false),
                    Triggers = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    BaselineVersionNumber = table.Column<int>(type: "integer", nullable: true),
                    CurrentVersionNumber = table.Column<int>(type: "integer", nullable: true),
                    BaselineStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    CurrentStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    RequirementCount = table.Column<int>(type: "integer", nullable: false),
                    DefinitionCount = table.Column<int>(type: "integer", nullable: false),
                    ProgramCount = table.Column<int>(type: "integer", nullable: false),
                    ActiveAssignmentCount = table.Column<int>(type: "integer", nullable: false),
                    ActiveQualificationCount = table.Column<int>(type: "integer", nullable: false),
                    LastAssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_rule_pack_impact_states", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_tenant_rule_pack_impact_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    StalenessHours = table.Column<int>(type: "integer", nullable: false),
                    AutoUpdateRequirementBaselines = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_tenant_rule_pack_impact_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_rule_pack_impact_runs_TenantId",
                table: "trainarr_rule_pack_impact_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_rule_pack_impact_runs_TenantId_ProcessedAt",
                table: "trainarr_rule_pack_impact_runs",
                columns: new[] { "TenantId", "ProcessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_rule_pack_impact_runs_TenantId_RulePackKey",
                table: "trainarr_rule_pack_impact_runs",
                columns: new[] { "TenantId", "RulePackKey" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_rule_pack_impact_states_TenantId",
                table: "trainarr_rule_pack_impact_states",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_rule_pack_impact_states_TenantId_ComputedAt",
                table: "trainarr_rule_pack_impact_states",
                columns: new[] { "TenantId", "ComputedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_rule_pack_impact_states_TenantId_RulePackKey",
                table: "trainarr_rule_pack_impact_states",
                columns: new[] { "TenantId", "RulePackKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_tenant_rule_pack_impact_settings_TenantId",
                table: "trainarr_tenant_rule_pack_impact_settings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainarr_rule_pack_impact_runs");

            migrationBuilder.DropTable(
                name: "trainarr_rule_pack_impact_states");

            migrationBuilder.DropTable(
                name: "trainarr_tenant_rule_pack_impact_settings");
        }
    }
}
