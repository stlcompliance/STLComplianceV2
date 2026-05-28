using System.Security.Claims;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class MaintainArrAuthorizationService
{
    public void RequireAuthenticated(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
        }
    }

    public void RequireMaintainArrEntitlement(ClaimsPrincipal principal)
    {
        RequireAuthenticated(principal);
        if (!principal.HasProductEntitlement("maintainarr"))
        {
            throw new StlApiException("auth.not_entitled", "MaintainArr entitlement is required.", 403);
        }
    }

    public void RequireAssetsRead(ClaimsPrincipal principal)
    {
        RequireMaintainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "maintainarr_admin",
                "maintainarr_manager",
                "maintainarr_technician",
                "tenant_member"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Asset read access requires MaintainArr entitlement.",
            403);
    }

    public void RequireAssetsManage(ClaimsPrincipal principal)
    {
        RequireMaintainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "maintainarr_admin",
                "maintainarr_manager"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Asset management requires maintainarr.assets.create scope.",
            403);
    }

    public void RequirePmRead(ClaimsPrincipal principal)
    {
        RequireMaintainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "maintainarr_admin",
                "maintainarr_manager",
                "maintainarr_technician",
                "tenant_member"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Preventive maintenance read access requires MaintainArr entitlement.",
            403);
    }

    public void RequirePmManage(ClaimsPrincipal principal)
    {
        RequireMaintainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "maintainarr_admin",
                "maintainarr_manager"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Preventive maintenance management requires maintainarr.pm.manage scope.",
            403);
    }

    public void RequireInspectionsRead(ClaimsPrincipal principal)
    {
        RequireMaintainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "maintainarr_admin",
                "maintainarr_manager",
                "maintainarr_technician",
                "tenant_member"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Inspection template read access requires MaintainArr entitlement.",
            403);
    }

    public void RequireInspectionsManage(ClaimsPrincipal principal)
    {
        RequireMaintainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "maintainarr_admin",
                "maintainarr_manager"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Inspection template management requires maintainarr.inspections.manage scope.",
            403);
    }

    public void RequireInspectionsExecute(ClaimsPrincipal principal) => RequireInspectionsRead(principal);

    public bool CanViewAllInspectionRuns(ClaimsPrincipal principal)
    {
        RequireMaintainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return true;
        }

        return MatchesRole(
            principal.GetTenantRoleKey(),
            "tenant_admin",
            "maintainarr_admin",
            "maintainarr_manager");
    }

    public void RequireInspectionRunAccess(
        ClaimsPrincipal principal,
        Guid runStartedByUserId)
    {
        RequireInspectionsRead(principal);
        if (CanViewAllInspectionRuns(principal))
        {
            return;
        }

        var actorUserId = principal.GetUserId();
        if (runStartedByUserId != actorUserId)
        {
            throw new StlApiException(
                "auth.forbidden",
                "You can only access inspection runs you started.",
                403);
        }
    }

    public void RequireDefectsRead(ClaimsPrincipal principal) => RequireInspectionsRead(principal);

    public void RequireDefectsCreate(ClaimsPrincipal principal) => RequireInspectionsExecute(principal);

    public bool CanViewAllDefects(ClaimsPrincipal principal) => CanViewAllInspectionRuns(principal);

    public void RequireDefectAccess(ClaimsPrincipal principal, Guid reportedByUserId)
    {
        RequireDefectsRead(principal);
        if (CanViewAllDefects(principal))
        {
            return;
        }

        var actorUserId = principal.GetUserId();
        if (reportedByUserId != actorUserId)
        {
            throw new StlApiException(
                "auth.forbidden",
                "You can only access defects you reported.",
                403);
        }
    }

    public void RequireDefectsStatusManage(ClaimsPrincipal principal) => RequireInspectionsManage(principal);

    public void RequireMetersRead(ClaimsPrincipal principal) => RequirePmRead(principal);

    public void RequireMetersManage(ClaimsPrincipal principal) => RequireAssetsManage(principal);

    public void RequireMetersRecord(ClaimsPrincipal principal) => RequireMetersRead(principal);

    public void RequireWorkOrdersRead(ClaimsPrincipal principal) => RequirePmRead(principal);

    public void RequireWorkOrdersCreate(ClaimsPrincipal principal) => RequireInspectionsExecute(principal);

    public void RequireWorkOrdersPerform(ClaimsPrincipal principal) => RequireInspectionsExecute(principal);

    public void RequireWorkOrdersClose(ClaimsPrincipal principal) => RequireInspectionsManage(principal);

    public bool CanViewAllWorkOrders(ClaimsPrincipal principal) => CanViewAllDefects(principal);

    public bool CanCloseAnyWorkOrder(ClaimsPrincipal principal) => CanViewAllWorkOrders(principal);

    public void RequireWorkOrderAccess(
        ClaimsPrincipal principal,
        Guid createdByUserId,
        string? assignedTechnicianPersonId)
    {
        RequireWorkOrdersRead(principal);
        if (CanViewAllWorkOrders(principal))
        {
            return;
        }

        var actorUserId = principal.GetUserId();
        var actorPersonId = principal.GetPersonId().ToString();
        if (createdByUserId == actorUserId)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(assignedTechnicianPersonId)
            && string.Equals(assignedTechnicianPersonId, actorPersonId, StringComparison.Ordinal))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "You can only access work orders you created or are assigned to.",
            403);
    }

    public void RequireMaintenanceHistoryRead(ClaimsPrincipal principal) => RequireAssetsRead(principal);

    public void RequireAssetReadinessRead(ClaimsPrincipal principal) => RequireAssetsRead(principal);

    public void RequireAuditPackageRead(ClaimsPrincipal principal) => RequireAssetsRead(principal);

    public void RequireAuditPackageExport(ClaimsPrincipal principal)
    {
        RequireMaintainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "maintainarr_admin",
                "maintainarr_manager"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Audit package export requires MaintainArr administrator access.",
            403);
    }

    public void RequireNotificationSettingsManage(ClaimsPrincipal principal)
    {
        RequireMaintainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "maintainarr_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Maintenance notification settings require maintainarr admin access.",
            403);
    }

    public void RequireDefectEscalationSettingsManage(ClaimsPrincipal principal)
    {
        RequireNotificationSettingsManage(principal);
    }

    public void RequireAssetStatusRollupSettingsManage(ClaimsPrincipal principal)
    {
        RequireNotificationSettingsManage(principal);
    }

    public void RequireMaintenanceHistoryRollupSettingsManage(ClaimsPrincipal principal)
    {
        RequireNotificationSettingsManage(principal);
    }

    public void RequirePmDueScanSettingsManage(ClaimsPrincipal principal)
    {
        RequireNotificationSettingsManage(principal);
    }

    public void RequireAssetStatusRollupRead(ClaimsPrincipal principal) => RequireAssetsRead(principal);

    public void RequireMaintenanceReportRead(ClaimsPrincipal principal) => RequireAssetsRead(principal);

    public void RequireMaintenanceReportExport(ClaimsPrincipal principal) => RequireAuditPackageExport(principal);

    public void RequireExecutiveReportRead(ClaimsPrincipal principal)
    {
        RequireMaintainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "maintainarr_admin",
                "maintainarr_manager"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Executive report access requires MaintainArr manager or administrator role.",
            403);
    }

    public void RequireExecutiveReportExport(ClaimsPrincipal principal) => RequireAuditPackageExport(principal);

    public void RequireComplianceReportRead(ClaimsPrincipal principal) => RequireExecutiveReportRead(principal);

    public void RequireComplianceReportExport(ClaimsPrincipal principal) => RequireAuditPackageExport(principal);

    public void RequireAssetImportManage(ClaimsPrincipal principal) => RequireAssetsManage(principal);

    public void RequireEntityExport(ClaimsPrincipal principal) => RequireAuditPackageExport(principal);

    private static bool MatchesRole(string roleKey, params string[] candidates) =>
        candidates.Any(candidate => string.Equals(roleKey, candidate, StringComparison.OrdinalIgnoreCase));
}
