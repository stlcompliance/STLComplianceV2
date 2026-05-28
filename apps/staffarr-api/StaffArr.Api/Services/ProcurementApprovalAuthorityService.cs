using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class ProcurementApprovalAuthorityService(
    StaffArrDbContext db,
    PermissionProjectionService permissionProjectionService)
{
    public const string ReadAuthorityActionScope = "staffarr.procurement_approval_authority.read";

    public async Task<ProcurementApprovalAuthorityResponse> GetByPersonIdAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        var person = await db.People.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == personId)
            .Select(x => new { x.Id, x.ExternalUserId, x.EmploymentStatus })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new StlApiException("people.not_found", "Person was not found.", 404);

        if (!string.Equals(person.EmploymentStatus, "active", StringComparison.OrdinalIgnoreCase))
        {
            return EmptyAuthority(person.Id, person.ExternalUserId);
        }

        var projection = await permissionProjectionService.GetEffectivePermissionProjectionAsync(
            tenantId,
            personId,
            cancellationToken);

        return MapAuthority(person.Id, person.ExternalUserId, projection);
    }

    public async Task<ProcurementApprovalAuthorityResponse> GetByExternalUserIdAsync(
        Guid tenantId,
        Guid externalUserId,
        CancellationToken cancellationToken = default)
    {
        var person = await db.People.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ExternalUserId == externalUserId)
            .Select(x => new { x.Id, x.ExternalUserId, x.EmploymentStatus })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new StlApiException("people.not_found", "Person was not found for the external user.", 404);

        return await GetByPersonIdAsync(tenantId, person.Id, cancellationToken);
    }

    private static ProcurementApprovalAuthorityResponse MapAuthority(
        Guid personId,
        Guid? externalUserId,
        EffectivePermissionProjectionResponse projection)
    {
        var grants = new List<ProcurementApprovalAuthorityGrantResponse>();
        var orgUnitScopeIds = new HashSet<Guid>();

        foreach (var permission in projection.Permissions)
        {
            if (!StaffArrProcurementPermissionKeys.All.Contains(permission.PermissionKey))
            {
                continue;
            }

            foreach (var source in permission.Sources)
            {
                grants.Add(new ProcurementApprovalAuthorityGrantResponse(
                    permission.PermissionKey,
                    permission.PermissionName,
                    permission.ScopeType,
                    permission.ScopeValue,
                    source.RoleKey,
                    source.RoleName));
            }

            if (string.Equals(permission.ScopeType, StaffArrProcurementScopeTypes.OrgUnit, StringComparison.OrdinalIgnoreCase)
                && Guid.TryParse(permission.ScopeValue, out var orgUnitId))
            {
                orgUnitScopeIds.Add(orgUnitId);
            }
        }

        var submitLimits = CollectMonetaryLimits(
            projection.Permissions,
            StaffArrProcurementPermissionKeys.PurchaseRequestsSubmit);
        var approveLimits = CollectMonetaryLimits(
            projection.Permissions,
            StaffArrProcurementPermissionKeys.PurchaseRequestsApprove);
        var issueLimits = CollectMonetaryLimits(
            projection.Permissions,
            StaffArrProcurementPermissionKeys.PurchaseOrdersIssue);

        return new ProcurementApprovalAuthorityResponse(
            personId,
            externalUserId,
            projection.ComputedAt,
            HasPermission(projection.Permissions, StaffArrProcurementPermissionKeys.PurchaseRequestsSubmit),
            HasPermission(projection.Permissions, StaffArrProcurementPermissionKeys.PurchaseRequestsApprove),
            HasPermission(projection.Permissions, StaffArrProcurementPermissionKeys.PurchaseOrdersIssue),
            submitLimits,
            approveLimits,
            issueLimits,
            orgUnitScopeIds.OrderBy(x => x).ToList(),
            grants);
    }

    private static bool HasPermission(
        IReadOnlyList<EffectivePermissionResponse> permissions,
        string permissionKey) =>
        permissions.Any(x => string.Equals(x.PermissionKey, permissionKey, StringComparison.OrdinalIgnoreCase));

    private static decimal? CollectMonetaryLimits(
        IReadOnlyList<EffectivePermissionResponse> permissions,
        string permissionKey)
    {
        decimal? maxLimit = null;
        var hasUnlimitedTenantGrant = false;

        foreach (var permission in permissions.Where(x =>
                     string.Equals(x.PermissionKey, permissionKey, StringComparison.OrdinalIgnoreCase)))
        {
            if (string.Equals(permission.ScopeType, StaffArrProcurementScopeTypes.Tenant, StringComparison.OrdinalIgnoreCase))
            {
                hasUnlimitedTenantGrant = true;
                continue;
            }

            if (string.Equals(permission.ScopeType, StaffArrProcurementScopeTypes.MonetaryLimit, StringComparison.OrdinalIgnoreCase)
                && TryParseLimit(permission.ScopeValue, out var limit))
            {
                maxLimit = maxLimit.HasValue ? Math.Max(maxLimit.Value, limit) : limit;
            }
        }

        return hasUnlimitedTenantGrant ? null : maxLimit;
    }

    private static bool TryParseLimit(string? scopeValue, out decimal limit)
    {
        limit = 0m;
        return !string.IsNullOrWhiteSpace(scopeValue)
            && decimal.TryParse(scopeValue.Trim(), out limit)
            && limit >= 0m;
    }

    private static ProcurementApprovalAuthorityResponse EmptyAuthority(Guid personId, Guid? externalUserId) =>
        new(
            personId,
            externalUserId,
            DateTimeOffset.UtcNow,
            false,
            false,
            false,
            null,
            null,
            null,
            [],
            []);
}
