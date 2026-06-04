namespace RoutArr.Api.Contracts;

public sealed record RouteReportCountItem(string Key, int Count);

public sealed record RouteReportRouteSummaryItem(
    Guid RouteId,
    string RouteNumber,
    string Title,
    string RouteStatus,
    Guid? TripId,
    string? TripNumber,
    int TotalStopCount,
    int PendingStopCount,
    int ArrivedStopCount,
    int CompletedStopCount,
    int SkippedStopCount,
    int CompletionPercent,
    int WaitStopCount,
    int DetentionStopCount,
    int TotalWaitMinutes,
    int TotalDetentionMinutes);

public sealed record RouteReportStopRow(
    Guid StopId,
    Guid RouteId,
    string RouteNumber,
    string StopKey,
    string Label,
    string StopType,
    string StopStatus,
    int SequenceNumber,
    DateTimeOffset? ScheduledArrivalAt,
    int WaitMinutes,
    int DetentionMinutes,
    DateTimeOffset UpdatedAt);

public sealed record RouteReportSummaryResponse(
    DateTimeOffset GeneratedAt,
    string Scope,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    int TotalRouteCount,
    int TotalStopCount,
    int PendingStopCount,
    int ArrivedStopCount,
    int CompletedStopCount,
    int SkippedStopCount,
    int WaitStopCount,
    int DetentionStopCount,
    int TotalWaitMinutes,
    int TotalDetentionMinutes,
    IReadOnlyList<RouteReportCountItem> RouteStatusCounts,
    IReadOnlyList<RouteReportCountItem> StopStatusCounts,
    IReadOnlyList<RouteReportCountItem> StopTypeCounts,
    IReadOnlyList<RouteReportRouteSummaryItem> Routes,
    IReadOnlyList<RouteReportStopRow> RecentStops);

public sealed record RouteReportStopSummaryRow(
    Guid StopId,
    string StopKey,
    string Label,
    string AddressLabel,
    string StopType,
    string StopStatus,
    int SequenceNumber,
    DateTimeOffset? ScheduledArrivalAt,
    DateTimeOffset? ArrivedAt,
    DateTimeOffset? CompletedAt,
    int WaitMinutes,
    int DetentionMinutes,
    DateTimeOffset UpdatedAt);

public sealed record RouteReportRouteDetailResponse(
    Guid RouteId,
    string RouteNumber,
    string Title,
    string Description,
    string RouteStatus,
    Guid? TripId,
    string? TripNumber,
    string? TripTitle,
    int TotalStopCount,
    int PendingStopCount,
    int CompletedStopCount,
    int SkippedStopCount,
    int CompletionPercent,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ActivatedAt,
    DateTimeOffset? CompletedAt,
    IReadOnlyList<RouteReportStopSummaryRow> Stops,
    IReadOnlyList<RouteReportAuditHistoryItem> History);

public sealed record RouteReportAuditHistoryItem(
    DateTimeOffset OccurredAt,
    string Action,
    string Result,
    string? ReasonCode,
    Guid? ActorUserId);

public sealed record RouteReportStopDetailResponse(
    Guid StopId,
    Guid RouteId,
    string RouteNumber,
    string RouteTitle,
    Guid? TripId,
    string? TripNumber,
    string StopKey,
    string Label,
    string AddressLabel,
    string StopType,
    string StopStatus,
    int SequenceNumber,
    DateTimeOffset? ScheduledArrivalAt,
    DateTimeOffset? ArrivedAt,
    DateTimeOffset? CompletedAt,
    int WaitMinutes,
    int DetentionMinutes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
