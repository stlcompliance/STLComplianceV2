using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class VendorProcurementGuardService(SupplyArrDbContext db)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly HashSet<string> BlockedApprovalStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "restricted",
        "inactive",
    };

    public async Task EnsureVendorAllowedForScopeAsync(
        Guid tenantId,
        Guid vendorPartyId,
        string scope,
        CancellationToken cancellationToken = default)
    {
        var party = await db.ExternalParties
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == vendorPartyId,
                cancellationToken);

        if (party is null
            || !VendorRestrictionPartyTypes.Allowed.Contains(party.PartyType)
            || !string.Equals(party.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "vendor_restrictions.party_not_found",
                "Active vendor or supplier party was not found.",
                404);
        }

        if (BlockedApprovalStatuses.Contains(party.ApprovalStatus))
        {
            throw new StlApiException(
                "vendor_restrictions.approval_blocked",
                $"Party approval status '{party.ApprovalStatus}' blocks procurement activity.",
                409);
        }

        var asOfUtc = DateTimeOffset.UtcNow;
        var restrictions = await db.VendorRestrictions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.ExternalPartyId == vendorPartyId
                && x.Status == VendorRestrictionStatuses.Active)
            .ToListAsync(cancellationToken);

        foreach (var restriction in restrictions)
        {
            if (!VendorRestrictionRules.IsRestrictionEffective(restriction, asOfUtc))
            {
                continue;
            }

            var scopes = DeserializeScopes(restriction.ScopesJson);
            if (VendorRestrictionRules.ScopeBlocks(scopes, scope))
            {
                throw new StlApiException(
                    "vendor_restrictions.scope_blocked",
                    $"Vendor is restricted for scope '{scope}': {restriction.Reason}",
                    409);
            }
        }
    }

    public async Task<VendorRestrictionEnforcementResponse> GetEnforcementAsync(
        Guid tenantId,
        Guid externalPartyId,
        CancellationToken cancellationToken = default)
    {
        var party = await db.ExternalParties
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == externalPartyId,
                cancellationToken)
            ?? throw new StlApiException(
                "vendor_restrictions.party_not_found",
                "Party was not found.",
                404);

        var asOfUtc = DateTimeOffset.UtcNow;
        var activeScopes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (BlockedApprovalStatuses.Contains(party.ApprovalStatus))
        {
            activeScopes.Add(VendorRestrictionScopes.AllProcurement);
            return new VendorRestrictionEnforcementResponse(
                externalPartyId,
                IsBlocked: true,
                BlockReason: $"Party approval status is {party.ApprovalStatus}.",
                activeScopes.OrderBy(x => x).ToList());
        }

        var restrictions = await db.VendorRestrictions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.ExternalPartyId == externalPartyId
                && x.Status == VendorRestrictionStatuses.Active)
            .ToListAsync(cancellationToken);

        string? blockReason = null;
        foreach (var restriction in restrictions)
        {
            if (!VendorRestrictionRules.IsRestrictionEffective(restriction, asOfUtc))
            {
                continue;
            }

            foreach (var scope in DeserializeScopes(restriction.ScopesJson))
            {
                activeScopes.Add(scope);
            }

            blockReason ??= restriction.Reason;
        }

        return new VendorRestrictionEnforcementResponse(
            externalPartyId,
            IsBlocked: activeScopes.Count > 0,
            blockReason,
            activeScopes.OrderBy(x => x).ToList());
    }

    private static IReadOnlyList<string> DeserializeScopes(string scopesJson)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(scopesJson, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
