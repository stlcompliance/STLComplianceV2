namespace RoutArr.Api.Contracts;

public sealed record ActiveTripsSummary(
    int TotalCount,
    int LateCount,
    int AtRiskCount,
    int DispatchedCount,
    int InProgressCount,
    int UnassignedCount,
    int OpenExceptionCount);

public sealed record ActiveTripRow(
    Guid TripId,
    string TripNumber,
    string Title,
    string DispatchStatus,
    string? AssignedDriverPersonId,
    string? AssignedDriverDisplayName,
    string? VehicleRefKey,
    DateTimeOffset? ScheduledStartAt,
    DateTimeOffset? ScheduledEndAt,
    DateTimeOffset? DispatchedAt,
    DateTimeOffset? StartedAt,
    bool IsLate,
    bool IsAtRisk,
    int RouteCount,
    int PendingStopCount,
    int CompletedStopCount,
    int TotalStopCount,
    int StopProgressPercent,
    int OpenExceptionCount,
    double TimelineOffsetPercent,
    double TimelineWidthPercent);

public sealed record ActiveTripsResponse(
    string Scope,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    ActiveTripsSummary Summary,
    IReadOnlyList<ActiveTripRow> Items,
    DateTimeOffset GeneratedAt);
