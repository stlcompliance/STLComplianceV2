using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public sealed class MaintenancePlatformEventProcessingService(
    MaintainArrDbContext db,
    MaintenancePlatformEventSettingsService settingsService,
    IMaintainArrAuditService audit)
{
    public const string ProcessEventsActionScope = "maintainarr.platform_events.process";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f8");

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task TryProcessSingleAsync(
        MaintenancePlatformOutboxEvent outboxEvent,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(outboxEvent.ProcessingStatus, MaintenancePlatformEventStatuses.Pending, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var settings = await settingsService.LoadSnapshotAsync(outboxEvent.TenantId, cancellationToken);
        if (!MaintenancePlatformEventRules.ShouldEmitForTenant(settings))
        {
            return;
        }

        await ProcessEventAsync(outboxEvent, settings, cancellationToken);
    }

    public async Task<PendingMaintenancePlatformOutboxResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = MaintenancePlatformEventRules.NormalizeBatchSize(batchSize);
        var pending = await LoadPendingAsync(tenantId, asOf, normalizedBatchSize, cancellationToken);

        return new PendingMaintenancePlatformOutboxResponse(
            asOf,
            normalizedBatchSize,
            pending.Select(x => new PendingMaintenancePlatformOutboxItem(
                x.Id,
                x.TenantId,
                x.EventKind,
                x.RelatedEntityId,
                x.AttemptCount,
                x.NextRetryAt,
                x.CreatedAt)).ToList());
    }

    public async Task<ProcessMaintenancePlatformEventsResponse> ProcessBatchAsync(
        ProcessMaintenancePlatformEventsRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = MaintenancePlatformEventRules.NormalizeBatchSize(request.BatchSize);
        var pending = await LoadPendingAsync(request.TenantId, asOf, batchSize, cancellationToken);

        var results = new List<MaintenancePlatformEventProcessResult>();
        var skipped = new List<MaintenancePlatformEventProcessSkip>();
        var processedCount = 0;
        var retriedCount = 0;
        var abandonedCount = 0;
        var runStats = new Dictionary<Guid, (int Processed, int Retried, int Abandoned, int Skipped)>();

        foreach (var outboxEvent in pending)
        {
            if (!runStats.ContainsKey(outboxEvent.TenantId))
            {
                runStats[outboxEvent.TenantId] = (0, 0, 0, 0);
            }

            try
            {
                var settings = await settingsService.LoadSnapshotAsync(outboxEvent.TenantId, cancellationToken);
                if (!MaintenancePlatformEventRules.ShouldEmitForTenant(settings))
                {
                    skipped.Add(new MaintenancePlatformEventProcessSkip(outboxEvent.Id, "tenant_processing_disabled"));
                    IncrementSkipped(outboxEvent.TenantId, runStats);
                    continue;
                }

                var beforeStatus = outboxEvent.ProcessingStatus;
                await ProcessEventAsync(outboxEvent, settings, cancellationToken);

                if (string.Equals(outboxEvent.ProcessingStatus, MaintenancePlatformEventStatuses.Processed, StringComparison.OrdinalIgnoreCase))
                {
                    processedCount++;
                    IncrementProcessed(outboxEvent.TenantId, runStats);
                }
                else if (string.Equals(outboxEvent.ProcessingStatus, MaintenancePlatformEventStatuses.Abandoned, StringComparison.OrdinalIgnoreCase))
                {
                    abandonedCount++;
                    IncrementAbandoned(outboxEvent.TenantId, runStats);
                }
                else if (outboxEvent.AttemptCount > 0 && beforeStatus == outboxEvent.ProcessingStatus)
                {
                    retriedCount++;
                    IncrementRetried(outboxEvent.TenantId, runStats);
                }

                results.Add(new MaintenancePlatformEventProcessResult(
                    outboxEvent.Id,
                    outboxEvent.ProcessingStatus,
                    outboxEvent.AttemptCount));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new MaintenancePlatformEventProcessSkip(outboxEvent.Id, ex.Message));
                IncrementSkipped(outboxEvent.TenantId, runStats);
            }
        }

        foreach (var (tenantId, stats) in runStats)
        {
            db.MaintenancePlatformEventProcessingRuns.Add(new MaintenancePlatformEventProcessingRun
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PendingFound = pending.Count(x => x.TenantId == tenantId),
                ProcessedCount = stats.Processed,
                RetriedCount = stats.Retried,
                AbandonedCount = stats.Abandoned,
                SkippedCount = stats.Skipped,
                CreatedAt = asOf,
            });
        }

        if (runStats.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        if (results.Count > 0 && request.TenantId is Guid scopedTenantId)
        {
            await audit.WriteAsync(
                "maintainarr.platform_events.batch",
                scopedTenantId,
                WorkerActorUserId,
                "maintenance_platform_outbox_event",
                $"{results.Count}",
                "success",
                cancellationToken: cancellationToken);
        }

        return new ProcessMaintenancePlatformEventsResponse(
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

    public async Task<MaintenancePlatformOutboxEventsResponse> ListRecentAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = MaintenancePlatformEventRules.NormalizeEventListLimit(limit);
        var rows = await db.MaintenancePlatformOutboxEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new MaintenancePlatformOutboxEventsResponse(
            rows.Select(x => new MaintenancePlatformOutboxEventItem(
                x.Id,
                x.EventKind,
                x.ProcessingStatus,
                x.RelatedEntityId,
                x.AttemptCount,
                x.ErrorMessage,
                x.CreatedAt,
                x.ProcessedAt)).ToList());
    }

    public async Task<MaintenancePlatformEventProcessingRunsResponse> ListRecentRunsAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = MaintenancePlatformEventRules.NormalizeRunListLimit(limit);
        var rows = await db.MaintenancePlatformEventProcessingRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new MaintenancePlatformEventProcessingRunsResponse(
            rows.Select(x => new MaintenancePlatformEventProcessingRunItem(
                x.Id,
                x.PendingFound,
                x.ProcessedCount,
                x.RetriedCount,
                x.AbandonedCount,
                x.SkippedCount,
                x.CreatedAt)).ToList());
    }

    private async Task<List<MaintenancePlatformOutboxEvent>> LoadPendingAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var query = db.MaintenancePlatformOutboxEvents
            .Where(x => x.ProcessingStatus == MaintenancePlatformEventStatuses.Pending
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
        MaintenancePlatformOutboxEvent outboxEvent,
        TenantMaintenancePlatformEventSettingsSnapshot? settings,
        CancellationToken cancellationToken)
    {
        var maxAttempts = MaintenancePlatformEventRules.NormalizeMaxAttempts(settings?.MaxAttempts);
        var retryIntervalMinutes = MaintenancePlatformEventRules.NormalizeRetryIntervalMinutes(settings?.RetryIntervalMinutes);
        var now = DateTimeOffset.UtcNow;

        outboxEvent.AttemptCount += 1;
        outboxEvent.UpdatedAt = now;

        try
        {
            _ = JsonSerializer.Deserialize<MaintenancePlatformEventPayload>(outboxEvent.PayloadJson, JsonOptions)
                ?? throw new InvalidOperationException("Maintenance platform event payload is invalid.");

            outboxEvent.ProcessingStatus = MaintenancePlatformEventStatuses.Processed;
            outboxEvent.ProcessedAt = now;
            outboxEvent.NextRetryAt = null;
            outboxEvent.ErrorMessage = null;

            await db.SaveChangesAsync(cancellationToken);

            await audit.WriteAsync(
                "maintainarr.platform_event.processed",
                outboxEvent.TenantId,
                WorkerActorUserId,
                "maintenance_platform_outbox_event",
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

    private static void ApplyFailure(
        MaintenancePlatformOutboxEvent outboxEvent,
        int maxAttempts,
        int retryIntervalMinutes,
        DateTimeOffset now)
    {
        if (outboxEvent.AttemptCount >= maxAttempts)
        {
            outboxEvent.ProcessingStatus = MaintenancePlatformEventStatuses.Abandoned;
            outboxEvent.NextRetryAt = null;
        }
        else
        {
            outboxEvent.ProcessingStatus = MaintenancePlatformEventStatuses.Pending;
            outboxEvent.NextRetryAt = MaintenancePlatformEventRules.ComputeNextRetryAt(now, retryIntervalMinutes);
        }
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];

    private static void IncrementProcessed(Guid tenantId, Dictionary<Guid, (int Processed, int Retried, int Abandoned, int Skipped)> runStats)
    {
        var stats = runStats[tenantId];
        stats.Processed++;
        runStats[tenantId] = stats;
    }

    private static void IncrementRetried(Guid tenantId, Dictionary<Guid, (int Processed, int Retried, int Abandoned, int Skipped)> runStats)
    {
        var stats = runStats[tenantId];
        stats.Retried++;
        runStats[tenantId] = stats;
    }

    private static void IncrementAbandoned(Guid tenantId, Dictionary<Guid, (int Processed, int Retried, int Abandoned, int Skipped)> runStats)
    {
        var stats = runStats[tenantId];
        stats.Abandoned++;
        runStats[tenantId] = stats;
    }

    private static void IncrementSkipped(Guid tenantId, Dictionary<Guid, (int Processed, int Retried, int Abandoned, int Skipped)> runStats)
    {
        var stats = runStats[tenantId];
        stats.Skipped++;
        runStats[tenantId] = stats;
    }
}
