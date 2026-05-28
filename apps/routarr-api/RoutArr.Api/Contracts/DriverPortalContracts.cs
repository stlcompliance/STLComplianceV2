namespace RoutArr.Api.Contracts;

public sealed record DriverPortalTripRow(
    Guid TripId,
    string TripNumber,
    string Title,
    string DispatchStatus,
    string? VehicleRefKey,
    DateTimeOffset? ScheduledStartAt,
    DateTimeOffset? ScheduledEndAt,
    DateTimeOffset? DispatchedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    bool CanDispatch,
    bool CanStart,
    bool CanComplete,
    bool CanClose,
    int ProofCount,
    bool HasPreTripDvir,
    bool HasPostTripDvir);

public sealed record DriverPortalScheduleResponse(
    DateTimeOffset TodayStart,
    DateTimeOffset TodayEnd,
    DateTimeOffset UpcomingEnd,
    IReadOnlyList<DriverPortalTripRow> TodayTrips,
    IReadOnlyList<DriverPortalTripRow> UpcomingTrips,
    DateTimeOffset GeneratedAt);
