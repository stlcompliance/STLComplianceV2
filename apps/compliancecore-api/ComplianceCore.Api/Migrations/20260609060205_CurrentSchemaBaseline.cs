using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceCore.Api.Migrations
{
    /// <inheritdoc />
    public partial class CurrentSchemaBaseline : Migration
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
                name: "compliancecore_audit_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TargetType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TargetId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_audit_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_audit_package_generation_jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Format = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    FromUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ToUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PackageId = table.Column<Guid>(type: "uuid", nullable: true),
                    ArtifactZip = table.Column<byte[]>(type: "bytea", nullable: true),
                    ArtifactJson = table.Column<string>(type: "text", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_audit_package_generation_jobs", x => x.Id);
                });

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
                    ClaimedExceptionExemptionKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ClaimedExceptionExemptionType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExceptionExemptionLegalBasis = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ExceptionExemptionProofKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExceptionExemptionScopeResult = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExceptionExemptionEffectiveResult = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ResultBeforeException = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ResultAfterException = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FinalComplianceResult = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExceptionExemptionApplied = table.Column<bool>(type: "boolean", nullable: false),
                    ExceptionExemptionProofRequired = table.Column<bool>(type: "boolean", nullable: false),
                    ExceptionExemptionProofValid = table.Column<bool>(type: "boolean", nullable: false),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_audit_traces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_compliance_keys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_compliance_keys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_control_effectiveness_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PacksEvaluatedCount = table.Column<int>(type: "integer", nullable: false),
                    LowestEffectivenessScore = table.Column<int>(type: "integer", nullable: false),
                    LowestEffectivenessLevel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AverageEffectivenessScore = table.Column<int>(type: "integer", nullable: false),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_control_effectiveness_runs", x => x.Id);
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
                name: "compliancecore_fact_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FactKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ValueType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_fact_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_governing_bodies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BodyKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_governing_bodies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_hazcom_references",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    HazComKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    LinkedSdsKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    StaffarrSiteOrgUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    StaffarrSiteNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValue: ""),
                    LocationRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DocumentUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_hazcom_references", x => x.Id);
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
                name: "compliancecore_m12_analytics_batch_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IntervalHours = table.Column<int>(type: "integer", nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RiskScoringRan = table.Column<bool>(type: "boolean", nullable: false),
                    MissingEvidenceRan = table.Column<bool>(type: "boolean", nullable: false),
                    ControlEffectivenessRan = table.Column<bool>(type: "boolean", nullable: false),
                    ReadinessForecastRan = table.Column<bool>(type: "boolean", nullable: false),
                    AuditDeliveryQueued = table.Column<bool>(type: "boolean", nullable: false),
                    RiskScoreRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    MissingEvidenceWarningRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    ControlEffectivenessRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReadinessForecastRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuditPackageJobId = table.Column<Guid>(type: "uuid", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_m12_analytics_batch_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_material_keys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_material_keys", x => x.Id);
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
                name: "compliancecore_product_fact_mirrors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FactKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ValueType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StringValue = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    BooleanValue = table.Column<bool>(type: "boolean", nullable: true),
                    NumberValue = table.Column<decimal>(type: "numeric", nullable: true),
                    DateValue = table.Column<DateOnly>(type: "date", nullable: true),
                    SourceEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceEventKind = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourcePublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_product_fact_mirrors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_readiness_forecast_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PacksForecastCount = table.Column<int>(type: "integer", nullable: false),
                    ReadinessScore = table.Column<int>(type: "integer", nullable: false),
                    ReadinessLevel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    LowestReadinessScore = table.Column<int>(type: "integer", nullable: false),
                    AverageReadinessScore = table.Column<int>(type: "integer", nullable: false),
                    HighestRiskScore = table.Column<int>(type: "integer", nullable: false),
                    MissingEvidenceWarningCount = table.Column<int>(type: "integer", nullable: false),
                    AverageEffectivenessScore = table.Column<int>(type: "integer", nullable: false),
                    RiskScoreRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    MissingEvidenceWarningRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    ControlEffectivenessRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    ForecastedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_readiness_forecast_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_risk_score_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PacksEvaluatedCount = table.Column<int>(type: "integer", nullable: false),
                    HighestRiskScore = table.Column<int>(type: "integer", nullable: false),
                    HighestRiskLevel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_risk_score_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_rule_change_scan_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    PacksScannedCount = table.Column<int>(type: "integer", nullable: false),
                    ChangesDetectedCount = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_rule_change_scan_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_rule_pack_monitor_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ContentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CapturedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_rule_pack_monitor_snapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_scheduled_rule_evaluation_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IntervalHours = table.Column<int>(type: "integer", nullable: false),
                    PacksDueCount = table.Column<int>(type: "integer", nullable: false),
                    PacksProcessedCount = table.Column<int>(type: "integer", nullable: false),
                    EvaluatedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    AllowCount = table.Column<int>(type: "integer", nullable: false),
                    WarnCount = table.Column<int>(type: "integer", nullable: false),
                    BlockCount = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_scheduled_rule_evaluation_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_source_ingestion_batches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IngestionType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Phase = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    DryRun = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    TotalJobs = table.Column<int>(type: "integer", nullable: false),
                    SuccessCount = table.Column<int>(type: "integer", nullable: false),
                    ErrorCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceProduct = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    PublicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_source_ingestion_batches", x => x.Id);
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
                name: "compliancecore_tenant_fact_source_sync_worker_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    LastBatchRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_tenant_fact_source_sync_worker_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_tenant_m12_analytics_worker_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IntervalHours = table.Column<int>(type: "integer", nullable: false),
                    RiskScoringEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    MissingEvidenceEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ControlEffectivenessEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ReadinessForecastEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AuditDeliveryEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LastBatchRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastRiskScoringRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastMissingEvidenceRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastControlEffectivenessRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastReadinessForecastRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastAuditDeliveryRunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_tenant_m12_analytics_worker_settings", x => x.Id);
                });

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
                name: "compliancecore_vocabulary_terms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TermKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    VocabularyTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_vocabulary_terms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_vocabulary_types",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_vocabulary_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "platform_metadata",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_metadata", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_control_effectiveness_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RunId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EffectivenessScore = table.Column<int>(type: "integer", nullable: false),
                    EffectivenessLevel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ControlStatus = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    RuleOutcome = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    EvaluationResult = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    TotalRuleCount = table.Column<int>(type: "integer", nullable: false),
                    PassedRuleCount = table.Column<int>(type: "integer", nullable: false),
                    FailedRuleCount = table.Column<int>(type: "integer", nullable: false),
                    UnresolvedFactCount = table.Column<int>(type: "integer", nullable: false),
                    ResolvedFactCount = table.Column<int>(type: "integer", nullable: false),
                    Summary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_control_effectiveness_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_control_effectiveness_records_compliancecore~",
                        column: x => x.RunId,
                        principalTable: "compliancecore_control_effectiveness_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "compliancecore_fact_sources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FactDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ProductKey = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ProductReference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConfigJson = table.Column<string>(type: "jsonb", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_fact_sources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_fact_sources_compliancecore_fact_definitions~",
                        column: x => x.FactDefinitionId,
                        principalTable: "compliancecore_fact_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_jurisdictions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoverningBodyId = table.Column<Guid>(type: "uuid", nullable: false),
                    JurisdictionKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_jurisdictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_jurisdictions_compliancecore_governing_bodie~",
                        column: x => x.GoverningBodyId,
                        principalTable: "compliancecore_governing_bodies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "compliancecore_sds_references",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SdsKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MaterialKeyId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Manufacturer = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DocumentUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    RevisionDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_sds_references", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_sds_references_compliancecore_material_keys_~",
                        column: x => x.MaterialKeyId,
                        principalTable: "compliancecore_material_keys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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

            migrationBuilder.CreateTable(
                name: "compliancecore_readiness_forecasts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RunId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReadinessScore = table.Column<int>(type: "integer", nullable: false),
                    ReadinessLevel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    RiskScore = table.Column<int>(type: "integer", nullable: false),
                    RiskLevel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    EffectivenessScore = table.Column<int>(type: "integer", nullable: false),
                    EffectivenessLevel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MissingEvidenceWarningCount = table.Column<int>(type: "integer", nullable: false),
                    HighestMissingEvidenceSeverity = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Summary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ForecastedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_readiness_forecasts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_readiness_forecasts_compliancecore_readiness~",
                        column: x => x.RunId,
                        principalTable: "compliancecore_readiness_forecast_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_risk_scores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RunId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RiskScoreValue = table.Column<int>(type: "integer", nullable: false),
                    RiskLevel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    RuleOutcome = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    EvaluationResult = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    UnresolvedFactCount = table.Column<int>(type: "integer", nullable: false),
                    FailedRuleCount = table.Column<int>(type: "integer", nullable: false),
                    ResolvedFactCount = table.Column<int>(type: "integer", nullable: false),
                    MirrorFactCount = table.Column<int>(type: "integer", nullable: false),
                    Summary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_risk_scores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_risk_scores_compliancecore_risk_score_runs_R~",
                        column: x => x.RunId,
                        principalTable: "compliancecore_risk_score_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_rule_change_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProgramKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ChangeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Summary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    FromVersion = table.Column<int>(type: "integer", nullable: true),
                    ToVersion = table.Column<int>(type: "integer", nullable: true),
                    PreviousContentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    NewContentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Source = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScanRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    DetectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_rule_change_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_rule_change_events_compliancecore_rule_chang~",
                        column: x => x.ScanRunId,
                        principalTable: "compliancecore_rule_change_scan_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_source_ingestion_jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowIndex = table.Column<int>(type: "integer", nullable: false),
                    JobKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    ErrorCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Message = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_source_ingestion_jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_source_ingestion_jobs_compliancecore_source_~",
                        column: x => x.BatchId,
                        principalTable: "compliancecore_source_ingestion_batches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "compliancecore_vocabulary_aliases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    VocabularyTermId = table.Column<Guid>(type: "uuid", nullable: false),
                    AliasText = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_vocabulary_aliases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_vocabulary_aliases_compliancecore_vocabulary~",
                        column: x => x.VocabularyTermId,
                        principalTable: "compliancecore_vocabulary_terms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_fact_source_sync_statuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FactSourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    HealthStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LastAttemptAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastSuccessAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastFailureAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ConsecutiveFailureCount = table.Column<int>(type: "integer", nullable: false),
                    LastMirrorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_fact_source_sync_statuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_fact_source_sync_statuses_compliancecore_fac~",
                        column: x => x.FactSourceId,
                        principalTable: "compliancecore_fact_sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_regulatory_programs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_regulatory_programs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_regulatory_programs_compliancecore_jurisdict~",
                        column: x => x.JurisdictionId,
                        principalTable: "compliancecore_jurisdictions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    EvidenceMappingPurpose = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExceptionExemptionKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ResidualRequirementsJson = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
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
                    NormalRuleResult = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExceptionExemptionKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExceptionExemptionType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExceptionExemptionLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExceptionExemptionConsidered = table.Column<bool>(type: "boolean", nullable: false),
                    ExceptionExemptionApplies = table.Column<bool>(type: "boolean", nullable: false),
                    ExceptionExemptionProofRequired = table.Column<bool>(type: "boolean", nullable: false),
                    ExceptionExemptionProofValid = table.Column<bool>(type: "boolean", nullable: false),
                    ResultBeforeException = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ResultAfterException = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FinalComplianceResult = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
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

            migrationBuilder.CreateTable(
                name: "compliancecore_rule_packs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegulatoryProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RuleContentJson = table.Column<string>(type: "jsonb", nullable: true),
                    LastScheduledEvaluationAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_rule_packs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_rule_packs_compliancecore_regulatory_program~",
                        column: x => x.RegulatoryProgramId,
                        principalTable: "compliancecore_regulatory_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_regulatory_citations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegulatoryProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: true),
                    CitationKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceReference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    SupersedesCitationId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_regulatory_citations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_regulatory_citations_program",
                        column: x => x.RegulatoryProgramId,
                        principalTable: "compliancecore_regulatory_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_compliancecore_regulatory_citations_rule_pack",
                        column: x => x.RulePackId,
                        principalTable: "compliancecore_rule_packs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_compliancecore_regulatory_citations_supersedes",
                        column: x => x.SupersedesCitationId,
                        principalTable: "compliancecore_regulatory_citations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                    AppliedWaiverId = table.Column<Guid>(type: "uuid", nullable: true),
                    AppliedWaiverKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
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

            migrationBuilder.CreateTable(
                name: "compliancecore_waivers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WaiverKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RuleKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    GateKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SubjectScopeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Explanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EffectiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_waivers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_waivers_compliancecore_rule_packs_RulePackId",
                        column: x => x.RulePackId,
                        principalTable: "compliancecore_rule_packs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_workflow_gate_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: false),
                    GateKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_workflow_gate_definitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_workflow_gate_definitions_compliancecore_rul~",
                        column: x => x.RulePackId,
                        principalTable: "compliancecore_rule_packs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_fact_requirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FactDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: true),
                    CitationId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequirementKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ApplicabilityKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceEntity = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SourceFieldOrRecordType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ValueType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Operator = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExpectedValue = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    EvidenceKind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequiredDocumentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RetentionPeriod = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AuditQuestion = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    FailureSeverity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AutomaticFailureFlag = table.Column<bool>(type: "boolean", nullable: false),
                    OverrideAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    OverridePermission = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RemediationRequired = table.Column<bool>(type: "boolean", nullable: false),
                    ExternallyAssertable = table.Column<bool>(type: "boolean", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_fact_requirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_fact_requirements_compliancecore_fact_defini~",
                        column: x => x.FactDefinitionId,
                        principalTable: "compliancecore_fact_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_compliancecore_fact_requirements_compliancecore_regulatory_~",
                        column: x => x.CitationId,
                        principalTable: "compliancecore_regulatory_citations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_compliancecore_fact_requirements_compliancecore_rule_packs_~",
                        column: x => x.RulePackId,
                        principalTable: "compliancecore_rule_packs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_regulatory_mappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MappingKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    TargetKind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RegulatoryProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: true),
                    CitationId = table.Column<Guid>(type: "uuid", nullable: true),
                    FactDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ComplianceKeyId = table.Column<Guid>(type: "uuid", nullable: true),
                    MaterialKeyId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_regulatory_mappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_regulatory_mappings_citation",
                        column: x => x.CitationId,
                        principalTable: "compliancecore_regulatory_citations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_compliancecore_regulatory_mappings_compliance_key",
                        column: x => x.ComplianceKeyId,
                        principalTable: "compliancecore_compliance_keys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_compliancecore_regulatory_mappings_fact_definition",
                        column: x => x.FactDefinitionId,
                        principalTable: "compliancecore_fact_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_compliancecore_regulatory_mappings_material_key",
                        column: x => x.MaterialKeyId,
                        principalTable: "compliancecore_material_keys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_compliancecore_regulatory_mappings_program",
                        column: x => x.RegulatoryProgramId,
                        principalTable: "compliancecore_regulatory_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_compliancecore_regulatory_mappings_rule_pack",
                        column: x => x.RulePackId,
                        principalTable: "compliancecore_rule_packs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_findings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RulePackId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleEvaluationRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    FindingKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Severity = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RuleKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FactKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Message = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_findings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_findings_compliancecore_rule_evaluation_runs~",
                        column: x => x.RuleEvaluationRunId,
                        principalTable: "compliancecore_rule_evaluation_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_compliancecore_findings_compliancecore_rule_packs_RulePackId",
                        column: x => x.RulePackId,
                        principalTable: "compliancecore_rule_packs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_workflow_gate_check_results",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowGateDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleEvaluationRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    GateKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Outcome = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Message = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ReasonsJson = table.Column<string>(type: "jsonb", nullable: false),
                    ContextJson = table.Column<string>(type: "jsonb", nullable: false),
                    AppliedWaiverId = table.Column<Guid>(type: "uuid", nullable: true),
                    AppliedWaiverKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_workflow_gate_check_results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_workflow_gate_check_results_compliancecore_r~",
                        column: x => x.RuleEvaluationRunId,
                        principalTable: "compliancecore_rule_evaluation_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_compliancecore_workflow_gate_check_results_compliancecore_w~",
                        column: x => x.WorkflowGateDefinitionId,
                        principalTable: "compliancecore_workflow_gate_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "compliancecore_product_gate_responses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowGateCheckResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ResponseOutcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ResponseCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ResponseMessage = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ResponsePayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    RespondedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compliancecore_product_gate_responses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_compliancecore_product_gate_responses_compliancecore_workfl~",
                        column: x => x.WorkflowGateCheckResultId,
                        principalTable: "compliancecore_workflow_gate_check_results",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_compliancecore_audit_events_OccurredAt",
                table: "compliancecore_audit_events",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_audit_events_TenantId",
                table: "compliancecore_audit_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_audit_package_generation_jobs_CreatedAt",
                table: "compliancecore_audit_package_generation_jobs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_audit_package_generation_jobs_TenantId_Statu~",
                table: "compliancecore_audit_package_generation_jobs",
                columns: new[] { "TenantId", "Status", "CreatedAt" });

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
                name: "IX_compliancecore_audit_traces_TenantId_ClaimedExceptionExempt~",
                table: "compliancecore_audit_traces",
                columns: new[] { "TenantId", "ClaimedExceptionExemptionKey" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_audit_traces_TenantId_FactKey",
                table: "compliancecore_audit_traces",
                columns: new[] { "TenantId", "FactKey" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_audit_traces_TenantId_PackKey",
                table: "compliancecore_audit_traces",
                columns: new[] { "TenantId", "PackKey" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_compliance_keys_TenantId",
                table: "compliancecore_compliance_keys",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_compliance_keys_TenantId_Key",
                table: "compliancecore_compliance_keys",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_control_effectiveness_records_RunId",
                table: "compliancecore_control_effectiveness_records",
                column: "RunId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_control_effectiveness_records_TenantId",
                table: "compliancecore_control_effectiveness_records",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_control_effectiveness_records_TenantId_Scope~",
                table: "compliancecore_control_effectiveness_records",
                columns: new[] { "TenantId", "ScopeKey", "PackKey", "EvaluatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_control_effectiveness_runs_TenantId",
                table: "compliancecore_control_effectiveness_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_control_effectiveness_runs_TenantId_Evaluate~",
                table: "compliancecore_control_effectiveness_runs",
                columns: new[] { "TenantId", "EvaluatedAt" });

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

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_definitions_TenantId",
                table: "compliancecore_fact_definitions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_definitions_TenantId_FactKey",
                table: "compliancecore_fact_definitions",
                columns: new[] { "TenantId", "FactKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_requirements_CitationId",
                table: "compliancecore_fact_requirements",
                column: "CitationId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_requirements_FactDefinitionId",
                table: "compliancecore_fact_requirements",
                column: "FactDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_requirements_RulePackId",
                table: "compliancecore_fact_requirements",
                column: "RulePackId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_requirements_TenantId",
                table: "compliancecore_fact_requirements",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_requirements_TenantId_RequirementKey",
                table: "compliancecore_fact_requirements",
                columns: new[] { "TenantId", "RequirementKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_requirements_TenantId_SourceEntity",
                table: "compliancecore_fact_requirements",
                columns: new[] { "TenantId", "SourceEntity" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_requirements_TenantId_SourceProduct",
                table: "compliancecore_fact_requirements",
                columns: new[] { "TenantId", "SourceProduct" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_source_sync_statuses_FactSourceId",
                table: "compliancecore_fact_source_sync_statuses",
                column: "FactSourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_source_sync_statuses_TenantId",
                table: "compliancecore_fact_source_sync_statuses",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_sources_FactDefinitionId",
                table: "compliancecore_fact_sources",
                column: "FactDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_sources_TenantId",
                table: "compliancecore_fact_sources",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_fact_sources_TenantId_SourceKey",
                table: "compliancecore_fact_sources",
                columns: new[] { "TenantId", "SourceKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_findings_CreatedAt",
                table: "compliancecore_findings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_findings_RuleEvaluationRunId",
                table: "compliancecore_findings",
                column: "RuleEvaluationRunId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_findings_RulePackId",
                table: "compliancecore_findings",
                column: "RulePackId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_findings_TenantId",
                table: "compliancecore_findings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_findings_TenantId_FindingKey",
                table: "compliancecore_findings",
                columns: new[] { "TenantId", "FindingKey" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_governing_bodies_TenantId",
                table: "compliancecore_governing_bodies",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_governing_bodies_TenantId_BodyKey",
                table: "compliancecore_governing_bodies",
                columns: new[] { "TenantId", "BodyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_hazcom_references_TenantId",
                table: "compliancecore_hazcom_references",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_hazcom_references_TenantId_HazComKey",
                table: "compliancecore_hazcom_references",
                columns: new[] { "TenantId", "HazComKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_hazcom_references_TenantId_StaffarrSiteOrgUn~",
                table: "compliancecore_hazcom_references",
                columns: new[] { "TenantId", "StaffarrSiteOrgUnitId" });

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
                name: "IX_compliancecore_jurisdictions_GoverningBodyId",
                table: "compliancecore_jurisdictions",
                column: "GoverningBodyId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_jurisdictions_TenantId",
                table: "compliancecore_jurisdictions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_jurisdictions_TenantId_JurisdictionKey",
                table: "compliancecore_jurisdictions",
                columns: new[] { "TenantId", "JurisdictionKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_m12_analytics_batch_runs_TenantId_StartedAt",
                table: "compliancecore_m12_analytics_batch_runs",
                columns: new[] { "TenantId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_material_keys_TenantId",
                table: "compliancecore_material_keys",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_material_keys_TenantId_Key",
                table: "compliancecore_material_keys",
                columns: new[] { "TenantId", "Key" },
                unique: true);

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
                name: "IX_compliancecore_product_fact_mirrors_TenantId",
                table: "compliancecore_product_fact_mirrors",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_product_fact_mirrors_TenantId_IdempotencyKey",
                table: "compliancecore_product_fact_mirrors",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_product_fact_mirrors_TenantId_SourceProduct_~",
                table: "compliancecore_product_fact_mirrors",
                columns: new[] { "TenantId", "SourceProduct", "FactKey", "ScopeKey" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_product_gate_responses_TenantId",
                table: "compliancecore_product_gate_responses",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_product_gate_responses_TenantId_WorkflowGate~",
                table: "compliancecore_product_gate_responses",
                columns: new[] { "TenantId", "WorkflowGateCheckResultId", "RespondedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_product_gate_responses_WorkflowGateCheckResu~",
                table: "compliancecore_product_gate_responses",
                column: "WorkflowGateCheckResultId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_readiness_forecast_runs_TenantId",
                table: "compliancecore_readiness_forecast_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_readiness_forecast_runs_TenantId_ForecastedAt",
                table: "compliancecore_readiness_forecast_runs",
                columns: new[] { "TenantId", "ForecastedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_readiness_forecasts_RunId",
                table: "compliancecore_readiness_forecasts",
                column: "RunId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_readiness_forecasts_TenantId",
                table: "compliancecore_readiness_forecasts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_readiness_forecasts_TenantId_ScopeKey_PackKe~",
                table: "compliancecore_readiness_forecasts",
                columns: new[] { "TenantId", "ScopeKey", "PackKey", "ForecastedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_citations_RegulatoryProgramId",
                table: "compliancecore_regulatory_citations",
                column: "RegulatoryProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_citations_RulePackId",
                table: "compliancecore_regulatory_citations",
                column: "RulePackId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_citations_SupersedesCitationId",
                table: "compliancecore_regulatory_citations",
                column: "SupersedesCitationId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_citations_TenantId",
                table: "compliancecore_regulatory_citations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_citations_TenantId_CitationKey_Ve~",
                table: "compliancecore_regulatory_citations",
                columns: new[] { "TenantId", "CitationKey", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_mappings_CitationId",
                table: "compliancecore_regulatory_mappings",
                column: "CitationId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_mappings_ComplianceKeyId",
                table: "compliancecore_regulatory_mappings",
                column: "ComplianceKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_mappings_FactDefinitionId",
                table: "compliancecore_regulatory_mappings",
                column: "FactDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_mappings_MaterialKeyId",
                table: "compliancecore_regulatory_mappings",
                column: "MaterialKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_mappings_RegulatoryProgramId",
                table: "compliancecore_regulatory_mappings",
                column: "RegulatoryProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_mappings_RulePackId",
                table: "compliancecore_regulatory_mappings",
                column: "RulePackId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_mappings_TenantId",
                table: "compliancecore_regulatory_mappings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_mappings_TenantId_MappingKey",
                table: "compliancecore_regulatory_mappings",
                columns: new[] { "TenantId", "MappingKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_programs_JurisdictionId",
                table: "compliancecore_regulatory_programs",
                column: "JurisdictionId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_programs_TenantId",
                table: "compliancecore_regulatory_programs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_regulatory_programs_TenantId_ProgramKey",
                table: "compliancecore_regulatory_programs",
                columns: new[] { "TenantId", "ProgramKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_risk_score_runs_TenantId",
                table: "compliancecore_risk_score_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_risk_score_runs_TenantId_EvaluatedAt",
                table: "compliancecore_risk_score_runs",
                columns: new[] { "TenantId", "EvaluatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_risk_scores_RunId",
                table: "compliancecore_risk_scores",
                column: "RunId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_risk_scores_TenantId",
                table: "compliancecore_risk_scores",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_risk_scores_TenantId_ScopeKey_PackKey_Evalua~",
                table: "compliancecore_risk_scores",
                columns: new[] { "TenantId", "ScopeKey", "PackKey", "EvaluatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_change_events_RulePackId",
                table: "compliancecore_rule_change_events",
                column: "RulePackId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_change_events_ScanRunId",
                table: "compliancecore_rule_change_events",
                column: "ScanRunId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_change_events_TenantId",
                table: "compliancecore_rule_change_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_change_events_TenantId_PackKey_Detected~",
                table: "compliancecore_rule_change_events",
                columns: new[] { "TenantId", "PackKey", "DetectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_change_scan_runs_StartedAt",
                table: "compliancecore_rule_change_scan_runs",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_change_scan_runs_TenantId",
                table: "compliancecore_rule_change_scan_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_evaluation_runs_AppliedWaiverId",
                table: "compliancecore_rule_evaluation_runs",
                column: "AppliedWaiverId");

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

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_pack_monitor_snapshots_RulePackId",
                table: "compliancecore_rule_pack_monitor_snapshots",
                column: "RulePackId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_pack_monitor_snapshots_TenantId",
                table: "compliancecore_rule_pack_monitor_snapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_packs_RegulatoryProgramId",
                table: "compliancecore_rule_packs",
                column: "RegulatoryProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_packs_TenantId",
                table: "compliancecore_rule_packs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_packs_TenantId_PackKey_VersionNumber",
                table: "compliancecore_rule_packs",
                columns: new[] { "TenantId", "PackKey", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_rule_packs_TenantId_Status_LastScheduledEval~",
                table: "compliancecore_rule_packs",
                columns: new[] { "TenantId", "Status", "LastScheduledEvaluationAt" });

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

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_scheduled_rule_evaluation_runs_StartedAt",
                table: "compliancecore_scheduled_rule_evaluation_runs",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_scheduled_rule_evaluation_runs_TenantId",
                table: "compliancecore_scheduled_rule_evaluation_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_sds_references_MaterialKeyId",
                table: "compliancecore_sds_references",
                column: "MaterialKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_sds_references_TenantId",
                table: "compliancecore_sds_references",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_sds_references_TenantId_SdsKey",
                table: "compliancecore_sds_references",
                columns: new[] { "TenantId", "SdsKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_source_ingestion_batches_TenantId",
                table: "compliancecore_source_ingestion_batches",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_source_ingestion_batches_TenantId_IngestionT~",
                table: "compliancecore_source_ingestion_batches",
                columns: new[] { "TenantId", "IngestionType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_source_ingestion_jobs_BatchId",
                table: "compliancecore_source_ingestion_jobs",
                column: "BatchId");

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

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_tenant_fact_source_sync_worker_settings_Tena~",
                table: "compliancecore_tenant_fact_source_sync_worker_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_tenant_m12_analytics_worker_settings_TenantId",
                table: "compliancecore_tenant_m12_analytics_worker_settings",
                column: "TenantId",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_vocabulary_aliases_TenantId",
                table: "compliancecore_vocabulary_aliases",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_vocabulary_aliases_TenantId_VocabularyTermId~",
                table: "compliancecore_vocabulary_aliases",
                columns: new[] { "TenantId", "VocabularyTermId", "AliasText" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_vocabulary_aliases_VocabularyTermId",
                table: "compliancecore_vocabulary_aliases",
                column: "VocabularyTermId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_vocabulary_terms_TenantId",
                table: "compliancecore_vocabulary_terms",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_vocabulary_terms_TenantId_TermKey",
                table: "compliancecore_vocabulary_terms",
                columns: new[] { "TenantId", "TermKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_vocabulary_terms_TenantId_VocabularyTypeKey",
                table: "compliancecore_vocabulary_terms",
                columns: new[] { "TenantId", "VocabularyTypeKey" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_vocabulary_types_SortOrder",
                table: "compliancecore_vocabulary_types",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_vocabulary_types_TypeKey",
                table: "compliancecore_vocabulary_types",
                column: "TypeKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_waivers_RulePackId",
                table: "compliancecore_waivers",
                column: "RulePackId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_waivers_TenantId",
                table: "compliancecore_waivers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_waivers_TenantId_Status_ExpiresAt",
                table: "compliancecore_waivers",
                columns: new[] { "TenantId", "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_waivers_TenantId_WaiverKey",
                table: "compliancecore_waivers",
                columns: new[] { "TenantId", "WaiverKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_workflow_gate_check_results_AppliedWaiverId",
                table: "compliancecore_workflow_gate_check_results",
                column: "AppliedWaiverId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_workflow_gate_check_results_CreatedAt",
                table: "compliancecore_workflow_gate_check_results",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_workflow_gate_check_results_RuleEvaluationRu~",
                table: "compliancecore_workflow_gate_check_results",
                column: "RuleEvaluationRunId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_workflow_gate_check_results_TenantId",
                table: "compliancecore_workflow_gate_check_results",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_workflow_gate_check_results_WorkflowGateDefi~",
                table: "compliancecore_workflow_gate_check_results",
                column: "WorkflowGateDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_workflow_gate_definitions_RulePackId",
                table: "compliancecore_workflow_gate_definitions",
                column: "RulePackId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_workflow_gate_definitions_TenantId",
                table: "compliancecore_workflow_gate_definitions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_compliancecore_workflow_gate_definitions_TenantId_GateKey",
                table: "compliancecore_workflow_gate_definitions",
                columns: new[] { "TenantId", "GateKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_metadata_TenantId",
                table: "platform_metadata",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_platform_metadata_TenantId_Key",
                table: "platform_metadata",
                columns: new[] { "TenantId", "Key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compliance_exception_exemption");

            migrationBuilder.DropTable(
                name: "compliancecore_asset_references");

            migrationBuilder.DropTable(
                name: "compliancecore_audit_events");

            migrationBuilder.DropTable(
                name: "compliancecore_audit_package_generation_jobs");

            migrationBuilder.DropTable(
                name: "compliancecore_audit_traces");

            migrationBuilder.DropTable(
                name: "compliancecore_control_effectiveness_records");

            migrationBuilder.DropTable(
                name: "compliancecore_document_references");

            migrationBuilder.DropTable(
                name: "compliancecore_evidence_options");

            migrationBuilder.DropTable(
                name: "compliancecore_external_object_references");

            migrationBuilder.DropTable(
                name: "compliancecore_fact_assertions");

            migrationBuilder.DropTable(
                name: "compliancecore_fact_requirements");

            migrationBuilder.DropTable(
                name: "compliancecore_fact_source_sync_statuses");

            migrationBuilder.DropTable(
                name: "compliancecore_findings");

            migrationBuilder.DropTable(
                name: "compliancecore_hazcom_references");

            migrationBuilder.DropTable(
                name: "compliancecore_import_session_source_files");

            migrationBuilder.DropTable(
                name: "compliancecore_import_staged_compliance_keys");

            migrationBuilder.DropTable(
                name: "compliancecore_import_staged_controlled_vocabulary");

            migrationBuilder.DropTable(
                name: "compliancecore_import_staged_evidence_references");

            migrationBuilder.DropTable(
                name: "compliancecore_import_staged_exception_exemptions");

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
                name: "compliancecore_m12_analytics_batch_runs");

            migrationBuilder.DropTable(
                name: "compliancecore_material_references");

            migrationBuilder.DropTable(
                name: "compliancecore_missing_evidence_warnings");

            migrationBuilder.DropTable(
                name: "compliancecore_part_references");

            migrationBuilder.DropTable(
                name: "compliancecore_product_fact_mirrors");

            migrationBuilder.DropTable(
                name: "compliancecore_product_gate_responses");

            migrationBuilder.DropTable(
                name: "compliancecore_readiness_forecasts");

            migrationBuilder.DropTable(
                name: "compliancecore_regulatory_mappings");

            migrationBuilder.DropTable(
                name: "compliancecore_risk_scores");

            migrationBuilder.DropTable(
                name: "compliancecore_rule_change_events");

            migrationBuilder.DropTable(
                name: "compliancecore_rule_pack_monitor_snapshots");

            migrationBuilder.DropTable(
                name: "compliancecore_rule_test_cases");

            migrationBuilder.DropTable(
                name: "compliancecore_scheduled_rule_evaluation_runs");

            migrationBuilder.DropTable(
                name: "compliancecore_sds_references");

            migrationBuilder.DropTable(
                name: "compliancecore_source_ingestion_jobs");

            migrationBuilder.DropTable(
                name: "compliancecore_system_references");

            migrationBuilder.DropTable(
                name: "compliancecore_tenant_fact_source_sync_worker_settings");

            migrationBuilder.DropTable(
                name: "compliancecore_tenant_m12_analytics_worker_settings");

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
                name: "compliancecore_vocabulary_aliases");

            migrationBuilder.DropTable(
                name: "compliancecore_vocabulary_types");

            migrationBuilder.DropTable(
                name: "compliancecore_waivers");

            migrationBuilder.DropTable(
                name: "platform_metadata");

            migrationBuilder.DropTable(
                name: "compliancecore_control_effectiveness_runs");

            migrationBuilder.DropTable(
                name: "compliancecore_evidence_option_groups");

            migrationBuilder.DropTable(
                name: "compliancecore_evidence_references");

            migrationBuilder.DropTable(
                name: "compliancecore_fact_sources");

            migrationBuilder.DropTable(
                name: "compliancecore_import_staged_mapping_candidates");

            migrationBuilder.DropTable(
                name: "compliancecore_missing_evidence_warning_runs");

            migrationBuilder.DropTable(
                name: "compliancecore_workflow_gate_check_results");

            migrationBuilder.DropTable(
                name: "compliancecore_readiness_forecast_runs");

            migrationBuilder.DropTable(
                name: "compliancecore_regulatory_citations");

            migrationBuilder.DropTable(
                name: "compliancecore_compliance_keys");

            migrationBuilder.DropTable(
                name: "compliancecore_risk_score_runs");

            migrationBuilder.DropTable(
                name: "compliancecore_rule_change_scan_runs");

            migrationBuilder.DropTable(
                name: "compliancecore_material_keys");

            migrationBuilder.DropTable(
                name: "compliancecore_source_ingestion_batches");

            migrationBuilder.DropTable(
                name: "compliancecore_theoretical_situation_evaluations");

            migrationBuilder.DropTable(
                name: "compliancecore_vocabulary_terms");

            migrationBuilder.DropTable(
                name: "compliancecore_fact_definitions");

            migrationBuilder.DropTable(
                name: "compliancecore_import_sessions");

            migrationBuilder.DropTable(
                name: "compliancecore_rule_evaluation_runs");

            migrationBuilder.DropTable(
                name: "compliancecore_workflow_gate_definitions");

            migrationBuilder.DropTable(
                name: "compliancecore_theoretical_situations");

            migrationBuilder.DropTable(
                name: "compliancecore_rule_packs");

            migrationBuilder.DropTable(
                name: "compliancecore_regulatory_programs");

            migrationBuilder.DropTable(
                name: "compliancecore_jurisdictions");

            migrationBuilder.DropTable(
                name: "compliancecore_governing_bodies");
        }
    }
}
