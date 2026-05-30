using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class ComplianceCoreStagedImportWizard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compliancecore_asset_references",
                columns: table => new
                {
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ObjectKind = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalRecordId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StableKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_asset_references", x => x.ReferenceId);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_document_references",
                columns: table => new
                {
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ObjectKind = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalRecordId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StableKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_document_references", x => x.ReferenceId);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_evidence_option_groups",
                columns: table => new
                {
                    EvidenceOptionGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FactKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PackKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CitationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LogicType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ApplicabilityKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_evidence_option_groups", x => x.EvidenceOptionGroupId);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_external_object_references",
                columns: table => new
                {
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ObjectKind = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalRecordId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StableKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_external_object_references", x => x.ReferenceId);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_import_sessions",
                columns: table => new
                {
                    ImportSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceFilename = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    SourceHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ImportType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ValidationStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MappingStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CommitStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ValidatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MappedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CommittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_import_sessions", x => x.ImportSessionId);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_material_references",
                columns: table => new
                {
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ObjectKind = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalRecordId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StableKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_material_references", x => x.ReferenceId);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_part_references",
                columns: table => new
                {
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ObjectKind = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalRecordId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StableKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_part_references", x => x.ReferenceId);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_system_references",
                columns: table => new
                {
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ObjectKind = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalRecordId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StableKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_system_references", x => x.ReferenceId);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_evidence_options",
                columns: table => new
                {
                    EvidenceOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidenceOptionGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    OptionKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    OptionLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    EvidenceKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TargetKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceEntity = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SourceFieldOrRecordType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DocumentTypeKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MaterialKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PartKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SystemKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AssetKind = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalRegistryKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FactKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    ConfidenceHint = table.Column<decimal>(type: "numeric(5,3)", precision: 5, scale: 3, nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_evidence_options", x => x.EvidenceOptionId);
                    table.ForeignKey(
                        name: "FK_compliancecore_evidence_options_compliancecore_evidence_opt~",
                        column: x => x.EvidenceOptionGroupId,
                        principalTable: "compliancecore_evidence_option_groups",
                        principalColumn: "EvidenceOptionGroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_import_session_source_files",
                columns: table => new
                {
                    ImportSessionSourceFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceFile = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    OriginalFilename = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    FileHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ByteLength = table.Column<long>(type: "bigint", nullable: false),
                    ValidationStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ValidationErrorsJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_import_session_source_files", x => x.ImportSessionSourceFileId);
                    table.ForeignKey(
                        name: "FK_compliancecore_import_session_source_files_compliancecore_i~",
                        column: x => x.ImportSessionId,
                        principalTable: "compliancecore_import_sessions",
                        principalColumn: "ImportSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_import_staged_compliance_keys",
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
                    table.PrimaryKey("PK_compliancecore_import_staged_compliance_keys", x => x.StagedRowId);
                    table.ForeignKey(
                        name: "FK_compliancecore_import_staged_compliance_keys_compliancecore~",
                        column: x => x.ImportSessionId,
                        principalTable: "compliancecore_import_sessions",
                        principalColumn: "ImportSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_import_staged_controlled_vocabulary",
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
                    table.PrimaryKey("PK_compliancecore_import_staged_controlled_vocabulary", x => x.StagedRowId);
                    table.ForeignKey(
                        name: "FK_compliancecore_import_staged_controlled_vocabulary_complian~",
                        column: x => x.ImportSessionId,
                        principalTable: "compliancecore_import_sessions",
                        principalColumn: "ImportSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_import_staged_evidence_references",
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
                    table.PrimaryKey("PK_compliancecore_import_staged_evidence_references", x => x.StagedRowId);
                    table.ForeignKey(
                        name: "FK_compliancecore_import_staged_evidence_references_compliance~",
                        column: x => x.ImportSessionId,
                        principalTable: "compliancecore_import_sessions",
                        principalColumn: "ImportSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_import_staged_fact_requirements",
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
                    table.PrimaryKey("PK_compliancecore_import_staged_fact_requirements", x => x.StagedRowId);
                    table.ForeignKey(
                        name: "FK_compliancecore_import_staged_fact_requirements_complianceco~",
                        column: x => x.ImportSessionId,
                        principalTable: "compliancecore_import_sessions",
                        principalColumn: "ImportSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_import_staged_mapping_candidates",
                columns: table => new
                {
                    MappingCandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StagedRowId = table.Column<Guid>(type: "uuid", nullable: false),
                    StagedSourceFile = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StagedRowNumber = table.Column<int>(type: "integer", nullable: false),
                    SourceKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SourceLabel = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    EvidenceOptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvidenceOptionKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    EvidenceOptionLabel = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    OptionLogicGroup = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TargetKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TargetId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TargetKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TargetLabel = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "numeric(5,3)", precision: 5, scale: 3, nullable: false),
                    ConfidenceBand = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MatchReasonsJson = table.Column<string>(type: "jsonb", nullable: false),
                    RiskFlagsJson = table.Column<string>(type: "jsonb", nullable: false),
                    ProposedAction = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SatisfiesRequirementIfConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresAdditionalSupportingEvidence = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresConfirmation = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_import_staged_mapping_candidates", x => x.MappingCandidateId);
                    table.ForeignKey(
                        name: "FK_compliancecore_import_staged_mapping_candidates_compliancec~",
                        column: x => x.ImportSessionId,
                        principalTable: "compliancecore_import_sessions",
                        principalColumn: "ImportSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_import_staged_material_keys",
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
                    table.PrimaryKey("PK_compliancecore_import_staged_material_keys", x => x.StagedRowId);
                    table.ForeignKey(
                        name: "FK_compliancecore_import_staged_material_keys_compliancecore_i~",
                        column: x => x.ImportSessionId,
                        principalTable: "compliancecore_import_sessions",
                        principalColumn: "ImportSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_import_staged_regulatory_mappings",
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
                    table.PrimaryKey("PK_compliancecore_import_staged_regulatory_mappings", x => x.StagedRowId);
                    table.ForeignKey(
                        name: "FK_compliancecore_import_staged_regulatory_mappings_compliance~",
                        column: x => x.ImportSessionId,
                        principalTable: "compliancecore_import_sessions",
                        principalColumn: "ImportSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_import_staged_rule_packs",
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
                    table.PrimaryKey("PK_compliancecore_import_staged_rule_packs", x => x.StagedRowId);
                    table.ForeignKey(
                        name: "FK_compliancecore_import_staged_rule_packs_compliancecore_impo~",
                        column: x => x.ImportSessionId,
                        principalTable: "compliancecore_import_sessions",
                        principalColumn: "ImportSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_import_staged_rule_requirements",
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
                    table.PrimaryKey("PK_compliancecore_import_staged_rule_requirements", x => x.StagedRowId);
                    table.ForeignKey(
                        name: "FK_compliancecore_import_staged_rule_requirements_complianceco~",
                        column: x => x.ImportSessionId,
                        principalTable: "compliancecore_import_sessions",
                        principalColumn: "ImportSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_import_staged_sds_references",
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
                    table.PrimaryKey("PK_compliancecore_import_staged_sds_references", x => x.StagedRowId);
                    table.ForeignKey(
                        name: "FK_compliancecore_import_staged_sds_references_compliancecore_~",
                        column: x => x.ImportSessionId,
                        principalTable: "compliancecore_import_sessions",
                        principalColumn: "ImportSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_import_staged_vocabulary_aliases",
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
                    table.PrimaryKey("PK_compliancecore_import_staged_vocabulary_aliases", x => x.StagedRowId);
                    table.ForeignKey(
                        name: "FK_compliancecore_import_staged_vocabulary_aliases_compliancec~",
                        column: x => x.ImportSessionId,
                        principalTable: "compliancecore_import_sessions",
                        principalColumn: "ImportSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_import_staged_mapping_decisions",
                columns: table => new
                {
                    MappingDecisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    MappingCandidateId = table.Column<Guid>(type: "uuid", nullable: true),
                    StagedRowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Decision = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SelectedEvidenceOptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    SelectedEvidenceOptionKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SelectedTargetKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SelectedTargetId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SelectedTargetKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreateNewPayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    OverrideUsed = table.Column<bool>(type: "boolean", nullable: false),
                    OverrideReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DecidedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    DecidedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_import_staged_mapping_decisions", x => x.MappingDecisionId);
                    table.ForeignKey(
                        name: "FK_compliancecore_import_staged_mapping_decisions_complianceco~",
                        column: x => x.ImportSessionId,
                        principalTable: "compliancecore_import_sessions",
                        principalColumn: "ImportSessionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_compliancecore_import_staged_mapping_decisions_compliancec~1",
                        column: x => x.MappingCandidateId,
                        principalTable: "compliancecore_import_staged_mapping_candidates",
                        principalColumn: "MappingCandidateId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_asset_references_TenantId",
                table: "compliancecore_asset_references",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_asset_references_TenantId_SourceProduct_Exte~",
                table: "compliancecore_asset_references",
                columns: new[] { "TenantId", "SourceProduct", "ExternalRecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_asset_references_TenantId_SourceProduct_Obje~",
                table: "compliancecore_asset_references",
                columns: new[] { "TenantId", "SourceProduct", "ObjectKind" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_asset_references_TenantId_StableKey",
                table: "compliancecore_asset_references",
                columns: new[] { "TenantId", "StableKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_document_references_TenantId",
                table: "compliancecore_document_references",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_document_references_TenantId_SourceProduct_E~",
                table: "compliancecore_document_references",
                columns: new[] { "TenantId", "SourceProduct", "ExternalRecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_document_references_TenantId_SourceProduct_O~",
                table: "compliancecore_document_references",
                columns: new[] { "TenantId", "SourceProduct", "ObjectKind" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_document_references_TenantId_StableKey",
                table: "compliancecore_document_references",
                columns: new[] { "TenantId", "StableKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_evidence_option_groups_TenantId",
                table: "compliancecore_evidence_option_groups",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_evidence_option_groups_TenantId_CitationKey",
                table: "compliancecore_evidence_option_groups",
                columns: new[] { "TenantId", "CitationKey" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_evidence_option_groups_TenantId_PackKey",
                table: "compliancecore_evidence_option_groups",
                columns: new[] { "TenantId", "PackKey" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_evidence_option_groups_TenantId_RequirementK~",
                table: "compliancecore_evidence_option_groups",
                columns: new[] { "TenantId", "RequirementKey", "FactKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_evidence_options_EvidenceOptionGroupId",
                table: "compliancecore_evidence_options",
                column: "EvidenceOptionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_evidence_options_TenantId",
                table: "compliancecore_evidence_options",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_evidence_options_TenantId_OptionKey",
                table: "compliancecore_evidence_options",
                columns: new[] { "TenantId", "OptionKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_external_object_references_TenantId",
                table: "compliancecore_external_object_references",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_external_object_references_TenantId_SourceP~1",
                table: "compliancecore_external_object_references",
                columns: new[] { "TenantId", "SourceProduct", "ObjectKind" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_external_object_references_TenantId_SourcePr~",
                table: "compliancecore_external_object_references",
                columns: new[] { "TenantId", "SourceProduct", "ExternalRecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_external_object_references_TenantId_StableKey",
                table: "compliancecore_external_object_references",
                columns: new[] { "TenantId", "StableKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_session_source_files_ImportSessionId",
                table: "compliancecore_import_session_source_files",
                column: "ImportSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_session_source_files_ImportSessionId_~",
                table: "compliancecore_import_session_source_files",
                columns: new[] { "ImportSessionId", "SourceFile" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_session_source_files_TenantId",
                table: "compliancecore_import_session_source_files",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_sessions_TenantId",
                table: "compliancecore_import_sessions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_sessions_TenantId_SourceHash",
                table: "compliancecore_import_sessions",
                columns: new[] { "TenantId", "SourceHash" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_sessions_TenantId_Status_CreatedAt",
                table: "compliancecore_import_sessions",
                columns: new[] { "TenantId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_compliance_keys_ImportSession~1",
                table: "compliancecore_import_staged_compliance_keys",
                columns: new[] { "ImportSessionId", "SourceFile", "RowNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_compliance_keys_ImportSessionI~",
                table: "compliancecore_import_staged_compliance_keys",
                columns: new[] { "ImportSessionId", "CanonicalKeyCandidate" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_compliance_keys_ImportSessionId",
                table: "compliancecore_import_staged_compliance_keys",
                column: "ImportSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_compliance_keys_TenantId",
                table: "compliancecore_import_staged_compliance_keys",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_controlled_vocabulary_ImportS~1",
                table: "compliancecore_import_staged_controlled_vocabulary",
                columns: new[] { "ImportSessionId", "CanonicalKeyCandidate" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_controlled_vocabulary_ImportS~2",
                table: "compliancecore_import_staged_controlled_vocabulary",
                columns: new[] { "ImportSessionId", "SourceFile", "RowNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_controlled_vocabulary_ImportSe~",
                table: "compliancecore_import_staged_controlled_vocabulary",
                column: "ImportSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_controlled_vocabulary_TenantId",
                table: "compliancecore_import_staged_controlled_vocabulary",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_evidence_references_ImportSes~1",
                table: "compliancecore_import_staged_evidence_references",
                columns: new[] { "ImportSessionId", "CanonicalKeyCandidate" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_evidence_references_ImportSes~2",
                table: "compliancecore_import_staged_evidence_references",
                columns: new[] { "ImportSessionId", "SourceFile", "RowNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_evidence_references_ImportSess~",
                table: "compliancecore_import_staged_evidence_references",
                column: "ImportSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_evidence_references_TenantId",
                table: "compliancecore_import_staged_evidence_references",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_fact_requirements_ImportSessi~1",
                table: "compliancecore_import_staged_fact_requirements",
                columns: new[] { "ImportSessionId", "CanonicalKeyCandidate" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_fact_requirements_ImportSessi~2",
                table: "compliancecore_import_staged_fact_requirements",
                columns: new[] { "ImportSessionId", "SourceFile", "RowNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_fact_requirements_ImportSessio~",
                table: "compliancecore_import_staged_fact_requirements",
                column: "ImportSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_fact_requirements_TenantId",
                table: "compliancecore_import_staged_fact_requirements",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_mapping_candidates_ImportSess~1",
                table: "compliancecore_import_staged_mapping_candidates",
                columns: new[] { "ImportSessionId", "ConfidenceBand" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_mapping_candidates_ImportSessi~",
                table: "compliancecore_import_staged_mapping_candidates",
                column: "ImportSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_mapping_candidates_StagedRowId",
                table: "compliancecore_import_staged_mapping_candidates",
                column: "StagedRowId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_mapping_candidates_TenantId",
                table: "compliancecore_import_staged_mapping_candidates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_mapping_decisions_ImportSessio~",
                table: "compliancecore_import_staged_mapping_decisions",
                column: "ImportSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_mapping_decisions_MappingCandi~",
                table: "compliancecore_import_staged_mapping_decisions",
                column: "MappingCandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_mapping_decisions_StagedRowId",
                table: "compliancecore_import_staged_mapping_decisions",
                column: "StagedRowId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_mapping_decisions_TenantId",
                table: "compliancecore_import_staged_mapping_decisions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_material_keys_ImportSessionId",
                table: "compliancecore_import_staged_material_keys",
                column: "ImportSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_material_keys_ImportSessionId_~",
                table: "compliancecore_import_staged_material_keys",
                columns: new[] { "ImportSessionId", "CanonicalKeyCandidate" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_material_keys_ImportSessionId~1",
                table: "compliancecore_import_staged_material_keys",
                columns: new[] { "ImportSessionId", "SourceFile", "RowNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_material_keys_TenantId",
                table: "compliancecore_import_staged_material_keys",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_regulatory_mappings_ImportSes~1",
                table: "compliancecore_import_staged_regulatory_mappings",
                columns: new[] { "ImportSessionId", "CanonicalKeyCandidate" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_regulatory_mappings_ImportSes~2",
                table: "compliancecore_import_staged_regulatory_mappings",
                columns: new[] { "ImportSessionId", "SourceFile", "RowNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_regulatory_mappings_ImportSess~",
                table: "compliancecore_import_staged_regulatory_mappings",
                column: "ImportSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_regulatory_mappings_TenantId",
                table: "compliancecore_import_staged_regulatory_mappings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_rule_packs_ImportSessionId",
                table: "compliancecore_import_staged_rule_packs",
                column: "ImportSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_rule_packs_ImportSessionId_Can~",
                table: "compliancecore_import_staged_rule_packs",
                columns: new[] { "ImportSessionId", "CanonicalKeyCandidate" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_rule_packs_ImportSessionId_Sou~",
                table: "compliancecore_import_staged_rule_packs",
                columns: new[] { "ImportSessionId", "SourceFile", "RowNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_rule_packs_TenantId",
                table: "compliancecore_import_staged_rule_packs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_rule_requirements_ImportSessi~1",
                table: "compliancecore_import_staged_rule_requirements",
                columns: new[] { "ImportSessionId", "CanonicalKeyCandidate" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_rule_requirements_ImportSessi~2",
                table: "compliancecore_import_staged_rule_requirements",
                columns: new[] { "ImportSessionId", "SourceFile", "RowNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_rule_requirements_ImportSessio~",
                table: "compliancecore_import_staged_rule_requirements",
                column: "ImportSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_rule_requirements_TenantId",
                table: "compliancecore_import_staged_rule_requirements",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_sds_references_ImportSessionI~1",
                table: "compliancecore_import_staged_sds_references",
                columns: new[] { "ImportSessionId", "SourceFile", "RowNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_sds_references_ImportSessionId",
                table: "compliancecore_import_staged_sds_references",
                column: "ImportSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_sds_references_ImportSessionId~",
                table: "compliancecore_import_staged_sds_references",
                columns: new[] { "ImportSessionId", "CanonicalKeyCandidate" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_sds_references_TenantId",
                table: "compliancecore_import_staged_sds_references",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_vocabulary_aliases_ImportSess~1",
                table: "compliancecore_import_staged_vocabulary_aliases",
                columns: new[] { "ImportSessionId", "CanonicalKeyCandidate" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_vocabulary_aliases_ImportSess~2",
                table: "compliancecore_import_staged_vocabulary_aliases",
                columns: new[] { "ImportSessionId", "SourceFile", "RowNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_vocabulary_aliases_ImportSessi~",
                table: "compliancecore_import_staged_vocabulary_aliases",
                column: "ImportSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_import_staged_vocabulary_aliases_TenantId",
                table: "compliancecore_import_staged_vocabulary_aliases",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_material_references_TenantId",
                table: "compliancecore_material_references",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_material_references_TenantId_SourceProduct_E~",
                table: "compliancecore_material_references",
                columns: new[] { "TenantId", "SourceProduct", "ExternalRecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_material_references_TenantId_SourceProduct_O~",
                table: "compliancecore_material_references",
                columns: new[] { "TenantId", "SourceProduct", "ObjectKind" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_material_references_TenantId_StableKey",
                table: "compliancecore_material_references",
                columns: new[] { "TenantId", "StableKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_part_references_TenantId",
                table: "compliancecore_part_references",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_part_references_TenantId_SourceProduct_Exter~",
                table: "compliancecore_part_references",
                columns: new[] { "TenantId", "SourceProduct", "ExternalRecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_part_references_TenantId_SourceProduct_Objec~",
                table: "compliancecore_part_references",
                columns: new[] { "TenantId", "SourceProduct", "ObjectKind" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_part_references_TenantId_StableKey",
                table: "compliancecore_part_references",
                columns: new[] { "TenantId", "StableKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_system_references_TenantId",
                table: "compliancecore_system_references",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_system_references_TenantId_SourceProduct_Ext~",
                table: "compliancecore_system_references",
                columns: new[] { "TenantId", "SourceProduct", "ExternalRecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_system_references_TenantId_SourceProduct_Obj~",
                table: "compliancecore_system_references",
                columns: new[] { "TenantId", "SourceProduct", "ObjectKind" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_system_references_TenantId_StableKey",
                table: "compliancecore_system_references",
                columns: new[] { "TenantId", "StableKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliancecore_asset_references");

            migrationBuilder.DropTable(
                name: "compliancecore_document_references");

            migrationBuilder.DropTable(
                name: "compliancecore_evidence_options");

            migrationBuilder.DropTable(
                name: "compliancecore_external_object_references");

            migrationBuilder.DropTable(
                name: "compliancecore_import_session_source_files");

            migrationBuilder.DropTable(
                name: "compliancecore_import_staged_compliance_keys");

            migrationBuilder.DropTable(
                name: "compliancecore_import_staged_controlled_vocabulary");

            migrationBuilder.DropTable(
                name: "compliancecore_import_staged_evidence_references");

            migrationBuilder.DropTable(
                name: "compliancecore_import_staged_fact_requirements");

            migrationBuilder.DropTable(
                name: "compliancecore_import_staged_mapping_decisions");

            migrationBuilder.DropTable(
                name: "compliancecore_import_staged_material_keys");

            migrationBuilder.DropTable(
                name: "compliancecore_import_staged_regulatory_mappings");

            migrationBuilder.DropTable(
                name: "compliancecore_import_staged_rule_packs");

            migrationBuilder.DropTable(
                name: "compliancecore_import_staged_rule_requirements");

            migrationBuilder.DropTable(
                name: "compliancecore_import_staged_sds_references");

            migrationBuilder.DropTable(
                name: "compliancecore_import_staged_vocabulary_aliases");

            migrationBuilder.DropTable(
                name: "compliancecore_material_references");

            migrationBuilder.DropTable(
                name: "compliancecore_part_references");

            migrationBuilder.DropTable(
                name: "compliancecore_system_references");

            migrationBuilder.DropTable(
                name: "compliancecore_evidence_option_groups");

            migrationBuilder.DropTable(
                name: "compliancecore_import_staged_mapping_candidates");

            migrationBuilder.DropTable(
                name: "compliancecore_import_sessions");
        }
    }
}
