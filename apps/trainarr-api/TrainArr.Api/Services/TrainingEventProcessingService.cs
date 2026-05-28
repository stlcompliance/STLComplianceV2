using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class TrainingEventProcessingService(
    TrainArrDbContext db,
    EventProcessingSettingsService settingsService,
    ITrainArrAuditService audit)
{
    public const string ProcessEventsActionScope = "trainarr.events.process";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f6");

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task TryProcessSingleAsync(
        TrainingDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(domainEvent.ProcessingStatus, TrainingDomainEventStatuses.Pending, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var settings = await settingsService.LoadSnapshotAsync(domainEvent.TenantId, cancellationToken);
        if (!EventProcessingRules.ShouldProcessForTenant(settings))
        {
            return;
        }

        await ProcessEventAsync(domainEvent, settings, cancellationToken);
    }

    public async Task<PendingTrainingDomainEventsResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = EventProcessingRules.NormalizeBatchSize(batchSize);
        var items = await LoadPendingAsync(tenantId, asOf, normalizedBatchSize, cancellationToken);

        return new PendingTrainingDomainEventsResponse(
            asOf,
            normalizedBatchSize,
            items.Select(x => new PendingTrainingDomainEventItem(
                x.Id,
                x.TenantId,
                x.EventKind,
                x.StaffarrPersonId,
                x.RelatedEntityType,
                x.RelatedEntityId,
                x.AttemptCount,
                x.NextRetryAt,
                x.CreatedAt)).ToList());
    }

    public async Task<ProcessTrainingDomainEventsResponse> ProcessBatchAsync(
        ProcessTrainingDomainEventsRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = EventProcessingRules.NormalizeBatchSize(request.BatchSize);
        var pending = await LoadPendingAsync(request.TenantId, asOf, batchSize, cancellationToken);

        var results = new List<TrainingDomainEventProcessResult>();
        var skipped = new List<TrainingDomainEventProcessSkip>();
        var processedCount = 0;
        var retriedCount = 0;
        var abandonedCount = 0;

        foreach (var domainEvent in pending)
        {
            try
            {
                var settings = await settingsService.LoadSnapshotAsync(domainEvent.TenantId, cancellationToken);
                if (!EventProcessingRules.ShouldProcessForTenant(settings))
                {
                    skipped.Add(new TrainingDomainEventProcessSkip(domainEvent.Id, "tenant_processing_disabled"));
                    continue;
                }

                var beforeStatus = domainEvent.ProcessingStatus;
                await ProcessEventAsync(domainEvent, settings, cancellationToken);

                if (string.Equals(domainEvent.ProcessingStatus, TrainingDomainEventStatuses.Processed, StringComparison.OrdinalIgnoreCase))
                {
                    processedCount++;
                }
                else if (string.Equals(domainEvent.ProcessingStatus, TrainingDomainEventStatuses.Abandoned, StringComparison.OrdinalIgnoreCase))
                {
                    abandonedCount++;
                }
                else if (domainEvent.AttemptCount > 0 && beforeStatus == domainEvent.ProcessingStatus)
                {
                    retriedCount++;
                }

                results.Add(new TrainingDomainEventProcessResult(
                    domainEvent.Id,
                    domainEvent.ProcessingStatus,
                    domainEvent.AttemptCount));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new TrainingDomainEventProcessSkip(domainEvent.Id, ex.Message));
            }
        }

        if (results.Count > 0 && request.TenantId is Guid tenantId)
        {
            await audit.WriteAsync(
                "trainarr.training_events.batch",
                tenantId,
                WorkerActorUserId,
                "training_domain_event",
                $"{results.Count}",
                "success",
                cancellationToken: cancellationToken);
        }

        return new ProcessTrainingDomainEventsResponse(
            asOf,
            batchSize,
            pending.Count,
            processedCount,
            retriedCount,
            abandonedCount,
            skipped.Count,
            results,
            skipped);
    }

    public async Task<TrainingDomainEventsResponse> ListRecentAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = EventProcessingRules.NormalizeEventListLimit(limit);
        var rows = await db.TrainingDomainEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new TrainingDomainEventsResponse(
            rows.Select(x => new TrainingDomainEventItem(
                x.Id,
                x.EventKind,
                x.ProcessingStatus,
                x.StaffarrPersonId,
                x.RelatedEntityType,
                x.RelatedEntityId,
                x.AttemptCount,
                x.ErrorMessage,
                x.CreatedAt,
                x.ProcessedAt)).ToList());
    }

    private async Task<List<TrainingDomainEvent>> LoadPendingAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var query = db.TrainingDomainEvents
            .Where(x => x.ProcessingStatus == TrainingDomainEventStatuses.Pending
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

    private async Task ProcessEventAsync(
        TrainingDomainEvent domainEvent,
        TenantEventProcessingSettingsSnapshot? settings,
        CancellationToken cancellationToken)
    {
        var maxAttempts = EventProcessingRules.NormalizeMaxAttempts(settings?.MaxAttempts);
        var retryIntervalMinutes = EventProcessingRules.NormalizeRetryIntervalMinutes(settings?.RetryIntervalMinutes);
        var now = DateTimeOffset.UtcNow;

        domainEvent.AttemptCount += 1;
        domainEvent.UpdatedAt = now;

        try
        {
            var payload = JsonSerializer.Deserialize<TrainingDomainEventPayload>(domainEvent.PayloadJson, JsonOptions)
                ?? throw new InvalidOperationException("Training domain event payload is invalid.");

            var historyExists = await db.PersonTrainingHistoryEntries.AnyAsync(
                x => x.TenantId == domainEvent.TenantId && x.SourceDomainEventId == domainEvent.Id,
                cancellationToken);

            if (!historyExists)
            {
                db.PersonTrainingHistoryEntries.Add(new PersonTrainingHistoryEntry
                {
                    Id = Guid.NewGuid(),
                    TenantId = domainEvent.TenantId,
                    StaffarrPersonId = domainEvent.StaffarrPersonId,
                    SourceDomainEventId = domainEvent.Id,
                    EventKind = domainEvent.EventKind,
                    Summary = Truncate(payload.Summary, 1024),
                    RelatedEntityType = domainEvent.RelatedEntityType,
                    RelatedEntityId = domainEvent.RelatedEntityId,
                    OccurredAt = payload.OccurredAt,
                    CreatedAt = now,
                });
            }

            domainEvent.ProcessingStatus = TrainingDomainEventStatuses.Processed;
            domainEvent.ProcessedAt = now;
            domainEvent.NextRetryAt = null;
            domainEvent.ErrorMessage = null;

            await db.SaveChangesAsync(cancellationToken);

            await audit.WriteAsync(
                "trainarr.training_event.processed",
                domainEvent.TenantId,
                WorkerActorUserId,
                "training_domain_event",
                domainEvent.Id.ToString(),
                "Succeeded",
                reasonCode: domainEvent.EventKind,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            domainEvent.ErrorMessage = Truncate(ex.Message, 512);
            await ApplyFailureAsync(domainEvent, maxAttempts, retryIntervalMinutes, now);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static Task ApplyFailureAsync(
        TrainingDomainEvent domainEvent,
        int maxAttempts,
        int retryIntervalMinutes,
        DateTimeOffset now)
    {
        if (domainEvent.AttemptCount >= maxAttempts)
        {
            domainEvent.ProcessingStatus = TrainingDomainEventStatuses.Abandoned;
            domainEvent.NextRetryAt = null;
        }
        else
        {
            domainEvent.ProcessingStatus = TrainingDomainEventStatuses.Pending;
            domainEvent.NextRetryAt = EventProcessingRules.ComputeNextRetryAt(now, retryIntervalMinutes);
        }

        return Task.CompletedTask;
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
