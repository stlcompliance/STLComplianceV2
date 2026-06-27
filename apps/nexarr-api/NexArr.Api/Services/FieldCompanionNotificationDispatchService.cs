using System.Net.Http.Json;

using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Options;

using NexArr.Api.Contracts;

using NexArr.Api.Data;

using NexArr.Api.Entities;

using NexArr.Api.Options;



namespace NexArr.Api.Services;



public sealed class FieldCompanionNotificationDispatchService(

    NexArrDbContext db,

    FieldCompanionNotificationEnqueueService enqueueService,

    FieldCompanionNotificationSettingsService settingsService,

    FieldCompanionPushSubscriptionService pushSubscriptionService,

    IFieldCompanionWebPushSender webPushSender,

    IHttpClientFactory httpClientFactory,

    IOptions<FieldCompanionWebPushOptions> webPushOptions,

    IPlatformAuditService audit)

{

    public const string ProcessNotificationsActionScope = "nexarr.fieldcompanion.notifications.dispatch";



    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000fa");



    public const string WebhookHttpClientName = "NexArrFieldCompanionNotificationWebhook";



    public async Task<PendingFieldCompanionNotificationsResponse> ListPendingAsync(

        Guid? tenantId,

        DateTimeOffset? asOfUtc,

        int? batchSize,

        CancellationToken cancellationToken = default)

    {

        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;

        var normalizedBatchSize = FieldCompanionNotificationRules.NormalizeBatchSize(batchSize);

        var items = await LoadPendingAsync(tenantId, normalizedBatchSize, cancellationToken);



        return new PendingFieldCompanionNotificationsResponse(

            asOf,

            normalizedBatchSize,

            items.Select(ToPendingItem).ToList());

    }


    public async Task<FieldCompanionNotificationDispatchItem> SendTestAsync(

        Guid tenantId,

        Guid actorUserId,

        CancellationToken cancellationToken = default)

    {

        var dispatch = await enqueueService.CreatePendingDispatchAsync(

            tenantId,

            FieldCompanionNotificationEventKinds.TestNotification,

            actorUserId,

            "fieldcompanion_notification_test",

            Guid.NewGuid(),

            cancellationToken);



        await DispatchOneAsync(dispatch, cancellationToken);



        return ToItem(dispatch);

    }



    public async Task<ProcessFieldCompanionNotificationsResponse> ProcessBatchAsync(

        ProcessFieldCompanionNotificationsRequest request,

        CancellationToken cancellationToken = default)

    {

        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;

        var batchSize = FieldCompanionNotificationRules.NormalizeBatchSize(request.BatchSize);

        var pending = await LoadPendingAsync(request.TenantId, batchSize, cancellationToken);

        var dispatches = new List<FieldCompanionNotificationDispatchResult>();

        var skipped = new List<FieldCompanionNotificationDispatchSkip>();



        foreach (var item in pending)

        {

            try

            {

                var result = await DispatchOneAsync(item, cancellationToken);

                dispatches.Add(result);

            }

            catch (Exception ex) when (ex is not OperationCanceledException)

            {

                skipped.Add(new FieldCompanionNotificationDispatchSkip(item.Id, ex.Message));

            }

        }



        if (dispatches.Count > 0 && request.TenantId is Guid tenantId)

        {

            await audit.WriteAsync(

                "fieldcompanion.notification_dispatch.batch",

                "fieldcompanion_notification_dispatch",

                $"{dispatches.Count}",

                "Success",

                tenantId: tenantId,

                actorUserId: WorkerActorUserId,

                cancellationToken: cancellationToken);

        }



        return new ProcessFieldCompanionNotificationsResponse(

            asOf,

            batchSize,

            pending.Count,

            dispatches.Count,

            skipped.Count,

            dispatches,

            skipped);

    }



    public async Task<FieldCompanionNotificationDispatchesResponse> ListRecentAsync(

        Guid tenantId,

        int? limit,

        CancellationToken cancellationToken = default)

    {

        var take = FieldCompanionNotificationRules.NormalizeDispatchListLimit(limit);

        var rows = await db.FieldCompanionNotificationDispatches

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId)

            .OrderByDescending(x => x.CreatedAt)

            .Take(take)

            .ToListAsync(cancellationToken);



        var items = rows

            .Select(ToItem)

            .ToList();



        return new FieldCompanionNotificationDispatchesResponse(items);

    }



    private async Task<List<FieldCompanionNotificationDispatch>> LoadPendingAsync(

        Guid? tenantId,

        int batchSize,

        CancellationToken cancellationToken)

    {

        var query = db.FieldCompanionNotificationDispatches

            .Where(x => x.DispatchStatus == FieldCompanionNotificationDispatchStatuses.Pending);



        if (tenantId is Guid scopedTenantId)

        {

            query = query.Where(x => x.TenantId == scopedTenantId);

        }



        return await query

            .OrderBy(x => x.CreatedAt)

            .Take(batchSize)

            .ToListAsync(cancellationToken);

    }



    private async Task<FieldCompanionNotificationDispatchResult> DispatchOneAsync(

        FieldCompanionNotificationDispatch item,

        CancellationToken cancellationToken)

    {

        var settings = await settingsService.LoadSnapshotAsync(item.TenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;



        if (settings is null || !FieldCompanionNotificationRules.EventKindEnabled(settings, item.EventKind))

        {

            return await MarkAsync(

                item,

                FieldCompanionNotificationDispatchStatuses.Skipped,

                null,

                null,

                null,

                "notifications_disabled",

                now,

                cancellationToken);

        }



        var webhookAttempted = false;

        var webhookSent = false;

        string? webhookError = null;

        string? webhookHost = null;

        int? webhookStatusCode = null;



        if (FieldCompanionNotificationRules.ShouldNotifyForEvent(settings, item.EventKind))

        {

            webhookAttempted = true;

            var webhookUrl = settings.NotificationWebhookUrl!;

            webhookHost = FieldCompanionNotificationRules.TryGetWebhookHost(webhookUrl);

            var payload = BuildWebhookPayload(item);



            try

            {

                var client = httpClientFactory.CreateClient(WebhookHttpClientName);

                using var response = await client.PostAsJsonAsync(webhookUrl, payload, cancellationToken);

                webhookStatusCode = (int)response.StatusCode;

                if (response.IsSuccessStatusCode)

                {

                    webhookSent = true;

                }

                else

                {

                    var body = await response.Content.ReadAsStringAsync(cancellationToken);

                    var truncated = body.Length > 200 ? body[..200] : body;

                    webhookError = $"http_{webhookStatusCode}:{truncated}";

                }

            }

            catch (Exception ex) when (ex is not OperationCanceledException)

            {

                webhookError = ex.Message;

            }

        }



        var pushDeliveredCount = 0;

        string? pushError = null;

        var pushAttempted = false;



        if (webPushOptions.Value.IsConfigured)

        {

            var targetUserId = FieldCompanionNotificationEnqueueService.ResolveTargetUserId(

                item.EventKind,

                item.ActorUserId,

                item.RelatedEntityId);



            if (targetUserId is Guid userId)

            {

                var subscriptions = await pushSubscriptionService.ListForUserAsync(

                    item.TenantId,

                    userId,

                    cancellationToken);



                if (subscriptions.Count > 0)

                {

                    pushAttempted = true;

                    var payloadJson = FieldCompanionWebPushPayloadBuilder.Build(item);

                    var staleEndpoints = new List<string>();



                    foreach (var subscription in subscriptions)

                    {

                        var sendResult = await webPushSender.SendAsync(subscription, payloadJson, cancellationToken);

                        if (sendResult.Success)

                        {

                            pushDeliveredCount++;

                            continue;

                        }



                        pushError = sendResult.ErrorMessage;

                        if (sendResult.HttpStatusCode is 404 or 410)

                        {

                            staleEndpoints.Add(subscription.Endpoint);

                        }

                    }



                    if (staleEndpoints.Count > 0)

                    {

                        var stale = await db.FieldCompanionPushSubscriptions

                            .Where(x => x.TenantId == item.TenantId

                                && x.UserId == userId

                                && staleEndpoints.Contains(x.Endpoint))

                            .ToListAsync(cancellationToken);

                        db.FieldCompanionPushSubscriptions.RemoveRange(stale);

                        await db.SaveChangesAsync(cancellationToken);

                    }

                }

            }

        }



        if (!webhookAttempted && !pushAttempted)

        {

            return await MarkAsync(

                item,

                FieldCompanionNotificationDispatchStatuses.Skipped,

                webhookHost,

                webhookStatusCode,

                pushDeliveredCount,

                "webhook_and_push_unavailable",

                now,

                cancellationToken);

        }



        if (webhookSent || pushDeliveredCount > 0)

        {

            return await MarkAsync(

                item,

                FieldCompanionNotificationDispatchStatuses.Sent,

                webhookHost,

                webhookStatusCode,

                pushDeliveredCount,

                null,

                now,

                cancellationToken);

        }



        var combinedError = string.Join(

            "; ",

            new[] { webhookError, pushError }.Where(static message => !string.IsNullOrWhiteSpace(message)));



        return await MarkAsync(

            item,

            FieldCompanionNotificationDispatchStatuses.Failed,

            webhookHost,

            webhookStatusCode,

            pushDeliveredCount,

            string.IsNullOrWhiteSpace(combinedError) ? "dispatch_failed" : combinedError,

            now,

            cancellationToken);

    }



    private static object BuildWebhookPayload(FieldCompanionNotificationDispatch item) =>

        item.EventKind switch

        {

            FieldCompanionNotificationEventKinds.HandoffRedeemed => new

            {

                @event = "fieldcompanion.handoff.redeemed",

                tenantId = item.TenantId,

                actorUserId = item.ActorUserId,

                handoffCodeId = item.RelatedEntityId,

            },

            FieldCompanionNotificationEventKinds.FieldInboxRefreshed => new

            {

                @event = "fieldcompanion.field_inbox.refreshed",

                tenantId = item.TenantId,

                actorUserId = item.ActorUserId,

                userId = item.RelatedEntityId,

            },

            FieldCompanionNotificationEventKinds.TestNotification => new

            {

                @event = "fieldcompanion.notification.test",

                tenantId = item.TenantId,

                actorUserId = item.ActorUserId,

                notificationId = item.Id,

                relatedEntityType = item.RelatedEntityType,

                relatedEntityId = item.RelatedEntityId,

            },

            _ => new

            {

                @event = "fieldcompanion.notification.unknown",

                tenantId = item.TenantId,

                eventKind = item.EventKind,

                relatedEntityId = item.RelatedEntityId,

            },

        };

    private static PendingFieldCompanionNotificationItem ToPendingItem(FieldCompanionNotificationDispatch x) =>
        new(
            x.Id,
            x.TenantId,
            x.EventKind,
            x.ActorUserId,
            x.CreatedAt);

    private static FieldCompanionNotificationDispatchItem ToItem(FieldCompanionNotificationDispatch x) =>
        new(
            x.Id,
            x.EventKind,
            x.DispatchStatus,
            x.ActorUserId,
            x.RelatedEntityType,
            x.RelatedEntityId,
            x.WebhookHost,
            x.HttpStatusCode,
            x.ErrorMessage,
            x.PushDeliveredCount,
            x.CreatedAt,
            x.DispatchedAt);



    private async Task<FieldCompanionNotificationDispatchResult> MarkAsync(

        FieldCompanionNotificationDispatch item,

        string status,

        string? webhookHost,

        int? httpStatusCode,

        int? pushDeliveredCount,

        string? errorMessage,

        DateTimeOffset dispatchedAt,

        CancellationToken cancellationToken)

    {

        item.DispatchStatus = status;

        item.WebhookHost = webhookHost;

        item.HttpStatusCode = httpStatusCode;

        item.PushDeliveredCount = pushDeliveredCount;

        item.ErrorMessage = errorMessage is null ? null : Truncate(errorMessage, 512);

        item.DispatchedAt = dispatchedAt;

        await db.SaveChangesAsync(cancellationToken);



        await audit.WriteAsync(

            "fieldcompanion.notification_dispatch",

            item.RelatedEntityType,

            item.RelatedEntityId.ToString(),

            status,

            tenantId: item.TenantId,

            actorUserId: WorkerActorUserId,

            reasonCode: item.EventKind,

            cancellationToken: cancellationToken);



        return new FieldCompanionNotificationDispatchResult(item.Id, status);

    }



    private static string Truncate(string value, int maxLength) =>

        value.Length <= maxLength ? value : value[..maxLength];

}


