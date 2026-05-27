namespace RoutArr.Api.Contracts;

public sealed record RouteCalendarResponse(
    string Scope,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    IReadOnlyList<RouteCalendarDay> Days,
    RouteCalendarSummary Summary,
    DateTimeOffset GeneratedAt);

public sealed record RouteCalendarDay(
    DateTimeOffset Date,
    IReadOnlyList<RouteCalendarEvent> Events);

public sealed record RouteCalendarEvent(
    string EventType,
    Guid EntityId,
    string Label,
    string Status,
    DateTimeOffset ScheduledAt,
    DateTimeOffset? ScheduledEndAt,
    Guid? TripId,
    Guid? RouteId,
    string? TripNumber,
    string? RouteNumber,
    string? AssignedDriverPersonId,
    bool IsLate,
    bool IsAtRisk);

public sealed record RouteCalendarSummary(
    int TripCount,
    int RouteCount,
    int StopCount,
    int LateTripCount,
    int AtRiskTripCount);
