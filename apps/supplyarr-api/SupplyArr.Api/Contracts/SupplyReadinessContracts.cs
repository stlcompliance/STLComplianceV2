namespace SupplyArr.Api.Contracts;

public sealed record SupplyReadinessTotalsResponse(
    int ActivePartsCount,
    int PartsBelowReorderCount,
    int StockLineCount,
    decimal TotalQuantityOnHand,
    decimal TotalQuantityReserved,
    decimal TotalQuantityAvailable,
    int OpenBackorderCount,
    int OpenPurchaseRequestCount,
    int OpenPurchaseOrderCount,
    int IssuedPurchaseOrderCount,
    int OpenDemandRefCount,
    int ComplianceAttentionCount,
    int ActiveVendorRestrictionCount,
    int ActiveProcurementExceptionCount);

public sealed record SupplyReadinessDemandRefSourceCountResponse(
    string Source,
    int OpenCount);

public sealed record SupplyReadinessAttentionItemResponse(
    string Category,
    string Title,
    string Detail,
    string? Status,
    DateTimeOffset? OccurredAt,
    string? RelatedEntityType,
    Guid? RelatedEntityId);

public sealed record SupplyReadinessDashboardResponse(
    DateTimeOffset GeneratedAt,
    SupplyReadinessTotalsResponse Totals,
    IReadOnlyList<SupplyReadinessDemandRefSourceCountResponse> DemandRefsBySource,
    IReadOnlyList<SupplyReadinessAttentionItemResponse> AttentionItems);
