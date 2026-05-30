using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreAuditReadyFactRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicabilityKey",
                table: "compliancecore_fact_requirements",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AuditQuestion",
                table: "compliancecore_fact_requirements",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "AutomaticFailureFlag",
                table: "compliancecore_fact_requirements",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EvidenceKind",
                table: "compliancecore_fact_requirements",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExpectedValue",
                table: "compliancecore_fact_requirements",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ExternallyAssertable",
                table: "compliancecore_fact_requirements",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FailureSeverity",
                table: "compliancecore_fact_requirements",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Operator",
                table: "compliancecore_fact_requirements",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "OverrideAllowed",
                table: "compliancecore_fact_requirements",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OverridePermission",
                table: "compliancecore_fact_requirements",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "RemediationRequired",
                table: "compliancecore_fact_requirements",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RequiredDocumentType",
                table: "compliancecore_fact_requirements",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RetentionPeriod",
                table: "compliancecore_fact_requirements",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SourceEntity",
                table: "compliancecore_fact_requirements",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SourceFieldOrRecordType",
                table: "compliancecore_fact_requirements",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SourceProduct",
                table: "compliancecore_fact_requirements",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ValueType",
                table: "compliancecore_fact_requirements",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "compliancecore_audit_traces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditTraceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PackKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FactKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CitationKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SubjectKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SubjectId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EvaluatedValue = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ExpectedValue = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Operator = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FailureSeverity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AutomaticFailureFlag = table.Column<bool>(type: "boolean", nullable: false),
                    OverrideUsed = table.Column<bool>(type: "boolean", nullable: false),
                    OverridePersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    OverrideReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    RemediationRequired = table.Column<bool>(type: "boolean", nullable: false),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_audit_traces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_evidence_references",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidenceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FactKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceEntity = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceRecordId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceField = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DocumentUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    FileHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CapturedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EffectiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_evidence_references", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_fact_assertions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FactKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SubjectKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SubjectId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ValueType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceRecordId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EvidenceReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvidenceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AssertedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EffectiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_fact_assertions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_fact_assertions_compliancecore_evidence_refe~",
                        column: x => x.EvidenceReferenceId,
                        principalTable: "compliancecore_evidence_references",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_requirements_TenantId_SourceEntity",
                table: "compliancecore_fact_requirements",
                columns: new[] { "TenantId", "SourceEntity" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_requirements_TenantId_SourceProduct",
                table: "compliancecore_fact_requirements",
                columns: new[] { "TenantId", "SourceProduct" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_audit_traces_TenantId",
                table: "compliancecore_audit_traces",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_audit_traces_TenantId_AuditTraceId",
                table: "compliancecore_audit_traces",
                columns: new[] { "TenantId", "AuditTraceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_audit_traces_TenantId_CitationKey",
                table: "compliancecore_audit_traces",
                columns: new[] { "TenantId", "CitationKey" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_audit_traces_TenantId_FactKey",
                table: "compliancecore_audit_traces",
                columns: new[] { "TenantId", "FactKey" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_audit_traces_TenantId_PackKey",
                table: "compliancecore_audit_traces",
                columns: new[] { "TenantId", "PackKey" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_evidence_references_TenantId",
                table: "compliancecore_evidence_references",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_evidence_references_TenantId_EvidenceId",
                table: "compliancecore_evidence_references",
                columns: new[] { "TenantId", "EvidenceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_evidence_references_TenantId_FactKey",
                table: "compliancecore_evidence_references",
                columns: new[] { "TenantId", "FactKey" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_evidence_references_TenantId_SourceProduct_S~",
                table: "compliancecore_evidence_references",
                columns: new[] { "TenantId", "SourceProduct", "SourceEntity" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_assertions_EvidenceReferenceId",
                table: "compliancecore_fact_assertions",
                column: "EvidenceReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_assertions_TenantId",
                table: "compliancecore_fact_assertions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_assertions_TenantId_FactKey_SubjectKind~",
                table: "compliancecore_fact_assertions",
                columns: new[] { "TenantId", "FactKey", "SubjectKind", "SubjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_assertions_TenantId_SourceProduct",
                table: "compliancecore_fact_assertions",
                columns: new[] { "TenantId", "SourceProduct" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_audit_traces");

            migrationBuilder.DropTable(
                name: "compliancecore_fact_assertions");

            migrationBuilder.DropTable(
                name: "compliancecore_evidence_references");

            migrationBuilder.DropIndex(
                name: "IX_compliancecore_fact_requirements_TenantId_SourceEntity",
                table: "compliancecore_fact_requirements");

            migrationBuilder.DropIndex(
                name: "IX_compliancecore_fact_requirements_TenantId_SourceProduct",
                table: "compliancecore_fact_requirements");

            migrationBuilder.DropColumn(
                name: "ApplicabilityKey",
                table: "compliancecore_fact_requirements");

            migrationBuilder.DropColumn(
                name: "AuditQuestion",
                table: "compliancecore_fact_requirements");

            migrationBuilder.DropColumn(
                name: "AutomaticFailureFlag",
                table: "compliancecore_fact_requirements");

            migrationBuilder.DropColumn(
                name: "EvidenceKind",
                table: "compliancecore_fact_requirements");

            migrationBuilder.DropColumn(
                name: "ExpectedValue",
                table: "compliancecore_fact_requirements");

            migrationBuilder.DropColumn(
                name: "ExternallyAssertable",
                table: "compliancecore_fact_requirements");

            migrationBuilder.DropColumn(
                name: "FailureSeverity",
                table: "compliancecore_fact_requirements");

            migrationBuilder.DropColumn(
                name: "Operator",
                table: "compliancecore_fact_requirements");

            migrationBuilder.DropColumn(
                name: "OverrideAllowed",
                table: "compliancecore_fact_requirements");

            migrationBuilder.DropColumn(
                name: "OverridePermission",
                table: "compliancecore_fact_requirements");

            migrationBuilder.DropColumn(
                name: "RemediationRequired",
                table: "compliancecore_fact_requirements");

            migrationBuilder.DropColumn(
                name: "RequiredDocumentType",
                table: "compliancecore_fact_requirements");

            migrationBuilder.DropColumn(
                name: "RetentionPeriod",
                table: "compliancecore_fact_requirements");

            migrationBuilder.DropColumn(
                name: "SourceEntity",
                table: "compliancecore_fact_requirements");

            migrationBuilder.DropColumn(
                name: "SourceFieldOrRecordType",
                table: "compliancecore_fact_requirements");

            migrationBuilder.DropColumn(
                name: "SourceProduct",
                table: "compliancecore_fact_requirements");

            migrationBuilder.DropColumn(
                name: "ValueType",
                table: "compliancecore_fact_requirements");
        }
    }
}
