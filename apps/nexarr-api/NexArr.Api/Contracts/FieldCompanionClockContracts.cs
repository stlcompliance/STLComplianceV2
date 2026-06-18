namespace NexArr.Api.Contracts;

public sealed record FieldCompanionClockEventResponse(
    Guid Id,
    string EventType,
    DateTimeOffset EventTimestamp,
    DateTimeOffset CapturedTimestamp,
    string Timezone,
    string? SourceDeviceId,
    string? GeoPoint,
    string? SiteRef,
    string? LocationRef,
    string? Notes,
    IReadOnlyList<string> AnomalyFlags);

public sealed record FieldCompanionClockStatusResponse(
    string CurrentState,
    FieldCompanionClockEventResponse? LatestEvent,
    IReadOnlyList<FieldCompanionClockEventResponse> RecentEvents);

public sealed record SubmitFieldCompanionClockEventRequest(
    string EventType,
    DateTimeOffset EventTimestamp,
    DateTimeOffset? CapturedAt,
    string Timezone,
    string IdempotencyKey,
    string? SourceDeviceId,
    string? GeoPoint,
    string? SiteRef,
    string? LocationRef,
    string? Notes);

public sealed record FieldCompanionClockSubmissionResponse(
    Guid ClockEventId,
    bool Created,
    bool ConflictDetected,
    string Status,
    string CurrentState,
    FieldCompanionClockEventResponse Event);

public sealed record StaffArrFieldCompanionClockStatusUpstreamResponse(
    string CurrentState,
    StaffArrFieldCompanionClockEventUpstreamResponse? LatestEvent,
    IReadOnlyList<StaffArrFieldCompanionClockEventUpstreamResponse> RecentEvents);

public sealed record StaffArrFieldCompanionClockEventUpstreamResponse(
    Guid Id,
    string EventType,
    DateTimeOffset EventTimestamp,
    DateTimeOffset CapturedTimestamp,
    string Timezone,
    string? SourceDeviceId,
    string? GeoPoint,
    string? SiteRef,
    string? LocationRef,
    string? Notes,
    IReadOnlyList<string> AnomalyFlags);

public sealed record StaffArrSubmitFieldCompanionClockEventUpstreamRequest(
    string EventType,
    DateTimeOffset EventTimestamp,
    DateTimeOffset? CapturedAt,
    string Timezone,
    string IdempotencyKey,
    string? SourceDeviceId,
    string? GeoPoint,
    string? SiteRef,
    string? LocationRef,
    string? Notes);

public sealed record StaffArrFieldCompanionClockSubmissionUpstreamResponse(
    Guid ClockEventId,
    bool Created,
    bool ConflictDetected,
    string Status,
    string CurrentState,
    StaffArrFieldCompanionClockEventUpstreamResponse Event);
