using System.Security.Claims;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class SupplyArrAuthorizationService
{
    public void RequireAuthenticated(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
        }
    }

    public void RequireSupplyArrEntitlement(ClaimsPrincipal principal)
    {
        RequireAuthenticated(principal);
        if (!principal.HasProductEntitlement("supplyarr"))
        {
            throw new StlApiException("auth.not_entitled", "SupplyArr entitlement is required.", 403);
        }
    }

    public void RequirePartiesRead(ClaimsPrincipal principal)
    {
        RequireSupplyArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "supplyarr_admin",
                "supplyarr_manager",
                "supplyarr_clerk",
                "tenant_member"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Party read access requires SupplyArr entitlement.",
            403);
    }

    public void RequirePartiesManage(ClaimsPrincipal principal)
    {
        RequireSupplyArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "supplyarr_admin",
                "supplyarr_manager"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Party management requires supplyarr.parties.manage scope.",
            403);
    }

    public void RequirePartsRead(ClaimsPrincipal principal)
    {
        RequireSupplyArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "supplyarr_admin",
                "supplyarr_manager",
                "supplyarr_clerk",
                "tenant_member"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Part read access requires SupplyArr entitlement.",
            403);
    }

    public void RequirePartsManage(ClaimsPrincipal principal)
    {
        RequireSupplyArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "supplyarr_admin",
                "supplyarr_manager"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Part management requires supplyarr.parts.manage scope.",
            403);
    }

    public void RequireInventoryRead(ClaimsPrincipal principal)
    {
        RequireSupplyArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "supplyarr_admin",
                "supplyarr_manager",
                "supplyarr_clerk",
                "tenant_member"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Inventory read access requires SupplyArr entitlement.",
            403);
    }

    public void RequireInventoryManage(ClaimsPrincipal principal)
    {
        RequireSupplyArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "supplyarr_admin",
                "supplyarr_manager"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Inventory management requires supplyarr.inventory.manage scope.",
            403);
    }

    public void RequirePurchaseRequestRead(ClaimsPrincipal principal)
    {
        RequireSupplyArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "supplyarr_admin",
                "supplyarr_manager",
                "supplyarr_buyer",
                "supplyarr_clerk",
                "tenant_member"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Purchase request read access requires SupplyArr entitlement.",
            403);
    }

    public void RequirePurchaseRequestCreate(ClaimsPrincipal principal)
    {
        RequireSupplyArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "supplyarr_admin",
                "supplyarr_manager",
                "supplyarr_buyer"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Purchase request creation requires supplyarr.purchaseRequests.create scope.",
            403);
    }

    public void RequirePurchaseRequestApprove(ClaimsPrincipal principal)
    {
        RequireSupplyArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "supplyarr_admin",
                "supplyarr_manager"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Purchase request approval requires supplyarr.purchaseRequests.approve scope.",
            403);
    }

    public void RequireDemandRefRead(ClaimsPrincipal principal)
    {
        RequirePurchaseRequestRead(principal);
    }

    public void RequirePurchaseOrderRead(ClaimsPrincipal principal)
    {
        RequirePurchaseRequestRead(principal);
    }

    public void RequirePurchaseOrderCreate(ClaimsPrincipal principal)
    {
        RequirePurchaseRequestCreate(principal);
    }

    public void RequirePurchaseOrderApprove(ClaimsPrincipal principal)
    {
        RequirePurchaseRequestApprove(principal);
    }

    public void RequireReceivingRead(ClaimsPrincipal principal)
    {
        RequireInventoryRead(principal);
    }

    public void RequireReceivingPerform(ClaimsPrincipal principal)
    {
        RequireSupplyArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "supplyarr_admin",
                "supplyarr_manager",
                "supplyarr_clerk"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Receiving requires supplyarr.receiving.perform scope.",
            403);
    }

    public void RequireBackorderRead(ClaimsPrincipal principal) =>
        RequirePurchaseOrderRead(principal);

    public void RequireBackorderManage(ClaimsPrincipal principal) =>
        RequireReceivingPerform(principal);

    public void RequireReturnRead(ClaimsPrincipal principal) =>
        RequirePurchaseOrderRead(principal);

    public void RequireReturnManage(ClaimsPrincipal principal) =>
        RequireReceivingPerform(principal);

    public void RequireNotificationSettingsManage(ClaimsPrincipal principal)
    {
        RequireSupplyArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "supplyarr_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Procurement notification settings require SupplyArr admin access.",
            403);
    }

    public void RequirePriceSnapshotSettingsManage(ClaimsPrincipal principal) =>
        RequireNotificationSettingsManage(principal);

    public void RequireLeadTimeSnapshotSettingsManage(ClaimsPrincipal principal) =>
        RequireNotificationSettingsManage(principal);

    private static bool MatchesRole(string roleKey, params string[] candidates) =>
        candidates.Any(candidate => string.Equals(roleKey, candidate, StringComparison.OrdinalIgnoreCase));
}
