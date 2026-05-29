namespace RoutArr.Api.Contracts;

public sealed record UnassignedWorkQueueSummary(
    int UnassignedCount,
    int LateCount,
    int AtRiskCount,
    int UrgentCount);

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
    int PendingStopCount,
    int MinutesUntilStart);

public sealed record UnassignedWorkQueueResponse(
    string Scope,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    UnassignedWorkQueueSummary Summary,
    IReadOnlyList<UnassignedWorkQueueTripRow> Items,
    StaffarrPersonRefListResponse DriverRefs,
    DateTimeOffset GeneratedAt);
