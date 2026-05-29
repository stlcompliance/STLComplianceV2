using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class PeopleService(
    StaffArrDbContext db,
    IStaffArrAuditService audit,
    StaffArrMaintainArrTechnicianRefSyncService maintainarrTechnicianRefSync)
{
    private static readonly HashSet<string> AllowedEmploymentStatuses =
        new(StringComparer.OrdinalIgnoreCase) { "active", "inactive", "terminated" };

    public async Task<IReadOnlyList<StaffPersonSummaryResponse>> ListAsync(
        Guid tenantId,
        string? query,
        Guid? orgUnitId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = Math.Clamp(limit <= 0 ? 50 : limit, 1, 200);
        var peopleQuery = db.People
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim().ToLowerInvariant();
            peopleQuery = peopleQuery.Where(p =>
                p.DisplayName.ToLower().Contains(term)
                || p.PrimaryEmail.ToLower().Contains(term));
        }

        if (orgUnitId is Guid requestedOrgUnitId)
        {
            peopleQuery = peopleQuery.Where(p => p.PrimaryOrgUnitId == requestedOrgUnitId);
        }

        return await peopleQuery
            .OrderBy(p => p.DisplayName)
            .Take(normalizedLimit)
            .Select(p => new StaffPersonSummaryResponse(
                p.Id,
                p.ExternalUserId,
                p.DisplayName,
                p.PrimaryEmail,
                p.EmploymentStatus,
                p.PrimaryOrgUnitId,
                p.PrimaryOrgUnit != null ? p.PrimaryOrgUnit.Name : null,
                p.ManagerPersonId,
                p.JobTitle))
            .ToListAsync(cancellationToken);
    }

    public async Task<StaffPersonDetailResponse> GetByIdAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        var person = await db.People
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.Id == personId)
            .Select(p => new StaffPersonDetailResponse(
                p.Id,
                p.ExternalUserId,
                p.GivenName,
                p.FamilyName,
                p.DisplayName,
                p.PrimaryEmail,
                p.EmploymentStatus,
                p.PrimaryOrgUnitId,
                p.PrimaryOrgUnit != null ? p.PrimaryOrgUnit.Name : null,
                p.ManagerPersonId,
                p.JobTitle,
                p.CreatedAt,
                p.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return person ?? throw new StlApiException("people.not_found", "Person was not found.", 404);
    }

    public async Task<StaffPersonDetailResponse> CreateAsync(
        Guid tenantId,
        Guid? actorUserId,
        CreateStaffPersonRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidatePersonFields(request.GivenName, request.FamilyName, request.PrimaryEmail, request.JobTitle);
        ValidateEmploymentStatus(request.EmploymentStatus);

        var normalizedEmail = request.PrimaryEmail.Trim().ToLowerInvariant();
        await EnsureEmailAvailableAsync(tenantId, normalizedEmail, null, cancellationToken);
        await EnsurePrimaryOrgUnitExistsAsync(tenantId, request.PrimaryOrgUnitId, cancellationToken);

        var personId = Guid.NewGuid();
        await ValidateManagerReferenceAsync(tenantId, personId, request.ManagerPersonId, cancellationToken);

        var person = new StaffPerson
        {
            Id = personId,
            TenantId = tenantId,
            GivenName = request.GivenName.Trim(),
            FamilyName = request.FamilyName.Trim(),
            DisplayName = BuildDisplayName(request.GivenName, request.FamilyName),
            PrimaryEmail = normalizedEmail,
            EmploymentStatus = request.EmploymentStatus.Trim().ToLowerInvariant(),
            PrimaryOrgUnitId = request.PrimaryOrgUnitId,
            ManagerPersonId = request.ManagerPersonId,
            JobTitle = request.JobTitle?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.People.Add(person);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "person.create",
            tenantId,
            actorUserId,
            "person",
            person.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        await maintainarrTechnicianRefSync.TryPublishPersonChangedAsync(
            person,
            "staffarr.person.created",
            cancellationToken);

        return await GetByIdAsync(tenantId, person.Id, cancellationToken);
    }

    public async Task<StaffPersonDetailResponse> UpdateAsync(
        Guid tenantId,
        Guid personId,
        Guid? actorUserId,
        UpdateStaffPersonRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidatePersonFields(request.GivenName, request.FamilyName, request.PrimaryEmail, request.JobTitle);

        var person = await db.People.FirstOrDefaultAsync(
            p => p.TenantId == tenantId && p.Id == personId,
            cancellationToken);
        if (person is null)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }

        var normalizedEmail = request.PrimaryEmail.Trim().ToLowerInvariant();
        await EnsureEmailAvailableAsync(tenantId, normalizedEmail, personId, cancellationToken);
        await EnsurePrimaryOrgUnitExistsAsync(tenantId, request.PrimaryOrgUnitId, cancellationToken);
        await ValidateManagerReferenceAsync(tenantId, personId, request.ManagerPersonId, cancellationToken);

        person.GivenName = request.GivenName.Trim();
        person.FamilyName = request.FamilyName.Trim();
        person.DisplayName = BuildDisplayName(request.GivenName, request.FamilyName);
        person.PrimaryEmail = normalizedEmail;
        person.PrimaryOrgUnitId = request.PrimaryOrgUnitId;
        person.ManagerPersonId = request.ManagerPersonId;
        person.JobTitle = request.JobTitle?.Trim();
        person.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "person.update",
            tenantId,
            actorUserId,
            "person",
            person.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        await maintainarrTechnicianRefSync.TryPublishPersonChangedAsync(
            person,
            "staffarr.person.updated",
            cancellationToken);

        return await GetByIdAsync(tenantId, person.Id, cancellationToken);
    }

    public async Task<StaffPersonDetailResponse> UpdateEmploymentStatusAsync(
        Guid tenantId,
        Guid personId,
        Guid? actorUserId,
        UpdatePersonEmploymentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateEmploymentStatus(request.EmploymentStatus);

        var person = await db.People.FirstOrDefaultAsync(
            p => p.TenantId == tenantId && p.Id == personId,
            cancellationToken);
        if (person is null)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }

        var normalizedStatus = request.EmploymentStatus.Trim().ToLowerInvariant();
        if (!string.Equals(person.EmploymentStatus, normalizedStatus, StringComparison.OrdinalIgnoreCase)
            && normalizedStatus is not "active")
        {
            await EnsureCanDeactivateAsync(tenantId, personId, cancellationToken);
        }

        person.EmploymentStatus = normalizedStatus;
        person.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "person.employment_status_update",
            tenantId,
            actorUserId,
            "person",
            person.Id.ToString(),
            "success",
            reasonCode: string.IsNullOrWhiteSpace(request.Reason) ? normalizedStatus : request.Reason.Trim(),
            cancellationToken: cancellationToken);

        await maintainarrTechnicianRefSync.TryPublishPersonChangedAsync(
            person,
            "staffarr.person.employment_status_updated",
            cancellationToken);

        return await GetByIdAsync(tenantId, person.Id, cancellationToken);
    }

    private async Task EnsureEmailAvailableAsync(
        Guid tenantId,
        string normalizedEmail,
        Guid? excludePersonId,
        CancellationToken cancellationToken)
    {
        var query = db.People.Where(p => p.TenantId == tenantId && p.PrimaryEmail.ToLower() == normalizedEmail);
        if (excludePersonId is Guid personId)
        {
            query = query.Where(p => p.Id != personId);
        }

        if (await query.AnyAsync(cancellationToken))
        {
            throw new StlApiException("people.email_conflict", "A person with that email already exists in this tenant.", 409);
        }
    }

    private async Task EnsurePrimaryOrgUnitExistsAsync(
        Guid tenantId,
        Guid? orgUnitId,
        CancellationToken cancellationToken)
    {
        if (orgUnitId is not Guid requestedOrgUnitId)
        {
            return;
        }

        var unitExists = await db.OrgUnits.AnyAsync(
            u => u.TenantId == tenantId && u.Id == requestedOrgUnitId,
            cancellationToken);
        if (!unitExists)
        {
            throw new StlApiException("org_unit.not_found", "Primary org unit was not found.", 404);
        }
    }

    private async Task EnsureCanDeactivateAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken)
    {
        var hasActiveSubordinates = await db.People.AnyAsync(
            p => p.TenantId == tenantId
                && p.ManagerPersonId == personId
                && p.EmploymentStatus == "active",
            cancellationToken);
        if (hasActiveSubordinates)
        {
            throw new StlApiException(
                "people.deactivate_conflict",
                "Cannot deactivate a person who still has active direct reports.",
                409);
        }
    }

    private async Task ValidateManagerReferenceAsync(
        Guid tenantId,
        Guid personId,
        Guid? managerPersonId,
        CancellationToken cancellationToken)
    {
        if (managerPersonId is null)
        {
            return;
        }

        if (managerPersonId.Value == personId)
        {
            throw new StlApiException("people.manager_invalid", "A person cannot be their own manager.", 400);
        }

        var manager = await db.People.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == managerPersonId.Value,
            cancellationToken);
        if (manager is null)
        {
            throw new StlApiException("people.manager_not_found", "Manager person was not found.", 404);
        }

        if (!string.Equals(manager.EmploymentStatus, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException("people.manager_invalid", "Manager person must be active.", 409);
        }

        var cursor = manager.ManagerPersonId;
        var visited = new HashSet<Guid> { manager.Id };
        while (cursor is Guid managerCursorId)
        {
            if (!visited.Add(managerCursorId))
            {
                break;
            }

            if (managerCursorId == personId)
            {
                throw new StlApiException(
                    "people.manager_cycle",
                    "Manager assignment would create a cycle in the hierarchy.",
                    409);
            }

            cursor = await db.People
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Id == managerCursorId)
                .Select(x => x.ManagerPersonId)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }

    private static void ValidatePersonFields(
        string givenName,
        string familyName,
        string primaryEmail,
        string? jobTitle)
    {
        if (string.IsNullOrWhiteSpace(givenName) || givenName.Trim().Length > 100)
        {
            throw new StlApiException("people.validation", "Given name is required and must be 100 characters or less.", 400);
        }

        if (string.IsNullOrWhiteSpace(familyName) || familyName.Trim().Length > 100)
        {
            throw new StlApiException("people.validation", "Family name is required and must be 100 characters or less.", 400);
        }

        if (string.IsNullOrWhiteSpace(primaryEmail) || primaryEmail.Trim().Length > 320)
        {
            throw new StlApiException("people.validation", "Primary email is required and must be 320 characters or less.", 400);
        }

        if (!new EmailAddressAttribute().IsValid(primaryEmail.Trim()))
        {
            throw new StlApiException("people.validation", "Primary email must be valid.", 400);
        }

        if (jobTitle is { Length: > 128 })
        {
            throw new StlApiException("people.validation", "Job title must be 128 characters or less.", 400);
        }
    }

    private static void ValidateEmploymentStatus(string employmentStatus)
    {
        if (string.IsNullOrWhiteSpace(employmentStatus) || employmentStatus.Trim().Length > 32)
        {
            throw new StlApiException("people.validation", "Employment status is required and must be 32 characters or less.", 400);
        }

        if (!AllowedEmploymentStatuses.Contains(employmentStatus.Trim()))
        {
            throw new StlApiException(
                "people.validation",
                "Employment status must be active, inactive, or terminated.",
                400);
        }
    }

    private static string BuildDisplayName(string givenName, string familyName) =>
        $"{givenName.Trim()} {familyName.Trim()}".Trim();
}
