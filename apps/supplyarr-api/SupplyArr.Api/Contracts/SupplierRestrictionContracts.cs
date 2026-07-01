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

public record CreateSupplierRestrictionRequest(
    string RestrictionKey,
    IReadOnlyList<string> Scopes,
    string Reason,
    DateTimeOffset? EffectiveFrom,
    DateTimeOffset? EffectiveUntil);

public record UpdateSupplierRestrictionRequest(
    IReadOnlyList<string> Scopes,
    string Reason,
    DateTimeOffset? EffectiveUntil);

public record LiftSupplierRestrictionRequest(string? LiftNotes);

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
