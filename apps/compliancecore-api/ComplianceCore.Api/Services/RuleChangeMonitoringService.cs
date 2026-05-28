using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class RuleChangeMonitoringService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public const string MonitorActionScope = "compliancecore.rule_changes.monitor";

    public const int DefaultListLimit = 50;

    public const int MaxListLimit = 200;

    public const int MaxScanBatchSize = 200;

    public async Task RecordVersionCreatedAsync(
        Guid tenantId,
        Guid? actorUserId,
        RulePack pack,
        string programKey,
        CancellationToken cancellationToken = default)
    {
        var summary =
            $"New rule pack version {pack.VersionNumber} created for {pack.PackKey} ({pack.Label}).";
        await PersistEventAsync(
            tenantId,
            pack,
            programKey,
            RuleChangeTypes.VersionCreated,
            summary,
            fromStatus: null,
            toStatus: pack.Status,
            fromVersion: null,
            toVersion: pack.VersionNumber,
            previousContentHash: null,
            newContentHash: RuleChangeHash.Compute(pack.RuleContentJson),
            RuleChangeSources.Api,
            actorUserId,
            scanRunId: null,
            cancellationToken);

        await UpsertSnapshotAsync(tenantId, pack, cancellationToken);
    }

    public async Task RecordStatusChangedAsync(
        Guid tenantId,
        Guid? actorUserId,
        RulePack pack,
        string programKey,
        string fromStatus,
        string toStatus,
        CancellationToken cancellationToken = default)
    {
        var summary = $"Rule pack {pack.PackKey} v{pack.VersionNumber} status changed from {fromStatus} to {toStatus}.";
        await PersistEventAsync(
            tenantId,
            pack,
            programKey,
            RuleChangeTypes.StatusChanged,
            summary,
            fromStatus,
            toStatus,
            fromVersion: pack.VersionNumber,
            toVersion: pack.VersionNumber,
            previousContentHash: null,
            newContentHash: RuleChangeHash.Compute(pack.RuleContentJson),
            RuleChangeSources.Api,
            actorUserId,
            scanRunId: null,
            cancellationToken);

        await UpsertSnapshotAsync(tenantId, pack, cancellationToken);
    }

    public async Task RecordContentUpdatedAsync(
        Guid tenantId,
        Guid? actorUserId,
        RulePack pack,
        string programKey,
        string? previousContentHash,
        string? newContentHash,
        CancellationToken cancellationToken = default)
    {
        var summary = $"Rule content updated for {pack.PackKey} v{pack.VersionNumber} ({pack.Status}).";
        await PersistEventAsync(
            tenantId,
            pack,
            programKey,
            RuleChangeTypes.ContentUpdated,
            summary,
            fromStatus: pack.Status,
            toStatus: pack.Status,
            fromVersion: pack.VersionNumber,
            toVersion: pack.VersionNumber,
            previousContentHash,
            newContentHash,
            RuleChangeSources.Api,
            actorUserId,
            scanRunId: null,
            cancellationToken);

        await UpsertSnapshotAsync(tenantId, pack, cancellationToken);
    }

    public async Task<IReadOnlyList<RuleChangeEventResponse>> ListEventsAsync(
        Guid tenantId,
        string? packKey,
        string? changeType,
        DateTimeOffset? since,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var cappedLimit = Math.Clamp(limit ?? DefaultListLimit, 1, MaxListLimit);
        var query = db.RuleChangeEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(packKey))
        {
            var normalized = packKey.Trim().ToLowerInvariant();
            query = query.Where(x => x.PackKey == normalized);
        }

        if (!string.IsNullOrWhiteSpace(changeType))
        {
            var normalizedType = changeType.Trim().ToLowerInvariant();
            if (!RuleChangeTypes.All.Contains(normalizedType))
            {
                throw new StlApiException(
                    "rule_changes.invalid_change_type",
                    "Change type filter is not recognized.",
                    400);
            }

            query = query.Where(x => x.ChangeType == normalizedType);
        }

        if (since.HasValue)
        {
            query = query.Where(x => x.DetectedAt >= since.Value);
        }

        return await query
            .OrderByDescending(x => x.DetectedAt)
            .Take(cappedLimit)
            .Select(x => MapResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<RuleChangeEventResponse?> GetEventAsync(
        Guid tenantId,
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.RuleChangeEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == eventId, cancellationToken);

        return entity is null ? null : MapResponse(entity);
    }

    public async Task<RuleChangeMonitoringSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var last24 = now.AddHours(-24);
        var last7 = now.AddDays(-7);

        var events = await db.RuleChangeEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new { x.ChangeType, x.DetectedAt })
            .ToListAsync(cancellationToken);

        return new RuleChangeMonitoringSummaryResponse(
            events.Count,
            events.Count(x => x.DetectedAt >= last24),
            events.Count(x => x.DetectedAt >= last7),
            events.Count(x => x.ChangeType == RuleChangeTypes.VersionCreated),
            events.Count(x => x.ChangeType == RuleChangeTypes.StatusChanged),
            events.Count(x => x.ChangeType == RuleChangeTypes.ContentUpdated),
            events.Count(x => x.ChangeType == RuleChangeTypes.ScanDetected),
            now);
    }

    public async Task<PendingRuleChangeScansResponse> ListPendingScansAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = NormalizeBatchSize(batchSize ?? 100);
        var items = await LoadPacksForScanAsync(tenantId, normalizedBatchSize, cancellationToken);

        var responses = new List<PendingRuleChangeScanItem>();
        foreach (var pack in items)
        {
            var programKey = await ResolveProgramKeyAsync(pack.RegulatoryProgramId, cancellationToken);
            var currentHash = RuleChangeHash.Compute(pack.RuleContentJson);
            var snapshot = await db.RulePackMonitorSnapshots
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.RulePackId == pack.Id, cancellationToken);

            if (snapshot is null || SnapshotDiffers(snapshot, pack, currentHash))
            {
                responses.Add(new PendingRuleChangeScanItem(
                    pack.TenantId,
                    pack.Id,
                    pack.PackKey,
                    programKey,
                    pack.VersionNumber,
                    pack.Status,
                    currentHash,
                    snapshot?.ContentHash));
            }
        }

        return new PendingRuleChangeScansResponse(asOf, normalizedBatchSize, responses);
    }

    public async Task<ProcessRuleChangeScanResponse> ProcessScanBatchAsync(
        ProcessRuleChangeScanRequest request,
        CancellationToken cancellationToken = default)
    {
        var batchSize = NormalizeBatchSize(request.BatchSize ?? 100);
        var packs = await LoadPacksForScanAsync(request.TenantId, batchSize, cancellationToken);

        var run = new RuleChangeScanRun
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            StartedAt = request.AsOfUtc ?? DateTimeOffset.UtcNow,
            Status = RuleChangeScanRunStatuses.InProgress,
        };

        db.RuleChangeScanRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);

        var detected = new List<RuleChangeEventResponse>();
        var changesCount = 0;

        foreach (var pack in packs)
        {
            var programKey = await ResolveProgramKeyAsync(pack.RegulatoryProgramId, cancellationToken);
            var currentHash = RuleChangeHash.Compute(pack.RuleContentJson);
            var snapshot = await db.RulePackMonitorSnapshots
                .FirstOrDefaultAsync(x => x.RulePackId == pack.Id, cancellationToken);

            if (snapshot is null)
            {
                await UpsertSnapshotAsync(pack.TenantId, pack, cancellationToken);
                continue;
            }

            if (!SnapshotDiffers(snapshot, pack, currentHash))
            {
                continue;
            }

            var summary = BuildScanSummary(snapshot, pack, currentHash);
            var entity = await PersistEventAsync(
                pack.TenantId,
                pack,
                programKey,
                RuleChangeTypes.ScanDetected,
                summary,
                snapshot.Status,
                pack.Status,
                snapshot.VersionNumber,
                pack.VersionNumber,
                snapshot.ContentHash,
                currentHash,
                RuleChangeSources.Worker,
                actorUserId: null,
                run.Id,
                cancellationToken);

            detected.Add(MapResponse(entity));
            changesCount++;
            await UpsertSnapshotAsync(pack.TenantId, pack, cancellationToken);
        }

        run.CompletedAt = DateTimeOffset.UtcNow;
        run.Status = RuleChangeScanRunStatuses.Completed;
        run.PacksScannedCount = packs.Count;
        run.ChangesDetectedCount = changesCount;
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "rule_changes.scan.completed",
            request.TenantId ?? Guid.Empty,
            actorUserId: null,
            "rule_change_scan_run",
            run.Id.ToString(),
            run.Status,
            reasonCode: $"{changesCount}/{packs.Count}",
            cancellationToken: cancellationToken);

        return new ProcessRuleChangeScanResponse(
            run.Id,
            run.Status,
            run.PacksScannedCount,
            run.ChangesDetectedCount,
            detected);
    }

    private async Task<RuleChangeEvent> PersistEventAsync(
        Guid tenantId,
        RulePack pack,
        string programKey,
        string changeType,
        string summary,
        string? fromStatus,
        string? toStatus,
        int? fromVersion,
        int? toVersion,
        string? previousContentHash,
        string? newContentHash,
        string source,
        Guid? actorUserId,
        Guid? scanRunId,
        CancellationToken cancellationToken)
    {
        var entity = new RuleChangeEvent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RulePackId = pack.Id,
            PackKey = pack.PackKey,
            ProgramKey = programKey,
            ChangeType = changeType,
            Summary = summary,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            FromVersion = fromVersion,
            ToVersion = toVersion,
            PreviousContentHash = previousContentHash,
            NewContentHash = newContentHash,
            Source = source,
            ActorUserId = actorUserId,
            ScanRunId = scanRunId,
            DetectedAt = DateTimeOffset.UtcNow,
        };

        db.RuleChangeEvents.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "rule_change.detected",
            tenantId,
            actorUserId,
            "rule_change_event",
            entity.Id.ToString(),
            "success",
            reasonCode: $"{changeType}:{pack.PackKey}",
            cancellationToken: cancellationToken);

        return entity;
    }

    private async Task UpsertSnapshotAsync(
        Guid tenantId,
        RulePack pack,
        CancellationToken cancellationToken)
    {
        var snapshot = await db.RulePackMonitorSnapshots
            .FirstOrDefaultAsync(x => x.RulePackId == pack.Id, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (snapshot is null)
        {
            snapshot = new RulePackMonitorSnapshot
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RulePackId = pack.Id,
            };
            db.RulePackMonitorSnapshots.Add(snapshot);
        }

        snapshot.PackKey = pack.PackKey;
        snapshot.VersionNumber = pack.VersionNumber;
        snapshot.Status = pack.Status;
        snapshot.ContentHash = RuleChangeHash.Compute(pack.RuleContentJson);
        snapshot.CapturedAt = now;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<RulePack>> LoadPacksForScanAsync(
        Guid? tenantId,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var query = db.RulePacks
            .Where(x => x.IsActive);

        if (tenantId.HasValue)
        {
            query = query.Where(x => x.TenantId == tenantId.Value);
        }

        return await query
            .OrderBy(x => x.UpdatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    private async Task<string> ResolveProgramKeyAsync(Guid programId, CancellationToken cancellationToken)
    {
        var program = await db.RegulatoryPrograms
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == programId, cancellationToken);

        return program?.ProgramKey ?? "unknown";
    }

    private static bool SnapshotDiffers(RulePackMonitorSnapshot snapshot, RulePack pack, string? currentHash) =>
        snapshot.VersionNumber != pack.VersionNumber
        || !string.Equals(snapshot.Status, pack.Status, StringComparison.OrdinalIgnoreCase)
        || !string.Equals(snapshot.ContentHash, currentHash, StringComparison.Ordinal);

    private static string BuildScanSummary(RulePackMonitorSnapshot snapshot, RulePack pack, string? currentHash)
    {
        var parts = new List<string>();
        if (snapshot.VersionNumber != pack.VersionNumber)
        {
            parts.Add($"version {snapshot.VersionNumber}→{pack.VersionNumber}");
        }

        if (!string.Equals(snapshot.Status, pack.Status, StringComparison.OrdinalIgnoreCase))
        {
            parts.Add($"status {snapshot.Status}→{pack.Status}");
        }

        if (!string.Equals(snapshot.ContentHash, currentHash, StringComparison.Ordinal))
        {
            parts.Add("rule content hash changed");
        }

        return $"Scan detected changes for {pack.PackKey}: {string.Join(", ", parts)}.";
    }

    private static int NormalizeBatchSize(int batchSize) => Math.Clamp(batchSize, 1, MaxScanBatchSize);

    private static RuleChangeEventResponse MapResponse(RuleChangeEvent entity) =>
        new(
            entity.Id,
            entity.RulePackId,
            entity.PackKey,
            entity.ProgramKey,
            entity.ChangeType,
            entity.Summary,
            entity.FromStatus,
            entity.ToStatus,
            entity.FromVersion,
            entity.ToVersion,
            entity.PreviousContentHash,
            entity.NewContentHash,
            entity.Source,
            entity.ActorUserId,
            entity.ScanRunId,
            entity.DetectedAt);
}
