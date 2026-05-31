namespace MaintainArr.Api.Contracts;

public sealed record IngestRoutarrEventRequest(
    Guid TenantId,
    Guid SourceEventId,
    string EventKind,
    string RelatedEntityType,
    Guid RelatedEntityId,
    Guid CorrelationId,
    RoutarrEventPayload Payload,
    DateTimeOffset? OccurredAt = null);

public sealed record RoutarrEventPayload(
    Guid TenantId,
    string Summary,
    Guid? TripId,
    string? TripNumber = null,
    string? DriverPersonId = null,
    string? VehicleRefKey = null,
    string? DispatchStatus = null,
    Guid? RouteId = null,
    Guid? StopId = null,
    string? StopKey = null,
    string? StopStatus = null,
    Guid? ProofId = null,
    string? ProofType = null,
    Guid? DvirId = null,
    string? DvirPhase = null,
    string? DvirResult = null,
    string? DefectNotes = null,
    Guid? ExceptionId = null,
    string? ExceptionKey = null,
    string? ExceptionCategory = null,
    string? IncidentType = null,
    string? IncidentSeverity = null,
    string? IncidentReviewStatus = null,
    string? IncidentRoutedProduct = null,
    string? OverrideTargetType = null,
    IReadOnlyList<string>? OverrideKinds = null,
    string? RouteNumber = null,
    string? RouteStatus = null);

public sealed record IngestRoutarrEventResponse(
    Guid InboundEventId,
    string Outcome,
    Guid? DefectId,
    bool IdempotentReplay);
