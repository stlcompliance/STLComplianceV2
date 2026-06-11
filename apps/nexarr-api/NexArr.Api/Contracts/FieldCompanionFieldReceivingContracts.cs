namespace NexArr.Api.Contracts;

public sealed record FieldCompanionFieldReceivingLine(
    string LineId,
    int LineNumber,
    string PartKey,
    string PartDisplayName,
    decimal QuantityExpected,
    decimal QuantityReceived,
    decimal QuantityOrdered,
    decimal QuantityRemainingOnOrder,
    int OpenExceptionCount);

public sealed record FieldCompanionFieldReceivingDetailResponse(
    string TaskKey,
    string ProductKey,
    string ReceivingReceiptId,
    string ReceiptKey,
    string Status,
    string PurchaseOrderKey,
    string BinKey,
    string BinName,
    string LocationName,
    string Notes,
    IReadOnlyList<FieldCompanionFieldReceivingLine> Lines);

public sealed record UpdateFieldCompanionFieldReceivingLineRequest(
    string TaskKey,
    string LineId,
    decimal QuantityReceived);

public sealed record FieldCompanionFieldReceivingLineResponse(
    string TaskKey,
    string ProductKey,
    string ReceivingReceiptId,
    string LineId,
    decimal QuantityReceived,
    string Status,
    DateTimeOffset UpdatedAt);

public sealed record PostFieldCompanionFieldReceivingRequest(string TaskKey);

public sealed record FieldCompanionFieldReceivingPostResponse(
    string TaskKey,
    string ProductKey,
    string ReceivingReceiptId,
    string Status,
    DateTimeOffset PostedAt);

public sealed record LoadArrReceivingSessionUpstreamResponse(
    string Id,
    string ReceivingNumber,
    string ReceivingType,
    string Status,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string SourceProductKey,
    string SourceObjectType,
    string SourceObjectId,
    string SupplierNameSnapshot,
    string StartedByPersonId,
    string? CompletedByPersonId,
    string StartedAtUtc,
    string? CompletedAtUtc,
    IReadOnlyList<LoadArrReceivingLineUpstreamResponse> Lines);

public sealed record LoadArrReceivingLineUpstreamResponse(
    string Id,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal ExpectedQuantity,
    decimal ReceivedQuantity,
    string UnitOfMeasure,
    string WarehouseLocationId,
    string LocationNameSnapshot,
    string? LotCode,
    string? SerialCode,
    string Condition,
    string Status,
    string? DiscrepancyReasonCode,
    string? EvidenceSummary);

public sealed record CompleteLoadArrReceivingSessionUpstreamRequest(
    string ReceivingType,
    string SourceProductKey,
    string SourceObjectType,
    string SourceObjectId,
    string SupplierNameSnapshot,
    string CompletedByPersonId,
    string SupplyarrItemId,
    decimal ExpectedQuantity,
    decimal ReceivedQuantity,
    string WarehouseLocationId,
    string? LotCode,
    string? SerialCode,
    string Condition,
    string? DiscrepancyReasonCode,
    string? ComplianceEvaluationId,
    string? EvidenceSummary);

public sealed record LoadArrReceivingCompletionUpstreamResponse(
    LoadArrReceivingSessionUpstreamResponse Session);
