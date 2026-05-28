using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;

namespace NexArr.Api.Services;

public sealed class CompanionNotificationDispatchService(
    NexArrDbContext db,
    CompanionNotificationSettingsService settingsService,
    IHttpClientFactory httpClientFactory,
    IPlatformAuditService audit)
{
    public const string ProcessNotificationsActionScope = "nexarr.companion.notifications.dispatch";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000fa");

    public const string WebhookHttpClientName = "NexArrCompanionNotificationWebhook";

    public async Task<PendingCompanionNotificationsResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = CompanionNotificationRules.NormalizeBatchSize(batchSize);
        var items = await LoadPendingAsync(tenantId, normalizedBatchSize, cancellationToken);

        return new PendingCompanionNotificationsResponse(
            asOf,
            normalizedBatchSize,
            items.Select(x => new PendingCompanionNotificationItem(
                x.Id,
                x.TenantId,
                x.EventKind,
                x.ActorUserId,
                x.CreatedAt)).ToList());
    }

    public async Task<ProcessCompanionNotificationsResponse> ProcessBatchAsync(
        ProcessCompanionNotificationsRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = CompanionNotificationRules.NormalizeBatchSize(request.BatchSize);
        var pending = await LoadPendingAsync(request.TenantId, batchSize, cancellationToken);
        var dispatches = new List<CompanionNotificationDispatchResult>();
        var skipped = new List<CompanionNotificationDispatchSkip>();

        foreach (var item in pending)
        {
            try
            {
                var result = await DispatchOneAsync(item, cancellationToken);
                dispatches.Add(result);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new CompanionNotificationDispatchSkip(item.Id, ex.Message));
            }
        }

        if (dispatches.Count > 0 && request.TenantId is Guid tenantId)
        {
            await audit.WriteAsync(
                "companion.notification_dispatch.batch",
                "companion_notification_dispatch",
                $"{dispatches.Count}",
                "Success",
                tenantId: tenantId,
                actorUserId: WorkerActorUserId,
                cancellationToken: cancellationToken);
        }

        return new ProcessCompanionNotificationsResponse(
            asOf,
            batchSize,
            pending.Count,
            dispatches.Count,
            skipped.Count,
            dispatches,
            skipped);
    }

    public async Task<CompanionNotificationDispatchesResponse> ListRecentAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = CompanionNotificationRules.NormalizeDispatchListLimit(limit);
        var rows = await db.CompanionNotificationDispatches
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(x => new CompanionNotificationDispatchItem(
                x.Id,
                x.EventKind,
                x.DispatchStatus,
                x.ActorUserId,
                x.RelatedEntityType,
                x.RelatedEntityId,
                x.WebhookHost,
                x.HttpStatusCode,
                x.ErrorMessage,
                x.CreatedAt,
                x.DispatchedAt))
            .ToList();

        return new CompanionNotificationDispatchesResponse(items);
    }

    private async Task<List<CompanionNotificationDispatch>> LoadPendingAsync(
        Guid? tenantId,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var query = db.CompanionNotificationDispatches
            .Where(x => x.DispatchStatus == CompanionNotificationDispatchStatuses.Pending);

        if (tenantId is Guid scopedTenantId)
        {
            query = query.Where(x => x.TenantId == scopedTenantId);
        }

        return await query
            .OrderBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    private async Task<CompanionNotificationDispatchResult> DispatchOneAsync(
        CompanionNotificationDispatch item,
        CancellationToken cancellationToken)
    {
        var settings = await settingsService.LoadSnapshotAsync(item.TenantId, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        if (settings is null || !CompanionNotificationRules.ShouldNotifyForEvent(settings, item.EventKind))
        {
            return await MarkAsync(
                item,
                CompanionNotificationDispatchStatuses.Skipped,
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
                    CompanionNotificationDispatchStatuses.Sent,
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
                CompanionNotificationDispatchStatuses.Failed,
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
                CompanionNotificationDispatchStatuses.Failed,
                webhookUrl,
                null,
                ex.Message,
                now,
                cancellationToken);
        }
    }

    private static object BuildPayload(CompanionNotificationDispatch item) =>
        item.EventKind switch
        {
            CompanionNotificationEventKinds.HandoffRedeemed => new
            {
                @event = "companion.handoff.redeemed",
                tenantId = item.TenantId,
                actorUserId = item.ActorUserId,
                handoffCodeId = item.RelatedEntityId,
            },
            CompanionNotificationEventKinds.FieldInboxRefreshed => new
            {
                @event = "companion.field_inbox.refreshed",
                tenantId = item.TenantId,
                actorUserId = item.ActorUserId,
                userId = item.RelatedEntityId,
            },
            _ => new
            {
                @event = "companion.notification.unknown",
                tenantId = item.TenantId,
                eventKind = item.EventKind,
                relatedEntityId = item.RelatedEntityId,
            },
        };

    private async Task<CompanionNotificationDispatchResult> MarkAsync(
        CompanionNotificationDispatch item,
        string status,
        string? webhookUrl,
        int? httpStatusCode,
        string? errorMessage,
        DateTimeOffset dispatchedAt,
        CancellationToken cancellationToken)
    {
        item.DispatchStatus = status;
        item.WebhookHost = CompanionNotificationRules.TryGetWebhookHost(webhookUrl);
        item.HttpStatusCode = httpStatusCode;
        item.ErrorMessage = errorMessage is null ? null : Truncate(errorMessage, 512);
        item.DispatchedAt = dispatchedAt;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "companion.notification_dispatch",
            item.RelatedEntityType,
            item.RelatedEntityId.ToString(),
            status,
            tenantId: item.TenantId,
            actorUserId: WorkerActorUserId,
            reasonCode: item.EventKind,
            cancellationToken: cancellationToken);

        return new CompanionNotificationDispatchResult(item.Id, status);
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
