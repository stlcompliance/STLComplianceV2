namespace SupplyArr.Api.Contracts;

public sealed record IngestStaffarrDemandRequest(
    Guid TenantId,
    Guid StaffarrPublicationId,
    Guid StaffarrIncidentId,
    Guid StaffarrPersonId,
    string StaffarrIncidentTitle,
    string Title,
    string? Notes,
    bool CreatePurchaseRequestDraft,
    IReadOnlyList<IngestStaffarrDemandLineRequest> Lines);

public sealed record IngestStaffarrDemandLineRequest(
    Guid StaffarrDemandLineId,
    Guid? SupplyarrPartId,
    string? PartNumber,
    string? Description,
    decimal QuantityRequested,
    string? UnitOfMeasure,
    string? Notes);

public sealed record StaffarrDemandIntakeResponse(
    Guid DemandRefId,
    string Status,
    Guid? PurchaseRequestId,
    bool CreatedPurchaseRequestDraft,
    bool IdempotentReplay);

public sealed record StaffArrDemandRefLineResponse(
    Guid LineId,
    int LineNumber,
    Guid StaffarrDemandLineId,
    Guid? PartId,
    string PartNumber,
    string Description,
    decimal QuantityRequested,
    string UnitOfMeasure,
    string Notes);

public sealed record StaffArrDemandRefResponse(
    Guid DemandRefId,
    Guid StaffarrPublicationId,
    Guid StaffarrIncidentId,
    Guid StaffarrPersonId,
    string StaffarrIncidentTitle,
    string Title,
    string Notes,
    string Status,
    string ProcurementStatus,
    Guid? PurchaseRequestId,
    Guid? PurchaseOrderId,
    DateTimeOffset ReceivedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<StaffArrDemandRefLineResponse> Lines);

public sealed record CreatePurchaseRequestFromStaffarrDemandRefRequest(
    string RequestKey,
    string Title,
    string? Notes);
