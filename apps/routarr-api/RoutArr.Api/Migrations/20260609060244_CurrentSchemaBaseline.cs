using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
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
                name: "routarr_attachment_retention_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AttachmentsPurgedCount = table.Column<int>(type: "integer", nullable: false),
                    BytesReclaimed = table.Column<long>(type: "bigint", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    SkipReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_attachment_retention_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_audit_events",
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
                    table.PrimaryKey("PK_routarr_audit_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_audit_package_generation_jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Format = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    FromUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ToUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FilterJson = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
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
                    table.PrimaryKey("PK_routarr_audit_package_generation_jobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_dispatch_exceptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExceptionKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IncidentType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IncidentSeverity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IncidentReviewStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IncidentRoutedProduct = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StaffarrPersonnelIncidentId = table.Column<Guid>(type: "uuid", nullable: true),
                    StaffarrIncidentRoutedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StaffarrIncidentRouteStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TrainarrIncidentRemediationId = table.Column<Guid>(type: "uuid", nullable: true),
                    TrainarrIncidentRoutedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TrainarrIncidentRouteStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MaintainarrInboundEventId = table.Column<Guid>(type: "uuid", nullable: true),
                    MaintainarrDefectId = table.Column<Guid>(type: "uuid", nullable: true),
                    MaintainarrIncidentRoutedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MaintainarrIncidentRouteStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CompliancecoreFactPublicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompliancecoreIncidentRoutedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompliancecoreIncidentRouteStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SlaDueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResolutionTemplateKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ResolutionNotes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_dispatch_exceptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_dispatch_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DispatchNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    DispatchDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DispatchType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PlannerPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DispatcherPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    StaffarrSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    RouteRefsJson = table.Column<string>(type: "text", nullable: false),
                    TripRefsJson = table.Column<string>(type: "text", nullable: false),
                    BlockerRefsJson = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    CreatedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReleasedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReleasedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CanceledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_dispatch_plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_driver_availability",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AvailabilityStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_driver_availability", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_driver_time_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EntryType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    EditReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_driver_time_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_equipment_availability",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleRefKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AvailabilityStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_equipment_availability", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_integration_outbox_events",
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
                    table.PrimaryKey("PK_routarr_integration_outbox_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_notification_dispatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: true),
                    RouteId = table.Column<Guid>(type: "uuid", nullable: true),
                    DriverPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
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
                    table.PrimaryKey("PK_routarr_notification_dispatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_staffarr_person_refs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    MirroredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_staffarr_person_refs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_supplyarr_shipment_intents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyarrShipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipmentKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DestinationName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DestinationAddressSnapshot = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RouteId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_supplyarr_shipment_intents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_tenant_attachment_retention_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    RetentionDaysAfterTripClose = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_tenant_attachment_retention_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_tenant_dispatch_board_state",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefaultScope = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_tenant_dispatch_board_state", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_tenant_integration_event_settings",
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
                    table.PrimaryKey("PK_routarr_tenant_integration_event_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_tenant_notification_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    NotificationWebhookUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    NotifyOnTripAssigned = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnTripDispatched = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnTripAccepted = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnTripInProgress = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnTripCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnTripCancelled = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnDriverAssignmentChanged = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnRouteCancelled = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_tenant_notification_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_tenant_trip_completion_rollup_settings",
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
                    table.PrimaryKey("PK_routarr_tenant_trip_completion_rollup_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_tenant_trip_execution_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirePreTripDvirBeforeStart = table.Column<bool>(type: "boolean", nullable: false),
                    RequirePostTripDvirBeforeComplete = table.Column<bool>(type: "boolean", nullable: false),
                    RequireDeliveryProofBeforeComplete = table.Column<bool>(type: "boolean", nullable: false),
                    RequirePickupProofBeforeStart = table.Column<bool>(type: "boolean", nullable: false),
                    BlockTripStartOnDvirFail = table.Column<bool>(type: "boolean", nullable: false),
                    BlockTripCompleteOnDvirFail = table.Column<bool>(type: "boolean", nullable: false),
                    RequirePickupProofPhotoBeforeStart = table.Column<bool>(type: "boolean", nullable: false),
                    RequireDeliveryProofPhotoBeforeComplete = table.Column<bool>(type: "boolean", nullable: false),
                    RequireDeliverySignatureBeforeComplete = table.Column<bool>(type: "boolean", nullable: false),
                    RequirePreTripDvirPhotoBeforeStart = table.Column<bool>(type: "boolean", nullable: false),
                    RequirePostTripDvirPhotoBeforeComplete = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_tenant_trip_execution_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_trip_completion_rollup_runs",
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
                    table.PrimaryKey("PK_routarr_trip_completion_rollup_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_trip_completion_rollups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DispatchStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AssignedDriverPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VehicleRefKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ScheduledStartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ScheduledEndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    RouteCount = table.Column<int>(type: "integer", nullable: false),
                    CompletedRouteCount = table.Column<int>(type: "integer", nullable: false),
                    StopCount = table.Column<int>(type: "integer", nullable: false),
                    CompletedStopCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedStopCount = table.Column<int>(type: "integer", nullable: false),
                    PendingStopCount = table.Column<int>(type: "integer", nullable: false),
                    LoadCount = table.Column<int>(type: "integer", nullable: false),
                    DeliveredLoadCount = table.Column<int>(type: "integer", nullable: false),
                    PendingLoadCount = table.Column<int>(type: "integer", nullable: false),
                    SourceUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_trip_completion_rollups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_trip_parts_demand_status_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoutarrPublicationId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_routarr_trip_parts_demand_status_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_trips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    DispatchStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AssignedDriverPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VehicleRefKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ScheduledStartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ScheduledEndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AcceptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DispatchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_trips", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_vehicle_refs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleRefKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AssetTag = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SourceProduct = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    MirroredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_vehicle_refs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_supplyarr_shipment_intent_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShipmentIntentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyarrShipmentLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartDisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_supplyarr_shipment_intent_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_supplyarr_shipment_intent_lines_routarr_supplyarr_s~",
                        column: x => x.ShipmentIntentId,
                        principalTable: "routarr_supplyarr_shipment_intents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "routarr_trip_completion_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    RollupId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventKind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Detail = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    SourceEntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceEntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_trip_completion_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_trip_completion_events_routarr_trip_completion_roll~",
                        column: x => x.RollupId,
                        principalTable: "routarr_trip_completion_rollups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "routarr_dispatch_messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SenderRole = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RequiresAcknowledgement = table.Column<bool>(type: "boolean", nullable: false),
                    AcknowledgedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcknowledgedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AcknowledgedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_dispatch_messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_dispatch_messages_routarr_trips_TripId",
                        column: x => x.TripId,
                        principalTable: "routarr_trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "routarr_routes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    RouteStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActivatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_routes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_routes_routarr_trips_TripId",
                        column: x => x.TripId,
                        principalTable: "routarr_trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "routarr_trip_capture_attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttachmentKind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CapturedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_trip_capture_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_trip_capture_attachments_routarr_trips_TripId",
                        column: x => x.TripId,
                        principalTable: "routarr_trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "routarr_trip_dispatch_release_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReleasedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReleasedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DriverCanAssign = table.Column<bool>(type: "boolean", nullable: false),
                    VehicleCanAssign = table.Column<bool>(type: "boolean", nullable: false),
                    HasMissingExternalData = table.Column<bool>(type: "boolean", nullable: false),
                    HasStaleExternalData = table.Column<bool>(type: "boolean", nullable: false),
                    Summary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    SnapshotJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_trip_dispatch_release_snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_trip_dispatch_release_snapshots_routarr_trips_TripId",
                        column: x => x.TripId,
                        principalTable: "routarr_trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "routarr_trip_dvir_inspections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    Phase = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    VehicleRefKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OdometerReading = table.Column<long>(type: "bigint", nullable: true),
                    DefectNotes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    MaintainarrInboundEventId = table.Column<Guid>(type: "uuid", nullable: true),
                    MaintainarrDefectId = table.Column<Guid>(type: "uuid", nullable: true),
                    MaintainarrEventRoutedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MaintainarrEventRouteStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SubmittedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_trip_dvir_inspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_trip_dvir_inspections_routarr_trips_TripId",
                        column: x => x.TripId,
                        principalTable: "routarr_trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "routarr_trip_loads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoadKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    LoadType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    OriginLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DestinationLabel = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_trip_loads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_trip_loads_routarr_trips_TripId",
                        column: x => x.TripId,
                        principalTable: "routarr_trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "routarr_trip_parts_demand_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    SupplyarrPartId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    QuantityRequested = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RoutarrPublicationId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_routarr_trip_parts_demand_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_trip_parts_demand_lines_routarr_trips_TripId",
                        column: x => x.TripId,
                        principalTable: "routarr_trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "routarr_trip_proof_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProofType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CapturedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    VehicleRefKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReferenceKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ReviewStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "pending_review"),
                    ReviewedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewNotes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false, defaultValue: ""),
                    CapturedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_trip_proof_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_trip_proof_records_routarr_trips_TripId",
                        column: x => x.TripId,
                        principalTable: "routarr_trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "routarr_route_stops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    StopKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AddressLabel = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    StaffarrSiteOrgUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    StaffarrSiteNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValue: ""),
                    StopType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StopStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    GeofenceAnchorLatitude = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: true),
                    GeofenceAnchorLongitude = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: true),
                    GeofenceRadiusMeters = table.Column<int>(type: "integer", nullable: true),
                    LastGeofenceCheckAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastGeofenceResult = table.Column<string>(type: "text", nullable: true),
                    LastGeofenceDistanceMeters = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    LastGeofenceReportedLatitude = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: true),
                    LastGeofenceReportedLongitude = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: true),
                    ScheduledArrivalAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ArrivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_route_stops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_route_stops_routarr_routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "routarr_routes",
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
                name: "IX_routarr_attachment_retention_runs_TenantId",
                table: "routarr_attachment_retention_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_attachment_retention_runs_TenantId_ProcessedAt",
                table: "routarr_attachment_retention_runs",
                columns: new[] { "TenantId", "ProcessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_audit_events_TenantId",
                table: "routarr_audit_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_audit_events_TenantId_OccurredAt",
                table: "routarr_audit_events",
                columns: new[] { "TenantId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_audit_package_generation_jobs_CreatedAt",
                table: "routarr_audit_package_generation_jobs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_audit_package_generation_jobs_TenantId_Status_Creat~",
                table: "routarr_audit_package_generation_jobs",
                columns: new[] { "TenantId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_exceptions_compliancecore_publication",
                table: "routarr_dispatch_exceptions",
                columns: new[] { "TenantId", "CompliancecoreFactPublicationId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_exceptions_maintainarr_defect",
                table: "routarr_dispatch_exceptions",
                columns: new[] { "TenantId", "MaintainarrDefectId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_exceptions_staffarr_incident",
                table: "routarr_dispatch_exceptions",
                columns: new[] { "TenantId", "StaffarrPersonnelIncidentId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_exceptions_TenantId",
                table: "routarr_dispatch_exceptions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_exceptions_TenantId_AssignedToUserId",
                table: "routarr_dispatch_exceptions",
                columns: new[] { "TenantId", "AssignedToUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_exceptions_TenantId_ExceptionKey",
                table: "routarr_dispatch_exceptions",
                columns: new[] { "TenantId", "ExceptionKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_exceptions_TenantId_IncidentReviewStatus_U~",
                table: "routarr_dispatch_exceptions",
                columns: new[] { "TenantId", "IncidentReviewStatus", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_exceptions_TenantId_IncidentType_UpdatedAt",
                table: "routarr_dispatch_exceptions",
                columns: new[] { "TenantId", "IncidentType", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_exceptions_TenantId_SlaDueAt",
                table: "routarr_dispatch_exceptions",
                columns: new[] { "TenantId", "SlaDueAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_exceptions_TenantId_Status_UpdatedAt",
                table: "routarr_dispatch_exceptions",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_exceptions_TenantId_TripId",
                table: "routarr_dispatch_exceptions",
                columns: new[] { "TenantId", "TripId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_exceptions_trainarr_remediation",
                table: "routarr_dispatch_exceptions",
                columns: new[] { "TenantId", "TrainarrIncidentRemediationId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_messages_TenantId",
                table: "routarr_dispatch_messages",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_messages_TenantId_TripId_CreatedAt",
                table: "routarr_dispatch_messages",
                columns: new[] { "TenantId", "TripId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_messages_TenantId_TripId_RequiresAcknowled~",
                table: "routarr_dispatch_messages",
                columns: new[] { "TenantId", "TripId", "RequiresAcknowledgement", "AcknowledgedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_messages_TripId",
                table: "routarr_dispatch_messages",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_plans_TenantId",
                table: "routarr_dispatch_plans",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_plans_TenantId_DispatchNumber",
                table: "routarr_dispatch_plans",
                columns: new[] { "TenantId", "DispatchNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_dispatch_plans_TenantId_Status_DispatchDate",
                table: "routarr_dispatch_plans",
                columns: new[] { "TenantId", "Status", "DispatchDate" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_driver_availability_TenantId",
                table: "routarr_driver_availability",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_driver_availability_TenantId_PersonId_StartsAt",
                table: "routarr_driver_availability",
                columns: new[] { "TenantId", "PersonId", "StartsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_driver_availability_TenantId_StartsAt_EndsAt",
                table: "routarr_driver_availability",
                columns: new[] { "TenantId", "StartsAt", "EndsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_driver_time_entries_TenantId",
                table: "routarr_driver_time_entries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_driver_time_entries_TenantId_PersonId_EntryType_Sta~",
                table: "routarr_driver_time_entries",
                columns: new[] { "TenantId", "PersonId", "EntryType", "StartsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_driver_time_entries_TenantId_PersonId_StartsAt",
                table: "routarr_driver_time_entries",
                columns: new[] { "TenantId", "PersonId", "StartsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_equipment_availability_TenantId",
                table: "routarr_equipment_availability",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_equipment_availability_TenantId_StartsAt_EndsAt",
                table: "routarr_equipment_availability",
                columns: new[] { "TenantId", "StartsAt", "EndsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_equipment_availability_TenantId_VehicleRefKey_Start~",
                table: "routarr_equipment_availability",
                columns: new[] { "TenantId", "VehicleRefKey", "StartsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_integration_outbox_events_TenantId",
                table: "routarr_integration_outbox_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_integration_outbox_events_TenantId_IdempotencyKey",
                table: "routarr_integration_outbox_events",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_integration_outbox_events_TenantId_ProcessingStatus~",
                table: "routarr_integration_outbox_events",
                columns: new[] { "TenantId", "ProcessingStatus", "NextRetryAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_notification_dispatches_TenantId",
                table: "routarr_notification_dispatches",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_notification_dispatches_TenantId_DispatchStatus_Cre~",
                table: "routarr_notification_dispatches",
                columns: new[] { "TenantId", "DispatchStatus", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_notification_dispatches_TenantId_EventKind_RelatedE~",
                table: "routarr_notification_dispatches",
                columns: new[] { "TenantId", "EventKind", "RelatedEntityType", "RelatedEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_route_stops_RouteId",
                table: "routarr_route_stops",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_route_stops_TenantId",
                table: "routarr_route_stops",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_route_stops_TenantId_RouteId",
                table: "routarr_route_stops",
                columns: new[] { "TenantId", "RouteId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_route_stops_TenantId_RouteId_SequenceNumber",
                table: "routarr_route_stops",
                columns: new[] { "TenantId", "RouteId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_route_stops_TenantId_RouteId_StopKey",
                table: "routarr_route_stops",
                columns: new[] { "TenantId", "RouteId", "StopKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_route_stops_TenantId_StaffarrSiteOrgUnitId",
                table: "routarr_route_stops",
                columns: new[] { "TenantId", "StaffarrSiteOrgUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_routes_TenantId",
                table: "routarr_routes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_routes_TenantId_RouteNumber",
                table: "routarr_routes",
                columns: new[] { "TenantId", "RouteNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_routes_TenantId_RouteStatus_UpdatedAt",
                table: "routarr_routes",
                columns: new[] { "TenantId", "RouteStatus", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_routes_TenantId_TripId",
                table: "routarr_routes",
                columns: new[] { "TenantId", "TripId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_routes_TripId",
                table: "routarr_routes",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_staffarr_person_refs_TenantId",
                table: "routarr_staffarr_person_refs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_staffarr_person_refs_TenantId_PersonId",
                table: "routarr_staffarr_person_refs",
                columns: new[] { "TenantId", "PersonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_supplyarr_shipment_intent_lines_ShipmentIntentId",
                table: "routarr_supplyarr_shipment_intent_lines",
                column: "ShipmentIntentId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_supplyarr_shipment_intent_lines_TenantId",
                table: "routarr_supplyarr_shipment_intent_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_supplyarr_shipment_intent_lines_TenantId_ShipmentIn~",
                table: "routarr_supplyarr_shipment_intent_lines",
                columns: new[] { "TenantId", "ShipmentIntentId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_supplyarr_shipment_intents_TenantId",
                table: "routarr_supplyarr_shipment_intents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_supplyarr_shipment_intents_TenantId_Status_UpdatedAt",
                table: "routarr_supplyarr_shipment_intents",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_supplyarr_shipment_intents_TenantId_SupplyarrShipme~",
                table: "routarr_supplyarr_shipment_intents",
                columns: new[] { "TenantId", "SupplyarrShipmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_attachment_retention_settings_TenantId",
                table: "routarr_tenant_attachment_retention_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_dispatch_board_state_TenantId",
                table: "routarr_tenant_dispatch_board_state",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_integration_event_settings_TenantId",
                table: "routarr_tenant_integration_event_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_notification_settings_TenantId",
                table: "routarr_tenant_notification_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_trip_completion_rollup_settings_TenantId",
                table: "routarr_tenant_trip_completion_rollup_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_tenant_trip_execution_settings_TenantId",
                table: "routarr_tenant_trip_execution_settings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_capture_attachments_TenantId",
                table: "routarr_trip_capture_attachments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_capture_attachments_TenantId_TripId",
                table: "routarr_trip_capture_attachments",
                columns: new[] { "TenantId", "TripId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_capture_attachments_TenantId_TripId_Attachment~",
                table: "routarr_trip_capture_attachments",
                columns: new[] { "TenantId", "TripId", "AttachmentKind", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_capture_attachments_TenantId_TripId_SubjectTyp~",
                table: "routarr_trip_capture_attachments",
                columns: new[] { "TenantId", "TripId", "SubjectType", "SubjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_capture_attachments_TripId",
                table: "routarr_trip_capture_attachments",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_completion_events_RollupId",
                table: "routarr_trip_completion_events",
                column: "RollupId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_completion_events_TenantId",
                table: "routarr_trip_completion_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_completion_events_TenantId_TripId_SequenceNumb~",
                table: "routarr_trip_completion_events",
                columns: new[] { "TenantId", "TripId", "SequenceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_completion_rollup_runs_TenantId",
                table: "routarr_trip_completion_rollup_runs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_completion_rollup_runs_TenantId_CreatedAt",
                table: "routarr_trip_completion_rollup_runs",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_completion_rollups_TenantId",
                table: "routarr_trip_completion_rollups",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_completion_rollups_TenantId_DispatchStatus_Com~",
                table: "routarr_trip_completion_rollups",
                columns: new[] { "TenantId", "DispatchStatus", "CompletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_completion_rollups_TenantId_TripId",
                table: "routarr_trip_completion_rollups",
                columns: new[] { "TenantId", "TripId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_dispatch_release_snapshots_TenantId",
                table: "routarr_trip_dispatch_release_snapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_dispatch_release_snapshots_TenantId_ReleasedAt",
                table: "routarr_trip_dispatch_release_snapshots",
                columns: new[] { "TenantId", "ReleasedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_dispatch_release_snapshots_TenantId_TripId",
                table: "routarr_trip_dispatch_release_snapshots",
                columns: new[] { "TenantId", "TripId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_dispatch_release_snapshots_TripId",
                table: "routarr_trip_dispatch_release_snapshots",
                column: "TripId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_dvir_inspections_maintainarr_defect",
                table: "routarr_trip_dvir_inspections",
                columns: new[] { "TenantId", "MaintainarrDefectId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_dvir_inspections_TenantId",
                table: "routarr_trip_dvir_inspections",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_dvir_inspections_TenantId_TripId",
                table: "routarr_trip_dvir_inspections",
                columns: new[] { "TenantId", "TripId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_dvir_inspections_TenantId_TripId_Phase",
                table: "routarr_trip_dvir_inspections",
                columns: new[] { "TenantId", "TripId", "Phase" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_dvir_inspections_TripId",
                table: "routarr_trip_dvir_inspections",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_loads_TenantId",
                table: "routarr_trip_loads",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_loads_TenantId_TripId",
                table: "routarr_trip_loads",
                columns: new[] { "TenantId", "TripId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_loads_TenantId_TripId_LoadKey",
                table: "routarr_trip_loads",
                columns: new[] { "TenantId", "TripId", "LoadKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_loads_TripId",
                table: "routarr_trip_loads",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_parts_demand_lines_TenantId",
                table: "routarr_trip_parts_demand_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_parts_demand_lines_TenantId_ProcurementStatus",
                table: "routarr_trip_parts_demand_lines",
                columns: new[] { "TenantId", "ProcurementStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_parts_demand_lines_TenantId_RoutarrPublication~",
                table: "routarr_trip_parts_demand_lines",
                columns: new[] { "TenantId", "RoutarrPublicationId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_parts_demand_lines_TenantId_TripId",
                table: "routarr_trip_parts_demand_lines",
                columns: new[] { "TenantId", "TripId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_parts_demand_lines_TenantId_TripId_LineNumber",
                table: "routarr_trip_parts_demand_lines",
                columns: new[] { "TenantId", "TripId", "LineNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_parts_demand_lines_TripId",
                table: "routarr_trip_parts_demand_lines",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_parts_demand_status_events_TenantId",
                table: "routarr_trip_parts_demand_status_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_parts_demand_status_events_TenantId_RoutarrPub~",
                table: "routarr_trip_parts_demand_status_events",
                columns: new[] { "TenantId", "RoutarrPublicationId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_parts_demand_status_events_TenantId_SupplyarrC~",
                table: "routarr_trip_parts_demand_status_events",
                columns: new[] { "TenantId", "SupplyarrCallbackPublicationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_proof_records_TenantId",
                table: "routarr_trip_proof_records",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_proof_records_TenantId_TripId",
                table: "routarr_trip_proof_records",
                columns: new[] { "TenantId", "TripId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_proof_records_TenantId_TripId_CapturedAt",
                table: "routarr_trip_proof_records",
                columns: new[] { "TenantId", "TripId", "CapturedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_proof_records_TenantId_TripId_ReviewStatus",
                table: "routarr_trip_proof_records",
                columns: new[] { "TenantId", "TripId", "ReviewStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trip_proof_records_TripId",
                table: "routarr_trip_proof_records",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trips_TenantId",
                table: "routarr_trips",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trips_TenantId_AcceptedAt",
                table: "routarr_trips",
                columns: new[] { "TenantId", "AcceptedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trips_TenantId_AssignedDriverPersonId",
                table: "routarr_trips",
                columns: new[] { "TenantId", "AssignedDriverPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trips_TenantId_DispatchStatus_UpdatedAt",
                table: "routarr_trips",
                columns: new[] { "TenantId", "DispatchStatus", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_trips_TenantId_TripNumber",
                table: "routarr_trips",
                columns: new[] { "TenantId", "TripNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_vehicle_refs_TenantId",
                table: "routarr_vehicle_refs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_vehicle_refs_TenantId_VehicleRefKey",
                table: "routarr_vehicle_refs",
                columns: new[] { "TenantId", "VehicleRefKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "platform_metadata");

            migrationBuilder.DropTable(
                name: "routarr_attachment_retention_runs");

            migrationBuilder.DropTable(
                name: "routarr_audit_events");

            migrationBuilder.DropTable(
                name: "routarr_audit_package_generation_jobs");

            migrationBuilder.DropTable(
                name: "routarr_dispatch_exceptions");

            migrationBuilder.DropTable(
                name: "routarr_dispatch_messages");

            migrationBuilder.DropTable(
                name: "routarr_dispatch_plans");

            migrationBuilder.DropTable(
                name: "routarr_driver_availability");

            migrationBuilder.DropTable(
                name: "routarr_driver_time_entries");

            migrationBuilder.DropTable(
                name: "routarr_equipment_availability");

            migrationBuilder.DropTable(
                name: "routarr_integration_outbox_events");

            migrationBuilder.DropTable(
                name: "routarr_notification_dispatches");

            migrationBuilder.DropTable(
                name: "routarr_route_stops");

            migrationBuilder.DropTable(
                name: "routarr_staffarr_person_refs");

            migrationBuilder.DropTable(
                name: "routarr_supplyarr_shipment_intent_lines");

            migrationBuilder.DropTable(
                name: "routarr_tenant_attachment_retention_settings");

            migrationBuilder.DropTable(
                name: "routarr_tenant_dispatch_board_state");

            migrationBuilder.DropTable(
                name: "routarr_tenant_integration_event_settings");

            migrationBuilder.DropTable(
                name: "routarr_tenant_notification_settings");

            migrationBuilder.DropTable(
                name: "routarr_tenant_trip_completion_rollup_settings");

            migrationBuilder.DropTable(
                name: "routarr_tenant_trip_execution_settings");

            migrationBuilder.DropTable(
                name: "routarr_trip_capture_attachments");

            migrationBuilder.DropTable(
                name: "routarr_trip_completion_events");

            migrationBuilder.DropTable(
                name: "routarr_trip_completion_rollup_runs");

            migrationBuilder.DropTable(
                name: "routarr_trip_dispatch_release_snapshots");

            migrationBuilder.DropTable(
                name: "routarr_trip_dvir_inspections");

            migrationBuilder.DropTable(
                name: "routarr_trip_loads");

            migrationBuilder.DropTable(
                name: "routarr_trip_parts_demand_lines");

            migrationBuilder.DropTable(
                name: "routarr_trip_parts_demand_status_events");

            migrationBuilder.DropTable(
                name: "routarr_trip_proof_records");

            migrationBuilder.DropTable(
                name: "routarr_vehicle_refs");

            migrationBuilder.DropTable(
                name: "routarr_routes");

            migrationBuilder.DropTable(
                name: "routarr_supplyarr_shipment_intents");

            migrationBuilder.DropTable(
                name: "routarr_trip_completion_rollups");

            migrationBuilder.DropTable(
                name: "routarr_trips");
        }
    }
}
