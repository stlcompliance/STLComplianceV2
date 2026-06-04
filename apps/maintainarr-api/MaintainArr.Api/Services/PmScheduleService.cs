using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class PmScheduleService(
    MaintainArrDbContext db,
    AssetService assetService,
    PmOccurrenceService pmOccurrences,
    IMaintainArrAuditService audit,
    MaintenancePlatformOutboxEnqueueService platformOutboxEnqueue)
{
    private static readonly HashSet<string> AllowedScheduleStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active",
        "paused"
    };

    public async Task<IReadOnlyList<PmScheduleResponse>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await BuildScheduleQuery(tenantId, dueOnly: false)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PmScheduleResponse>> ListDueAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var schedules = await BuildScheduleQuery(tenantId, dueOnly: true)
            .ToListAsync(cancellationToken);

        return await EnrichWithLinkedWorkOrdersAsync(tenantId, schedules, cancellationToken);
    }

    public async Task<PmScheduleResponse> GetAsync(
        Guid tenantId,
        Guid pmScheduleId,
        CancellationToken cancellationToken = default)
    {
        var schedule = await QuerySchedules(tenantId)
            .FirstOrDefaultAsync(x => x.PmScheduleId == pmScheduleId, cancellationToken);

        if (schedule is null)
        {
            throw new StlApiException("pm_schedule.not_found", "PM schedule was not found.", 404);
        }

        return schedule;
    }

    public async Task<PmScheduleResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreatePmScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = await assetService.GetAsync(tenantId, request.AssetId, cancellationToken);

        var scheduleKey = NormalizeScheduleKey(request.ScheduleKey);
        var exists = await db.PmSchedules.AnyAsync(
            x => x.TenantId == tenantId && x.AssetId == request.AssetId && x.ScheduleKey == scheduleKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "pm_schedule.duplicate_key",
                "A PM schedule with this key already exists for the asset.",
                409);
        }

        var scheduleMode = NormalizeScheduleMode(request.ScheduleMode);
        var meterFields = await ResolveMeterScheduleFieldsAsync(
            tenantId,
            request.AssetId,
            scheduleMode,
            request.AssetMeterId,
            request.IntervalUsage,
            request.NextDueAtUsage,
            cancellationToken);

        var intervalDays = NormalizeIntervalDays(request.IntervalDays);
        var now = DateTimeOffset.UtcNow;
        var entity = new PmSchedule
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssetId = request.AssetId,
            ScheduleKey = scheduleKey,
            Name = NormalizeName(request.Name),
            Description = NormalizeDescription(request.Description),
            ScheduleMode = scheduleMode,
            AssetMeterId = meterFields.AssetMeterId,
            IntervalUsage = meterFields.IntervalUsage,
            NextDueAtUsage = meterFields.NextDueAtUsage,
            IntervalDays = intervalDays,
            NextDueAt = request.NextDueAt,
            DueStatus = PmDueStatuses.Scheduled,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        };

        db.PmSchedules.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "pm_schedule.create",
            tenantId,
            actorUserId,
            "pm_schedule",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await EnqueuePlanLifecycleEventAsync(
            tenantId,
            actorUserId,
            entity,
            MaintenancePlatformOutboxEventKinds.PmPlanCreated,
            $"PM plan {entity.ScheduleKey} created for asset {request.AssetId:D}.",
            "created",
            now,
            cancellationToken);

        await EnqueuePlanLifecycleEventAsync(
            tenantId,
            actorUserId,
            entity,
            MaintenancePlatformOutboxEventKinds.PmPlanActivated,
            $"PM plan {entity.ScheduleKey} activated for asset {request.AssetId:D}.",
            "active",
            now,
            cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<PmScheduleResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid pmScheduleId,
        UpdatePmScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.PmSchedules.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == pmScheduleId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("pm_schedule.not_found", "PM schedule was not found.", 404);
        }

        var scheduleMode = NormalizeScheduleMode(request.ScheduleMode ?? entity.ScheduleMode);
        var meterFields = await ResolveMeterScheduleFieldsAsync(
            tenantId,
            entity.AssetId,
            scheduleMode,
            request.AssetMeterId ?? entity.AssetMeterId,
            request.IntervalUsage ?? entity.IntervalUsage,
            request.NextDueAtUsage ?? entity.NextDueAtUsage,
            cancellationToken);

        entity.Name = NormalizeName(request.Name);
        entity.Description = NormalizeDescription(request.Description);
        entity.ScheduleMode = scheduleMode;
        entity.AssetMeterId = meterFields.AssetMeterId;
        entity.IntervalUsage = meterFields.IntervalUsage;
        entity.NextDueAtUsage = meterFields.NextDueAtUsage;
        entity.IntervalDays = NormalizeIntervalDays(request.IntervalDays);
        entity.NextDueAt = request.NextDueAt;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "pm_schedule.update",
            tenantId,
            actorUserId,
            "pm_schedule",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<PmScheduleResponse> UpdateStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid pmScheduleId,
        UpdatePmScheduleStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var status = request.Status.Trim().ToLowerInvariant();
        if (!AllowedScheduleStatuses.Contains(status))
        {
            throw new StlApiException(
                "pm_schedule.invalid_status",
                "PM schedule status must be active or paused.",
                400);
        }

        var entity = await db.PmSchedules.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == pmScheduleId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("pm_schedule.not_found", "PM schedule was not found.", 404);
        }

        var previousStatus = entity.Status;
        entity.Status = status;
        var now = DateTimeOffset.UtcNow;
        entity.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "pm_schedule.status.update",
            tenantId,
            actorUserId,
            "pm_schedule",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        if (!string.Equals(previousStatus, status, StringComparison.OrdinalIgnoreCase)
            && string.Equals(status, "active", StringComparison.OrdinalIgnoreCase))
        {
            await EnqueuePlanLifecycleEventAsync(
                tenantId,
                actorUserId,
                entity,
                MaintenancePlatformOutboxEventKinds.PmPlanActivated,
                $"PM plan {entity.ScheduleKey} activated for asset {entity.AssetId:D}.",
                "active",
                now,
                cancellationToken);
        }

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<PmScheduleResponse> SkipAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid pmScheduleId,
        SkipPmScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.PmSchedules.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == pmScheduleId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("pm_schedule.not_found", "PM schedule was not found.", 404);
        }

        if (!PmDueScanRules.IsScannableScheduleStatus(entity.Status))
        {
            throw new StlApiException(
                "pm_schedule.not_skippable",
                "Only active PM schedules can be skipped.",
                409);
        }

        if (string.Equals(entity.DueStatus, PmDueStatuses.Completed, StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.DueStatus, PmDueStatuses.Skipped, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "pm_schedule.not_skippable",
                "Completed or already skipped PM schedules cannot be skipped.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        entity.DueStatus = PmDueStatuses.Skipped;
        entity.SkippedAt = now;
        entity.SkippedByPersonId = actorUserId;
        entity.SkippedReason = NormalizeDescription(request.Reason ?? string.Empty);
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await pmOccurrences.MarkSkippedAsync(entity, actorUserId, now, entity.SkippedReason, cancellationToken);
        await audit.WriteAsync(
            "pm_schedule.skip",
            tenantId,
            actorUserId,
            "pm_schedule",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await EnqueueOccurrenceLifecycleEventAsync(
            tenantId,
            actorUserId,
            entity,
            MaintenancePlatformOutboxEventKinds.PmOccurrenceSkipped,
            $"PM occurrence {entity.ScheduleKey} skipped for asset {entity.AssetId:D}.",
            "skipped",
            now,
            cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    private IQueryable<PmScheduleResponse> BuildScheduleQuery(Guid tenantId, bool dueOnly)
    {
        var schedules = db.PmSchedules.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (dueOnly)
        {
            schedules = schedules.Where(x =>
                x.DueStatus == PmDueStatuses.Due || x.DueStatus == PmDueStatuses.Overdue);

            return
                from schedule in schedules
                join asset in db.Assets.AsNoTracking().Where(a => a.TenantId == tenantId)
                    on schedule.AssetId equals asset.Id
                join meter in db.AssetMeters.AsNoTracking().Where(m => m.TenantId == tenantId)
                    on schedule.AssetMeterId equals meter.Id into meterJoin
                from meter in meterJoin.DefaultIfEmpty()
                orderby schedule.NextDueAt, asset.AssetTag
                select new PmScheduleResponse(
                    schedule.Id,
                    schedule.AssetId,
                    asset.AssetTag,
                    asset.Name,
                    schedule.ScheduleKey,
                    schedule.Name,
                    schedule.Description,
                    schedule.ScheduleMode,
                    schedule.AssetMeterId,
                    meter != null ? meter.MeterKey : null,
                    meter != null ? meter.Unit : null,
                    schedule.IntervalUsage,
                    schedule.NextDueAtUsage,
                    schedule.LastCompletedUsage,
                    schedule.IntervalDays,
                    schedule.NextDueAt,
                    schedule.LastCompletedAt,
                    schedule.SkippedAt,
                    schedule.SkippedByPersonId,
                    schedule.SkippedReason,
                    schedule.DueStatus,
                    schedule.Status,
                    schedule.LastDueScanAt,
                    null,
                    null,
                    null,
                    schedule.CreatedAt,
                    schedule.UpdatedAt);
        }

        return
            from schedule in schedules
            join asset in db.Assets.AsNoTracking().Where(a => a.TenantId == tenantId)
                on schedule.AssetId equals asset.Id
            join meter in db.AssetMeters.AsNoTracking().Where(m => m.TenantId == tenantId)
                on schedule.AssetMeterId equals meter.Id into meterJoin
            from meter in meterJoin.DefaultIfEmpty()
            orderby schedule.NextDueAt, schedule.ScheduleKey
            select new PmScheduleResponse(
                schedule.Id,
                schedule.AssetId,
                asset.AssetTag,
                asset.Name,
                schedule.ScheduleKey,
                schedule.Name,
                schedule.Description,
                schedule.ScheduleMode,
                schedule.AssetMeterId,
                meter != null ? meter.MeterKey : null,
                meter != null ? meter.Unit : null,
                schedule.IntervalUsage,
                schedule.NextDueAtUsage,
                schedule.LastCompletedUsage,
                schedule.IntervalDays,
                schedule.NextDueAt,
                schedule.LastCompletedAt,
                schedule.SkippedAt,
                schedule.SkippedByPersonId,
                schedule.SkippedReason,
                schedule.DueStatus,
                schedule.Status,
                schedule.LastDueScanAt,
                null,
                null,
                null,
                schedule.CreatedAt,
                schedule.UpdatedAt);
    }

    private IQueryable<PmScheduleResponse> QuerySchedules(Guid tenantId) =>
        BuildScheduleQuery(tenantId, dueOnly: false);

    private static string NormalizeScheduleKey(string scheduleKey)
    {
        var normalized = scheduleKey.Trim().ToLowerInvariant();
        if (normalized.Length is < 2 or > 128)
        {
            throw new StlApiException(
                "pm_schedule.invalid_key",
                "Schedule key must be between 2 and 128 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeName(string name)
    {
        var normalized = name.Trim();
        if (normalized.Length is < 2 or > 128)
        {
            throw new StlApiException(
                "pm_schedule.invalid_name",
                "Schedule name must be between 2 and 128 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeDescription(string description) =>
        description.Trim().Length <= 512 ? description.Trim() : description.Trim()[..512];

    private static int NormalizeIntervalDays(int intervalDays)
    {
        if (intervalDays is < 1 or > 3650)
        {
            throw new StlApiException(
                "pm_schedule.invalid_interval",
                "Interval days must be between 1 and 3650.",
                400);
        }

        return intervalDays;
    }

    private async Task<MeterScheduleFields> ResolveMeterScheduleFieldsAsync(
        Guid tenantId,
        Guid assetId,
        string scheduleMode,
        Guid? assetMeterId,
        decimal? intervalUsage,
        decimal? nextDueAtUsage,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(scheduleMode, PmScheduleModes.Meter, StringComparison.OrdinalIgnoreCase))
        {
            return new MeterScheduleFields(null, null, null);
        }

        if (!assetMeterId.HasValue)
        {
            throw new StlApiException(
                "pm_schedule.meter_required",
                "Asset meter is required for meter-based PM schedules.",
                400);
        }

        if (!intervalUsage.HasValue || intervalUsage.Value <= 0)
        {
            throw new StlApiException(
                "pm_schedule.invalid_interval_usage",
                "Interval usage must be greater than zero for meter-based schedules.",
                400);
        }

        var meter = await db.AssetMeters.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == assetMeterId.Value && x.AssetId == assetId,
            cancellationToken);
        if (meter is null)
        {
            throw new StlApiException(
                "pm_schedule.meter_not_found",
                "Asset meter was not found on this asset.",
                404);
        }

        var normalizedInterval = AssetMeterService.NormalizeReading(
            intervalUsage.Value,
            "pm_schedule.invalid_interval_usage");
        var resolvedNextDue = nextDueAtUsage.HasValue
            ? AssetMeterService.NormalizeReading(nextDueAtUsage.Value, "pm_schedule.invalid_next_due_usage")
            : MeterPmForecastRules.ComputeInitialNextDueAtUsage(meter.CurrentReading, normalizedInterval);

        return new MeterScheduleFields(assetMeterId, normalizedInterval, resolvedNextDue);
    }

    private static string NormalizeScheduleMode(string? scheduleMode)
    {
        var normalized = string.IsNullOrWhiteSpace(scheduleMode)
            ? PmScheduleModes.Calendar
            : scheduleMode.Trim().ToLowerInvariant();
        if (!string.Equals(normalized, PmScheduleModes.Calendar, StringComparison.Ordinal)
            && !string.Equals(normalized, PmScheduleModes.Meter, StringComparison.Ordinal))
        {
            throw new StlApiException(
                "pm_schedule.invalid_mode",
                "Schedule mode must be calendar or meter.",
                400);
        }

        return normalized;
    }

    private sealed record MeterScheduleFields(
        Guid? AssetMeterId,
        decimal? IntervalUsage,
        decimal? NextDueAtUsage);

    private async Task EnqueuePlanLifecycleEventAsync(
        Guid tenantId,
        Guid actorUserId,
        PmSchedule schedule,
        string eventKind,
        string summary,
        string eventResult,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == schedule.AssetId, cancellationToken);
        if (asset is null)
        {
            return;
        }

        await platformOutboxEnqueue.TryEnqueuePmScheduleEventAsync(
            tenantId,
            eventKind,
            schedule,
            asset,
            actorUserId,
            occurredAt,
            summary,
            eventResult,
            cancellationToken: cancellationToken);
    }

    private async Task EnqueueOccurrenceLifecycleEventAsync(
        Guid tenantId,
        Guid actorUserId,
        PmSchedule schedule,
        string eventKind,
        string summary,
        string eventResult,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == schedule.AssetId, cancellationToken);
        if (asset is null)
        {
            return;
        }

        await platformOutboxEnqueue.TryEnqueuePmOccurrenceEventAsync(
            tenantId,
            eventKind,
            schedule,
            asset,
            actorUserId,
            occurredAt,
            summary,
            eventResult,
            cancellationToken: cancellationToken);
    }

    private async Task<IReadOnlyList<PmScheduleResponse>> EnrichWithLinkedWorkOrdersAsync(
        Guid tenantId,
        IReadOnlyList<PmScheduleResponse> schedules,
        CancellationToken cancellationToken)
    {
        if (schedules.Count == 0)
        {
            return schedules;
        }

        var scheduleIds = schedules.Select(x => x.PmScheduleId).ToList();
        var linkedWorkOrders = await db.WorkOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.PmScheduleId != null
                && scheduleIds.Contains(x.PmScheduleId.Value)
                && WorkOrderStatuses.Active.Contains(x.Status))
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var workOrderByScheduleId = linkedWorkOrders
            .GroupBy(x => x.PmScheduleId!.Value)
            .ToDictionary(x => x.Key, x => x.First());

        return schedules
            .Select(schedule =>
            {
                if (!workOrderByScheduleId.TryGetValue(schedule.PmScheduleId, out var workOrder))
                {
                    return schedule;
                }

                return schedule with
                {
                    LinkedWorkOrderId = workOrder.Id,
                    LinkedWorkOrderNumber = workOrder.WorkOrderNumber,
                    LinkedWorkOrderStatus = workOrder.Status,
                };
            })
            .ToList();
    }
}
