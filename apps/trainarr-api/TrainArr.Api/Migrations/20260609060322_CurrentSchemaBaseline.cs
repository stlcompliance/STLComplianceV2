using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class CurrentSchemaBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "trainarr_assignment_due_reminder_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsOfUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CandidatesFound = table.Column<int>(type: "integer", nullable: false),
                    RemindersSentCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_assignment_due_reminder_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_assignment_escalation_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EscalationCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_assignment_escalation_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_assignment_escalation_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsOfUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CandidatesFound = table.Column<int>(type: "integer", nullable: false),
                    EscalatedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_assignment_escalation_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_audit_events",
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
                    table.PrimaryKey("PK_trainarr_audit_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_audit_package_generation_jobs",
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
                    table.PrimaryKey("PK_trainarr_audit_package_generation_jobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_certification_publications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualificationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    QualificationName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PublicationType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BlockerType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Message = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_certification_publications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_evidence_retention_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EvidencePurgedCount = table.Column<int>(type: "integer", nullable: false),
                    BytesReclaimed = table.Column<long>(type: "bigint", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    SkipReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_evidence_retention_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_orphan_reference_findings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReferenceKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SampleSourceEntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SampleSourceEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    AffectedSourceCount = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    FirstDetectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastDetectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_orphan_reference_findings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_orphan_reference_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReferencesCheckedCount = table.Column<int>(type: "integer", nullable: false),
                    FindingsDetectedCount = table.Column<int>(type: "integer", nullable: false),
                    FindingsResolvedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    SkipReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_orphan_reference_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_person_training_history_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceDomainEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Summary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    RelatedEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_person_training_history_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_qualification_check_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualificationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Outcome = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Message = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    RulePackKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TrainingDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TrainingProgramId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    CheckedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_qualification_check_records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_qualification_recalculation_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualificationIssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CheckOutcome = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    SkipReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_qualification_recalculation_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_qualification_recalculation_states",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualificationIssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualificationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Outcome = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Message = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    RulePackKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PreviousOutcome = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_qualification_recalculation_states", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_recertification_assignment_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualificationIssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingAssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Outcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SkipReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_recertification_assignment_runs", x => x.Id);
                });

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
                name: "trainarr_staffarr_incident_remediations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrIncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceIncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceEventKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ReasonCategoryKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReportedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_staffarr_incident_remediations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_staffarr_publication_deliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CertificationPublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PayloadJson = table.Column<string>(type: "character varying(8192)", maxLength: 8192, nullable: false),
                    DeliveryStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    NextRetryAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    HttpStatusCode = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeliveredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_staffarr_publication_deliveries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_tenant_assignment_due_reminder_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DueSoonLeadDays = table.Column<int>(type: "integer", nullable: false),
                    ReminderCooldownHours = table.Column<int>(type: "integer", nullable: false),
                    MaxRemindersPerAssignment = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_tenant_assignment_due_reminder_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_tenant_assignment_escalation_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    OverdueEscalationAfterHours = table.Column<int>(type: "integer", nullable: false),
                    EscalationCooldownHours = table.Column<int>(type: "integer", nullable: false),
                    MaxEscalationsPerAssignment = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_tenant_assignment_escalation_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_tenant_event_processing_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    MaxAttempts = table.Column<int>(type: "integer", nullable: false),
                    RetryIntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_tenant_event_processing_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_tenant_evidence_retention_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    RetentionDaysAfterAssignmentClose = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_tenant_evidence_retention_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_tenant_integration_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffArrIntegrationEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    StaffArrIncidentIntakeEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    StaffArrPublicationDeliveryEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ComplianceCoreIntegrationEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ComplianceCoreQualificationChecksEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    RoutarrIntegrationEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    RoutarrQualificationDispatchEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_tenant_integration_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_tenant_orphan_reference_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ScanStalenessHours = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_tenant_orphan_reference_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_tenant_qualification_recalculation_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    StalenessHours = table.Column<int>(type: "integer", nullable: false),
                    AutoSuspendOnBlock = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_tenant_qualification_recalculation_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_tenant_recertification_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LeadDays = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_tenant_recertification_settings", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "trainarr_tenant_staffarr_publication_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    MaxAttempts = table.Column<int>(type: "integer", nullable: false),
                    RetryIntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_tenant_staffarr_publication_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_tenant_training_notification_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    NotificationWebhookUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    NotifyOnAssignmentCreated = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnQualificationExpiring = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnQualificationExpired = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnAssignmentCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnQualificationIssued = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnQualificationSuspended = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnQualificationRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnAssignmentDueReminder = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnAssignmentOverdueEscalation = table.Column<bool>(type: "boolean", nullable: false),
                    ExpiringLeadDays = table.Column<int>(type: "integer", nullable: false),
                    MaxAttempts = table.Column<int>(type: "integer", nullable: false),
                    RetryIntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_tenant_training_notification_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_applicability_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileKey = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ScopeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    SourceRecordId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_applicability_profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_assignment_material_demand_status_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainarrPublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyarrDemandRefId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyarrCallbackPublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProcurementStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SupplyarrPurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplyarrPurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplyarrReceivingReceiptId = table.Column<Guid>(type: "uuid", nullable: true),
                    Message = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_assignment_material_demand_status_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_citation_attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComplianceCoreCitationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CitationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CitationVersion = table.Column<int>(type: "integer", nullable: false),
                    AttachedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_citation_attachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefinitionKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    QualificationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    QualificationName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_domain_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelatedEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayloadJson = table.Column<string>(type: "character varying(8192)", maxLength: 8192, nullable: false),
                    ProcessingStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    NextRetryAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_domain_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_evaluation_revisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingEvaluationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Score = table.Column<decimal>(type: "numeric", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    EvaluatorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SupersededAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SupersededByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_evaluation_revisions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_notification_dispatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelatedEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    DispatchStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    NextRetryAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    WebhookHost = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    HttpStatusCode = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DispatchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_notification_dispatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_programs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_programs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_rule_pack_requirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    RulePackKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    KnownVersionNumber = table.Column<int>(type: "integer", nullable: true),
                    KnownStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    AttachedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_rule_pack_requirements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrIncidentRemediationId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceQualificationIssueId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignmentReason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    BlockerPublicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuthorizationQualificationCheckId = table.Column<Guid>(type: "uuid", nullable: true),
                    StaffarrAcknowledgementRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    StaffarrAcknowledgementStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    StaffarrAcknowledgementAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastDueReminderSentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DueReminderCount = table.Column<int>(type: "integer", nullable: false),
                    LastEscalatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EscalationCount = table.Column<int>(type: "integer", nullable: false),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_assignments_trainarr_staffarr_incident_re~",
                        column: x => x.StaffarrIncidentRemediationId,
                        principalTable: "trainarr_staffarr_incident_remediations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trainarr_training_assignments_trainarr_training_definitions~",
                        column: x => x.TrainingDefinitionId,
                        principalTable: "trainarr_training_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_definition_completion_rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RuleType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ConfigJson = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_definition_completion_rules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_definition_completion_rules_trainarr_trai~",
                        column: x => x.TrainingDefinitionId,
                        principalTable: "trainarr_training_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_definition_steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    StepType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ConfigJson = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_definition_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_definition_steps_trainarr_training_defini~",
                        column: x => x.TrainingDefinitionId,
                        principalTable: "trainarr_training_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_matrix_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicabilityKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ApplicabilityLabel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TrainingProgramId = table.Column<Guid>(type: "uuid", nullable: true),
                    TrainingDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequirementLevel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_matrix_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_matrix_entries_trainarr_training_definiti~",
                        column: x => x.TrainingDefinitionId,
                        principalTable: "trainarr_training_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trainarr_training_matrix_entries_trainarr_training_programs~",
                        column: x => x.TrainingProgramId,
                        principalTable: "trainarr_training_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_program_content_references",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ReferenceValue = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    LocaleTag = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_program_content_references", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_program_content_references_trainarr_train~",
                        column: x => x.TrainingProgramId,
                        principalTable: "trainarr_training_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_program_definitions",
                columns: table => new
                {
                    TrainingProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_program_definitions", x => new { x.TrainingProgramId, x.TrainingDefinitionId });
                    table.ForeignKey(
                        name: "FK_trainarr_training_program_definitions_trainarr_training_def~",
                        column: x => x.TrainingDefinitionId,
                        principalTable: "trainarr_training_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trainarr_training_program_definitions_trainarr_training_pro~",
                        column: x => x.TrainingProgramId,
                        principalTable: "trainarr_training_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_program_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublishedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_program_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_program_versions_trainarr_training_progra~",
                        column: x => x.TrainingProgramId,
                        principalTable: "trainarr_training_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_requirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    RequirementSource = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TrainingProgramId = table.Column<Guid>(type: "uuid", nullable: true),
                    TrainingDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApplicabilityProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequirementLevel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_requirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_requirements_trainarr_training_applicabil~",
                        column: x => x.ApplicabilityProfileId,
                        principalTable: "trainarr_training_applicability_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_trainarr_training_requirements_trainarr_training_definition~",
                        column: x => x.TrainingDefinitionId,
                        principalTable: "trainarr_training_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trainarr_training_requirements_trainarr_training_programs_T~",
                        column: x => x.TrainingProgramId,
                        principalTable: "trainarr_training_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_qualification_issues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualificationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    QualificationName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    GrantPublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IssuedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StatusChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LifecycleReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    LifecyclePublicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_qualification_issues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_qualification_issues_trainarr_training_assignments~",
                        column: x => x.TrainingAssignmentId,
                        principalTable: "trainarr_training_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_assignment_labor_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    LaborTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    HoursWorked = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CostPerHour = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    LoggedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LoggedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_assignment_labor_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_assignment_labor_entries_trainarr_trainin~",
                        column: x => x.TrainingAssignmentId,
                        principalTable: "trainarr_training_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_assignment_material_demand_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    SupplyarrPartId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    QuantityRequested = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TrainarrPublicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplyarrDemandRefId = table.Column<Guid>(type: "uuid", nullable: true),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProcurementStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SupplyarrPurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplyarrPurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuantityReceived = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ProcurementStatusMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    LastProcurementStatusAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_assignment_material_demand_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_assignment_material_demand_lines_trainarr~",
                        column: x => x.TrainingAssignmentId,
                        principalTable: "trainarr_training_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_evaluations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Score = table.Column<decimal>(type: "numeric", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    EvaluatorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_evaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_evaluations_trainarr_training_assignments~",
                        column: x => x.TrainingAssignmentId,
                        principalTable: "trainarr_training_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_evidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidenceTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_evidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_evidence_trainarr_training_assignments_Tr~",
                        column: x => x.TrainingAssignmentId,
                        principalTable: "trainarr_training_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_signoffs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SignoffRole = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SignedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    SignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_signoffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_signoffs_trainarr_training_assignments_Tr~",
                        column: x => x.TrainingAssignmentId,
                        principalTable: "trainarr_training_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_assignment_step_progress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingDefinitionStepId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    QuizScorePercent = table.Column<int>(type: "integer", nullable: true),
                    ResponseJson = table.Column<string>(type: "text", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_assignment_step_progress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_assignment_step_progress_trainarr_trainin~",
                        column: x => x.TrainingAssignmentId,
                        principalTable: "trainarr_training_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_trainarr_training_assignment_step_progress_trainarr_traini~1",
                        column: x => x.TrainingDefinitionStepId,
                        principalTable: "trainarr_training_definition_steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_definition_step_branches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingDefinitionStepId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BranchType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ConfigJson = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_definition_step_branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trainarr_training_definition_step_branches_trainarr_trainin~",
                        column: x => x.TrainingDefinitionStepId,
                        principalTable: "trainarr_training_definition_steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trainarr_training_program_version_definitions",
                columns: table => new
                {
                    TrainingProgramVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainarr_training_program_version_definitions", x => new { x.TrainingProgramVersionId, x.TrainingDefinitionId });
                    table.ForeignKey(
                        name: "FK_trainarr_training_program_version_definitions_trainarr_trai~",
                        column: x => x.TrainingDefinitionId,
                        principalTable: "trainarr_training_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trainarr_training_program_version_definitions_trainarr_tra~1",
                        column: x => x.TrainingProgramVersionId,
                        principalTable: "trainarr_training_program_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_platform_metadata_TenantId",
                table: "platform_metadata",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_platform_metadata_TenantId_Key",
                table: "platform_metadata",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_assignment_due_reminder_runs_TenantId",
                table: "trainarr_assignment_due_reminder_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_assignment_due_reminder_runs_TenantId_CreatedAt",
                table: "trainarr_assignment_due_reminder_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_assignment_escalation_events_TenantId",
                table: "trainarr_assignment_escalation_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_assignment_escalation_events_TenantId_TrainingAssi~",
                table: "trainarr_assignment_escalation_events",
                columns: new[] { "TenantId", "TrainingAssignmentId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_assignment_escalation_runs_TenantId",
                table: "trainarr_assignment_escalation_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_assignment_escalation_runs_TenantId_CreatedAt",
                table: "trainarr_assignment_escalation_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_audit_events_OccurredAt",
                table: "trainarr_audit_events",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_audit_events_TenantId",
                table: "trainarr_audit_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_audit_package_generation_jobs_CreatedAt",
                table: "trainarr_audit_package_generation_jobs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_audit_package_generation_jobs_TenantId_Status_Crea~",
                table: "trainarr_audit_package_generation_jobs",
                columns: new[] { "TenantId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_certification_publications_TenantId",
                table: "trainarr_certification_publications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_certification_publications_TenantId_StaffarrPerson~",
                table: "trainarr_certification_publications",
                columns: new[] { "TenantId", "StaffarrPersonId", "PublishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_evidence_retention_runs_TenantId",
                table: "trainarr_evidence_retention_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_evidence_retention_runs_TenantId_ProcessedAt",
                table: "trainarr_evidence_retention_runs",
                columns: new[] { "TenantId", "ProcessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_orphan_reference_findings_TenantId",
                table: "trainarr_orphan_reference_findings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_orphan_reference_findings_TenantId_IsActive_LastDe~",
                table: "trainarr_orphan_reference_findings",
                columns: new[] { "TenantId", "IsActive", "LastDetectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_orphan_reference_findings_TenantId_ReferenceKind_R~",
                table: "trainarr_orphan_reference_findings",
                columns: new[] { "TenantId", "ReferenceKind", "ReferenceKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_orphan_reference_runs_TenantId",
                table: "trainarr_orphan_reference_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_orphan_reference_runs_TenantId_ProcessedAt",
                table: "trainarr_orphan_reference_runs",
                columns: new[] { "TenantId", "ProcessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_person_training_history_entries_TenantId",
                table: "trainarr_person_training_history_entries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_person_training_history_entries_TenantId_SourceDom~",
                table: "trainarr_person_training_history_entries",
                columns: new[] { "TenantId", "SourceDomainEventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_person_training_history_entries_TenantId_StaffarrP~",
                table: "trainarr_person_training_history_entries",
                columns: new[] { "TenantId", "StaffarrPersonId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_check_records_TenantId",
                table: "trainarr_qualification_check_records",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_check_records_TenantId_BatchId",
                table: "trainarr_qualification_check_records",
                columns: new[] { "TenantId", "BatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_check_records_TenantId_Qualification~",
                table: "trainarr_qualification_check_records",
                columns: new[] { "TenantId", "QualificationKey", "CheckedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_check_records_TenantId_StaffarrPerso~",
                table: "trainarr_qualification_check_records",
                columns: new[] { "TenantId", "StaffarrPersonId", "CheckedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_issues_TenantId",
                table: "trainarr_qualification_issues",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_issues_TenantId_GrantPublicationId",
                table: "trainarr_qualification_issues",
                columns: new[] { "TenantId", "GrantPublicationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_issues_TenantId_LifecyclePublication~",
                table: "trainarr_qualification_issues",
                columns: new[] { "TenantId", "LifecyclePublicationId" },
                unique: true,
                filter: "\"LifecyclePublicationId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_issues_TenantId_Status_ExpiresAt",
                table: "trainarr_qualification_issues",
                columns: new[] { "TenantId", "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_issues_TenantId_TrainingAssignmentId",
                table: "trainarr_qualification_issues",
                columns: new[] { "TenantId", "TrainingAssignmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_issues_TrainingAssignmentId",
                table: "trainarr_qualification_issues",
                column: "TrainingAssignmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_recalculation_runs_TenantId",
                table: "trainarr_qualification_recalculation_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_recalculation_runs_TenantId_Processe~",
                table: "trainarr_qualification_recalculation_runs",
                columns: new[] { "TenantId", "ProcessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_recalculation_runs_TenantId_Qualific~",
                table: "trainarr_qualification_recalculation_runs",
                columns: new[] { "TenantId", "QualificationIssueId" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_recalculation_states_TenantId",
                table: "trainarr_qualification_recalculation_states",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_recalculation_states_TenantId_Comput~",
                table: "trainarr_qualification_recalculation_states",
                columns: new[] { "TenantId", "ComputedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_qualification_recalculation_states_TenantId_Qualif~",
                table: "trainarr_qualification_recalculation_states",
                columns: new[] { "TenantId", "QualificationIssueId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_recertification_assignment_runs_TenantId",
                table: "trainarr_recertification_assignment_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_recertification_assignment_runs_TenantId_Processed~",
                table: "trainarr_recertification_assignment_runs",
                columns: new[] { "TenantId", "ProcessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_recertification_assignment_runs_TenantId_Qualifica~",
                table: "trainarr_recertification_assignment_runs",
                columns: new[] { "TenantId", "QualificationIssueId", "Outcome" });

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
                name: "IX_trainarr_staffarr_incident_remediations_TenantId",
                table: "trainarr_staffarr_incident_remediations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_staffarr_incident_remediations_TenantId_SourceProd~",
                table: "trainarr_staffarr_incident_remediations",
                columns: new[] { "TenantId", "SourceProduct", "SourceIncidentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_staffarr_incident_remediations_TenantId_StaffarrIn~",
                table: "trainarr_staffarr_incident_remediations",
                columns: new[] { "TenantId", "StaffarrIncidentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_staffarr_incident_remediations_TenantId_StaffarrPe~",
                table: "trainarr_staffarr_incident_remediations",
                columns: new[] { "TenantId", "StaffarrPersonId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_staffarr_publication_deliveries_TenantId",
                table: "trainarr_staffarr_publication_deliveries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_staffarr_publication_deliveries_TenantId_Certifica~",
                table: "trainarr_staffarr_publication_deliveries",
                columns: new[] { "TenantId", "CertificationPublicationId", "OperationKind", "DeliveryStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_staffarr_publication_deliveries_TenantId_DeliveryS~",
                table: "trainarr_staffarr_publication_deliveries",
                columns: new[] { "TenantId", "DeliveryStatus", "NextRetryAt", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_tenant_assignment_due_reminder_settings_TenantId",
                table: "trainarr_tenant_assignment_due_reminder_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_tenant_assignment_escalation_settings_TenantId",
                table: "trainarr_tenant_assignment_escalation_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_tenant_event_processing_settings_TenantId",
                table: "trainarr_tenant_event_processing_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_tenant_evidence_retention_settings_TenantId",
                table: "trainarr_tenant_evidence_retention_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_tenant_integration_settings_TenantId",
                table: "trainarr_tenant_integration_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_tenant_orphan_reference_settings_TenantId",
                table: "trainarr_tenant_orphan_reference_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_tenant_qualification_recalculation_settings_Tenant~",
                table: "trainarr_tenant_qualification_recalculation_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_tenant_recertification_settings_TenantId",
                table: "trainarr_tenant_recertification_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_tenant_rule_pack_impact_settings_TenantId",
                table: "trainarr_tenant_rule_pack_impact_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_tenant_staffarr_publication_settings_TenantId",
                table: "trainarr_tenant_staffarr_publication_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_tenant_training_notification_settings_TenantId",
                table: "trainarr_tenant_training_notification_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_applicability_profiles_TenantId",
                table: "trainarr_training_applicability_profiles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_applicability_profiles_TenantId_ProfileKey",
                table: "trainarr_training_applicability_profiles",
                columns: new[] { "TenantId", "ProfileKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_labor_entries_TenantId",
                table: "trainarr_training_assignment_labor_entries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_labor_entries_TenantId_Trainin~",
                table: "trainarr_training_assignment_labor_entries",
                columns: new[] { "TenantId", "TrainingAssignmentId", "LoggedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_labor_entries_TrainingAssignme~",
                table: "trainarr_training_assignment_labor_entries",
                column: "TrainingAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_material_demand_lines_TenantI~1",
                table: "trainarr_training_assignment_material_demand_lines",
                columns: new[] { "TenantId", "TrainarrPublicationId" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_material_demand_lines_TenantI~2",
                table: "trainarr_training_assignment_material_demand_lines",
                columns: new[] { "TenantId", "TrainingAssignmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_material_demand_lines_TenantI~3",
                table: "trainarr_training_assignment_material_demand_lines",
                columns: new[] { "TenantId", "TrainingAssignmentId", "LineNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_material_demand_lines_TenantId",
                table: "trainarr_training_assignment_material_demand_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_material_demand_lines_TenantId~",
                table: "trainarr_training_assignment_material_demand_lines",
                columns: new[] { "TenantId", "ProcurementStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_material_demand_lines_Training~",
                table: "trainarr_training_assignment_material_demand_lines",
                column: "TrainingAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_material_demand_status_events_~",
                table: "trainarr_training_assignment_material_demand_status_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_material_demand_status_events~1",
                table: "trainarr_training_assignment_material_demand_status_events",
                columns: new[] { "TenantId", "SupplyarrCallbackPublicationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_material_demand_status_events~2",
                table: "trainarr_training_assignment_material_demand_status_events",
                columns: new[] { "TenantId", "TrainarrPublicationId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_step_progress_TenantId",
                table: "trainarr_training_assignment_step_progress",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_step_progress_TenantId_Trainin~",
                table: "trainarr_training_assignment_step_progress",
                columns: new[] { "TenantId", "TrainingAssignmentId", "TrainingDefinitionStepId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_step_progress_TrainingAssignme~",
                table: "trainarr_training_assignment_step_progress",
                column: "TrainingAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignment_step_progress_TrainingDefiniti~",
                table: "trainarr_training_assignment_step_progress",
                column: "TrainingDefinitionStepId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignments_StaffarrIncidentRemediationId",
                table: "trainarr_training_assignments",
                column: "StaffarrIncidentRemediationId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignments_TenantId",
                table: "trainarr_training_assignments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignments_TenantId_SourceQualificationI~",
                table: "trainarr_training_assignments",
                columns: new[] { "TenantId", "SourceQualificationIssueId" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignments_TenantId_StaffarrIncidentReme~",
                table: "trainarr_training_assignments",
                columns: new[] { "TenantId", "StaffarrIncidentRemediationId" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignments_TenantId_StaffarrPersonId_Cre~",
                table: "trainarr_training_assignments",
                columns: new[] { "TenantId", "StaffarrPersonId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_assignments_TrainingDefinitionId",
                table: "trainarr_training_assignments",
                column: "TrainingDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_citation_attachments_TenantId",
                table: "trainarr_training_citation_attachments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_citation_attachments_TenantId_EntityType_~",
                table: "trainarr_training_citation_attachments",
                columns: new[] { "TenantId", "EntityType", "EntityId", "CitationKey" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_citation_attachments_TenantId_EntityType~1",
                table: "trainarr_training_citation_attachments",
                columns: new[] { "TenantId", "EntityType", "EntityId", "ComplianceCoreCitationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_definition_completion_rules_TenantId",
                table: "trainarr_training_definition_completion_rules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_definition_completion_rules_TenantId_Trai~",
                table: "trainarr_training_definition_completion_rules",
                columns: new[] { "TenantId", "TrainingDefinitionId", "RuleKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_definition_completion_rules_TrainingDefin~",
                table: "trainarr_training_definition_completion_rules",
                column: "TrainingDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_definition_step_branches_TenantId",
                table: "trainarr_training_definition_step_branches",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_definition_step_branches_TenantId_Trainin~",
                table: "trainarr_training_definition_step_branches",
                columns: new[] { "TenantId", "TrainingDefinitionStepId", "BranchKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_definition_step_branches_TrainingDefiniti~",
                table: "trainarr_training_definition_step_branches",
                column: "TrainingDefinitionStepId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_definition_steps_TenantId",
                table: "trainarr_training_definition_steps",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_definition_steps_TenantId_TrainingDefinit~",
                table: "trainarr_training_definition_steps",
                columns: new[] { "TenantId", "TrainingDefinitionId", "StepKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_definition_steps_TrainingDefinitionId",
                table: "trainarr_training_definition_steps",
                column: "TrainingDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_definitions_TenantId",
                table: "trainarr_training_definitions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_definitions_TenantId_DefinitionKey",
                table: "trainarr_training_definitions",
                columns: new[] { "TenantId", "DefinitionKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_domain_events_TenantId",
                table: "trainarr_training_domain_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_domain_events_TenantId_IdempotencyKey",
                table: "trainarr_training_domain_events",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_domain_events_TenantId_ProcessingStatus_N~",
                table: "trainarr_training_domain_events",
                columns: new[] { "TenantId", "ProcessingStatus", "NextRetryAt", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_domain_events_TenantId_StaffarrPersonId_C~",
                table: "trainarr_training_domain_events",
                columns: new[] { "TenantId", "StaffarrPersonId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_evaluation_revisions_TenantId",
                table: "trainarr_training_evaluation_revisions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_evaluation_revisions_TenantId_TrainingAss~",
                table: "trainarr_training_evaluation_revisions",
                columns: new[] { "TenantId", "TrainingAssignmentId", "SupersededAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_evaluations_TenantId",
                table: "trainarr_training_evaluations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_evaluations_TenantId_TrainingAssignmentId",
                table: "trainarr_training_evaluations",
                columns: new[] { "TenantId", "TrainingAssignmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_evaluations_TrainingAssignmentId",
                table: "trainarr_training_evaluations",
                column: "TrainingAssignmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_evidence_TenantId",
                table: "trainarr_training_evidence",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_evidence_TenantId_TrainingAssignmentId_Cr~",
                table: "trainarr_training_evidence",
                columns: new[] { "TenantId", "TrainingAssignmentId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_evidence_TrainingAssignmentId",
                table: "trainarr_training_evidence",
                column: "TrainingAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_matrix_entries_TenantId",
                table: "trainarr_training_matrix_entries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_matrix_entries_TenantId_ApplicabilityKey_~",
                table: "trainarr_training_matrix_entries",
                columns: new[] { "TenantId", "ApplicabilityKey", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_matrix_entries_TrainingDefinitionId",
                table: "trainarr_training_matrix_entries",
                column: "TrainingDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_matrix_entries_TrainingProgramId",
                table: "trainarr_training_matrix_entries",
                column: "TrainingProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_notification_dispatches_TenantId",
                table: "trainarr_training_notification_dispatches",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_notification_dispatches_TenantId_Dispatch~",
                table: "trainarr_training_notification_dispatches",
                columns: new[] { "TenantId", "DispatchStatus", "NextRetryAt", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_notification_dispatches_TenantId_EventKin~",
                table: "trainarr_training_notification_dispatches",
                columns: new[] { "TenantId", "EventKind", "RelatedEntityType", "RelatedEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_program_content_references_TenantId",
                table: "trainarr_training_program_content_references",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_program_content_references_TenantId_Trai~1",
                table: "trainarr_training_program_content_references",
                columns: new[] { "TenantId", "TrainingProgramId", "ContentType", "ReferenceValue", "LocaleTag" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_program_content_references_TenantId_Train~",
                table: "trainarr_training_program_content_references",
                columns: new[] { "TenantId", "TrainingProgramId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_program_content_references_TrainingProgra~",
                table: "trainarr_training_program_content_references",
                column: "TrainingProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_program_definitions_TrainingDefinitionId",
                table: "trainarr_training_program_definitions",
                column: "TrainingDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_program_definitions_TrainingProgramId_Sor~",
                table: "trainarr_training_program_definitions",
                columns: new[] { "TrainingProgramId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_program_version_definitions_TrainingDefin~",
                table: "trainarr_training_program_version_definitions",
                column: "TrainingDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_program_version_definitions_TrainingProgr~",
                table: "trainarr_training_program_version_definitions",
                columns: new[] { "TrainingProgramVersionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_program_versions_TenantId",
                table: "trainarr_training_program_versions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_program_versions_TenantId_TrainingProgram~",
                table: "trainarr_training_program_versions",
                columns: new[] { "TenantId", "TrainingProgramId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_program_versions_TrainingProgramId",
                table: "trainarr_training_program_versions",
                column: "TrainingProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_programs_TenantId",
                table: "trainarr_training_programs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_programs_TenantId_ProgramKey",
                table: "trainarr_training_programs",
                columns: new[] { "TenantId", "ProgramKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_requirements_ApplicabilityProfileId",
                table: "trainarr_training_requirements",
                column: "ApplicabilityProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_requirements_TenantId",
                table: "trainarr_training_requirements",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_requirements_TenantId_RequirementKey",
                table: "trainarr_training_requirements",
                columns: new[] { "TenantId", "RequirementKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_requirements_TrainingDefinitionId",
                table: "trainarr_training_requirements",
                column: "TrainingDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_requirements_TrainingProgramId",
                table: "trainarr_training_requirements",
                column: "TrainingProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_rule_pack_requirements_TenantId",
                table: "trainarr_training_rule_pack_requirements",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_rule_pack_requirements_TenantId_EntityTyp~",
                table: "trainarr_training_rule_pack_requirements",
                columns: new[] { "TenantId", "EntityType", "EntityId", "RulePackKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_rule_pack_requirements_TenantId_RulePackK~",
                table: "trainarr_training_rule_pack_requirements",
                columns: new[] { "TenantId", "RulePackKey" });

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_signoffs_TenantId",
                table: "trainarr_training_signoffs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_signoffs_TenantId_TrainingAssignmentId_Si~",
                table: "trainarr_training_signoffs",
                columns: new[] { "TenantId", "TrainingAssignmentId", "SignoffRole" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trainarr_training_signoffs_TrainingAssignmentId",
                table: "trainarr_training_signoffs",
                column: "TrainingAssignmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "platform_metadata");

            migrationBuilder.DropTable(
                name: "trainarr_assignment_due_reminder_runs");

            migrationBuilder.DropTable(
                name: "trainarr_assignment_escalation_events");

            migrationBuilder.DropTable(
                name: "trainarr_assignment_escalation_runs");

            migrationBuilder.DropTable(
                name: "trainarr_audit_events");

            migrationBuilder.DropTable(
                name: "trainarr_audit_package_generation_jobs");

            migrationBuilder.DropTable(
                name: "trainarr_certification_publications");

            migrationBuilder.DropTable(
                name: "trainarr_evidence_retention_runs");

            migrationBuilder.DropTable(
                name: "trainarr_orphan_reference_findings");

            migrationBuilder.DropTable(
                name: "trainarr_orphan_reference_runs");

            migrationBuilder.DropTable(
                name: "trainarr_person_training_history_entries");

            migrationBuilder.DropTable(
                name: "trainarr_qualification_check_records");

            migrationBuilder.DropTable(
                name: "trainarr_qualification_issues");

            migrationBuilder.DropTable(
                name: "trainarr_qualification_recalculation_runs");

            migrationBuilder.DropTable(
                name: "trainarr_qualification_recalculation_states");

            migrationBuilder.DropTable(
                name: "trainarr_recertification_assignment_runs");

            migrationBuilder.DropTable(
                name: "trainarr_rule_pack_impact_runs");

            migrationBuilder.DropTable(
                name: "trainarr_rule_pack_impact_states");

            migrationBuilder.DropTable(
                name: "trainarr_staffarr_publication_deliveries");

            migrationBuilder.DropTable(
                name: "trainarr_tenant_assignment_due_reminder_settings");

            migrationBuilder.DropTable(
                name: "trainarr_tenant_assignment_escalation_settings");

            migrationBuilder.DropTable(
                name: "trainarr_tenant_event_processing_settings");

            migrationBuilder.DropTable(
                name: "trainarr_tenant_evidence_retention_settings");

            migrationBuilder.DropTable(
                name: "trainarr_tenant_integration_settings");

            migrationBuilder.DropTable(
                name: "trainarr_tenant_orphan_reference_settings");

            migrationBuilder.DropTable(
                name: "trainarr_tenant_qualification_recalculation_settings");

            migrationBuilder.DropTable(
                name: "trainarr_tenant_recertification_settings");

            migrationBuilder.DropTable(
                name: "trainarr_tenant_rule_pack_impact_settings");

            migrationBuilder.DropTable(
                name: "trainarr_tenant_staffarr_publication_settings");

            migrationBuilder.DropTable(
                name: "trainarr_tenant_training_notification_settings");

            migrationBuilder.DropTable(
                name: "trainarr_training_assignment_labor_entries");

            migrationBuilder.DropTable(
                name: "trainarr_training_assignment_material_demand_lines");

            migrationBuilder.DropTable(
                name: "trainarr_training_assignment_material_demand_status_events");

            migrationBuilder.DropTable(
                name: "trainarr_training_assignment_step_progress");

            migrationBuilder.DropTable(
                name: "trainarr_training_citation_attachments");

            migrationBuilder.DropTable(
                name: "trainarr_training_definition_completion_rules");

            migrationBuilder.DropTable(
                name: "trainarr_training_definition_step_branches");

            migrationBuilder.DropTable(
                name: "trainarr_training_domain_events");

            migrationBuilder.DropTable(
                name: "trainarr_training_evaluation_revisions");

            migrationBuilder.DropTable(
                name: "trainarr_training_evaluations");

            migrationBuilder.DropTable(
                name: "trainarr_training_evidence");

            migrationBuilder.DropTable(
                name: "trainarr_training_matrix_entries");

            migrationBuilder.DropTable(
                name: "trainarr_training_notification_dispatches");

            migrationBuilder.DropTable(
                name: "trainarr_training_program_content_references");

            migrationBuilder.DropTable(
                name: "trainarr_training_program_definitions");

            migrationBuilder.DropTable(
                name: "trainarr_training_program_version_definitions");

            migrationBuilder.DropTable(
                name: "trainarr_training_requirements");

            migrationBuilder.DropTable(
                name: "trainarr_training_rule_pack_requirements");

            migrationBuilder.DropTable(
                name: "trainarr_training_signoffs");

            migrationBuilder.DropTable(
                name: "trainarr_training_definition_steps");

            migrationBuilder.DropTable(
                name: "trainarr_training_program_versions");

            migrationBuilder.DropTable(
                name: "trainarr_training_applicability_profiles");

            migrationBuilder.DropTable(
                name: "trainarr_training_assignments");

            migrationBuilder.DropTable(
                name: "trainarr_training_programs");

            migrationBuilder.DropTable(
                name: "trainarr_staffarr_incident_remediations");

            migrationBuilder.DropTable(
                name: "trainarr_training_definitions");
        }
    }
}
