namespace RoutArr.Api.Contracts;

public sealed record DispatchReportCountItem(string Key, int Count);

public sealed record DispatchReportTripSummaryItem(
    Guid TripId,
    string TripNumber,
    string Title,
    string DispatchStatus,
    string? AssignedDriverPersonId,
    string? VehicleRefKey,
    DateTimeOffset? ScheduledStartAt,
    DateTimeOffset? ScheduledEndAt,
    bool IsLate,
    bool IsAtRisk,
    bool IsUnassigned,
    int RouteCount,
    int MissingRequiredProofCount,
    int OpenExceptionCount,
    bool HasDispatchReleaseSnapshot,
    bool ReleaseHasMissingExternalData,
    bool ReleaseHasStaleExternalData);

public sealed record DispatchReportExceptionRow(
    Guid ExceptionId,
    string ExceptionKey,
    string Title,
    string Category,
    string Status,
    Guid? TripId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record DispatchReportSummaryResponse(
    DateTimeOffset GeneratedAt,
    string Scope,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    int TotalTripCount,
    int LateTripCount,
    int AtRiskTripCount,
    int UnassignedTripCount,
    int MissingProofTripCount,
    int OpenExceptionCount,
    int DelayExceptionCount,
    int TripsWithDispatchReleaseSnapshotCount,
    int ReleaseSnapshotsMissingExternalDataCount,
    int ReleaseSnapshotsStaleExternalDataCount,
    IReadOnlyList<DispatchReportCountItem> TripStatusCounts,
    IReadOnlyList<DispatchReportCountItem> ExceptionStatusCounts,
    IReadOnlyList<DispatchReportCountItem> ExceptionCategoryCounts,
    IReadOnlyList<DispatchReportTripSummaryItem> Trips,
    IReadOnlyList<DispatchReportExceptionRow> RecentExceptions);

public sealed record DispatchReportTripDetailResponse(
    Guid TripId,
    string TripNumber,
    string Title,
    string Description,
    string DispatchStatus,
    string? AssignedDriverPersonId,
    string? VehicleRefKey,
    DateTimeOffset? ScheduledStartAt,
    DateTimeOffset? ScheduledEndAt,
    DateTimeOffset? DispatchedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? CancelledAt,
    bool IsLate,
    bool IsAtRisk,
    int RouteCount,
    int PendingStopCount,
    int MissingRequiredProofCount,
    int LinkedExceptionCount,
    int DelayExceptionCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    TripDispatchReleaseSnapshotResponse? DispatchReleaseSnapshot);

public sealed record DispatchReportExceptionDetailResponse(
    Guid ExceptionId,
    string ExceptionKey,
    string Title,
    string Description,
    string Category,
    string Status,
    Guid? TripId,
    string? TripNumber,
    string? TripTitle,
    Guid? AssignedToUserId,
    string ResolutionNotes,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? AssignedAt,
    Guid? ResolvedByUserId,
    DateTimeOffset? ResolvedAt);

public sealed record DispatchReportAlertResponse(
    string AlertType,
    string Severity,
    Guid? TripId,
    string? TripNumber,
    Guid? ExceptionId,
    string Message,
    DateTimeOffset DetectedAt);

public sealed record DispatchTimeSummaryTripRow(
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
    DateTimeOffset? ClosedAt,
    int? ScheduledDurationMinutes,
    int? ActualDurationMinutes,
    int? VarianceMinutes,
    bool IsCompleted,
    bool IsClosed);

public sealed record DispatchTimeSummaryResponse(
    DateTimeOffset GeneratedAt,
    string Scope,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    int TripCount,
    int CompletedTripCount,
    int ClosedTripCount,
    int TotalScheduledMinutes,
    int TotalActualMinutes,
    int TotalVarianceMinutes,
    IReadOnlyList<DispatchTimeSummaryTripRow> Trips);
