namespace StaffArr.Api.Contracts;

public sealed record IncidentSupplyDemandLineResponse(
    Guid DemandLineId,
    int LineNumber,
    Guid? SupplyarrPartId,
    string PartNumber,
    string Description,
    decimal QuantityRequested,
    string UnitOfMeasure,
    string Notes,
    string Status,
    Guid? StaffarrPublicationId,
    Guid? SupplyarrDemandRefId,
    DateTimeOffset? PublishedAt,
    string ProcurementStatus,
    Guid? SupplyarrPurchaseRequestId,
    Guid? SupplyarrPurchaseOrderId,
    decimal QuantityReceived,
    string ProcurementStatusMessage,
    DateTimeOffset? LastProcurementStatusAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateIncidentSupplyDemandLineRequest(
    Guid? SupplyarrPartId,
    string? PartNumber,
    string? Description,
    decimal QuantityRequested,
    string? UnitOfMeasure,
    string? Notes);

public sealed record PublishIncidentSupplyDemandRequest(bool CreatePurchaseRequestDraft);

public sealed record PublishIncidentSupplyDemandResponse(
    Guid PublicationId,
    Guid DemandRefId,
    Guid? PurchaseRequestId,
    bool CreatedPurchaseRequestDraft,
    IReadOnlyList<IncidentSupplyDemandLineResponse> Lines);
