using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Services;

public sealed class PlatformOutboxPublisherWorkerService(
    NexArrDbContext db,
    PlatformOutboxPublisherSettingsService settingsService,
    IPlatformAuditService audit)
{
    public const string ProcessPublishActionScope = "nexarr.platform_outbox.publish";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000b2");

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<PendingPlatformOutboxPublisherResponse> ListPendingAsync(
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadOrDefaultAsync(cancellationToken);
        if (!settings.IsEnabled)
        {
            var asOfDisabled = asOfUtc ?? DateTimeOffset.UtcNow;
            return new PendingPlatformOutboxPublisherResponse(
                asOfDisabled,
                PlatformOutboxRules.NormalizeBatchSize(batchSize),
                []);
        }

        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = PlatformOutboxRules.NormalizeBatchSize(batchSize);
        var items = await LoadPendingEventsAsync(asOf, normalizedBatchSize, cancellationToken);
        return new PendingPlatformOutboxPublisherResponse(asOf, normalizedBatchSize, items);
    }

    public async Task<PlatformOutboxPublisherStatusResponse> GetStatusAsync(
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadOrDefaultAsync(cancellationToken);
        var asOf = DateTimeOffset.UtcNow;

        var pendingCount = await db.PlatformOutboxEvents.CountAsync(
            x => x.ProcessingStatus == PlatformOutboxEventStatuses.Pending,
            cancellationToken);
        var deadLetterCount = await db.PlatformOutboxEvents.CountAsync(
            x => x.ProcessingStatus == PlatformOutboxEventStatuses.DeadLetter,
            cancellationToken);

        var latestRun = await db.PlatformOutboxPublisherRuns
            .AsNoTracking()
            .OrderByDescending(x => x.ProcessedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return new PlatformOutboxPublisherStatusResponse(
            asOf,
            settings.IsEnabled,
            pendingCount,
            deadLetterCount,
            latestRun is null
                ? null
                : new PlatformOutboxPublisherRunItem(
                    latestRun.Id,
                    latestRun.Outcome,
                    latestRun.PublishedCount,
                    latestRun.FailedCount,
                    latestRun.DeadLetterCount,
                    latestRun.SkippedCount,
                    latestRun.SkipReason,
                    latestRun.ProcessedAt));
    }

    public async Task<PlatformOutboxEventsListResponse> ListRecentEventsAsync(
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = PlatformOutboxRules.NormalizeEventListLimit(limit);
        var rows = await db.PlatformOutboxEvents
            .AsNoTracking()
            .OrderByDescending(x => x.OccurredAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new PlatformOutboxEventsListResponse(rows.Select(MapEventItem).ToList());
    }

    public async Task<ProcessPlatformOutboxPublisherResponse> ProcessBatchAsync(
        ProcessPlatformOutboxPublisherRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadOrDefaultAsync(cancellationToken);
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = PlatformOutboxRules.NormalizeBatchSize(request.BatchSize);

        if (!settings.IsEnabled)
        {
            return new ProcessPlatformOutboxPublisherResponse(
                asOf,
                batchSize,
                0,
                0,
                0,
                0,
                0,
                [],
                []);
        }

        var candidates = await LoadPendingEventsAsync(asOf, batchSize, cancellationToken);
        var publishedEventIds = new List<Guid>();
        var skipped = new List<PlatformOutboxPublishSkip>();
        var failedCount = 0;
        var deadLetterCount = 0;

        foreach (var candidate in candidates)
        {
            try
            {
                var record = await db.PlatformOutboxEvents
                    .FirstOrDefaultAsync(x => x.Id == candidate.EventId, cancellationToken);

                if (record is null)
                {
                    skipped.Add(new PlatformOutboxPublishSkip(candidate.EventId, "Outbox event was not found."));
                    continue;
                }

                if (!string.Equals(record.ProcessingStatus, PlatformOutboxEventStatuses.Pending, StringComparison.Ordinal))
                {
                    skipped.Add(new PlatformOutboxPublishSkip(candidate.EventId, "Outbox event is no longer pending."));
                    continue;
                }

                if (!PlatformOutboxRules.IsReadyForProcessing(record.NextRetryAt, asOf))
                {
                    skipped.Add(new PlatformOutboxPublishSkip(candidate.EventId, "Outbox event is not ready for retry."));
                    continue;
                }

                await PublishEventAsync(record, settings, asOf, cancellationToken);
                publishedEventIds.Add(record.Id);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                failedCount++;
                await ApplyFailureAsync(
                    candidate.EventId,
                    settings,
                    asOf,
                    ex.Message,
                    cancellationToken);

                var updated = await db.PlatformOutboxEvents
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == candidate.EventId, cancellationToken);

                if (updated?.ProcessingStatus == PlatformOutboxEventStatuses.DeadLetter)
                {
                    deadLetterCount++;
                }
            }
        }

        if (candidates.Count > 0 || publishedEventIds.Count > 0 || skipped.Count > 0 || failedCount > 0)
        {
            var outcome = publishedEventIds.Count > 0
                ? "published"
                : deadLetterCount > 0
                    ? "dead_letter"
                    : failedCount > 0
                        ? "failed"
                        : skipped.Count > 0
                            ? "skipped"
                            : "none";

            db.PlatformOutboxPublisherRuns.Add(new PlatformOutboxPublisherRun
            {
                Id = Guid.NewGuid(),
                Outcome = outcome,
                PublishedCount = publishedEventIds.Count,
                FailedCount = failedCount,
                DeadLetterCount = deadLetterCount,
                SkippedCount = skipped.Count,
                SkipReason = skipped.Count > 0 && publishedEventIds.Count == 0 && failedCount == 0
                    ? PlatformOutboxRules.Truncate("One or more outbox events could not be published.", 512)
                    : null,
                ProcessedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
            });

            await db.SaveChangesAsync(cancellationToken);
        }

        return new ProcessPlatformOutboxPublisherResponse(
            asOf,
            batchSize,
            candidates.Count,
            publishedEventIds.Count,
            failedCount,
            deadLetterCount,
            skipped.Count,
            publishedEventIds,
            skipped);
    }

    public async Task<PlatformOutboxPublisherRunsResponse> ListRecentRunsAsync(
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = PlatformOutboxRules.NormalizeRunListLimit(limit);
        var rows = await db.PlatformOutboxPublisherRuns
            .AsNoTracking()
            .OrderByDescending(x => x.ProcessedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new PlatformOutboxPublisherRunsResponse(
            rows.Select(x => new PlatformOutboxPublisherRunItem(
                x.Id,
                x.Outcome,
                x.PublishedCount,
                x.FailedCount,
                x.DeadLetterCount,
                x.SkippedCount,
                x.SkipReason,
                x.ProcessedAt)).ToList());
    }

    private async Task PublishEventAsync(
        PlatformOutboxEvent record,
        PlatformOutboxPublisherSettings settings,
        DateTimeOffset asOf,
        CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<PlatformOutboxPayload>(record.PayloadJson, JsonOptions)
            ?? throw new InvalidOperationException("Platform outbox payload is invalid.");

        var now = DateTimeOffset.UtcNow;
        record.AttemptCount += 1;
        record.ProcessingStatus = PlatformOutboxEventStatuses.Published;
        record.PublishedAt = now;
        record.NextRetryAt = null;
        record.ErrorMessage = null;
        record.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "platform_outbox.published",
            "platform_outbox_event",
            record.Id.ToString(),
            "Success",
            tenantId: record.TenantId,
            actorUserId: WorkerActorUserId,
            reasonCode: record.EventType,
            cancellationToken: cancellationToken);
    }

    private async Task ApplyFailureAsync(
        Guid eventId,
        PlatformOutboxPublisherSettings settings,
        DateTimeOffset asOf,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        var record = await db.PlatformOutboxEvents.FirstOrDefaultAsync(x => x.Id == eventId, cancellationToken);
        if (record is null)
        {
            return;
        }

        record.AttemptCount += 1;
        record.ErrorMessage = PlatformOutboxRules.Truncate(errorMessage, 512);
        record.UpdatedAt = asOf;

        if (record.AttemptCount >= settings.MaxRetryAttempts)
        {
            record.ProcessingStatus = PlatformOutboxEventStatuses.DeadLetter;
            record.NextRetryAt = null;

            await audit.WriteAsync(
                "platform_outbox.dead_letter",
                "platform_outbox_event",
                record.Id.ToString(),
                "Failure",
                tenantId: record.TenantId,
                actorUserId: WorkerActorUserId,
                reasonCode: record.EventType,
                cancellationToken: cancellationToken);
        }
        else
        {
            record.NextRetryAt = PlatformOutboxRules.ComputeNextRetryAt(
                record.AttemptCount,
                settings.RetryIntervalMinutes,
                asOf);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<PendingPlatformOutboxEventItem>> LoadPendingEventsAsync(
        DateTimeOffset asOfUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var rows = await db.PlatformOutboxEvents
            .AsNoTracking()
            .Where(x => x.ProcessingStatus == PlatformOutboxEventStatuses.Pending
                && (x.NextRetryAt == null || x.NextRetryAt <= asOfUtc))
            .OrderBy(x => x.OccurredAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new PendingPlatformOutboxEventItem(
                x.Id,
                x.EventType,
                x.TenantId,
                x.ProcessingStatus,
                x.AttemptCount,
                x.OccurredAt,
                x.NextRetryAt))
            .ToList();
    }

    private static PlatformOutboxEventItemResponse MapEventItem(PlatformOutboxEvent x) =>
        new(
            x.Id,
            x.EventType,
            x.TenantId,
            x.ProcessingStatus,
            x.AttemptCount,
            x.ErrorMessage,
            x.OccurredAt,
            x.PublishedAt);
}
