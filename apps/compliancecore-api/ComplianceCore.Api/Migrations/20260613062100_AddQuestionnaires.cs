using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionnaires : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliancecore_questionnaire_runs",
                columns: table => new
                {
                    QuestionnaireRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    WorkflowKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SubjectType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SubjectId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SourceRecordId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SourceEntity = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SourceRecordContextJson = table.Column<string>(type: "jsonb", nullable: false),
                    KnownFactsJson = table.Column<string>(type: "jsonb", nullable: false),
                    TemplateKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SummaryJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_questionnaire_runs", x => x.QuestionnaireRunId);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_questionnaire_answers",
                columns: table => new
                {
                    QuestionnaireAnswerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionnaireRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    QuestionLabel = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    SectionKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SectionLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AnswerKind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SelectedOptionKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AnswerText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    DocumentUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    FileName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    FileHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NormalizedFactKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NormalizedFactValue = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    NormalizedFactValueType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    WorkflowKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SubjectType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SubjectId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SourceRecordId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ReviewStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Confidence = table.Column<decimal>(type: "numeric(5,3)", precision: 5, scale: 3, nullable: false),
                    EffectiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EvidenceReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvidenceId = table.Column<string>(type: "text", nullable: true),
                    SourceContextJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_questionnaire_answers", x => x.QuestionnaireAnswerId);
                    table.ForeignKey(
                        name: "FK_compliancecore_questionnaire_answers_compliancecore_evidenc~",
                        column: x => x.EvidenceReferenceId,
                        principalTable: "compliancecore_evidence_references",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_compliancecore_questionnaire_answers_compliancecore_questio~",
                        column: x => x.QuestionnaireRunId,
                        principalTable: "compliancecore_questionnaire_runs",
                        principalColumn: "QuestionnaireRunId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_questionnaire_answers_EvidenceReferenceId",
                table: "compliancecore_questionnaire_answers",
                column: "EvidenceReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_questionnaire_answers_QuestionnaireRunId",
                table: "compliancecore_questionnaire_answers",
                column: "QuestionnaireRunId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_questionnaire_answers_TenantId",
                table: "compliancecore_questionnaire_answers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_questionnaire_answers_TenantId_Questionnaire~",
                table: "compliancecore_questionnaire_answers",
                columns: new[] { "TenantId", "QuestionnaireRunId", "QuestionKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_questionnaire_runs_TenantId",
                table: "compliancecore_questionnaire_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_questionnaire_runs_TenantId_ProductKey_Workf~",
                table: "compliancecore_questionnaire_runs",
                columns: new[] { "TenantId", "ProductKey", "WorkflowKey", "SubjectType", "SubjectId", "SourceRecordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_questionnaire_runs_TenantId_Status_UpdatedAt",
                table: "compliancecore_questionnaire_runs",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_questionnaire_answers");

            migrationBuilder.DropTable(
                name: "compliancecore_questionnaire_runs");
        }
    }
}
