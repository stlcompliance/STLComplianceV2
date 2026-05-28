namespace RoutArr.Api.Contracts;



public sealed record DriverAvailabilitySummaryResponse(

    Guid AvailabilityId,

    string PersonId,

    string AvailabilityStatus,

    DateTimeOffset StartsAt,

    DateTimeOffset EndsAt,

    string Reason,

    string Notes,

    bool HasConflict,

    int ConflictingTripCount,

    Guid CreatedByUserId,

    DateTimeOffset CreatedAt,

    DateTimeOffset UpdatedAt);



public sealed record DriverAvailabilityDetailResponse(

    Guid AvailabilityId,

    string PersonId,

    string AvailabilityStatus,

    DateTimeOffset StartsAt,

    DateTimeOffset EndsAt,

    string Reason,

    string Notes,

    bool HasConflict,

    IReadOnlyList<DriverAvailabilityTripConflictResponse> ConflictingTrips,

    Guid CreatedByUserId,

    DateTimeOffset CreatedAt,

    DateTimeOffset UpdatedAt);



public sealed record DriverAvailabilityTripConflictResponse(

    Guid TripId,

    string TripNumber,

    string Title,

    string DispatchStatus,

    DateTimeOffset? ScheduledStartAt,

    DateTimeOffset? ScheduledEndAt);



public sealed record CreateDriverAvailabilityRequest(

    string PersonId,

    string AvailabilityStatus,

    DateTimeOffset StartsAt,

    DateTimeOffset EndsAt,

    string? Reason,

    string? Notes);



public sealed record UpdateDriverAvailabilityRequest(

    string? AvailabilityStatus,

    DateTimeOffset? StartsAt,

    DateTimeOffset? EndsAt,

    string? Reason,

    string? Notes);



public sealed record DriverAvailabilityPanelSummary(

    int RecordCount,

    int UnavailableCount,

    int LimitedCount,

    int AvailableCount,

    int ConflictCount);



public sealed record DriverAvailabilityPanelRow(

    Guid AvailabilityId,

    string PersonId,

    string AvailabilityStatus,

    DateTimeOffset StartsAt,

    DateTimeOffset EndsAt,

    string Reason,

    bool HasConflict,

    int ConflictingTripCount,

    IReadOnlyList<DriverAvailabilityTripConflictResponse> ConflictingTrips);



public sealed record DriverAvailabilityPanelResponse(

    string Scope,

    DateTimeOffset WindowStart,

    DateTimeOffset WindowEnd,

    DriverAvailabilityPanelSummary Summary,

    IReadOnlyList<DriverAvailabilityPanelRow> Records,

    DateTimeOffset GeneratedAt);

