using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public sealed class MaintenanceNotificationDispatchService(
    MaintainArrDbContext db,
    MaintenanceNotificationSettingsService settingsService,
    IHttpClientFactory httpClientFactory,
    IMaintainArrAuditService audit)
{
    public const string ProcessNotificationsActionScope = "maintainarr.notifications.dispatch";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f5");

    public const string WebhookHttpClientName = "MaintainArrMaintenanceNotificationWebhook";

    public async Task<PendingMaintenanceNotificationsResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = MaintenanceNotificationRules.NormalizeBatchSize(batchSize);
        var items = await LoadPendingAsync(tenantId, normalizedBatchSize, cancellationToken);

        return new PendingMaintenanceNotificationsResponse(
            asOf,
            normalizedBatchSize,
            items.Select(x => new PendingMaintenanceNotificationItem(
                x.Id,
                x.TenantId,
                x.EventKind,
                x.AssetId,
                x.CreatedAt)).ToList());
    }

    public async Task<ProcessMaintenanceNotificationsResponse> ProcessBatchAsync(
        ProcessMaintenanceNotificationsRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = MaintenanceNotificationRules.NormalizeBatchSize(request.BatchSize);
        var enqueuedPmDue = await EnqueuePmDueSchedulesAsync(
            request.TenantId,
            batchSize,
            cancellationToken);

        var pending = await LoadPendingAsync(request.TenantId, batchSize, cancellationToken);
        var dispatches = new List<MaintenanceNotificationDispatchResult>();
        var skipped = new List<MaintenanceNotificationDispatchSkip>();

        foreach (var item in pending)
        {
            try
            {
                var result = await DispatchOneAsync(item, cancellationToken);
                dispatches.Add(result);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new MaintenanceNotificationDispatchSkip(item.Id, ex.Message));
            }
        }

        if (dispatches.Count > 0 && request.TenantId is Guid tenantId)
        {
            await audit.WriteAsync(
                "maintainarr.notification_dispatch.batch",
                tenantId,
                WorkerActorUserId,
                "maintenance_notification_dispatch",
                $"{dispatches.Count}",
                "success",
                cancellationToken: cancellationToken);
        }

        return new ProcessMaintenanceNotificationsResponse(
            asOf,
            batchSize,
            enqueuedPmDue,
            pending.Count,
            dispatches.Count,
            skipped.Count,
            dispatches,
            skipped);
    }

    public async Task<MaintenanceNotificationDispatchesResponse> ListRecentAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = MaintenanceNotificationRules.NormalizeDispatchListLimit(limit);
        var rows = await db.MaintenanceNotificationDispatches
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(x => new MaintenanceNotificationDispatchItem(
                x.Id,
                x.EventKind,
                x.DispatchStatus,
                x.AssetId,
                x.RelatedEntityType,
                x.RelatedEntityId,
                x.WebhookHost,
                x.HttpStatusCode,
                x.ErrorMessage,
                x.CreatedAt,
                x.DispatchedAt))
            .ToList();

        return new MaintenanceNotificationDispatchesResponse(items);
    }

    private async Task<int> EnqueuePmDueSchedulesAsync(
        Guid? tenantId,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var settingsQuery = db.TenantMaintenanceNotificationSettings
            .AsNoTracking()
            .Where(x => x.IsEnabled && (x.NotifyOnPmScheduleDue || x.NotifyOnPmScheduleOverdue));

        if (tenantId is Guid scopedTenantId)
        {
            settingsQuery = settingsQuery.Where(x => x.TenantId == scopedTenantId);
        }

        var tenantSettings = await settingsQuery.ToListAsync(cancellationToken);
        var enqueued = 0;

        foreach (var settings in tenantSettings)
        {
            if (enqueued >= batchSize)
            {
                break;
            }

            var schedules = await db.PmSchedules
                .AsNoTracking()
                .Where(x => x.TenantId == settings.TenantId)
                .Where(x => PmDueScanRules.ScannableScheduleStatuses.Contains(x.Status))
                .Where(x => x.DueStatus == PmDueStatuses.Due || x.DueStatus == PmDueStatuses.Overdue)
                .OrderBy(x => x.NextDueAt)
                .Take(batchSize - enqueued)
                .ToListAsync(cancellationToken);

            foreach (var schedule in schedules)
            {
                var eventKind = MaintenanceNotificationRules.MapPmDueStatusToEventKind(schedule.DueStatus);
                if (eventKind is null)
                {
                    continue;
                }

                var snapshot = MaintenanceNotificationSettingsService.ToSnapshot(settings);
                if (!MaintenanceNotificationRules.ShouldNotifyForEvent(snapshot, eventKind))
                {
                    continue;
                }

                var duplicate = await db.MaintenanceNotificationDispatches.AnyAsync(
                    x => x.TenantId == schedule.TenantId
                        && x.EventKind == eventKind
                        && x.RelatedEntityType == "pm_schedule"
                        && x.RelatedEntityId == schedule.Id
                        && (x.DispatchStatus == MaintenanceNotificationDispatchStatuses.Pending
                            || x.DispatchStatus == MaintenanceNotificationDispatchStatuses.Sent),
                    cancellationToken);

                if (duplicate)
                {
                    continue;
                }

                db.MaintenanceNotificationDispatches.Add(new MaintenanceNotificationDispatch
                {
                    Id = Guid.NewGuid(),
                    TenantId = schedule.TenantId,
                    EventKind = eventKind,
                    AssetId = schedule.AssetId,
                    RelatedEntityType = "pm_schedule",
                    RelatedEntityId = schedule.Id,
                    DispatchStatus = MaintenanceNotificationDispatchStatuses.Pending,
                    CreatedAt = DateTimeOffset.UtcNow,
                });
                enqueued++;
            }
        }

        if (enqueued > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return enqueued;
    }

    private async Task<List<MaintenanceNotificationDispatch>> LoadPendingAsync(
        Guid? tenantId,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var query = db.MaintenanceNotificationDispatches
            .Where(x => x.DispatchStatus == MaintenanceNotificationDispatchStatuses.Pending);

        if (tenantId is Guid scopedTenantId)
        {
            query = query.Where(x => x.TenantId == scopedTenantId);
        }

        return await query
            .OrderBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    private async Task<MaintenanceNotificationDispatchResult> DispatchOneAsync(
        MaintenanceNotificationDispatch item,
        CancellationToken cancellationToken)
    {
        var settings = await settingsService.LoadSnapshotAsync(item.TenantId, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        if (settings is null || !MaintenanceNotificationRules.ShouldNotifyForEvent(settings, item.EventKind))
        {
            return await MarkAsync(
                item,
                MaintenanceNotificationDispatchStatuses.Skipped,
                settings?.NotificationWebhookUrl,
                null,
                "notifications_disabled_or_webhook_missing",
                now,
                cancellationToken);
        }

        var payload = BuildPayload(item);
        var webhookUrl = settings.NotificationWebhookUrl!;

        try
        {
            var client = httpClientFactory.CreateClient(WebhookHttpClientName);
            using var response = await client.PostAsJsonAsync(webhookUrl, payload, cancellationToken);
            var statusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                return await MarkAsync(
                    item,
                    MaintenanceNotificationDispatchStatuses.Sent,
                    webhookUrl,
                    statusCode,
                    null,
                    now,
                    cancellationToken);
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var truncated = body.Length > 200 ? body[..200] : body;
            return await MarkAsync(
                item,
                MaintenanceNotificationDispatchStatuses.Failed,
                webhookUrl,
                statusCode,
                $"http_{statusCode}:{truncated}",
                now,
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return await MarkAsync(
                item,
                MaintenanceNotificationDispatchStatuses.Failed,
                webhookUrl,
                null,
                ex.Message,
                now,
                cancellationToken);
        }
    }

    private static object BuildPayload(MaintenanceNotificationDispatch item) =>
        item.EventKind switch
        {
            MaintenanceNotificationEventKinds.WorkOrderCreated => new
            {
                @event = "maintainarr.work_order.created",
                tenantId = item.TenantId,
                assetId = item.AssetId,
                workOrderId = item.RelatedEntityId,
            },
            MaintenanceNotificationEventKinds.PmScheduleDue => new
            {
                @event = "maintainarr.pm_schedule.due",
                tenantId = item.TenantId,
                assetId = item.AssetId,
                pmScheduleId = item.RelatedEntityId,
            },
            MaintenanceNotificationEventKinds.PmScheduleOverdue => new
            {
                @event = "maintainarr.pm_schedule.overdue",
                tenantId = item.TenantId,
                assetId = item.AssetId,
                pmScheduleId = item.RelatedEntityId,
            },
            _ => new
            {
                @event = "maintainarr.notification.unknown",
                tenantId = item.TenantId,
                eventKind = item.EventKind,
                relatedEntityId = item.RelatedEntityId,
            },
        };

    private async Task<MaintenanceNotificationDispatchResult> MarkAsync(
        MaintenanceNotificationDispatch item,
        string status,
        string? webhookUrl,
        int? httpStatusCode,
        string? errorMessage,
        DateTimeOffset dispatchedAt,
        CancellationToken cancellationToken)
    {
        item.DispatchStatus = status;
        item.WebhookHost = MaintenanceNotificationRules.TryGetWebhookHost(webhookUrl);
        item.HttpStatusCode = httpStatusCode;
        item.ErrorMessage = errorMessage is null ? null : Truncate(errorMessage, 512);
        item.DispatchedAt = dispatchedAt;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "maintainarr.notification_dispatch",
            item.TenantId,
            WorkerActorUserId,
            item.RelatedEntityType,
            item.RelatedEntityId.ToString(),
            status,
            reasonCode: item.EventKind,
            cancellationToken: cancellationToken);

        return new MaintenanceNotificationDispatchResult(item.Id, status);
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
