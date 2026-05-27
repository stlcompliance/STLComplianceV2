using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class PlatformAuthorizationService(NexArrDbContext db)
{
    public async Task RequirePlatformAdminAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        if (!principal.IsPlatformAdmin())
        {
            throw new StlApiException("auth.forbidden", "Platform administrator access is required.", 403);
        }

        var userId = principal.GetUserId();
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);
        if (user is null)
        {
            throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
        }
    }

    public async Task RequireNexArrAccessAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        if (principal.IsPlatformAdmin() || principal.HasProductEntitlement("nexarr"))
        {
            return;
        }

        throw new StlApiException("auth.forbidden", "NexArr entitlement is required.", 403);
    }

    public async Task RequireTenantAccessAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        bool allowTenantAdmin = false,
        CancellationToken cancellationToken = default)
    {
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        var jwtTenantId = principal.GetTenantId();
        if (jwtTenantId != tenantId)
        {
            throw new StlApiException("auth.tenant_forbidden", "Access to the requested tenant is forbidden.", 403);
        }

        if (!allowTenantAdmin)
        {
            throw new StlApiException("auth.forbidden", "Platform administrator access is required.", 403);
        }

        var userId = principal.GetUserId();
        var membership = await db.TenantMemberships.AsNoTracking()
            .FirstOrDefaultAsync(
                m => m.UserId == userId && m.TenantId == tenantId && m.IsActive,
                cancellationToken);

        if (membership is null || !IsTenantAdminRole(membership.RoleKey))
        {
            throw new StlApiException("auth.forbidden", "Tenant administrator access is required.", 403);
        }
    }

    public static bool IsTenantAdminRole(string roleKey) =>
        string.Equals(roleKey, "tenant_admin", StringComparison.OrdinalIgnoreCase)
        || string.Equals(roleKey, "platform_admin", StringComparison.OrdinalIgnoreCase);

    public async Task RequireProductLaunchAsync(
        ClaimsPrincipal principal,
        string productKey,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        if (principal.IsPlatformAdmin() || principal.HasProductEntitlement(productKey))
        {
            if (principal.IsPlatformAdmin())
            {
                return;
            }

            var jwtTenantId = principal.GetTenantId();
            if (jwtTenantId != tenantId)
            {
                throw new StlApiException("auth.tenant_forbidden", "Access to the requested tenant is forbidden.", 403);
            }

            return;
        }

        throw new StlApiException("auth.forbidden", "Product entitlement is required to launch this product.", 403);
    }
}
