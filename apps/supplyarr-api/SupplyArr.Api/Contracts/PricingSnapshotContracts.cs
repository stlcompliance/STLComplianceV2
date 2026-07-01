namespace SupplyArr.Api.Contracts;

public sealed record PricingSnapshotResponse(
    Guid PricingSnapshotId,
    string SnapshotKey,
    Guid PartSupplierLinkId,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    Guid SupplierId,
    string SupplierKey,
    string SupplierDisplayName,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
    string SupplierPartNumber,
    decimal UnitPrice,
    string CurrencyCode,
    decimal? MinimumOrderQuantity,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    string Source,
    string Notes,
    bool IsCurrent,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreatePricingSnapshotRequest(
    string SnapshotKey,
    Guid PartSupplierLinkId,
    decimal UnitPrice,
    string? CurrencyCode,
    decimal? MinimumOrderQuantity,
    DateTimeOffset? EffectiveFrom,
    string? Source,
    string? Notes);
