namespace SupplyArr.Api.Contracts;



public sealed record PricingSnapshotResponse(

    Guid PricingSnapshotId,

    string SnapshotKey,

    Guid PartVendorLinkId,

    Guid PartId,

    string PartKey,

    string PartDisplayName,

    Guid VendorPartyId,

    string VendorPartyKey,

    string VendorDisplayName,

    string VendorPartNumber,

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

    Guid PartVendorLinkId,

    decimal UnitPrice,

    string? CurrencyCode,

    decimal? MinimumOrderQuantity,

    DateTimeOffset? EffectiveFrom,

    string? Source,

    string? Notes);


