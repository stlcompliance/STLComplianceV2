using System.Security.Claims;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public static class CompanionNotificationAuthorization
{
    public static void RequireNotificationSettingsManage(ClaimsPrincipal principal)
    {
        CompanionFieldInboxService.RequireCompanionAccess(principal);

        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (PlatformAuthorizationService.IsTenantAdminRole(principal.GetTenantRoleKey()))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Companion notification settings require tenant administrator access.",
            403);
    }
}
