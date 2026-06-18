using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class PmDueScanService(
    MaintainArrDbContext db,
    WorkOrderService workOrders,
    InspectionRunService inspectionRuns,
    PmOccurrenceService pmOccurrences,
    IMaintainArrAuditService audit,
    MaintenanceNotificationEnqueueService notificationEnqueueService,
    MaintenancePlatformOutboxEnqueueService platformOutboxEnqueue,
    MaintainArrTenantSettingsService tenantSettings)
{
    public const string ProcessDueScanActionScope = "maintainarr.pm.scan";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f4");

    public async Task<PendingPmDueResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = NormalizeBatchSize(batchSize);
        var settings = tenantId.HasValue
            ? await tenantSettings.LoadEffectiveSettingsAsync(tenantId.Value, cancellationToken)
            : MaintainArrTenantSettingsDefaults.Create();
        var dueThrough = asOf.AddDays(settings.PreventiveMaintenance.PmGenerateDaysAhead);
        var items = await LoadPendingCandidatesAsync(tenantId, dueThrough, normalizedBatchSize, cancellationToken);
        return new PendingPmDueResponse(asOf, normalizedBatchSize, items);
    }

    public async Task<ProcessPmDueScanResponse> ProcessBatchAsync(
        ProcessPmDueScanRequest request,
        bool recordRun = false,
        CancellationToken cancellationToken = default)
    {
        if (request.TenantId is null)
        {
            return await ProcessEnabledTenantsAsync(request, recordRun, cancellationToken);
        }

        return await ProcessTenantBatchAsync(request, request.TenantId.Value, recordRun, cancellationToken);
    }

    public async Task<PmDueScanRunsResponse> ListRecentRunsAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = PmDueScanSettingsRules.NormalizeRunListLimit(limit);
        var runs = await db.PmDueScanRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(normalizedLimit)
            .Select(x => new PmDueScanRunItem(
                x.Id,
                x.AsOfUtc,
                x.CandidatesFound,
                x.MarkedDueCount,
                x.MarkedOverdueCount,
                x.SkippedCount,
                x.WorkOrdersCreatedCount,
                x.WorkOrdersLinkedCount,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PmDueScanRunsResponse(runs);
    }

    private async Task<ProcessPmDueScanResponse> ProcessEnabledTenantsAsync(
        ProcessPmDueScanRequest request,
        bool recordRun,
        CancellationToken cancellationToken)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var enabledTenants = await db.TenantPmDueScanSettings
            .AsNoTracking()
            .Where(x => x.IsEnabled)
            .ToListAsync(cancellationToken);

        ProcessPmDueScanResponse? aggregate = null;
        foreach (var settings in enabledTenants)
        {
            if (!PmDueScanSettingsRules.IsScheduledRunDue(settings.LastRunAt, settings.ScanIntervalMinutes, asOf))
            {
                continue;
            }

            var behaviorSettings = await tenantSettings.LoadEffectiveSettingsAsync(settings.TenantId, cancellationToken);
            var tenantRequest = new ProcessPmDueScanRequest(
                settings.TenantId,
                asOf,
                request.BatchSize ?? settings.BatchSize,
                request.OverdueGraceDays ?? behaviorSettings.PreventiveMaintenance.PmGracePeriodDays);

            var result = await ProcessTenantBatchAsync(tenantRequest, settings.TenantId, recordRun, cancellationToken);
            aggregate = aggregate is null ? result : MergeProcessResponses(aggregate, result);
        }

        return aggregate ?? EmptyProcessResponse(asOf, NormalizeBatchSize(request.BatchSize ?? 100));
    }

    private async Task<ProcessPmDueScanResponse> ProcessTenantBatchAsync(
        ProcessPmDueScanRequest request,
        Guid tenantId,
        bool recordRun,
        CancellationToken cancellationToken)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = NormalizeBatchSize(request.BatchSize ?? 100);
        var settings = await tenantSettings.LoadEffectiveSettingsAsync(tenantId, cancellationToken);
        var overdueGraceDays = NormalizeOverdueGraceDays(request.OverdueGraceDays ?? settings.PreventiveMaintenance.PmGracePeriodDays);
        var dueThrough = asOf.AddDays(settings.PreventiveMaintenance.PmGenerateDaysAhead);
        var candidates = await LoadPendingCandidatesAsync(tenantId, dueThrough, batchSize, cancellationToken);

        var updatedIds = new List<Guid>();
        var markedDue = 0;
        var markedOverdue = 0;
        var skipped = new List<PmDueScanSkip>();
        var createdWorkOrderIds = new List<Guid>();
        var workOrdersCreated = 0;
        var workOrdersLinked = 0;
        var workOrderGenerationSkipped = new List<PmWorkOrderGenerationSkip>();

        foreach (var candidate in candidates)
        {
            try
            {
                var outcome = await ApplyDueScanAsync(
                    candidate.PmScheduleId,
                    asOf,
                    overdueGraceDays,
                    cancellationToken);

                var effectiveDueStatus = outcome
                    ?? PmDueScanRules.ResolveTargetDueStatus(
                        "active",
                        candidate.DueStatus,
                        candidate.NextDueAt,
                        asOf,
                        overdueGraceDays);

                if (outcome is not null)
                {
                    updatedIds.Add(candidate.PmScheduleId);
                    if (string.Equals(outcome, PmDueStatuses.Due, StringComparison.OrdinalIgnoreCase))
                    {
                        markedDue++;
                    }
                    else if (string.Equals(outcome, PmDueStatuses.Overdue, StringComparison.OrdinalIgnoreCase))
                    {
                        markedOverdue++;
                    }
                }

                if (PmWorkOrderGenerationRules.ShouldEnsureWorkOrder(effectiveDueStatus))
                {
                    try
                    {
                        var workOrderResult = await workOrders.EnsureForDuePmScheduleAsync(
                            candidate.PmScheduleId,
                            effectiveDueStatus,
                            WorkerActorUserId,
                            cancellationToken);

                        if (workOrderResult.LinkedExisting)
                        {
                            workOrdersLinked++;
                        }
                        else
                        {
                            workOrdersCreated++;
                            createdWorkOrderIds.Add(workOrderResult.WorkOrderId);
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                            workOrderGenerationSkipped.Add(
                                new PmWorkOrderGenerationSkip(candidate.PmScheduleId, ex.Message));
                    }
                }

                await TryGenerateInspectionAsync(candidate.PmScheduleId, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new PmDueScanSkip(candidate.PmScheduleId, ex.Message));
            }
        }

        var response = new ProcessPmDueScanResponse(
            asOf,
            batchSize,
            candidates.Count,
            markedDue,
            markedOverdue,
            skipped.Count,
            workOrdersCreated,
            workOrdersLinked,
            workOrderGenerationSkipped.Count,
            updatedIds,
            createdWorkOrderIds,
            skipped,
            workOrderGenerationSkipped);

        if (recordRun)
        {
            await RecordRunAsync(
                tenantId,
                asOf,
                response,
                cancellationToken);
        }

        return response;
    }

    private async Task RecordRunAsync(
        Guid tenantId,
        DateTimeOffset asOfUtc,
        ProcessPmDueScanResponse response,
        CancellationToken cancellationToken)
    {
        db.PmDueScanRuns.Add(new PmDueScanRun
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AsOfUtc = asOfUtc,
            CandidatesFound = response.CandidatesFound,
            MarkedDueCount = response.MarkedDueCount,
            MarkedOverdueCount = response.MarkedOverdueCount,
            SkippedCount = response.SkippedCount,
            WorkOrdersCreatedCount = response.WorkOrdersCreatedCount,
            WorkOrdersLinkedCount = response.WorkOrdersLinkedCount,
            CreatedAt = asOfUtc,
        });

        var settings = await db.TenantPmDueScanSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        if (settings is not null)
        {
            settings.LastRunAt = asOfUtc;
            settings.UpdatedAt = asOfUtc;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static ProcessPmDueScanResponse EmptyProcessResponse(DateTimeOffset asOfUtc, int batchSize) =>
        new(
            asOfUtc,
            NormalizeBatchSize(batchSize),
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            [],
            [],
            [],
            []);

    private static ProcessPmDueScanResponse MergeProcessResponses(
        ProcessPmDueScanResponse left,
        ProcessPmDueScanResponse right) =>
        new(
            right.AsOfUtc,
            right.BatchSize,
            left.CandidatesFound + right.CandidatesFound,
            left.MarkedDueCount + right.MarkedDueCount,
            left.MarkedOverdueCount + right.MarkedOverdueCount,
            left.SkippedCount + right.SkippedCount,
            left.WorkOrdersCreatedCount + right.WorkOrdersCreatedCount,
            left.WorkOrdersLinkedCount + right.WorkOrdersLinkedCount,
            left.WorkOrderGenerationSkippedCount + right.WorkOrderGenerationSkippedCount,
            left.UpdatedPmScheduleIds.Concat(right.UpdatedPmScheduleIds).ToList(),
            left.CreatedWorkOrderIds.Concat(right.CreatedWorkOrderIds).ToList(),
            left.Skipped.Concat(right.Skipped).ToList(),
            left.WorkOrderGenerationSkipped.Concat(right.WorkOrderGenerationSkipped).ToList());

    private static int NormalizeBatchSize(int batchSize) =>
        batchSize is < 1 or > 500 ? 100 : batchSize;

    public async Task<string?> ApplyDueScanAsync(
        Guid pmScheduleId,
        DateTimeOffset asOfUtc,
        int overdueGraceDays,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.PmSchedules.FirstOrDefaultAsync(
            x => x.Id == pmScheduleId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("pm_schedule.not_found", "PM schedule was not found.", 404);
        }

        if (!PmDueScanRules.IsScannableScheduleStatus(entity.Status))
        {
            throw new StlApiException(
                "pm_schedule.not_scannable",
                $"PM schedule status '{entity.Status}' is not eligible for due scanning.",
                409);
        }

        var targetDueStatus = PmDueScanRules.ResolveTargetDueStatus(
            entity.Status,
            entity.DueStatus,
            entity.NextDueAt,
            asOfUtc,
            overdueGraceDays);
        var previousDueStatus = entity.DueStatus;

        if (string.Equals(targetDueStatus, PmDueStatuses.Due, StringComparison.OrdinalIgnoreCase)
            || string.Equals(targetDueStatus, PmDueStatuses.Overdue, StringComparison.OrdinalIgnoreCase))
        {
            await pmOccurrences.EnsureDueOccurrenceAsync(entity, targetDueStatus, asOfUtc, cancellationToken);
        }

        if (string.Equals(targetDueStatus, entity.DueStatus, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        entity.DueStatus = targetDueStatus;
        entity.LastDueScanAt = asOfUtc;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            targetDueStatus == PmDueStatuses.Overdue
                ? "pm_schedule.due_scan.overdue"
                : "pm_schedule.due_scan.due",
            entity.TenantId,
            WorkerActorUserId,
            "pm_schedule",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        var eventKind = MaintenanceNotificationRules.MapPmDueStatusToEventKind(targetDueStatus);
        if (eventKind is not null)
        {
            await notificationEnqueueService.TryEnqueueAsync(
                entity.TenantId,
                eventKind,
                entity.AssetId,
                "pm_schedule",
                entity.Id,
                cancellationToken);
        }

        await EnqueuePmDuePlatformEventAsync(entity, targetDueStatus, asOfUtc, cancellationToken);
        await EnqueuePmOccurrenceEventsAsync(
            entity,
            previousDueStatus,
            targetDueStatus,
            asOfUtc,
            cancellationToken);

        return targetDueStatus;
    }

    private async Task EnqueuePmDuePlatformEventAsync(
        PmSchedule schedule,
        string targetDueStatus,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        var eventKind = targetDueStatus switch
        {
            PmDueStatuses.Due => MaintenancePlatformOutboxEventKinds.PmDue,
            PmDueStatuses.Overdue => MaintenancePlatformOutboxEventKinds.PmOverdue,
            _ => null,
        };

        if (eventKind is null)
        {
            return;
        }

        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == schedule.TenantId && x.Id == schedule.AssetId, cancellationToken);
        if (asset is null)
        {
            return;
        }

        await platformOutboxEnqueue.TryEnqueuePmScheduleEventAsync(
            schedule.TenantId,
            eventKind,
            schedule,
            asset,
            WorkerActorUserId,
            occurredAt,
            $"PM schedule {schedule.ScheduleKey} changed to {targetDueStatus} for asset {asset.AssetTag}.",
            eventResult: targetDueStatus,
            cancellationToken: cancellationToken);
    }

    private async Task EnqueuePmOccurrenceEventsAsync(
        PmSchedule schedule,
        string previousDueStatus,
        string targetDueStatus,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == schedule.TenantId && x.Id == schedule.AssetId, cancellationToken);
        if (asset is null)
        {
            return;
        }

        await pmOccurrences.EnsureDueOccurrenceAsync(schedule, targetDueStatus, occurredAt, cancellationToken);

        if (string.Equals(previousDueStatus, PmDueStatuses.Scheduled, StringComparison.OrdinalIgnoreCase)
            && (string.Equals(targetDueStatus, PmDueStatuses.Due, StringComparison.OrdinalIgnoreCase)
                || string.Equals(targetDueStatus, PmDueStatuses.Overdue, StringComparison.OrdinalIgnoreCase)))
        {
            await platformOutboxEnqueue.TryEnqueuePmOccurrenceEventAsync(
                schedule.TenantId,
                MaintenancePlatformOutboxEventKinds.PmOccurrenceCreated,
                schedule,
                asset,
                WorkerActorUserId,
                occurredAt,
                $"PM occurrence created for schedule {schedule.ScheduleKey} on asset {asset.AssetTag}.",
                eventResult: targetDueStatus,
                cancellationToken: cancellationToken);
        }

        var occurrenceEventKind = targetDueStatus switch
        {
            PmDueStatuses.Due => MaintenancePlatformOutboxEventKinds.PmOccurrenceDue,
            PmDueStatuses.Overdue => MaintenancePlatformOutboxEventKinds.PmOccurrenceOverdue,
            _ => null,
        };

        if (occurrenceEventKind is null)
        {
            return;
        }

        await platformOutboxEnqueue.TryEnqueuePmOccurrenceEventAsync(
            schedule.TenantId,
            occurrenceEventKind,
            schedule,
            asset,
            WorkerActorUserId,
            occurredAt,
            $"PM occurrence {schedule.ScheduleKey} changed to {targetDueStatus} for asset {asset.AssetTag}.",
            eventResult: targetDueStatus,
            cancellationToken: cancellationToken);
    }

    private async Task TryGenerateInspectionAsync(
        Guid pmScheduleId,
        CancellationToken cancellationToken)
    {
        var program = await db.PmProgramSchedules
            .AsNoTracking()
            .Where(x => x.PmScheduleId == pmScheduleId
                && x.PmProgram.Status == PmProgramStatuses.Active
                && x.PmProgram.AutoGenerateInspection
                && x.PmProgram.InspectionTemplateId.HasValue)
            .Select(x => new
            {
                x.PmProgramId,
                x.PmProgram.InspectionTemplateId,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (program?.InspectionTemplateId is not Guid inspectionTemplateId)
        {
            return;
        }

        var schedule = await db.PmSchedules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == pmScheduleId, cancellationToken);
        if (schedule is null)
        {
            return;
        }

        var existingRun = await db.InspectionRuns.AnyAsync(
            x => x.TenantId == schedule.TenantId
                && x.AssetId == schedule.AssetId
                && x.InspectionTemplateId == inspectionTemplateId
                && x.Status == InspectionRunStatuses.InProgress,
            cancellationToken);
        if (existingRun)
        {
            return;
        }

        await inspectionRuns.StartAsync(
            schedule.TenantId,
            WorkerActorUserId,
            new StartInspectionRunRequest(schedule.AssetId, inspectionTemplateId),
            cancellationToken,
            pmScheduleId);
    }

    private async Task<IReadOnlyList<PendingPmDueItem>> LoadPendingCandidatesAsync(
        Guid? tenantId,
        DateTimeOffset dueThroughUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var query = db.PmSchedules.AsNoTracking()
            .Where(x => PmDueScanRules.ScannableScheduleStatuses.Contains(x.Status))
            .Where(x => PmDueScanRules.UpdatableDueStatuses.Contains(x.DueStatus))
            .Where(x => x.NextDueAt <= dueThroughUtc);

        if (tenantId is Guid scopedTenantId)
        {
            query = query.Where(x => x.TenantId == scopedTenantId);
        }

        return await query
            .Join(
                db.Assets.AsNoTracking(),
                schedule => new { schedule.AssetId, schedule.TenantId },
                asset => new { AssetId = asset.Id, asset.TenantId },
                (schedule, asset) => new { schedule, asset })
            .OrderBy(x => x.schedule.NextDueAt)
            .ThenBy(x => x.schedule.ScheduleKey)
            .Take(batchSize)
            .Select(x => new PendingPmDueItem(
                x.schedule.Id,
                x.schedule.TenantId,
                x.schedule.AssetId,
                x.asset.AssetTag,
                x.asset.Name,
                x.schedule.ScheduleKey,
                x.schedule.DueStatus,
                x.schedule.NextDueAt))
            .ToListAsync(cancellationToken);
    }

    private static int NormalizeOverdueGraceDays(int? overdueGraceDays) =>
        overdueGraceDays is null or < 0 or > 365
            ? PmDueScanRules.DefaultOverdueGraceDays
            : overdueGraceDays.Value;
}
