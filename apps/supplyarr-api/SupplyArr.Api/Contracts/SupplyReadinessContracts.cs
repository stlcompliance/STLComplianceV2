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

public sealed record SupplyReadinessPredictiveStockoutResponse(
    Guid PartId,
    string PartKey,
    string DisplayName,
    decimal QuantityAvailable,
    decimal OpenDemandQuantity,
    decimal OpenBackorderQuantity,
    decimal ProjectedQuantity,
    decimal ShortageQuantity,
    decimal? ReorderPoint,
    string RiskLevel,
    string Reason,
    DateTimeOffset? SourceTimestamp);

public sealed record SupplyReadinessDashboardResponse(
    DateTimeOffset GeneratedAt,
    SupplyReadinessTotalsResponse Totals,
    IReadOnlyList<SupplyReadinessDemandRefSourceCountResponse> DemandRefsBySource,
    IReadOnlyList<SupplyReadinessAttentionItemResponse> AttentionItems,
    IReadOnlyList<SupplyReadinessPredictiveStockoutResponse> PredictiveStockoutItems);

public sealed record SupplyReadinessBlockerResponse(
    string ReasonCode,
    string Message,
    string SourceEntityType,
    string SourceEntityId,
    string? RelatedEntityId);

public sealed record SupplyReadinessAvailabilitySnapshotResponse(
    decimal QuantityOnHand,
    decimal QuantityReserved,
    decimal QuantityAvailable,
    decimal? ReorderPoint,
    int ActiveReservationCount,
    int OpenBackorderCount,
    DateTimeOffset? SourceTimestamp = null);

public sealed record SupplyReadinessSourceSnapshotResponse(
    DateTimeOffset? SourceTimestamp,
    bool IsStale,
    int? StalenessMinutes);

public sealed record SupplyReadinessPricingLeadTimeSnapshotResponse(
    Guid PartVendorLinkId,
    decimal? UnitPrice,
    string? CurrencyCode,
    decimal? MinimumOrderQuantity,
    int? LeadTimeDays,
    DateTimeOffset? PriceSourceTimestamp,
    DateTimeOffset? LeadTimeSourceTimestamp,
    bool IsCatalogFallback);

public sealed record SupplyReadinessSubstituteRecommendationResponse(
    Guid PartId,
    string PartKey,
    string DisplayName,
    decimal QuantityAvailable,
    string RecommendationBasis,
    DateTimeOffset? SourceTimestamp);

public sealed record SupplyReadinessDecisionSnapshotResponse(
    Guid AuditEventId,
    string SnapshotKind,
    DateTimeOffset CapturedAt);

public sealed record PartSupplyReadinessResponse(
    Guid PartId,
    string PartKey,
    string DisplayName,
    string Status,
    string ReadinessStatus,
    string ReadinessBasis,
    DateTimeOffset CalculatedAt,
    IReadOnlyList<SupplyReadinessBlockerResponse> Blockers,
    SupplyReadinessAvailabilitySnapshotResponse Availability,
    IReadOnlyList<SupplyReadinessSubstituteRecommendationResponse> SubstituteRecommendations,
    SupplyReadinessSourceSnapshotResponse? SourceSnapshot = null,
    SupplyReadinessDecisionSnapshotResponse? AuditSnapshot = null);

public record SupplierSupplyReadinessResponse(
    Guid SupplierId,
    string SupplierKey,
    string DisplayName,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
    string ApprovalStatus,
    string Status,
    string ReadinessStatus,
    string ReadinessBasis,
    DateTimeOffset CalculatedAt,
    IReadOnlyList<SupplyReadinessBlockerResponse> Blockers,
    SupplyReadinessSourceSnapshotResponse? SourceSnapshot = null,
    SupplyReadinessDecisionSnapshotResponse? AuditSnapshot = null);

public sealed record VendorSupplyReadinessResponse(
    Guid SupplierId,
    string SupplierKey,
    string DisplayName,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
    string ApprovalStatus,
    string Status,
    string ReadinessStatus,
    string ReadinessBasis,
    DateTimeOffset CalculatedAt,
    IReadOnlyList<SupplyReadinessBlockerResponse> Blockers,
    SupplyReadinessSourceSnapshotResponse? SourceSnapshot = null,
    SupplyReadinessDecisionSnapshotResponse? AuditSnapshot = null)
    : SupplierSupplyReadinessResponse(
        SupplierId,
        SupplierKey,
        DisplayName,
        ParentSupplierId,
        ParentSupplierDisplayName,
        SupplierUnitKind,
        SupplierServiceTypes,
        ApprovalStatus,
        Status,
        ReadinessStatus,
        ReadinessBasis,
        CalculatedAt,
        Blockers,
        SourceSnapshot,
        AuditSnapshot);

public sealed record ProcurementPathReadinessResponse(
    Guid PartId,
    string PartKey,
    Guid SupplierId,
    string SupplierKey,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
    decimal? RequestedQuantity,
    string ReadinessStatus,
    string ReadinessBasis,
    DateTimeOffset CalculatedAt,
    IReadOnlyList<SupplyReadinessBlockerResponse> Blockers,
    SupplyReadinessPricingLeadTimeSnapshotResponse? PricingLeadTime,
    SupplyReadinessSourceSnapshotResponse? SourceSnapshot = null,
    SupplyReadinessDecisionSnapshotResponse? AuditSnapshot = null);
