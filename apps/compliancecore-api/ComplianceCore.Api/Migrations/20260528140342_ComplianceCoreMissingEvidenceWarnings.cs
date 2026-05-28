using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreMissingEvidenceWarnings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliancecore_missing_evidence_warning_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PacksAnalyzedCount = table.Column<int>(type: "integer", nullable: false),
                    WarningsEmittedCount = table.Column<int>(type: "integer", nullable: false),
                    HighestSeverity = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_missing_evidence_warning_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_missing_evidence_warnings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RunId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FactKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FactDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    WarningType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Severity = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    HasMirrorAtScope = table.Column<bool>(type: "boolean", nullable: false),
                    IsRequiredInRule = table.Column<bool>(type: "boolean", nullable: false),
                    IsRequiredInCatalog = table.Column<bool>(type: "boolean", nullable: false),
                    Summary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_missing_evidence_warnings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_missing_evidence_warnings_compliancecore_mis~",
                        column: x => x.RunId,
                        principalTable: "compliancecore_missing_evidence_warning_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_missing_evidence_warning_runs_TenantId",
                table: "compliancecore_missing_evidence_warning_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_missing_evidence_warning_runs_TenantId_Evalu~",
                table: "compliancecore_missing_evidence_warning_runs",
                columns: new[] { "TenantId", "EvaluatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_missing_evidence_warnings_RunId",
                table: "compliancecore_missing_evidence_warnings",
                column: "RunId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_missing_evidence_warnings_TenantId",
                table: "compliancecore_missing_evidence_warnings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_missing_evidence_warnings_TenantId_ScopeKey_~",
                table: "compliancecore_missing_evidence_warnings",
                columns: new[] { "TenantId", "ScopeKey", "PackKey", "Severity", "EvaluatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_missing_evidence_warnings");

            migrationBuilder.DropTable(
                name: "compliancecore_missing_evidence_warning_runs");
        }
    }
}
