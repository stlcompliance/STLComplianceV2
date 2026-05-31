namespace RoutArr.Api.Contracts;

public sealed record DispatchExceptionSummaryResponse(
    Guid ExceptionId,
    string ExceptionKey,
    string Title,
    string Description,
    string Category,
    string Status,
    string IncidentType,
    string IncidentSeverity,
    string IncidentReviewStatus,
    string IncidentRoutedProduct,
    Guid? StaffarrPersonnelIncidentId,
    DateTimeOffset? StaffarrIncidentRoutedAt,
    string StaffarrIncidentRouteStatus,
    Guid? TrainarrIncidentRemediationId,
    DateTimeOffset? TrainarrIncidentRoutedAt,
    string TrainarrIncidentRouteStatus,
    Guid? MaintainarrInboundEventId,
    Guid? MaintainarrDefectId,
    DateTimeOffset? MaintainarrIncidentRoutedAt,
    string MaintainarrIncidentRouteStatus,
    Guid? CompliancecoreFactPublicationId,
    DateTimeOffset? CompliancecoreIncidentRoutedAt,
    string CompliancecoreIncidentRouteStatus,
    Guid? TripId,
    string? TripNumber,
    string? TripTitle,
    Guid? AssignedToUserId,
    DateTimeOffset? SlaDueAt,
    bool IsSlaBreached,
    string ResolutionTemplateKey,
    string ResolutionNotes,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? AssignedAt,
    DateTimeOffset? ResolvedAt);

public sealed record DispatchExceptionListResponse(
    int TotalCount,
    int OpenCount,
    int OverdueCount,
    IReadOnlyList<DispatchExceptionSummaryResponse> Items);

public sealed record CreateDispatchExceptionRequest(
    string Title,
    string Description,
    string? Category,
    Guid? TripId,
    Guid? AssignedToUserId,
    DateTimeOffset? SlaDueAt);

public sealed record CreateDispatchIncidentRequest(
    string Title,
    string Description,
    string? IncidentType,
    string? IncidentSeverity,
    Guid? TripId,
    Guid? AssignedToUserId,
    DateTimeOffset? SlaDueAt,
    string? RoutedProduct);

public sealed record AssignDispatchExceptionRequest(
    Guid AssignedToUserId,
    DateTimeOffset? SlaDueAt);

public sealed record ResolveDispatchExceptionRequest(
    string? ResolutionNotes,
    string? ResolutionTemplateKey);

public sealed record LinkDispatchExceptionTripRequest(Guid TripId);
