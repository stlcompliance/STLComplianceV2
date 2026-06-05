using System.Security.Claims;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public static class FieldCompanionNotificationAuthorization
{
    public static void RequireNotificationSettingsManage(ClaimsPrincipal principal)
    {
        FieldCompanionFieldInboxService.RequireFieldCompanionAccess(principal);

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
            "fieldcompanion notification settings require tenant administrator access.",
            403);
    }
}
