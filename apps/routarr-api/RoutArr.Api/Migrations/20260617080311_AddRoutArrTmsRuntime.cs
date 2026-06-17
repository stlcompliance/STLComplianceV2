using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoutArr.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRoutArrTmsRuntime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "routarr_carrier_tenders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransportationDemandId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenderNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RoutingGuideSequence = table.Column<int>(type: "integer", nullable: false),
                    CarrierSupplierRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CarrierSnapshotJson = table.Column<string>(type: "text", nullable: false),
                    TenderMethod = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RespondedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeclineReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CounterSummary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ProposedAlternative = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_carrier_tenders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_driver_capacity_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ShiftWindowStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ShiftWindowEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    HosRemainingMinutes = table.Column<int>(type: "integer", nullable: true),
                    DriveTimeRemainingMinutes = table.Column<int>(type: "integer", nullable: true),
                    OnDutyRemainingMinutes = table.Column<int>(type: "integer", nullable: true),
                    BreakRequiredBy = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DomicileLocationRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FeasibilityStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BlockerSummary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    SnapshotAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FreshnessState = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_driver_capacity_snapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_freight_accessorials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FreightRatingId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransportationDemandId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: true),
                    AccessorialType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceEventRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EvidenceRefsJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_freight_accessorials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_freight_claims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TransportationDemandId = table.Column<Guid>(type: "uuid", nullable: true),
                    TripId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClaimAgainstPartyType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ClaimReason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ClaimAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    RecoveryAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EvidenceRefsJson = table.Column<string>(type: "text", nullable: false),
                    AssurarrNonconformanceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SupplyarrPerformanceImpactRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    OrdarrCloseoutImpactRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_freight_claims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_freight_ratings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransportationDemandId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: true),
                    RatingNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BuyRateEstimate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    SellRateEstimate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PlannedFreightCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ActualFreightCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    RateSourceSnapshot = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    FuelSurcharge = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    AccessorialTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    VarianceAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    VarianceReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    AllocationSnapshotJson = table.Column<string>(type: "text", nullable: false),
                    AuditStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_freight_ratings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_mode_specific_requirement_refs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransportationDemandId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransportMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequirementType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceRequirementRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SummarySnapshot = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    DocumentRequirementRefsJson = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_mode_specific_requirement_refs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_portal_collaboration_submissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransportationDemandId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenderId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExternalActorType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExternalActorRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ActionType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SubmittedDataSummary = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    UploadedRecordRefsJson = table.Column<string>(type: "text", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReviewedByPersonId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_portal_collaboration_submissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_routing_guide_steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransportationDemandId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    CarrierSupplierRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CarrierSnapshotJson = table.Column<string>(type: "text", nullable: false),
                    TenderMethod = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ServiceLevel = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EquipmentRequirement = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LaneSnapshot = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    RateAgreementSnapshotRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FallbackType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_routing_guide_steps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_transportation_appointment_clocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransportationDemandId = table.Column<Guid>(type: "uuid", nullable: true),
                    TripId = table.Column<Guid>(type: "uuid", nullable: true),
                    StopId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClockType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_transportation_appointment_clocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_transportation_demands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DemandNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceObjectType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceObjectId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SourceObjectNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OriginLocationRef = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    DestinationLocationRef = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    RequestedPickupStartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RequestedPickupEndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RequestedDeliveryStartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RequestedDeliveryEndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PromisedPickupStartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PromisedPickupEndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PromisedDeliveryStartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PromisedDeliveryEndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ScheduledPickupStartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ScheduledPickupEndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ScheduledDeliveryStartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ScheduledDeliveryEndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TransportMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ServiceLevel = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EquipmentRequirement = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    HandlingRequirementsJson = table.Column<string>(type: "text", nullable: false),
                    CustomerRefsJson = table.Column<string>(type: "text", nullable: false),
                    OrderRefsJson = table.Column<string>(type: "text", nullable: false),
                    VendorRefsJson = table.Column<string>(type: "text", nullable: false),
                    RequirementRefsJson = table.Column<string>(type: "text", nullable: false),
                    PlanningStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TenderStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RatingStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    VisibilityStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FreshnessState = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: true),
                    RouteId = table.Column<Guid>(type: "uuid", nullable: true),
                    DispatchPlanId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CanceledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_transportation_demands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_transportation_document_packet_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransportationDemandId = table.Column<Guid>(type: "uuid", nullable: true),
                    TripId = table.Column<Guid>(type: "uuid", nullable: true),
                    PacketType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequiredDocumentTypesJson = table.Column<string>(type: "text", nullable: false),
                    SourceFactsJson = table.Column<string>(type: "text", nullable: false),
                    RecordPackageRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_transportation_document_packet_requests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_transportation_finance_packet_contributions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContributionNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TransportationDemandId = table.Column<Guid>(type: "uuid", nullable: true),
                    TripId = table.Column<Guid>(type: "uuid", nullable: true),
                    FreightRatingId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContributionType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TargetProduct = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OperationalSummary = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    CostSnapshotJson = table.Column<string>(type: "text", nullable: false),
                    AccessorialRefsJson = table.Column<string>(type: "text", nullable: false),
                    ProofRefsJson = table.Column<string>(type: "text", nullable: false),
                    DocumentPacketRefsJson = table.Column<string>(type: "text", nullable: false),
                    ClaimRefsJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AcceptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_transportation_finance_packet_contributions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_transportation_planning_scenarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScenarioNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Objective = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DemandRefsJson = table.Column<string>(type: "text", nullable: false),
                    RouteRefsJson = table.Column<string>(type: "text", nullable: false),
                    TripRefsJson = table.Column<string>(type: "text", nullable: false),
                    HardBlockersJson = table.Column<string>(type: "text", nullable: false),
                    WarningsJson = table.Column<string>(type: "text", nullable: false),
                    ServiceRiskEstimate = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    CostEstimate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_transportation_planning_scenarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_transportation_planning_suggestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanningScenarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    SuggestionType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Summary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    HardBlockersJson = table.Column<string>(type: "text", nullable: false),
                    SoftWarningsJson = table.Column<string>(type: "text", nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    EstimatedMiles = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    EstimatedServiceRisk = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    AffectedDemandRefsJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_transportation_planning_suggestions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_transportation_tracking_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransportationDemandId = table.Column<Guid>(type: "uuid", nullable: true),
                    TripId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CurrentLatitude = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: true),
                    CurrentLongitude = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: true),
                    CurrentEta = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EtaConfidence = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LastVisibilityEventId = table.Column<Guid>(type: "uuid", nullable: true),
                    TrackingSource = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FreshnessState = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StaleReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_transportation_tracking_snapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_transportation_visibility_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransportationDemandId = table.Column<Guid>(type: "uuid", nullable: true),
                    TripId = table.Column<Guid>(type: "uuid", nullable: true),
                    StopId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceOccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    NormalizedStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: true),
                    Eta = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EtaConfidence = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FreshnessState = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReviewStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RawExternalRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Summary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    UpdatedTrackingState = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_transportation_visibility_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_transportation_yard_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransportationDemandId = table.Column<Guid>(type: "uuid", nullable: true),
                    TripId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TrailerAssetRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TractorAssetRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StaffarrYardLocationRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StaffarrDockLocationRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LoadedEmptyStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SealNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EvidenceRefsJson = table.Column<string>(type: "text", nullable: false),
                    DispatchImpact = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_transportation_yard_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routarr_transportation_demand_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransportationDemandId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceObjectRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DescriptionSnapshot = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    QuantitySnapshot = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    WeightSnapshot = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    VolumeSnapshot = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    PalletCountSnapshot = table.Column<int>(type: "integer", nullable: true),
                    HandlingRequirementSnapshot = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_transportation_demand_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_transportation_demand_lines_routarr_transportation_~",
                        column: x => x.TransportationDemandId,
                        principalTable: "routarr_transportation_demands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "routarr_transportation_demand_requirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransportationDemandId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceRequirementRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EvidenceRefsJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_transportation_demand_requirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_transportation_demand_requirements_routarr_transpor~",
                        column: x => x.TransportationDemandId,
                        principalTable: "routarr_transportation_demands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "routarr_transportation_demand_source_refs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransportationDemandId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceProduct = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceObjectType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceObjectId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SourceObjectNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DisplayNameSnapshot = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StatusSnapshot = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SnapshotAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FreshnessState = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routarr_transportation_demand_source_refs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routarr_transportation_demand_source_refs_routarr_transport~",
                        column: x => x.TransportationDemandId,
                        principalTable: "routarr_transportation_demands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_carrier_tenders_TenantId",
                table: "routarr_carrier_tenders",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_carrier_tenders_TenantId_TenderNumber",
                table: "routarr_carrier_tenders",
                columns: new[] { "TenantId", "TenderNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_carrier_tenders_TenantId_TransportationDemandId_Sta~",
                table: "routarr_carrier_tenders",
                columns: new[] { "TenantId", "TransportationDemandId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_driver_capacity_snapshots_TenantId",
                table: "routarr_driver_capacity_snapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_driver_capacity_snapshots_TenantId_PersonId_Snapsho~",
                table: "routarr_driver_capacity_snapshots",
                columns: new[] { "TenantId", "PersonId", "SnapshotAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_freight_accessorials_TenantId",
                table: "routarr_freight_accessorials",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_freight_accessorials_TenantId_FreightRatingId",
                table: "routarr_freight_accessorials",
                columns: new[] { "TenantId", "FreightRatingId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_freight_claims_TenantId",
                table: "routarr_freight_claims",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_freight_claims_TenantId_ClaimNumber",
                table: "routarr_freight_claims",
                columns: new[] { "TenantId", "ClaimNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_freight_claims_TenantId_TransportationDemandId_Stat~",
                table: "routarr_freight_claims",
                columns: new[] { "TenantId", "TransportationDemandId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_freight_ratings_TenantId",
                table: "routarr_freight_ratings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_freight_ratings_TenantId_RatingNumber",
                table: "routarr_freight_ratings",
                columns: new[] { "TenantId", "RatingNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_freight_ratings_TenantId_TransportationDemandId_Sta~",
                table: "routarr_freight_ratings",
                columns: new[] { "TenantId", "TransportationDemandId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_mode_specific_requirement_refs_TenantId",
                table: "routarr_mode_specific_requirement_refs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_mode_specific_requirement_refs_TenantId_Transportat~",
                table: "routarr_mode_specific_requirement_refs",
                columns: new[] { "TenantId", "TransportationDemandId", "TransportMode" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_portal_collaboration_submissions_TenantId",
                table: "routarr_portal_collaboration_submissions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_portal_collaboration_submissions_TenantId_TenderId_~",
                table: "routarr_portal_collaboration_submissions",
                columns: new[] { "TenantId", "TenderId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_portal_collaboration_submissions_TenantId_Transport~",
                table: "routarr_portal_collaboration_submissions",
                columns: new[] { "TenantId", "TransportationDemandId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_routing_guide_steps_TenantId",
                table: "routarr_routing_guide_steps",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_routing_guide_steps_TenantId_TransportationDemandId~",
                table: "routarr_routing_guide_steps",
                columns: new[] { "TenantId", "TransportationDemandId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_appointment_clocks_TenantId",
                table: "routarr_transportation_appointment_clocks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_appointment_clocks_TenantId_Transpor~",
                table: "routarr_transportation_appointment_clocks",
                columns: new[] { "TenantId", "TransportationDemandId", "ClockType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_demand_lines_TenantId",
                table: "routarr_transportation_demand_lines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_demand_lines_TenantId_Transportation~",
                table: "routarr_transportation_demand_lines",
                columns: new[] { "TenantId", "TransportationDemandId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_demand_lines_TransportationDemandId",
                table: "routarr_transportation_demand_lines",
                column: "TransportationDemandId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_demand_requirements_TenantId",
                table: "routarr_transportation_demand_requirements",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_demand_requirements_TenantId_Transpo~",
                table: "routarr_transportation_demand_requirements",
                columns: new[] { "TenantId", "TransportationDemandId", "RequirementType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_demand_requirements_TransportationDe~",
                table: "routarr_transportation_demand_requirements",
                column: "TransportationDemandId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_demand_source_refs_TenantId",
                table: "routarr_transportation_demand_source_refs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_demand_source_refs_TenantId_SourcePr~",
                table: "routarr_transportation_demand_source_refs",
                columns: new[] { "TenantId", "SourceProduct", "SourceObjectType", "SourceObjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_demand_source_refs_TenantId_Transpor~",
                table: "routarr_transportation_demand_source_refs",
                columns: new[] { "TenantId", "TransportationDemandId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_demand_source_refs_TransportationDem~",
                table: "routarr_transportation_demand_source_refs",
                column: "TransportationDemandId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_demands_TenantId",
                table: "routarr_transportation_demands",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_demands_TenantId_DemandNumber",
                table: "routarr_transportation_demands",
                columns: new[] { "TenantId", "DemandNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_demands_TenantId_RouteId",
                table: "routarr_transportation_demands",
                columns: new[] { "TenantId", "RouteId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_demands_TenantId_SourceProduct_Sourc~",
                table: "routarr_transportation_demands",
                columns: new[] { "TenantId", "SourceProduct", "SourceObjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_demands_TenantId_Status_UpdatedAt",
                table: "routarr_transportation_demands",
                columns: new[] { "TenantId", "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_demands_TenantId_TripId",
                table: "routarr_transportation_demands",
                columns: new[] { "TenantId", "TripId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_document_packet_requests_TenantId",
                table: "routarr_transportation_document_packet_requests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_document_packet_requests_TenantId_T~1",
                table: "routarr_transportation_document_packet_requests",
                columns: new[] { "TenantId", "TripId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_document_packet_requests_TenantId_Tr~",
                table: "routarr_transportation_document_packet_requests",
                columns: new[] { "TenantId", "TransportationDemandId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_finance_packet_contributions_Tenant~1",
                table: "routarr_transportation_finance_packet_contributions",
                columns: new[] { "TenantId", "TargetProduct", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_finance_packet_contributions_Tenant~2",
                table: "routarr_transportation_finance_packet_contributions",
                columns: new[] { "TenantId", "TransportationDemandId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_finance_packet_contributions_TenantI~",
                table: "routarr_transportation_finance_packet_contributions",
                columns: new[] { "TenantId", "ContributionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_finance_packet_contributions_TenantId",
                table: "routarr_transportation_finance_packet_contributions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_planning_scenarios_TenantId",
                table: "routarr_transportation_planning_scenarios",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_planning_scenarios_TenantId_Scenario~",
                table: "routarr_transportation_planning_scenarios",
                columns: new[] { "TenantId", "ScenarioNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_planning_scenarios_TenantId_Status_C~",
                table: "routarr_transportation_planning_scenarios",
                columns: new[] { "TenantId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_planning_suggestions_TenantId",
                table: "routarr_transportation_planning_suggestions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_planning_suggestions_TenantId_Planni~",
                table: "routarr_transportation_planning_suggestions",
                columns: new[] { "TenantId", "PlanningScenarioId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_tracking_snapshots_TenantId",
                table: "routarr_transportation_tracking_snapshots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_tracking_snapshots_TenantId_Transpor~",
                table: "routarr_transportation_tracking_snapshots",
                columns: new[] { "TenantId", "TransportationDemandId" },
                unique: true,
                filter: "\"TransportationDemandId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_tracking_snapshots_TenantId_TripId",
                table: "routarr_transportation_tracking_snapshots",
                columns: new[] { "TenantId", "TripId" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_visibility_events_TenantId",
                table: "routarr_transportation_visibility_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_visibility_events_TenantId_RawExtern~",
                table: "routarr_transportation_visibility_events",
                columns: new[] { "TenantId", "RawExternalRef" },
                unique: true,
                filter: "\"RawExternalRef\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_visibility_events_TenantId_Transport~",
                table: "routarr_transportation_visibility_events",
                columns: new[] { "TenantId", "TransportationDemandId", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_visibility_events_TenantId_TripId_Re~",
                table: "routarr_transportation_visibility_events",
                columns: new[] { "TenantId", "TripId", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_yard_events_TenantId",
                table: "routarr_transportation_yard_events",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_yard_events_TenantId_TransportationD~",
                table: "routarr_transportation_yard_events",
                columns: new[] { "TenantId", "TransportationDemandId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_routarr_transportation_yard_events_TenantId_TripId_Occurred~",
                table: "routarr_transportation_yard_events",
                columns: new[] { "TenantId", "TripId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "routarr_carrier_tenders");

            migrationBuilder.DropTable(
                name: "routarr_driver_capacity_snapshots");

            migrationBuilder.DropTable(
                name: "routarr_freight_accessorials");

            migrationBuilder.DropTable(
                name: "routarr_freight_claims");

            migrationBuilder.DropTable(
                name: "routarr_freight_ratings");

            migrationBuilder.DropTable(
                name: "routarr_mode_specific_requirement_refs");

            migrationBuilder.DropTable(
                name: "routarr_portal_collaboration_submissions");

            migrationBuilder.DropTable(
                name: "routarr_routing_guide_steps");

            migrationBuilder.DropTable(
                name: "routarr_transportation_appointment_clocks");

            migrationBuilder.DropTable(
                name: "routarr_transportation_demand_lines");

            migrationBuilder.DropTable(
                name: "routarr_transportation_demand_requirements");

            migrationBuilder.DropTable(
                name: "routarr_transportation_demand_source_refs");

            migrationBuilder.DropTable(
                name: "routarr_transportation_document_packet_requests");

            migrationBuilder.DropTable(
                name: "routarr_transportation_finance_packet_contributions");

            migrationBuilder.DropTable(
                name: "routarr_transportation_planning_scenarios");

            migrationBuilder.DropTable(
                name: "routarr_transportation_planning_suggestions");

            migrationBuilder.DropTable(
                name: "routarr_transportation_tracking_snapshots");

            migrationBuilder.DropTable(
                name: "routarr_transportation_visibility_events");

            migrationBuilder.DropTable(
                name: "routarr_transportation_yard_events");

            migrationBuilder.DropTable(
                name: "routarr_transportation_demands");
        }
    }
}
