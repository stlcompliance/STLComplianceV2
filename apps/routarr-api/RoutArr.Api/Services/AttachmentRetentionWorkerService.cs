using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public sealed class AttachmentRetentionWorkerService(
    RoutArrDbContext db,
    RoutArrCaptureAttachmentStorageService storage,
    IRoutArrAuditService audit)
{
    public const string ProcessAttachmentRetentionActionScope = "routarr.attachments.retention.purge";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000fc");

    public async Task<PendingAttachmentRetentionResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = AttachmentRetentionRules.NormalizeBatchSize(batchSize);
        var items = await LoadPendingCandidatesAsync(tenantId, asOf, normalizedBatchSize, cancellationToken);

        var responseItems = items
            .Select(x => new PendingAttachmentRetentionItem(
                x.AttachmentId,
                x.TenantId,
                x.TripId,
                x.AttachmentCreatedAt,
                x.TripClosedAt))
            .ToList();

        return new PendingAttachmentRetentionResponse(asOf, normalizedBatchSize, responseItems);
    }

    public async Task<ProcessAttachmentRetentionResponse> ProcessBatchAsync(
        ProcessAttachmentRetentionRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = AttachmentRetentionRules.NormalizeBatchSize(request.BatchSize);
        var candidates = await LoadPendingCandidatesAsync(request.TenantId, asOf, batchSize, cancellationToken);

        var purgedAttachmentIds = new List<Guid>();
        var skipped = new List<AttachmentRetentionPurgeSkip>();
        long bytesReclaimed = 0;
        var runsByTenant = new Dictionary<Guid, TenantRunAccumulator>();

        foreach (var candidate in candidates)
        {
            var accumulator = GetOrCreateAccumulator(runsByTenant, candidate.TenantId);

            try
            {
                var attachment = await db.TripCaptureAttachments
                    .FirstOrDefaultAsync(
                        x => x.TenantId == candidate.TenantId && x.Id == candidate.AttachmentId,
                        cancellationToken);

                if (attachment is null)
                {
                    accumulator.SkippedCount++;
                    skipped.Add(new AttachmentRetentionPurgeSkip(candidate.AttachmentId, "Attachment record was not found."));
                    continue;
                }

                var reclaimedBytes = storage.TryDelete(attachment.StorageKey);
                bytesReclaimed += reclaimedBytes;
                db.TripCaptureAttachments.Remove(attachment);
                await db.SaveChangesAsync(cancellationToken);

                purgedAttachmentIds.Add(attachment.Id);
                accumulator.PurgedCount++;
                accumulator.BytesReclaimed += reclaimedBytes;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                accumulator.SkippedCount++;
                skipped.Add(new AttachmentRetentionPurgeSkip(candidate.AttachmentId, ex.Message));
            }
        }

        foreach (var (tenantId, accumulator) in runsByTenant)
        {
            var outcome = accumulator.PurgedCount > 0
                ? "purged"
                : accumulator.SkippedCount > 0
                    ? "skipped"
                    : "none";

            db.AttachmentRetentionRuns.Add(new AttachmentRetentionRun
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Outcome = outcome,
                AttachmentsPurgedCount = accumulator.PurgedCount,
                BytesReclaimed = accumulator.BytesReclaimed,
                SkippedCount = accumulator.SkippedCount,
                SkipReason = accumulator.SkippedCount > 0 && accumulator.PurgedCount == 0
                    ? Truncate("One or more capture attachments could not be purged.", 512)
                    : null,
                ProcessedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
            });
        }

        if (runsByTenant.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        if (purgedAttachmentIds.Count > 0 && request.TenantId is Guid scopedTenantId)
        {
            await audit.WriteAsync(
                "attachment_retention.batch",
                scopedTenantId,
                WorkerActorUserId,
                "trip_capture_attachment",
                $"{purgedAttachmentIds.Count}",
                "Succeeded",
                cancellationToken: cancellationToken);
        }

        return new ProcessAttachmentRetentionResponse(
            asOf,
            batchSize,
            candidates.Count,
            purgedAttachmentIds.Count,
            bytesReclaimed,
            skipped.Count,
            purgedAttachmentIds,
            skipped);
    }

    public async Task<AttachmentRetentionRunsResponse> ListRecentRunsAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = AttachmentRetentionRules.NormalizeRunListLimit(limit);
        var rows = await db.AttachmentRetentionRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.ProcessedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(x => new AttachmentRetentionRunItem(
                x.Id,
                x.Outcome,
                x.AttachmentsPurgedCount,
                x.BytesReclaimed,
                x.SkippedCount,
                x.SkipReason,
                x.ProcessedAt))
            .ToList();

        return new AttachmentRetentionRunsResponse(items);
    }

    private async Task<IReadOnlyList<PendingAttachmentCandidate>> LoadPendingCandidatesAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var settingsQuery = db.TenantAttachmentRetentionSettings
            .AsNoTracking()
            .Where(x => x.IsEnabled);

        if (tenantId is Guid scopedTenantId)
        {
            settingsQuery = settingsQuery.Where(x => x.TenantId == scopedTenantId);
        }

        var tenantSettings = await settingsQuery.ToListAsync(cancellationToken);
        var results = new List<PendingAttachmentCandidate>();

        foreach (var settings in tenantSettings)
        {
            if (results.Count >= batchSize)
            {
                break;
            }

            var retentionDays = AttachmentRetentionRules.NormalizeRetentionDays(
                settings.RetentionDaysAfterTripClose);
            var cutoff = asOfUtc.AddDays(-retentionDays);
            var remaining = batchSize - results.Count;

            var rows = await (
                    from attachment in db.TripCaptureAttachments.AsNoTracking()
                    join trip in db.Trips.AsNoTracking()
                        on attachment.TripId equals trip.Id
                    where attachment.TenantId == settings.TenantId
                          && trip.TenantId == settings.TenantId
                          && AttachmentRetentionRules.IsClosedTripStatus(trip.DispatchStatus)
                          && (
                              (trip.DispatchStatus == TripDispatchStatuses.Completed
                               && (trip.ClosedAt ?? trip.CompletedAt ?? trip.UpdatedAt) < cutoff)
                              || (trip.DispatchStatus == TripDispatchStatuses.Cancelled
                                  && (trip.CancelledAt ?? trip.UpdatedAt) < cutoff))
                    orderby attachment.CreatedAt
                    select new
                    {
                        attachment.Id,
                        attachment.TenantId,
                        attachment.TripId,
                        attachment.CreatedAt,
                        TripClosedAt = trip.DispatchStatus == TripDispatchStatuses.Completed
                            ? (trip.ClosedAt ?? trip.CompletedAt ?? trip.UpdatedAt)
                            : (trip.CancelledAt ?? trip.UpdatedAt),
                    })
                .Take(remaining)
                .ToListAsync(cancellationToken);

            foreach (var row in rows)
            {
                results.Add(new PendingAttachmentCandidate(
                    row.Id,
                    row.TenantId,
                    row.TripId,
                    row.CreatedAt,
                    row.TripClosedAt));
            }
        }

        return results;
    }

    private static TenantRunAccumulator GetOrCreateAccumulator(
        Dictionary<Guid, TenantRunAccumulator> runsByTenant,
        Guid tenantId)
    {
        if (!runsByTenant.TryGetValue(tenantId, out var accumulator))
        {
            accumulator = new TenantRunAccumulator();
            runsByTenant[tenantId] = accumulator;
        }

        return accumulator;
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];

    private sealed class TenantRunAccumulator
    {
        public int PurgedCount { get; set; }

        public long BytesReclaimed { get; set; }

        public int SkippedCount { get; set; }
    }

    private sealed record PendingAttachmentCandidate(
        Guid AttachmentId,
        Guid TenantId,
        Guid TripId,
        DateTimeOffset AttachmentCreatedAt,
        DateTimeOffset TripClosedAt);
}
