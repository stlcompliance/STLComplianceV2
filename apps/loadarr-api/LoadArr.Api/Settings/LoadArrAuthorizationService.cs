using System.Security.Claims;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace LoadArr.Api.Settings;

public sealed class LoadArrAuthorizationService
{
    public void RequireTenantSettingsRead(ClaimsPrincipal principal)
    {
        RequireLoadArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "loadarr_admin",
                "loadarr_manager",
                "warehouse_manager",
                "warehouse_supervisor"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "LoadArr tenant settings require LoadArr admin or manager access.",
            403);
    }

    public void RequireTenantSettingsUpdate(ClaimsPrincipal principal)
    {
        RequireLoadArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "loadarr_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Updating LoadArr tenant settings requires LoadArr product admin access.",
            403);
    }

    public void RequireTenantSettingsReset(ClaimsPrincipal principal) =>
        RequireTenantSettingsUpdate(principal);

    public void RequireTenantSettingsAuditRead(ClaimsPrincipal principal) =>
        RequireTenantSettingsUpdate(principal);

    private static void RequireLoadArrEntitlement(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
        }

        if (!principal.HasProductEntitlement("loadarr"))
        {
            throw new StlApiException("auth.not_entitled", "LoadArr entitlement is required.", 403);
        }
    }

    private static bool MatchesRole(string roleKey, params string[] candidates) =>
        candidates.Any(candidate => string.Equals(roleKey, candidate, StringComparison.OrdinalIgnoreCase));
}

