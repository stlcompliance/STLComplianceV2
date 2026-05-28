namespace RoutArr.Api.Contracts;

public sealed record DispatchCloseoutRequest(
    string? Scope,
    string RemainingTripDisposition,
    string OpenStopDisposition,
    IReadOnlyList<Guid>? TripIds = null);

public sealed record DispatchCloseoutSummaryResponse(
    string Scope,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    DispatchCloseoutCountsSummary Counts,
    DispatchCloseoutTripsSummary Trips,
    DispatchCloseoutRoutesSummary Routes,
    DispatchCloseoutStopsSummary Stops,
    IReadOnlyList<DispatchCloseoutTripRow> OpenTrips,
    IReadOnlyList<DispatchCloseoutRouteRow> OpenRoutes);

public sealed record DispatchCloseoutCountsSummary(
    int OpenTrips,
    int OpenRoutes,
    int OpenStops,
    int TotalInScopeTrips,
    int TotalInScopeRoutes);

public sealed record DispatchCloseoutTripsSummary(
    int Planned,
    int Assigned,
    int Dispatched,
    int InProgress,
    int Completed,
    int Cancelled);

public sealed record DispatchCloseoutRoutesSummary(
    int Draft,
    int Planned,
    int Active,
    int Completed,
    int Cancelled);

public sealed record DispatchCloseoutStopsSummary(
    int Pending,
    int Arrived,
    int Completed,
    int Skipped);

public sealed record DispatchCloseoutTripRow(
    Guid TripId,
    string TripNumber,
    string Title,
    string DispatchStatus,
    string? AssignedDriverPersonId);

public sealed record DispatchCloseoutRouteRow(
    Guid RouteId,
    string RouteNumber,
    string Title,
    string RouteStatus,
    Guid? TripId,
    int OpenStopCount);

public sealed record DispatchCloseoutPreviewResponse(
    string Scope,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    string RemainingTripDisposition,
    string OpenStopDisposition,
    DispatchCloseoutApplySummary Summary,
    IReadOnlyList<DispatchCloseoutTripActionPreview> TripActions,
    IReadOnlyList<DispatchCloseoutStopActionPreview> StopActions,
    IReadOnlyList<DispatchCloseoutRouteActionPreview> RouteActions);

public sealed record DispatchCloseoutApplySummary(
    int TripCount,
    int TripsCanApply,
    int TripsBlocked,
    int StopCount,
    int StopsCanApply,
    int StopsBlocked,
    int RouteCount,
    int RoutesCanApply,
    int RoutesBlocked);

public sealed record DispatchCloseoutTripActionPreview(
    Guid TripId,
    string TripNumber,
    string CurrentDispatchStatus,
    string TargetDispatchStatus,
    bool CanApply,
    string? BlockCode,
    string? BlockMessage,
    IReadOnlyList<string> TransitionSteps);

public sealed record DispatchCloseoutStopActionPreview(
    Guid StopId,
    Guid RouteId,
    string StopKey,
    string CurrentStopStatus,
    string TargetStopStatus,
    bool CanApply,
    string? BlockCode,
    string? BlockMessage);

public sealed record DispatchCloseoutRouteActionPreview(
    Guid RouteId,
    string RouteNumber,
    string CurrentRouteStatus,
    string TargetRouteStatus,
    bool CanApply,
    string? BlockCode,
    string? BlockMessage);

public sealed record DispatchCloseoutApplyResponse(
    string Scope,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    DispatchCloseoutApplySummary Summary,
    IReadOnlyList<DispatchCloseoutTripApplyResult> TripResults,
    IReadOnlyList<DispatchCloseoutStopApplyResult> StopResults,
    IReadOnlyList<DispatchCloseoutRouteApplyResult> RouteResults);

public sealed record DispatchCloseoutTripApplyResult(
    Guid TripId,
    bool Applied,
    string? FinalDispatchStatus,
    string? ErrorCode,
    string? ErrorMessage);

public sealed record DispatchCloseoutStopApplyResult(
    Guid StopId,
    bool Applied,
    string? FinalStopStatus,
    string? ErrorCode,
    string? ErrorMessage);

public sealed record DispatchCloseoutRouteApplyResult(
    Guid RouteId,
    bool Applied,
    string? FinalRouteStatus,
    string? ErrorCode,
    string? ErrorMessage);

public sealed record DispatchCloseoutChecklistItem(
    string Key,
    string Label,
    bool Satisfied,
    bool Required,
    string? Detail);

public sealed record DispatchCloseoutTripChecklistResponse(
    Guid TripId,
    string TripNumber,
    string DispatchStatus,
    bool ReadyForCloseout,
    IReadOnlyList<DispatchCloseoutChecklistItem> Items);

public sealed record DispatchCloseoutChecklistsResponse(
    string Scope,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    string RemainingTripDisposition,
    IReadOnlyList<DispatchCloseoutTripChecklistResponse> Trips);

public sealed record DispatchCloseoutAuditEntry(
    Guid Id,
    Guid? ActorUserId,
    string Action,
    string TargetType,
    string? TargetId,
    string Result,
    DateTimeOffset OccurredAt);

public sealed record DispatchCloseoutAuditListResponse(
    IReadOnlyList<DispatchCloseoutAuditEntry> Entries);
