namespace SupplyArr.Api.Contracts;

public sealed record VendorRestrictionResponse(
    Guid RestrictionId,
    Guid ExternalPartyId,
    string PartyKey,
    string PartyDisplayName,
    string PartyType,
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
    DateTimeOffset UpdatedAt);

public sealed record CreateVendorRestrictionRequest(
    string RestrictionKey,
    IReadOnlyList<string> Scopes,
    string Reason,
    DateTimeOffset? EffectiveFrom,
    DateTimeOffset? EffectiveUntil);

public sealed record UpdateVendorRestrictionRequest(
    IReadOnlyList<string> Scopes,
    string Reason,
    DateTimeOffset? EffectiveUntil);

public sealed record LiftVendorRestrictionRequest(string? LiftNotes);

public sealed record VendorRestrictionEnforcementResponse(
    Guid ExternalPartyId,
    bool IsBlocked,
    string? BlockReason,
    IReadOnlyList<string> ActiveScopes);
