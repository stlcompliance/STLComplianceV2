using System.Security.Claims;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace LoadArr.Api.Settings;

public sealed class LoadArrAuthorizationService
{
    public void RequireWorkspaceRead(ClaimsPrincipal principal)
    {
        RequireLoadArrLaunchContext(principal);
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
            "LoadArr workspace access requires warehouse or LoadArr manager access.",
            403);
    }

    public void RequireOperationalWrite(ClaimsPrincipal principal)
    {
        RequireLoadArrLaunchContext(principal);
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
            "LoadArr operational changes require warehouse or LoadArr supervisor access.",
            403);
    }

    public void RequireIntegrationRead(ClaimsPrincipal principal)
    {
        RequireLoadArrLaunchContext(principal);
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
            "LoadArr integration read access requires warehouse or LoadArr manager access.",
            403);
    }

    public void RequireIntegrationManage(ClaimsPrincipal principal)
    {
        RequireLoadArrLaunchContext(principal);
        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "loadarr_admin",
                "loadarr_manager",
                "warehouse_manager"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "LoadArr integration changes require LoadArr or warehouse manager access.",
            403);
    }

    public void RequireTenantSettingsRead(ClaimsPrincipal principal)
    {
        RequireLoadArrLaunchContext(principal);
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
        RequireLoadArrLaunchContext(principal);
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

    private static void RequireLoadArrAccess(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
        }
    }

    private static void RequireLoadArrLaunchContext(ClaimsPrincipal principal) =>
        RequireLoadArrAccess(principal);

    private static bool MatchesRole(string roleKey, params string[] candidates) =>
        candidates.Any(candidate => string.Equals(roleKey, candidate, StringComparison.OrdinalIgnoreCase));
}
