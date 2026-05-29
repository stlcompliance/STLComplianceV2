using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;

namespace ComplianceCore.Api.Services;

public sealed class FactSourceSyncWorkerService(
    ComplianceCoreDbContext db,
    FactSourceSyncWorkerSettingsService settingsService,
    ProductFactApiFetcher productFactApiFetcher,
    FactSourceSyncCacheService syncCacheService,
    IComplianceCoreAuditService auditService)
{
    public const string ProcessBatchActionScope = "compliancecore.fact_sources.sync";

    public async Task<PendingFactSourceSyncsResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? intervalMinutes,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedInterval = FactSourceSyncRules.NormalizeIntervalMinutes(intervalMinutes);
        var normalizedBatchSize = FactSourceSyncRules.NormalizeBatchSize(batchSize);
        var items = await BuildDueItemsAsync(tenantId, asOf, normalizedInterval, normalizedBatchSize, cancellationToken);

        return new PendingFactSourceSyncsResponse(asOf, normalizedInterval, normalizedBatchSize, items);
    }

    public async Task<ProcessFactSourceSyncsResponse> ProcessBatchAsync(
        ProcessFactSourceSyncsRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var intervalMinutes = FactSourceSyncRules.NormalizeIntervalMinutes(request.IntervalMinutes);
        var batchSize = FactSourceSyncRules.NormalizeBatchSize(request.BatchSize);
        var dueItems = await BuildDueItemsAsync(request.TenantId, asOf, intervalMinutes, batchSize, cancellationToken);

        var results = new List<FactSourceSyncRunResult>();
        var succeeded = 0;
        var failed = 0;
        var skipped = 0;
        var tenantsTouched = new HashSet<Guid>();

        foreach (var item in dueItems)
        {
            tenantsTouched.Add(item.TenantId);
            var result = await SyncSourceAsync(item.FactSourceId, asOf, intervalMinutes, cancellationToken);
            results.Add(result);

            if (string.Equals(result.Status, "succeeded", StringComparison.OrdinalIgnoreCase))
            {
                succeeded++;
            }
            else if (string.Equals(result.Status, "skipped", StringComparison.OrdinalIgnoreCase))
            {
                skipped++;
            }
            else
            {
                failed++;
            }
        }

        foreach (var tenant in tenantsTouched)
        {
            var settings = await db.TenantFactSourceSyncWorkerSettings
                .FirstOrDefaultAsync(x => x.TenantId == tenant, cancellationToken);
            if (settings is not null)
            {
                settings.LastBatchRunAt = asOf;
                settings.UpdatedAt = asOf;
            }
        }

        if (tenantsTouched.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        await auditService.WriteAsync(
            "fact_source_sync.process_batch",
            request.TenantId ?? Guid.Empty,
            actorUserId: null,
            "fact_source_sync_batch",
            $"{succeeded}/{dueItems.Count}",
            failed == 0 ? "success" : "partial",
            cancellationToken: cancellationToken);

        return new ProcessFactSourceSyncsResponse(
            asOf,
            intervalMinutes,
            batchSize,
            dueItems.Count,
            succeeded,
            failed,
            skipped,
            results);
    }

    private async Task<FactSourceSyncRunResult> SyncSourceAsync(
        Guid factSourceId,
        DateTimeOffset asOf,
        int intervalMinutes,
        CancellationToken cancellationToken)
    {
        var source = await db.FactSources
            .Include(x => x.FactDefinition)
            .FirstOrDefaultAsync(x => x.Id == factSourceId && x.IsActive, cancellationToken);

        if (source is null
            || source.FactDefinition is null
            || !string.Equals(source.SourceType, FactSourceTypes.ProductApi, StringComparison.Ordinal))
        {
            return new FactSourceSyncRunResult(factSourceId, string.Empty, string.Empty, "skipped", "Source not found or not product_api.", null);
        }

        var tenantSettings = await settingsService.LoadEnabledSettingsAsync(source.TenantId, cancellationToken);
        if (tenantSettings is null)
        {
            return new FactSourceSyncRunResult(
                source.Id,
                source.SourceKey,
                source.FactDefinition.FactKey,
                "skipped",
                "Fact source sync worker is disabled for tenant.",
                null);
        }

        var config = FactSourceApiSyncConfigParser.Parse(
            source.ConfigJson,
            tenantSettings.DefaultScopeKey);

        var syncStatus = await EnsureSyncStatusAsync(source, config.ScopeKey, cancellationToken);
        syncStatus.LastAttemptAt = asOf;
        syncStatus.UpdatedAt = asOf;

        ProductFactApiFetchResult fetch;
        if (config.HasSnapshotValue)
        {
            fetch = productFactApiFetcher.FetchSnapshot(source.FactDefinition, config);
        }
        else
        {
            fetch = await productFactApiFetcher.FetchFromProductApiAsync(
                source.TenantId,
                source.FactDefinition,
                source,
                config,
                cancellationToken);
        }

        if (!fetch.Succeeded)
        {
            syncStatus.HealthStatus = FactSourceSyncStatuses.Failed;
            syncStatus.LastFailureAt = asOf;
            syncStatus.LastErrorMessage = fetch.ErrorMessage;
            syncStatus.ConsecutiveFailureCount++;
            await db.SaveChangesAsync(cancellationToken);

            return new FactSourceSyncRunResult(
                source.Id,
                source.SourceKey,
                source.FactDefinition.FactKey,
                "failed",
                fetch.ErrorMessage,
                null);
        }

        var mirrorId = await syncCacheService.UpsertMirrorAsync(
            source.TenantId,
            source.FactDefinition,
            source,
            config,
            fetch,
            asOf,
            cancellationToken);

        syncStatus.HealthStatus = FactSourceSyncStatuses.Healthy;
        syncStatus.LastSuccessAt = asOf;
        syncStatus.LastFailureAt = null;
        syncStatus.LastErrorMessage = null;
        syncStatus.ConsecutiveFailureCount = 0;
        syncStatus.LastMirrorId = mirrorId;
        syncStatus.ScopeKey = config.ScopeKey;
        await db.SaveChangesAsync(cancellationToken);

        return new FactSourceSyncRunResult(
            source.Id,
            source.SourceKey,
            source.FactDefinition.FactKey,
            "succeeded",
            null,
            mirrorId);
    }

    private async Task<FactSourceSyncStatus> EnsureSyncStatusAsync(
        FactSource source,
        string scopeKey,
        CancellationToken cancellationToken)
    {
        var status = await db.FactSourceSyncStatuses
            .FirstOrDefaultAsync(x => x.FactSourceId == source.Id, cancellationToken);

        if (status is not null)
        {
            return status;
        }

        var now = DateTimeOffset.UtcNow;
        status = new FactSourceSyncStatus
        {
            Id = Guid.NewGuid(),
            TenantId = source.TenantId,
            FactSourceId = source.Id,
            ScopeKey = scopeKey,
            HealthStatus = FactSourceSyncStatuses.Pending,
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.FactSourceSyncStatuses.Add(status);
        await db.SaveChangesAsync(cancellationToken);
        return status;
    }

    private async Task<IReadOnlyList<PendingFactSourceSyncItem>> BuildDueItemsAsync(
        Guid? tenantId,
        DateTimeOffset asOf,
        int intervalMinutes,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var enabledTenantIds = await db.TenantFactSourceSyncWorkerSettings
            .AsNoTracking()
            .Where(x => x.IsEnabled && (!tenantId.HasValue || x.TenantId == tenantId.Value))
            .Select(x => x.TenantId)
            .ToListAsync(cancellationToken);

        if (enabledTenantIds.Count == 0)
        {
            return Array.Empty<PendingFactSourceSyncItem>();
        }

        var sources = await db.FactSources
            .AsNoTracking()
            .Where(x => x.IsActive
                && x.SourceType == FactSourceTypes.ProductApi
                && enabledTenantIds.Contains(x.TenantId))
            .Join(
                db.FactDefinitions.AsNoTracking(),
                source => source.FactDefinitionId,
                definition => definition.Id,
                (source, definition) => new { source, definition })
            .ToListAsync(cancellationToken);

        var sourceIds = sources.Select(x => x.source.Id).ToList();
        var statuses = await db.FactSourceSyncStatuses
            .AsNoTracking()
            .Where(x => sourceIds.Contains(x.FactSourceId))
            .ToDictionaryAsync(x => x.FactSourceId, cancellationToken);

        var due = new List<PendingFactSourceSyncItem>();
        foreach (var row in sources)
        {
            statuses.TryGetValue(row.source.Id, out var status);
            if (!FactSourceSyncRules.IsDue(status?.LastAttemptAt, intervalMinutes, asOf))
            {
                continue;
            }

            var health = status is null
                ? FactSourceSyncStatuses.Pending
                : FactSourceSyncRules.ResolveHealthStatus(
                    status.LastSuccessAt,
                    status.LastFailureAt,
                    intervalMinutes,
                    asOf);

            due.Add(new PendingFactSourceSyncItem(
                row.source.Id,
                row.source.TenantId,
                row.source.SourceKey,
                row.definition.FactKey,
                row.source.ProductKey,
                status?.ScopeKey ?? FactSourceApiSyncConfigParser.Parse(row.source.ConfigJson, "tenant").ScopeKey,
                health,
                status?.LastSuccessAt));
        }

        return due
            .OrderBy(x => x.LastSuccessAt ?? DateTimeOffset.MinValue)
            .Take(batchSize)
            .ToList();
    }
}
