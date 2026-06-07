using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public sealed class MaintenancePlatformOutboxEnqueueService(
    MaintainArrDbContext db,
    MaintenancePlatformEventSettingsService settingsService,
    MaintenancePlatformEventProcessingService processingService)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<Guid>> TryEnqueueReadinessTransitionsAsync(
        Guid tenantId,
        AssetReadinessResponse readiness,
        string? previousReadinessStatus,
        string? previousLifecycleStatus,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default)
    {
        if (!MaintenancePlatformEventRules.HasReadinessTransition(
                previousReadinessStatus,
                readiness.ReadinessStatus,
                previousLifecycleStatus,
                readiness.LifecycleStatus))
        {
            return [];
        }

        var settings = await settingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        if (!MaintenancePlatformEventRules.ShouldEmitForTenant(settings))
        {
            return [];
        }

        var enqueued = new List<Guid>();
        var summary = BuildReadinessSummary(readiness, previousReadinessStatus, previousLifecycleStatus);
        var payload = new MaintenancePlatformEventPayload(
            readiness.AssetId,
            readiness.AssetTag,
            readiness.AssetName,
            previousReadinessStatus ?? string.Empty,
            readiness.ReadinessStatus,
            previousLifecycleStatus ?? string.Empty,
            readiness.LifecycleStatus,
            readiness.ReadinessBasis,
            readiness.Blockers.Count,
            readiness.Blockers.Count > 0 ? readiness.Blockers[0].Message : null,
            occurredAt,
            summary);

        var readinessEventId = await TryEnqueueAsync(
            tenantId,
            MaintenancePlatformOutboxEventKinds.AssetReadinessChanged,
            MaintenancePlatformEventRelatedEntityTypes.Asset,
            readiness.AssetId,
            MaintenancePlatformEventRules.BuildReadinessChangedIdempotencyKey(
                readiness.AssetId,
                previousReadinessStatus ?? string.Empty,
                readiness.ReadinessStatus,
                previousLifecycleStatus ?? string.Empty,
                readiness.LifecycleStatus),
            payload,
            cancellationToken);

        if (readinessEventId is Guid readinessId)
        {
            enqueued.Add(readinessId);
        }

        if (MaintenancePlatformEventRules.IsOutOfServiceTransition(previousLifecycleStatus, readiness.LifecycleStatus))
        {
            var oosEventId = await TryEnqueueAsync(
                tenantId,
                MaintenancePlatformOutboxEventKinds.AssetOutOfService,
                MaintenancePlatformEventRelatedEntityTypes.Asset,
                readiness.AssetId,
                MaintenancePlatformEventRules.BuildLifecycleTransitionIdempotencyKey(
                    MaintenancePlatformOutboxEventKinds.AssetOutOfService,
                    readiness.AssetId,
                    previousLifecycleStatus ?? string.Empty,
                    readiness.LifecycleStatus),
                payload with
                {
                    Summary = $"Asset {readiness.AssetTag} entered out-of-service.",
                },
                cancellationToken);

            if (oosEventId is Guid oosId)
            {
                enqueued.Add(oosId);
            }
        }
        else if (MaintenancePlatformEventRules.IsReturnedToServiceTransition(
                     previousLifecycleStatus,
                     readiness.LifecycleStatus))
        {
            var returnedEventId = await TryEnqueueAsync(
                tenantId,
                MaintenancePlatformOutboxEventKinds.AssetReturnedToService,
                MaintenancePlatformEventRelatedEntityTypes.Asset,
                readiness.AssetId,
                MaintenancePlatformEventRules.BuildLifecycleTransitionIdempotencyKey(
                    MaintenancePlatformOutboxEventKinds.AssetReturnedToService,
                    readiness.AssetId,
                    previousLifecycleStatus ?? string.Empty,
                    readiness.LifecycleStatus),
                payload with
                {
                    Summary = $"Asset {readiness.AssetTag} returned to service ({readiness.ReadinessStatus}).",
                },
                cancellationToken);

            if (returnedEventId is Guid returnedId)
            {
                enqueued.Add(returnedId);
            }
        }

        return enqueued;
    }

    public async Task<Guid?> TryEnqueueInspectionRunEventAsync(
        Guid tenantId,
        string eventKind,
        InspectionRun run,
        AssetResponse asset,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        string summary,
        string? eventResult = null,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        if (!MaintenancePlatformEventRules.ShouldEmitForTenant(settings))
        {
            return null;
        }

        var payload = BuildEntityPayload(
            asset,
            occurredAt,
            summary,
            MaintenancePlatformEventRelatedEntityTypes.InspectionRun,
            run.Id,
            eventResult,
            actorUserId);

        return await TryEnqueueAsync(
            tenantId,
            eventKind,
            MaintenancePlatformEventRelatedEntityTypes.InspectionRun,
            run.Id,
            MaintenancePlatformEventRules.BuildEntityEventIdempotencyKey(
                eventKind,
                MaintenancePlatformEventRelatedEntityTypes.InspectionRun,
                run.Id),
            payload,
            cancellationToken);
    }

    public async Task<Guid?> TryEnqueueDefectEventAsync(
        Guid tenantId,
        string eventKind,
        Defect defect,
        Asset asset,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        string summary,
        string? eventResult = null,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        if (!MaintenancePlatformEventRules.ShouldEmitForTenant(settings))
        {
            return null;
        }

        var payload = BuildEntityPayload(
            asset.Id,
            asset.AssetTag,
            asset.Name,
            asset.LifecycleStatus,
            occurredAt,
            summary,
            MaintenancePlatformEventRelatedEntityTypes.Defect,
            defect.Id,
            eventResult,
            actorUserId);

        return await TryEnqueueAsync(
            tenantId,
            eventKind,
            MaintenancePlatformEventRelatedEntityTypes.Defect,
            defect.Id,
            MaintenancePlatformEventRules.BuildEntityEventIdempotencyKey(
                eventKind,
                MaintenancePlatformEventRelatedEntityTypes.Defect,
                defect.Id),
            payload,
            cancellationToken);
    }

    public async Task<Guid?> TryEnqueueWorkOrderEventAsync(
        Guid tenantId,
        string eventKind,
        WorkOrder workOrder,
        Asset asset,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        string summary,
        string? eventResult = null,
        string? idempotencyDiscriminator = null,
        CancellationToken cancellationToken = default)
        => await TryEnqueueWorkOrderScopedEventAsync(
            tenantId,
            eventKind,
            MaintenancePlatformEventRelatedEntityTypes.WorkOrder,
            workOrder.Id,
            workOrder,
            asset,
            actorUserId,
            occurredAt,
            summary,
            eventResult,
            idempotencyDiscriminator,
            cancellationToken);

    public async Task<Guid?> TryEnqueueLaborEntryEventAsync(
        Guid tenantId,
        string eventKind,
        WorkOrder workOrder,
        Asset asset,
        WorkOrderLaborEntry laborEntry,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        string summary,
        string? eventResult = null,
        string? idempotencyDiscriminator = null,
        CancellationToken cancellationToken = default)
        => await TryEnqueueWorkOrderScopedEventAsync(
            tenantId,
            eventKind,
            MaintenancePlatformEventRelatedEntityTypes.LaborEntry,
            laborEntry.Id,
            workOrder,
            asset,
            actorUserId,
            occurredAt,
            summary,
            eventResult,
            idempotencyDiscriminator,
            cancellationToken);

    public async Task<Guid?> TryEnqueueVendorWorkEventAsync(
        Guid tenantId,
        string eventKind,
        WorkOrder workOrder,
        Asset asset,
        MaintenanceVendorWork vendorWork,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        string summary,
        string? eventResult = null,
        string? idempotencyDiscriminator = null,
        CancellationToken cancellationToken = default)
        => await TryEnqueueWorkOrderScopedEventAsync(
            tenantId,
            eventKind,
            MaintenancePlatformEventRelatedEntityTypes.VendorWork,
            vendorWork.Id,
            workOrder,
            asset,
            actorUserId,
            occurredAt,
            summary,
            eventResult,
            idempotencyDiscriminator,
            cancellationToken);

    private async Task<Guid?> TryEnqueueWorkOrderScopedEventAsync(
        Guid tenantId,
        string eventKind,
        string targetEntityType,
        Guid targetEntityId,
        WorkOrder workOrder,
        Asset asset,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        string summary,
        string? eventResult,
        string? idempotencyDiscriminator,
        CancellationToken cancellationToken)
    {
        var settings = await settingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        if (!MaintenancePlatformEventRules.ShouldEmitForTenant(settings))
        {
            return null;
        }

        var payload = BuildEntityPayload(
            asset.Id,
            asset.AssetTag,
            asset.Name,
            asset.LifecycleStatus,
            occurredAt,
            summary,
            MaintenancePlatformEventRelatedEntityTypes.WorkOrder,
            workOrder.Id,
            eventResult,
            actorUserId);

        var idempotencyKey = string.IsNullOrWhiteSpace(idempotencyDiscriminator)
            ? MaintenancePlatformEventRules.BuildEntityEventIdempotencyKey(
                eventKind,
                targetEntityType,
                targetEntityId)
            : MaintenancePlatformEventRules.BuildEntityEventIdempotencyKey(
                eventKind,
                targetEntityType,
                targetEntityId,
                idempotencyDiscriminator);

        return await TryEnqueueAsync(
            tenantId,
            eventKind,
            targetEntityType,
            targetEntityId,
            idempotencyKey,
            payload,
            cancellationToken);
    }

    public async Task<Guid?> TryEnqueuePmScheduleEventAsync(
        Guid tenantId,
        string eventKind,
        PmSchedule schedule,
        Asset asset,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        string summary,
        string? eventResult = null,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        if (!MaintenancePlatformEventRules.ShouldEmitForTenant(settings))
        {
            return null;
        }

        var payload = BuildEntityPayload(
            asset.Id,
            asset.AssetTag,
            asset.Name,
            asset.LifecycleStatus,
            occurredAt,
            summary,
            MaintenancePlatformEventRelatedEntityTypes.PmSchedule,
            schedule.Id,
            eventResult,
            actorUserId);

        return await TryEnqueueAsync(
            tenantId,
            eventKind,
            MaintenancePlatformEventRelatedEntityTypes.PmSchedule,
            schedule.Id,
            MaintenancePlatformEventRules.BuildEntityEventIdempotencyKey(
                eventKind,
                MaintenancePlatformEventRelatedEntityTypes.PmSchedule,
                schedule.Id),
            payload,
            cancellationToken);
    }

    public async Task<Guid?> TryEnqueuePmOccurrenceEventAsync(
        Guid tenantId,
        string eventKind,
        PmSchedule schedule,
        Asset asset,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        string summary,
        string? eventResult = null,
        string? idempotencyDiscriminator = null,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        if (!MaintenancePlatformEventRules.ShouldEmitForTenant(settings))
        {
            return null;
        }

        var payload = BuildEntityPayload(
            asset.Id,
            asset.AssetTag,
            asset.Name,
            asset.LifecycleStatus,
            occurredAt,
            summary,
            MaintenancePlatformEventRelatedEntityTypes.PmOccurrence,
            schedule.Id,
            eventResult,
            actorUserId);

        var idempotencyKey = string.IsNullOrWhiteSpace(idempotencyDiscriminator)
            ? MaintenancePlatformEventRules.BuildEntityEventIdempotencyKey(
                eventKind,
                MaintenancePlatformEventRelatedEntityTypes.PmOccurrence,
                schedule.Id)
            : MaintenancePlatformEventRules.BuildEntityEventIdempotencyKey(
                eventKind,
                MaintenancePlatformEventRelatedEntityTypes.PmOccurrence,
                schedule.Id,
                idempotencyDiscriminator);

        return await TryEnqueueAsync(
            tenantId,
            eventKind,
            MaintenancePlatformEventRelatedEntityTypes.PmOccurrence,
            schedule.Id,
            idempotencyKey,
            payload,
            cancellationToken);
    }

    public async Task<Guid?> TryEnqueueMeterReadingEventAsync(
        Guid tenantId,
        string eventKind,
        AssetMeter meter,
        Guid relatedEntityId,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        string summary,
        string? eventResult = null,
        string? idempotencyDiscriminator = null,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        if (!MaintenancePlatformEventRules.ShouldEmitForTenant(settings))
        {
            return null;
        }

        var asset = await db.Assets.AsNoTracking()
            .SingleAsync(
                x => x.TenantId == tenantId && x.Id == meter.AssetId,
                cancellationToken);

        var payload = BuildEntityPayload(
            asset.Id,
            asset.AssetTag,
            asset.Name,
            asset.LifecycleStatus,
            occurredAt,
            summary,
            MaintenancePlatformEventRelatedEntityTypes.MeterReading,
            relatedEntityId,
            eventResult,
            actorUserId);

        var idempotencyKey = string.IsNullOrWhiteSpace(idempotencyDiscriminator)
            ? MaintenancePlatformEventRules.BuildEntityEventIdempotencyKey(
                eventKind,
                MaintenancePlatformEventRelatedEntityTypes.MeterReading,
                relatedEntityId)
            : MaintenancePlatformEventRules.BuildEntityEventIdempotencyKey(
                eventKind,
                MaintenancePlatformEventRelatedEntityTypes.MeterReading,
                relatedEntityId,
                idempotencyDiscriminator);

        return await TryEnqueueAsync(
            tenantId,
            eventKind,
            MaintenancePlatformEventRelatedEntityTypes.MeterReading,
            relatedEntityId,
            idempotencyKey,
            payload,
            cancellationToken);
    }

    public async Task<Guid?> TryEnqueueComponentEventAsync(
        Guid tenantId,
        string eventKind,
        AssetInstalledComponent component,
        Asset asset,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        string summary,
        string? eventResult = null,
        string? idempotencyDiscriminator = null,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        if (!MaintenancePlatformEventRules.ShouldEmitForTenant(settings))
        {
            return null;
        }

        var payload = BuildEntityPayload(
            asset.Id,
            asset.AssetTag,
            asset.Name,
            asset.LifecycleStatus,
            occurredAt,
            summary,
            MaintenancePlatformEventRelatedEntityTypes.Component,
            component.Id,
            eventResult,
            actorUserId);

        var idempotencyKey = string.IsNullOrWhiteSpace(idempotencyDiscriminator)
            ? MaintenancePlatformEventRules.BuildEntityEventIdempotencyKey(
                eventKind,
                MaintenancePlatformEventRelatedEntityTypes.Component,
                component.Id)
            : MaintenancePlatformEventRules.BuildEntityEventIdempotencyKey(
                eventKind,
                MaintenancePlatformEventRelatedEntityTypes.Component,
                component.Id,
                idempotencyDiscriminator);

        return await TryEnqueueAsync(
            tenantId,
            eventKind,
            MaintenancePlatformEventRelatedEntityTypes.Component,
            component.Id,
            idempotencyKey,
            payload,
            cancellationToken);
    }

    private async Task<Guid?> TryEnqueueAsync(
        Guid tenantId,
        string eventKind,
        string relatedEntityType,
        Guid relatedEntityId,
        string idempotencyKey,
        MaintenancePlatformEventPayload payload,
        CancellationToken cancellationToken)
    {
        var duplicate = await db.MaintenancePlatformOutboxEvents.AnyAsync(
            x => x.TenantId == tenantId
                && x.IdempotencyKey == idempotencyKey
                && (x.ProcessingStatus == MaintenancePlatformEventStatuses.Pending
                    || x.ProcessingStatus == MaintenancePlatformEventStatuses.Processed),
            cancellationToken);

        if (duplicate)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var domainEvent = new MaintenancePlatformOutboxEvent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EventKind = eventKind,
            IdempotencyKey = idempotencyKey,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            PayloadJson = JsonSerializer.Serialize(payload, JsonOptions),
            ProcessingStatus = MaintenancePlatformEventStatuses.Pending,
            AttemptCount = 0,
            CorrelationId = Guid.NewGuid(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.MaintenancePlatformOutboxEvents.Add(domainEvent);
        await db.SaveChangesAsync(cancellationToken);

        await processingService.TryProcessSingleAsync(domainEvent, cancellationToken);
        return domainEvent.Id;
    }

    private static string BuildReadinessSummary(
        AssetReadinessResponse readiness,
        string? previousReadinessStatus,
        string? previousLifecycleStatus)
    {
        if (string.IsNullOrWhiteSpace(previousReadinessStatus) && string.IsNullOrWhiteSpace(previousLifecycleStatus))
        {
            return $"Asset {readiness.AssetTag} readiness materialized as {readiness.ReadinessStatus}.";
        }

        return
            $"Asset {readiness.AssetTag} readiness changed from {previousReadinessStatus ?? "unknown"} to {readiness.ReadinessStatus} (lifecycle {previousLifecycleStatus ?? "unknown"} -> {readiness.LifecycleStatus}).";
    }

    private static MaintenancePlatformEventPayload BuildEntityPayload(
        AssetResponse asset,
        DateTimeOffset occurredAt,
        string summary,
        string targetEntityType,
        Guid targetEntityId,
        string? eventResult,
        Guid actorUserId) =>
        BuildEntityPayload(
            asset.AssetId,
            asset.AssetTag,
            asset.Name,
            asset.LifecycleStatus,
            occurredAt,
            summary,
            targetEntityType,
            targetEntityId,
            eventResult,
            actorUserId);

    private static MaintenancePlatformEventPayload BuildEntityPayload(
        Guid assetId,
        string assetTag,
        string assetName,
        string lifecycleStatus,
        DateTimeOffset occurredAt,
        string summary,
        string targetEntityType,
        Guid targetEntityId,
        string? eventResult,
        Guid actorUserId) =>
        new(
            assetId,
            assetTag,
            assetName,
            string.Empty,
            string.Empty,
            string.Empty,
            lifecycleStatus,
            string.Empty,
            0,
            null,
            occurredAt,
            summary,
            targetEntityType,
            targetEntityId,
            eventResult,
            actorUserId);
}
