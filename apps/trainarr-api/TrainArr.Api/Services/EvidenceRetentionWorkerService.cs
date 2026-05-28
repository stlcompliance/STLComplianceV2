using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class EvidenceRetentionWorkerService(
    TrainArrDbContext db,
    EvidenceRetentionSettingsService settingsService,
    TrainArrEvidenceStorageService storage,
    ITrainArrAuditService audit)
{
    public const string ProcessEvidenceRetentionActionScope = "trainarr.evidence.retention.purge";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f6");

    public async Task<PendingEvidenceRetentionResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = EvidenceRetentionRules.NormalizeBatchSize(batchSize);
        var items = await LoadPendingCandidatesAsync(tenantId, asOf, normalizedBatchSize, cancellationToken);

        var responseItems = items
            .Select(x => new PendingEvidenceRetentionItem(
                x.EvidenceId,
                x.TenantId,
                x.TrainingAssignmentId,
                x.EvidenceCreatedAt,
                x.AssignmentClosedAt))
            .ToList();

        return new PendingEvidenceRetentionResponse(asOf, normalizedBatchSize, responseItems);
    }

    public async Task<ProcessEvidenceRetentionResponse> ProcessBatchAsync(
        ProcessEvidenceRetentionRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = EvidenceRetentionRules.NormalizeBatchSize(request.BatchSize);
        var candidates = await LoadPendingCandidatesAsync(request.TenantId, asOf, batchSize, cancellationToken);

        var purgedEvidenceIds = new List<Guid>();
        var skipped = new List<EvidenceRetentionPurgeSkip>();
        long bytesReclaimed = 0;
        var runsByTenant = new Dictionary<Guid, TenantRunAccumulator>();

        foreach (var candidate in candidates)
        {
            var accumulator = GetOrCreateAccumulator(runsByTenant, candidate.TenantId);

            try
            {
                var evidence = await db.TrainingEvidence
                    .FirstOrDefaultAsync(
                        x => x.TenantId == candidate.TenantId && x.Id == candidate.EvidenceId,
                        cancellationToken);

                if (evidence is null)
                {
                    accumulator.SkippedCount++;
                    skipped.Add(new EvidenceRetentionPurgeSkip(candidate.EvidenceId, "Evidence record was not found."));
                    continue;
                }

                var reclaimedBytes = storage.TryDelete(evidence.StorageKey);
                bytesReclaimed += reclaimedBytes;
                db.TrainingEvidence.Remove(evidence);
                await db.SaveChangesAsync(cancellationToken);

                purgedEvidenceIds.Add(evidence.Id);
                accumulator.PurgedCount++;
                accumulator.BytesReclaimed += reclaimedBytes;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                accumulator.SkippedCount++;
                skipped.Add(new EvidenceRetentionPurgeSkip(candidate.EvidenceId, ex.Message));
            }
        }

        foreach (var (tenantId, accumulator) in runsByTenant)
        {
            var outcome = accumulator.PurgedCount > 0
                ? "purged"
                : accumulator.SkippedCount > 0
                    ? "skipped"
                    : "none";

            db.EvidenceRetentionRuns.Add(new EvidenceRetentionRun
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Outcome = outcome,
                EvidencePurgedCount = accumulator.PurgedCount,
                BytesReclaimed = accumulator.BytesReclaimed,
                SkippedCount = accumulator.SkippedCount,
                SkipReason = accumulator.SkippedCount > 0 && accumulator.PurgedCount == 0
                    ? Truncate("One or more evidence records could not be purged.", 512)
                    : null,
                ProcessedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
            });
        }

        if (runsByTenant.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        if (purgedEvidenceIds.Count > 0 && request.TenantId is Guid scopedTenantId)
        {
            await audit.WriteAsync(
                "evidence_retention.batch",
                scopedTenantId,
                WorkerActorUserId,
                "training_evidence",
                $"{purgedEvidenceIds.Count}",
                "Succeeded",
                cancellationToken: cancellationToken);
        }

        return new ProcessEvidenceRetentionResponse(
            asOf,
            batchSize,
            candidates.Count,
            purgedEvidenceIds.Count,
            bytesReclaimed,
            skipped.Count,
            purgedEvidenceIds,
            skipped);
    }

    public async Task<EvidenceRetentionRunsResponse> ListRecentRunsAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = EvidenceRetentionRules.NormalizeRunListLimit(limit);
        var rows = await db.EvidenceRetentionRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.ProcessedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(x => new EvidenceRetentionRunItem(
                x.Id,
                x.Outcome,
                x.EvidencePurgedCount,
                x.BytesReclaimed,
                x.SkippedCount,
                x.SkipReason,
                x.ProcessedAt))
            .ToList();

        return new EvidenceRetentionRunsResponse(items);
    }

    private async Task<IReadOnlyList<PendingEvidenceCandidate>> LoadPendingCandidatesAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var settingsQuery = db.TenantEvidenceRetentionSettings
            .AsNoTracking()
            .Where(x => x.IsEnabled);

        if (tenantId is Guid scopedTenantId)
        {
            settingsQuery = settingsQuery.Where(x => x.TenantId == scopedTenantId);
        }

        var tenantSettings = await settingsQuery.ToListAsync(cancellationToken);
        var results = new List<PendingEvidenceCandidate>();

        foreach (var settings in tenantSettings)
        {
            if (results.Count >= batchSize)
            {
                break;
            }

            var retentionDays = EvidenceRetentionRules.NormalizeRetentionDays(
                settings.RetentionDaysAfterAssignmentClose);
            var cutoff = asOfUtc.AddDays(-retentionDays);
            var remaining = batchSize - results.Count;

            var rows = await (
                    from evidence in db.TrainingEvidence.AsNoTracking()
                    join assignment in db.TrainingAssignments.AsNoTracking()
                        on evidence.TrainingAssignmentId equals assignment.Id
                    where evidence.TenantId == settings.TenantId
                          && assignment.TenantId == settings.TenantId
                          && EvidenceRetentionRules.IsClosedAssignmentStatus(assignment.Status)
                          && (
                              (assignment.Status == "completed"
                               && (assignment.CompletedAt ?? assignment.UpdatedAt) < cutoff)
                              || (assignment.Status == "cancelled" && assignment.UpdatedAt < cutoff))
                    orderby evidence.CreatedAt
                    select new
                    {
                        evidence.Id,
                        evidence.TenantId,
                        evidence.TrainingAssignmentId,
                        evidence.CreatedAt,
                        AssignmentClosedAt = assignment.Status == "completed"
                            ? (assignment.CompletedAt ?? assignment.UpdatedAt)
                            : assignment.UpdatedAt,
                    })
                .Take(remaining)
                .ToListAsync(cancellationToken);

            foreach (var row in rows)
            {
                results.Add(new PendingEvidenceCandidate(
                    row.Id,
                    row.TenantId,
                    row.TrainingAssignmentId,
                    row.CreatedAt,
                    row.AssignmentClosedAt));
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

    private sealed record PendingEvidenceCandidate(
        Guid EvidenceId,
        Guid TenantId,
        Guid TrainingAssignmentId,
        DateTimeOffset EvidenceCreatedAt,
        DateTimeOffset AssignmentClosedAt);
}
