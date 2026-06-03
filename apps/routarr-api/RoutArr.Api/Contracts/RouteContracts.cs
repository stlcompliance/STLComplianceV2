namespace RoutArr.Api.Contracts;

public sealed record RouteStopSummaryResponse(
    Guid StopId,
    string StopKey,
    string Label,
    string AddressLabel,
    Guid? StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string StopType,
    string StopStatus,
    int SequenceNumber,
    decimal? GeofenceAnchorLatitude,
    decimal? GeofenceAnchorLongitude,
    int? GeofenceRadiusMeters,
    DateTimeOffset? LastGeofenceCheckAt,
    string? LastGeofenceResult,
    decimal? LastGeofenceDistanceMeters,
    decimal? LastGeofenceReportedLatitude,
    decimal? LastGeofenceReportedLongitude,
    DateTimeOffset? ScheduledArrivalAt,
    DateTimeOffset? ArrivedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record RouteSummaryResponse(
    Guid RouteId,
    string RouteNumber,
    string Title,
    string RouteStatus,
    Guid? TripId,
    int StopCount,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ActivatedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? CancelledAt);

public sealed record RouteDetailResponse(
    Guid RouteId,
    string RouteNumber,
    string Title,
    string Description,
    string RouteStatus,
    Guid? TripId,
    IReadOnlyList<RouteStopSummaryResponse> Stops,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ActivatedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? CancelledAt);

public sealed record CreateRouteStopRequest(
    string StopKey,
    string Label,
    string AddressLabel,
    string StopType,
    int SequenceNumber,
    decimal? GeofenceAnchorLatitude = null,
    decimal? GeofenceAnchorLongitude = null,
    int? GeofenceRadiusMeters = null,
    DateTimeOffset? ScheduledArrivalAt = null,
    Guid? StaffarrSiteOrgUnitId = null);

public sealed record CreateRouteRequest(
    string Title,
    string Description,
    Guid? TripId,
    IReadOnlyList<CreateRouteStopRequest>? Stops);

public sealed record CreateRouteTemplateRequest(
    string Title,
    string Description,
    IReadOnlyList<CreateRouteStopRequest>? Stops);

public sealed record LinkRouteTripRequest(Guid TripId);

public sealed record ReorderRouteStopsRequest(IReadOnlyList<Guid> StopIds);

public sealed record AddRouteStopRequest(
    string StopKey,
    string Label,
    string AddressLabel,
    string StopType,
    int SequenceNumber,
    decimal? GeofenceAnchorLatitude = null,
    decimal? GeofenceAnchorLongitude = null,
    int? GeofenceRadiusMeters = null,
    DateTimeOffset? ScheduledArrivalAt = null,
    Guid? StaffarrSiteOrgUnitId = null);

public sealed record UpdateRouteStopStatusRequest(string StopStatus);

public sealed record CheckRouteStopGeofenceRequest(
    decimal ReportedLatitude,
    decimal ReportedLongitude);

public sealed record UpdateRouteStatusRequest(string RouteStatus);
