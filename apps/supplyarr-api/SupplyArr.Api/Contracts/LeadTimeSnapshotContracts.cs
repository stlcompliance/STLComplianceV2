namespace SupplyArr.Api.Contracts;

public sealed record LeadTimeSnapshotResponse(
    Guid LeadTimeSnapshotId,
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
    int LeadTimeDays,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    string Source,
    string Notes,
    bool IsCurrent,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateLeadTimeSnapshotRequest(
    string SnapshotKey,
    Guid PartSupplierLinkId,
    int LeadTimeDays,
    DateTimeOffset? EffectiveFrom,
    string? Source,
    string? Notes);
