using System.Security.Claims;
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

    public void RequireAuditPackageRead(ClaimsPrincipal principal)
    {
        RequirePeopleRead(principal);
    }

    public void RequireAuditPackageExport(ClaimsPrincipal principal)
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
            "Audit package export requires staffarr.audit.export scope.",
            403);
    }

    public void RequirePersonnelReportRead(ClaimsPrincipal principal) =>
        RequireAuditPackageRead(principal);

    public void RequirePersonnelReportExport(ClaimsPrincipal principal) =>
        RequireAuditPackageExport(principal);

    public void RequireReadinessReportRead(ClaimsPrincipal principal) =>
        RequireAuditPackageRead(principal);

    public void RequireReadinessReportExport(ClaimsPrincipal principal) =>
        RequireAuditPackageExport(principal);

    public void RequireIncidentReportRead(ClaimsPrincipal principal) =>
        RequireAuditPackageRead(principal);

    public void RequireIncidentReportExport(ClaimsPrincipal principal) =>
        RequireAuditPackageExport(principal);

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

    private static bool CanWriteByRole(string roleKey) =>
        MatchesRole(roleKey, "platform_admin", "tenant_admin", "staffarr_admin", "hr_admin");

    private static bool MatchesRole(string roleKey, params string[] candidates) =>
        candidates.Any(candidate => string.Equals(roleKey, candidate, StringComparison.OrdinalIgnoreCase));
}
