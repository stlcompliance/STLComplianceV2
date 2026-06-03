namespace RoutArr.Api.Contracts;

public sealed record DriverTimeTrackingSummaryResponse(
    int EntryCount,
    int OnDutyMinutes,
    int OffDutyMinutes,
    int BreakMinutes,
    int OpenEntryCount,
    DateTimeOffset? WorkdayStartAt,
    DateTimeOffset? WorkdayEndAt,
    bool ShortHaulCandidate,
    bool ShortHaulException,
    string SummaryNote);

public sealed record DriverTimeEntryResponse(
    Guid EntryId,
    string PersonId,
    string EntryType,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt,
    string Notes,
    string EditReason,
    bool IsOpen,
    int DurationMinutes,
    Guid CreatedByUserId,
    Guid? UpdatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record DriverTimeTrackingResponse(
    string Date,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    DriverTimeTrackingSummaryResponse Summary,
    IReadOnlyList<DriverTimeEntryResponse> Entries,
    DateTimeOffset GeneratedAt);

public sealed record CreateDriverTimeEntryRequest(
    string EntryType,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt,
    string? Notes);

public sealed record UpdateDriverTimeEntryRequest(
    string? EntryType,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    string? Notes,
    string EditReason);
