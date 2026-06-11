using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class IntegrationEventProcessingService(
    SupplyArrDbContext db,
    IntegrationEventSettingsService settingsService,
    MaintainArrDemandIntakeService maintainarrDemandIntake,
    RoutArrDemandIntakeService routarrDemandIntake,
    TrainArrDemandIntakeService trainarrDemandIntake,
    StaffArrDemandIntakeService staffarrDemandIntake,
    ProcurementNotificationEnqueueService notificationEnqueue,
    ComplianceCoreFactPublisherService complianceCoreFactPublisher,
    StaffArrProductIncidentPublisherService staffarrIncidentPublisher,
    TrainArrSupplierIncidentPublisherService trainarrIncidentPublisher,
    RoutArrVendorOrderClient routarrVendorOrderClient,
    ISupplyArrAuditService audit)
{
    public const string ProcessEventsActionScope = "supplyarr.integration.events.process";

    public const string EnqueueInboxActionScope = "supplyarr.integration.inbox.enqueue";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000ff");

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task TryProcessSingleOutboxAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(outboxEvent.ProcessingStatus, IntegrationEventStatuses.Pending, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var settings = await settingsService.LoadSnapshotAsync(outboxEvent.TenantId, cancellationToken);
        if (!IntegrationEventRules.ShouldProcessForTenant(settings))
        {
            return;
        }

        await ProcessOutboxEventAsync(outboxEvent, settings, cancellationToken);
    }

    public async Task TryProcessSingleInboxAsync(
        IntegrationInboxEvent inboxEvent,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(inboxEvent.ProcessingStatus, IntegrationEventStatuses.Pending, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var settings = await settingsService.LoadSnapshotAsync(inboxEvent.TenantId, cancellationToken);
        if (!IntegrationEventRules.ShouldProcessForTenant(settings))
        {
            return;
        }

        await ProcessInboxEventAsync(inboxEvent, settings, cancellationToken);
    }

    public async Task<PendingIntegrationEventsResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = IntegrationEventRules.NormalizeBatchSize(batchSize);
        var outbox = await LoadPendingOutboxAsync(tenantId, asOf, normalizedBatchSize, cancellationToken);
        var inbox = await LoadPendingInboxAsync(tenantId, asOf, normalizedBatchSize, cancellationToken);

        return new PendingIntegrationEventsResponse(
            outbox.Count,
            inbox.Count,
            outbox.Select(MapOutboxItem).ToList(),
            inbox.Select(MapInboxItem).ToList());
    }

    public async Task<ProcessIntegrationEventsResponse> ProcessBatchAsync(
        ProcessIntegrationEventsRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = DateTimeOffset.UtcNow;
        var batchSize = IntegrationEventRules.NormalizeBatchSize(request.BatchSize);
        var outboxPending = await LoadPendingOutboxAsync(request.TenantId, asOf, batchSize, cancellationToken);
        var inboxPending = await LoadPendingInboxAsync(request.TenantId, asOf, batchSize, cancellationToken);

        var outboxProcessed = 0;
        var inboxProcessed = 0;
        var skipped = 0;
        var abandoned = 0;

        foreach (var outboxEvent in outboxPending)
        {
            try
            {
                var settings = await settingsService.LoadSnapshotAsync(outboxEvent.TenantId, cancellationToken);
                if (!IntegrationEventRules.ShouldProcessForTenant(settings))
                {
                    skipped++;
                    continue;
                }

                var before = outboxEvent.ProcessingStatus;
                await ProcessOutboxEventAsync(outboxEvent, settings, cancellationToken);
                if (string.Equals(outboxEvent.ProcessingStatus, IntegrationEventStatuses.Processed, StringComparison.OrdinalIgnoreCase))
                {
                    outboxProcessed++;
                }
                else if (string.Equals(outboxEvent.ProcessingStatus, IntegrationEventStatuses.Abandoned, StringComparison.OrdinalIgnoreCase))
                {
                    abandoned++;
                }
                else if (before == outboxEvent.ProcessingStatus)
                {
                    skipped++;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped++;
            }
        }

        foreach (var inboxEvent in inboxPending)
        {
            try
            {
                var settings = await settingsService.LoadSnapshotAsync(inboxEvent.TenantId, cancellationToken);
                if (!IntegrationEventRules.ShouldProcessForTenant(settings))
                {
                    skipped++;
                    continue;
                }

                var before = inboxEvent.ProcessingStatus;
                await ProcessInboxEventAsync(inboxEvent, settings, cancellationToken);
                if (string.Equals(inboxEvent.ProcessingStatus, IntegrationEventStatuses.Processed, StringComparison.OrdinalIgnoreCase))
                {
                    inboxProcessed++;
                }
                else if (string.Equals(inboxEvent.ProcessingStatus, IntegrationEventStatuses.Abandoned, StringComparison.OrdinalIgnoreCase))
                {
                    abandoned++;
                }
                else if (before == inboxEvent.ProcessingStatus)
                {
                    skipped++;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped++;
            }
        }

        Guid? runId = null;
        if (request.TenantId is Guid tenantId && (outboxProcessed > 0 || inboxProcessed > 0 || abandoned > 0))
        {
            var run = new IntegrationEventProcessingRun
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                OutboxProcessedCount = outboxProcessed,
                InboxProcessedCount = inboxProcessed,
                SkippedCount = skipped,
                AbandonedCount = abandoned,
                CreatedAt = asOf,
            };
            db.IntegrationEventProcessingRuns.Add(run);
            await db.SaveChangesAsync(cancellationToken);
            runId = run.Id;

            await audit.WriteAsync(
                "supplyarr.integration_events.batch",
                tenantId,
                WorkerActorUserId,
                "integration_event_processing_run",
                run.Id.ToString(),
                "Succeeded",
                cancellationToken: cancellationToken);
        }

        return new ProcessIntegrationEventsResponse(
            request.TenantId,
            outboxProcessed,
            inboxProcessed,
            skipped,
            abandoned,
            runId);
    }

    public async Task<IntegrationEventsListResponse> ListRecentOutboxAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = IntegrationEventRules.NormalizeEventListLimit(limit);
        var rows = await db.IntegrationOutboxEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new IntegrationEventsListResponse(rows.Select(MapOutboxItem).ToList());
    }

    public async Task<IntegrationEventsListResponse> ListRecentInboxAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = IntegrationEventRules.NormalizeEventListLimit(limit);
        var rows = await db.IntegrationInboxEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new IntegrationEventsListResponse(rows.Select(MapInboxItem).ToList());
    }

    public async Task<AbandonIntegrationEventResponse> AbandonOutboxAsync(
        Guid tenantId,
        Guid eventId,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.IntegrationOutboxEvents.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == eventId,
            cancellationToken)
            ?? throw new StlApiException("integration_events.not_found", "Outbox event was not found.", 404);

        entity.ProcessingStatus = IntegrationEventStatuses.Abandoned;
        entity.NextRetryAt = null;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplyarr.integration_outbox.abandon",
            tenantId,
            actorUserId,
            "integration_outbox_event",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return new AbandonIntegrationEventResponse(entity.Id, "outbox", entity.ProcessingStatus);
    }

    public async Task<AbandonIntegrationEventResponse> AbandonInboxAsync(
        Guid tenantId,
        Guid eventId,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.IntegrationInboxEvents.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == eventId,
            cancellationToken)
            ?? throw new StlApiException("integration_events.not_found", "Inbox event was not found.", 404);

        entity.ProcessingStatus = IntegrationEventStatuses.Abandoned;
        entity.NextRetryAt = null;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplyarr.integration_inbox.abandon",
            tenantId,
            actorUserId,
            "integration_inbox_event",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return new AbandonIntegrationEventResponse(entity.Id, "inbox", entity.ProcessingStatus);
    }

    private async Task ProcessOutboxEventAsync(
        IntegrationOutboxEvent outboxEvent,
        TenantIntegrationEventSettingsSnapshot? settings,
        CancellationToken cancellationToken)
    {
        var maxAttempts = IntegrationEventRules.NormalizeMaxAttempts(settings?.MaxAttempts);
        var retryIntervalMinutes = IntegrationEventRules.NormalizeRetryIntervalMinutes(settings?.RetryIntervalMinutes);
        var now = DateTimeOffset.UtcNow;

        outboxEvent.AttemptCount += 1;
        outboxEvent.UpdatedAt = now;

        try
        {
            var payload = JsonSerializer.Deserialize<IntegrationOutboxPayload>(outboxEvent.PayloadJson, JsonOptions)
                ?? throw new InvalidOperationException("Integration outbox payload is invalid.");

            var notificationKind = MapOutboxToNotificationKind(outboxEvent.EventKind);
            if (notificationKind is not null)
            {
                await notificationEnqueue.TryEnqueueAsync(
                    outboxEvent.TenantId,
                    notificationKind,
                    payload.VendorPartyId,
                    outboxEvent.RelatedEntityType,
                    outboxEvent.RelatedEntityId,
                    cancellationToken);
            }

            await complianceCoreFactPublisher.TryPublishFromOutboxAsync(outboxEvent, cancellationToken);
            await staffarrIncidentPublisher.TryPublishFromOutboxAsync(outboxEvent, cancellationToken);
            await trainarrIncidentPublisher.TryPublishFromOutboxAsync(outboxEvent, cancellationToken);
            await TryPublishVendorOrderEventAsync(outboxEvent, cancellationToken);

            outboxEvent.ProcessingStatus = IntegrationEventStatuses.Processed;
            outboxEvent.ProcessedAt = now;
            outboxEvent.NextRetryAt = null;
            outboxEvent.ErrorMessage = null;

            await db.SaveChangesAsync(cancellationToken);

            await audit.WriteAsync(
                "supplyarr.integration_outbox.processed",
                outboxEvent.TenantId,
                WorkerActorUserId,
                "integration_outbox_event",
                outboxEvent.Id.ToString(),
                "Succeeded",
                reasonCode: outboxEvent.EventKind,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            outboxEvent.ErrorMessage = Truncate(ex.Message, 512);
            ApplyFailure(outboxEvent, maxAttempts, retryIntervalMinutes, now);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task ProcessInboxEventAsync(
        IntegrationInboxEvent inboxEvent,
        TenantIntegrationEventSettingsSnapshot? settings,
        CancellationToken cancellationToken)
    {
        var maxAttempts = IntegrationEventRules.NormalizeMaxAttempts(settings?.MaxAttempts);
        var retryIntervalMinutes = IntegrationEventRules.NormalizeRetryIntervalMinutes(settings?.RetryIntervalMinutes);
        var now = DateTimeOffset.UtcNow;

        inboxEvent.AttemptCount += 1;
        inboxEvent.UpdatedAt = now;

        try
        {
            if (string.Equals(inboxEvent.EventKind, IntegrationInboxEventKinds.MaintainarrDemandIngest, StringComparison.OrdinalIgnoreCase))
            {
                var request = JsonSerializer.Deserialize<IngestMaintainarrDemandRequest>(inboxEvent.PayloadJson, JsonOptions)
                    ?? throw new InvalidOperationException("MaintainArr demand ingest payload is invalid.");

                await maintainarrDemandIntake.IngestAsync(request, cancellationToken);
            }
            else if (string.Equals(inboxEvent.EventKind, IntegrationInboxEventKinds.RoutarrDemandIngest, StringComparison.OrdinalIgnoreCase))
            {
                var request = JsonSerializer.Deserialize<IngestRoutarrDemandRequest>(inboxEvent.PayloadJson, JsonOptions)
                    ?? throw new InvalidOperationException("RoutArr demand ingest payload is invalid.");

                await routarrDemandIntake.IngestAsync(request, cancellationToken);
            }
            else if (string.Equals(inboxEvent.EventKind, IntegrationInboxEventKinds.TrainarrDemandIngest, StringComparison.OrdinalIgnoreCase))
            {
                var request = JsonSerializer.Deserialize<IngestTrainarrDemandRequest>(inboxEvent.PayloadJson, JsonOptions)
                    ?? throw new InvalidOperationException("TrainArr demand ingest payload is invalid.");

                await trainarrDemandIntake.IngestAsync(request, cancellationToken);
            }
            else if (string.Equals(inboxEvent.EventKind, IntegrationInboxEventKinds.StaffarrDemandIngest, StringComparison.OrdinalIgnoreCase))
            {
                var request = JsonSerializer.Deserialize<IngestStaffarrDemandRequest>(inboxEvent.PayloadJson, JsonOptions)
                    ?? throw new InvalidOperationException("StaffArr demand ingest payload is invalid.");

                await staffarrDemandIntake.IngestAsync(request, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException($"Unsupported inbox event kind '{inboxEvent.EventKind}'.");
            }

            inboxEvent.ProcessingStatus = IntegrationEventStatuses.Processed;
            inboxEvent.ProcessedAt = now;
            inboxEvent.NextRetryAt = null;
            inboxEvent.ErrorMessage = null;

            await db.SaveChangesAsync(cancellationToken);

            await audit.WriteAsync(
                "supplyarr.integration_inbox.processed",
                inboxEvent.TenantId,
                WorkerActorUserId,
                "integration_inbox_event",
                inboxEvent.Id.ToString(),
                "Succeeded",
                reasonCode: inboxEvent.EventKind,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            inboxEvent.ErrorMessage = Truncate(ex.Message, 512);
            ApplyFailure(inboxEvent, maxAttempts, retryIntervalMinutes, now);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static string? MapOutboxToNotificationKind(string eventKind) =>
        eventKind switch
        {
            IntegrationOutboxEventKinds.PurchaseRequestSubmitted => ProcurementNotificationEventKinds.PurchaseRequestSubmitted,
            IntegrationOutboxEventKinds.PurchaseRequestApproved => ProcurementNotificationEventKinds.PurchaseRequestApproved,
            IntegrationOutboxEventKinds.PurchaseOrderIssued => ProcurementNotificationEventKinds.PurchaseOrderIssued,
            IntegrationOutboxEventKinds.ReceivingReceiptPosted => ProcurementNotificationEventKinds.ReceivingReceiptPosted,
            _ => null,
        };

    private async Task TryPublishVendorOrderEventAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(outboxEvent.EventKind, IntegrationOutboxEventKinds.VendorOrderStatusChanged, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(outboxEvent.EventKind, IntegrationOutboxEventKinds.VendorOrderCompletedForDispatch, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(outboxEvent.EventKind, IntegrationOutboxEventKinds.VendorOrderPartialDispatchAuthorized, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(outboxEvent.EventKind, IntegrationOutboxEventKinds.VendorOrderSplitCreated, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var payload = await BuildVendorOrderEventAsync(outboxEvent, cancellationToken);
        await routarrVendorOrderClient.PublishVendorOrderEventAsync(payload, cancellationToken);
    }

    private async Task<SupplyArrVendorOrderEventEnvelope> BuildVendorOrderEventAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken)
    {
        if (string.Equals(outboxEvent.EventKind, IntegrationOutboxEventKinds.VendorOrderPartialDispatchAuthorized, StringComparison.OrdinalIgnoreCase))
        {
            var decision = await db.VendorOrderBrokerDecisions
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.TenantId == outboxEvent.TenantId && x.Id == outboxEvent.RelatedEntityId,
                    cancellationToken)
                ?? throw new InvalidOperationException("Vendor-order broker decision was not found for outbox publication.");

            var order = await db.VendorOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.TenantId == outboxEvent.TenantId && x.Id == decision.VendorOrderId,
                    cancellationToken)
                ?? throw new InvalidOperationException("Vendor order was not found for outbox publication.");

            return new SupplyArrVendorOrderEventEnvelope(
                outboxEvent.Id,
                outboxEvent.EventKind,
                outboxEvent.CreatedAt,
                outboxEvent.TenantId,
                order.Id,
                order.BrokerOrderId,
                order.BrokerOrderNumberSnapshot,
                null,
                order.Status,
                order.VendorId,
                order.VendorNameSnapshot,
                order.PickupLocationNameSnapshot,
                order.PickupAddressSnapshot,
                order.DeliveryLocationNameSnapshot,
                order.DeliveryAddressSnapshot,
                order.ItemDescription,
                order.OrderedQuantity,
                order.QuantityReady,
                order.QuantityRemaining,
                order.ExpectedReadyAt,
                order.ConfirmedReadyAt,
                order.PickupWindowStart,
                order.PickupWindowEnd,
                order.PickupInstructions,
                VendorOrderStatusUpdateSources.BrokerUser,
                decision.SelectedTripId,
                decision.AuthorizedQuantity,
                null,
                null);
        }

        if (string.Equals(outboxEvent.EventKind, IntegrationOutboxEventKinds.VendorOrderSplitCreated, StringComparison.OrdinalIgnoreCase))
        {
            var decision = await db.VendorOrderBrokerDecisions
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.TenantId == outboxEvent.TenantId && x.Id == outboxEvent.RelatedEntityId,
                    cancellationToken)
                ?? throw new InvalidOperationException("Vendor-order split decision was not found for outbox publication.");

            var parent = await db.VendorOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.TenantId == outboxEvent.TenantId && x.Id == decision.VendorOrderId,
                    cancellationToken)
                ?? throw new InvalidOperationException("Vendor order split parent was not found for outbox publication.");

            var children = await db.VendorOrders
                .AsNoTracking()
                .Where(x => x.TenantId == outboxEvent.TenantId && x.ParentVendorOrderId == parent.Id)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
            var readyChild = children.FirstOrDefault(x => string.Equals(x.Status, VendorOrderStatuses.CompletedReadyForDispatch, StringComparison.OrdinalIgnoreCase));
            var remainingChild = children.FirstOrDefault(x => x.Id != readyChild?.Id);

            return new SupplyArrVendorOrderEventEnvelope(
                outboxEvent.Id,
                outboxEvent.EventKind,
                outboxEvent.CreatedAt,
                outboxEvent.TenantId,
                parent.Id,
                parent.BrokerOrderId,
                parent.BrokerOrderNumberSnapshot,
                null,
                parent.Status,
                parent.VendorId,
                parent.VendorNameSnapshot,
                parent.PickupLocationNameSnapshot,
                parent.PickupAddressSnapshot,
                parent.DeliveryLocationNameSnapshot,
                parent.DeliveryAddressSnapshot,
                parent.ItemDescription,
                parent.OrderedQuantity,
                parent.QuantityReady,
                parent.QuantityRemaining,
                parent.ExpectedReadyAt,
                parent.ConfirmedReadyAt,
                parent.PickupWindowStart,
                parent.PickupWindowEnd,
                parent.PickupInstructions,
                VendorOrderStatusUpdateSources.BrokerUser,
                decision.SelectedTripId,
                decision.AuthorizedQuantity,
                readyChild?.Id,
                remainingChild?.Id);
        }

        var statusUpdate = await db.VendorOrderStatusUpdates
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == outboxEvent.TenantId && x.Id == outboxEvent.RelatedEntityId,
                cancellationToken)
            ?? throw new InvalidOperationException("Vendor-order status update was not found for outbox publication.");

        var orderEntity = await db.VendorOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == outboxEvent.TenantId && x.Id == statusUpdate.VendorOrderId,
                cancellationToken)
            ?? throw new InvalidOperationException("Vendor order was not found for outbox publication.");

        return new SupplyArrVendorOrderEventEnvelope(
            outboxEvent.Id,
            outboxEvent.EventKind,
            outboxEvent.CreatedAt,
            outboxEvent.TenantId,
            orderEntity.Id,
            orderEntity.BrokerOrderId,
            orderEntity.BrokerOrderNumberSnapshot,
            statusUpdate.PreviousStatus,
            statusUpdate.NewStatus,
            orderEntity.VendorId,
            orderEntity.VendorNameSnapshot,
            orderEntity.PickupLocationNameSnapshot,
            orderEntity.PickupAddressSnapshot,
            orderEntity.DeliveryLocationNameSnapshot,
            orderEntity.DeliveryAddressSnapshot,
            orderEntity.ItemDescription,
            statusUpdate.OrderedQuantitySnapshot,
            statusUpdate.QuantityReady,
            statusUpdate.QuantityRemaining,
            statusUpdate.EstimatedReadyAt,
            statusUpdate.ConfirmedReadyAt,
            statusUpdate.PickupWindowStart,
            statusUpdate.PickupWindowEnd,
            orderEntity.PickupInstructions,
            statusUpdate.Source,
            null,
            null,
            null,
            null);
    }

    private static void ApplyFailure(
        IntegrationOutboxEvent domainEvent,
        int maxAttempts,
        int retryIntervalMinutes,
        DateTimeOffset now)
    {
        if (domainEvent.AttemptCount >= maxAttempts)
        {
            domainEvent.ProcessingStatus = IntegrationEventStatuses.Abandoned;
            domainEvent.NextRetryAt = null;
        }
        else
        {
            domainEvent.ProcessingStatus = IntegrationEventStatuses.Pending;
            domainEvent.NextRetryAt = IntegrationEventRules.ComputeNextRetryAt(now, retryIntervalMinutes);
        }
    }

    private static void ApplyFailure(
        IntegrationInboxEvent domainEvent,
        int maxAttempts,
        int retryIntervalMinutes,
        DateTimeOffset now)
    {
        if (domainEvent.AttemptCount >= maxAttempts)
        {
            domainEvent.ProcessingStatus = IntegrationEventStatuses.Abandoned;
            domainEvent.NextRetryAt = null;
        }
        else
        {
            domainEvent.ProcessingStatus = IntegrationEventStatuses.Pending;
            domainEvent.NextRetryAt = IntegrationEventRules.ComputeNextRetryAt(now, retryIntervalMinutes);
        }
    }

    private async Task<List<IntegrationOutboxEvent>> LoadPendingOutboxAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var query = db.IntegrationOutboxEvents
            .Where(x => x.ProcessingStatus == IntegrationEventStatuses.Pending
                && (x.NextRetryAt == null || x.NextRetryAt <= asOfUtc));

        if (tenantId is Guid scopedTenantId)
        {
            query = query.Where(x => x.TenantId == scopedTenantId);
        }

        return await query
            .OrderBy(x => x.NextRetryAt ?? x.CreatedAt)
            .ThenBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<IntegrationInboxEvent>> LoadPendingInboxAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var query = db.IntegrationInboxEvents
            .Where(x => x.ProcessingStatus == IntegrationEventStatuses.Pending
                && (x.NextRetryAt == null || x.NextRetryAt <= asOfUtc));

        if (tenantId is Guid scopedTenantId)
        {
            query = query.Where(x => x.TenantId == scopedTenantId);
        }

        return await query
            .OrderBy(x => x.NextRetryAt ?? x.CreatedAt)
            .ThenBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    private static IntegrationEventItemResponse MapOutboxItem(IntegrationOutboxEvent x) =>
        new(
            x.Id,
            "outbox",
            x.EventKind,
            x.IdempotencyKey,
            null,
            x.RelatedEntityType,
            x.RelatedEntityId.ToString(),
            x.ProcessingStatus,
            x.AttemptCount,
            x.ErrorMessage,
            x.CreatedAt,
            x.ProcessedAt);

    private static IntegrationEventItemResponse MapInboxItem(IntegrationInboxEvent x) =>
        new(
            x.Id,
            "inbox",
            x.EventKind,
            x.IdempotencyKey,
            x.SourceProduct,
            x.RelatedEntityType,
            x.RelatedEntityId,
            x.ProcessingStatus,
            x.AttemptCount,
            x.ErrorMessage,
            x.CreatedAt,
            x.ProcessedAt);

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
