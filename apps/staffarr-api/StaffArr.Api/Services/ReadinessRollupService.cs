using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class ReadinessRollupService(
    StaffArrDbContext db,
    ReadinessService readinessService,
    IStaffArrAuditService audit)
{
    public const string ProcessRollupsActionScope = "staffarr.readiness.rollup";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f3");

    public async Task<IReadOnlyList<ReadinessRollupSummaryResponse>> ListTeamRollupsAsync(
        Guid tenantId,
        Guid? siteOrgUnitId,
        CancellationToken cancellationToken = default)
    {
        var query = db.ReadinessRollups.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ScopeType == ReadinessRollupRules.TeamScope);

        if (siteOrgUnitId is Guid siteId)
        {
            var teamIds = await db.OrgUnitAssignments.AsNoTracking()
                .Where(x => x.TenantId == tenantId
                    && x.Status == "active"
                    && x.SiteOrgUnitId == siteId)
                .Select(x => x.TeamOrgUnitId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (teamIds.Count == 0)
            {
                return [];
            }

            query = query.Where(x => teamIds.Contains(x.OrgUnitId));
        }

        var rollups = await query
            .OrderBy(x => x.OrgUnitName)
            .ToListAsync(cancellationToken);

        return rollups.Select(MapSummary).ToList();
    }

    public async Task<IReadOnlyList<ReadinessRollupSummaryResponse>> ListSiteRollupsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var rollups = await db.ReadinessRollups.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ScopeType == ReadinessRollupRules.SiteScope)
            .OrderBy(x => x.OrgUnitName)
            .ToListAsync(cancellationToken);

        return rollups.Select(MapSummary).ToList();
    }

    public async Task<IReadOnlyList<ReadinessRollupSummaryResponse>> ListDepartmentRollupsAsync(
        Guid tenantId,
        Guid? siteOrgUnitId,
        CancellationToken cancellationToken = default)
    {
        var query = db.ReadinessRollups.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ScopeType == ReadinessRollupRules.DepartmentScope);

        if (siteOrgUnitId is Guid siteId)
        {
            var departmentIds = await db.OrgUnitAssignments.AsNoTracking()
                .Where(x => x.TenantId == tenantId
                    && x.Status == "active"
                    && x.SiteOrgUnitId == siteId)
                .Select(x => x.DepartmentOrgUnitId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (departmentIds.Count == 0)
            {
                return [];
            }

            query = query.Where(x => departmentIds.Contains(x.OrgUnitId));
        }

        var rollups = await query
            .OrderBy(x => x.OrgUnitName)
            .ToListAsync(cancellationToken);

        return rollups.Select(MapSummary).ToList();
    }

    public async Task<ReadinessRollupSummaryResponse> GetRollupAsync(
        Guid tenantId,
        string scopeType,
        Guid orgUnitId,
        CancellationToken cancellationToken = default)
    {
        if (!ReadinessRollupRules.SupportedScopeTypes.Contains(scopeType))
        {
            throw new StlApiException(
                "readiness_rollup.invalid_scope",
                "Readiness rollup scope must be team, site, or department.",
                400);
        }

        var rollup = await db.ReadinessRollups.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.ScopeType == scopeType && x.OrgUnitId == orgUnitId,
                cancellationToken);

        if (rollup is null)
        {
            throw new StlApiException(
                "readiness_rollup.not_found",
                "Readiness rollup has not been computed for this org unit yet.",
                404);
        }

        return MapSummary(rollup);
    }

    public async Task<ReadinessRollupMembersResponse> ListMembersAsync(
        Guid tenantId,
        string scopeType,
        Guid orgUnitId,
        string? readinessStatus,
        CancellationToken cancellationToken = default)
    {
        if (!ReadinessRollupRules.SupportedScopeTypes.Contains(scopeType))
        {
            throw new StlApiException(
                "readiness_rollup.invalid_scope",
                "Readiness rollup scope must be team, site, or department.",
                400);
        }

        if (!string.IsNullOrWhiteSpace(readinessStatus)
            && !ReadinessRollupRules.SupportedMemberReadinessFilters.Contains(readinessStatus))
        {
            throw new StlApiException(
                "readiness_rollup.invalid_member_filter",
                "Member readiness filter must be ready, not_ready, or missing_certifications.",
                400);
        }

        var rollup = await GetRollupAsync(tenantId, scopeType, orgUnitId, cancellationToken);
        var personIds = await ResolveMemberPersonIdsAsync(
            tenantId,
            scopeType,
            orgUnitId,
            cancellationToken);

        if (personIds.Count == 0)
        {
            return new ReadinessRollupMembersResponse(rollup, []);
        }

        var people = await db.People.AsNoTracking()
            .Where(x => x.TenantId == tenantId && personIds.Contains(x.Id))
            .Select(x => new { x.Id, x.DisplayName, x.GivenName, x.FamilyName })
            .ToListAsync(cancellationToken);

        var displayNameLookup = people.ToDictionary(
            x => x.Id,
            x => string.IsNullOrWhiteSpace(x.DisplayName)
                ? $"{x.GivenName} {x.FamilyName}".Trim()
                : x.DisplayName);

        var members = new List<ReadinessRollupMemberResponse>();
        foreach (var personId in personIds)
        {
            var readiness = await readinessService.GetPersonReadinessAsync(
                tenantId,
                personId,
                cancellationToken);

            if (!MatchesMemberFilter(readiness, readinessStatus))
            {
                continue;
            }

            displayNameLookup.TryGetValue(personId, out var displayName);
            members.Add(MapMember(personId, displayName ?? personId.ToString(), readiness));
        }

        var ordered = members
            .OrderBy(x => string.Equals(x.ReadinessStatus, "ready", StringComparison.OrdinalIgnoreCase) ? 1 : 0)
            .ThenBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new ReadinessRollupMembersResponse(rollup, ordered);
    }

    public async Task<PendingReadinessRollupsResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        int? stalenessHours,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = ReadinessRollupRules.NormalizeBatchSize(batchSize);
        var normalizedStalenessHours = ReadinessRollupRules.NormalizeStalenessHours(stalenessHours);
        var candidates = await LoadPendingCandidatesAsync(
            tenantId,
            asOf,
            normalizedStalenessHours,
            normalizedBatchSize,
            cancellationToken);

        var items = candidates
            .Select(x => new PendingReadinessRollupItem(
                x.OrgUnitId,
                x.ScopeType,
                x.OrgUnitName,
                x.LastComputedAt))
            .ToList();

        return new PendingReadinessRollupsResponse(
            asOf,
            normalizedStalenessHours,
            normalizedBatchSize,
            items);
    }

    public async Task<ProcessReadinessRollupsResponse> ProcessBatchAsync(
        ProcessReadinessRollupsRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = ReadinessRollupRules.NormalizeBatchSize(request.BatchSize);
        var stalenessHours = ReadinessRollupRules.NormalizeStalenessHours(request.StalenessHours);
        var candidates = await LoadPendingCandidatesAsync(
            request.TenantId,
            asOf,
            stalenessHours,
            batchSize,
            cancellationToken);

        var refreshed = new List<ReadinessRollupSummaryResponse>();
        var skipped = new List<ReadinessRollupRefreshSkip>();

        foreach (var candidate in candidates)
        {
            try
            {
                var summary = await RefreshRollupAsync(
                    candidate.TenantId,
                    candidate.ScopeType,
                    candidate.OrgUnitId,
                    candidate.OrgUnitName,
                    asOf,
                    cancellationToken);
                refreshed.Add(summary);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new ReadinessRollupRefreshSkip(
                    candidate.OrgUnitId,
                    candidate.ScopeType,
                    ex.Message));
            }
        }

        if (refreshed.Count > 0 && request.TenantId is Guid tenantId)
        {
            await audit.WriteAsync(
                "readiness_rollup.refresh.batch",
                tenantId,
                WorkerActorUserId,
                "readiness_rollup",
                $"{refreshed.Count}",
                "Succeeded",
                cancellationToken: cancellationToken);
        }

        return new ProcessReadinessRollupsResponse(
            asOf,
            batchSize,
            stalenessHours,
            candidates.Count,
            refreshed.Count,
            skipped.Count,
            refreshed,
            skipped);
    }

    private async Task<ReadinessRollupSummaryResponse> RefreshRollupAsync(
        Guid tenantId,
        string scopeType,
        Guid orgUnitId,
        string orgUnitName,
        DateTimeOffset asOfUtc,
        CancellationToken cancellationToken)
    {
        var personIds = await ResolveMemberPersonIdsAsync(
            tenantId,
            scopeType,
            orgUnitId,
            cancellationToken);

        var snapshots = new List<PersonReadinessRollupSnapshot>();
        foreach (var personId in personIds)
        {
            var readiness = await readinessService.GetPersonReadinessAsync(
                tenantId,
                personId,
                cancellationToken);
            snapshots.Add(new PersonReadinessRollupSnapshot(
                personId,
                readiness.ReadinessStatus,
                readiness.ActiveOverride is not null));
        }

        var (readyCount, notReadyCount, overrideCount) = ReadinessRollupRules.AggregateCounts(snapshots);
        var totalMembers = snapshots.Count;
        var readyPercent = ReadinessRollupRules.ComputeReadyPercent(totalMembers, readyCount);

        var existing = await db.ReadinessRollups.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.ScopeType == scopeType && x.OrgUnitId == orgUnitId,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (existing is null)
        {
            existing = new ReadinessRollup
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ScopeType = scopeType,
                OrgUnitId = orgUnitId,
                CreatedAt = now
            };
            db.ReadinessRollups.Add(existing);
        }

        existing.OrgUnitName = orgUnitName;
        existing.TotalMembers = totalMembers;
        existing.ReadyCount = readyCount;
        existing.NotReadyCount = notReadyCount;
        existing.OverrideCount = overrideCount;
        existing.ReadyPercent = readyPercent;
        existing.ComputedAt = asOfUtc;
        existing.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        return MapSummary(existing);
    }

    private async Task<IReadOnlyList<PendingRollupCandidate>> LoadPendingCandidatesAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int stalenessHours,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var orgUnitQuery = db.OrgUnits.AsNoTracking()
            .Where(x => x.Status == "active")
            .Where(x =>
                x.UnitType == ReadinessRollupRules.TeamScope
                || x.UnitType == ReadinessRollupRules.SiteScope
                || x.UnitType == ReadinessRollupRules.DepartmentScope);

        if (tenantId is Guid scopedTenantId)
        {
            orgUnitQuery = orgUnitQuery.Where(x => x.TenantId == scopedTenantId);
        }

        var orgUnits = await orgUnitQuery
            .OrderBy(x => x.TenantId)
            .ThenBy(x => x.UnitType)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var rollupLookup = await db.ReadinessRollups.AsNoTracking()
            .Where(x => tenantId == null || x.TenantId == tenantId)
            .ToDictionaryAsync(
                x => (x.TenantId, x.ScopeType, x.OrgUnitId),
                cancellationToken);

        var pending = new List<PendingRollupCandidate>();
        foreach (var orgUnit in orgUnits)
        {
            rollupLookup.TryGetValue((orgUnit.TenantId, orgUnit.UnitType, orgUnit.Id), out var rollup);
            if (!ReadinessRollupRules.IsStale(rollup?.ComputedAt, asOfUtc, stalenessHours))
            {
                continue;
            }

            pending.Add(new PendingRollupCandidate(
                orgUnit.TenantId,
                orgUnit.UnitType,
                orgUnit.Id,
                orgUnit.Name,
                rollup?.ComputedAt));
        }

        return pending
            .OrderBy(x => x.LastComputedAt.HasValue ? 1 : 0)
            .ThenBy(x => x.LastComputedAt)
            .Take(batchSize)
            .ToList();
    }

    private async Task<IReadOnlyList<Guid>> ResolveMemberPersonIdsAsync(
        Guid tenantId,
        string scopeType,
        Guid orgUnitId,
        CancellationToken cancellationToken)
    {
        var assignmentQuery = db.OrgUnitAssignments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == "active");

        assignmentQuery = scopeType switch
        {
            ReadinessRollupRules.TeamScope => assignmentQuery.Where(x => x.TeamOrgUnitId == orgUnitId),
            ReadinessRollupRules.SiteScope => assignmentQuery.Where(x => x.SiteOrgUnitId == orgUnitId),
            ReadinessRollupRules.DepartmentScope => assignmentQuery.Where(x => x.DepartmentOrgUnitId == orgUnitId),
            _ => throw new StlApiException(
                "readiness_rollup.invalid_scope",
                "Readiness rollup scope must be team, site, or department.",
                400)
        };

        return await assignmentQuery
            .Select(x => x.PersonId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private static ReadinessRollupSummaryResponse MapSummary(ReadinessRollup rollup) =>
        new(
            rollup.OrgUnitId,
            rollup.ScopeType,
            rollup.OrgUnitName,
            rollup.TotalMembers,
            rollup.ReadyCount,
            rollup.NotReadyCount,
            rollup.OverrideCount,
            rollup.ReadyPercent,
            rollup.ComputedAt);

    private static ReadinessRollupMemberResponse MapMember(
        Guid personId,
        string displayName,
        PersonReadinessResponse readiness) =>
        new(
            personId,
            displayName,
            readiness.ReadinessStatus,
            readiness.ReadinessBasis,
            readiness.ActiveOverride is not null,
            readiness.Blockers.Count,
            readiness.Blockers.FirstOrDefault()?.Message);

    private static bool MatchesMemberFilter(PersonReadinessResponse readiness, string? readinessStatus)
    {
        if (string.IsNullOrWhiteSpace(readinessStatus) || string.Equals(readinessStatus, "all", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(readinessStatus, "missing_certifications", StringComparison.OrdinalIgnoreCase))
        {
            return readiness.Requirements.Any(x =>
                string.Equals(x.RequirementStatus, "missing", StringComparison.OrdinalIgnoreCase)
                || string.Equals(x.RequirementStatus, "expired", StringComparison.OrdinalIgnoreCase)
                || string.Equals(x.RequirementStatus, "revoked", StringComparison.OrdinalIgnoreCase));
        }

        return string.Equals(readiness.ReadinessStatus, readinessStatus, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record PendingRollupCandidate(
        Guid TenantId,
        string ScopeType,
        Guid OrgUnitId,
        string OrgUnitName,
        DateTimeOffset? LastComputedAt);
}
