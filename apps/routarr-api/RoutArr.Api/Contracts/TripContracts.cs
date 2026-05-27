namespace RoutArr.Api.Contracts;

public sealed record TripLoadSummaryResponse(
    Guid LoadId,
    string LoadKey,
    string Description,
    string LoadType,
    string Status,
    int SequenceNumber,
    string OriginLabel,
    string DestinationLabel,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record TripSummaryResponse(
    Guid TripId,
    string TripNumber,
    string Title,
    string DispatchStatus,
    string? AssignedDriverPersonId,
    string? VehicleRefKey,
    DateTimeOffset? ScheduledStartAt,
    DateTimeOffset? ScheduledEndAt,
    int LoadCount,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? AssignedAt,
    DateTimeOffset? DispatchedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? CancelledAt);

public sealed record TripDetailResponse(
    Guid TripId,
    string TripNumber,
    string Title,
    string Description,
    string DispatchStatus,
    string? AssignedDriverPersonId,
    string? VehicleRefKey,
    DateTimeOffset? ScheduledStartAt,
    DateTimeOffset? ScheduledEndAt,
    IReadOnlyList<TripLoadSummaryResponse> Loads,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? AssignedAt,
    DateTimeOffset? DispatchedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? CancelledAt);

public sealed record CreateTripLoadRequest(
    string LoadKey,
    string Description,
    string LoadType,
    int SequenceNumber,
    string OriginLabel,
    string DestinationLabel);

public sealed record CreateTripRequest(
    string Title,
    string Description,
    string? VehicleRefKey,
    DateTimeOffset? ScheduledStartAt,
    DateTimeOffset? ScheduledEndAt,
    IReadOnlyList<CreateTripLoadRequest>? Loads);

public sealed record AssignTripDriverRequest(
    string DriverPersonId,
    bool IgnoreAvailabilityConflicts = false,
    bool IgnoreEligibilityBlocks = false,
    bool IgnoreWorkflowGateBlocks = false);

public sealed record AssignTripVehicleRequest(
    string? VehicleRefKey,
    bool IgnoreAvailabilityConflicts = false,
    bool IgnoreDispatchabilityBlocks = false,
    bool IgnoreWorkflowGateBlocks = false);

public sealed record UpdateTripDispatchStatusRequest(string DispatchStatus);
