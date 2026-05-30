using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreExceptionExemptionSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ExceptionExemptionApplies",
                table: "compliancecore_theoretical_situation_evaluation_details",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ExceptionExemptionConsidered",
                table: "compliancecore_theoretical_situation_evaluation_details",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ExceptionExemptionKey",
                table: "compliancecore_theoretical_situation_evaluation_details",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExceptionExemptionLabel",
                table: "compliancecore_theoretical_situation_evaluation_details",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ExceptionExemptionProofRequired",
                table: "compliancecore_theoretical_situation_evaluation_details",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ExceptionExemptionProofValid",
                table: "compliancecore_theoretical_situation_evaluation_details",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ExceptionExemptionType",
                table: "compliancecore_theoretical_situation_evaluation_details",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FinalComplianceResult",
                table: "compliancecore_theoretical_situation_evaluation_details",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalRuleResult",
                table: "compliancecore_theoretical_situation_evaluation_details",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResultAfterException",
                table: "compliancecore_theoretical_situation_evaluation_details",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResultBeforeException",
                table: "compliancecore_theoretical_situation_evaluation_details",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EvidenceMappingPurpose",
                table: "compliancecore_import_staged_mapping_decisions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExceptionExemptionKey",
                table: "compliancecore_import_staged_mapping_decisions",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResidualRequirementsJson",
                table: "compliancecore_import_staged_mapping_decisions",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "ClaimedExceptionExemptionKey",
                table: "compliancecore_audit_traces",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClaimedExceptionExemptionType",
                table: "compliancecore_audit_traces",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ExceptionExemptionApplied",
                table: "compliancecore_audit_traces",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ExceptionExemptionEffectiveResult",
                table: "compliancecore_audit_traces",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExceptionExemptionLegalBasis",
                table: "compliancecore_audit_traces",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExceptionExemptionProofKey",
                table: "compliancecore_audit_traces",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ExceptionExemptionProofRequired",
                table: "compliancecore_audit_traces",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ExceptionExemptionProofValid",
                table: "compliancecore_audit_traces",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ExceptionExemptionScopeResult",
                table: "compliancecore_audit_traces",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FinalComplianceResult",
                table: "compliancecore_audit_traces",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResultAfterException",
                table: "compliancecore_audit_traces",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResultBeforeException",
                table: "compliancecore_audit_traces",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "compliance_exception_exemption",
                columns: table => new
                {
                    ExceptionExemptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    GoverningBody = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProgramKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PackKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CitationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ApplicabilityKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AppliesToSubjectKind = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AppliesToSourceProduct = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AppliesToSourceEntity = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    EffectType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ConditionLogicJson = table.Column<string>(type: "jsonb", nullable: false),
                    RequiredEvidenceOptionGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    IssuingAuthority = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AuthorizationNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EffectiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliance_exception_exemption", x => x.ExceptionExemptionId);
                    table.ForeignKey(
                        name: "FK_compliance_exception_exemption_compliancecore_evidence_opti~",
                        column: x => x.RequiredEvidenceOptionGroupId,
                        principalTable: "compliancecore_evidence_option_groups",
                        principalColumn: "EvidenceOptionGroupId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_import_staged_exception_exemptions",
                columns: table => new
                {
                    StagedRowId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceFile = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RowNumber = table.Column<int>(type: "integer", nullable: false),
                    RawRowJson = table.Column<string>(type: "jsonb", nullable: false),
                    NormalizedRowJson = table.Column<string>(type: "jsonb", nullable: false),
                    RowHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ValidationStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ValidationErrorsJson = table.Column<string>(type: "jsonb", nullable: false),
                    CanonicalKeyCandidate = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_import_staged_exception_exemptions", x => x.StagedRowId);
                    table.ForeignKey(
                        name: "FK_compliancecore_import_staged_exception_exemptions_complianc~",
                        column: x => x.ImportSessionId,
                        principalTable: "compliancecore_import_sessions",
                        principalColumn: "ImportSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_audit_traces_TenantId_ClaimedExceptionExempt~",
                table: "compliancecore_audit_traces",
                columns: new[] { "TenantId", "ClaimedExceptionExemptionKey" });

            migrationBuilder.CreateIndex(
                name: "IX_compliance_exception_exemption_RequiredEvidenceOptionGroupId",
                table: "compliance_exception_exemption",
                column: "RequiredEvidenceOptionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_compliance_exception_exemption_TenantId",
                table: "compliance_exception_exemption",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliance_exception_exemption_TenantId_Active_ExpiresAt",
                table: "compliance_exception_exemption",
                columns: new[] { "TenantId", "Active", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_compliance_exception_exemption_TenantId_Key",
                table: "compliance_exception_exemption",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliance_exception_exemption_TenantId_ProgramKey_PackKey_~",
                table: "compliance_exception_exemption",
                columns: new[] { "TenantId", "ProgramKey", "PackKey", "CitationKey" });

            migrationBuilder.CreateIndex(
                name: "IX_compliance_exception_exemption_TenantId_Type",
                table: "compliance_exception_exemption",
                columns: new[] { "TenantId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_exception_exemptions_ImportSe~1",
                table: "compliancecore_import_staged_exception_exemptions",
                columns: new[] { "ImportSessionId", "CanonicalKeyCandidate" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_exception_exemptions_ImportSe~2",
                table: "compliancecore_import_staged_exception_exemptions",
                columns: new[] { "ImportSessionId", "SourceFile", "RowNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_exception_exemptions_ImportSes~",
                table: "compliancecore_import_staged_exception_exemptions",
                column: "ImportSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_exception_exemptions_TenantId",
                table: "compliancecore_import_staged_exception_exemptions",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliance_exception_exemption");

            migrationBuilder.DropTable(
                name: "compliancecore_import_staged_exception_exemptions");

            migrationBuilder.DropIndex(
                name: "IX_compliancecore_audit_traces_TenantId_ClaimedExceptionExempt~",
                table: "compliancecore_audit_traces");

            migrationBuilder.DropColumn(
                name: "ExceptionExemptionApplies",
                table: "compliancecore_theoretical_situation_evaluation_details");

            migrationBuilder.DropColumn(
                name: "ExceptionExemptionConsidered",
                table: "compliancecore_theoretical_situation_evaluation_details");

            migrationBuilder.DropColumn(
                name: "ExceptionExemptionKey",
                table: "compliancecore_theoretical_situation_evaluation_details");

            migrationBuilder.DropColumn(
                name: "ExceptionExemptionLabel",
                table: "compliancecore_theoretical_situation_evaluation_details");

            migrationBuilder.DropColumn(
                name: "ExceptionExemptionProofRequired",
                table: "compliancecore_theoretical_situation_evaluation_details");

            migrationBuilder.DropColumn(
                name: "ExceptionExemptionProofValid",
                table: "compliancecore_theoretical_situation_evaluation_details");

            migrationBuilder.DropColumn(
                name: "ExceptionExemptionType",
                table: "compliancecore_theoretical_situation_evaluation_details");

            migrationBuilder.DropColumn(
                name: "FinalComplianceResult",
                table: "compliancecore_theoretical_situation_evaluation_details");

            migrationBuilder.DropColumn(
                name: "NormalRuleResult",
                table: "compliancecore_theoretical_situation_evaluation_details");

            migrationBuilder.DropColumn(
                name: "ResultAfterException",
                table: "compliancecore_theoretical_situation_evaluation_details");

            migrationBuilder.DropColumn(
                name: "ResultBeforeException",
                table: "compliancecore_theoretical_situation_evaluation_details");

            migrationBuilder.DropColumn(
                name: "EvidenceMappingPurpose",
                table: "compliancecore_import_staged_mapping_decisions");

            migrationBuilder.DropColumn(
                name: "ExceptionExemptionKey",
                table: "compliancecore_import_staged_mapping_decisions");

            migrationBuilder.DropColumn(
                name: "ResidualRequirementsJson",
                table: "compliancecore_import_staged_mapping_decisions");

            migrationBuilder.DropColumn(
                name: "ClaimedExceptionExemptionKey",
                table: "compliancecore_audit_traces");

            migrationBuilder.DropColumn(
                name: "ClaimedExceptionExemptionType",
                table: "compliancecore_audit_traces");

            migrationBuilder.DropColumn(
                name: "ExceptionExemptionApplied",
                table: "compliancecore_audit_traces");

            migrationBuilder.DropColumn(
                name: "ExceptionExemptionEffectiveResult",
                table: "compliancecore_audit_traces");

            migrationBuilder.DropColumn(
                name: "ExceptionExemptionLegalBasis",
                table: "compliancecore_audit_traces");

            migrationBuilder.DropColumn(
                name: "ExceptionExemptionProofKey",
                table: "compliancecore_audit_traces");

            migrationBuilder.DropColumn(
                name: "ExceptionExemptionProofRequired",
                table: "compliancecore_audit_traces");

            migrationBuilder.DropColumn(
                name: "ExceptionExemptionProofValid",
                table: "compliancecore_audit_traces");

            migrationBuilder.DropColumn(
                name: "ExceptionExemptionScopeResult",
                table: "compliancecore_audit_traces");

            migrationBuilder.DropColumn(
                name: "FinalComplianceResult",
                table: "compliancecore_audit_traces");

            migrationBuilder.DropColumn(
                name: "ResultAfterException",
                table: "compliancecore_audit_traces");

            migrationBuilder.DropColumn(
                name: "ResultBeforeException",
                table: "compliancecore_audit_traces");
        }
    }
}
