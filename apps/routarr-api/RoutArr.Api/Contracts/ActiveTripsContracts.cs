namespace RoutArr.Api.Contracts;

public sealed record ActiveTripsSummary(
    int TotalCount,
    int LateCount,
    int AtRiskCount,
    int DispatchedCount,
    int InProgressCount);

public sealed record ActiveTripRow(
    Guid TripId,
    string TripNumber,
    string Title,
    string DispatchStatus,
    string? AssignedDriverPersonId,
    string? VehicleRefKey,
    DateTimeOffset? ScheduledStartAt,
    DateTimeOffset? ScheduledEndAt,
    DateTimeOffset? DispatchedAt,
    DateTimeOffset? StartedAt,
    bool IsLate,
    bool IsAtRisk,
    int RouteCount,
    int PendingStopCount,
    double TimelineOffsetPercent,
    double TimelineWidthPercent);

public sealed record ActiveTripsResponse(
    string Scope,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    ActiveTripsSummary Summary,
    IReadOnlyList<ActiveTripRow> Items,
    DateTimeOffset GeneratedAt);
