using System.Security.Claims;
using StaffArr.Api.Contracts;
using StaffArr.Api.Entities;
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

    public void RequireSelfServicePortalAccess(ClaimsPrincipal principal)
    {
        RequireStaffArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (principal.GetPersonId() == Guid.Empty)
        {
            throw new StlApiException(
                "auth.forbidden",
                "Self-service portal requires a linked workforce person record.",
                403);
        }
    }

    public void RequireManagerTeamAccess(ClaimsPrincipal principal) =>
        RequireSelfServicePortalAccess(principal);

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

    public void RequirePersonTimelineRead(ClaimsPrincipal principal, Guid personId)
    {
        RequirePersonHistoryRead(principal, personId);
    }

    public void RequirePersonHistoryRead(ClaimsPrincipal principal, Guid personId)
    {
        RequireStaffArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (principal.GetPersonId() == personId)
        {
            return;
        }

        RequirePeopleRead(principal);
    }

    public void RequirePersonLookupRead(ClaimsPrincipal principal, Guid personId)
    {
        RequireStaffArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (principal.GetPersonId() == personId)
        {
            return;
        }

        RequirePeopleRead(principal);
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

    public void RequireWorkerAdminSettingsManage(ClaimsPrincipal principal) => RequirePeopleWrite(principal);

    public void RequireTenantSettingsView(ClaimsPrincipal principal) => RequirePeopleWrite(principal);

    public void RequireTenantSettingsManage(ClaimsPrincipal principal) => RequirePeopleWrite(principal);

    public void RequireOrganizationRead(ClaimsPrincipal principal, EffectivePermissionProjectionResponse? projection = null)
    {
        RequireScopedPermission(
            principal,
            projection,
            allowSupervisor: true,
            failureMessage: "Organization structure read access requires staffarr.organization.read scope.",
            "staffarr.organization.read",
            "staffarr.organization.manage",
            "staffarr.org.read",
            "staffarr.org.manage");
    }

    public void RequireOrganizationCreate(ClaimsPrincipal principal, EffectivePermissionProjectionResponse? projection = null)
    {
        RequireScopedPermission(
            principal,
            projection,
            allowSupervisor: false,
            failureMessage: "Organization structure create access requires staffarr.organization.create scope.",
            "staffarr.organization.create",
            "staffarr.organization.manage",
            "staffarr.org.manage");
    }

    public void RequireOrganizationUpdate(ClaimsPrincipal principal, EffectivePermissionProjectionResponse? projection = null)
    {
        RequireScopedPermission(
            principal,
            projection,
            allowSupervisor: false,
            failureMessage: "Organization structure update access requires staffarr.organization.update scope.",
            "staffarr.organization.update",
            "staffarr.organization.manage",
            "staffarr.org.manage");
    }

    public void RequireOrganizationArchive(ClaimsPrincipal principal, EffectivePermissionProjectionResponse? projection = null)
    {
        RequireScopedPermission(
            principal,
            projection,
            allowSupervisor: false,
            failureMessage: "Organization structure archive access requires staffarr.organization.archive scope.",
            "staffarr.organization.archive",
            "staffarr.organization.manage",
            "staffarr.org.manage");
    }

    public void RequireSiteRead(ClaimsPrincipal principal, EffectivePermissionProjectionResponse? projection = null)
    {
        RequireScopedPermission(
            principal,
            projection,
            allowSupervisor: true,
            failureMessage: "Site access requires staffarr.sites.read scope.",
            "staffarr.sites.read",
            "staffarr.sites.manage");
    }

    public void RequireSiteCreate(ClaimsPrincipal principal, EffectivePermissionProjectionResponse? projection = null)
    {
        RequireScopedPermission(
            principal,
            projection,
            allowSupervisor: false,
            failureMessage: "Site create access requires staffarr.sites.create scope.",
            "staffarr.sites.create",
            "staffarr.sites.manage");
    }

    public void RequireSiteUpdate(ClaimsPrincipal principal, EffectivePermissionProjectionResponse? projection = null)
    {
        RequireScopedPermission(
            principal,
            projection,
            allowSupervisor: false,
            failureMessage: "Site update access requires staffarr.sites.update scope.",
            "staffarr.sites.update",
            "staffarr.sites.manage");
    }

    public void RequireSiteArchive(ClaimsPrincipal principal, EffectivePermissionProjectionResponse? projection = null)
    {
        RequireScopedPermission(
            principal,
            projection,
            allowSupervisor: false,
            failureMessage: "Site archive access requires staffarr.sites.archive scope.",
            "staffarr.sites.archive",
            "staffarr.sites.manage");
    }

    public void RequireLocationRead(ClaimsPrincipal principal, EffectivePermissionProjectionResponse? projection = null)
    {
        RequireScopedPermission(
            principal,
            projection,
            allowSupervisor: true,
            failureMessage: "Location access requires staffarr.locations.read scope.",
            "staffarr.locations.read",
            "staffarr.locations.manage");
    }

    public void RequireLocationCreate(ClaimsPrincipal principal, EffectivePermissionProjectionResponse? projection = null)
    {
        RequireScopedPermission(
            principal,
            projection,
            allowSupervisor: false,
            failureMessage: "Location create access requires staffarr.locations.create scope.",
            "staffarr.locations.create",
            "staffarr.locations.manage");
    }

    public void RequireLocationUpdate(ClaimsPrincipal principal, EffectivePermissionProjectionResponse? projection = null)
    {
        RequireScopedPermission(
            principal,
            projection,
            allowSupervisor: false,
            failureMessage: "Location update access requires staffarr.locations.update scope.",
            "staffarr.locations.update",
            "staffarr.locations.manage");
    }

    public void RequireLocationArchive(ClaimsPrincipal principal, EffectivePermissionProjectionResponse? projection = null)
    {
        RequireScopedPermission(
            principal,
            projection,
            allowSupervisor: false,
            failureMessage: "Location archive access requires staffarr.locations.archive scope.",
            "staffarr.locations.archive",
            "staffarr.locations.manage");
    }

    public bool CanReadByRole(string roleKey) =>
        MatchesRole(roleKey, "platform_admin", "tenant_admin", "staffarr_admin", "hr_admin", "supervisor");

    public void RequireRoleRead(ClaimsPrincipal principal)
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
            "Role read access requires staffarr.roles.read scope.",
            403);
    }

    public void RequireRoleWrite(ClaimsPrincipal principal)
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
            "Role write access requires staffarr.roles.manage scope.",
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

    public void RequireCertificationRead(ClaimsPrincipal principal, Guid? personId = null)
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

        if (personId is Guid requestedPersonId
            && MatchesRole(roleKey, "tenant_member")
            && principal.GetPersonId() == requestedPersonId)
        {
            return;
        }

        if (personId is null && MatchesRole(roleKey, "tenant_member"))
        {
            throw new StlApiException(
                "auth.forbidden",
                "Certification catalog read access requires staffarr.certifications.read scope.",
                403);
        }

        throw new StlApiException(
            "auth.forbidden",
            "Certification read access requires staffarr.certifications.read scope.",
            403);
    }

    public void RequireCertificationManageWrite(ClaimsPrincipal principal)
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
            "Certification management requires staffarr.certifications.manage scope.",
            403);
    }

    public void RequireReadinessRead(ClaimsPrincipal principal, Guid personId) =>
        RequireCertificationRead(principal, personId);

    public void RequireReadinessRollupRead(ClaimsPrincipal principal) =>
        RequireCertificationRead(principal);

    public void RequireReadinessOverrideWrite(ClaimsPrincipal principal)
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
            "Readiness override requires staffarr.readiness.override scope.",
            403);
    }

    public void RequireIncidentsRead(ClaimsPrincipal principal, Guid? personId = null)
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

        if (personId is Guid requestedPersonId
            && MatchesRole(roleKey, "tenant_member")
            && principal.GetPersonId() == requestedPersonId)
        {
            return;
        }

        if (personId is null && MatchesRole(roleKey, "tenant_member"))
        {
            throw new StlApiException(
                "auth.forbidden",
                "Incident list access requires staffarr.incidents.manage scope or a personId filter for self.",
                403);
        }

        throw new StlApiException(
            "auth.forbidden",
            "Incident read access requires staffarr.incidents.manage scope.",
            403);
    }

    public void RequireIncidentsManageWrite(ClaimsPrincipal principal)
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
            "Incident intake requires staffarr.incidents.manage scope.",
            403);
    }

    public void RequireTrainingAcknowledgementRead(ClaimsPrincipal principal, Guid? personId)
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

        if (personId is Guid requestedPersonId
            && MatchesRole(roleKey, "tenant_member")
            && principal.GetPersonId() == requestedPersonId)
        {
            return;
        }

        if (personId is null
            && MatchesRole(roleKey, "tenant_member")
            && principal.GetPersonId() is Guid selfPersonId)
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Training acknowledgement read requires supervisor access or your own person record.",
            403);
    }

    public void RequireTrainingAcknowledgementAcknowledge(ClaimsPrincipal principal, Guid personId)
    {
        RequireStaffArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_member")
            && principal.GetPersonId() == personId)
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "You may only acknowledge training assignments assigned to your own person record.",
            403);
    }

    public void RequireAuditTimelineRead(ClaimsPrincipal principal)
    {
        RequirePeopleRead(principal);
    }

    public void RequireAuditPackageRead(ClaimsPrincipal principal)
    {
        RequireAuditTimelineRead(principal);
    }

    public void RequireAuditPackageExport(ClaimsPrincipal principal)
    {
        RequireEntityExport(principal);
    }

    public void RequireEntityExport(ClaimsPrincipal principal)
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
            "Bulk entity export requires tenant admin, StaffArr admin, or HR admin access.",
            403);
    }

    public void RequireTimekeepingRead(ClaimsPrincipal principal)
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

        if (principal.GetPersonId() != Guid.Empty)
        {
            return;
        }

        throw new StlApiException("auth.forbidden", "Timekeeping read access requires StaffArr team visibility.", 403);
    }

    public void RequireTimekeepingManage(ClaimsPrincipal principal)
    {
        RequireStaffArrEntitlement(principal);
        if (principal.IsPlatformAdmin() || MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "staffarr_admin", "hr_admin", "supervisor"))
        {
            return;
        }

        throw new StlApiException("auth.forbidden", "Timekeeping manage access requires supervisor or admin access.", 403);
    }

    public void RequireTimekeepingClock(ClaimsPrincipal principal)
    {
        RequireStaffArrEntitlement(principal);
        if (principal.IsPlatformAdmin() || principal.GetPersonId() != Guid.Empty || MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "staffarr_admin", "hr_admin", "supervisor"))
        {
            return;
        }

        throw new StlApiException("auth.forbidden", "Clock access requires a linked worker or StaffArr admin access.", 403);
    }

    public void RequireTimekeepingManualEntry(ClaimsPrincipal principal) => RequireTimekeepingManage(principal);

    public void RequireTimekeepingCorrect(ClaimsPrincipal principal) => RequireTimekeepingManage(principal);

    public void RequireTimekeepingApprove(ClaimsPrincipal principal)
    {
        RequireStaffArrEntitlement(principal);
        if (principal.IsPlatformAdmin() || MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "staffarr_admin", "hr_admin", "supervisor"))
        {
            return;
        }

        throw new StlApiException("auth.forbidden", "Timekeeping approval requires supervisor or admin access.", 403);
    }

    public void RequireTimekeepingPayrollReady(ClaimsPrincipal principal) => RequireTimekeepingApprove(principal);

    public void RequireTimekeepingAdmin(ClaimsPrincipal principal)
    {
        RequireStaffArrEntitlement(principal);
        if (principal.IsPlatformAdmin() || MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "staffarr_admin", "hr_admin"))
        {
            return;
        }

        throw new StlApiException("auth.forbidden", "Timekeeping administration requires StaffArr admin access.", 403);
    }

    public void RequirePersonnelNotesRead(ClaimsPrincipal principal, Guid personId)
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

        if (principal.GetPersonId() == personId)
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Personnel notes read access requires staffarr.notes.read scope.",
            403);
    }

    public void RequirePersonnelNotesManageWrite(ClaimsPrincipal principal)
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
            "Personnel note creation requires staffarr.notes.manage scope.",
            403);
    }

    public bool CanViewPersonnelNote(ClaimsPrincipal principal, Guid personId, PersonnelNote note)
    {
        if (principal.IsPlatformAdmin())
        {
            return true;
        }

        var roleKey = principal.GetTenantRoleKey();
        if (MatchesRole(roleKey, "tenant_admin", "staffarr_admin", "hr_admin"))
        {
            return true;
        }

        if (string.Equals(note.VisibilityKey, "hr_only", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.Equals(note.VisibilityKey, "management", StringComparison.OrdinalIgnoreCase))
        {
            return MatchesRole(roleKey, "supervisor");
        }

        if (string.Equals(note.VisibilityKey, "personnel_visible", StringComparison.OrdinalIgnoreCase))
        {
            return MatchesRole(roleKey, "supervisor") || principal.GetPersonId() == personId;
        }

        return false;
    }

    public void RequirePersonnelDocumentsRead(ClaimsPrincipal principal, Guid personId)
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

        if (principal.GetPersonId() == personId)
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Personnel documents read access requires staffarr.documents.read scope.",
            403);
    }

    public void RequirePersonnelDocumentsManageWrite(ClaimsPrincipal principal)
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
            "Personnel document upload requires staffarr.documents.manage scope.",
            403);
    }

    private void RequireScopedPermission(
        ClaimsPrincipal principal,
        EffectivePermissionProjectionResponse? projection,
        bool allowSupervisor,
        string failureMessage,
        params string[] permissionKeys)
    {
        RequireStaffArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (HasAnyPermission(projection, permissionKeys))
        {
            return;
        }

        var roleKey = principal.GetTenantRoleKey();
        if (allowSupervisor)
        {
            if (MatchesRole(roleKey, "tenant_admin", "staffarr_admin", "hr_admin", "supervisor"))
            {
                return;
            }
        }
        else if (MatchesRole(roleKey, "tenant_admin", "staffarr_admin", "hr_admin"))
        {
            return;
        }

        throw new StlApiException("auth.forbidden", failureMessage, 403);
    }

    private static bool HasAnyPermission(
        EffectivePermissionProjectionResponse? projection,
        IReadOnlyCollection<string> permissionKeys)
    {
        if (projection is null || projection.Permissions.Count == 0)
        {
            return false;
        }

        return projection.Permissions.Any(permission =>
            permissionKeys.Any(key => string.Equals(permission.PermissionKey, key, StringComparison.OrdinalIgnoreCase)));
    }

    public async Task RequirePersonnelUpdateRequestReviewAsync(
        ClaimsPrincipal principal,
        Guid subjectPersonId,
        ManagerHierarchyService managerHierarchy,
        CancellationToken cancellationToken = default)
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

        var reviewerPersonId = principal.GetPersonId();
        if (reviewerPersonId == Guid.Empty)
        {
            throw new StlApiException(
                "auth.forbidden",
                "Personnel update review requires HR access or a linked manager person record.",
                403);
        }

        var isDirectManager = await managerHierarchy.IsDirectManagerOfAsync(
            principal.GetTenantId(),
            reviewerPersonId,
            subjectPersonId,
            cancellationToken);
        if (!isDirectManager)
        {
            throw new StlApiException(
                "auth.forbidden",
                "Personnel update review requires HR access or direct manager responsibility for the requester.",
                403);
        }
    }

    private static bool CanWriteByRole(string roleKey) =>
        MatchesRole(roleKey, "platform_admin", "tenant_admin", "staffarr_admin", "hr_admin");

    private static bool MatchesRole(string roleKey, params string[] candidates) =>
        candidates.Any(candidate => string.Equals(roleKey, candidate, StringComparison.OrdinalIgnoreCase));
}
