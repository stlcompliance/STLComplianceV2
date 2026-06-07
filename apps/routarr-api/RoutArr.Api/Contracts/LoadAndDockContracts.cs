namespace RoutArr.Api.Contracts;

public sealed record TransportationLoadVisibilityResponse(
    Guid TransportationLoadVisibilityId,
    string LoadNumber,
    Guid? TripId,
    Guid? RouteId,
    string SourceProduct,
    string? SourceObjectRef,
    string LoadType,
    string Status,
    string? OriginLocationRef,
    string? DestinationLocationRef,
    string? CustomerRef,
    string? SupplierRef,
    IReadOnlyList<string> OrderRefs,
    IReadOnlyList<string> ExpectedReceiptRefs,
    string ItemSummarySnapshot,
    IReadOnlyList<string> HandlingRequirements,
    string? TemperatureRequirement,
    bool HazmatFlag,
    decimal? WeightSnapshot,
    decimal? VolumeSnapshot,
    string? SealNumber,
    IReadOnlyList<string> DocumentRefs,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record DockAppointmentNotificationResponse(
    Guid DockAppointmentNotificationId,
    string NotificationNumber,
    Guid? SourceTripId,
    Guid? SourceRouteId,
    Guid? SourceStopId,
    string AppointmentType,
    DateTimeOffset? RequestedWindowStart,
    DateTimeOffset? RequestedWindowEnd,
    DateTimeOffset? ConfirmedWindowStart,
    DateTimeOffset? ConfirmedWindowEnd,
    DateTimeOffset? Eta,
    string Status,
    string? CarrierNameSnapshot,
    string? DriverSnapshot,
    string? VehicleSnapshot,
    string? TrailerSnapshot,
    string SourceProduct,
    string? SourceObjectRef,
    string? RejectionReason,
    DateTimeOffset? SentAt,
    DateTimeOffset? AcknowledgedAt,
    DateTimeOffset? ConfirmedAt,
    DateTimeOffset? CanceledAt);

public sealed record LoadReadinessCheckRequest(
    Guid TripId);

public sealed record LoadReadinessCheckResponse(
    Guid TripId,
    string Status,
    string ReadinessDetails,
    IReadOnlyList<string> BlockerRefs,
    DateTimeOffset CheckedAt);
