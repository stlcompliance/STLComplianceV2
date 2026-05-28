using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public sealed class DispatchNotificationDispatchService(
    RoutArrDbContext db,
    DispatchNotificationSettingsService settingsService,
    IHttpClientFactory httpClientFactory,
    IRoutArrAuditService audit)
{
    public const string ProcessNotificationsActionScope = "routarr.notifications.dispatch";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f6");

    public const string WebhookHttpClientName = "RoutArrDispatchNotificationWebhook";

    public async Task<PendingDispatchNotificationsResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = DispatchNotificationRules.NormalizeBatchSize(batchSize);
        var items = await LoadPendingAsync(tenantId, normalizedBatchSize, cancellationToken);

        return new PendingDispatchNotificationsResponse(
            asOf,
            normalizedBatchSize,
            items.Select(x => new PendingDispatchNotificationItem(
                x.Id,
                x.TenantId,
                x.EventKind,
                x.TripId,
                x.CreatedAt)).ToList());
    }

    public async Task<ProcessDispatchNotificationsResponse> ProcessBatchAsync(
        ProcessDispatchNotificationsRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = DispatchNotificationRules.NormalizeBatchSize(request.BatchSize);
        var pending = await LoadPendingAsync(request.TenantId, batchSize, cancellationToken);
        var dispatches = new List<DispatchNotificationDispatchResult>();
        var skipped = new List<DispatchNotificationDispatchSkip>();

        foreach (var item in pending)
        {
            try
            {
                var result = await DispatchOneAsync(item, cancellationToken);
                dispatches.Add(result);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new DispatchNotificationDispatchSkip(item.Id, ex.Message));
            }
        }

        if (dispatches.Count > 0 && request.TenantId is Guid tenantId)
        {
            await audit.WriteAsync(
                "routarr.notification_dispatch.batch",
                tenantId,
                WorkerActorUserId,
                "dispatch_notification_dispatch",
                $"{dispatches.Count}",
                "success",
                cancellationToken: cancellationToken);
        }

        return new ProcessDispatchNotificationsResponse(
            asOf,
            batchSize,
            pending.Count,
            dispatches.Count,
            skipped.Count,
            dispatches,
            skipped);
    }

    public async Task<DispatchNotificationDispatchesResponse> ListRecentAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = DispatchNotificationRules.NormalizeDispatchListLimit(limit);
        var rows = await db.DispatchNotificationDispatches
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(x => new DispatchNotificationDispatchItem(
                x.Id,
                x.EventKind,
                x.DispatchStatus,
                x.TripId,
                x.DriverPersonId,
                x.RelatedEntityType,
                x.RelatedEntityId,
                x.WebhookHost,
                x.HttpStatusCode,
                x.ErrorMessage,
                x.CreatedAt,
                x.DispatchedAt))
            .ToList();

        return new DispatchNotificationDispatchesResponse(items);
    }

    private async Task<List<DispatchNotificationDispatch>> LoadPendingAsync(
        Guid? tenantId,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var query = db.DispatchNotificationDispatches
            .Where(x => x.DispatchStatus == DispatchNotificationDispatchStatuses.Pending);

        if (tenantId is Guid scopedTenantId)
        {
            query = query.Where(x => x.TenantId == scopedTenantId);
        }

        return await query
            .OrderBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    private async Task<DispatchNotificationDispatchResult> DispatchOneAsync(
        DispatchNotificationDispatch item,
        CancellationToken cancellationToken)
    {
        var settings = await settingsService.LoadSnapshotAsync(item.TenantId, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        if (settings is null || !DispatchNotificationRules.ShouldNotifyForEvent(settings, item.EventKind))
        {
            return await MarkAsync(
                item,
                DispatchNotificationDispatchStatuses.Skipped,
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
                    DispatchNotificationDispatchStatuses.Sent,
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
                DispatchNotificationDispatchStatuses.Failed,
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
                DispatchNotificationDispatchStatuses.Failed,
                webhookUrl,
                null,
                ex.Message,
                now,
                cancellationToken);
        }
    }

    private static object BuildPayload(DispatchNotificationDispatch item) =>
        item.EventKind switch
        {
            DispatchNotificationEventKinds.TripAssigned => new
            {
                @event = "routarr.trip.assigned",
                tenantId = item.TenantId,
                tripId = item.TripId,
                driverPersonId = item.DriverPersonId,
            },
            DispatchNotificationEventKinds.TripDispatched => new
            {
                @event = "routarr.trip.dispatched",
                tenantId = item.TenantId,
                tripId = item.TripId,
                driverPersonId = item.DriverPersonId,
            },
            DispatchNotificationEventKinds.TripInProgress => new
            {
                @event = "routarr.trip.in_progress",
                tenantId = item.TenantId,
                tripId = item.TripId,
                driverPersonId = item.DriverPersonId,
            },
            DispatchNotificationEventKinds.TripCompleted => new
            {
                @event = "routarr.trip.completed",
                tenantId = item.TenantId,
                tripId = item.TripId,
                driverPersonId = item.DriverPersonId,
            },
            DispatchNotificationEventKinds.TripCancelled => new
            {
                @event = "routarr.trip.cancelled",
                tenantId = item.TenantId,
                tripId = item.TripId,
                driverPersonId = item.DriverPersonId,
            },
            _ => new
            {
                @event = "routarr.notification.unknown",
                tenantId = item.TenantId,
                eventKind = item.EventKind,
                tripId = item.TripId,
            },
        };

    private async Task<DispatchNotificationDispatchResult> MarkAsync(
        DispatchNotificationDispatch item,
        string status,
        string? webhookUrl,
        int? httpStatusCode,
        string? errorMessage,
        DateTimeOffset dispatchedAt,
        CancellationToken cancellationToken)
    {
        item.DispatchStatus = status;
        item.WebhookHost = DispatchNotificationRules.TryGetWebhookHost(webhookUrl);
        item.HttpStatusCode = httpStatusCode;
        item.ErrorMessage = errorMessage is null ? null : Truncate(errorMessage, 512);
        item.DispatchedAt = dispatchedAt;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "routarr.notification_dispatch",
            item.TenantId,
            WorkerActorUserId,
            item.RelatedEntityType,
            item.RelatedEntityId.ToString(),
            status,
            reasonCode: item.EventKind,
            cancellationToken: cancellationToken);

        return new DispatchNotificationDispatchResult(item.Id, status);
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
