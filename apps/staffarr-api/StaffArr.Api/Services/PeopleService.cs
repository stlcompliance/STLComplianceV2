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
    StaffArrTenantSettingsService tenantSettingsService,
    StaffArrMaintainArrTechnicianRefSyncService maintainarrTechnicianRefSync,
    OrgUnitAssignmentService orgUnitAssignmentService)
{
    private static readonly IReadOnlySet<string> AllowedEmploymentStatuses = StaffArrControlledFieldCatalog.EmploymentStatusKeys;
    private static readonly IReadOnlySet<string> AllowedWorkRelationshipTypes = StaffArrControlledFieldCatalog.WorkRelationshipKeys;
    private static readonly IReadOnlySet<string> AllowedEmploymentTypes = StaffArrControlledFieldCatalog.EmploymentTypeKeys;

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
                p.GivenName,
                p.FamilyName,
                p.DisplayName,
                p.PrimaryEmail,
                p.EmploymentStatus,
                p.PrimaryOrgUnitId,
                p.PrimaryOrgUnit != null ? p.PrimaryOrgUnit.Name : null,
                p.ManagerPersonId,
                p.JobTitle,
                p.PreferredName,
                p.WorkRelationshipType,
                p.EmploymentType,
                p.WorkerCategory,
                p.FlsaStatus,
                p.PositionNumber,
                p.CurrentEmploymentAction,
                p.CurrentEmploymentActionAt,
                p.LeaveStatus,
                p.EligibleForRehire,
                p.CanLoginSnapshot,
                p.HasUserAccountSnapshot))
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
                p.LegalFirstName,
                p.LegalMiddleName,
                p.LegalLastName,
                p.PreferredName,
                p.Pronouns,
                p.DisplayName,
                p.PrimaryEmail,
                p.AlternateEmail,
                p.PrimaryPhone,
                p.AlternatePhone,
                p.WorkPhone,
                p.EmploymentStatus,
                p.WorkRelationshipType,
                p.EmploymentType,
                p.WorkerCategory,
                p.FlsaStatus,
                p.PositionNumber,
                p.CurrentEmploymentAction,
                p.CurrentEmploymentActionAt,
                p.LeaveStatus,
                p.EligibleForRehire,
                p.PrimaryOrgUnitId,
                p.PrimaryOrgUnit != null ? p.PrimaryOrgUnit.Name : null,
                p.ManagerPersonId,
                p.JobTitle,
                p.StartDate,
                p.ExpectedStartDate,
                p.HomeBaseLocationId,
                p.HomeBaseLocation != null ? p.HomeBaseLocation.Name : null,
                p.CanLoginSnapshot,
                p.ExternalUserId != null || p.HasUserAccountSnapshot,
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
        var legalFirstName = ResolveLegalFirstName(request.LegalFirstName, request.GivenName);
        var legalLastName = ResolveLegalLastName(request.LegalLastName, request.FamilyName);
        var legalMiddleName = NormalizeOptionalNamePart(request.LegalMiddleName, "Legal middle name");
        var preferredName = NormalizeOptionalNamePart(request.PreferredName, "Preferred name");
        var pronouns = NormalizeOptionalText(request.Pronouns, 64, "Pronouns");
        var normalizedPrimaryEmail = NormalizeRequiredEmail(request.PrimaryEmail, "Primary email");
        var normalizedAlternateEmail = NormalizeOptionalEmail(request.AlternateEmail, "Alternate email");
        var normalizedPrimaryPhone = NormalizeOptionalPhone(request.PrimaryPhone, "Primary phone");
        var normalizedAlternatePhone = NormalizeOptionalPhone(request.AlternatePhone, "Alternate phone");
        var normalizedWorkPhone = NormalizeOptionalPhone(request.WorkPhone, "Work phone");
        var normalizedJobTitle = NormalizeOptionalText(request.JobTitle, 128, "Job title");
        var normalizedWorkRelationshipType = NormalizeOptionalCatalogValue(
            request.WorkRelationshipType,
            AllowedWorkRelationshipTypes,
            "Work relationship type");
        var normalizedEmploymentType = NormalizeOptionalCatalogValue(
            request.EmploymentType,
            AllowedEmploymentTypes,
            "Employment type");
        var normalizedWorkerCategory = NormalizeOptionalText(request.WorkerCategory, 64, "Worker category");
        var normalizedFlsaStatus = NormalizeOptionalText(request.FlsaStatus, 64, "FLSA status");
        var normalizedPositionNumber = NormalizeOptionalText(request.PositionNumber, 64, "Position number");
        var normalizedCurrentEmploymentAction = NormalizeOptionalText(
            request.CurrentEmploymentAction,
            64,
            "Current employment action");
        var normalizedLeaveStatus = NormalizeOptionalText(request.LeaveStatus, 64, "Leave status");
        var settings = await tenantSettingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        var normalizedEmploymentStatus = NormalizeEmploymentStatus(
            ResolveCreateEmploymentStatus(request.EmploymentStatus, settings.DefaultPersonStatusOnCreate));
        var primaryOrgUnitId = ResolvePrimaryOrgUnitId(
            request.PrimaryOrgUnitId,
            request.DepartmentOrgUnitId,
            request.SiteOrgUnitId,
            request.TeamOrgUnitId,
            request.PositionOrgUnitId);

        ValidateChronology(request.ExpectedStartDate, request.StartDate);
        ValidateInitialPlacementSeed(
            request.SiteOrgUnitId,
            request.DepartmentOrgUnitId,
            request.TeamOrgUnitId,
            request.PositionOrgUnitId);

        await EnsureEmailAvailableAsync(tenantId, normalizedPrimaryEmail, null, cancellationToken);
        await EnsureAlternateEmailAvailableAsync(
            tenantId,
            normalizedAlternateEmail,
            normalizedPrimaryEmail,
            null,
            cancellationToken);
        await EnsurePrimaryOrgUnitExistsAsync(tenantId, primaryOrgUnitId, cancellationToken);
        await ValidateHomeBaseLocationAsync(
            tenantId,
            request.HomeBaseLocationId,
            request.SiteOrgUnitId,
            primaryOrgUnitId,
            cancellationToken);

        var personId = Guid.NewGuid();
        await ValidateManagerReferenceAsync(tenantId, personId, request.ManagerPersonId, cancellationToken);
        await ValidateActivationRequirementsAsync(
            normalizedEmploymentStatus,
            request.ManagerPersonId,
            request.HomeBaseLocationId,
            request.PositionOrgUnitId,
            primaryOrgUnitId,
            settings,
            tenantId,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var person = new StaffPerson
        {
            Id = personId,
            TenantId = tenantId,
            GivenName = legalFirstName,
            FamilyName = legalLastName,
            LegalFirstName = legalFirstName,
            LegalMiddleName = legalMiddleName,
            LegalLastName = legalLastName,
            PreferredName = settings.PreferredNameEnabled ? preferredName : null,
            Pronouns = pronouns,
            DisplayName = BuildDisplayName(
                legalFirstName,
                legalLastName,
                settings.PreferredNameEnabled ? preferredName : null,
                settings.DisplayNameFormat),
            PrimaryEmail = normalizedPrimaryEmail,
            AlternateEmail = normalizedAlternateEmail,
            PrimaryPhone = normalizedPrimaryPhone,
            AlternatePhone = normalizedAlternatePhone,
            WorkPhone = normalizedWorkPhone,
            EmploymentStatus = normalizedEmploymentStatus,
            WorkRelationshipType = normalizedWorkRelationshipType,
            EmploymentType = normalizedEmploymentType,
            WorkerCategory = normalizedWorkerCategory,
            FlsaStatus = normalizedFlsaStatus,
            PositionNumber = normalizedPositionNumber,
            CurrentEmploymentAction = normalizedCurrentEmploymentAction,
            CurrentEmploymentActionAt = request.CurrentEmploymentActionAt,
            LeaveStatus = normalizedLeaveStatus,
            EligibleForRehire = request.EligibleForRehire,
            PrimaryOrgUnitId = primaryOrgUnitId,
            ManagerPersonId = request.ManagerPersonId,
            JobTitle = normalizedJobTitle,
            StartDate = request.StartDate,
            ExpectedStartDate = request.ExpectedStartDate,
            HomeBaseLocationId = request.HomeBaseLocationId,
            CanLoginSnapshot = request.CanLogin,
            HasUserAccountSnapshot = false,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        db.People.Add(person);
        await db.SaveChangesAsync(cancellationToken);

        if (HasInitialPlacementSeed(
                request.SiteOrgUnitId,
                request.DepartmentOrgUnitId,
                request.TeamOrgUnitId,
                request.PositionOrgUnitId))
        {
            await orgUnitAssignmentService.CreateAsync(
                tenantId,
                actorUserId ?? Guid.Empty,
                personId,
                new CreateOrgUnitAssignmentRequest(
                    request.SiteOrgUnitId!.Value,
                    request.DepartmentOrgUnitId!.Value,
                    request.TeamOrgUnitId!.Value,
                    request.PositionOrgUnitId!.Value),
                cancellationToken);
        }

        await audit.WriteAsync(
            "person.create",
            tenantId,
            actorUserId,
            "person",
            person.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        if (settings.PublishPersonLifecycleEvents)
        {
            await maintainarrTechnicianRefSync.TryPublishPersonChangedAsync(
                person,
                "staffarr.person.created",
                cancellationToken);
        }

        return await GetByIdAsync(tenantId, person.Id, cancellationToken);
    }

    public async Task<StaffPersonDetailResponse> UpdateAsync(
        Guid tenantId,
        Guid personId,
        Guid? actorUserId,
        UpdateStaffPersonRequest request,
        CancellationToken cancellationToken = default)
    {
        var person = await db.People.FirstOrDefaultAsync(
            p => p.TenantId == tenantId && p.Id == personId,
            cancellationToken);
        if (person is null)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }

        var legalFirstName = ResolveLegalFirstName(request.LegalFirstName, request.GivenName);
        var legalLastName = ResolveLegalLastName(request.LegalLastName, request.FamilyName);
        var legalMiddleName = NormalizeOptionalNamePart(request.LegalMiddleName, "Legal middle name");
        var preferredName = NormalizeOptionalNamePart(request.PreferredName, "Preferred name");
        var pronouns = NormalizeOptionalText(request.Pronouns, 64, "Pronouns");
        var normalizedPrimaryEmail = NormalizeRequiredEmail(request.PrimaryEmail, "Primary email");
        var normalizedAlternateEmail = NormalizeOptionalEmail(request.AlternateEmail, "Alternate email");
        var normalizedPrimaryPhone = NormalizeOptionalPhone(request.PrimaryPhone, "Primary phone");
        var normalizedAlternatePhone = NormalizeOptionalPhone(request.AlternatePhone, "Alternate phone");
        var normalizedWorkPhone = NormalizeOptionalPhone(request.WorkPhone, "Work phone");
        var normalizedJobTitle = NormalizeOptionalText(request.JobTitle, 128, "Job title");
        var normalizedWorkRelationshipType = NormalizeOptionalCatalogValue(
            request.WorkRelationshipType,
            AllowedWorkRelationshipTypes,
            "Work relationship type");
        var normalizedEmploymentType = NormalizeOptionalCatalogValue(
            request.EmploymentType,
            AllowedEmploymentTypes,
            "Employment type");
        var normalizedWorkerCategory = NormalizeOptionalText(request.WorkerCategory, 64, "Worker category");
        var normalizedFlsaStatus = NormalizeOptionalText(request.FlsaStatus, 64, "FLSA status");
        var normalizedPositionNumber = NormalizeOptionalText(request.PositionNumber, 64, "Position number");
        var normalizedCurrentEmploymentAction = NormalizeOptionalText(
            request.CurrentEmploymentAction,
            64,
            "Current employment action");
        var normalizedLeaveStatus = NormalizeOptionalText(request.LeaveStatus, 64, "Leave status");
        var settings = await tenantSettingsService.LoadSnapshotAsync(tenantId, cancellationToken);

        ValidateChronology(request.ExpectedStartDate, request.StartDate);

        await EnsureEmailAvailableAsync(tenantId, normalizedPrimaryEmail, personId, cancellationToken);
        await EnsureAlternateEmailAvailableAsync(
            tenantId,
            normalizedAlternateEmail,
            normalizedPrimaryEmail,
            personId,
            cancellationToken);
        await EnsurePrimaryOrgUnitExistsAsync(tenantId, request.PrimaryOrgUnitId, cancellationToken);
        await ValidateHomeBaseLocationAsync(
            tenantId,
            request.HomeBaseLocationId,
            request.SiteOrgUnitId,
            request.PrimaryOrgUnitId ?? person.PrimaryOrgUnitId,
            cancellationToken);
        await ValidateManagerReferenceAsync(tenantId, personId, request.ManagerPersonId, cancellationToken);
        await ValidateActivationRequirementsAsync(
            person.EmploymentStatus,
            request.ManagerPersonId,
            request.HomeBaseLocationId,
            null,
            request.PrimaryOrgUnitId ?? person.PrimaryOrgUnitId,
            settings,
            tenantId,
            cancellationToken);

        person.GivenName = legalFirstName;
        person.FamilyName = legalLastName;
        person.LegalFirstName = legalFirstName;
        person.LegalMiddleName = legalMiddleName;
        person.LegalLastName = legalLastName;
        person.PreferredName = settings.PreferredNameEnabled ? preferredName : null;
        person.Pronouns = pronouns;
        person.DisplayName = BuildDisplayName(
            legalFirstName,
            legalLastName,
            settings.PreferredNameEnabled ? preferredName : null,
            settings.DisplayNameFormat);
        person.PrimaryEmail = normalizedPrimaryEmail;
        person.AlternateEmail = normalizedAlternateEmail;
        person.PrimaryPhone = normalizedPrimaryPhone;
        person.AlternatePhone = normalizedAlternatePhone;
        person.WorkPhone = normalizedWorkPhone;
        person.WorkRelationshipType = normalizedWorkRelationshipType;
        person.EmploymentType = normalizedEmploymentType;
        person.WorkerCategory = normalizedWorkerCategory;
        person.FlsaStatus = normalizedFlsaStatus;
        person.PositionNumber = normalizedPositionNumber;
        person.CurrentEmploymentAction = normalizedCurrentEmploymentAction;
        person.CurrentEmploymentActionAt = request.CurrentEmploymentActionAt;
        person.LeaveStatus = normalizedLeaveStatus;
        person.EligibleForRehire = request.EligibleForRehire;
        person.PrimaryOrgUnitId = request.PrimaryOrgUnitId;
        person.ManagerPersonId = request.ManagerPersonId;
        person.JobTitle = normalizedJobTitle;
        person.StartDate = request.StartDate;
        person.ExpectedStartDate = request.ExpectedStartDate;
        person.HomeBaseLocationId = request.HomeBaseLocationId;
        if (request.CanLoginSnapshot.HasValue)
        {
            person.CanLoginSnapshot = request.CanLoginSnapshot.Value;
        }

        person.HasUserAccountSnapshot = person.ExternalUserId != null || person.HasUserAccountSnapshot;
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

        if (settings.PublishPersonLifecycleEvents)
        {
            await maintainarrTechnicianRefSync.TryPublishPersonChangedAsync(
                person,
                "staffarr.person.updated",
                cancellationToken);
        }

        return await GetByIdAsync(tenantId, person.Id, cancellationToken);
    }

    public async Task<StaffPersonDetailResponse> UpdateEmploymentStatusAsync(
        Guid tenantId,
        Guid personId,
        Guid? actorUserId,
        UpdatePersonEmploymentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedStatus = NormalizeEmploymentStatus(request.EmploymentStatus);
        var settings = await tenantSettingsService.LoadSnapshotAsync(tenantId, cancellationToken);

        var person = await db.People.FirstOrDefaultAsync(
            p => p.TenantId == tenantId && p.Id == personId,
            cancellationToken);
        if (person is null)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }

        if (!string.Equals(person.EmploymentStatus, normalizedStatus, StringComparison.OrdinalIgnoreCase)
            && normalizedStatus is not "active")
        {
        if (settings.DeactivationReasonRequired && string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new StlApiException("people.validation", "Deactivation reason is required.", 400);
        }

            await EnsureCanDeactivateAsync(tenantId, personId, cancellationToken);
        }
        else if (normalizedStatus == "active")
        {
            await ValidateActivationRequirementsAsync(
                normalizedStatus,
                person.ManagerPersonId,
                person.HomeBaseLocationId,
                null,
                person.PrimaryOrgUnitId,
                settings,
                tenantId,
                cancellationToken);
        }

        var previousStatus = person.EmploymentStatus;
        person.EmploymentStatus = normalizedStatus;
        person.CurrentEmploymentAction = normalizedStatus switch
        {
            "active" => string.Equals(previousStatus, "terminated", StringComparison.OrdinalIgnoreCase)
                || string.Equals(previousStatus, "inactive", StringComparison.OrdinalIgnoreCase)
                ? "rehire"
                : "hire",
            "leave" => "leave_start",
            "suspended" => "suspension",
            "terminated" => "termination",
            "inactive" => "termination",
            _ => "status_update",
        };
        person.CurrentEmploymentActionAt = DateTimeOffset.UtcNow;
        person.LeaveStatus = normalizedStatus switch
        {
            "active" => "active",
            "leave" => "leave",
            "suspended" => "suspended",
            "terminated" => "terminated",
            "inactive" => "inactive",
            _ => person.LeaveStatus,
        };
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

        if (settings.PublishPersonLifecycleEvents)
        {
            await maintainarrTechnicianRefSync.TryPublishPersonChangedAsync(
                person,
                "staffarr.person.employment_status_updated",
                cancellationToken);
        }

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

    private async Task EnsureAlternateEmailAvailableAsync(
        Guid tenantId,
        string? alternateEmail,
        string primaryEmail,
        Guid? excludePersonId,
        CancellationToken cancellationToken)
    {
        if (alternateEmail is null)
        {
            return;
        }

        if (string.Equals(alternateEmail, primaryEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException("people.validation", "Alternate email must differ from primary email.", 400);
        }

        var query = db.People.Where(p =>
            p.TenantId == tenantId
            && p.AlternateEmail != null
            && p.AlternateEmail.ToLower() == alternateEmail);
        if (excludePersonId is Guid personId)
        {
            query = query.Where(p => p.Id != personId);
        }

        if (await query.AnyAsync(cancellationToken))
        {
            throw new StlApiException(
                "people.email_conflict",
                "A person with that alternate email already exists in this tenant.",
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

    private async Task ValidateHomeBaseLocationAsync(
        Guid tenantId,
        Guid? homeBaseLocationId,
        Guid? siteOrgUnitId,
        Guid? primaryOrgUnitId,
        CancellationToken cancellationToken)
    {
        if (homeBaseLocationId is not Guid requestedLocationId)
        {
            return;
        }

        var locationExists = await db.InternalLocations.AnyAsync(
            x => x.TenantId == tenantId && x.Id == requestedLocationId,
            cancellationToken);
        if (!locationExists)
        {
            throw new StlApiException("location.not_found", "Home base location was not found.", 404);
        }

        var expectedSiteId = siteOrgUnitId
            ?? await ResolveSiteOrgUnitIdForOrgUnitAsync(tenantId, primaryOrgUnitId, cancellationToken);
        if (expectedSiteId is not Guid resolvedSiteId)
        {
            return;
        }

        var locationSiteId = await db.InternalLocations
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == requestedLocationId)
            .Select(x => x.SiteOrgUnitId)
            .FirstOrDefaultAsync(cancellationToken);
        if (locationSiteId is not Guid resolvedLocationSiteId || resolvedLocationSiteId != resolvedSiteId)
        {
            throw new StlApiException(
                "people.validation",
                "Home base location must belong to the selected site.",
                409);
        }
    }

    private async Task<Guid?> ResolveSiteOrgUnitIdForOrgUnitAsync(
        Guid tenantId,
        Guid? orgUnitId,
        CancellationToken cancellationToken)
    {
        if (orgUnitId is not Guid requestedOrgUnitId)
        {
            return null;
        }

        var cursor = requestedOrgUnitId;
        while (true)
        {
            var node = await db.OrgUnits
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Id == cursor)
                .Select(x => new { x.Id, x.UnitType, x.ParentOrgUnitId })
                .FirstOrDefaultAsync(cancellationToken);

            if (node is null)
            {
                return null;
            }

            if (string.Equals(node.UnitType, "site", StringComparison.OrdinalIgnoreCase))
            {
                return node.Id;
            }

            if (node.ParentOrgUnitId is not Guid parentId)
            {
                return null;
            }

            cursor = parentId;
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

    private static string ResolveLegalFirstName(string? legalFirstName, string? givenName)
    {
        var resolved = string.IsNullOrWhiteSpace(legalFirstName) ? givenName : legalFirstName;
        return NormalizeRequiredNamePart(resolved, "Legal first name");
    }

    private static string ResolveLegalLastName(string? legalLastName, string? familyName)
    {
        var resolved = string.IsNullOrWhiteSpace(legalLastName) ? familyName : legalLastName;
        return NormalizeRequiredNamePart(resolved, "Legal last name");
    }

    private static string NormalizeRequiredNamePart(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException("people.validation", $"{fieldName} is required.", 400);
        }

        var normalized = value.Trim();
        if (normalized.Length > 100)
        {
            throw new StlApiException("people.validation", $"{fieldName} must be 100 characters or less.", 400);
        }

        return normalized;
    }

    private static string? NormalizeOptionalNamePart(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > 100)
        {
            throw new StlApiException("people.validation", $"{fieldName} must be 100 characters or less.", 400);
        }

        return normalized;
    }

    private static string NormalizeRequiredEmail(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > 320)
        {
            throw new StlApiException("people.validation", $"{fieldName} is required and must be 320 characters or less.", 400);
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (!new EmailAddressAttribute().IsValid(normalized))
        {
            throw new StlApiException("people.validation", $"{fieldName} must be valid.", 400);
        }

        return normalized;
    }

    private static string? NormalizeOptionalEmail(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return NormalizeRequiredEmail(value, fieldName);
    }

    private static string? NormalizeOptionalPhone(string? value, string fieldName)
    {
        return NormalizeOptionalText(value, 32, fieldName);
    }

    private static string? NormalizeOptionalText(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new StlApiException("people.validation", $"{fieldName} must be {maxLength} characters or less.", 400);
        }

        return normalized;
    }

    private static string? NormalizeOptionalCatalogValue(
        string? value,
        IReadOnlySet<string> allowedValues,
        string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length > 32)
        {
            throw new StlApiException("people.validation", $"{fieldName} must be 32 characters or less.", 400);
        }

        if (!allowedValues.Contains(normalized))
        {
            throw new StlApiException(
                "people.validation",
                $"{fieldName} is invalid.",
                400);
        }

        return normalized;
    }

    private static void ValidateChronology(DateTimeOffset? expectedStartDate, DateTimeOffset? startDate)
    {
        if (expectedStartDate is DateTimeOffset expected
            && startDate is DateTimeOffset actual
            && expected > actual.AddYears(5))
        {
            throw new StlApiException(
                "people.validation",
                "Expected start date is not realistic relative to the start date.",
                400);
        }
    }

    private static string NormalizeEmploymentStatus(string employmentStatus)
    {
        if (string.IsNullOrWhiteSpace(employmentStatus) || employmentStatus.Trim().Length > 32)
        {
            throw new StlApiException("people.validation", "Employment status is required and must be 32 characters or less.", 400);
        }

        var normalized = employmentStatus.Trim().ToLowerInvariant();
        if (!AllowedEmploymentStatuses.Contains(normalized))
        {
            throw new StlApiException(
                "people.validation",
                "Employment status must be applicant, pending_start, onboarding, active, leave, suspended, terminated, inactive, or archived.",
                400);
        }

        return normalized;
    }

    private static string ResolveCreateEmploymentStatus(string? requestedStatus, string tenantDefaultStatus)
    {
        if (string.IsNullOrWhiteSpace(requestedStatus))
        {
            return tenantDefaultStatus;
        }

        var normalized = requestedStatus.Trim().ToLowerInvariant();
        return normalized == StaffArrTenantSettingsDefaults.DefaultPersonStatusOnCreate
            ? tenantDefaultStatus
            : normalized;
    }

    private async Task ValidateActivationRequirementsAsync(
        string status,
        Guid? managerPersonId,
        Guid? homeBaseLocationId,
        Guid? positionOrgUnitId,
        Guid? primaryOrgUnitId,
        StaffArrTenantSettings settings,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(status, "active", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (settings.RequireManagerBeforeActivation && managerPersonId is null)
        {
            throw new StlApiException(
                "people.activation_requirements",
                "A manager is required before activating a person.",
                409);
        }

        if (settings.RequireHomeLocationBeforeActivation && homeBaseLocationId is null)
        {
            throw new StlApiException(
                "people.activation_requirements",
                "A home location is required before activating a person.",
                409);
        }

        if (settings.RequireEveryPersonInOrgUnit && primaryOrgUnitId is null)
        {
            throw new StlApiException(
                "people.activation_requirements",
                "An org unit is required before activating a person.",
                409);
        }

        if (!settings.RequirePositionBeforeActivation)
        {
            return;
        }

        if (positionOrgUnitId is Guid)
        {
            return;
        }

        if (primaryOrgUnitId is Guid requestedPrimaryOrgUnitId
            && await db.OrgUnits
                .AsNoTracking()
                .AnyAsync(
                    x => x.TenantId == tenantId
                        && x.Id == requestedPrimaryOrgUnitId
                        && x.UnitType == "position",
                    cancellationToken))
        {
            return;
        }

        throw new StlApiException(
            "people.activation_requirements",
            "A position is required before activating a person.",
            409);
    }

    private static Guid? ResolvePrimaryOrgUnitId(
        Guid? primaryOrgUnitId,
        Guid? departmentOrgUnitId,
        Guid? siteOrgUnitId,
        Guid? teamOrgUnitId,
        Guid? positionOrgUnitId) =>
        primaryOrgUnitId
        ?? departmentOrgUnitId
        ?? siteOrgUnitId
        ?? teamOrgUnitId
        ?? positionOrgUnitId;

    private static void ValidateInitialPlacementSeed(
        Guid? siteOrgUnitId,
        Guid? departmentOrgUnitId,
        Guid? teamOrgUnitId,
        Guid? positionOrgUnitId)
    {
        Guid?[] values =
        [
            siteOrgUnitId,
            departmentOrgUnitId,
            teamOrgUnitId,
            positionOrgUnitId,
        ];

        var populatedCount = values.Count(value => value.HasValue);
        if (populatedCount is > 0 and < 4)
        {
            throw new StlApiException(
                "people.validation",
                "Initial placement requires site, department, team, and position together.",
                400);
        }
    }

    private static bool HasInitialPlacementSeed(
        Guid? siteOrgUnitId,
        Guid? departmentOrgUnitId,
        Guid? teamOrgUnitId,
        Guid? positionOrgUnitId) =>
        siteOrgUnitId.HasValue
        && departmentOrgUnitId.HasValue
        && teamOrgUnitId.HasValue
        && positionOrgUnitId.HasValue;

    private static string BuildDisplayName(
        string legalFirstName,
        string legalLastName,
        string? preferredName,
        string displayNameFormat)
    {
        var firstName = string.IsNullOrWhiteSpace(preferredName) ? legalFirstName.Trim() : preferredName.Trim();
        var lastName = legalLastName.Trim();

        return displayNameFormat switch
        {
            "legal_first_last" => $"{legalFirstName.Trim()} {lastName}".Trim(),
            "last_first" => $"{lastName}, {legalFirstName.Trim()}".Trim(' ', ','),
            "first_last_employee_number" => $"{firstName} {lastName}".Trim(),
            _ => $"{firstName} {lastName}".Trim()
        };
    }
}
