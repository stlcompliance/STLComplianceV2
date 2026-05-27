namespace SupplyArr.Api.Contracts;

public sealed record AvailabilitySnapshotResponse(
    Guid AvailabilitySnapshotId,
    string SnapshotKey,
    Guid PartVendorLinkId,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    Guid VendorPartyId,
    string VendorPartyKey,
    string VendorDisplayName,
    string VendorPartNumber,
    decimal? QuantityAvailable,
    string AvailabilityStatus,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    string Source,
    string Notes,
    bool IsCurrent,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateAvailabilitySnapshotRequest(
    string SnapshotKey,
    Guid PartVendorLinkId,
    decimal? QuantityAvailable,
    string AvailabilityStatus,
    DateTimeOffset? EffectiveFrom,
    string? Source,
    string? Notes);
