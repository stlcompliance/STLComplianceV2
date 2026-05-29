using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class AssetDowntimeService(
    MaintainArrDbContext db,
    DowntimeTrackingSettingsService settingsService,
    IMaintainArrAuditService audit)
{
    public async Task<IReadOnlyList<AssetDowntimeEventResponse>> ListEventsAsync(
        Guid tenantId,
        Guid? assetId,
        bool? activeOnly,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = AssetDowntimeRules.NormalizeRunListLimit(limit ?? 50);
        var query = db.AssetDowntimeEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (assetId.HasValue)
        {
            query = query.Where(x => x.AssetId == assetId.Value);
        }

        if (activeOnly == true)
        {
            query = query.Where(x => x.EndedAt == null);
        }

        var events = await query
            .OrderByDescending(x => x.StartedAt)
            .Take(normalizedLimit)
            .ToListAsync(cancellationToken);

        return events.Select(MapEvent).ToList();
    }

    public async Task<AssetDowntimeEventResponse> GetEventAsync(
        Guid tenantId,
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.AssetDowntimeEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == eventId, cancellationToken);

        if (entity is null)
        {
            throw new StlApiException("downtime.event_not_found", "Downtime event was not found.", 404);
        }

        return MapEvent(entity);
    }

    public async Task<AssetDowntimeEventResponse> CreateManualEventAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateManualDowntimeEventRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!AssetDowntimeRules.IsManualReason(request.Reason))
        {
            throw new StlApiException(
                "downtime.invalid_reason",
                "Manual downtime reason is not supported.",
                400);
        }

        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.AssetId, cancellationToken);
        if (asset is null)
        {
            throw new StlApiException("assets.not_found", "Asset was not found.", 404);
        }

        if (request.WorkOrderId is Guid workOrderId)
        {
            var workOrderExists = await db.WorkOrders
                .AsNoTracking()
                .AnyAsync(
                    x => x.TenantId == tenantId && x.Id == workOrderId && x.AssetId == request.AssetId,
                    cancellationToken);
            if (!workOrderExists)
            {
                throw new StlApiException("work_orders.not_found", "Work order was not found for this asset.", 404);
            }
        }

        if (request.DefectId is Guid defectId)
        {
            var defectExists = await db.Defects
                .AsNoTracking()
                .AnyAsync(
                    x => x.TenantId == tenantId && x.Id == defectId && x.AssetId == request.AssetId,
                    cancellationToken);
            if (!defectExists)
            {
                throw new StlApiException("defects.not_found", "Defect was not found for this asset.", 404);
            }
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new AssetDowntimeEvent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssetId = asset.Id,
            AssetTag = asset.AssetTag,
            AssetName = asset.Name,
            Source = AssetDowntimeSources.Manual,
            Reason = request.Reason.Trim().ToLowerInvariant(),
            IsPlanned = request.IsPlanned,
            StartedAt = request.StartedAt,
            Notes = NormalizeNotes(request.Notes),
            WorkOrderId = request.WorkOrderId,
            DefectId = request.DefectId,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.AssetDowntimeEvents.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "maintainarr.downtime_event.create",
            tenantId,
            actorUserId,
            "asset_downtime_event",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapEvent(entity);
    }

    public async Task<AssetDowntimeEventResponse> CloseEventAsync(
        Guid tenantId,
        Guid eventId,
        Guid actorUserId,
        CloseDowntimeEventRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.AssetDowntimeEvents
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == eventId, cancellationToken);

        if (entity is null)
        {
            throw new StlApiException("downtime.event_not_found", "Downtime event was not found.", 404);
        }

        if (entity.EndedAt is not null)
        {
            throw new StlApiException("downtime.already_closed", "Downtime event is already closed.", 409);
        }

        var endedAt = request.EndedAt ?? DateTimeOffset.UtcNow;
        if (endedAt < entity.StartedAt)
        {
            throw new StlApiException(
                "downtime.invalid_end_time",
                "End time must be after the downtime start time.",
                400);
        }

        entity.EndedAt = endedAt;
        entity.ClosedByUserId = actorUserId;
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            entity.Notes = string.IsNullOrWhiteSpace(entity.Notes)
                ? request.Notes.Trim()
                : $"{entity.Notes}\n{request.Notes.Trim()}";
        }

        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "maintainarr.downtime_event.close",
            tenantId,
            actorUserId,
            "asset_downtime_event",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapEvent(entity);
    }

    public async Task<DowntimeFollowUpResponse?> TryOpenWorkOrderRepairDowntimeAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid workOrderId,
        Guid assetId,
        string assetTag,
        string assetName,
        CancellationToken cancellationToken = default)
    {
        var hasActiveWorkOrderDowntime = await db.AssetDowntimeEvents
            .AsNoTracking()
            .AnyAsync(
                x => x.TenantId == tenantId
                    && x.WorkOrderId == workOrderId
                    && x.EndedAt == null,
                cancellationToken);
        if (hasActiveWorkOrderDowntime)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new AssetDowntimeEvent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssetId = assetId,
            AssetTag = assetTag,
            AssetName = assetName,
            Source = AssetDowntimeSources.Manual,
            Reason = AssetDowntimeReasons.InRepair,
            IsPlanned = false,
            StartedAt = now,
            StatusTrigger = "work_order:started",
            WorkOrderId = workOrderId,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.AssetDowntimeEvents.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "maintainarr.downtime_event.create",
            tenantId,
            actorUserId,
            "asset_downtime_event",
            entity.Id.ToString(),
            "work_order_started",
            cancellationToken: cancellationToken);

        return new DowntimeFollowUpResponse(
            entity.Id,
            assetId,
            DowntimeDeepLinkBuilder.BuildPath(assetId, workOrderId: workOrderId, eventId: entity.Id),
            entity.Reason,
            "work_order_started");
    }

    public async Task<DowntimeFollowUpResponse?> TryOpenCriticalDefectOutOfServiceDowntimeAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid defectId,
        Guid assetId,
        string assetTag,
        string assetName,
        CancellationToken cancellationToken = default)
    {
        var openEvent = await db.AssetDowntimeEvents
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.AssetId == assetId
                    && x.Source == AssetDowntimeSources.AutomaticStatus
                    && x.EndedAt == null,
                cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (openEvent is null)
        {
            openEvent = new AssetDowntimeEvent
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AssetId = assetId,
                AssetTag = assetTag,
                AssetName = assetName,
                Source = AssetDowntimeSources.AutomaticStatus,
                Reason = AssetDowntimeReasons.OutOfService,
                IsPlanned = false,
                StartedAt = now,
                StatusTrigger = "defect:critical_oos",
                DefectId = defectId,
                CreatedByUserId = actorUserId,
                CreatedAt = now,
                UpdatedAt = now,
            };
            db.AssetDowntimeEvents.Add(openEvent);
        }
        else if (openEvent.DefectId is null)
        {
            openEvent.DefectId = defectId;
            openEvent.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "maintainarr.downtime_event.create",
            tenantId,
            actorUserId,
            "asset_downtime_event",
            openEvent.Id.ToString(),
            "critical_defect_oos",
            cancellationToken: cancellationToken);

        return new DowntimeFollowUpResponse(
            openEvent.Id,
            assetId,
            DowntimeDeepLinkBuilder.BuildPath(assetId, defectId: defectId, eventId: openEvent.Id),
            openEvent.Reason,
            "critical_defect_oos");
    }

    public async Task<AssetAvailabilityResponse> GetAssetAvailabilityAsync(
        Guid tenantId,
        Guid assetId,
        DateTimeOffset? periodStart,
        DateTimeOffset? periodEnd,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await db.AssetAvailabilitySnapshots
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.AssetId == assetId, cancellationToken);

        var settings = await settingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        var periodDays = AssetDowntimeRules.NormalizeAvailabilityPeriodDays(settings?.AvailabilityPeriodDays);
        var (start, end) = ResolvePeriod(periodStart, periodEnd, periodDays);

        if (snapshot is not null
            && snapshot.PeriodStart == start
            && snapshot.PeriodEnd == end
            && snapshot.ComputedAt >= DateTimeOffset.UtcNow.AddHours(-1))
        {
            return MapAssetAvailability(snapshot, isMaterialized: true);
        }

        return await ComputeAssetAvailabilityAsync(tenantId, assetId, start, end, cancellationToken);
    }

    public async Task<FleetAvailabilityResponse> GetFleetAvailabilityAsync(
        Guid tenantId,
        DateTimeOffset? periodStart,
        DateTimeOffset? periodEnd,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await db.FleetAvailabilitySnapshots
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var settings = await settingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        var periodDays = AssetDowntimeRules.NormalizeAvailabilityPeriodDays(settings?.AvailabilityPeriodDays);
        var (start, end) = ResolvePeriod(periodStart, periodEnd, periodDays);

        if (snapshot is not null
            && snapshot.PeriodStart == start
            && snapshot.PeriodEnd == end
            && snapshot.ComputedAt >= DateTimeOffset.UtcNow.AddHours(-1))
        {
            return MapFleetAvailability(snapshot, isMaterialized: true);
        }

        return await ComputeFleetAvailabilityAsync(tenantId, start, end, cancellationToken);
    }

    internal async Task<AssetAvailabilityResponse> ComputeAssetAvailabilityAsync(
        Guid tenantId,
        Guid assetId,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        CancellationToken cancellationToken = default)
    {
        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == assetId, cancellationToken);
        if (asset is null)
        {
            throw new StlApiException("assets.not_found", "Asset was not found.", 404);
        }

        var events = await LoadDowntimeIntervalsAsync(tenantId, assetId, periodStart, periodEnd, cancellationToken);
        var totalHours = (decimal)(periodEnd - periodStart).TotalHours;
        var downtimeHours = AssetDowntimeRules.ComputeDowntimeHoursForPeriod(events, periodStart, periodEnd);
        var (plannedHours, unplannedHours) = AssetDowntimeRules.SplitPlannedDowntimeHours(events, periodStart, periodEnd);
        var hasActive = events.Any(x => x.EndedAt is null);

        return new AssetAvailabilityResponse(
            assetId,
            asset.AssetTag,
            asset.Name,
            periodStart,
            periodEnd,
            totalHours,
            downtimeHours,
            AssetDowntimeRules.ComputeAvailabilityPercent(totalHours, downtimeHours),
            plannedHours,
            unplannedHours,
            hasActive,
            DateTimeOffset.UtcNow,
            IsMaterialized: false);
    }

    internal async Task<FleetAvailabilityResponse> ComputeFleetAvailabilityAsync(
        Guid tenantId,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        CancellationToken cancellationToken = default)
    {
        var assets = await db.Assets
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        decimal totalHours = 0m;
        decimal downtimeHours = 0m;
        decimal plannedHours = 0m;
        decimal unplannedHours = 0m;
        var activeCount = 0;

        foreach (var assetId in assets)
        {
            var assetAvailability = await ComputeAssetAvailabilityAsync(
                tenantId,
                assetId,
                periodStart,
                periodEnd,
                cancellationToken);
            totalHours += assetAvailability.TotalHours;
            downtimeHours += assetAvailability.DowntimeHours;
            plannedHours += assetAvailability.PlannedDowntimeHours;
            unplannedHours += assetAvailability.UnplannedDowntimeHours;
            if (assetAvailability.HasActiveDowntime)
            {
                activeCount++;
            }
        }

        return new FleetAvailabilityResponse(
            periodStart,
            periodEnd,
            assets.Count,
            totalHours,
            downtimeHours,
            AssetDowntimeRules.ComputeAvailabilityPercent(totalHours, downtimeHours),
            plannedHours,
            unplannedHours,
            activeCount,
            DateTimeOffset.UtcNow,
            IsMaterialized: false);
    }

    internal async Task RefreshAssetAvailabilitySnapshotAsync(
        Guid tenantId,
        Guid assetId,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        DateTimeOffset asOfUtc,
        CancellationToken cancellationToken = default)
    {
        var computed = await ComputeAssetAvailabilityAsync(
            tenantId,
            assetId,
            periodStart,
            periodEnd,
            cancellationToken);

        var existing = await db.AssetAvailabilitySnapshots
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.AssetId == assetId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (existing is null)
        {
            existing = new AssetAvailabilitySnapshot
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AssetId = assetId,
                CreatedAt = now,
            };
            db.AssetAvailabilitySnapshots.Add(existing);
        }

        existing.AssetTag = computed.AssetTag;
        existing.AssetName = computed.AssetName;
        existing.PeriodStart = computed.PeriodStart;
        existing.PeriodEnd = computed.PeriodEnd;
        existing.TotalHours = computed.TotalHours;
        existing.DowntimeHours = computed.DowntimeHours;
        existing.AvailabilityPercent = computed.AvailabilityPercent;
        existing.PlannedDowntimeHours = computed.PlannedDowntimeHours;
        existing.UnplannedDowntimeHours = computed.UnplannedDowntimeHours;
        existing.HasActiveDowntime = computed.HasActiveDowntime;
        existing.ComputedAt = asOfUtc;
        existing.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
    }

    internal async Task RefreshFleetAvailabilitySnapshotAsync(
        Guid tenantId,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        DateTimeOffset asOfUtc,
        CancellationToken cancellationToken = default)
    {
        var computed = await ComputeFleetAvailabilityAsync(tenantId, periodStart, periodEnd, cancellationToken);

        var existing = await db.FleetAvailabilitySnapshots
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (existing is null)
        {
            existing = new FleetAvailabilitySnapshot
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.FleetAvailabilitySnapshots.Add(existing);
        }

        existing.PeriodStart = computed.PeriodStart;
        existing.PeriodEnd = computed.PeriodEnd;
        existing.AssetCount = computed.AssetCount;
        existing.TotalHours = computed.TotalHours;
        existing.DowntimeHours = computed.DowntimeHours;
        existing.AvailabilityPercent = computed.AvailabilityPercent;
        existing.PlannedDowntimeHours = computed.PlannedDowntimeHours;
        existing.UnplannedDowntimeHours = computed.UnplannedDowntimeHours;
        existing.ActiveDowntimeEventCount = computed.ActiveDowntimeEventCount;
        existing.ComputedAt = asOfUtc;
        existing.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<DowntimeInterval>> LoadDowntimeIntervalsAsync(
        Guid tenantId,
        Guid assetId,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        CancellationToken cancellationToken)
    {
        var events = await db.AssetDowntimeEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.AssetId == assetId
                && x.StartedAt < periodEnd
                && (x.EndedAt == null || x.EndedAt > periodStart))
            .ToListAsync(cancellationToken);

        return events
            .Select(x => new DowntimeInterval(x.StartedAt, x.EndedAt, x.IsPlanned))
            .ToList();
    }

    private static (DateTimeOffset Start, DateTimeOffset End) ResolvePeriod(
        DateTimeOffset? periodStart,
        DateTimeOffset? periodEnd,
        int periodDays)
    {
        var end = periodEnd ?? DateTimeOffset.UtcNow;
        var start = periodStart ?? end.AddDays(-periodDays);
        if (end <= start)
        {
            throw new StlApiException(
                "downtime.invalid_period",
                "Availability period end must be after period start.",
                400);
        }

        return (start, end);
    }

    private static string? NormalizeNotes(string? notes) =>
        string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

    internal static AssetDowntimeEventResponse MapEvent(AssetDowntimeEvent entity) =>
        new(
            entity.Id,
            entity.AssetId,
            entity.AssetTag,
            entity.AssetName,
            entity.Source,
            entity.Reason,
            entity.IsPlanned,
            entity.StartedAt,
            entity.EndedAt,
            entity.StatusTrigger,
            entity.WorkOrderId,
            entity.DefectId,
            entity.Notes,
            entity.EndedAt is null,
            entity.CreatedAt,
            entity.UpdatedAt);

    internal static AssetAvailabilityResponse MapAssetAvailability(
        AssetAvailabilitySnapshot snapshot,
        bool isMaterialized) =>
        new(
            snapshot.AssetId,
            snapshot.AssetTag,
            snapshot.AssetName,
            snapshot.PeriodStart,
            snapshot.PeriodEnd,
            snapshot.TotalHours,
            snapshot.DowntimeHours,
            snapshot.AvailabilityPercent,
            snapshot.PlannedDowntimeHours,
            snapshot.UnplannedDowntimeHours,
            snapshot.HasActiveDowntime,
            snapshot.ComputedAt,
            isMaterialized);

    internal static FleetAvailabilityResponse MapFleetAvailability(
        FleetAvailabilitySnapshot snapshot,
        bool isMaterialized) =>
        new(
            snapshot.PeriodStart,
            snapshot.PeriodEnd,
            snapshot.AssetCount,
            snapshot.TotalHours,
            snapshot.DowntimeHours,
            snapshot.AvailabilityPercent,
            snapshot.PlannedDowntimeHours,
            snapshot.UnplannedDowntimeHours,
            snapshot.ActiveDowntimeEventCount,
            snapshot.ComputedAt,
            isMaterialized);
}
