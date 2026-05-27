using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class PeopleService(StaffArrDbContext db)
{
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
        CreateStaffPersonRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);
        var normalizedEmail = request.PrimaryEmail.Trim().ToLowerInvariant();

        var existingEmail = await db.People.AnyAsync(
            p => p.TenantId == tenantId && p.PrimaryEmail.ToLower() == normalizedEmail,
            cancellationToken);
        if (existingEmail)
        {
            throw new StlApiException("people.email_conflict", "A person with that email already exists in this tenant.", 409);
        }

        if (request.PrimaryOrgUnitId is Guid orgUnitId)
        {
            var unitExists = await db.OrgUnits.AnyAsync(u => u.TenantId == tenantId && u.Id == orgUnitId, cancellationToken);
            if (!unitExists)
            {
                throw new StlApiException("org_unit.not_found", "Primary org unit was not found.", 404);
            }
        }

        if (request.ManagerPersonId is Guid managerPersonId)
        {
            var managerExists = await db.People.AnyAsync(p => p.TenantId == tenantId && p.Id == managerPersonId, cancellationToken);
            if (!managerExists)
            {
                throw new StlApiException("people.manager_not_found", "Manager person was not found.", 404);
            }
        }

        var person = new StaffPerson
        {
            Id = Guid.NewGuid(),
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
        return await GetByIdAsync(tenantId, person.Id, cancellationToken);
    }

    private static void ValidateRequest(CreateStaffPersonRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.GivenName) || request.GivenName.Trim().Length > 100)
        {
            throw new StlApiException("people.validation", "Given name is required and must be 100 characters or less.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.FamilyName) || request.FamilyName.Trim().Length > 100)
        {
            throw new StlApiException("people.validation", "Family name is required and must be 100 characters or less.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.PrimaryEmail) || request.PrimaryEmail.Trim().Length > 320)
        {
            throw new StlApiException("people.validation", "Primary email is required and must be 320 characters or less.", 400);
        }

        if (!new EmailAddressAttribute().IsValid(request.PrimaryEmail.Trim()))
        {
            throw new StlApiException("people.validation", "Primary email must be valid.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.EmploymentStatus) || request.EmploymentStatus.Trim().Length > 32)
        {
            throw new StlApiException("people.validation", "Employment status is required and must be 32 characters or less.", 400);
        }

        if (request.JobTitle is { Length: > 128 })
        {
            throw new StlApiException("people.validation", "Job title must be 128 characters or less.", 400);
        }
    }

    private static string BuildDisplayName(string givenName, string familyName) =>
        $"{givenName.Trim()} {familyName.Trim()}".Trim();
}
