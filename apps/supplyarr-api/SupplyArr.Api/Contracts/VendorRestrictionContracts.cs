namespace SupplyArr.Api.Contracts;

public record SupplierRestrictionResponse(
    Guid RestrictionId,
    Guid SupplierId,
    string SupplierKey,
    string SupplierDisplayName,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
    string RestrictionKey,
    IReadOnlyList<string> Scopes,
    string Reason,
    string Status,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveUntil,
    Guid CreatedByUserId,
    Guid? LiftedByUserId,
    DateTimeOffset? LiftedAt,
    string? LiftNotes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid? SupplierRestrictionId = null);

public sealed record VendorRestrictionResponse(
    Guid RestrictionId,
    Guid SupplierId,
    string SupplierKey,
    string SupplierDisplayName,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
    string RestrictionKey,
    IReadOnlyList<string> Scopes,
    string Reason,
    string Status,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveUntil,
    Guid CreatedByUserId,
    Guid? LiftedByUserId,
    DateTimeOffset? LiftedAt,
    string? LiftNotes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid? SupplierRestrictionId = null,
    Guid? ExternalPartyId = null,
    string? PartyKey = null,
    string? PartyDisplayName = null,
    string? PartyType = null,
    string? SupplierType = null)
    : SupplierRestrictionResponse(
        RestrictionId,
        SupplierId,
        SupplierKey,
        SupplierDisplayName,
        ParentSupplierId,
        ParentSupplierDisplayName,
        SupplierUnitKind,
        SupplierServiceTypes,
        RestrictionKey,
        Scopes,
        Reason,
        Status,
        EffectiveFrom,
        EffectiveUntil,
        CreatedByUserId,
        LiftedByUserId,
        LiftedAt,
        LiftNotes,
        CreatedAt,
        UpdatedAt,
        SupplierRestrictionId);

public record CreateSupplierRestrictionRequest(
    string RestrictionKey,
    IReadOnlyList<string> Scopes,
    string Reason,
    DateTimeOffset? EffectiveFrom,
    DateTimeOffset? EffectiveUntil);

public sealed record CreateVendorRestrictionRequest(
    string RestrictionKey,
    IReadOnlyList<string> Scopes,
    string Reason,
    DateTimeOffset? EffectiveFrom,
    DateTimeOffset? EffectiveUntil)
    : CreateSupplierRestrictionRequest(
        RestrictionKey,
        Scopes,
        Reason,
        EffectiveFrom,
        EffectiveUntil);

public record UpdateSupplierRestrictionRequest(
    IReadOnlyList<string> Scopes,
    string Reason,
    DateTimeOffset? EffectiveUntil);

public sealed record UpdateVendorRestrictionRequest(
    IReadOnlyList<string> Scopes,
    string Reason,
    DateTimeOffset? EffectiveUntil)
    : UpdateSupplierRestrictionRequest(
        Scopes,
        Reason,
        EffectiveUntil);

public record LiftSupplierRestrictionRequest(string? LiftNotes);

public sealed record LiftVendorRestrictionRequest(string? LiftNotes)
    : LiftSupplierRestrictionRequest(LiftNotes);

public record SupplierRestrictionEnforcementResponse(
    Guid SupplierId,
    string SupplierKey,
    string SupplierDisplayName,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
    bool IsBlocked,
    string? BlockReason,
    IReadOnlyList<string> ActiveScopes);

public sealed record VendorRestrictionEnforcementResponse(
    Guid SupplierId,
    string SupplierKey,
    string SupplierDisplayName,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
    bool IsBlocked,
    string? BlockReason,
    IReadOnlyList<string> ActiveScopes,
    Guid? ExternalPartyId = null)
    : SupplierRestrictionEnforcementResponse(
        SupplierId,
        SupplierKey,
        SupplierDisplayName,
        ParentSupplierId,
        ParentSupplierDisplayName,
        SupplierUnitKind,
        SupplierServiceTypes,
        IsBlocked,
        BlockReason,
        ActiveScopes);
