using System.Security.Claims;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace AssurArr.Api.Services;

public sealed class AssurArrAuthorizationService
{
    public void RequireQualityRead(ClaimsPrincipal principal)
    {
        RequireAssurArrAccess(principal);
        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "assurarr_admin",
                "assurarr_manager",
                "quality_manager",
                "quality_supervisor"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "AssurArr access requires quality or AssurArr manager access.",
            403);
    }

    public void RequireQualityManage(ClaimsPrincipal principal)
    {
        RequireAssurArrAccess(principal);
        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "assurarr_admin",
                "assurarr_manager",
                "quality_manager"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "AssurArr changes require quality or AssurArr manager access.",
            403);
    }

    private static void RequireAssurArrAccess(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
        }
    }

    private static bool MatchesRole(string roleKey, params string[] candidates) =>
        candidates.Any(candidate => string.Equals(roleKey, candidate, StringComparison.OrdinalIgnoreCase));
}
