namespace TrainArr.Api.Contracts;

public sealed record RoutarrQualificationCheckRequest(
    Guid TenantId,
    Guid StaffarrPersonId,
    string QualificationKey,
    string? RulePackKey,
    IReadOnlyDictionary<string, string>? Context);

public sealed record IngestRoutarrIncidentRemediationRequest(
    Guid TenantId,
    Guid SourceEventId,
    string EventKind,
    Guid RelatedEntityId,
    Guid CorrelationId,
    RoutarrIncidentPayload Payload,
    DateTimeOffset? OccurredAt = null);

public sealed record RoutarrIncidentPayload(
    Guid TenantId,
    string Summary,
    Guid? TripId,
    string? TripNumber = null,
    string? DriverPersonId = null,
    string? VehicleRefKey = null,
    string? DispatchStatus = null,
    Guid? ExceptionId = null,
    string? ExceptionKey = null,
    string? ExceptionCategory = null,
    string? IncidentType = null,
    string? IncidentSeverity = null,
    string? IncidentReviewStatus = null,
    string? IncidentRoutedProduct = null);

public sealed record IngestRoutarrIncidentRemediationResponse(
    Guid RemediationId,
    Guid TenantId,
    Guid SourceEventId,
    Guid StaffarrPersonId,
    string Status,
    bool IdempotentReplay);

public sealed record IngestSupplyarrIncidentRemediationRequest(
    Guid TenantId,
    Guid SourceEventId,
    string EventKind,
    Guid SupplierIncidentId,
    Guid CorrelationId,
    Guid StaffarrPersonId,
    SupplyarrIncidentPayload Payload,
    DateTimeOffset? OccurredAt = null);

public sealed record SupplyarrIncidentPayload(
    Guid TenantId,
    string Summary,
    Guid SupplierIncidentId,
    string IncidentKey,
    string IncidentType,
    string Severity,
    string Status,
    Guid ExternalPartyId,
    string PartyDisplayName,
    Guid? PurchaseRequestId = null,
    Guid? PurchaseOrderId = null,
    Guid? ReceivingReceiptId = null,
    Guid? ReceivingExceptionId = null);

public sealed record IngestSupplyarrIncidentRemediationResponse(
    Guid RemediationId,
    Guid TenantId,
    Guid SourceEventId,
    Guid StaffarrPersonId,
    string Status,
    bool IdempotentReplay);
