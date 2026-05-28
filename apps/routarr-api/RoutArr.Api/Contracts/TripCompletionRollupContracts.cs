namespace RoutArr.Api.Contracts;

public sealed record TripCompletionSummaryResponse(
    Guid TripId,
    string TripNumber,
    string Title,
    string DispatchStatus,
    string? AssignedDriverPersonId,
    string? VehicleRefKey,
    DateTimeOffset? ScheduledStartAt,
    DateTimeOffset? ScheduledEndAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? CancelledAt,
    int? DurationMinutes,
    int RouteCount,
    int CompletedRouteCount,
    int StopCount,
    int CompletedStopCount,
    int SkippedStopCount,
    int PendingStopCount,
    int LoadCount,
    int DeliveredLoadCount,
    int PendingLoadCount,
    DateTimeOffset SourceUpdatedAt,
    DateTimeOffset ComputedAt,
    bool IsMaterialized);

public sealed record TripCompletionEventResponse(
    string EventKind,
    string Title,
    string? Detail,
    DateTimeOffset OccurredAt,
    int SequenceNumber,
    string SourceEntityType,
    string SourceEntityId);

public sealed record TripCompletionDetailResponse(
    TripCompletionSummaryResponse Summary,
    IReadOnlyList<TripCompletionEventResponse> Events);

public sealed record RouteCompletionSummaryResponse(
    Guid RouteId,
    string RouteNumber,
    string Title,
    string RouteStatus,
    Guid? TripId,
    string? TripNumber,
    string? TripDispatchStatus,
    int StopCount,
    int CompletedStopCount,
    int SkippedStopCount,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? ComputedAt,
    bool IsMaterialized);

public sealed record TripCompletionRollupSettingsResponse(
    bool IsEnabled,
    int StalenessHours,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertTripCompletionRollupSettingsRequest(
    bool IsEnabled,
    int StalenessHours);

public sealed record PendingTripCompletionRollupItem(
    Guid TripId,
    string TripNumber,
    string Title,
    string DispatchStatus,
    DateTimeOffset TripUpdatedAt,
    DateTimeOffset? LastComputedAt);

public sealed record PendingTripCompletionRollupsResponse(
    DateTimeOffset AsOfUtc,
    int StalenessHours,
    int BatchSize,
    IReadOnlyList<PendingTripCompletionRollupItem> Items);

public sealed record TripCompletionRollupRunItem(
    Guid RunId,
    DateTimeOffset AsOfUtc,
    int CandidatesFound,
    int RefreshedCount,
    int SkippedCount,
    DateTimeOffset CreatedAt);

public sealed record TripCompletionRollupRunsResponse(
    IReadOnlyList<TripCompletionRollupRunItem> Items);

public sealed record ProcessTripCompletionRollupsRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize,
    int? StalenessHours);

public sealed record TripCompletionRollupRefreshSkip(
    Guid TripId,
    string Reason);

public sealed record ProcessTripCompletionRollupsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int StalenessHours,
    int CandidatesFound,
    int RefreshedCount,
    int SkippedCount,
    IReadOnlyList<TripCompletionSummaryResponse> Refreshed,
    IReadOnlyList<TripCompletionRollupRefreshSkip> Skipped);
