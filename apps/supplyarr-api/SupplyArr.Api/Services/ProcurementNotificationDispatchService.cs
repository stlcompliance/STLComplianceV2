using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class ProcurementNotificationDispatchService(
    SupplyArrDbContext db,
    ProcurementNotificationSettingsService settingsService,
    IHttpClientFactory httpClientFactory,
    ISupplyArrAuditService audit)
{
    public const string ProcessNotificationsActionScope = "supplyarr.notifications.dispatch";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f8");

    public const string WebhookHttpClientName = "SupplyArrProcurementNotificationWebhook";

    public async Task<PendingProcurementNotificationsResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = ProcurementNotificationRules.NormalizeBatchSize(batchSize);
        var items = await LoadPendingAsync(tenantId, normalizedBatchSize, cancellationToken);

        return new PendingProcurementNotificationsResponse(
            asOf,
            normalizedBatchSize,
            items.Select(x => new PendingProcurementNotificationItem(
                x.Id,
                x.TenantId,
                x.EventKind,
                x.VendorPartyId,
                x.CreatedAt)).ToList());
    }

    public async Task<ProcessProcurementNotificationsResponse> ProcessBatchAsync(
        ProcessProcurementNotificationsRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = ProcurementNotificationRules.NormalizeBatchSize(request.BatchSize);
        var pending = await LoadPendingAsync(request.TenantId, batchSize, cancellationToken);
        var dispatches = new List<ProcurementNotificationDispatchResult>();
        var skipped = new List<ProcurementNotificationDispatchSkip>();

        foreach (var item in pending)
        {
            try
            {
                var result = await DispatchOneAsync(item, cancellationToken);
                dispatches.Add(result);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new ProcurementNotificationDispatchSkip(item.Id, ex.Message));
            }
        }

        if (dispatches.Count > 0 && request.TenantId is Guid tenantId)
        {
            await audit.WriteAsync(
                "supplyarr.notification_dispatch.batch",
                tenantId,
                WorkerActorUserId,
                "procurement_notification_dispatch",
                $"{dispatches.Count}",
                "success",
                cancellationToken: cancellationToken);
        }

        return new ProcessProcurementNotificationsResponse(
            asOf,
            batchSize,
            pending.Count,
            dispatches.Count,
            skipped.Count,
            dispatches,
            skipped);
    }

    public async Task<ProcurementNotificationDispatchesResponse> ListRecentAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = ProcurementNotificationRules.NormalizeDispatchListLimit(limit);
        var rows = await db.ProcurementNotificationDispatches
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(x => new ProcurementNotificationDispatchItem(
                x.Id,
                x.EventKind,
                x.DispatchStatus,
                x.VendorPartyId,
                x.RelatedEntityType,
                x.RelatedEntityId,
                x.WebhookHost,
                x.HttpStatusCode,
                x.ErrorMessage,
                x.CreatedAt,
                x.DispatchedAt))
            .ToList();

        return new ProcurementNotificationDispatchesResponse(items);
    }

    private async Task<List<ProcurementNotificationDispatch>> LoadPendingAsync(
        Guid? tenantId,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var query = db.ProcurementNotificationDispatches
            .Where(x => x.DispatchStatus == ProcurementNotificationDispatchStatuses.Pending);

        if (tenantId is Guid scopedTenantId)
        {
            query = query.Where(x => x.TenantId == scopedTenantId);
        }

        return await query
            .OrderBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    private async Task<ProcurementNotificationDispatchResult> DispatchOneAsync(
        ProcurementNotificationDispatch item,
        CancellationToken cancellationToken)
    {
        var settings = await settingsService.LoadSnapshotAsync(item.TenantId, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        if (settings is null || !ProcurementNotificationRules.ShouldNotifyForEvent(settings, item.EventKind))
        {
            return await MarkAsync(
                item,
                ProcurementNotificationDispatchStatuses.Skipped,
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
                    ProcurementNotificationDispatchStatuses.Sent,
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
                ProcurementNotificationDispatchStatuses.Failed,
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
                ProcurementNotificationDispatchStatuses.Failed,
                webhookUrl,
                null,
                ex.Message,
                now,
                cancellationToken);
        }
    }

    private static object BuildPayload(ProcurementNotificationDispatch item) =>
        item.EventKind switch
        {
            ProcurementNotificationEventKinds.PurchaseRequestSubmitted => new
            {
                @event = "supplyarr.purchase_request.submitted",
                tenantId = item.TenantId,
                vendorPartyId = item.VendorPartyId,
                purchaseRequestId = item.RelatedEntityId,
            },
            ProcurementNotificationEventKinds.PurchaseRequestApproved => new
            {
                @event = "supplyarr.purchase_request.approved",
                tenantId = item.TenantId,
                vendorPartyId = item.VendorPartyId,
                purchaseRequestId = item.RelatedEntityId,
            },
            ProcurementNotificationEventKinds.PurchaseOrderIssued => new
            {
                @event = "supplyarr.purchase_order.issued",
                tenantId = item.TenantId,
                vendorPartyId = item.VendorPartyId,
                purchaseOrderId = item.RelatedEntityId,
            },
            ProcurementNotificationEventKinds.ReceivingReceiptPosted => new
            {
                @event = "supplyarr.receiving_receipt.posted",
                tenantId = item.TenantId,
                vendorPartyId = item.VendorPartyId,
                receivingReceiptId = item.RelatedEntityId,
            },
            ProcurementNotificationEventKinds.PurchaseRequestApprovalReminder => new
            {
                @event = "supplyarr.purchase_request.approval_reminder",
                tenantId = item.TenantId,
                vendorPartyId = item.VendorPartyId,
                purchaseRequestId = item.RelatedEntityId,
            },
            ProcurementNotificationEventKinds.PurchaseOrderApprovalReminder => new
            {
                @event = "supplyarr.purchase_order.approval_reminder",
                tenantId = item.TenantId,
                vendorPartyId = item.VendorPartyId,
                purchaseOrderId = item.RelatedEntityId,
            },
            ProcurementNotificationEventKinds.MaintainArrDemandPrDrafted => new
            {
                @event = "supplyarr.maintainarr_demand.pr_drafted",
                tenantId = item.TenantId,
                demandRefId = item.RelatedEntityId,
            },
            ProcurementNotificationEventKinds.ProcurementExceptionSlaEscalation => new
            {
                @event = "supplyarr.procurement_exception.sla_escalation",
                tenantId = item.TenantId,
                procurementExceptionId = item.RelatedEntityId,
            },
            _ => new
            {
                @event = "supplyarr.notification.unknown",
                tenantId = item.TenantId,
                eventKind = item.EventKind,
                relatedEntityId = item.RelatedEntityId,
            },
        };

    private async Task<ProcurementNotificationDispatchResult> MarkAsync(
        ProcurementNotificationDispatch item,
        string status,
        string? webhookUrl,
        int? httpStatusCode,
        string? errorMessage,
        DateTimeOffset dispatchedAt,
        CancellationToken cancellationToken)
    {
        item.DispatchStatus = status;
        item.WebhookHost = ProcurementNotificationRules.TryGetWebhookHost(webhookUrl);
        item.HttpStatusCode = httpStatusCode;
        item.ErrorMessage = errorMessage is null ? null : Truncate(errorMessage, 512);
        item.DispatchedAt = dispatchedAt;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplyarr.notification_dispatch",
            item.TenantId,
            WorkerActorUserId,
            item.RelatedEntityType,
            item.RelatedEntityId.ToString(),
            status,
            reasonCode: item.EventKind,
            cancellationToken: cancellationToken);

        return new ProcurementNotificationDispatchResult(item.Id, status);
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
