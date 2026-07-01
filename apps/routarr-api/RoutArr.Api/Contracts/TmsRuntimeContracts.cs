namespace RoutArr.Api.Contracts;

public sealed record TransportationDemandLineRequest(
    string? SourceProduct,
    string? SourceObjectRef,
    string DescriptionSnapshot,
    decimal? QuantitySnapshot,
    string? UnitOfMeasure,
    decimal? WeightSnapshot,
    decimal? VolumeSnapshot,
    int? PalletCountSnapshot,
    string? HandlingRequirementSnapshot);

public sealed record TransportationDemandRequirementRequest(
    string RequirementType,
    string? SourceProduct,
    string? SourceRequirementRef,
    bool Required,
    string? Status,
    IReadOnlyList<string>? EvidenceRefs);

public sealed record TransportationDemandSourceRefRequest(
    string SourceProduct,
    string SourceObjectType,
    string SourceObjectId,
    string? SourceObjectNumber,
    string DisplayNameSnapshot,
    string? StatusSnapshot,
    string? FreshnessState);

public sealed record CreateTransportationDemandRequest(
    string Title,
    string? Description,
    string? Status,
    string? SourceProduct,
    string? SourceObjectType,
    string? SourceObjectId,
    string? SourceObjectNumber,
    string OriginLocationRef,
    string DestinationLocationRef,
    DateTimeOffset? RequestedPickupStartAt,
    DateTimeOffset? RequestedPickupEndAt,
    DateTimeOffset? RequestedDeliveryStartAt,
    DateTimeOffset? RequestedDeliveryEndAt,
    DateTimeOffset? PromisedPickupStartAt,
    DateTimeOffset? PromisedPickupEndAt,
    DateTimeOffset? PromisedDeliveryStartAt,
    DateTimeOffset? PromisedDeliveryEndAt,
    DateTimeOffset? ScheduledPickupStartAt,
    DateTimeOffset? ScheduledPickupEndAt,
    DateTimeOffset? ScheduledDeliveryStartAt,
    DateTimeOffset? ScheduledDeliveryEndAt,
    string? TransportMode,
    string? ServiceLevel,
    string? EquipmentRequirement,
    IReadOnlyList<string>? HandlingRequirements,
    IReadOnlyList<string>? CustomerRefs,
    IReadOnlyList<string>? OrderRefs,
    IReadOnlyList<string>? SupplierRefs,
    IReadOnlyList<string>? RequirementRefs,
    IReadOnlyList<TransportationDemandLineRequest>? Lines,
    IReadOnlyList<TransportationDemandRequirementRequest>? Requirements,
    IReadOnlyList<TransportationDemandSourceRefRequest>? SourceRefs);

public sealed record UpdateTransportationDemandStatusRequest(
    string Status,
    string? Reason = null);

public sealed record LinkTransportationDemandTripRequest(
    Guid? TripId,
    Guid? RouteId,
    Guid? DispatchPlanId);

public sealed record TransportationDemandLineResponse(
    Guid DemandLineId,
    int LineNumber,
    string SourceProduct,
    string? SourceObjectRef,
    string DescriptionSnapshot,
    decimal? QuantitySnapshot,
    string UnitOfMeasure,
    decimal? WeightSnapshot,
    decimal? VolumeSnapshot,
    int? PalletCountSnapshot,
    string HandlingRequirementSnapshot);

public sealed record TransportationDemandRequirementResponse(
    Guid RequirementId,
    string RequirementType,
    string SourceProduct,
    string? SourceRequirementRef,
    bool Required,
    string Status,
    IReadOnlyList<string> EvidenceRefs);

public sealed record TransportationDemandSourceRefResponse(
    Guid SourceRefId,
    string SourceProduct,
    string SourceObjectType,
    string SourceObjectId,
    string? SourceObjectNumber,
    string DisplayNameSnapshot,
    string StatusSnapshot,
    DateTimeOffset SnapshotAt,
    string FreshnessState);

public sealed record TransportationDemandResponse(
    Guid TransportationDemandId,
    string DemandNumber,
    string Title,
    string Description,
    string Status,
    string SourceProduct,
    string SourceObjectType,
    string? SourceObjectId,
    string? SourceObjectNumber,
    string OriginLocationRef,
    string DestinationLocationRef,
    DateTimeOffset? RequestedPickupStartAt,
    DateTimeOffset? RequestedPickupEndAt,
    DateTimeOffset? RequestedDeliveryStartAt,
    DateTimeOffset? RequestedDeliveryEndAt,
    DateTimeOffset? PromisedPickupStartAt,
    DateTimeOffset? PromisedPickupEndAt,
    DateTimeOffset? PromisedDeliveryStartAt,
    DateTimeOffset? PromisedDeliveryEndAt,
    DateTimeOffset? ScheduledPickupStartAt,
    DateTimeOffset? ScheduledPickupEndAt,
    DateTimeOffset? ScheduledDeliveryStartAt,
    DateTimeOffset? ScheduledDeliveryEndAt,
    string TransportMode,
    string ServiceLevel,
    string EquipmentRequirement,
    IReadOnlyList<string> HandlingRequirements,
    IReadOnlyList<string> CustomerRefs,
    IReadOnlyList<string> OrderRefs,
    IReadOnlyList<string> SupplierRefs,
    IReadOnlyList<string> RequirementRefs,
    string PlanningStatus,
    string TenderStatus,
    string RatingStatus,
    string VisibilityStatus,
    string FreshnessState,
    Guid? TripId,
    Guid? RouteId,
    Guid? DispatchPlanId,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? CanceledAt,
    string? CancelReason,
    IReadOnlyList<TransportationDemandLineResponse> Lines,
    IReadOnlyList<TransportationDemandRequirementResponse> Requirements,
    IReadOnlyList<TransportationDemandSourceRefResponse> SourceRefs);

public sealed record CreateCarrierTenderRequest(
    Guid TransportationDemandId,
    int RoutingGuideSequence,
    string CarrierSupplierRef,
    string? CarrierSnapshotJson,
    string? TenderMethod,
    DateTimeOffset? ExpiresAt);

public sealed record UpdateTenderStatusRequest(
    string Status,
    string? DeclineReason,
    string? CounterSummary,
    string? ProposedAlternative);

public sealed record CarrierTenderResponse(
    Guid TenderId,
    Guid TransportationDemandId,
    string TenderNumber,
    string Status,
    int RoutingGuideSequence,
    string CarrierSupplierRef,
    string CarrierSnapshotJson,
    string TenderMethod,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? SentAt,
    DateTimeOffset? RespondedAt,
    string? DeclineReason,
    string? CounterSummary,
    string? ProposedAlternative,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateFreightRatingRequest(
    Guid TransportationDemandId,
    Guid? TripId,
    decimal? BuyRateEstimate,
    decimal? SellRateEstimate,
    decimal? PlannedFreightCost,
    decimal? ActualFreightCost,
    string? CurrencyCode,
    string? RateSourceSnapshot,
    decimal? FuelSurcharge,
    string? AllocationSnapshotJson);

public sealed record CreateFreightAccessorialRequest(
    string AccessorialType,
    decimal Amount,
    string? CurrencyCode,
    string? Status,
    string? SourceEventRef,
    IReadOnlyList<string>? EvidenceRefs);

public sealed record FreightAccessorialResponse(
    Guid AccessorialId,
    Guid FreightRatingId,
    Guid TransportationDemandId,
    Guid? TripId,
    string AccessorialType,
    decimal Amount,
    string CurrencyCode,
    string Status,
    string? SourceEventRef,
    IReadOnlyList<string> EvidenceRefs,
    DateTimeOffset CreatedAt);

public sealed record FreightRatingResponse(
    Guid FreightRatingId,
    Guid TransportationDemandId,
    Guid? TripId,
    string RatingNumber,
    string Status,
    decimal? BuyRateEstimate,
    decimal? SellRateEstimate,
    decimal? PlannedFreightCost,
    decimal? ActualFreightCost,
    string CurrencyCode,
    string RateSourceSnapshot,
    decimal? FuelSurcharge,
    decimal? AccessorialTotal,
    decimal? VarianceAmount,
    string? VarianceReason,
    string AllocationSnapshotJson,
    string AuditStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateVisibilityEventRequest(
    Guid? TransportationDemandId,
    Guid? TripId,
    Guid? StopId,
    string EventType,
    string? Source,
    DateTimeOffset? SourceOccurredAt,
    string? NormalizedStatus,
    decimal? Latitude,
    decimal? Longitude,
    DateTimeOffset? Eta,
    string? EtaConfidence,
    string? FreshnessState,
    string? ReviewStatus,
    string? RawExternalRef,
    string? Summary);

public sealed record VisibilityEventResponse(
    Guid VisibilityEventId,
    Guid? TransportationDemandId,
    Guid? TripId,
    Guid? StopId,
    string EventType,
    string Source,
    DateTimeOffset SourceOccurredAt,
    DateTimeOffset ReceivedAt,
    string NormalizedStatus,
    decimal? Latitude,
    decimal? Longitude,
    DateTimeOffset? Eta,
    string EtaConfidence,
    string FreshnessState,
    string ReviewStatus,
    string? RawExternalRef,
    string Summary,
    bool UpdatedTrackingState);

public sealed record CreatePlanningScenarioRequest(
    IReadOnlyList<Guid> DemandRefs,
    IReadOnlyList<Guid>? RouteRefs,
    IReadOnlyList<Guid>? TripRefs,
    string? Objective);

public sealed record PlanningSuggestionResponse(
    Guid SuggestionId,
    Guid PlanningScenarioId,
    string SuggestionType,
    string Status,
    string Summary,
    string HardBlockersJson,
    string SoftWarningsJson,
    decimal? EstimatedCost,
    decimal? EstimatedMiles,
    decimal? EstimatedServiceRisk,
    string AffectedDemandRefsJson,
    DateTimeOffset CreatedAt);

public sealed record PlanningScenarioResponse(
    Guid PlanningScenarioId,
    string ScenarioNumber,
    string Status,
    string Objective,
    string DemandRefsJson,
    string RouteRefsJson,
    string TripRefsJson,
    string HardBlockersJson,
    string WarningsJson,
    decimal? ServiceRiskEstimate,
    decimal? CostEstimate,
    DateTimeOffset CreatedAt,
    DateTimeOffset? EvaluatedAt,
    IReadOnlyList<PlanningSuggestionResponse> Suggestions);

public sealed record CreateDriverCapacitySnapshotRequest(
    string PersonId,
    string? Source,
    DateTimeOffset? ShiftWindowStart,
    DateTimeOffset? ShiftWindowEnd,
    int? HosRemainingMinutes,
    int? DriveTimeRemainingMinutes,
    int? OnDutyRemainingMinutes,
    DateTimeOffset? BreakRequiredBy,
    string? DomicileLocationRef,
    string? FeasibilityStatus,
    string? BlockerSummary,
    string? FreshnessState);

public sealed record DriverCapacitySnapshotResponse(
    Guid DriverCapacitySnapshotId,
    string PersonId,
    string Source,
    DateTimeOffset? ShiftWindowStart,
    DateTimeOffset? ShiftWindowEnd,
    int? HosRemainingMinutes,
    int? DriveTimeRemainingMinutes,
    int? OnDutyRemainingMinutes,
    DateTimeOffset? BreakRequiredBy,
    string DomicileLocationRef,
    string FeasibilityStatus,
    string BlockerSummary,
    DateTimeOffset SnapshotAt,
    string FreshnessState);

public sealed record CreateYardEventRequest(
    Guid? TransportationDemandId,
    Guid? TripId,
    string EventType,
    string? TrailerAssetRef,
    string? TractorAssetRef,
    string? StaffarrYardLocationRef,
    string? StaffarrDockLocationRef,
    string? LoadedEmptyStatus,
    string? SealNumber,
    string? Source,
    DateTimeOffset? OccurredAt,
    IReadOnlyList<string>? EvidenceRefs,
    string? DispatchImpact);

public sealed record YardEventResponse(
    Guid YardEventId,
    Guid? TransportationDemandId,
    Guid? TripId,
    string EventType,
    string TrailerAssetRef,
    string TractorAssetRef,
    string StaffarrYardLocationRef,
    string StaffarrDockLocationRef,
    string LoadedEmptyStatus,
    string? SealNumber,
    string Source,
    DateTimeOffset OccurredAt,
    IReadOnlyList<string> EvidenceRefs,
    string DispatchImpact);

public sealed record CreateCollaborationSubmissionRequest(
    Guid? TransportationDemandId,
    Guid? TenderId,
    string? ExternalActorType,
    string ExternalActorRef,
    string ActionType,
    string SubmittedDataSummary,
    IReadOnlyList<string>? UploadedRecordRefs);

public sealed record ReviewCollaborationSubmissionRequest(string Status);

public sealed record CollaborationSubmissionResponse(
    Guid SubmissionId,
    Guid? TransportationDemandId,
    Guid? TenderId,
    string ExternalActorType,
    string ExternalActorRef,
    string ActionType,
    string Status,
    string SubmittedDataSummary,
    IReadOnlyList<string> UploadedRecordRefs,
    DateTimeOffset SubmittedAt,
    string? ReviewedByPersonId,
    DateTimeOffset? ReviewedAt);

public sealed record CreateFreightClaimRequest(
    Guid? TransportationDemandId,
    Guid? TripId,
    string? ClaimAgainstPartyType,
    string? ClaimReason,
    decimal? ClaimAmount,
    string? CurrencyCode,
    IReadOnlyList<string>? EvidenceRefs,
    string? AssurarrNonconformanceRef,
    string? SupplyarrPerformanceImpactRef,
    string? OrdarrCloseoutImpactRef);

public sealed record FreightClaimResponse(
    Guid FreightClaimId,
    string ClaimNumber,
    Guid? TransportationDemandId,
    Guid? TripId,
    string ClaimAgainstPartyType,
    string ClaimReason,
    decimal? ClaimAmount,
    decimal? RecoveryAmount,
    string CurrencyCode,
    string Status,
    IReadOnlyList<string> EvidenceRefs,
    string? AssurarrNonconformanceRef,
    string? SupplyarrPerformanceImpactRef,
    string? OrdarrCloseoutImpactRef,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateDocumentPacketRequest(
    Guid? TransportationDemandId,
    Guid? TripId,
    string PacketType,
    IReadOnlyList<string>? RequiredDocumentTypes,
    string? SourceFactsJson);

public sealed record DocumentPacketResponse(
    Guid DocumentPacketRequestId,
    Guid? TransportationDemandId,
    Guid? TripId,
    string PacketType,
    string Status,
    IReadOnlyList<string> RequiredDocumentTypes,
    string SourceFactsJson,
    string? RecordPackageRef,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateFinancePacketContributionRequest(
    Guid? TransportationDemandId,
    Guid? TripId,
    Guid? FreightRatingId,
    string ContributionType,
    string TargetProduct,
    string OperationalSummary,
    string? CostSnapshotJson,
    IReadOnlyList<string>? AccessorialRefs,
    IReadOnlyList<string>? ProofRefs,
    IReadOnlyList<string>? DocumentPacketRefs,
    IReadOnlyList<string>? ClaimRefs);

public sealed record FinancePacketContributionResponse(
    Guid FinancePacketContributionId,
    string ContributionNumber,
    Guid? TransportationDemandId,
    Guid? TripId,
    Guid? FreightRatingId,
    string ContributionType,
    string TargetProduct,
    string Status,
    string OperationalSummary,
    string CostSnapshotJson,
    IReadOnlyList<string> AccessorialRefs,
    IReadOnlyList<string> ProofRefs,
    IReadOnlyList<string> DocumentPacketRefs,
    IReadOnlyList<string> ClaimRefs,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? SentAt,
    DateTimeOffset? AcceptedAt);

public sealed record UpdateTmsRecordStatusRequest(string Status);
