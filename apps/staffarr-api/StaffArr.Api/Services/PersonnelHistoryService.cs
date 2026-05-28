using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class PersonnelHistoryService(
    StaffArrDbContext db,
    IStaffArrAuditService audit)
{
    public const string RollupActionScope = "staffarr.personnel.history.rollup";

    public const string IntegrationReadActionScope = "staffarr.personnel.history.read";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f5");

    public async Task<PersonnelHistorySummaryResponse> GetSummaryAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(tenantId, personId, cancellationToken);

        var rollup = await db.PersonnelHistoryRollups.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.PersonId == personId, cancellationToken);

        if (rollup is null)
        {
            return new PersonnelHistorySummaryResponse(
                personId,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                null,
                DateTimeOffset.MinValue,
                IsMaterialized: false);
        }

        return MapSummary(rollup, isMaterialized: true);
    }

    public async Task<PagedResult<PersonTimelineEntryResponse>> ListPersonHistoryAsync(
        Guid tenantId,
        Guid personId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(tenantId, personId, cancellationToken);

        page = page < 1 ? 1 : page;
        pageSize = pageSize switch
        {
            < 1 => 50,
            > 100 => 100,
            _ => pageSize
        };

        var asOf = DateTimeOffset.UtcNow;
        var materialized = await TryListMaterializedEventsAsync(
            tenantId,
            personId,
            page,
            pageSize,
            asOf,
            PersonnelHistoryRules.DefaultReadStalenessHours,
            cancellationToken);

        if (materialized is not null)
        {
            return materialized;
        }

        var liveEntries = await PersonTimelineBuilder.BuildTimelineEntriesAsync(
            db,
            tenantId,
            personId,
            cancellationToken);
        return PageTimelineEntries(liveEntries, page, pageSize);
    }

    public async Task<PagedResult<PersonTimelineEntryResponse>?> TryListMaterializedEventsAsync(
        Guid tenantId,
        Guid personId,
        int page,
        int pageSize,
        DateTimeOffset asOfUtc,
        int stalenessHours,
        CancellationToken cancellationToken = default)
    {
        var rollup = await db.PersonnelHistoryRollups.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.PersonId == personId, cancellationToken);

        if (rollup is null || PersonnelHistoryRules.IsStale(rollup.ComputedAt, asOfUtc, stalenessHours))
        {
            return null;
        }

        var query = db.PersonnelHistoryEvents.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.EntryId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PersonTimelineEntryResponse(
                x.EntryId,
                x.PersonId,
                x.Category,
                x.EventType,
                x.Title,
                x.Detail,
                x.OccurredAt,
                x.ActorUserId,
                x.SourceEntityType,
                x.SourceEntityId,
                x.ExternalReferenceId))
            .ToListAsync(cancellationToken);

        return new PagedResult<PersonTimelineEntryResponse>(
            items,
            page,
            pageSize,
            total,
            page * pageSize < total);
    }

    public async Task<PendingPersonnelHistoryResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        int? stalenessHours,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = PersonnelHistoryRules.NormalizeBatchSize(batchSize);
        var normalizedStalenessHours = PersonnelHistoryRules.NormalizeStalenessHours(stalenessHours);
        var candidates = await LoadPendingCandidatesAsync(
            tenantId,
            asOf,
            normalizedStalenessHours,
            normalizedBatchSize,
            cancellationToken);

        var items = candidates
            .Select(x => new PendingPersonnelHistoryItem(x.PersonId, x.DisplayName, x.LastComputedAt))
            .ToList();

        return new PendingPersonnelHistoryResponse(
            asOf,
            normalizedStalenessHours,
            normalizedBatchSize,
            items);
    }

    public async Task<ProcessPersonnelHistoryResponse> ProcessBatchAsync(
        ProcessPersonnelHistoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = PersonnelHistoryRules.NormalizeBatchSize(request.BatchSize);
        var stalenessHours = PersonnelHistoryRules.NormalizeStalenessHours(request.StalenessHours);
        var candidates = await LoadPendingCandidatesAsync(
            request.TenantId,
            asOf,
            stalenessHours,
            batchSize,
            cancellationToken);

        var refreshed = new List<PersonnelHistorySummaryResponse>();
        var skipped = new List<PersonnelHistoryRefreshSkip>();

        foreach (var candidate in candidates)
        {
            try
            {
                var summary = await RefreshRollupAsync(
                    candidate.TenantId,
                    candidate.PersonId,
                    asOf,
                    cancellationToken);
                refreshed.Add(summary);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new PersonnelHistoryRefreshSkip(candidate.PersonId, ex.Message));
            }
        }

        if (refreshed.Count > 0 && request.TenantId is Guid tenantId)
        {
            await audit.WriteAsync(
                "personnel_history.rollup.refresh.batch",
                tenantId,
                WorkerActorUserId,
                "personnel_history_rollup",
                $"{refreshed.Count}",
                "Succeeded",
                cancellationToken: cancellationToken);
        }

        return new ProcessPersonnelHistoryResponse(
            asOf,
            batchSize,
            stalenessHours,
            candidates.Count,
            refreshed.Count,
            skipped.Count,
            refreshed,
            skipped);
    }

    private async Task<PersonnelHistorySummaryResponse> RefreshRollupAsync(
        Guid tenantId,
        Guid personId,
        DateTimeOffset asOfUtc,
        CancellationToken cancellationToken)
    {
        var entries = await PersonTimelineBuilder.BuildTimelineEntriesAsync(
            db,
            tenantId,
            personId,
            cancellationToken);
        var counts = PersonnelHistoryRules.AggregateCategoryCounts(entries);
        var lastEventAt = entries.Count == 0
            ? (DateTimeOffset?)null
            : entries.Max(x => x.OccurredAt);

        var existing = await db.PersonnelHistoryRollups
            .Include(x => x.Events)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.PersonId == personId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (existing is null)
        {
            existing = new PersonnelHistoryRollup
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PersonId = personId,
                CreatedAt = now
            };
            db.PersonnelHistoryRollups.Add(existing);
        }
        else if (existing.Events.Count > 0)
        {
            db.PersonnelHistoryEvents.RemoveRange(existing.Events);
            existing.Events.Clear();
        }

        existing.EventCount = entries.Count;
        existing.IncidentCount = counts.IncidentCount;
        existing.CertificationCount = counts.CertificationCount;
        existing.PermissionCount = counts.PermissionCount;
        existing.ReadinessCount = counts.ReadinessCount;
        existing.TrainingBlockerCount = counts.TrainingBlockerCount;
        existing.PersonnelNoteCount = counts.PersonnelNoteCount;
        existing.PersonnelDocumentCount = counts.PersonnelDocumentCount;
        existing.LastEventAt = lastEventAt;
        existing.ComputedAt = asOfUtc;
        existing.UpdatedAt = now;

        foreach (var entry in entries)
        {
            existing.Events.Add(new PersonnelHistoryEvent
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PersonId = personId,
                RollupId = existing.Id,
                EntryId = entry.EntryId,
                Category = entry.Category,
                EventType = entry.EventType,
                Title = entry.Title,
                Detail = entry.Detail,
                OccurredAt = entry.OccurredAt,
                ActorUserId = entry.ActorUserId,
                SourceEntityType = entry.SourceEntityType,
                SourceEntityId = entry.SourceEntityId,
                ExternalReferenceId = entry.ExternalReferenceId
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        return MapSummary(existing, isMaterialized: true);
    }

    private async Task<IReadOnlyList<PendingPersonCandidate>> LoadPendingCandidatesAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int stalenessHours,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var peopleQuery = db.People.AsNoTracking()
            .Where(x => x.EmploymentStatus == "active");

        if (tenantId is Guid scopedTenantId)
        {
            peopleQuery = peopleQuery.Where(x => x.TenantId == scopedTenantId);
        }

        var people = await peopleQuery
            .OrderBy(x => x.TenantId)
            .ThenBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);

        var rollupLookup = await db.PersonnelHistoryRollups.AsNoTracking()
            .Where(x => tenantId == null || x.TenantId == tenantId)
            .ToDictionaryAsync(x => (x.TenantId, x.PersonId), cancellationToken);

        var pending = new List<PendingPersonCandidate>();
        foreach (var person in people)
        {
            rollupLookup.TryGetValue((person.TenantId, person.Id), out var rollup);
            if (!PersonnelHistoryRules.IsStale(rollup?.ComputedAt, asOfUtc, stalenessHours))
            {
                continue;
            }

            pending.Add(new PendingPersonCandidate(
                person.TenantId,
                person.Id,
                person.DisplayName,
                rollup?.ComputedAt));
        }

        return pending
            .OrderBy(x => x.LastComputedAt.HasValue ? 1 : 0)
            .ThenBy(x => x.LastComputedAt)
            .Take(batchSize)
            .ToList();
    }

    private static PagedResult<PersonTimelineEntryResponse> PageTimelineEntries(
        List<PersonTimelineEntryResponse> entries,
        int page,
        int pageSize)
    {
        var total = entries.Count;
        var items = entries
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.EntryId, StringComparer.Ordinal)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<PersonTimelineEntryResponse>(
            items,
            page,
            pageSize,
            total,
            page * pageSize < total);
    }

    private async Task EnsurePersonExistsAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken)
    {
        var personExists = await db.People.AnyAsync(
            x => x.TenantId == tenantId && x.Id == personId,
            cancellationToken);
        if (!personExists)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }
    }

    private static PersonnelHistorySummaryResponse MapSummary(
        PersonnelHistoryRollup rollup,
        bool isMaterialized) =>
        new(
            rollup.PersonId,
            rollup.EventCount,
            rollup.IncidentCount,
            rollup.CertificationCount,
            rollup.PermissionCount,
            rollup.ReadinessCount,
            rollup.TrainingBlockerCount,
            rollup.PersonnelNoteCount,
            rollup.PersonnelDocumentCount,
            rollup.LastEventAt,
            rollup.ComputedAt,
            isMaterialized);

    private sealed record PendingPersonCandidate(
        Guid TenantId,
        Guid PersonId,
        string DisplayName,
        DateTimeOffset? LastComputedAt);
}
