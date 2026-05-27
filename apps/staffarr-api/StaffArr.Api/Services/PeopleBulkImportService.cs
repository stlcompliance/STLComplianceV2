using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class PeopleBulkImportService(
    StaffArrDbContext db,
    IStaffArrAuditService audit)
{
    public const int MaxBatchSize = 100;

    private static readonly HashSet<string> AllowedEmploymentStatuses =
        new(StringComparer.OrdinalIgnoreCase) { "active", "inactive", "terminated" };

    public async Task<BulkPersonImportResponse> ImportAsync(
        Guid tenantId,
        Guid? actorUserId,
        BulkPersonImportRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.People is null || request.People.Count == 0)
        {
            throw new StlApiException("people.import.validation", "At least one person row is required.", 400);
        }

        if (request.People.Count > MaxBatchSize)
        {
            throw new StlApiException(
                "people.import.validation",
                $"Bulk import supports at most {MaxBatchSize} rows per request.",
                400);
        }

        var importId = Guid.NewGuid();
        var results = new List<BulkPersonImportRowResult>();
        var batchEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var batchEmailToPersonId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var createdCount = 0;
        var validatedCount = 0;
        var errorCount = 0;

        for (var index = 0; index < request.People.Count; index++)
        {
            var row = request.People[index];
            var normalizedEmail = row.PrimaryEmail.Trim().ToLowerInvariant();

            if (!batchEmails.Add(normalizedEmail))
            {
                errorCount++;
                results.Add(new BulkPersonImportRowResult(
                    index,
                    normalizedEmail,
                    "error",
                    null,
                    "people.email_conflict",
                    "Duplicate primary email within the import batch."));
                continue;
            }

            try
            {
                ValidatePersonFields(row.GivenName, row.FamilyName, row.PrimaryEmail, row.JobTitle);
                ValidateEmploymentStatus(row.EmploymentStatus);
                await EnsureEmailAvailableAsync(tenantId, normalizedEmail, cancellationToken);
                await EnsurePrimaryOrgUnitExistsAsync(tenantId, row.PrimaryOrgUnitId, cancellationToken);

                var managerPersonId = await ResolveManagerPersonIdAsync(
                    tenantId,
                    row.ManagerPersonId,
                    row.ManagerEmail,
                    batchEmailToPersonId,
                    cancellationToken);

                if (request.DryRun)
                {
                    validatedCount++;
                    results.Add(new BulkPersonImportRowResult(
                        index,
                        normalizedEmail,
                        "validated",
                        null,
                        null,
                        null));
                    continue;
                }

                var personId = Guid.NewGuid();
                await ValidateManagerReferenceAsync(tenantId, personId, managerPersonId, cancellationToken);

                var person = new StaffPerson
                {
                    Id = personId,
                    TenantId = tenantId,
                    GivenName = row.GivenName.Trim(),
                    FamilyName = row.FamilyName.Trim(),
                    DisplayName = BuildDisplayName(row.GivenName, row.FamilyName),
                    PrimaryEmail = normalizedEmail,
                    EmploymentStatus = row.EmploymentStatus.Trim().ToLowerInvariant(),
                    PrimaryOrgUnitId = row.PrimaryOrgUnitId,
                    ManagerPersonId = managerPersonId,
                    JobTitle = row.JobTitle?.Trim(),
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                };

                db.People.Add(person);
                await db.SaveChangesAsync(cancellationToken);

                batchEmailToPersonId[normalizedEmail] = personId;
                createdCount++;
                results.Add(new BulkPersonImportRowResult(
                    index,
                    normalizedEmail,
                    "created",
                    personId,
                    null,
                    null));

                await audit.WriteAsync(
                    "person.create",
                    tenantId,
                    actorUserId,
                    "person",
                    personId.ToString(),
                    "success",
                    reasonCode: "bulk_import",
                    cancellationToken: cancellationToken);
            }
            catch (StlApiException ex)
            {
                errorCount++;
                results.Add(new BulkPersonImportRowResult(
                    index,
                    normalizedEmail,
                    "error",
                    null,
                    ex.Code,
                    ex.Message));
            }
        }

        if (!request.DryRun && createdCount > 0)
        {
            await audit.WriteAsync(
                "person.import.batch",
                tenantId,
                actorUserId,
                "person_import",
                importId.ToString(),
                "success",
                reasonCode: $"{createdCount}/{request.People.Count}",
                cancellationToken: cancellationToken);
        }

        return new BulkPersonImportResponse(
            importId,
            request.DryRun,
            request.People.Count,
            createdCount,
            validatedCount,
            errorCount,
            results);
    }

    private async Task<Guid?> ResolveManagerPersonIdAsync(
        Guid tenantId,
        Guid? managerPersonId,
        string? managerEmail,
        IReadOnlyDictionary<string, Guid> batchEmailToPersonId,
        CancellationToken cancellationToken)
    {
        if (managerPersonId is Guid explicitManagerId)
        {
            return explicitManagerId;
        }

        if (string.IsNullOrWhiteSpace(managerEmail))
        {
            return null;
        }

        var normalizedManagerEmail = managerEmail.Trim().ToLowerInvariant();
        if (batchEmailToPersonId.TryGetValue(normalizedManagerEmail, out var batchManagerId))
        {
            return batchManagerId;
        }

        var existingManagerId = await db.People
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.PrimaryEmail.ToLower() == normalizedManagerEmail)
            .Select(p => p.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingManagerId == Guid.Empty)
        {
            throw new StlApiException(
                "people.manager_not_found",
                "Manager email was not found in this tenant or earlier import rows.",
                404);
        }

        return existingManagerId;
    }

    private async Task EnsureEmailAvailableAsync(
        Guid tenantId,
        string normalizedEmail,
        CancellationToken cancellationToken)
    {
        if (await db.People.AnyAsync(
                p => p.TenantId == tenantId && p.PrimaryEmail.ToLower() == normalizedEmail,
                cancellationToken))
        {
            throw new StlApiException(
                "people.email_conflict",
                "A person with that email already exists in this tenant.",
                409);
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
