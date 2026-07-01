using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public static class SupplierRestrictionRules
{
    public static string NormalizeRestrictionKey(string restrictionKey)
    {
        var normalized = restrictionKey.Trim();
        if (normalized.Length is < 2 or > 64)
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "supplier_restrictions.invalid_key",
                "Restriction key must be between 2 and 64 characters.",
                400);
        }

        return normalized;
    }

    public static IReadOnlyList<string> NormalizeScopes(IReadOnlyList<string> scopes)
    {
        if (scopes is null || scopes.Count == 0)
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "supplier_restrictions.scopes_required",
                "At least one restriction scope is required.",
                400);
        }

        var normalized = scopes
            .Select(x => x.Trim().ToLowerInvariant())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var scope in normalized)
        {
            if (!SupplierRestrictionScopes.All.Contains(scope))
            {
                throw new STLCompliance.Shared.Contracts.StlApiException(
                    "supplier_restrictions.invalid_scope",
                    $"Scope '{scope}' is not supported.",
                    400);
            }
        }

        return normalized;
    }

    public static bool IsRestrictionEffective(
        SupplierRestriction restriction,
        DateTimeOffset asOfUtc) =>
        string.Equals(restriction.Status, SupplierRestrictionStatuses.Active, StringComparison.OrdinalIgnoreCase)
            && restriction.EffectiveFrom <= asOfUtc
            && (restriction.EffectiveUntil is null || restriction.EffectiveUntil > asOfUtc);

    public static bool ScopeBlocks(
        IReadOnlyList<string> restrictionScopes,
        string requestedScope) =>
        restrictionScopes.Contains(SupplierRestrictionScopes.AllProcurement, StringComparer.OrdinalIgnoreCase)
            || restrictionScopes.Contains(requestedScope, StringComparer.OrdinalIgnoreCase);

    public static string NormalizeReason(string reason)
    {
        var normalized = reason.Trim();
        if (normalized.Length is < 3 or > 512)
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "supplier_restrictions.invalid_reason",
                "Reason must be between 3 and 512 characters.",
                400);
        }

        return normalized;
    }

    public static string NormalizeLiftNotes(string? liftNotes) =>
        string.IsNullOrWhiteSpace(liftNotes) ? string.Empty : liftNotes.Trim()[..Math.Min(liftNotes.Trim().Length, 512)];
}
