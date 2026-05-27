using System.Security.Claims;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class StaffArrAuthorizationService
{
    public void RequireAuthenticated(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
        }
    }

    public void RequireStaffArrEntitlement(ClaimsPrincipal principal)
    {
        RequireAuthenticated(principal);
        if (!principal.HasProductEntitlement("staffarr"))
        {
            throw new StlApiException("auth.not_entitled", "StaffArr entitlement is required.", 403);
        }
    }

    public void RequirePeopleRead(ClaimsPrincipal principal)
    {
        RequireStaffArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (CanReadByRole(principal.GetTenantRoleKey()))
        {
            return;
        }

        throw new StlApiException("auth.forbidden", "People directory access requires a tenant role with people.read scope.", 403);
    }

    public void RequirePeopleWrite(ClaimsPrincipal principal)
    {
        RequireStaffArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (CanWriteByRole(principal.GetTenantRoleKey()))
        {
            return;
        }

        throw new StlApiException("auth.forbidden", "People write access requires a tenant role with people.write scope.", 403);
    }

    public bool CanReadByRole(string roleKey) =>
        MatchesRole(roleKey, "platform_admin", "tenant_admin", "staffarr_admin", "hr_admin", "supervisor");

    public void RequireRoleTemplateRead(ClaimsPrincipal principal)
    {
        RequireStaffArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "staffarr_admin", "hr_admin", "supervisor"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Role template read access requires staffarr.roles.read scope.",
            403);
    }

    public void RequireRoleTemplateWrite(ClaimsPrincipal principal)
    {
        RequireStaffArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "staffarr_admin", "hr_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Role template write access requires staffarr.roles.manage scope.",
            403);
    }

    public void RequirePermissionProjectionRead(ClaimsPrincipal principal, Guid personId)
    {
        RequireStaffArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        var roleKey = principal.GetTenantRoleKey();
        if (MatchesRole(roleKey, "tenant_admin", "staffarr_admin", "hr_admin", "supervisor"))
        {
            return;
        }

        if (MatchesRole(roleKey, "tenant_member") && principal.GetPersonId() == personId)
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Permission projection read access requires staffarr.permissions.read scope.",
            403);
    }

    private static bool CanWriteByRole(string roleKey) =>
        MatchesRole(roleKey, "platform_admin", "tenant_admin", "staffarr_admin", "hr_admin");

    private static bool MatchesRole(string roleKey, params string[] candidates) =>
        candidates.Any(candidate => string.Equals(roleKey, candidate, StringComparison.OrdinalIgnoreCase));
}
