namespace RoutArr.Api.Contracts;

public sealed record UnassignedWorkQueueTripRow(
    Guid TripId,
    string TripNumber,
    string Title,
    string DispatchStatus,
    DateTimeOffset? ScheduledStartAt,
    DateTimeOffset? ScheduledEndAt,
    bool IsLate,
    bool IsAtRisk,
    int RouteCount,
    int PendingStopCount);

public sealed record UnassignedWorkQueueResponse(
    string Scope,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    int UnassignedCount,
    IReadOnlyList<UnassignedWorkQueueTripRow> Items,
    StaffarrPersonRefListResponse DriverRefs,
    DateTimeOffset GeneratedAt);
