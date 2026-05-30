using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreTheoreticalSituationEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliancecore_theoretical_situations",
                columns: table => new
                {
                    SituationId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SituationKind = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EvaluationMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SavedAsTemplate = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_theoretical_situations", x => x.SituationId);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_theoretical_applicability_results",
                columns: table => new
                {
                    ApplicabilityResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SituationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PackKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CitationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ApplicabilityScore = table.Column<decimal>(type: "numeric(5,3)", precision: 5, scale: 3, nullable: false),
                    ApplicabilityBand = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MatchReasonsJson = table.Column<string>(type: "jsonb", nullable: false),
                    MissingContextJson = table.Column<string>(type: "jsonb", nullable: false),
                    ExclusionReasonsJson = table.Column<string>(type: "jsonb", nullable: false),
                    EdgeCase = table.Column<bool>(type: "boolean", nullable: false),
                    EdgeCaseReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    UserVisiblePriority = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_theoretical_applicability_results", x => x.ApplicabilityResultId);
                    table.ForeignKey(
                        name: "FK_compliancecore_theoretical_applicability_results_compliance~",
                        column: x => x.SituationId,
                        principalTable: "compliancecore_theoretical_situations",
                        principalColumn: "SituationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_theoretical_situation_contexts",
                columns: table => new
                {
                    ContextId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SituationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContextKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ContextLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContextValueKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ContextValueLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ControlledVocabularyType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Confidence = table.Column<decimal>(type: "numeric(5,3)", precision: 5, scale: 3, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_theoretical_situation_contexts", x => x.ContextId);
                    table.ForeignKey(
                        name: "FK_compliancecore_theoretical_situation_contexts_compliancecor~",
                        column: x => x.SituationId,
                        principalTable: "compliancecore_theoretical_situations",
                        principalColumn: "SituationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_theoretical_situation_evaluations",
                columns: table => new
                {
                    EvaluationId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SituationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EvaluatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Result = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Summary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    PrimaryProgramsJson = table.Column<string>(type: "jsonb", nullable: false),
                    LikelyProgramsJson = table.Column<string>(type: "jsonb", nullable: false),
                    EdgeCasesJson = table.Column<string>(type: "jsonb", nullable: false),
                    PassCount = table.Column<int>(type: "integer", nullable: false),
                    FailCount = table.Column<int>(type: "integer", nullable: false),
                    WarningCount = table.Column<int>(type: "integer", nullable: false),
                    BlockedCount = table.Column<int>(type: "integer", nullable: false),
                    NotApplicableCount = table.Column<int>(type: "integer", nullable: false),
                    UnknownCount = table.Column<int>(type: "integer", nullable: false),
                    OverrideAvailableCount = table.Column<int>(type: "integer", nullable: false),
                    OverrideBlockedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_theoretical_situation_evaluations", x => x.EvaluationId);
                    table.ForeignKey(
                        name: "FK_compliancecore_theoretical_situation_evaluations_compliance~",
                        column: x => x.SituationId,
                        principalTable: "compliancecore_theoretical_situations",
                        principalColumn: "SituationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_theoretical_situation_facts",
                columns: table => new
                {
                    SituationFactId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SituationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FactKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RequirementKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CitationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PackKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SimulatedValue = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ValueType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SimulatedState = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EvidenceOptionKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EvidenceKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TargetKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_theoretical_situation_facts", x => x.SituationFactId);
                    table.ForeignKey(
                        name: "FK_compliancecore_theoretical_situation_facts_compliancecore_t~",
                        column: x => x.SituationId,
                        principalTable: "compliancecore_theoretical_situations",
                        principalColumn: "SituationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_theoretical_situation_incidents",
                columns: table => new
                {
                    SituationIncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SituationId = table.Column<Guid>(type: "uuid", nullable: false),
                    IncidentTypeKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SeverityKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    InvolvedSubjectKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    InvolvedSubjectState = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TriggerKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TriggerValue = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ReportabilityState = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RemediationState = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_theoretical_situation_incidents", x => x.SituationIncidentId);
                    table.ForeignKey(
                        name: "FK_compliancecore_theoretical_situation_incidents_complianceco~",
                        column: x => x.SituationId,
                        principalTable: "compliancecore_theoretical_situations",
                        principalColumn: "SituationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_theoretical_situation_evaluation_details",
                columns: table => new
                {
                    DetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FactKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CitationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PackKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AuditQuestion = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    SimulatedState = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExpectedValue = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ActualValue = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Operator = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Result = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FailureSeverity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AutomaticFailureFlag = table.Column<bool>(type: "boolean", nullable: false),
                    OverrideAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    OverridePermission = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RemediationRequired = table.Column<bool>(type: "boolean", nullable: false),
                    Explanation = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    SuggestedNextAction = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    VisiblePriority = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_theoretical_situation_evaluation_details", x => x.DetailId);
                    table.ForeignKey(
                        name: "FK_compliancecore_theoretical_situation_evaluation_details_com~",
                        column: x => x.EvaluationId,
                        principalTable: "compliancecore_theoretical_situation_evaluations",
                        principalColumn: "EvaluationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_theoretical_applicability_results_SituationI~",
                table: "compliancecore_theoretical_applicability_results",
                columns: new[] { "SituationId", "ApplicabilityBand", "UserVisiblePriority" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_theoretical_applicability_results_SituationId",
                table: "compliancecore_theoretical_applicability_results",
                column: "SituationId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_theoretical_applicability_results_TenantId",
                table: "compliancecore_theoretical_applicability_results",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_theoretical_situation_contexts_SituationId",
                table: "compliancecore_theoretical_situation_contexts",
                column: "SituationId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_theoretical_situation_contexts_SituationId_C~",
                table: "compliancecore_theoretical_situation_contexts",
                columns: new[] { "SituationId", "ContextKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_theoretical_situation_contexts_TenantId",
                table: "compliancecore_theoretical_situation_contexts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_theoretical_situation_evaluation_details_Ev~1",
                table: "compliancecore_theoretical_situation_evaluation_details",
                columns: new[] { "EvaluationId", "VisiblePriority" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_theoretical_situation_evaluation_details_Eva~",
                table: "compliancecore_theoretical_situation_evaluation_details",
                column: "EvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_theoretical_situation_evaluation_details_Ten~",
                table: "compliancecore_theoretical_situation_evaluation_details",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_theoretical_situation_evaluations_SituationI~",
                table: "compliancecore_theoretical_situation_evaluations",
                columns: new[] { "SituationId", "EvaluatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_theoretical_situation_evaluations_SituationId",
                table: "compliancecore_theoretical_situation_evaluations",
                column: "SituationId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_theoretical_situation_evaluations_TenantId",
                table: "compliancecore_theoretical_situation_evaluations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_theoretical_situation_facts_SituationId",
                table: "compliancecore_theoretical_situation_facts",
                column: "SituationId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_theoretical_situation_facts_SituationId_Fact~",
                table: "compliancecore_theoretical_situation_facts",
                columns: new[] { "SituationId", "FactKey", "RequirementKey", "EvidenceOptionKey" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_theoretical_situation_facts_TenantId",
                table: "compliancecore_theoretical_situation_facts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_theoretical_situation_incidents_SituationId",
                table: "compliancecore_theoretical_situation_incidents",
                column: "SituationId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_theoretical_situation_incidents_TenantId",
                table: "compliancecore_theoretical_situation_incidents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_theoretical_situations_TenantId",
                table: "compliancecore_theoretical_situations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_theoretical_situations_TenantId_SituationKind",
                table: "compliancecore_theoretical_situations",
                columns: new[] { "TenantId", "SituationKind" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_theoretical_situations_TenantId_Status_Updat~",
                table: "compliancecore_theoretical_situations",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_theoretical_applicability_results");

            migrationBuilder.DropTable(
                name: "compliancecore_theoretical_situation_contexts");

            migrationBuilder.DropTable(
                name: "compliancecore_theoretical_situation_evaluation_details");

            migrationBuilder.DropTable(
                name: "compliancecore_theoretical_situation_facts");

            migrationBuilder.DropTable(
                name: "compliancecore_theoretical_situation_incidents");

            migrationBuilder.DropTable(
                name: "compliancecore_theoretical_situation_evaluations");

            migrationBuilder.DropTable(
                name: "compliancecore_theoretical_situations");
        }
    }
}
