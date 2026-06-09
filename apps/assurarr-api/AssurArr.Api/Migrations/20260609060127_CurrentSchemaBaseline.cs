using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssurArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class CurrentSchemaBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assurarr_audit_findings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceRequirementRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    EvidenceRecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    FindingType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AuditRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NonconformanceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CapaRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_audit_findings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_capa_action_blockers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CapaActionId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockerType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResolvedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_capa_action_blockers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_capa_actions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CapaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ActionType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AssignedPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedTeamRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SourceProductActionRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TargetProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TargetObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationRequired = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VerifiedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvidenceRecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    BlockerRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_capa_actions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_capas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    StaffArrSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    StaffArrLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    ActionPlanRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    VerificationPlanRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RelatedCustomerComplaintRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    RelatedSupplierIssueRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    ComplianceRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    AuditTrail = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OpenedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CapaType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SponsorPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RootCauseSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RelatedNonconformanceRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    RelatedAuditFindingRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    EffectivenessVerificationRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_capas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_containment_actions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    NonconformanceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ActionType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AssignedPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedTeamRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SourceProductActionRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationRequired = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EvidenceRecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_containment_actions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_customer_complaint_quality_cases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedOrderRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    AffectedShipmentRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    AffectedItemRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    AffectedAssetRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CustomerRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CustomerContactSnapshot = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CustomerLocationRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NonconformanceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    HoldRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CapaRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CustomerResponseRecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ComplaintType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReceivedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerResponseDueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_customer_complaint_quality_cases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_dispositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    NonconformanceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DispositionType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DecisionByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecisionAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Rationale = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RequiredActions = table.Column<string[]>(type: "text[]", nullable: false),
                    ExecutionProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ExecutionObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EvidenceRecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_dispositions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_effectiveness_verifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CapaId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerificationPlanId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PerformedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    PerformedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResultSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    EvidenceRecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    MetricResults = table.Column<string[]>(type: "text[]", nullable: false),
                    RecurrenceFound = table.Column<bool>(type: "boolean", nullable: false),
                    FollowUpRequired = table.Column<bool>(type: "boolean", nullable: false),
                    ReopenedCapaRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_effectiveness_verifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_nonconformances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DiscoveredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DiscoveredByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    StaffArrSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    StaffArrLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    ContainmentRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    AffectedItemRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    AffectedAssetRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    AffectedOrderRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    AffectedSupplierRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    AffectedCustomerRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    AffectedShipmentRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    DispositionRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CapaRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    ComplianceRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    FinancialImpactSnapshot = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    AuditTrail = table.Column<string[]>(type: "text[]", nullable: false),
                    HoldRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    NonconformanceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CustomerImpact = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SupplierImpact = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SafetyImpact = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ComplianceImpact = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RecurrenceFlag = table.Column<bool>(type: "boolean", nullable: false),
                    RepeatOfNonconformanceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RootCauseRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    BlockerRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_nonconformances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_quality_audit_checklist_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ChecklistId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Prompt = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    HelpText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RequirementRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ResponseType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    ResponseValue = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Result = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FindingCreated = table.Column<bool>(type: "boolean", nullable: false),
                    FindingRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EvidenceRecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    AnsweredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AnsweredByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_quality_audit_checklist_items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_quality_audit_checklists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AuditId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ItemRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_quality_audit_checklists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_quality_audits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    AuditType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AuditScope = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    StandardRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    ComplianceRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    AuditorPersonIds = table.Column<string[]>(type: "text[]", nullable: false),
                    LeadAuditorPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuditeeRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    StaffArrSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    StaffArrLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplierRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CustomerRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PlannedStartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PlannedEndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActualStartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActualEndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ChecklistRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    FindingRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    AuditTrail = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_quality_audits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_quality_holds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceNonconformanceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    StaffArrSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    StaffArrLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    AuditTrail = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    HoldType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    HoldScope = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    HoldReason = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ReleaseReason = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ConditionalReleaseTerms = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ReleaseRequirements = table.Column<string[]>(type: "text[]", nullable: false),
                    ReleaseApprovalRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    QuantityHeld = table.Column<decimal>(type: "numeric", nullable: true),
                    UnitOfMeasure = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    LotNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SerialNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PlacedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PlacedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReleasedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReleasedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejectedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_quality_holds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_quality_metrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScorecardId = table.Column<Guid>(type: "uuid", nullable: false),
                    MetricKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Numerator = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Denominator = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Unit = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TargetValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    WarningThreshold = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    CriticalThreshold = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProductRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_quality_metrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_quality_releases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    HoldRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ReleaseType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RequestedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExecutedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Conditions = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ExpirationAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EvidenceRecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_quality_releases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_quality_reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ReviewType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceReviewRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ReviewerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DecisionAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DecisionReason = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RequiredEvidenceRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    SubmittedEvidenceRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_quality_reviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_quality_risk_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TargetRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RiskLevel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RiskFactors = table.Column<string[]>(type: "text[]", nullable: false),
                    OpenIssueCount = table.Column<int>(type: "integer", nullable: false),
                    RepeatIssueCount = table.Column<int>(type: "integer", nullable: false),
                    CriticalIssueCount = table.Column<int>(type: "integer", nullable: false),
                    LastIncidentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MitigationActions = table.Column<string[]>(type: "text[]", nullable: false),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_quality_risk_profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_quality_scorecards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    TargetType = table.Column<string>(type: "text", nullable: false),
                    TargetRef = table.Column<string>(type: "text", nullable: false),
                    PeriodStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OverallScore = table.Column<decimal>(type: "numeric", nullable: true),
                    QualityStatus = table.Column<string>(type: "text", nullable: false),
                    Trend = table.Column<string>(type: "text", nullable: false),
                    GeneratedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    GeneratedBy = table.Column<string>(type: "text", nullable: false),
                    ReviewedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MetricRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_quality_scorecards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_quality_status_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    TargetProduct = table.Column<string>(type: "text", nullable: false),
                    TargetObjectRef = table.Column<string>(type: "text", nullable: false),
                    QualityStatus = table.Column<string>(type: "text", nullable: false),
                    ActiveHoldRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OpenNonconformanceRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OpenCapaRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OpenFindingRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    LastReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_quality_status_snapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_root_cause_analyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NonconformanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Method = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PrimaryCauseCategory = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RootCauseSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ContributingFactors = table.Column<string[]>(type: "text[]", nullable: false),
                    AnalyzedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EvidenceRecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_root_cause_analyses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_supplier_corrective_action_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedObjectRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    SupplierRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SourceNonconformanceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SourceCapaRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RequestedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SupplierDueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SupplierResponseRecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    ReviewPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewDecision = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    FollowUpCapaRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_supplier_corrective_action_requests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_supplier_quality_issues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AffectedReceiptRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    AffectedPurchaseOrderRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    AffectedItemRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    SupplierRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NonconformanceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ScarRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    HoldRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    RecordRefs = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosureSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    IssueType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OwnerPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    OpenedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_supplier_quality_issues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_timeline_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Details = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_timeline_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "assurarr_verification_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CapaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    VerificationType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SuccessCriteria = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    SampleSize = table.Column<int>(type: "integer", nullable: true),
                    ObservationPeriodDays = table.Column<int>(type: "integer", nullable: true),
                    RequiredEvidenceTypes = table.Column<string[]>(type: "text[]", nullable: false),
                    ResponsiblePersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlannedVerificationAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByPersonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assurarr_verification_plans", x => x.Id);
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

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_audit_findings_TenantId",
                table: "assurarr_audit_findings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_audit_findings_TenantId_Number",
                table: "assurarr_audit_findings",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_audit_findings_TenantId_Status",
                table: "assurarr_audit_findings",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_capa_action_blockers_TenantId",
                table: "assurarr_capa_action_blockers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_capa_action_blockers_TenantId_CapaActionId",
                table: "assurarr_capa_action_blockers",
                columns: new[] { "TenantId", "CapaActionId" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_capa_action_blockers_TenantId_Number",
                table: "assurarr_capa_action_blockers",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_capa_action_blockers_TenantId_Status",
                table: "assurarr_capa_action_blockers",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_capa_actions_TenantId",
                table: "assurarr_capa_actions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_capa_actions_TenantId_CapaId",
                table: "assurarr_capa_actions",
                columns: new[] { "TenantId", "CapaId" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_capa_actions_TenantId_Number",
                table: "assurarr_capa_actions",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_capa_actions_TenantId_Status",
                table: "assurarr_capa_actions",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_capas_TenantId",
                table: "assurarr_capas",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_capas_TenantId_Number",
                table: "assurarr_capas",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_capas_TenantId_Status",
                table: "assurarr_capas",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_containment_actions_TenantId",
                table: "assurarr_containment_actions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_containment_actions_TenantId_NonconformanceRef",
                table: "assurarr_containment_actions",
                columns: new[] { "TenantId", "NonconformanceRef" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_containment_actions_TenantId_Number",
                table: "assurarr_containment_actions",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_containment_actions_TenantId_Status",
                table: "assurarr_containment_actions",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_customer_complaint_quality_cases_TenantId",
                table: "assurarr_customer_complaint_quality_cases",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_customer_complaint_quality_cases_TenantId_Customer~",
                table: "assurarr_customer_complaint_quality_cases",
                columns: new[] { "TenantId", "CustomerRef" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_customer_complaint_quality_cases_TenantId_Number",
                table: "assurarr_customer_complaint_quality_cases",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_customer_complaint_quality_cases_TenantId_Status",
                table: "assurarr_customer_complaint_quality_cases",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_dispositions_TenantId",
                table: "assurarr_dispositions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_dispositions_TenantId_NonconformanceRef",
                table: "assurarr_dispositions",
                columns: new[] { "TenantId", "NonconformanceRef" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_dispositions_TenantId_Number",
                table: "assurarr_dispositions",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_dispositions_TenantId_Status",
                table: "assurarr_dispositions",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_effectiveness_verifications_TenantId",
                table: "assurarr_effectiveness_verifications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_effectiveness_verifications_TenantId_CapaId",
                table: "assurarr_effectiveness_verifications",
                columns: new[] { "TenantId", "CapaId" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_effectiveness_verifications_TenantId_Number",
                table: "assurarr_effectiveness_verifications",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_effectiveness_verifications_TenantId_Status",
                table: "assurarr_effectiveness_verifications",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_nonconformances_TenantId",
                table: "assurarr_nonconformances",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_nonconformances_TenantId_Number",
                table: "assurarr_nonconformances",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_nonconformances_TenantId_Status",
                table: "assurarr_nonconformances",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_audit_checklist_items_TenantId",
                table: "assurarr_quality_audit_checklist_items",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_audit_checklist_items_TenantId_ChecklistId",
                table: "assurarr_quality_audit_checklist_items",
                columns: new[] { "TenantId", "ChecklistId" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_audit_checklist_items_TenantId_Number",
                table: "assurarr_quality_audit_checklist_items",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_audit_checklist_items_TenantId_Sequence",
                table: "assurarr_quality_audit_checklist_items",
                columns: new[] { "TenantId", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_audit_checklists_TenantId",
                table: "assurarr_quality_audit_checklists",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_audit_checklists_TenantId_AuditId",
                table: "assurarr_quality_audit_checklists",
                columns: new[] { "TenantId", "AuditId" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_audit_checklists_TenantId_Number",
                table: "assurarr_quality_audit_checklists",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_audit_checklists_TenantId_Status",
                table: "assurarr_quality_audit_checklists",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_audits_TenantId",
                table: "assurarr_quality_audits",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_audits_TenantId_Number",
                table: "assurarr_quality_audits",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_audits_TenantId_Status",
                table: "assurarr_quality_audits",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_holds_TenantId",
                table: "assurarr_quality_holds",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_holds_TenantId_Number",
                table: "assurarr_quality_holds",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_holds_TenantId_Status",
                table: "assurarr_quality_holds",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_metrics_TenantId_MetricKey",
                table: "assurarr_quality_metrics",
                columns: new[] { "TenantId", "MetricKey" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_metrics_TenantId_ScorecardId",
                table: "assurarr_quality_metrics",
                columns: new[] { "TenantId", "ScorecardId" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_releases_TenantId",
                table: "assurarr_quality_releases",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_releases_TenantId_HoldRef",
                table: "assurarr_quality_releases",
                columns: new[] { "TenantId", "HoldRef" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_releases_TenantId_Number",
                table: "assurarr_quality_releases",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_releases_TenantId_Status",
                table: "assurarr_quality_releases",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_reviews_TenantId",
                table: "assurarr_quality_reviews",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_reviews_TenantId_Number",
                table: "assurarr_quality_reviews",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_reviews_TenantId_Status",
                table: "assurarr_quality_reviews",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_risk_profiles_TenantId_RiskLevel",
                table: "assurarr_quality_risk_profiles",
                columns: new[] { "TenantId", "RiskLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_risk_profiles_TenantId_TargetType_TargetRef",
                table: "assurarr_quality_risk_profiles",
                columns: new[] { "TenantId", "TargetType", "TargetRef" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_scorecards_TenantId",
                table: "assurarr_quality_scorecards",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_scorecards_TenantId_Number",
                table: "assurarr_quality_scorecards",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_scorecards_TenantId_Status",
                table: "assurarr_quality_scorecards",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_status_snapshots_TenantId",
                table: "assurarr_quality_status_snapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_status_snapshots_TenantId_Number",
                table: "assurarr_quality_status_snapshots",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_quality_status_snapshots_TenantId_Status",
                table: "assurarr_quality_status_snapshots",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_root_cause_analyses_TenantId",
                table: "assurarr_root_cause_analyses",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_root_cause_analyses_TenantId_NonconformanceId",
                table: "assurarr_root_cause_analyses",
                columns: new[] { "TenantId", "NonconformanceId" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_root_cause_analyses_TenantId_Number",
                table: "assurarr_root_cause_analyses",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_root_cause_analyses_TenantId_Status",
                table: "assurarr_root_cause_analyses",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_supplier_corrective_action_requests_TenantId",
                table: "assurarr_supplier_corrective_action_requests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_supplier_corrective_action_requests_TenantId_Number",
                table: "assurarr_supplier_corrective_action_requests",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_supplier_corrective_action_requests_TenantId_Sourc~",
                table: "assurarr_supplier_corrective_action_requests",
                columns: new[] { "TenantId", "SourceNonconformanceRef" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_supplier_corrective_action_requests_TenantId_Status",
                table: "assurarr_supplier_corrective_action_requests",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_supplier_corrective_action_requests_TenantId_Suppl~",
                table: "assurarr_supplier_corrective_action_requests",
                columns: new[] { "TenantId", "SupplierRef" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_supplier_quality_issues_TenantId",
                table: "assurarr_supplier_quality_issues",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_supplier_quality_issues_TenantId_Number",
                table: "assurarr_supplier_quality_issues",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_supplier_quality_issues_TenantId_Status",
                table: "assurarr_supplier_quality_issues",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_supplier_quality_issues_TenantId_SupplierRef",
                table: "assurarr_supplier_quality_issues",
                columns: new[] { "TenantId", "SupplierRef" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_timeline_events_TenantId_OccurredAt",
                table: "assurarr_timeline_events",
                columns: new[] { "TenantId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_timeline_events_TenantId_SubjectType_SubjectId",
                table: "assurarr_timeline_events",
                columns: new[] { "TenantId", "SubjectType", "SubjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_verification_plans_TenantId",
                table: "assurarr_verification_plans",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_verification_plans_TenantId_CapaId",
                table: "assurarr_verification_plans",
                columns: new[] { "TenantId", "CapaId" });

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_verification_plans_TenantId_Number",
                table: "assurarr_verification_plans",
                columns: new[] { "TenantId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assurarr_verification_plans_TenantId_Status",
                table: "assurarr_verification_plans",
                columns: new[] { "TenantId", "Status" });

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
                name: "assurarr_audit_findings");

            migrationBuilder.DropTable(
                name: "assurarr_capa_action_blockers");

            migrationBuilder.DropTable(
                name: "assurarr_capa_actions");

            migrationBuilder.DropTable(
                name: "assurarr_capas");

            migrationBuilder.DropTable(
                name: "assurarr_containment_actions");

            migrationBuilder.DropTable(
                name: "assurarr_customer_complaint_quality_cases");

            migrationBuilder.DropTable(
                name: "assurarr_dispositions");

            migrationBuilder.DropTable(
                name: "assurarr_effectiveness_verifications");

            migrationBuilder.DropTable(
                name: "assurarr_nonconformances");

            migrationBuilder.DropTable(
                name: "assurarr_quality_audit_checklist_items");

            migrationBuilder.DropTable(
                name: "assurarr_quality_audit_checklists");

            migrationBuilder.DropTable(
                name: "assurarr_quality_audits");

            migrationBuilder.DropTable(
                name: "assurarr_quality_holds");

            migrationBuilder.DropTable(
                name: "assurarr_quality_metrics");

            migrationBuilder.DropTable(
                name: "assurarr_quality_releases");

            migrationBuilder.DropTable(
                name: "assurarr_quality_reviews");

            migrationBuilder.DropTable(
                name: "assurarr_quality_risk_profiles");

            migrationBuilder.DropTable(
                name: "assurarr_quality_scorecards");

            migrationBuilder.DropTable(
                name: "assurarr_quality_status_snapshots");

            migrationBuilder.DropTable(
                name: "assurarr_root_cause_analyses");

            migrationBuilder.DropTable(
                name: "assurarr_supplier_corrective_action_requests");

            migrationBuilder.DropTable(
                name: "assurarr_supplier_quality_issues");

            migrationBuilder.DropTable(
                name: "assurarr_timeline_events");

            migrationBuilder.DropTable(
                name: "assurarr_verification_plans");

            migrationBuilder.DropTable(
                name: "platform_metadata");
        }
    }
}
