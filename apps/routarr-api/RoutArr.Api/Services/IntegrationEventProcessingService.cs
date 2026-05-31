using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public sealed class IntegrationEventProcessingService(
    RoutArrDbContext db,
    IntegrationEventSettingsService settingsService,
    StaffArrProductIncidentPublisherService staffarrIncidentPublisher,
    TrainArrIncidentRemediationPublisherService trainarrIncidentPublisher,
    MaintainArrRoutarrEventPublisherService maintainarrEventPublisher,
    ComplianceCoreIncidentFactPublisherService complianceCoreIncidentFactPublisher,
    IRoutArrAuditService audit)
{
    public const string ProcessEventsActionScope = "routarr.integration.events.process";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f7");

    public async Task<PendingIntegrationOutboxEventsResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = IntegrationEventRules.NormalizeBatchSize(batchSize);
        var items = await LoadPendingAsync(tenantId, asOf, normalizedBatchSize, cancellationToken);

        return new PendingIntegrationOutboxEventsResponse(
            asOf,
            normalizedBatchSize,
            items.Select(x => new PendingIntegrationOutboxEventItem(
                x.Id,
                x.TenantId,
                x.EventKind,
                x.RelatedEntityType,
                x.RelatedEntityId,
                x.PayloadJson,
                x.CorrelationId,
                x.CreatedAt)).ToList());
    }

    public async Task<ProcessIntegrationOutboxEventsResponse> ProcessBatchAsync(
        ProcessIntegrationOutboxEventsRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = IntegrationEventRules.NormalizeBatchSize(request.BatchSize);
        var pending = await LoadPendingAsync(request.TenantId, asOf, batchSize, cancellationToken);

        var processed = 0;
        var skipped = 0;
        var abandoned = 0;
        var results = new List<IntegrationOutboxEventProcessResult>();

        foreach (var item in pending)
        {
            try
            {
                var settings = await settingsService.LoadSnapshotAsync(item.TenantId, cancellationToken);
                if (!IntegrationEventRules.ShouldProcessForTenant(settings))
                {
                    skipped++;
                    continue;
                }

                var before = item.ProcessingStatus;
                await ProcessOneAsync(item, settings, cancellationToken);
                results.Add(new IntegrationOutboxEventProcessResult(item.Id, item.ProcessingStatus));

                if (string.Equals(item.ProcessingStatus, IntegrationEventStatuses.Processed, StringComparison.OrdinalIgnoreCase))
                {
                    processed++;
                }
                else if (string.Equals(item.ProcessingStatus, IntegrationEventStatuses.Abandoned, StringComparison.OrdinalIgnoreCase))
                {
                    abandoned++;
                }
                else if (before == item.ProcessingStatus)
                {
                    skipped++;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped++;
                results.Add(new IntegrationOutboxEventProcessResult(item.Id, item.ProcessingStatus));
            }
        }

        if (processed > 0)
        {
            var auditTenantId = request.TenantId ?? pending.FirstOrDefault()?.TenantId ?? Guid.Empty;
            await audit.WriteAsync(
                "routarr.integration_event.batch",
                auditTenantId,
                WorkerActorUserId,
                "integration_outbox_event",
                $"{processed}",
                "processed",
                cancellationToken: cancellationToken);
        }

        return new ProcessIntegrationOutboxEventsResponse(
            asOf,
            batchSize,
            pending.Count,
            processed,
            skipped,
            abandoned,
            results);
    }

    public async Task<IntegrationOutboxEventListResponse> ListRecentAsync(
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

        return new IntegrationOutboxEventListResponse(
            rows.Select(x => new IntegrationOutboxEventListItem(
                x.Id,
                x.EventKind,
                x.ProcessingStatus,
                x.RelatedEntityType,
                x.RelatedEntityId,
                x.AttemptCount,
                x.ErrorMessage,
                x.CreatedAt,
                x.ProcessedAt)).ToList());
    }

    private async Task<List<IntegrationOutboxEvent>> LoadPendingAsync(
        Guid? tenantId,
        DateTimeOffset asOf,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var query = db.IntegrationOutboxEvents
            .Where(x => x.ProcessingStatus == IntegrationEventStatuses.Pending
                && (x.NextRetryAt == null || x.NextRetryAt <= asOf));

        if (tenantId is Guid scopedTenantId)
        {
            query = query.Where(x => x.TenantId == scopedTenantId);
        }

        return await query
            .OrderBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    private async Task ProcessOneAsync(
        IntegrationOutboxEvent item,
        TenantIntegrationEventSettingsSnapshot settings,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        item.AttemptCount += 1;
        item.UpdatedAt = now;

        if (item.AttemptCount > settings.MaxAttempts)
        {
            item.ProcessingStatus = IntegrationEventStatuses.Abandoned;
            item.ErrorMessage = "max_attempts_exceeded";
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        await db.SaveChangesAsync(cancellationToken);

        try
        {
            await staffarrIncidentPublisher.TryPublishFromOutboxAsync(item, cancellationToken);
            await trainarrIncidentPublisher.TryPublishFromOutboxAsync(item, cancellationToken);
            await maintainarrEventPublisher.TryPublishFromOutboxAsync(item, cancellationToken);
            await complianceCoreIncidentFactPublisher.TryPublishFromOutboxAsync(item, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            item.ProcessingStatus = IntegrationEventStatuses.Pending;
            item.ErrorMessage = ex.Message;
            item.NextRetryAt = now.AddMinutes(settings.RetryIntervalMinutes);
            item.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            throw;
        }

        item.ProcessingStatus = IntegrationEventStatuses.Processed;
        item.ProcessedAt = now;
        item.ErrorMessage = null;
        item.NextRetryAt = null;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "routarr.integration_event.process",
            item.TenantId,
            WorkerActorUserId,
            item.RelatedEntityType,
            item.RelatedEntityId.ToString(),
            item.ProcessingStatus,
            reasonCode: item.EventKind,
            cancellationToken: cancellationToken);
    }
}
