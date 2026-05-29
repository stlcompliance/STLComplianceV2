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

    public void RequireAvailabilitySnapshotSettingsManage(ClaimsPrincipal principal) =>
        RequireNotificationSettingsManage(principal);

    public void RequireProcurementCoordinationSettingsManage(ClaimsPrincipal principal) =>
        RequireNotificationSettingsManage(principal);

    public void RequireProcurementCoordinationRead(ClaimsPrincipal principal) =>
        RequirePurchaseRequestRead(principal);

    public void RequireApprovalReminderSettingsManage(ClaimsPrincipal principal) =>
        RequireNotificationSettingsManage(principal);

    public void RequireApprovalReminderRead(ClaimsPrincipal principal) =>
        RequirePurchaseRequestRead(principal);

    public void RequireProcurementExceptionEscalationSettingsManage(ClaimsPrincipal principal) =>
        RequireNotificationSettingsManage(principal);

    public void RequireDemandProcessingSettingsManage(ClaimsPrincipal principal) =>
        RequireNotificationSettingsManage(principal);

    public void RequireDemandProcessingRead(ClaimsPrincipal principal) =>
        RequireDemandRefRead(principal);

    public void RequireDemandProcessingOperate(ClaimsPrincipal principal) =>
        RequirePurchaseRequestCreate(principal);

    public void RequireIntegrationEventSettingsManage(ClaimsPrincipal principal) =>
        RequireNotificationSettingsManage(principal);

    public void RequireRfqRead(ClaimsPrincipal principal) =>
        RequirePurchaseRequestRead(principal);

    public void RequireRfqManage(ClaimsPrincipal principal) =>
        RequirePurchaseRequestCreate(principal);

    public void RequireRfqAward(ClaimsPrincipal principal) =>
        RequirePurchaseRequestApprove(principal);

    public void RequireSupplierOnboardingRead(ClaimsPrincipal principal) =>
        RequirePartiesRead(principal);

    public void RequireSupplierOnboardingManage(ClaimsPrincipal principal) =>
        RequirePartiesManage(principal);

    public void RequireSupplierOnboardingReview(ClaimsPrincipal principal) =>
        RequirePurchaseRequestApprove(principal);

    public void RequireEmergencyPurchaseRead(ClaimsPrincipal principal) =>
        RequirePurchaseRequestRead(principal);

    public void RequireEmergencyPurchaseCreate(ClaimsPrincipal principal)
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
            "Emergency purchase creation requires manager or administrator role.",
            403);
    }

    public void RequireEmergencyPurchaseExpedite(ClaimsPrincipal principal) =>
        RequireEmergencyPurchaseCreate(principal);

    public void RequireEmergencyPurchaseOverrideApprove(ClaimsPrincipal principal)
    {
        RequireSupplyArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "supplyarr_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Emergency purchase manager override requires tenant or SupplyArr administrator role.",
            403);
    }

    public void RequireEmergencyPurchaseIssueOrder(ClaimsPrincipal principal) =>
        RequireEmergencyPurchaseCreate(principal);

    public void RequireVendorReportRead(ClaimsPrincipal principal) =>
        RequirePartiesRead(principal);

    public void RequireVendorReportExport(ClaimsPrincipal principal) =>
        RequirePurchaseRequestRead(principal);

    public void RequirePartsInventoryReportRead(ClaimsPrincipal principal) =>
        RequireInventoryRead(principal);

    public void RequirePartsInventoryReportExport(ClaimsPrincipal principal) =>
        RequireInventoryRead(principal);

    public void RequirePurchasingReportRead(ClaimsPrincipal principal) =>
        RequirePurchaseRequestRead(principal);

    public void RequirePurchasingReportExport(ClaimsPrincipal principal) =>
        RequirePurchaseRequestRead(principal);

    public void RequireComplianceReportRead(ClaimsPrincipal principal) =>
        RequirePartiesRead(principal);

    public void RequireComplianceReportExport(ClaimsPrincipal principal) =>
        RequirePartiesRead(principal);

    public void RequireForgivingSearch(ClaimsPrincipal principal) =>
        RequirePartiesRead(principal);

    public void RequireAuditHistoryRead(ClaimsPrincipal principal)
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
            "Audit history read requires SupplyArr admin or manager access.",
            403);
    }

    public void RequireSupplyReadinessRead(ClaimsPrincipal principal)
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
                "supplyarr_buyer",
                "tenant_member"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Supply readiness dashboard requires SupplyArr read access.",
            403);
    }

    private static bool MatchesRole(string roleKey, params string[] candidates) =>
        candidates.Any(candidate => string.Equals(roleKey, candidate, StringComparison.OrdinalIgnoreCase));
}
