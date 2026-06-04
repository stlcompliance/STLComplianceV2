namespace MaintainArr.Api.Contracts;

public sealed record IngestPartIssueEventRequest(
    Guid TenantId,
    Guid WorkOrderId,
    Guid? SupplyarrPartId,
    string? PartNumber,
    decimal Quantity,
    string? UnitOfMeasure,
    string? SourceReference,
    string? Message,
    DateTimeOffset OccurredAt);

public sealed record IngestPartIssueEventResponse(
    Guid IssueEventId,
    Guid WorkOrderId,
    int LinesUpdated,
    decimal QuantityIssued,
    string Status,
    DateTimeOffset OccurredAt,
    bool Duplicate);
