using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

public sealed class TransportationDemand : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string DemandNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = TransportationDemandStatuses.Draft;
    public string SourceProduct { get; set; } = "manual";
    public string SourceObjectType { get; set; } = string.Empty;
    public string? SourceObjectId { get; set; }
    public string? SourceObjectNumber { get; set; }
    public string OriginLocationRef { get; set; } = string.Empty;
    public string DestinationLocationRef { get; set; } = string.Empty;
    public DateTimeOffset? RequestedPickupStartAt { get; set; }
    public DateTimeOffset? RequestedPickupEndAt { get; set; }
    public DateTimeOffset? RequestedDeliveryStartAt { get; set; }
    public DateTimeOffset? RequestedDeliveryEndAt { get; set; }
    public DateTimeOffset? PromisedPickupStartAt { get; set; }
    public DateTimeOffset? PromisedPickupEndAt { get; set; }
    public DateTimeOffset? PromisedDeliveryStartAt { get; set; }
    public DateTimeOffset? PromisedDeliveryEndAt { get; set; }
    public DateTimeOffset? ScheduledPickupStartAt { get; set; }
    public DateTimeOffset? ScheduledPickupEndAt { get; set; }
    public DateTimeOffset? ScheduledDeliveryStartAt { get; set; }
    public DateTimeOffset? ScheduledDeliveryEndAt { get; set; }
    public string TransportMode { get; set; } = TransportationModes.Truckload;
    public string ServiceLevel { get; set; } = "standard";
    public string EquipmentRequirement { get; set; } = string.Empty;
    public string HandlingRequirementsJson { get; set; } = "[]";
    public string CustomerRefsJson { get; set; } = "[]";
    public string OrderRefsJson { get; set; } = "[]";
    public string VendorRefsJson { get; set; } = "[]";
    public string RequirementRefsJson { get; set; } = "[]";
    public string PlanningStatus { get; set; } = "not_started";
    public string TenderStatus { get; set; } = "not_required";
    public string RatingStatus { get; set; } = "not_rated";
    public string VisibilityStatus { get; set; } = "not_tracking";
    public string FreshnessState { get; set; } = "live";
    public Guid? TripId { get; set; }
    public Guid? RouteId { get; set; }
    public Guid? DispatchPlanId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? CanceledAt { get; set; }
    public string? CancelReason { get; set; }

    public ICollection<TransportationDemandLine> Lines { get; set; } = [];
    public ICollection<TransportationDemandRequirement> Requirements { get; set; } = [];
    public ICollection<TransportationDemandSourceRef> SourceRefs { get; set; } = [];
}

public sealed class TransportationDemandLine : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TransportationDemandId { get; set; }
    public int LineNumber { get; set; }
    public string SourceProduct { get; set; } = "manual";
    public string? SourceObjectRef { get; set; }
    public string DescriptionSnapshot { get; set; } = string.Empty;
    public decimal? QuantitySnapshot { get; set; }
    public string UnitOfMeasure { get; set; } = "each";
    public decimal? WeightSnapshot { get; set; }
    public decimal? VolumeSnapshot { get; set; }
    public int? PalletCountSnapshot { get; set; }
    public string HandlingRequirementSnapshot { get; set; } = string.Empty;
    public TransportationDemand? TransportationDemand { get; set; }
}

public sealed class TransportationDemandRequirement : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TransportationDemandId { get; set; }
    public string RequirementType { get; set; } = "other";
    public string SourceProduct { get; set; } = "routarr";
    public string? SourceRequirementRef { get; set; }
    public bool Required { get; set; } = true;
    public string Status { get; set; } = "pending";
    public string EvidenceRefsJson { get; set; } = "[]";
    public TransportationDemand? TransportationDemand { get; set; }
}

public sealed class TransportationDemandSourceRef : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TransportationDemandId { get; set; }
    public string SourceProduct { get; set; } = string.Empty;
    public string SourceObjectType { get; set; } = string.Empty;
    public string SourceObjectId { get; set; } = string.Empty;
    public string? SourceObjectNumber { get; set; }
    public string DisplayNameSnapshot { get; set; } = string.Empty;
    public string StatusSnapshot { get; set; } = string.Empty;
    public DateTimeOffset SnapshotAt { get; set; }
    public string FreshnessState { get; set; } = "live";
    public TransportationDemand? TransportationDemand { get; set; }
}

public sealed class CarrierTender : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TransportationDemandId { get; set; }
    public string TenderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = CarrierTenderStatuses.Created;
    public int RoutingGuideSequence { get; set; }
    public string CarrierSupplierRef { get; set; } = string.Empty;
    public string CarrierSnapshotJson { get; set; } = "{}";
    public string TenderMethod { get; set; } = "manual";
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset? RespondedAt { get; set; }
    public string? DeclineReason { get; set; }
    public string? CounterSummary { get; set; }
    public string? ProposedAlternative { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class RoutingGuideStep : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TransportationDemandId { get; set; }
    public int Sequence { get; set; }
    public string CarrierSupplierRef { get; set; } = string.Empty;
    public string CarrierSnapshotJson { get; set; } = "{}";
    public string TenderMethod { get; set; } = "manual";
    public string ServiceLevel { get; set; } = "standard";
    public string EquipmentRequirement { get; set; } = string.Empty;
    public string LaneSnapshot { get; set; } = string.Empty;
    public string RateAgreementSnapshotRef { get; set; } = string.Empty;
    public string FallbackType { get; set; } = "none";
    public string Status { get; set; } = "available";
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class FreightRating : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TransportationDemandId { get; set; }
    public Guid? TripId { get; set; }
    public string RatingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = FreightRatingStatuses.Estimated;
    public decimal? BuyRateEstimate { get; set; }
    public decimal? SellRateEstimate { get; set; }
    public decimal? PlannedFreightCost { get; set; }
    public decimal? ActualFreightCost { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string RateSourceSnapshot { get; set; } = string.Empty;
    public decimal? FuelSurcharge { get; set; }
    public decimal? AccessorialTotal { get; set; }
    public decimal? VarianceAmount { get; set; }
    public string? VarianceReason { get; set; }
    public string AllocationSnapshotJson { get; set; } = "[]";
    public string AuditStatus { get; set; } = "not_reviewed";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class FreightAccessorial : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid FreightRatingId { get; set; }
    public Guid TransportationDemandId { get; set; }
    public Guid? TripId { get; set; }
    public string AccessorialType { get; set; } = "other";
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string Status { get; set; } = "pending_review";
    public string? SourceEventRef { get; set; }
    public string EvidenceRefsJson { get; set; } = "[]";
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class TransportationVisibilityEvent : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? TransportationDemandId { get; set; }
    public Guid? TripId { get; set; }
    public Guid? StopId { get; set; }
    public string EventType { get; set; } = "status_update";
    public string Source { get; set; } = "manual_check_call";
    public DateTimeOffset SourceOccurredAt { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }
    public string NormalizedStatus { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public DateTimeOffset? Eta { get; set; }
    public string EtaConfidence { get; set; } = "unknown";
    public string FreshnessState { get; set; } = "live";
    public string ReviewStatus { get; set; } = "accepted";
    public string? RawExternalRef { get; set; }
    public string Summary { get; set; } = string.Empty;
    public bool UpdatedTrackingState { get; set; }
}

public sealed class TransportationTrackingSnapshot : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? TransportationDemandId { get; set; }
    public Guid? TripId { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public decimal? CurrentLatitude { get; set; }
    public decimal? CurrentLongitude { get; set; }
    public DateTimeOffset? CurrentEta { get; set; }
    public string EtaConfidence { get; set; } = "unknown";
    public Guid? LastVisibilityEventId { get; set; }
    public string TrackingSource { get; set; } = string.Empty;
    public string FreshnessState { get; set; } = "live";
    public string? StaleReason { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class TransportationPlanningScenario : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string ScenarioNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
    public string Objective { get; set; } = "balance_cost_service";
    public string DemandRefsJson { get; set; } = "[]";
    public string RouteRefsJson { get; set; } = "[]";
    public string TripRefsJson { get; set; } = "[]";
    public string HardBlockersJson { get; set; } = "[]";
    public string WarningsJson { get; set; } = "[]";
    public decimal? ServiceRiskEstimate { get; set; }
    public decimal? CostEstimate { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? EvaluatedAt { get; set; }
}

public sealed class TransportationPlanningSuggestion : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PlanningScenarioId { get; set; }
    public string SuggestionType { get; set; } = "consolidate_demands";
    public string Status { get; set; } = "proposed";
    public string Summary { get; set; } = string.Empty;
    public string HardBlockersJson { get; set; } = "[]";
    public string SoftWarningsJson { get; set; } = "[]";
    public decimal? EstimatedCost { get; set; }
    public decimal? EstimatedMiles { get; set; }
    public decimal? EstimatedServiceRisk { get; set; }
    public string AffectedDemandRefsJson { get; set; } = "[]";
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class DriverCapacitySnapshot : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string PersonId { get; set; } = string.Empty;
    public string Source { get; set; } = "dispatcher";
    public DateTimeOffset? ShiftWindowStart { get; set; }
    public DateTimeOffset? ShiftWindowEnd { get; set; }
    public int? HosRemainingMinutes { get; set; }
    public int? DriveTimeRemainingMinutes { get; set; }
    public int? OnDutyRemainingMinutes { get; set; }
    public DateTimeOffset? BreakRequiredBy { get; set; }
    public string DomicileLocationRef { get; set; } = string.Empty;
    public string FeasibilityStatus { get; set; } = "unknown";
    public string BlockerSummary { get; set; } = string.Empty;
    public DateTimeOffset SnapshotAt { get; set; }
    public string FreshnessState { get; set; } = "live";
}

public sealed class TransportationYardEvent : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? TransportationDemandId { get; set; }
    public Guid? TripId { get; set; }
    public string EventType { get; set; } = "gate_in";
    public string TrailerAssetRef { get; set; } = string.Empty;
    public string TractorAssetRef { get; set; } = string.Empty;
    public string StaffarrYardLocationRef { get; set; } = string.Empty;
    public string StaffarrDockLocationRef { get; set; } = string.Empty;
    public string LoadedEmptyStatus { get; set; } = "unknown";
    public string? SealNumber { get; set; }
    public string Source { get; set; } = "dispatcher";
    public DateTimeOffset OccurredAt { get; set; }
    public string EvidenceRefsJson { get; set; } = "[]";
    public string DispatchImpact { get; set; } = string.Empty;
}

public sealed class PortalCollaborationSubmission : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? TransportationDemandId { get; set; }
    public Guid? TenderId { get; set; }
    public string ExternalActorType { get; set; } = "carrier_contact";
    public string ExternalActorRef { get; set; } = string.Empty;
    public string ActionType { get; set; } = "submit_status_update";
    public string Status { get; set; } = "review_required";
    public string SubmittedDataSummary { get; set; } = string.Empty;
    public string UploadedRecordRefsJson { get; set; } = "[]";
    public DateTimeOffset SubmittedAt { get; set; }
    public string? ReviewedByPersonId { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
}

public sealed class FreightClaim : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public Guid? TransportationDemandId { get; set; }
    public Guid? TripId { get; set; }
    public string ClaimAgainstPartyType { get; set; } = "carrier";
    public string ClaimReason { get; set; } = "damage";
    public decimal? ClaimAmount { get; set; }
    public decimal? RecoveryAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string Status { get; set; } = "requested";
    public string EvidenceRefsJson { get; set; } = "[]";
    public string? AssurarrNonconformanceRef { get; set; }
    public string? SupplyarrPerformanceImpactRef { get; set; }
    public string? OrdarrCloseoutImpactRef { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class TransportationDocumentPacketRequest : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? TransportationDemandId { get; set; }
    public Guid? TripId { get; set; }
    public string PacketType { get; set; } = "trip_packet";
    public string Status { get; set; } = "requested";
    public string RequiredDocumentTypesJson { get; set; } = "[]";
    public string SourceFactsJson { get; set; } = "{}";
    public string? RecordPackageRef { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class TransportationAppointmentClock : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? TransportationDemandId { get; set; }
    public Guid? TripId { get; set; }
    public Guid? StopId { get; set; }
    public string ClockType { get; set; } = "detention";
    public string Status { get; set; } = "started";
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public sealed class ModeSpecificRequirementRef : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TransportationDemandId { get; set; }
    public string TransportMode { get; set; } = TransportationModes.Truckload;
    public string RequirementType { get; set; } = string.Empty;
    public string SourceProduct { get; set; } = "routarr";
    public string? SourceRequirementRef { get; set; }
    public string SummarySnapshot { get; set; } = string.Empty;
    public string DocumentRequirementRefsJson { get; set; } = "[]";
    public string Status { get; set; } = "pending";
}

public sealed class TransportationFinancePacketContribution : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string ContributionNumber { get; set; } = string.Empty;
    public Guid? TransportationDemandId { get; set; }
    public Guid? TripId { get; set; }
    public Guid? FreightRatingId { get; set; }
    public string ContributionType { get; set; } = "invoice_ready_context";
    public string TargetProduct { get; set; } = "ordarr";
    public string Status { get; set; } = "draft";
    public string OperationalSummary { get; set; } = string.Empty;
    public string CostSnapshotJson { get; set; } = "{}";
    public string AccessorialRefsJson { get; set; } = "[]";
    public string ProofRefsJson { get; set; } = "[]";
    public string DocumentPacketRefsJson { get; set; } = "[]";
    public string ClaimRefsJson { get; set; } = "[]";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
}

public static class TransportationDemandStatuses
{
    public const string Draft = "draft";
    public const string ReadyForPlanning = "ready_for_planning";
    public const string Planning = "planning";
    public const string Planned = "planned";
    public const string Assigned = "assigned";
    public const string TenderRequired = "tender_required";
    public const string Tendered = "tendered";
    public const string Accepted = "accepted";
    public const string Dispatched = "dispatched";
    public const string InTransit = "in_transit";
    public const string Delivered = "delivered";
    public const string Closed = "closed";
    public const string Canceled = "canceled";
    public const string Blocked = "blocked";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Draft,
        ReadyForPlanning,
        Planning,
        Planned,
        Assigned,
        TenderRequired,
        Tendered,
        Accepted,
        Dispatched,
        InTransit,
        Delivered,
        Closed,
        Canceled,
        Blocked,
    };
}

public static class CarrierTenderStatuses
{
    public const string Draft = "draft";
    public const string Created = "created";
    public const string Sent = "sent";
    public const string Accepted = "accepted";
    public const string Rejected = "rejected";
    public const string Expired = "expired";
    public const string Countered = "countered";
    public const string Withdrawn = "withdrawn";
    public const string FallbackRequired = "fallback_required";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Draft,
        Created,
        Sent,
        Accepted,
        Rejected,
        Expired,
        Countered,
        Withdrawn,
        FallbackRequired,
    };
}

public static class FreightRatingStatuses
{
    public const string Estimated = "estimated";
    public const string Quoted = "quoted";
    public const string Planned = "planned";
    public const string Actualized = "actualized";
    public const string VarianceDetected = "variance_detected";
    public const string AuditException = "audit_exception";
    public const string Canceled = "canceled";
}

public static class TransportationModes
{
    public const string PrivateFleet = "private_fleet";
    public const string DedicatedCarrier = "dedicated_carrier";
    public const string Truckload = "truckload";
    public const string Ltl = "ltl";
    public const string Parcel = "parcel";
    public const string Intermodal = "intermodal";
    public const string Rail = "rail";
    public const string Drayage = "drayage";
    public const string Ocean = "ocean";
    public const string Air = "air";
    public const string Courier = "courier";
    public const string Shuttle = "shuttle";
    public const string InternalTransfer = "internal_transfer";
}
