namespace RoutArr.Api.Contracts;

public sealed record DispatchAssignmentAvailabilityConflict(
    Guid AvailabilityId,
    string AvailabilityStatus,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string Reason);

public sealed record DispatchAssignmentTripConflict(
    Guid TripId,
    string TripNumber,
    string Title,
    string DispatchStatus,
    DateTimeOffset? ScheduledStartAt,
    DateTimeOffset? ScheduledEndAt);

public sealed record DispatchAssignmentPreviewRequest(
    Guid TripId,
    string AssignmentKind,
    string? DriverPersonId,
    string? VehicleRefKey);

public sealed record DispatchAssignmentPreviewResponse(
    Guid TripId,
    string AssignmentKind,
    bool CanAssign,
    bool HasBlockingConflicts,
    IReadOnlyList<DispatchAssignmentAvailabilityConflict> BlockingDriverAvailability,
    IReadOnlyList<DispatchAssignmentAvailabilityConflict> BlockingEquipmentAvailability,
    IReadOnlyList<DispatchAssignmentTripConflict> OverlappingTrips,
    DispatchAssignmentEligibilitySummary? DriverEligibility = null,
    DispatchAssignmentDispatchabilitySummary? AssetDispatchability = null,
    DispatchAssignmentWorkflowGateSummary? WorkflowGates = null);
