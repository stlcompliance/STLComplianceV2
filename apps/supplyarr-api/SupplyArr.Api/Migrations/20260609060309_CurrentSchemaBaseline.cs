using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupplyArr.Api.Migrations
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
                name: "supplyarr_approval_reminder_runs",
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
                    table.PrimaryKey("PK_supplyarr_approval_reminder_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_approval_reminder_states",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DocumentStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    VendorPartyId = table.Column<Guid>(type: "uuid", nullable: true),
                    PendingSince = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastReminderSentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReminderCount = table.Column<int>(type: "integer", nullable: false),
                    LastReminderEventKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_approval_reminder_states", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_audit_events",
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
                    table.PrimaryKey("PK_supplyarr_audit_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_availability_snapshot_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsOfUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CandidatesFound = table.Column<int>(type: "integer", nullable: false),
                    CapturedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_availability_snapshot_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_demand_processing_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsOfUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CandidatesFound = table.Column<int>(type: "integer", nullable: false),
                    ProcessedCount = table.Column<int>(type: "integer", nullable: false),
                    PrDraftsCreatedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_demand_processing_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_demand_processing_states",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DemandRefId = table.Column<Guid>(type: "uuid", nullable: false),
                    DemandRefSource = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MaintainarrWorkOrderNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ProcessingOutcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RecommendedAction = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LinesTotalCount = table.Column<int>(type: "integer", nullable: false),
                    LinesCatalogCount = table.Column<int>(type: "integer", nullable: false),
                    LinesShortCount = table.Column<int>(type: "integer", nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastProcessingMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    DemandReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_demand_processing_states", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_external_parties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartyKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PartyType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LegalName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TaxIdentifier = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ApprovalStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_external_parties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_integration_event_processing_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OutboxProcessedCount = table.Column<int>(type: "integer", nullable: false),
                    InboxProcessedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    AbandonedCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_integration_event_processing_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_integration_inbox_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EventKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RelatedEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelatedEntityId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    ProcessingStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    NextRetryAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_integration_inbox_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_integration_outbox_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RelatedEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    ProcessingStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    NextRetryAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_integration_outbox_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_inventory_locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LocationType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AddressLine = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    StaffarrSiteOrgUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    StaffarrSiteNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValue: ""),
                    StaffarrSiteResolutionStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "unassigned"),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_inventory_locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_lead_time_snapshot_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsOfUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CandidatesFound = table.Column<int>(type: "integer", nullable: false),
                    CapturedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_lead_time_snapshot_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_notification_dispatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    VendorPartyId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    DispatchStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    WebhookHost = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    HttpStatusCode = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DispatchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_notification_dispatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_part_catalogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_part_catalogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_price_snapshot_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsOfUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CandidatesFound = table.Column<int>(type: "integer", nullable: false),
                    CapturedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_price_snapshot_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_procurement_coordination_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CoordinationStage = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NextActionRequired = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    VendorPartyId = table.Column<Guid>(type: "uuid", nullable: true),
                    VendorDisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DocumentStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LineCount = table.Column<int>(type: "integer", nullable: false),
                    QuantityOrdered = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityReceived = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ReceiptProgressPercent = table.Column<int>(type: "integer", nullable: true),
                    IsTerminal = table.Column<bool>(type: "boolean", nullable: false),
                    SourceUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_procurement_coordination_records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_procurement_coordination_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsOfUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CandidatesFound = table.Column<int>(type: "integer", nullable: false),
                    RefreshedCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_procurement_coordination_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_procurement_exception_escalation_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcurementExceptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EscalationLevel = table.Column<int>(type: "integer", nullable: false),
                    ActionKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NotificationDispatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_procurement_exception_escalation_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_procurement_exception_escalation_runs",
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
                    table.PrimaryKey("PK_supplyarr_procurement_exception_escalation_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_procurement_exceptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExceptionKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SubjectType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    VendorPartyId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExceptionCategory = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ResolutionNotes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    WaiveJustification = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    WaiveRejectionReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SlaDueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastEscalatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EscalationCount = table.Column<int>(type: "integer", nullable: false),
                    ResolutionTemplateKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LinkedPurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    LinkedPurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    InvestigatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    InvestigatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    WaiveRequestedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    WaiveRequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    WaivedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    WaivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ReopenedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReopenedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastReopenReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ReopenCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_procurement_exceptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_staffarr_procurement_approval_authority_mirrors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CanSubmitPurchaseRequests = table.Column<bool>(type: "boolean", nullable: false),
                    CanApprovePurchaseRequests = table.Column<bool>(type: "boolean", nullable: false),
                    CanIssuePurchaseOrders = table.Column<bool>(type: "boolean", nullable: false),
                    MaxSubmitAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    MaxApproveAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    MaxIssueAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    OrgUnitScopeIdsJson = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    GrantsJson = table.Column<string>(type: "character varying(16384)", maxLength: 16384, nullable: false),
                    SourceComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RefreshedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_staffarr_procurement_approval_authority_mirrors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_tenant_approval_reminder_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    PrReminderAfterHours = table.Column<int>(type: "integer", nullable: false),
                    PoReminderAfterHours = table.Column<int>(type: "integer", nullable: false),
                    ReminderCooldownHours = table.Column<int>(type: "integer", nullable: false),
                    MaxRemindersPerSubject = table.Column<int>(type: "integer", nullable: false),
                    NotifyOnPrApprovalReminder = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnPoApprovalReminder = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_tenant_approval_reminder_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_tenant_availability_snapshot_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    StalenessHours = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_tenant_availability_snapshot_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_tenant_demand_processing_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AutoCreatePrDraftWhenShort = table.Column<bool>(type: "boolean", nullable: false),
                    MinHoursBeforeProcessing = table.Column<int>(type: "integer", nullable: false),
                    StalenessHours = table.Column<int>(type: "integer", nullable: false),
                    NotifyOnPrDraftCreated = table.Column<bool>(type: "boolean", nullable: false),
                    ProcessMaintainarrDemandRefs = table.Column<bool>(type: "boolean", nullable: false),
                    ProcessRoutarrDemandRefs = table.Column<bool>(type: "boolean", nullable: false),
                    ProcessTrainarrDemandRefs = table.Column<bool>(type: "boolean", nullable: false),
                    ProcessStaffarrDemandRefs = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_tenant_demand_processing_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_tenant_integration_event_settings",
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
                    table.PrimaryKey("PK_supplyarr_tenant_integration_event_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_tenant_lead_time_snapshot_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    StalenessHours = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_tenant_lead_time_snapshot_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_tenant_notification_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    NotificationWebhookUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    NotifyOnPurchaseRequestSubmitted = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnPurchaseRequestApproved = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnPurchaseOrderIssued = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnReceivingReceiptPosted = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_tenant_notification_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_tenant_price_snapshot_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    StalenessHours = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_tenant_price_snapshot_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_tenant_procurement_coordination_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    StalenessHours = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_tenant_procurement_coordination_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_tenant_procurement_exception_escalation_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    EscalationCooldownHours = table.Column<int>(type: "integer", nullable: false, defaultValue: 24),
                    MaxEscalationsPerException = table.Column<int>(type: "integer", nullable: false, defaultValue: 5),
                    NotifyOnProcurementExceptionSlaEscalation = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    AutoCloseCompletedExceptionsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AutoCloseCompletedExceptionsAfterHours = table.Column<int>(type: "integer", nullable: false, defaultValue: 48),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_tenant_procurement_exception_escalation_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_tenant_supplier_onboarding_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequiredDocumentTypeKeysJson = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_tenant_supplier_onboarding_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_vendor_email_inbox_messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MessageKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SenderEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SenderName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Subject = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    BodyPreview = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    MatchStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MatchReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    VendorPartyId = table.Column<Guid>(type: "uuid", nullable: true),
                    VendorPartyKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VendorDisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LinkedReferenceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    LinkedReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    LinkedReferenceKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_vendor_email_inbox_messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_wms_outbound_shipments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipmentKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ShipVia = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DestinationName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DestinationAddressSnapshot = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    RoutarrShipmentIntentId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoutarrRouteId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoutarrStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_wms_outbound_shipments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ContractType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    VendorPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RenewalAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PaymentTerms = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FreightTerms = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    WarrantyTerms = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    MinimumSpend = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ServiceLevelAgreement = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ApprovalStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_contracts_supplyarr_external_parties_VendorPartyId",
                        column: x => x.VendorPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_party_compliance_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DocumentTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    ReviewStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EffectiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_party_compliance_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_party_compliance_documents_supplyarr_external_par~",
                        column: x => x.ExternalPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_party_contacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RoleLabel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_party_contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_party_contacts_supplyarr_external_parties_Externa~",
                        column: x => x.ExternalPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_party_supplier_onboarding",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    OnboardingStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_party_supplier_onboarding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_party_supplier_onboarding_supplyarr_external_part~",
                        column: x => x.ExternalPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_purchase_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    VendorPartyId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    IsEmergency = table.Column<bool>(type: "boolean", nullable: false),
                    EmergencyReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    EmergencyExpeditedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EmergencyExpeditedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ManagerOverrideApproved = table.Column<bool>(type: "boolean", nullable: false),
                    ManagerOverrideJustification = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ManagerOverrideApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ManagerOverrideApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_purchase_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_purchase_requests_supplyarr_external_parties_Vend~",
                        column: x => x.VendorPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_rfqs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RfqKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AwardedVendorPartyId = table.Column<Guid>(type: "uuid", nullable: true),
                    SelectedVendorQuoteId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    AwardedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AwardedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_rfqs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_rfqs_supplyarr_external_parties_AwardedVendorPart~",
                        column: x => x.AwardedVendorPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_vendor_restrictions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    RestrictionKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ScopesJson = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EffectiveUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LiftedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LiftedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LiftNotes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_vendor_restrictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_restrictions_supplyarr_external_parties_Ex~",
                        column: x => x.ExternalPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_inventory_bins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    BinKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_inventory_bins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_inventory_bins_supplyarr_inventory_locations_Inve~",
                        column: x => x.InventoryLocationId,
                        principalTable: "supplyarr_inventory_locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_parts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartCatalogId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CategoryKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ManufacturerName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ManufacturerPartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequiresSerialLotTracking = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReorderPoint = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    ReorderQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_parts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_parts_supplyarr_part_catalogs_PartCatalogId",
                        column: x => x.PartCatalogId,
                        principalTable: "supplyarr_part_catalogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_procurement_coordination_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoordinationRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Detail = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    SourceEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceEntityId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_procurement_coordination_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_procurement_coordination_events_supplyarr_procure~",
                        column: x => x.CoordinationRecordId,
                        principalTable: "supplyarr_procurement_coordination_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_maintainarr_demand_refs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaintainarrPublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaintainarrWorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaintainarrWorkOrderNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MaintainarrAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProcurementStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastStatusCallbackAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_maintainarr_demand_refs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_maintainarr_demand_refs_supplyarr_purchase_reques~",
                        column: x => x.PurchaseRequestId,
                        principalTable: "supplyarr_purchase_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_purchase_orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IssuedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IssuedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_purchase_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_purchase_orders_supplyarr_external_parties_Vendor~",
                        column: x => x.VendorPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_purchase_orders_supplyarr_purchase_requests_Purch~",
                        column: x => x.PurchaseRequestId,
                        principalTable: "supplyarr_purchase_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_routarr_demand_refs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoutarrPublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoutarrTripId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoutarrTripNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RoutarrVehicleRefKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProcurementStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastStatusCallbackAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_routarr_demand_refs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_routarr_demand_refs_supplyarr_purchase_requests_P~",
                        column: x => x.PurchaseRequestId,
                        principalTable: "supplyarr_purchase_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_staffarr_demand_refs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrIncidentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffarrIncidentTitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProcurementStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastStatusCallbackAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_staffarr_demand_refs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_staffarr_demand_refs_supplyarr_purchase_requests_~",
                        column: x => x.PurchaseRequestId,
                        principalTable: "supplyarr_purchase_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_trainarr_demand_refs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainarrPublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainarrAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainarrAssignmentRefKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProcurementStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastStatusCallbackAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_trainarr_demand_refs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_trainarr_demand_refs_supplyarr_purchase_requests_~",
                        column: x => x.PurchaseRequestId,
                        principalTable: "supplyarr_purchase_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_rfq_vendor_invitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RfqId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    InvitedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    InvitedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PortalAccessCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PortalAccessCodeIssuedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PortalAccessCodeExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_rfq_vendor_invitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_rfq_vendor_invitations_supplyarr_external_parties~",
                        column: x => x.VendorPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_rfq_vendor_invitations_supplyarr_rfqs_RfqId",
                        column: x => x.RfqId,
                        principalTable: "supplyarr_rfqs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_vendor_quotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RfqId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuoteKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    LeadTimeDays = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_vendor_quotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_quotes_supplyarr_external_parties_VendorPa~",
                        column: x => x.VendorPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_quotes_supplyarr_rfqs_RfqId",
                        column: x => x.RfqId,
                        principalTable: "supplyarr_rfqs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_supplier_incidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    IncidentKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    IncidentType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivingReceiptId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivingExceptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    VendorRestrictionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReportedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    InvolvedStaffarrPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    StaffarrPersonnelIncidentId = table.Column<Guid>(type: "uuid", nullable: true),
                    StaffarrIncidentRoutedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StaffarrIncidentRouteStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: ""),
                    TrainarrIncidentRemediationId = table.Column<Guid>(type: "uuid", nullable: true),
                    TrainarrIncidentRoutedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TrainarrIncidentRouteStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: ""),
                    ResolutionNotes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ReopenedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReopenedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastReopenReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ReopenCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_supplier_incidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_supplier_incidents_supplyarr_external_parties_Ext~",
                        column: x => x.ExternalPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_supplyarr_supplier_incidents_supplyarr_vendor_restrictions_~",
                        column: x => x.VendorRestrictionId,
                        principalTable: "supplyarr_vendor_restrictions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_part_manufacturer_aliases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    AliasKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ManufacturerName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ManufacturerPartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_part_manufacturer_aliases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_manufacturer_aliases_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_part_stock_levels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryBinId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityOnHand = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityReserved = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_part_stock_levels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_stock_levels_supplyarr_inventory_bins_Invent~",
                        column: x => x.InventoryBinId,
                        principalTable: "supplyarr_inventory_bins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_stock_levels_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_part_vendor_links",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorPartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsPreferred = table.Column<bool>(type: "boolean", nullable: false),
                    CatalogUnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    CatalogCurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CatalogMinimumOrderQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    CatalogLeadTimeDays = table.Column<int>(type: "integer", nullable: true),
                    CatalogQuantityAvailable = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    CatalogAvailabilityStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_part_vendor_links", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_vendor_links_supplyarr_external_parties_Exte~",
                        column: x => x.ExternalPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_vendor_links_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_purchase_request_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityRequested = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_purchase_request_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_purchase_request_lines_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_purchase_request_lines_supplyarr_purchase_request~",
                        column: x => x.PurchaseRequestId,
                        principalTable: "supplyarr_purchase_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_rfq_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RfqId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityRequested = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_rfq_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_rfq_lines_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_rfq_lines_supplyarr_rfqs_RfqId",
                        column: x => x.RfqId,
                        principalTable: "supplyarr_rfqs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_wms_outbound_shipment_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OutboundShipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromInventoryBinId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityRequested = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityReserved = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityPicked = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityShipped = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_wms_outbound_shipment_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_wms_outbound_shipment_lines_supplyarr_inventory_b~",
                        column: x => x.FromInventoryBinId,
                        principalTable: "supplyarr_inventory_bins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_wms_outbound_shipment_lines_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_wms_outbound_shipment_lines_supplyarr_wms_outboun~",
                        column: x => x.OutboundShipmentId,
                        principalTable: "supplyarr_wms_outbound_shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_wms_stock_ledger",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MovementGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MovementType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryBinId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelatedInventoryBinId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuantityOnHandDelta = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityReservedDelta = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityOnHandAfter = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityReservedAfter = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_wms_stock_ledger", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_wms_stock_ledger_supplyarr_inventory_bins_Invento~",
                        column: x => x.InventoryBinId,
                        principalTable: "supplyarr_inventory_bins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_wms_stock_ledger_supplyarr_inventory_bins_Related~",
                        column: x => x.RelatedInventoryBinId,
                        principalTable: "supplyarr_inventory_bins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_wms_stock_ledger_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_maintainarr_demand_ref_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DemandRefId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    MaintainarrDemandLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    QuantityRequested = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_maintainarr_demand_ref_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_maintainarr_demand_ref_lines_supplyarr_maintainar~",
                        column: x => x.DemandRefId,
                        principalTable: "supplyarr_maintainarr_demand_refs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_supplyarr_maintainarr_demand_ref_lines_supplyarr_parts_Part~",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_receiving_receipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiptKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryBinId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    PackingSlipReference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PackingSlipFileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    InvoiceReference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    InvoiceFileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PostedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_receiving_receipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_receiving_receipts_supplyarr_inventory_bins_Inven~",
                        column: x => x.InventoryBinId,
                        principalTable: "supplyarr_inventory_bins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_receiving_receipts_supplyarr_purchase_orders_Purc~",
                        column: x => x.PurchaseOrderId,
                        principalTable: "supplyarr_purchase_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_vendor_returns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReturnKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    VendorPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    InventoryBinId = table.Column<Guid>(type: "uuid", nullable: false),
                    RmaNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PostedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_vendor_returns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_returns_supplyarr_external_parties_VendorP~",
                        column: x => x.VendorPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_returns_supplyarr_inventory_bins_Inventory~",
                        column: x => x.InventoryBinId,
                        principalTable: "supplyarr_inventory_bins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_returns_supplyarr_purchase_orders_Purchase~",
                        column: x => x.PurchaseOrderId,
                        principalTable: "supplyarr_purchase_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_routarr_demand_ref_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DemandRefId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    RoutarrDemandLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    QuantityRequested = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_routarr_demand_ref_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_routarr_demand_ref_lines_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_supplyarr_routarr_demand_ref_lines_supplyarr_routarr_demand~",
                        column: x => x.DemandRefId,
                        principalTable: "supplyarr_routarr_demand_refs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_staffarr_demand_ref_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DemandRefId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    StaffarrDemandLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    QuantityRequested = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_staffarr_demand_ref_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_staffarr_demand_ref_lines_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_supplyarr_staffarr_demand_ref_lines_supplyarr_staffarr_dema~",
                        column: x => x.DemandRefId,
                        principalTable: "supplyarr_staffarr_demand_refs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_trainarr_demand_ref_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DemandRefId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    TrainarrDemandLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    QuantityRequested = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_trainarr_demand_ref_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_trainarr_demand_ref_lines_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_supplyarr_trainarr_demand_ref_lines_supplyarr_trainarr_dema~",
                        column: x => x.DemandRefId,
                        principalTable: "supplyarr_trainarr_demand_refs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_part_stock_reservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReservationKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryBinId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartStockLevelId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityReserved = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FulfilledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    FulfilledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReleasedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReleasedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReleaseReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_part_stock_reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_stock_reservations_supplyarr_inventory_bins_~",
                        column: x => x.InventoryBinId,
                        principalTable: "supplyarr_inventory_bins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_stock_reservations_supplyarr_part_stock_leve~",
                        column: x => x.PartStockLevelId,
                        principalTable: "supplyarr_part_stock_levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_stock_reservations_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_part_vendor_availability_capture_states",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartVendorLinkId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastCapturedQuantityAvailable = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    LastCapturedAvailabilityStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    LastAvailabilitySnapshotId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastCapturedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_part_vendor_availability_capture_states", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_vendor_availability_capture_states_supplyarr~",
                        column: x => x.PartVendorLinkId,
                        principalTable: "supplyarr_part_vendor_links",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_part_vendor_availability_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartVendorLinkId = table.Column<Guid>(type: "uuid", nullable: false),
                    SnapshotKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    QuantityAvailable = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    AvailabilityStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_part_vendor_availability_snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_vendor_availability_snapshots_supplyarr_part~",
                        column: x => x.PartVendorLinkId,
                        principalTable: "supplyarr_part_vendor_links",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_part_vendor_lead_time_capture_states",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartVendorLinkId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastCapturedLeadTimeDays = table.Column<int>(type: "integer", nullable: true),
                    LastLeadTimeSnapshotId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastCapturedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_part_vendor_lead_time_capture_states", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_vendor_lead_time_capture_states_supplyarr_pa~",
                        column: x => x.PartVendorLinkId,
                        principalTable: "supplyarr_part_vendor_links",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_part_vendor_lead_time_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartVendorLinkId = table.Column<Guid>(type: "uuid", nullable: false),
                    SnapshotKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LeadTimeDays = table.Column<int>(type: "integer", nullable: false),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_part_vendor_lead_time_snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_vendor_lead_time_snapshots_supplyarr_part_ve~",
                        column: x => x.PartVendorLinkId,
                        principalTable: "supplyarr_part_vendor_links",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_part_vendor_price_capture_states",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartVendorLinkId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastCapturedUnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    LastCapturedCurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    LastCapturedMinimumOrderQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    LastPricingSnapshotId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastCapturedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_part_vendor_price_capture_states", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_vendor_price_capture_states_supplyarr_part_v~",
                        column: x => x.PartVendorLinkId,
                        principalTable: "supplyarr_part_vendor_links",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_part_vendor_pricing_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartVendorLinkId = table.Column<Guid>(type: "uuid", nullable: false),
                    SnapshotKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    MinimumOrderQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_part_vendor_pricing_snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_part_vendor_pricing_snapshots_supplyarr_part_vend~",
                        column: x => x.PartVendorLinkId,
                        principalTable: "supplyarr_part_vendor_links",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_purchase_order_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseRequestLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityOrdered = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityReceived = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_purchase_order_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_purchase_order_lines_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_purchase_order_lines_supplyarr_purchase_orders_Pu~",
                        column: x => x.PurchaseOrderId,
                        principalTable: "supplyarr_purchase_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_supplyarr_purchase_order_lines_supplyarr_purchase_request_l~",
                        column: x => x.PurchaseRequestLineId,
                        principalTable: "supplyarr_purchase_request_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_vendor_quote_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorQuoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    RfqLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    QuantityQuoted = table.Column<decimal>(type: "numeric", nullable: false),
                    LeadTimeDays = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_vendor_quote_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_quote_lines_supplyarr_rfq_lines_RfqLineId",
                        column: x => x.RfqLineId,
                        principalTable: "supplyarr_rfq_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_quote_lines_supplyarr_vendor_quotes_Vendor~",
                        column: x => x.VendorQuoteId,
                        principalTable: "supplyarr_vendor_quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_backorders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BackorderKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseOrderLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseRequestLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivingReceiptId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivingReceiptLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityBackordered = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityFulfilled = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ExpectedBy = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FulfilledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    FulfilledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_backorders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_backorders_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_backorders_supplyarr_purchase_order_lines_Purchas~",
                        column: x => x.PurchaseOrderLineId,
                        principalTable: "supplyarr_purchase_order_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_backorders_supplyarr_purchase_orders_PurchaseOrde~",
                        column: x => x.PurchaseOrderId,
                        principalTable: "supplyarr_purchase_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_receiving_receipt_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivingReceiptId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseOrderLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    QuantityExpected = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityReceived = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Condition = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SerialLotNumbersJson = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_receiving_receipt_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_receiving_receipt_lines_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_receiving_receipt_lines_supplyarr_purchase_order_~",
                        column: x => x.PurchaseOrderLineId,
                        principalTable: "supplyarr_purchase_order_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_receiving_receipt_lines_supplyarr_receiving_recei~",
                        column: x => x.ReceivingReceiptId,
                        principalTable: "supplyarr_receiving_receipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_vendor_return_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorReturnId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseOrderLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_vendor_return_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_return_lines_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_return_lines_supplyarr_purchase_order_line~",
                        column: x => x.PurchaseOrderLineId,
                        principalTable: "supplyarr_purchase_order_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_supplyarr_vendor_return_lines_supplyarr_vendor_returns_Vend~",
                        column: x => x.VendorReturnId,
                        principalTable: "supplyarr_vendor_returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_receiving_exceptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivingReceiptId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivingReceiptLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExceptionType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ReopenedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReopenedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastReopenReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ReopenCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_receiving_exceptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_receiving_exceptions_supplyarr_receiving_receipt_~",
                        column: x => x.ReceivingReceiptLineId,
                        principalTable: "supplyarr_receiving_receipt_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_supplyarr_receiving_exceptions_supplyarr_receiving_receipts~",
                        column: x => x.ReceivingReceiptId,
                        principalTable: "supplyarr_receiving_receipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplyarr_warranty_claims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ClaimType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    VendorPartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseOrderLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivingReceiptId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivingReceiptLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuantityClaimed = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ProblemDescription = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    VendorRmaNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    VendorDisposition = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    VendorResponseNotes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ClosureNotes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    DenialReason = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VendorRespondedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VendorRespondedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeniedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeniedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplyarr_warranty_claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplyarr_warranty_claims_supplyarr_external_parties_Vendor~",
                        column: x => x.VendorPartyId,
                        principalTable: "supplyarr_external_parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_warranty_claims_supplyarr_parts_PartId",
                        column: x => x.PartId,
                        principalTable: "supplyarr_parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplyarr_warranty_claims_supplyarr_purchase_order_lines_Pu~",
                        column: x => x.PurchaseOrderLineId,
                        principalTable: "supplyarr_purchase_order_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_supplyarr_warranty_claims_supplyarr_purchase_orders_Purchas~",
                        column: x => x.PurchaseOrderId,
                        principalTable: "supplyarr_purchase_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_supplyarr_warranty_claims_supplyarr_receiving_receipt_lines~",
                        column: x => x.ReceivingReceiptLineId,
                        principalTable: "supplyarr_receiving_receipt_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_supplyarr_warranty_claims_supplyarr_receiving_receipts_Rece~",
                        column: x => x.ReceivingReceiptId,
                        principalTable: "supplyarr_receiving_receipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                name: "IX_supplyarr_approval_reminder_runs_TenantId",
                table: "supplyarr_approval_reminder_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_approval_reminder_runs_TenantId_CreatedAt",
                table: "supplyarr_approval_reminder_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_approval_reminder_states_TenantId",
                table: "supplyarr_approval_reminder_states",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_approval_reminder_states_TenantId_LastReminderSen~",
                table: "supplyarr_approval_reminder_states",
                columns: new[] { "TenantId", "LastReminderSentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_approval_reminder_states_TenantId_SubjectType_Sub~",
                table: "supplyarr_approval_reminder_states",
                columns: new[] { "TenantId", "SubjectType", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_audit_events_TenantId",
                table: "supplyarr_audit_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_audit_events_TenantId_OccurredAt",
                table: "supplyarr_audit_events",
                columns: new[] { "TenantId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_availability_snapshot_runs_TenantId",
                table: "supplyarr_availability_snapshot_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_availability_snapshot_runs_TenantId_CreatedAt",
                table: "supplyarr_availability_snapshot_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_backorders_PartId",
                table: "supplyarr_backorders",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_backorders_PurchaseOrderId",
                table: "supplyarr_backorders",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_backorders_PurchaseOrderLineId",
                table: "supplyarr_backorders",
                column: "PurchaseOrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_backorders_TenantId",
                table: "supplyarr_backorders",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_backorders_TenantId_BackorderKey",
                table: "supplyarr_backorders",
                columns: new[] { "TenantId", "BackorderKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_backorders_TenantId_PartId_Status",
                table: "supplyarr_backorders",
                columns: new[] { "TenantId", "PartId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_backorders_TenantId_PurchaseOrderId",
                table: "supplyarr_backorders",
                columns: new[] { "TenantId", "PurchaseOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_backorders_TenantId_PurchaseOrderLineId_Status",
                table: "supplyarr_backorders",
                columns: new[] { "TenantId", "PurchaseOrderLineId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_contracts_TenantId",
                table: "supplyarr_contracts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_contracts_TenantId_ContractKey",
                table: "supplyarr_contracts",
                columns: new[] { "TenantId", "ContractKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_contracts_TenantId_Status_ExpiresAt",
                table: "supplyarr_contracts",
                columns: new[] { "TenantId", "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_contracts_TenantId_VendorPartyId",
                table: "supplyarr_contracts",
                columns: new[] { "TenantId", "VendorPartyId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_contracts_VendorPartyId",
                table: "supplyarr_contracts",
                column: "VendorPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_demand_processing_runs_TenantId",
                table: "supplyarr_demand_processing_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_demand_processing_runs_TenantId_CreatedAt",
                table: "supplyarr_demand_processing_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_demand_processing_states_TenantId",
                table: "supplyarr_demand_processing_states",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_demand_processing_states_TenantId_DemandRefId",
                table: "supplyarr_demand_processing_states",
                columns: new[] { "TenantId", "DemandRefId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_demand_processing_states_TenantId_DemandRefSource",
                table: "supplyarr_demand_processing_states",
                columns: new[] { "TenantId", "DemandRefSource" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_demand_processing_states_TenantId_LastProcessedAt",
                table: "supplyarr_demand_processing_states",
                columns: new[] { "TenantId", "LastProcessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_external_parties_TenantId",
                table: "supplyarr_external_parties",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_external_parties_TenantId_ApprovalStatus",
                table: "supplyarr_external_parties",
                columns: new[] { "TenantId", "ApprovalStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_external_parties_TenantId_PartyKey",
                table: "supplyarr_external_parties",
                columns: new[] { "TenantId", "PartyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_external_parties_TenantId_PartyType_Status",
                table: "supplyarr_external_parties",
                columns: new[] { "TenantId", "PartyType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_integration_event_processing_runs_TenantId",
                table: "supplyarr_integration_event_processing_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_integration_event_processing_runs_TenantId_Create~",
                table: "supplyarr_integration_event_processing_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_integration_inbox_events_TenantId",
                table: "supplyarr_integration_inbox_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_integration_inbox_events_TenantId_IdempotencyKey",
                table: "supplyarr_integration_inbox_events",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_integration_inbox_events_TenantId_ProcessingStatu~",
                table: "supplyarr_integration_inbox_events",
                columns: new[] { "TenantId", "ProcessingStatus", "NextRetryAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_integration_outbox_events_TenantId",
                table: "supplyarr_integration_outbox_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_integration_outbox_events_TenantId_IdempotencyKey",
                table: "supplyarr_integration_outbox_events",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_integration_outbox_events_TenantId_ProcessingStat~",
                table: "supplyarr_integration_outbox_events",
                columns: new[] { "TenantId", "ProcessingStatus", "NextRetryAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_inventory_bins_InventoryLocationId",
                table: "supplyarr_inventory_bins",
                column: "InventoryLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_inventory_bins_TenantId",
                table: "supplyarr_inventory_bins",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_inventory_bins_TenantId_InventoryLocationId",
                table: "supplyarr_inventory_bins",
                columns: new[] { "TenantId", "InventoryLocationId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_inventory_bins_TenantId_InventoryLocationId_BinKey",
                table: "supplyarr_inventory_bins",
                columns: new[] { "TenantId", "InventoryLocationId", "BinKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_inventory_locations_TenantId",
                table: "supplyarr_inventory_locations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_inventory_locations_TenantId_LocationKey",
                table: "supplyarr_inventory_locations",
                columns: new[] { "TenantId", "LocationKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_inventory_locations_TenantId_LocationType_Status",
                table: "supplyarr_inventory_locations",
                columns: new[] { "TenantId", "LocationType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_inventory_locations_TenantId_StaffarrSiteOrgUnitId",
                table: "supplyarr_inventory_locations",
                columns: new[] { "TenantId", "StaffarrSiteOrgUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_lead_time_snapshot_runs_TenantId",
                table: "supplyarr_lead_time_snapshot_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_lead_time_snapshot_runs_TenantId_CreatedAt",
                table: "supplyarr_lead_time_snapshot_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_maintainarr_demand_ref_lines_DemandRefId",
                table: "supplyarr_maintainarr_demand_ref_lines",
                column: "DemandRefId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_maintainarr_demand_ref_lines_PartId",
                table: "supplyarr_maintainarr_demand_ref_lines",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_maintainarr_demand_ref_lines_TenantId",
                table: "supplyarr_maintainarr_demand_ref_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_maintainarr_demand_ref_lines_TenantId_DemandRefId~",
                table: "supplyarr_maintainarr_demand_ref_lines",
                columns: new[] { "TenantId", "DemandRefId", "LineNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_maintainarr_demand_ref_lines_TenantId_Maintainarr~",
                table: "supplyarr_maintainarr_demand_ref_lines",
                columns: new[] { "TenantId", "MaintainarrDemandLineId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_maintainarr_demand_refs_PurchaseRequestId",
                table: "supplyarr_maintainarr_demand_refs",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_maintainarr_demand_refs_TenantId",
                table: "supplyarr_maintainarr_demand_refs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_maintainarr_demand_refs_TenantId_MaintainarrPubli~",
                table: "supplyarr_maintainarr_demand_refs",
                columns: new[] { "TenantId", "MaintainarrPublicationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_maintainarr_demand_refs_TenantId_MaintainarrWorkO~",
                table: "supplyarr_maintainarr_demand_refs",
                columns: new[] { "TenantId", "MaintainarrWorkOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_maintainarr_demand_refs_TenantId_PurchaseOrderId",
                table: "supplyarr_maintainarr_demand_refs",
                columns: new[] { "TenantId", "PurchaseOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_maintainarr_demand_refs_TenantId_PurchaseRequestId",
                table: "supplyarr_maintainarr_demand_refs",
                columns: new[] { "TenantId", "PurchaseRequestId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_maintainarr_demand_refs_TenantId_Status_ReceivedAt",
                table: "supplyarr_maintainarr_demand_refs",
                columns: new[] { "TenantId", "Status", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_notification_dispatches_TenantId",
                table: "supplyarr_notification_dispatches",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_notification_dispatches_TenantId_DispatchStatus_C~",
                table: "supplyarr_notification_dispatches",
                columns: new[] { "TenantId", "DispatchStatus", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_notification_dispatches_TenantId_EventKind_Relate~",
                table: "supplyarr_notification_dispatches",
                columns: new[] { "TenantId", "EventKind", "RelatedEntityType", "RelatedEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_catalogs_TenantId",
                table: "supplyarr_part_catalogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_catalogs_TenantId_CatalogKey",
                table: "supplyarr_part_catalogs",
                columns: new[] { "TenantId", "CatalogKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_catalogs_TenantId_Status",
                table: "supplyarr_part_catalogs",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_manufacturer_aliases_PartId",
                table: "supplyarr_part_manufacturer_aliases",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_manufacturer_aliases_TenantId",
                table: "supplyarr_part_manufacturer_aliases",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_manufacturer_aliases_TenantId_PartId",
                table: "supplyarr_part_manufacturer_aliases",
                columns: new[] { "TenantId", "PartId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_manufacturer_aliases_TenantId_PartId_AliasKey",
                table: "supplyarr_part_manufacturer_aliases",
                columns: new[] { "TenantId", "PartId", "AliasKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_levels_InventoryBinId",
                table: "supplyarr_part_stock_levels",
                column: "InventoryBinId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_levels_PartId",
                table: "supplyarr_part_stock_levels",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_levels_TenantId",
                table: "supplyarr_part_stock_levels",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_levels_TenantId_InventoryBinId",
                table: "supplyarr_part_stock_levels",
                columns: new[] { "TenantId", "InventoryBinId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_levels_TenantId_PartId",
                table: "supplyarr_part_stock_levels",
                columns: new[] { "TenantId", "PartId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_levels_TenantId_PartId_InventoryBinId",
                table: "supplyarr_part_stock_levels",
                columns: new[] { "TenantId", "PartId", "InventoryBinId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_reservations_InventoryBinId",
                table: "supplyarr_part_stock_reservations",
                column: "InventoryBinId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_reservations_PartId",
                table: "supplyarr_part_stock_reservations",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_reservations_PartStockLevelId",
                table: "supplyarr_part_stock_reservations",
                column: "PartStockLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_reservations_TenantId",
                table: "supplyarr_part_stock_reservations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_reservations_TenantId_InventoryBinId_S~",
                table: "supplyarr_part_stock_reservations",
                columns: new[] { "TenantId", "InventoryBinId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_reservations_TenantId_PartId_Status",
                table: "supplyarr_part_stock_reservations",
                columns: new[] { "TenantId", "PartId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_stock_reservations_TenantId_ReservationKey",
                table: "supplyarr_part_stock_reservations",
                columns: new[] { "TenantId", "ReservationKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_availability_capture_states_PartVendo~",
                table: "supplyarr_part_vendor_availability_capture_states",
                column: "PartVendorLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_availability_capture_states_TenantId",
                table: "supplyarr_part_vendor_availability_capture_states",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_availability_capture_states_TenantId_~",
                table: "supplyarr_part_vendor_availability_capture_states",
                columns: new[] { "TenantId", "PartVendorLinkId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_availability_snapshots_PartVendorLink~",
                table: "supplyarr_part_vendor_availability_snapshots",
                column: "PartVendorLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_availability_snapshots_TenantId",
                table: "supplyarr_part_vendor_availability_snapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_availability_snapshots_TenantId_Part~1",
                table: "supplyarr_part_vendor_availability_snapshots",
                columns: new[] { "TenantId", "PartVendorLinkId", "EffectiveTo" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_availability_snapshots_TenantId_PartV~",
                table: "supplyarr_part_vendor_availability_snapshots",
                columns: new[] { "TenantId", "PartVendorLinkId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_availability_snapshots_TenantId_Snaps~",
                table: "supplyarr_part_vendor_availability_snapshots",
                columns: new[] { "TenantId", "SnapshotKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_lead_time_capture_states_PartVendorLi~",
                table: "supplyarr_part_vendor_lead_time_capture_states",
                column: "PartVendorLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_lead_time_capture_states_TenantId",
                table: "supplyarr_part_vendor_lead_time_capture_states",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_lead_time_capture_states_TenantId_Par~",
                table: "supplyarr_part_vendor_lead_time_capture_states",
                columns: new[] { "TenantId", "PartVendorLinkId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_lead_time_snapshots_PartVendorLinkId",
                table: "supplyarr_part_vendor_lead_time_snapshots",
                column: "PartVendorLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_lead_time_snapshots_TenantId",
                table: "supplyarr_part_vendor_lead_time_snapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_lead_time_snapshots_TenantId_PartVen~1",
                table: "supplyarr_part_vendor_lead_time_snapshots",
                columns: new[] { "TenantId", "PartVendorLinkId", "EffectiveTo" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_lead_time_snapshots_TenantId_PartVend~",
                table: "supplyarr_part_vendor_lead_time_snapshots",
                columns: new[] { "TenantId", "PartVendorLinkId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_lead_time_snapshots_TenantId_Snapshot~",
                table: "supplyarr_part_vendor_lead_time_snapshots",
                columns: new[] { "TenantId", "SnapshotKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_links_ExternalPartyId",
                table: "supplyarr_part_vendor_links",
                column: "ExternalPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_links_PartId",
                table: "supplyarr_part_vendor_links",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_links_TenantId",
                table: "supplyarr_part_vendor_links",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_links_TenantId_PartId",
                table: "supplyarr_part_vendor_links",
                columns: new[] { "TenantId", "PartId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_links_TenantId_PartId_ExternalPartyId",
                table: "supplyarr_part_vendor_links",
                columns: new[] { "TenantId", "PartId", "ExternalPartyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_price_capture_states_PartVendorLinkId",
                table: "supplyarr_part_vendor_price_capture_states",
                column: "PartVendorLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_price_capture_states_TenantId",
                table: "supplyarr_part_vendor_price_capture_states",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_price_capture_states_TenantId_PartVen~",
                table: "supplyarr_part_vendor_price_capture_states",
                columns: new[] { "TenantId", "PartVendorLinkId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_pricing_snapshots_PartVendorLinkId",
                table: "supplyarr_part_vendor_pricing_snapshots",
                column: "PartVendorLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_pricing_snapshots_TenantId",
                table: "supplyarr_part_vendor_pricing_snapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_pricing_snapshots_TenantId_PartVendo~1",
                table: "supplyarr_part_vendor_pricing_snapshots",
                columns: new[] { "TenantId", "PartVendorLinkId", "EffectiveTo" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_pricing_snapshots_TenantId_PartVendor~",
                table: "supplyarr_part_vendor_pricing_snapshots",
                columns: new[] { "TenantId", "PartVendorLinkId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_part_vendor_pricing_snapshots_TenantId_SnapshotKey",
                table: "supplyarr_part_vendor_pricing_snapshots",
                columns: new[] { "TenantId", "SnapshotKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_parts_PartCatalogId",
                table: "supplyarr_parts",
                column: "PartCatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_parts_TenantId",
                table: "supplyarr_parts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_parts_TenantId_CategoryKey_Status",
                table: "supplyarr_parts",
                columns: new[] { "TenantId", "CategoryKey", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_parts_TenantId_PartCatalogId",
                table: "supplyarr_parts",
                columns: new[] { "TenantId", "PartCatalogId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_parts_TenantId_PartKey",
                table: "supplyarr_parts",
                columns: new[] { "TenantId", "PartKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_compliance_documents_ExternalPartyId",
                table: "supplyarr_party_compliance_documents",
                column: "ExternalPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_compliance_documents_TenantId",
                table: "supplyarr_party_compliance_documents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_compliance_documents_TenantId_ExpiresAt",
                table: "supplyarr_party_compliance_documents",
                columns: new[] { "TenantId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_compliance_documents_TenantId_ExternalPart~1",
                table: "supplyarr_party_compliance_documents",
                columns: new[] { "TenantId", "ExternalPartyId", "DocumentKey", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_compliance_documents_TenantId_ExternalParty~",
                table: "supplyarr_party_compliance_documents",
                columns: new[] { "TenantId", "ExternalPartyId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_compliance_documents_TenantId_ReviewStatus_~",
                table: "supplyarr_party_compliance_documents",
                columns: new[] { "TenantId", "ReviewStatus", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_contacts_ExternalPartyId",
                table: "supplyarr_party_contacts",
                column: "ExternalPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_contacts_TenantId",
                table: "supplyarr_party_contacts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_contacts_TenantId_ExternalPartyId",
                table: "supplyarr_party_contacts",
                columns: new[] { "TenantId", "ExternalPartyId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_supplier_onboarding_ExternalPartyId",
                table: "supplyarr_party_supplier_onboarding",
                column: "ExternalPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_supplier_onboarding_TenantId",
                table: "supplyarr_party_supplier_onboarding",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_supplier_onboarding_TenantId_ExternalPartyId",
                table: "supplyarr_party_supplier_onboarding",
                columns: new[] { "TenantId", "ExternalPartyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_party_supplier_onboarding_TenantId_OnboardingStat~",
                table: "supplyarr_party_supplier_onboarding",
                columns: new[] { "TenantId", "OnboardingStatus", "SubmittedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_price_snapshot_runs_TenantId",
                table: "supplyarr_price_snapshot_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_price_snapshot_runs_TenantId_CreatedAt",
                table: "supplyarr_price_snapshot_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_coordination_events_CoordinationRecor~",
                table: "supplyarr_procurement_coordination_events",
                column: "CoordinationRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_coordination_events_TenantId",
                table: "supplyarr_procurement_coordination_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_coordination_events_TenantId_Coordina~",
                table: "supplyarr_procurement_coordination_events",
                columns: new[] { "TenantId", "CoordinationRecordId", "SequenceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_coordination_records_TenantId",
                table: "supplyarr_procurement_coordination_records",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_coordination_records_TenantId_Coordin~",
                table: "supplyarr_procurement_coordination_records",
                columns: new[] { "TenantId", "CoordinationStage", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_coordination_records_TenantId_IsTermi~",
                table: "supplyarr_procurement_coordination_records",
                columns: new[] { "TenantId", "IsTerminal", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_coordination_records_TenantId_Subject~",
                table: "supplyarr_procurement_coordination_records",
                columns: new[] { "TenantId", "SubjectType", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_coordination_runs_TenantId",
                table: "supplyarr_procurement_coordination_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_coordination_runs_TenantId_CreatedAt",
                table: "supplyarr_procurement_coordination_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_exception_escalation_events_TenantId",
                table: "supplyarr_procurement_exception_escalation_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_exception_escalation_events_TenantId_~",
                table: "supplyarr_procurement_exception_escalation_events",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_exception_escalation_events_TenantId~1",
                table: "supplyarr_procurement_exception_escalation_events",
                columns: new[] { "TenantId", "ProcurementExceptionId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_exception_escalation_runs_TenantId",
                table: "supplyarr_procurement_exception_escalation_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_exception_escalation_runs_TenantId_Cr~",
                table: "supplyarr_procurement_exception_escalation_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_exceptions_TenantId",
                table: "supplyarr_procurement_exceptions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_exceptions_TenantId_ExceptionKey",
                table: "supplyarr_procurement_exceptions",
                columns: new[] { "TenantId", "ExceptionKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_exceptions_TenantId_LastEscalatedAt",
                table: "supplyarr_procurement_exceptions",
                columns: new[] { "TenantId", "LastEscalatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_exceptions_TenantId_SlaDueAt",
                table: "supplyarr_procurement_exceptions",
                columns: new[] { "TenantId", "SlaDueAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_exceptions_TenantId_Status_UpdatedAt",
                table: "supplyarr_procurement_exceptions",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_procurement_exceptions_TenantId_SubjectType_Subje~",
                table: "supplyarr_procurement_exceptions",
                columns: new[] { "TenantId", "SubjectType", "SubjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_order_lines_PartId",
                table: "supplyarr_purchase_order_lines",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_order_lines_PurchaseOrderId",
                table: "supplyarr_purchase_order_lines",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_order_lines_PurchaseRequestLineId",
                table: "supplyarr_purchase_order_lines",
                column: "PurchaseRequestLineId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_order_lines_TenantId",
                table: "supplyarr_purchase_order_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_order_lines_TenantId_PurchaseOrderId",
                table: "supplyarr_purchase_order_lines",
                columns: new[] { "TenantId", "PurchaseOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_order_lines_TenantId_PurchaseOrderId_Lin~",
                table: "supplyarr_purchase_order_lines",
                columns: new[] { "TenantId", "PurchaseOrderId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_orders_PurchaseRequestId",
                table: "supplyarr_purchase_orders",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_orders_TenantId",
                table: "supplyarr_purchase_orders",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_orders_TenantId_OrderKey",
                table: "supplyarr_purchase_orders",
                columns: new[] { "TenantId", "OrderKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_orders_TenantId_PurchaseRequestId",
                table: "supplyarr_purchase_orders",
                columns: new[] { "TenantId", "PurchaseRequestId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_orders_TenantId_Status_UpdatedAt",
                table: "supplyarr_purchase_orders",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_orders_VendorPartyId",
                table: "supplyarr_purchase_orders",
                column: "VendorPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_request_lines_PartId",
                table: "supplyarr_purchase_request_lines",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_request_lines_PurchaseRequestId",
                table: "supplyarr_purchase_request_lines",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_request_lines_TenantId",
                table: "supplyarr_purchase_request_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_request_lines_TenantId_PurchaseRequestId",
                table: "supplyarr_purchase_request_lines",
                columns: new[] { "TenantId", "PurchaseRequestId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_request_lines_TenantId_PurchaseRequestId~",
                table: "supplyarr_purchase_request_lines",
                columns: new[] { "TenantId", "PurchaseRequestId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_requests_TenantId",
                table: "supplyarr_purchase_requests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_requests_TenantId_IsEmergency_Status",
                table: "supplyarr_purchase_requests",
                columns: new[] { "TenantId", "IsEmergency", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_requests_TenantId_RequestKey",
                table: "supplyarr_purchase_requests",
                columns: new[] { "TenantId", "RequestKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_requests_TenantId_Status_UpdatedAt",
                table: "supplyarr_purchase_requests",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_purchase_requests_VendorPartyId",
                table: "supplyarr_purchase_requests",
                column: "VendorPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_exceptions_ReceivingReceiptId",
                table: "supplyarr_receiving_exceptions",
                column: "ReceivingReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_exceptions_ReceivingReceiptLineId",
                table: "supplyarr_receiving_exceptions",
                column: "ReceivingReceiptLineId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_exceptions_TenantId",
                table: "supplyarr_receiving_exceptions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_exceptions_TenantId_ReceivingReceiptId",
                table: "supplyarr_receiving_exceptions",
                columns: new[] { "TenantId", "ReceivingReceiptId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_exceptions_TenantId_ReceivingReceiptLi~1",
                table: "supplyarr_receiving_exceptions",
                columns: new[] { "TenantId", "ReceivingReceiptLineId", "ExceptionType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_exceptions_TenantId_ReceivingReceiptLin~",
                table: "supplyarr_receiving_exceptions",
                columns: new[] { "TenantId", "ReceivingReceiptLineId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_receipt_lines_PartId",
                table: "supplyarr_receiving_receipt_lines",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_receipt_lines_PurchaseOrderLineId",
                table: "supplyarr_receiving_receipt_lines",
                column: "PurchaseOrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_receipt_lines_ReceivingReceiptId",
                table: "supplyarr_receiving_receipt_lines",
                column: "ReceivingReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_receipt_lines_TenantId",
                table: "supplyarr_receiving_receipt_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_receipt_lines_TenantId_ReceivingReceip~1",
                table: "supplyarr_receiving_receipt_lines",
                columns: new[] { "TenantId", "ReceivingReceiptId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_receipt_lines_TenantId_ReceivingReceipt~",
                table: "supplyarr_receiving_receipt_lines",
                columns: new[] { "TenantId", "ReceivingReceiptId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_receipts_InventoryBinId",
                table: "supplyarr_receiving_receipts",
                column: "InventoryBinId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_receipts_PurchaseOrderId",
                table: "supplyarr_receiving_receipts",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_receipts_TenantId",
                table: "supplyarr_receiving_receipts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_receipts_TenantId_PurchaseOrderId",
                table: "supplyarr_receiving_receipts",
                columns: new[] { "TenantId", "PurchaseOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_receipts_TenantId_ReceiptKey",
                table: "supplyarr_receiving_receipts",
                columns: new[] { "TenantId", "ReceiptKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_receiving_receipts_TenantId_Status_UpdatedAt",
                table: "supplyarr_receiving_receipts",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfq_lines_PartId",
                table: "supplyarr_rfq_lines",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfq_lines_RfqId",
                table: "supplyarr_rfq_lines",
                column: "RfqId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfq_lines_TenantId",
                table: "supplyarr_rfq_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfq_lines_TenantId_RfqId_LineNumber",
                table: "supplyarr_rfq_lines",
                columns: new[] { "TenantId", "RfqId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfq_vendor_invitations_RfqId",
                table: "supplyarr_rfq_vendor_invitations",
                column: "RfqId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfq_vendor_invitations_TenantId",
                table: "supplyarr_rfq_vendor_invitations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfq_vendor_invitations_TenantId_RfqId_PortalAcces~",
                table: "supplyarr_rfq_vendor_invitations",
                columns: new[] { "TenantId", "RfqId", "PortalAccessCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfq_vendor_invitations_TenantId_RfqId_VendorParty~",
                table: "supplyarr_rfq_vendor_invitations",
                columns: new[] { "TenantId", "RfqId", "VendorPartyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfq_vendor_invitations_VendorPartyId",
                table: "supplyarr_rfq_vendor_invitations",
                column: "VendorPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfqs_AwardedVendorPartyId",
                table: "supplyarr_rfqs",
                column: "AwardedVendorPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfqs_TenantId",
                table: "supplyarr_rfqs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfqs_TenantId_RfqKey",
                table: "supplyarr_rfqs",
                columns: new[] { "TenantId", "RfqKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_rfqs_TenantId_Status_UpdatedAt",
                table: "supplyarr_rfqs",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_routarr_demand_ref_lines_DemandRefId",
                table: "supplyarr_routarr_demand_ref_lines",
                column: "DemandRefId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_routarr_demand_ref_lines_PartId",
                table: "supplyarr_routarr_demand_ref_lines",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_routarr_demand_ref_lines_TenantId",
                table: "supplyarr_routarr_demand_ref_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_routarr_demand_ref_lines_TenantId_DemandRefId_Lin~",
                table: "supplyarr_routarr_demand_ref_lines",
                columns: new[] { "TenantId", "DemandRefId", "LineNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_routarr_demand_ref_lines_TenantId_RoutarrDemandLi~",
                table: "supplyarr_routarr_demand_ref_lines",
                columns: new[] { "TenantId", "RoutarrDemandLineId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_routarr_demand_refs_PurchaseRequestId",
                table: "supplyarr_routarr_demand_refs",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_routarr_demand_refs_TenantId",
                table: "supplyarr_routarr_demand_refs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_routarr_demand_refs_TenantId_PurchaseRequestId",
                table: "supplyarr_routarr_demand_refs",
                columns: new[] { "TenantId", "PurchaseRequestId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_routarr_demand_refs_TenantId_RoutarrPublicationId",
                table: "supplyarr_routarr_demand_refs",
                columns: new[] { "TenantId", "RoutarrPublicationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_routarr_demand_refs_TenantId_RoutarrTripId",
                table: "supplyarr_routarr_demand_refs",
                columns: new[] { "TenantId", "RoutarrTripId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_routarr_demand_refs_TenantId_Status_ReceivedAt",
                table: "supplyarr_routarr_demand_refs",
                columns: new[] { "TenantId", "Status", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_demand_ref_lines_DemandRefId",
                table: "supplyarr_staffarr_demand_ref_lines",
                column: "DemandRefId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_demand_ref_lines_PartId",
                table: "supplyarr_staffarr_demand_ref_lines",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_demand_ref_lines_TenantId",
                table: "supplyarr_staffarr_demand_ref_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_demand_ref_lines_TenantId_DemandRefId_Li~",
                table: "supplyarr_staffarr_demand_ref_lines",
                columns: new[] { "TenantId", "DemandRefId", "LineNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_demand_ref_lines_TenantId_StaffarrDemand~",
                table: "supplyarr_staffarr_demand_ref_lines",
                columns: new[] { "TenantId", "StaffarrDemandLineId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_demand_refs_PurchaseRequestId",
                table: "supplyarr_staffarr_demand_refs",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_demand_refs_TenantId",
                table: "supplyarr_staffarr_demand_refs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_demand_refs_TenantId_PurchaseRequestId",
                table: "supplyarr_staffarr_demand_refs",
                columns: new[] { "TenantId", "PurchaseRequestId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_demand_refs_TenantId_StaffarrIncidentId",
                table: "supplyarr_staffarr_demand_refs",
                columns: new[] { "TenantId", "StaffarrIncidentId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_demand_refs_TenantId_StaffarrPublication~",
                table: "supplyarr_staffarr_demand_refs",
                columns: new[] { "TenantId", "StaffarrPublicationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_demand_refs_TenantId_Status_ReceivedAt",
                table: "supplyarr_staffarr_demand_refs",
                columns: new[] { "TenantId", "Status", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_procurement_approval_authority_mirrors_~1",
                table: "supplyarr_staffarr_procurement_approval_authority_mirrors",
                columns: new[] { "TenantId", "ExternalUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_procurement_approval_authority_mirrors_~2",
                table: "supplyarr_staffarr_procurement_approval_authority_mirrors",
                columns: new[] { "TenantId", "RefreshedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_procurement_approval_authority_mirrors_~3",
                table: "supplyarr_staffarr_procurement_approval_authority_mirrors",
                columns: new[] { "TenantId", "StaffarrPersonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_staffarr_procurement_approval_authority_mirrors_T~",
                table: "supplyarr_staffarr_procurement_approval_authority_mirrors",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_supplier_incidents_ExternalPartyId",
                table: "supplyarr_supplier_incidents",
                column: "ExternalPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_supplier_incidents_staffarr_incident",
                table: "supplyarr_supplier_incidents",
                columns: new[] { "TenantId", "StaffarrPersonnelIncidentId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_supplier_incidents_staffarr_person",
                table: "supplyarr_supplier_incidents",
                columns: new[] { "TenantId", "InvolvedStaffarrPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_supplier_incidents_TenantId",
                table: "supplyarr_supplier_incidents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_supplier_incidents_TenantId_ExternalPartyId",
                table: "supplyarr_supplier_incidents",
                columns: new[] { "TenantId", "ExternalPartyId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_supplier_incidents_TenantId_IncidentKey",
                table: "supplyarr_supplier_incidents",
                columns: new[] { "TenantId", "IncidentKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_supplier_incidents_TenantId_Severity",
                table: "supplyarr_supplier_incidents",
                columns: new[] { "TenantId", "Severity" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_supplier_incidents_TenantId_Status_UpdatedAt",
                table: "supplyarr_supplier_incidents",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_supplier_incidents_trainarr_remediation",
                table: "supplyarr_supplier_incidents",
                columns: new[] { "TenantId", "TrainarrIncidentRemediationId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_supplier_incidents_VendorRestrictionId",
                table: "supplyarr_supplier_incidents",
                column: "VendorRestrictionId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_tenant_approval_reminder_settings_TenantId",
                table: "supplyarr_tenant_approval_reminder_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_tenant_availability_snapshot_settings_TenantId",
                table: "supplyarr_tenant_availability_snapshot_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_tenant_demand_processing_settings_TenantId",
                table: "supplyarr_tenant_demand_processing_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_tenant_integration_event_settings_TenantId",
                table: "supplyarr_tenant_integration_event_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_tenant_lead_time_snapshot_settings_TenantId",
                table: "supplyarr_tenant_lead_time_snapshot_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_tenant_notification_settings_TenantId",
                table: "supplyarr_tenant_notification_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_tenant_price_snapshot_settings_TenantId",
                table: "supplyarr_tenant_price_snapshot_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_tenant_procurement_coordination_settings_TenantId",
                table: "supplyarr_tenant_procurement_coordination_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_tenant_procurement_exception_escalation_settings_~",
                table: "supplyarr_tenant_procurement_exception_escalation_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_tenant_supplier_onboarding_settings_TenantId",
                table: "supplyarr_tenant_supplier_onboarding_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_trainarr_demand_ref_lines_DemandRefId",
                table: "supplyarr_trainarr_demand_ref_lines",
                column: "DemandRefId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_trainarr_demand_ref_lines_PartId",
                table: "supplyarr_trainarr_demand_ref_lines",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_trainarr_demand_ref_lines_TenantId",
                table: "supplyarr_trainarr_demand_ref_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_trainarr_demand_ref_lines_TenantId_DemandRefId_Li~",
                table: "supplyarr_trainarr_demand_ref_lines",
                columns: new[] { "TenantId", "DemandRefId", "LineNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_trainarr_demand_ref_lines_TenantId_TrainarrDemand~",
                table: "supplyarr_trainarr_demand_ref_lines",
                columns: new[] { "TenantId", "TrainarrDemandLineId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_trainarr_demand_refs_PurchaseRequestId",
                table: "supplyarr_trainarr_demand_refs",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_trainarr_demand_refs_TenantId",
                table: "supplyarr_trainarr_demand_refs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_trainarr_demand_refs_TenantId_PurchaseRequestId",
                table: "supplyarr_trainarr_demand_refs",
                columns: new[] { "TenantId", "PurchaseRequestId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_trainarr_demand_refs_TenantId_Status_ReceivedAt",
                table: "supplyarr_trainarr_demand_refs",
                columns: new[] { "TenantId", "Status", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_trainarr_demand_refs_TenantId_TrainarrAssignmentId",
                table: "supplyarr_trainarr_demand_refs",
                columns: new[] { "TenantId", "TrainarrAssignmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_trainarr_demand_refs_TenantId_TrainarrPublication~",
                table: "supplyarr_trainarr_demand_refs",
                columns: new[] { "TenantId", "TrainarrPublicationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_email_inbox_messages_TenantId",
                table: "supplyarr_vendor_email_inbox_messages",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_email_inbox_messages_TenantId_LinkedRefere~",
                table: "supplyarr_vendor_email_inbox_messages",
                columns: new[] { "TenantId", "LinkedReferenceType", "LinkedReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_email_inbox_messages_TenantId_MatchStatus_~",
                table: "supplyarr_vendor_email_inbox_messages",
                columns: new[] { "TenantId", "MatchStatus", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_email_inbox_messages_TenantId_MessageKey",
                table: "supplyarr_vendor_email_inbox_messages",
                columns: new[] { "TenantId", "MessageKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_email_inbox_messages_TenantId_MessageKind_~",
                table: "supplyarr_vendor_email_inbox_messages",
                columns: new[] { "TenantId", "MessageKind", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_email_inbox_messages_TenantId_VendorPartyI~",
                table: "supplyarr_vendor_email_inbox_messages",
                columns: new[] { "TenantId", "VendorPartyId", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_quote_lines_RfqLineId",
                table: "supplyarr_vendor_quote_lines",
                column: "RfqLineId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_quote_lines_TenantId",
                table: "supplyarr_vendor_quote_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_quote_lines_TenantId_VendorQuoteId_RfqLine~",
                table: "supplyarr_vendor_quote_lines",
                columns: new[] { "TenantId", "VendorQuoteId", "RfqLineId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_quote_lines_VendorQuoteId",
                table: "supplyarr_vendor_quote_lines",
                column: "VendorQuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_quotes_RfqId",
                table: "supplyarr_vendor_quotes",
                column: "RfqId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_quotes_TenantId",
                table: "supplyarr_vendor_quotes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_quotes_TenantId_RfqId_QuoteKey",
                table: "supplyarr_vendor_quotes",
                columns: new[] { "TenantId", "RfqId", "QuoteKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_quotes_TenantId_RfqId_VendorPartyId",
                table: "supplyarr_vendor_quotes",
                columns: new[] { "TenantId", "RfqId", "VendorPartyId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_quotes_VendorPartyId",
                table: "supplyarr_vendor_quotes",
                column: "VendorPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_restrictions_ExternalPartyId",
                table: "supplyarr_vendor_restrictions",
                column: "ExternalPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_restrictions_TenantId",
                table: "supplyarr_vendor_restrictions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_restrictions_TenantId_ExternalPartyId",
                table: "supplyarr_vendor_restrictions",
                columns: new[] { "TenantId", "ExternalPartyId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_restrictions_TenantId_ExternalPartyId_Rest~",
                table: "supplyarr_vendor_restrictions",
                columns: new[] { "TenantId", "ExternalPartyId", "RestrictionKey", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_restrictions_TenantId_Status_UpdatedAt",
                table: "supplyarr_vendor_restrictions",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_return_lines_PartId",
                table: "supplyarr_vendor_return_lines",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_return_lines_PurchaseOrderLineId",
                table: "supplyarr_vendor_return_lines",
                column: "PurchaseOrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_return_lines_TenantId",
                table: "supplyarr_vendor_return_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_return_lines_TenantId_PartId",
                table: "supplyarr_vendor_return_lines",
                columns: new[] { "TenantId", "PartId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_return_lines_TenantId_VendorReturnId",
                table: "supplyarr_vendor_return_lines",
                columns: new[] { "TenantId", "VendorReturnId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_return_lines_TenantId_VendorReturnId_LineN~",
                table: "supplyarr_vendor_return_lines",
                columns: new[] { "TenantId", "VendorReturnId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_return_lines_VendorReturnId",
                table: "supplyarr_vendor_return_lines",
                column: "VendorReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_returns_InventoryBinId",
                table: "supplyarr_vendor_returns",
                column: "InventoryBinId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_returns_PurchaseOrderId",
                table: "supplyarr_vendor_returns",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_returns_TenantId",
                table: "supplyarr_vendor_returns",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_returns_TenantId_PurchaseOrderId",
                table: "supplyarr_vendor_returns",
                columns: new[] { "TenantId", "PurchaseOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_returns_TenantId_ReturnKey",
                table: "supplyarr_vendor_returns",
                columns: new[] { "TenantId", "ReturnKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_returns_TenantId_Status_UpdatedAt",
                table: "supplyarr_vendor_returns",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_returns_TenantId_VendorPartyId",
                table: "supplyarr_vendor_returns",
                columns: new[] { "TenantId", "VendorPartyId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_vendor_returns_VendorPartyId",
                table: "supplyarr_vendor_returns",
                column: "VendorPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_warranty_claims_PartId",
                table: "supplyarr_warranty_claims",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_warranty_claims_PurchaseOrderId",
                table: "supplyarr_warranty_claims",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_warranty_claims_PurchaseOrderLineId",
                table: "supplyarr_warranty_claims",
                column: "PurchaseOrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_warranty_claims_ReceivingReceiptId",
                table: "supplyarr_warranty_claims",
                column: "ReceivingReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_warranty_claims_ReceivingReceiptLineId",
                table: "supplyarr_warranty_claims",
                column: "ReceivingReceiptLineId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_warranty_claims_TenantId",
                table: "supplyarr_warranty_claims",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_warranty_claims_TenantId_ClaimKey",
                table: "supplyarr_warranty_claims",
                columns: new[] { "TenantId", "ClaimKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_warranty_claims_TenantId_PartId",
                table: "supplyarr_warranty_claims",
                columns: new[] { "TenantId", "PartId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_warranty_claims_TenantId_PurchaseOrderId",
                table: "supplyarr_warranty_claims",
                columns: new[] { "TenantId", "PurchaseOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_warranty_claims_TenantId_Status_UpdatedAt",
                table: "supplyarr_warranty_claims",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_warranty_claims_TenantId_VendorPartyId",
                table: "supplyarr_warranty_claims",
                columns: new[] { "TenantId", "VendorPartyId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_warranty_claims_VendorPartyId",
                table: "supplyarr_warranty_claims",
                column: "VendorPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_outbound_shipment_lines_FromInventoryBinId",
                table: "supplyarr_wms_outbound_shipment_lines",
                column: "FromInventoryBinId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_outbound_shipment_lines_OutboundShipmentId",
                table: "supplyarr_wms_outbound_shipment_lines",
                column: "OutboundShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_outbound_shipment_lines_PartId",
                table: "supplyarr_wms_outbound_shipment_lines",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_outbound_shipment_lines_TenantId",
                table: "supplyarr_wms_outbound_shipment_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_outbound_shipment_lines_TenantId_OutboundShip~",
                table: "supplyarr_wms_outbound_shipment_lines",
                columns: new[] { "TenantId", "OutboundShipmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_outbound_shipments_TenantId",
                table: "supplyarr_wms_outbound_shipments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_outbound_shipments_TenantId_IdempotencyKey",
                table: "supplyarr_wms_outbound_shipments",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_outbound_shipments_TenantId_ShipmentKey",
                table: "supplyarr_wms_outbound_shipments",
                columns: new[] { "TenantId", "ShipmentKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_outbound_shipments_TenantId_Status_UpdatedAt",
                table: "supplyarr_wms_outbound_shipments",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_stock_ledger_InventoryBinId",
                table: "supplyarr_wms_stock_ledger",
                column: "InventoryBinId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_stock_ledger_PartId",
                table: "supplyarr_wms_stock_ledger",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_stock_ledger_RelatedInventoryBinId",
                table: "supplyarr_wms_stock_ledger",
                column: "RelatedInventoryBinId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_stock_ledger_TenantId",
                table: "supplyarr_wms_stock_ledger",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_stock_ledger_TenantId_IdempotencyKey",
                table: "supplyarr_wms_stock_ledger",
                columns: new[] { "TenantId", "IdempotencyKey" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_stock_ledger_TenantId_MovementGroupId",
                table: "supplyarr_wms_stock_ledger",
                columns: new[] { "TenantId", "MovementGroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplyarr_wms_stock_ledger_TenantId_PartId_InventoryBinId_C~",
                table: "supplyarr_wms_stock_ledger",
                columns: new[] { "TenantId", "PartId", "InventoryBinId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "platform_metadata");

            migrationBuilder.DropTable(
                name: "supplyarr_approval_reminder_runs");

            migrationBuilder.DropTable(
                name: "supplyarr_approval_reminder_states");

            migrationBuilder.DropTable(
                name: "supplyarr_audit_events");

            migrationBuilder.DropTable(
                name: "supplyarr_availability_snapshot_runs");

            migrationBuilder.DropTable(
                name: "supplyarr_backorders");

            migrationBuilder.DropTable(
                name: "supplyarr_contracts");

            migrationBuilder.DropTable(
                name: "supplyarr_demand_processing_runs");

            migrationBuilder.DropTable(
                name: "supplyarr_demand_processing_states");

            migrationBuilder.DropTable(
                name: "supplyarr_integration_event_processing_runs");

            migrationBuilder.DropTable(
                name: "supplyarr_integration_inbox_events");

            migrationBuilder.DropTable(
                name: "supplyarr_integration_outbox_events");

            migrationBuilder.DropTable(
                name: "supplyarr_lead_time_snapshot_runs");

            migrationBuilder.DropTable(
                name: "supplyarr_maintainarr_demand_ref_lines");

            migrationBuilder.DropTable(
                name: "supplyarr_notification_dispatches");

            migrationBuilder.DropTable(
                name: "supplyarr_part_manufacturer_aliases");

            migrationBuilder.DropTable(
                name: "supplyarr_part_stock_reservations");

            migrationBuilder.DropTable(
                name: "supplyarr_part_vendor_availability_capture_states");

            migrationBuilder.DropTable(
                name: "supplyarr_part_vendor_availability_snapshots");

            migrationBuilder.DropTable(
                name: "supplyarr_part_vendor_lead_time_capture_states");

            migrationBuilder.DropTable(
                name: "supplyarr_part_vendor_lead_time_snapshots");

            migrationBuilder.DropTable(
                name: "supplyarr_part_vendor_price_capture_states");

            migrationBuilder.DropTable(
                name: "supplyarr_part_vendor_pricing_snapshots");

            migrationBuilder.DropTable(
                name: "supplyarr_party_compliance_documents");

            migrationBuilder.DropTable(
                name: "supplyarr_party_contacts");

            migrationBuilder.DropTable(
                name: "supplyarr_party_supplier_onboarding");

            migrationBuilder.DropTable(
                name: "supplyarr_price_snapshot_runs");

            migrationBuilder.DropTable(
                name: "supplyarr_procurement_coordination_events");

            migrationBuilder.DropTable(
                name: "supplyarr_procurement_coordination_runs");

            migrationBuilder.DropTable(
                name: "supplyarr_procurement_exception_escalation_events");

            migrationBuilder.DropTable(
                name: "supplyarr_procurement_exception_escalation_runs");

            migrationBuilder.DropTable(
                name: "supplyarr_procurement_exceptions");

            migrationBuilder.DropTable(
                name: "supplyarr_receiving_exceptions");

            migrationBuilder.DropTable(
                name: "supplyarr_rfq_vendor_invitations");

            migrationBuilder.DropTable(
                name: "supplyarr_routarr_demand_ref_lines");

            migrationBuilder.DropTable(
                name: "supplyarr_staffarr_demand_ref_lines");

            migrationBuilder.DropTable(
                name: "supplyarr_staffarr_procurement_approval_authority_mirrors");

            migrationBuilder.DropTable(
                name: "supplyarr_supplier_incidents");

            migrationBuilder.DropTable(
                name: "supplyarr_tenant_approval_reminder_settings");

            migrationBuilder.DropTable(
                name: "supplyarr_tenant_availability_snapshot_settings");

            migrationBuilder.DropTable(
                name: "supplyarr_tenant_demand_processing_settings");

            migrationBuilder.DropTable(
                name: "supplyarr_tenant_integration_event_settings");

            migrationBuilder.DropTable(
                name: "supplyarr_tenant_lead_time_snapshot_settings");

            migrationBuilder.DropTable(
                name: "supplyarr_tenant_notification_settings");

            migrationBuilder.DropTable(
                name: "supplyarr_tenant_price_snapshot_settings");

            migrationBuilder.DropTable(
                name: "supplyarr_tenant_procurement_coordination_settings");

            migrationBuilder.DropTable(
                name: "supplyarr_tenant_procurement_exception_escalation_settings");

            migrationBuilder.DropTable(
                name: "supplyarr_tenant_supplier_onboarding_settings");

            migrationBuilder.DropTable(
                name: "supplyarr_trainarr_demand_ref_lines");

            migrationBuilder.DropTable(
                name: "supplyarr_vendor_email_inbox_messages");

            migrationBuilder.DropTable(
                name: "supplyarr_vendor_quote_lines");

            migrationBuilder.DropTable(
                name: "supplyarr_vendor_return_lines");

            migrationBuilder.DropTable(
                name: "supplyarr_warranty_claims");

            migrationBuilder.DropTable(
                name: "supplyarr_wms_outbound_shipment_lines");

            migrationBuilder.DropTable(
                name: "supplyarr_wms_stock_ledger");

            migrationBuilder.DropTable(
                name: "supplyarr_maintainarr_demand_refs");

            migrationBuilder.DropTable(
                name: "supplyarr_part_stock_levels");

            migrationBuilder.DropTable(
                name: "supplyarr_part_vendor_links");

            migrationBuilder.DropTable(
                name: "supplyarr_procurement_coordination_records");

            migrationBuilder.DropTable(
                name: "supplyarr_routarr_demand_refs");

            migrationBuilder.DropTable(
                name: "supplyarr_staffarr_demand_refs");

            migrationBuilder.DropTable(
                name: "supplyarr_vendor_restrictions");

            migrationBuilder.DropTable(
                name: "supplyarr_trainarr_demand_refs");

            migrationBuilder.DropTable(
                name: "supplyarr_rfq_lines");

            migrationBuilder.DropTable(
                name: "supplyarr_vendor_quotes");

            migrationBuilder.DropTable(
                name: "supplyarr_vendor_returns");

            migrationBuilder.DropTable(
                name: "supplyarr_receiving_receipt_lines");

            migrationBuilder.DropTable(
                name: "supplyarr_wms_outbound_shipments");

            migrationBuilder.DropTable(
                name: "supplyarr_rfqs");

            migrationBuilder.DropTable(
                name: "supplyarr_purchase_order_lines");

            migrationBuilder.DropTable(
                name: "supplyarr_receiving_receipts");

            migrationBuilder.DropTable(
                name: "supplyarr_purchase_request_lines");

            migrationBuilder.DropTable(
                name: "supplyarr_inventory_bins");

            migrationBuilder.DropTable(
                name: "supplyarr_purchase_orders");

            migrationBuilder.DropTable(
                name: "supplyarr_parts");

            migrationBuilder.DropTable(
                name: "supplyarr_inventory_locations");

            migrationBuilder.DropTable(
                name: "supplyarr_purchase_requests");

            migrationBuilder.DropTable(
                name: "supplyarr_part_catalogs");

            migrationBuilder.DropTable(
                name: "supplyarr_external_parties");
        }
    }
}
