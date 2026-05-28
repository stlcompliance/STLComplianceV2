namespace RoutArr.Api.Contracts;

public sealed record DispatchBoardResponse(
    string Scope,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    DispatchBoardTripsSummary Trips,
    DispatchBoardRoutesSummary Routes,
    DispatchBoardStopsSummary Stops,
    DispatchBoardWorkQueueSummary WorkQueue,
    IReadOnlyList<DispatchBoardTripRow> AssignedTrips,
    IReadOnlyList<DispatchBoardTripRow> ActiveTrips,
    DateTimeOffset GeneratedAt);

public sealed record DispatchBoardTripsSummary(
    int PlannedCount,
    int AssignedCount,
    int DispatchedCount,
    int InProgressCount,
    int CompletedCount,
    int CancelledCount,
    int TotalCount,
    int LateCount,
    int AtRiskCount);

public sealed record DispatchBoardRoutesSummary(
    int DraftCount,
    int PlannedCount,
    int ActiveCount,
    int CompletedCount,
    int CancelledCount,
    int TotalCount);

public sealed record DispatchBoardStopsSummary(
    int PendingCount,
    int ArrivedCount,
    int CompletedCount,
    int SkippedCount,
    int TotalCount);

public sealed record DispatchBoardWorkQueueSummary(
    int UnassignedDriverTripCount,
    int UnlinkedRouteCount,
    int PendingStopCount);

public sealed record DispatchBoardTripRow(
    Guid TripId,
    string TripNumber,
    string Title,
    string DispatchStatus,
    string? AssignedDriverPersonId,
    string? VehicleRefKey,
    DateTimeOffset? ScheduledStartAt,
    DateTimeOffset? ScheduledEndAt,
    bool IsLate,
    bool IsAtRisk,
    int RouteCount,
    int PendingStopCount);
