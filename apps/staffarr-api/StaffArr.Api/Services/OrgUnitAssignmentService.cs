using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class OrgUnitAssignmentService(
    StaffArrDbContext db,
    IStaffArrAuditService audit,
    StaffArrTenantSettingsService tenantSettingsService)
{
    public async Task<IReadOnlyList<OrgUnitAssignmentResponse>> ListByPersonAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(tenantId, personId, cancellationToken);
        return await db.OrgUnitAssignments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .OrderByDescending(x => x.IsPrimary)
            .ThenByDescending(x => x.EffectiveAt)
            .ThenByDescending(x => x.UpdatedAt)
            .Select(ToResponseExpression())
            .ToListAsync(cancellationToken);
    }

    public async Task<OrgUnitAssignmentResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid personId,
        CreateOrgUnitAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = await tenantSettingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        var normalized = NormalizeRequest(
            request.Status,
            request.IsPrimary,
            request.EffectiveAt,
            request.EndsAt,
            request.Reason,
            settings);
        EnsureCreateStatusSupported(normalized.Status);

        var hasSelectablePrimary = await HasSelectablePrimaryAssignmentAsync(tenantId, personId, null, cancellationToken);
        var isPrimary = ResolvePrimaryValue(normalized.IsPrimary, hasSelectablePrimary);

        await ValidateAssignmentAsync(
            tenantId,
            personId,
            request.SiteOrgUnitId,
            request.DepartmentOrgUnitId,
            request.TeamOrgUnitId,
            request.PositionOrgUnitId,
            normalized.Status,
            isPrimary,
            null,
            settings,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var assignment = new OrgUnitAssignment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PersonId = personId,
            SiteOrgUnitId = request.SiteOrgUnitId,
            DepartmentOrgUnitId = request.DepartmentOrgUnitId,
            TeamOrgUnitId = request.TeamOrgUnitId,
            PositionOrgUnitId = request.PositionOrgUnitId,
            Status = normalized.Status,
            IsPrimary = isPrimary,
            EffectiveAt = normalized.EffectiveAt ?? now,
            EndsAt = normalized.EndsAt,
            Reason = normalized.Reason,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.OrgUnitAssignments.Add(assignment);
        await db.SaveChangesAsync(cancellationToken);
        await EnsureFallbackPrimaryAsync(tenantId, personId, cancellationToken);
        await SyncPrimaryOrgUnitSnapshotAsync(tenantId, personId, cancellationToken);

        await audit.WriteAsync(
            "org_assignment.create",
            tenantId,
            actorUserId,
            "org_assignment",
            assignment.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        if (assignment.Status == "active")
        {
            await WritePlacementChangeEventsAsync(
                tenantId,
                actorUserId,
                assignment.PersonId,
                previous: null,
                current: assignment,
                cancellationToken);
        }

        return await GetByIdAsync(tenantId, personId, assignment.Id, cancellationToken);
    }

    public async Task<OrgUnitAssignmentResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid personId,
        Guid assignmentId,
        UpdateOrgUnitAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var assignment = await GetAssignmentEntityAsync(tenantId, personId, assignmentId, cancellationToken);
        var settings = await tenantSettingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        var normalized = NormalizeRequest(
            request.Status,
            request.IsPrimary,
            request.EffectiveAt,
            request.EndsAt,
            request.Reason,
            settings);
        var chainChanged =
            assignment.SiteOrgUnitId != request.SiteOrgUnitId
            || assignment.DepartmentOrgUnitId != request.DepartmentOrgUnitId
            || assignment.TeamOrgUnitId != request.TeamOrgUnitId
            || assignment.PositionOrgUnitId != request.PositionOrgUnitId;

        if (assignment.Status is "ended" or "canceled")
        {
            throw new StlApiException(
                "org_assignment.immutable",
                "Historical placements cannot be edited.",
                409);
        }

        if (assignment.Status == "active" && chainChanged)
        {
            return await TransferActiveAssignmentAsync(
                tenantId,
                actorUserId,
                assignment,
                request,
                normalized,
                settings,
                cancellationToken);
        }

        if (assignment.Status == "active" && normalized.Status != "active")
        {
            throw new StlApiException(
                "org_assignment.status_conflict",
                "Use the placement status endpoint to end or cancel an active placement.",
                409);
        }

        var hasSelectablePrimary = await HasSelectablePrimaryAssignmentAsync(tenantId, personId, assignmentId, cancellationToken);
        var isPrimary = normalized.Status is "ended" or "canceled"
            ? false
            : ResolvePrimaryValue(normalized.IsPrimary ?? assignment.IsPrimary, hasSelectablePrimary);

        await ValidateAssignmentAsync(
            tenantId,
            personId,
            request.SiteOrgUnitId,
            request.DepartmentOrgUnitId,
            request.TeamOrgUnitId,
            request.PositionOrgUnitId,
            normalized.Status,
            isPrimary,
            assignmentId,
            settings,
            cancellationToken);

        assignment.SiteOrgUnitId = request.SiteOrgUnitId;
        assignment.DepartmentOrgUnitId = request.DepartmentOrgUnitId;
        assignment.TeamOrgUnitId = request.TeamOrgUnitId;
        assignment.PositionOrgUnitId = request.PositionOrgUnitId;
        assignment.Status = normalized.Status;
        assignment.IsPrimary = isPrimary;
        assignment.EffectiveAt = normalized.EffectiveAt ?? assignment.EffectiveAt;
        assignment.EndsAt = normalized.Status switch
        {
            "ended" => normalized.EndsAt ?? DateTimeOffset.UtcNow,
            "canceled" => normalized.EndsAt ?? (assignment.EffectiveAt > DateTimeOffset.UtcNow
                ? assignment.EffectiveAt
                : DateTimeOffset.UtcNow),
            _ => normalized.EndsAt
        };
        assignment.Reason = normalized.Reason;
        assignment.UpdatedAt = DateTimeOffset.UtcNow;

        if (assignment.Status != "canceled"
            && assignment.EndsAt.HasValue
            && assignment.EndsAt.Value < assignment.EffectiveAt)
        {
            throw new StlApiException(
                "org_assignment.validation",
                "Placement end date must be on or after the effective date.",
                400);
        }

        await db.SaveChangesAsync(cancellationToken);
        await EnsureFallbackPrimaryAsync(tenantId, personId, cancellationToken);
        await SyncPrimaryOrgUnitSnapshotAsync(tenantId, personId, cancellationToken);

        await audit.WriteAsync(
            "org_assignment.update",
            tenantId,
            actorUserId,
            "org_assignment",
            assignment.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetByIdAsync(tenantId, personId, assignment.Id, cancellationToken);
    }

    public async Task<OrgUnitAssignmentResponse> UpdateStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid personId,
        Guid assignmentId,
        UpdateOrgUnitAssignmentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var assignment = await GetAssignmentEntityAsync(tenantId, personId, assignmentId, cancellationToken);
        var settings = await tenantSettingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        var normalizedStatus = NormalizeAssignmentStatus(request.Status);
        var normalizedReason = NormalizeOptionalText(request.Reason, 256, "Reason");
        var requestedEndsAt = request.EndsAt;

        switch (normalizedStatus)
        {
            case "active":
                if (assignment.Status is "ended" or "canceled")
                {
                    throw new StlApiException("org_assignment.status_conflict", "Historical placements cannot be reactivated.", 409);
                }

                if (assignment.EffectiveAt > DateTimeOffset.UtcNow)
                {
                    throw new StlApiException(
                        "org_assignment.validation",
                        "Future-dated placements cannot be activated until the effective date.",
                        409);
                }

                await ValidateAssignmentAsync(
                    tenantId,
                    personId,
                    assignment.SiteOrgUnitId,
                    assignment.DepartmentOrgUnitId,
                    assignment.TeamOrgUnitId,
                    assignment.PositionOrgUnitId,
                    "active",
                    ResolvePrimaryValue(assignment.IsPrimary, await HasSelectablePrimaryAssignmentAsync(tenantId, personId, assignmentId, cancellationToken)),
                    assignmentId,
                    settings,
                    cancellationToken);
                assignment.Status = "active";
                assignment.IsPrimary = ResolvePrimaryValue(
                    assignment.IsPrimary,
                    await HasSelectablePrimaryAssignmentAsync(tenantId, personId, assignmentId, cancellationToken));
                assignment.EndsAt = null;
                break;

            case "planned":
                if (assignment.Status == "active")
                {
                    throw new StlApiException(
                        "org_assignment.status_conflict",
                        "Active placements cannot be moved back to planned status.",
                        409);
                }

                await ValidateAssignmentAsync(
                    tenantId,
                    personId,
                    assignment.SiteOrgUnitId,
                    assignment.DepartmentOrgUnitId,
                    assignment.TeamOrgUnitId,
                    assignment.PositionOrgUnitId,
                    "planned",
                    ResolvePrimaryValue(assignment.IsPrimary, await HasSelectablePrimaryAssignmentAsync(tenantId, personId, assignmentId, cancellationToken)),
                    assignmentId,
                    settings,
                    cancellationToken);
                assignment.Status = "planned";
                assignment.EndsAt = null;
                break;

            case "ended":
                assignment.Status = normalizedStatus;
                assignment.EndsAt = requestedEndsAt ?? DateTimeOffset.UtcNow;
                assignment.IsPrimary = false;
                break;

            case "canceled":
                assignment.Status = normalizedStatus;
                assignment.EndsAt = requestedEndsAt ?? (assignment.EffectiveAt > DateTimeOffset.UtcNow
                    ? assignment.EffectiveAt
                    : DateTimeOffset.UtcNow);
                assignment.IsPrimary = false;
                break;
        }

        if (normalizedStatus != "canceled"
            && assignment.EndsAt.HasValue
            && assignment.EndsAt.Value < assignment.EffectiveAt)
        {
            throw new StlApiException(
                "org_assignment.validation",
                "Placement end date must be on or after the effective date.",
                400);
        }

        assignment.Reason = normalizedReason ?? assignment.Reason;
        assignment.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await EnsureFallbackPrimaryAsync(tenantId, personId, cancellationToken);
        await SyncPrimaryOrgUnitSnapshotAsync(tenantId, personId, cancellationToken);

        await audit.WriteAsync(
            "org_assignment.status_update",
            tenantId,
            actorUserId,
            "org_assignment",
            assignment.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetByIdAsync(tenantId, personId, assignment.Id, cancellationToken);
    }

    private async Task<OrgUnitAssignmentResponse> TransferActiveAssignmentAsync(
        Guid tenantId,
        Guid actorUserId,
        OrgUnitAssignment currentAssignment,
        UpdateOrgUnitAssignmentRequest request,
        NormalizedAssignmentRequest normalized,
        StaffArrTenantSettings settings,
        CancellationToken cancellationToken)
    {
        if (normalized.Status is "ended" or "canceled")
        {
            throw new StlApiException(
                "org_assignment.status_conflict",
                "Transfers must create a planned or active successor placement.",
                409);
        }

        var successorStatus = normalized.Status;
        var successorEffectiveAt = normalized.EffectiveAt ?? DateTimeOffset.UtcNow;

        if (successorStatus == "active" && successorEffectiveAt > DateTimeOffset.UtcNow)
        {
            throw new StlApiException(
                "org_assignment.validation",
                "Future-dated transfers must use planned status.",
                400);
        }

        var hasSelectablePrimary = await HasSelectablePrimaryAssignmentAsync(
            tenantId,
            currentAssignment.PersonId,
            currentAssignment.Id,
            cancellationToken);
        var successorPrimary = successorStatus == "active"
            ? ResolvePrimaryValue(normalized.IsPrimary ?? currentAssignment.IsPrimary, hasSelectablePrimary)
            : false;

        await ValidateAssignmentAsync(
            tenantId,
            currentAssignment.PersonId,
            request.SiteOrgUnitId,
            request.DepartmentOrgUnitId,
            request.TeamOrgUnitId,
            request.PositionOrgUnitId,
            successorStatus,
            successorPrimary,
            currentAssignment.Id,
            settings,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var successor = new OrgUnitAssignment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PersonId = currentAssignment.PersonId,
            SiteOrgUnitId = request.SiteOrgUnitId,
            DepartmentOrgUnitId = request.DepartmentOrgUnitId,
            TeamOrgUnitId = request.TeamOrgUnitId,
            PositionOrgUnitId = request.PositionOrgUnitId,
            Status = successorStatus,
            IsPrimary = successorPrimary,
            EffectiveAt = successorEffectiveAt,
            EndsAt = normalized.EndsAt,
            Reason = normalized.Reason,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.OrgUnitAssignments.Add(successor);

        if (successorStatus == "active")
        {
            currentAssignment.Status = "ended";
            currentAssignment.EndsAt = successorEffectiveAt > now ? successorEffectiveAt : now;
            currentAssignment.IsPrimary = false;
            currentAssignment.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
        await EnsureFallbackPrimaryAsync(tenantId, currentAssignment.PersonId, cancellationToken);
        await SyncPrimaryOrgUnitSnapshotAsync(tenantId, currentAssignment.PersonId, cancellationToken);

        await audit.WriteAsync(
            "org_assignment.update",
            tenantId,
            actorUserId,
            "org_assignment",
            currentAssignment.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);
        await audit.WriteAsync(
            "org_assignment.create",
            tenantId,
            actorUserId,
            "org_assignment",
            successor.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        if (successor.Status == "active")
        {
            await WritePlacementChangeEventsAsync(
                tenantId,
                actorUserId,
                currentAssignment.PersonId,
                currentAssignment,
                successor,
                cancellationToken);
        }

        return await GetByIdAsync(tenantId, currentAssignment.PersonId, successor.Id, cancellationToken);
    }

    private async Task<OrgUnitAssignmentResponse> GetByIdAsync(
        Guid tenantId,
        Guid personId,
        Guid assignmentId,
        CancellationToken cancellationToken)
    {
        var assignment = await db.OrgUnitAssignments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId && x.Id == assignmentId)
            .Select(ToResponseExpression())
            .FirstOrDefaultAsync(cancellationToken);
        return assignment ?? throw new StlApiException("org_assignment.not_found", "Org assignment was not found.", 404);
    }

    private async Task<OrgUnitAssignment> GetAssignmentEntityAsync(
        Guid tenantId,
        Guid personId,
        Guid assignmentId,
        CancellationToken cancellationToken)
    {
        await EnsurePersonExistsAsync(tenantId, personId, cancellationToken);
        var assignment = await db.OrgUnitAssignments.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.PersonId == personId && x.Id == assignmentId,
            cancellationToken);
        return assignment ?? throw new StlApiException("org_assignment.not_found", "Org assignment was not found.", 404);
    }

    private async Task ValidateAssignmentAsync(
        Guid tenantId,
        Guid personId,
        Guid siteOrgUnitId,
        Guid departmentOrgUnitId,
        Guid teamOrgUnitId,
        Guid positionOrgUnitId,
        string assignmentStatus,
        bool isPrimary,
        Guid? excludedAssignmentId,
        StaffArrTenantSettings settings,
        CancellationToken cancellationToken)
    {
        await EnsurePersonExistsAsync(tenantId, personId, cancellationToken);

        var allowedOrgUnitStatuses = assignmentStatus == "active"
            ? new[] { "active" }
            : new[] { "planned", "active" };

        await EnsureOrgUnitAsync(tenantId, siteOrgUnitId, "site", allowedOrgUnitStatuses, cancellationToken);
        await EnsureOrgUnitAsync(tenantId, departmentOrgUnitId, "department", allowedOrgUnitStatuses, cancellationToken);
        await EnsureOrgUnitAsync(tenantId, teamOrgUnitId, "team", allowedOrgUnitStatuses, cancellationToken);
        await EnsureOrgUnitAsync(tenantId, positionOrgUnitId, "position", allowedOrgUnitStatuses, cancellationToken);
        await EnsureHierarchyLinkageAsync(
            tenantId,
            siteOrgUnitId,
            departmentOrgUnitId,
            teamOrgUnitId,
            positionOrgUnitId,
            settings,
            cancellationToken);

        if (OrgStructureCatalog.IsSelectableAssignmentStatus(assignmentStatus))
        {
            if (!settings.AllowMatrixMembership)
            {
                var anotherSelectableExists = await db.OrgUnitAssignments.AnyAsync(
                    x =>
                        x.TenantId == tenantId
                        && x.PersonId == personId
                        && (excludedAssignmentId == null || x.Id != excludedAssignmentId.Value)
                        && (x.Status == "planned" || x.Status == "active"),
                    cancellationToken);
                if (anotherSelectableExists)
                {
                    throw new StlApiException(
                        "org_assignment.matrix_disabled",
                        "Matrix membership is disabled for this tenant.",
                        409);
                }
            }

            var duplicateExists = await db.OrgUnitAssignments.AnyAsync(
                x =>
                    x.TenantId == tenantId
                    && x.PersonId == personId
                    && (excludedAssignmentId == null || x.Id != excludedAssignmentId.Value)
                    && (x.Status == "planned" || x.Status == "active")
                    && x.SiteOrgUnitId == siteOrgUnitId
                    && x.DepartmentOrgUnitId == departmentOrgUnitId
                    && x.TeamOrgUnitId == teamOrgUnitId
                    && x.PositionOrgUnitId == positionOrgUnitId,
                cancellationToken);

            if (duplicateExists)
            {
                throw new StlApiException("org_assignment.duplicate", "An identical selectable org assignment already exists for this person.", 409);
            }

            if (isPrimary)
            {
                var anotherPrimaryExists = await db.OrgUnitAssignments.AnyAsync(
                    x =>
                        x.TenantId == tenantId
                        && x.PersonId == personId
                        && (excludedAssignmentId == null || x.Id != excludedAssignmentId.Value)
                        && x.IsPrimary
                        && (x.Status == "planned" || x.Status == "active"),
                    cancellationToken);

                if (anotherPrimaryExists)
                {
                    throw new StlApiException(
                        "org_assignment.primary_conflict",
                        "Only one planned or active primary placement is allowed per person.",
                        409);
                }
            }
        }
    }

    private async Task EnsurePersonExistsAsync(Guid tenantId, Guid personId, CancellationToken cancellationToken)
    {
        var exists = await db.People.AnyAsync(x => x.TenantId == tenantId && x.Id == personId, cancellationToken);
        if (!exists)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }
    }

    private async Task EnsureOrgUnitAsync(
        Guid tenantId,
        Guid orgUnitId,
        string expectedType,
        IReadOnlyCollection<string> allowedStatuses,
        CancellationToken cancellationToken)
    {
        var orgUnit = await db.OrgUnits
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == orgUnitId)
            .Select(x => new { x.UnitType, x.Status })
            .FirstOrDefaultAsync(cancellationToken);

        if (orgUnit is null)
        {
            throw new StlApiException("org_assignment.org_unit_not_found", $"Referenced {expectedType} org unit was not found.", 404);
        }

        if (!string.Equals(orgUnit.UnitType, expectedType, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "org_assignment.link_invalid",
                $"Referenced org unit must be of type {expectedType}.",
                409);
        }

        if (!allowedStatuses.Contains(orgUnit.Status))
        {
            throw new StlApiException(
                "org_assignment.link_inactive",
                $"Referenced {expectedType} org unit must be {string.Join(" or ", allowedStatuses)}.",
                409);
        }
    }

    private async Task EnsureHierarchyLinkageAsync(
        Guid tenantId,
        Guid siteOrgUnitId,
        Guid departmentOrgUnitId,
        Guid teamOrgUnitId,
        Guid positionOrgUnitId,
        StaffArrTenantSettings settings,
        CancellationToken cancellationToken)
    {
        if (settings.RequireDepartmentUnderSite
            && !await IsDescendantOrSelfAsync(tenantId, departmentOrgUnitId, siteOrgUnitId, cancellationToken))
        {
            throw new StlApiException(
                "org_assignment.link_invalid",
                "Department must be linked under the selected site in the org hierarchy.",
                409);
        }

        if (!await IsDescendantOrSelfAsync(tenantId, teamOrgUnitId, departmentOrgUnitId, cancellationToken))
        {
            throw new StlApiException(
                "org_assignment.link_invalid",
                "Team must be linked under the selected department in the org hierarchy.",
                409);
        }

        if (!await IsDescendantOrSelfAsync(tenantId, positionOrgUnitId, teamOrgUnitId, cancellationToken))
        {
            throw new StlApiException(
                "org_assignment.link_invalid",
                "Position must be linked under the selected team in the org hierarchy.",
                409);
        }
    }

    private async Task<bool> IsDescendantOrSelfAsync(
        Guid tenantId,
        Guid nodeId,
        Guid ancestorId,
        CancellationToken cancellationToken)
    {
        if (nodeId == ancestorId)
        {
            return true;
        }

        var cursor = await db.OrgUnits
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == nodeId)
            .Select(x => x.ParentOrgUnitId)
            .FirstOrDefaultAsync(cancellationToken);

        while (cursor is Guid parentId)
        {
            if (parentId == ancestorId)
            {
                return true;
            }

            cursor = await db.OrgUnits
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Id == parentId)
                .Select(x => x.ParentOrgUnitId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return false;
    }

    private static void EnsureCreateStatusSupported(string status)
    {
        if (status is "ended" or "canceled")
        {
            throw new StlApiException(
                "org_assignment.validation",
                "Use planned or active when creating a placement.",
                400);
        }
    }

    private static string NormalizeAssignmentStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new StlApiException("org_assignment.validation", "Status is required.", 400);
        }

        var normalized = status.Trim().ToLowerInvariant();
        if (!OrgStructureCatalog.AssignmentStatuses.Contains(normalized))
        {
            throw new StlApiException(
                "org_assignment.validation",
                "Status must be planned, active, ended, or canceled.",
                400);
        }

        return normalized;
    }

    private static NormalizedAssignmentRequest NormalizeRequest(
        string status,
        bool? isPrimary,
        DateTimeOffset? effectiveAt,
        DateTimeOffset? endsAt,
        string? reason,
        StaffArrTenantSettings settings)
    {
        var normalizedStatus = NormalizeAssignmentStatus(status);
        var normalizedReason = NormalizeOptionalText(reason, 256, "Reason");

        if (!settings.AssignmentEffectiveDatingEnabled && (effectiveAt.HasValue || endsAt.HasValue))
        {
            throw new StlApiException(
                "org_assignment.effective_dating_disabled",
                "Assignment effective dating is disabled for this tenant.",
                409);
        }

        if (!settings.AllowTemporaryAssignments && endsAt.HasValue)
        {
            throw new StlApiException(
                "org_assignment.temporary_disabled",
                "Temporary assignments are disabled for this tenant.",
                409);
        }

        if (normalizedStatus == "active" && effectiveAt.HasValue && effectiveAt.Value > DateTimeOffset.UtcNow)
        {
            throw new StlApiException(
                "org_assignment.validation",
                "Future-dated placements must use planned status.",
                400);
        }

        if (effectiveAt.HasValue && endsAt.HasValue && endsAt.Value < effectiveAt.Value)
        {
            throw new StlApiException(
                "org_assignment.validation",
                "Placement end date must be on or after the effective date.",
                400);
        }

        if (settings.AllowTemporaryAssignments
            && settings.TemporaryAssignmentMaxDurationDays is int maxDays
            && endsAt.HasValue)
        {
            var startsAt = effectiveAt ?? DateTimeOffset.UtcNow;
            if (endsAt.Value > startsAt.AddDays(maxDays))
            {
                throw new StlApiException(
                    "org_assignment.temporary_duration",
                    "Temporary assignment duration exceeds this tenant's maximum.",
                    400);
            }
        }

        return new NormalizedAssignmentRequest(
            normalizedStatus,
            isPrimary,
            effectiveAt,
            endsAt,
            normalizedReason);
    }

    private static bool ResolvePrimaryValue(bool? requestedPrimary, bool hasSelectablePrimary) =>
        requestedPrimary ?? !hasSelectablePrimary;

    private async Task<bool> HasSelectablePrimaryAssignmentAsync(
        Guid tenantId,
        Guid personId,
        Guid? excludedAssignmentId,
        CancellationToken cancellationToken) =>
        await db.OrgUnitAssignments.AnyAsync(
            x =>
                x.TenantId == tenantId
                && x.PersonId == personId
                && (excludedAssignmentId == null || x.Id != excludedAssignmentId.Value)
                && x.IsPrimary
                && (x.Status == "planned" || x.Status == "active"),
            cancellationToken);

    private async Task EnsureFallbackPrimaryAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken)
    {
        var hasPrimary = await db.OrgUnitAssignments.AnyAsync(
            x =>
                x.TenantId == tenantId
                && x.PersonId == personId
                && x.IsPrimary
                && (x.Status == "planned" || x.Status == "active"),
            cancellationToken);
        if (hasPrimary)
        {
            return;
        }

        var candidate = await db.OrgUnitAssignments
            .Where(x =>
                x.TenantId == tenantId
                && x.PersonId == personId
                && (x.Status == "planned" || x.Status == "active"))
            .OrderByDescending(x => x.Status == "active")
            .ThenByDescending(x => x.EffectiveAt)
            .ThenByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (candidate is null)
        {
            return;
        }

        candidate.IsPrimary = true;
        candidate.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SyncPrimaryOrgUnitSnapshotAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken)
    {
        var person = await db.People.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == personId,
            cancellationToken);
        if (person is null)
        {
            return;
        }

        var placement = await db.OrgUnitAssignments
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId
                && x.PersonId == personId
                && (x.Status == "planned" || x.Status == "active"))
            .OrderByDescending(x => x.IsPrimary)
            .ThenByDescending(x => x.Status == "active")
            .ThenByDescending(x => x.EffectiveAt)
            .ThenByDescending(x => x.UpdatedAt)
            .Select(x => new { x.DepartmentOrgUnitId })
            .FirstOrDefaultAsync(cancellationToken);

        person.PrimaryOrgUnitId = placement?.DepartmentOrgUnitId;
        person.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task WritePlacementChangeEventsAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid personId,
        OrgUnitAssignment? previous,
        OrgUnitAssignment current,
        CancellationToken cancellationToken)
    {
        if (previous is null || previous.SiteOrgUnitId != current.SiteOrgUnitId)
        {
            await audit.WriteAsync(
                "person.site_changed",
                tenantId,
                actorUserId,
                "person",
                personId.ToString(),
                "Succeeded",
                cancellationToken: cancellationToken);
        }

        if (previous is null || previous.DepartmentOrgUnitId != current.DepartmentOrgUnitId)
        {
            await audit.WriteAsync(
                "person.department_changed",
                tenantId,
                actorUserId,
                "person",
                personId.ToString(),
                "Succeeded",
                cancellationToken: cancellationToken);
        }

        if (previous is null || previous.TeamOrgUnitId != current.TeamOrgUnitId)
        {
            await audit.WriteAsync(
                "person.team_changed",
                tenantId,
                actorUserId,
                "person",
                personId.ToString(),
                "Succeeded",
                cancellationToken: cancellationToken);
        }

        if (previous is null || previous.PositionOrgUnitId != current.PositionOrgUnitId)
        {
            await audit.WriteAsync(
                "person.position_changed",
                tenantId,
                actorUserId,
                "person",
                personId.ToString(),
                "Succeeded",
                cancellationToken: cancellationToken);
        }
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
            throw new StlApiException("org_assignment.validation", $"{fieldName} must be {maxLength} characters or less.", 400);
        }

        return normalized;
    }

    private static System.Linq.Expressions.Expression<Func<OrgUnitAssignment, OrgUnitAssignmentResponse>> ToResponseExpression() =>
        x => new OrgUnitAssignmentResponse(
            x.Id,
            x.PersonId,
            x.SiteOrgUnitId,
            x.DepartmentOrgUnitId,
            x.TeamOrgUnitId,
            x.PositionOrgUnitId,
            x.Status,
            x.CreatedAt,
            x.UpdatedAt,
            x.IsPrimary,
            x.EffectiveAt,
            x.EndsAt,
            x.Reason);

    private sealed record NormalizedAssignmentRequest(
        string Status,
        bool? IsPrimary,
        DateTimeOffset? EffectiveAt,
        DateTimeOffset? EndsAt,
        string? Reason);
}
