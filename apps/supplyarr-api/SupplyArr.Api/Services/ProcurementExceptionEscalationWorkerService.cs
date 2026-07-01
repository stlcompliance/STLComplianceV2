using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class ProcurementExceptionEscalationWorkerService(
    SupplyArrDbContext db,
    ProcurementExceptionEscalationSettingsService settingsService,
    ProcurementNotificationEnqueueService notificationEnqueue,
    ISupplyArrAuditService audit)
{
    public const string ProcessProcurementExceptionEscalationsActionScope =
        "supplyarr.procurement_exceptions.escalate";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000fe");

    public async Task<PendingProcurementExceptionEscalationsResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = ProcurementExceptionEscalationRules.NormalizeBatchSize(batchSize);
        var candidates = await LoadPendingCandidatesAsync(tenantId, asOf, normalizedBatchSize, cancellationToken);

        var items = candidates
            .Select(x => new PendingProcurementExceptionEscalationItem(
                x.ProcurementExceptionId,
                x.ExceptionKey,
                x.SubjectType,
                x.SubjectId,
                x.SubjectKey,
                x.Title,
                x.Status,
                x.SlaDueAt,
                x.EscalationCount,
                x.LastEscalatedAt,
                x.HoursOverdue,
                x.HoursUntilNextEscalation))
            .ToList();

        return new PendingProcurementExceptionEscalationsResponse(asOf, normalizedBatchSize, items);
    }

    public async Task<ProcessProcurementExceptionEscalationsResponse> ProcessBatchAsync(
        ProcessProcurementExceptionEscalationsRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = ProcurementExceptionEscalationRules.NormalizeBatchSize(request.BatchSize);
        var candidates = await LoadPendingCandidatesAsync(request.TenantId, asOf, batchSize, cancellationToken);

        var escalated = new List<ProcurementExceptionEscalationResult>();
        var skipped = new List<ProcurementExceptionEscalationSkip>();
        var runStats = new Dictionary<Guid, (int Candidates, int Escalated, int Skipped)>();

        foreach (var candidate in candidates)
        {
            if (!runStats.ContainsKey(candidate.TenantId))
            {
                runStats[candidate.TenantId] = (0, 0, 0);
            }

            var stats = runStats[candidate.TenantId];
            stats.Candidates++;
            runStats[candidate.TenantId] = stats;

            try
            {
                var result = await EscalateExceptionAsync(candidate.ProcurementExceptionId, asOf, cancellationToken);
                escalated.Add(result);
                stats = runStats[candidate.TenantId];
                stats.Escalated++;
                runStats[candidate.TenantId] = stats;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new ProcurementExceptionEscalationSkip(candidate.ProcurementExceptionId, ex.Message));
                stats = runStats[candidate.TenantId];
                stats.Skipped++;
                runStats[candidate.TenantId] = stats;
            }
        }

        foreach (var (tenantIdKey, stats) in runStats)
        {
            db.ProcurementExceptionEscalationRuns.Add(new ProcurementExceptionEscalationRun
            {
                Id = Guid.NewGuid(),
                TenantId = tenantIdKey,
                AsOfUtc = asOf,
                CandidatesFound = stats.Candidates,
                EscalatedCount = stats.Escalated,
                SkippedCount = stats.Skipped,
                CreatedAt = asOf,
            });
        }

        if (runStats.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        if (request.TenantId is Guid tenantId && escalated.Count > 0)
        {
            await audit.WriteAsync(
                "supplyarr.procurement_exception_escalation.batch",
                tenantId,
                WorkerActorUserId,
                "procurement_exception_escalation_run",
                $"{escalated.Count}",
                "success",
                cancellationToken: cancellationToken);
        }

        return new ProcessProcurementExceptionEscalationsResponse(
            asOf,
            batchSize,
            candidates.Count,
            escalated.Count,
            skipped.Count,
            escalated,
            skipped);
    }

    public async Task<ProcurementExceptionEscalationRunsResponse> ListRecentRunsAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = ProcurementExceptionEscalationRules.NormalizeRunListLimit(limit);
        var runs = await db.ProcurementExceptionEscalationRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(normalizedLimit)
            .Select(x => new ProcurementExceptionEscalationRunItem(
                x.Id,
                x.AsOfUtc,
                x.CandidatesFound,
                x.EscalatedCount,
                x.SkippedCount,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new ProcurementExceptionEscalationRunsResponse(runs);
    }

    public async Task<ProcurementExceptionEscalationEventsResponse> ListRecentEventsAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = ProcurementExceptionEscalationRules.NormalizeEventListLimit(limit);
        var events = await (
            from escalationEvent in db.ProcurementExceptionEscalationEvents.AsNoTracking()
            where escalationEvent.TenantId == tenantId
            join exception in db.ProcurementExceptions.AsNoTracking()
                on escalationEvent.ProcurementExceptionId equals exception.Id
            orderby escalationEvent.CreatedAt descending
            select new ProcurementExceptionEscalationEventItem(
                escalationEvent.Id,
                escalationEvent.ProcurementExceptionId,
                exception.ExceptionKey,
                escalationEvent.EscalationLevel,
                escalationEvent.ActionKind,
                escalationEvent.NotificationDispatchId,
                escalationEvent.CreatedAt))
            .Take(normalizedLimit)
            .ToListAsync(cancellationToken);

        return new ProcurementExceptionEscalationEventsResponse(events);
    }

    private async Task<ProcurementExceptionEscalationResult> EscalateExceptionAsync(
        Guid exceptionId,
        DateTimeOffset asOfUtc,
        CancellationToken cancellationToken)
    {
        var exception = await db.ProcurementExceptions
            .FirstOrDefaultAsync(x => x.Id == exceptionId, cancellationToken)
            ?? throw new InvalidOperationException($"Procurement exception {exceptionId} was not found.");

        var settings = await settingsService.LoadSnapshotAsync(exception.TenantId, cancellationToken)
            ?? throw new InvalidOperationException(
                "Procurement exception escalation settings are not configured for this tenant.");

        if (!ProcurementExceptionEscalationRules.IsDueForEscalation(exception, settings, asOfUtc))
        {
            throw new InvalidOperationException("Procurement exception is not due for SLA escalation.");
        }

        Guid? notificationDispatchId = null;
        if (ProcurementExceptionEscalationRules.ShouldNotify(settings))
        {
            notificationDispatchId = await notificationEnqueue.TryEnqueueRepeatableAsync(
                exception.TenantId,
                ProcurementNotificationEventKinds.ProcurementExceptionSlaEscalation,
                exception.SupplierId,
                "procurement_exception",
                exception.Id,
                cancellationToken);
        }

        var now = DateTimeOffset.UtcNow;
        exception.EscalationCount += 1;
        exception.LastEscalatedAt = asOfUtc;
        exception.UpdatedAt = now;

        db.ProcurementExceptionEscalationEvents.Add(new ProcurementExceptionEscalationEvent
        {
            Id = Guid.NewGuid(),
            TenantId = exception.TenantId,
            ProcurementExceptionId = exception.Id,
            EscalationLevel = exception.EscalationCount,
            ActionKind = notificationDispatchId is null
                ? ProcurementExceptionEscalationActionKinds.Escalated
                : ProcurementExceptionEscalationActionKinds.NotificationEnqueued,
            NotificationDispatchId = notificationDispatchId,
            CreatedAt = now,
        });

        await db.SaveChangesAsync(cancellationToken);

        return new ProcurementExceptionEscalationResult(
            exception.Id,
            exception.ExceptionKey,
            exception.EscalationCount,
            notificationDispatchId);
    }

    private async Task<IReadOnlyList<PendingProcurementExceptionEscalationCandidate>> LoadPendingCandidatesAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var enabledTenantIds = await db.TenantProcurementExceptionEscalationSettings
            .AsNoTracking()
            .Where(x => x.IsEnabled && (tenantId == null || x.TenantId == tenantId))
            .Select(x => x.TenantId)
            .ToListAsync(cancellationToken);

        if (enabledTenantIds.Count == 0)
        {
            return [];
        }

        var settingsByTenant = await db.TenantProcurementExceptionEscalationSettings
            .AsNoTracking()
            .Where(x => enabledTenantIds.Contains(x.TenantId))
            .ToDictionaryAsync(x => x.TenantId, cancellationToken);

        var activeStatuses = ProcurementExceptionStatuses.Active.ToList();
        var exceptions = await db.ProcurementExceptions
            .AsNoTracking()
            .Where(x =>
                enabledTenantIds.Contains(x.TenantId)
                && x.SlaDueAt != null
                && x.SlaDueAt < asOfUtc
                && activeStatuses.Contains(x.Status))
            .OrderBy(x => x.SlaDueAt)
            .Take(batchSize * 4)
            .ToListAsync(cancellationToken);

        var candidates = new List<PendingProcurementExceptionEscalationCandidate>();

        foreach (var exception in exceptions)
        {
            if (!settingsByTenant.TryGetValue(exception.TenantId, out var settingsEntity))
            {
                continue;
            }

            var settings = ProcurementExceptionEscalationSettingsService.ToSnapshot(settingsEntity);
            if (!ProcurementExceptionEscalationRules.IsDueForEscalation(exception, settings, asOfUtc))
            {
                continue;
            }

            var hoursOverdue = ProcurementExceptionEscalationRules.ComputeHoursOverdue(exception.SlaDueAt, asOfUtc);
            var hoursUntilNext = ProcurementExceptionEscalationRules.ComputeHoursUntilNextEscalation(
                exception,
                settings,
                asOfUtc) ?? 0;

            candidates.Add(new PendingProcurementExceptionEscalationCandidate(
                exception.TenantId,
                exception.Id,
                exception.ExceptionKey,
                exception.SubjectType,
                exception.SubjectId,
                exception.SubjectKey,
                exception.Title,
                exception.Status,
                exception.SlaDueAt,
                exception.EscalationCount,
                exception.LastEscalatedAt,
                hoursOverdue,
                hoursUntilNext));
        }

        return candidates
            .OrderByDescending(x => x.HoursOverdue)
            .Take(batchSize)
            .ToList();
    }

    private sealed record PendingProcurementExceptionEscalationCandidate(
        Guid TenantId,
        Guid ProcurementExceptionId,
        string ExceptionKey,
        string SubjectType,
        Guid SubjectId,
        string SubjectKey,
        string Title,
        string Status,
        DateTimeOffset? SlaDueAt,
        int EscalationCount,
        DateTimeOffset? LastEscalatedAt,
        double HoursOverdue,
        double HoursUntilNextEscalation);
}
