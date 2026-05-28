using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public sealed class DefectEscalationWorkerService(
    MaintainArrDbContext db,
    DefectEscalationSettingsService settingsService,
    WorkOrderService workOrders,
    MaintenanceNotificationEnqueueService notificationEnqueueService,
    IMaintainArrAuditService audit)
{
    public const string ProcessDefectEscalationsActionScope = "maintainarr.defects.escalate";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f6");

    public async Task<PendingDefectEscalationsResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = DefectEscalationRules.NormalizeBatchSize(batchSize);
        var items = await LoadPendingCandidatesAsync(tenantId, asOf, normalizedBatchSize, cancellationToken);
        return new PendingDefectEscalationsResponse(asOf, normalizedBatchSize, items);
    }

    public async Task<ProcessDefectEscalationsResponse> ProcessBatchAsync(
        ProcessDefectEscalationsRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = DefectEscalationRules.NormalizeBatchSize(request.BatchSize);
        var candidates = await LoadPendingCandidatesAsync(request.TenantId, asOf, batchSize, cancellationToken);

        var escalated = new List<DefectEscalationResult>();
        var skipped = new List<DefectEscalationSkip>();
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
                var result = await EscalateDefectAsync(candidate.DefectId, asOf, cancellationToken);
                escalated.Add(result);
                stats = runStats[candidate.TenantId];
                stats.Escalated++;
                runStats[candidate.TenantId] = stats;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new DefectEscalationSkip(candidate.DefectId, ex.Message));
                stats = runStats[candidate.TenantId];
                stats.Skipped++;
                runStats[candidate.TenantId] = stats;
            }
        }

        foreach (var (tenantIdKey, stats) in runStats)
        {
            db.DefectEscalationRuns.Add(new DefectEscalationRun
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
                "maintainarr.defect_escalation.batch",
                tenantId,
                WorkerActorUserId,
                "defect_escalation_run",
                $"{escalated.Count}",
                "success",
                cancellationToken: cancellationToken);
        }

        return new ProcessDefectEscalationsResponse(
            asOf,
            batchSize,
            candidates.Count,
            escalated.Count,
            skipped.Count,
            escalated,
            skipped);
    }

    public async Task<DefectEscalationRunsResponse> ListRecentRunsAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = DefectEscalationRules.NormalizeRunListLimit(limit);
        var runs = await db.DefectEscalationRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(normalizedLimit)
            .Select(x => new DefectEscalationRunItem(
                x.Id,
                x.AsOfUtc,
                x.CandidatesFound,
                x.EscalatedCount,
                x.SkippedCount,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new DefectEscalationRunsResponse(runs);
    }

    public async Task<DefectEscalationEventsResponse> ListRecentEventsAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = DefectEscalationRules.NormalizeEventListLimit(limit);
        var events = await db.DefectEscalationEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(normalizedLimit)
            .Select(x => new DefectEscalationEventItem(
                x.Id,
                x.DefectId,
                x.ActionKind,
                x.PreviousSeverity,
                x.NewSeverity,
                x.PreviousStatus,
                x.NewStatus,
                x.WorkOrderId,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new DefectEscalationEventsResponse(events);
    }

    private async Task<DefectEscalationResult> EscalateDefectAsync(
        Guid defectId,
        DateTimeOffset asOfUtc,
        CancellationToken cancellationToken)
    {
        var defect = await db.Defects
            .FirstOrDefaultAsync(x => x.Id == defectId, cancellationToken)
            ?? throw new InvalidOperationException($"Defect {defectId} was not found.");

        var settings = await settingsService.LoadSnapshotAsync(defect.TenantId, cancellationToken)
            ?? throw new InvalidOperationException("Defect escalation settings are not configured for this tenant.");

        if (!DefectEscalationRules.IsDueForEscalation(defect, settings, asOfUtc))
        {
            throw new InvalidOperationException("Defect is not due for escalation.");
        }

        var actionsTaken = new List<string>();

        if (DefectEscalationRules.ShouldAutoAcknowledge(settings, defect))
        {
            var previousStatus = defect.Status;
            defect.Status = DefectStatuses.Acknowledged;
            defect.UpdatedAt = asOfUtc;
            await RecordEventAsync(
                defect,
                DefectEscalationActionKinds.Acknowledged,
                previousStatus: previousStatus,
                newStatus: defect.Status,
                cancellationToken: cancellationToken);
            actionsTaken.Add(DefectEscalationActionKinds.Acknowledged);
        }

        if (DefectEscalationRules.ShouldBumpSeverity(settings, defect))
        {
            var previousSeverity = defect.Severity;
            var bumped = DefectEscalationRules.BumpSeverity(defect.Severity);
            if (bumped is not null)
            {
                defect.Severity = bumped;
                defect.UpdatedAt = asOfUtc;
                await RecordEventAsync(
                    defect,
                    DefectEscalationActionKinds.SeverityBumped,
                    previousSeverity: previousSeverity,
                    newSeverity: defect.Severity,
                    cancellationToken: cancellationToken);
                actionsTaken.Add(DefectEscalationActionKinds.SeverityBumped);
            }
        }

        if (settings.AutoCreateWorkOrderOnEscalation)
        {
            var hasActiveWorkOrder = await db.WorkOrders.AsNoTracking().AnyAsync(
                x => x.TenantId == defect.TenantId
                    && x.DefectId == defect.Id
                    && WorkOrderStatuses.Active.Contains(x.Status),
                cancellationToken);

            if (!hasActiveWorkOrder)
            {
                var workOrder = await workOrders.CreateFromDefectAsync(
                    defect.TenantId,
                    WorkerActorUserId,
                    defect.Id,
                    new CreateWorkOrderFromDefectRequest(null, null, null, null),
                    cancellationToken);

                await RecordEventAsync(
                    defect,
                    DefectEscalationActionKinds.WorkOrderCreated,
                    workOrderId: workOrder.WorkOrderId,
                    cancellationToken: cancellationToken);
                actionsTaken.Add(DefectEscalationActionKinds.WorkOrderCreated);
            }
        }

        if (settings.NotifyOnEscalation)
        {
            var notificationId = await notificationEnqueueService.TryEnqueueAsync(
                defect.TenantId,
                MaintenanceNotificationEventKinds.DefectEscalated,
                defect.AssetId,
                "defect",
                defect.Id,
                cancellationToken);

            if (notificationId.HasValue)
            {
                await RecordEventAsync(
                    defect,
                    DefectEscalationActionKinds.NotificationEnqueued,
                    cancellationToken: cancellationToken);
                actionsTaken.Add(DefectEscalationActionKinds.NotificationEnqueued);
            }
        }

        defect.LastEscalatedAt = asOfUtc;
        defect.EscalationCount++;
        defect.UpdatedAt = asOfUtc;

        await db.SaveChangesAsync(cancellationToken);

        return new DefectEscalationResult(defect.Id, actionsTaken);
    }

    private async Task RecordEventAsync(
        Defect defect,
        string actionKind,
        string? previousSeverity = null,
        string? newSeverity = null,
        string? previousStatus = null,
        string? newStatus = null,
        Guid? workOrderId = null,
        CancellationToken cancellationToken = default)
    {
        db.DefectEscalationEvents.Add(new DefectEscalationEvent
        {
            Id = Guid.NewGuid(),
            TenantId = defect.TenantId,
            DefectId = defect.Id,
            ActionKind = actionKind,
            PreviousSeverity = previousSeverity,
            NewSeverity = newSeverity,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            WorkOrderId = workOrderId,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        await Task.CompletedTask;
    }

    private async Task<IReadOnlyList<PendingDefectEscalationItem>> LoadPendingCandidatesAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var enabledTenantIds = await db.TenantDefectEscalationSettings
            .AsNoTracking()
            .Where(x => x.IsEnabled && (tenantId == null || x.TenantId == tenantId))
            .Select(x => x.TenantId)
            .ToListAsync(cancellationToken);

        if (enabledTenantIds.Count == 0)
        {
            return [];
        }

        var settingsByTenant = await db.TenantDefectEscalationSettings
            .AsNoTracking()
            .Where(x => enabledTenantIds.Contains(x.TenantId))
            .ToDictionaryAsync(x => x.TenantId, cancellationToken);

        var defects = await db.Defects
            .AsNoTracking()
            .Where(x => enabledTenantIds.Contains(x.TenantId))
            .Where(x =>
                x.Status == DefectStatuses.Open
                || x.Status == DefectStatuses.Acknowledged
                || x.Status == DefectStatuses.InRepair)
            .OrderBy(x => x.UpdatedAt)
            .Take(batchSize * 4)
            .ToListAsync(cancellationToken);

        var pending = new List<PendingDefectEscalationItem>();
        foreach (var defect in defects)
        {
            if (!settingsByTenant.TryGetValue(defect.TenantId, out var settingsEntity))
            {
                continue;
            }

            var snapshot = DefectEscalationSettingsService.ToSnapshot(settingsEntity);
            if (!DefectEscalationRules.IsDueForEscalation(defect, snapshot, asOfUtc))
            {
                continue;
            }

            var anchor = DefectEscalationRules.GetStagnationAnchor(defect);
            var thresholdHours = DefectEscalationRules.GetThresholdHours(snapshot, defect.Severity);
            pending.Add(new PendingDefectEscalationItem(
                defect.Id,
                defect.TenantId,
                defect.AssetId,
                defect.Title,
                defect.Severity,
                defect.Status,
                defect.EscalationCount,
                anchor,
                thresholdHours,
                (asOfUtc - anchor).TotalHours));

            if (pending.Count >= batchSize)
            {
                break;
            }
        }

        return pending;
    }
}
