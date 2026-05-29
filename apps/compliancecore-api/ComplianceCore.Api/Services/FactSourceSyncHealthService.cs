using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;

namespace ComplianceCore.Api.Services;

public sealed class FactSourceSyncHealthService(
    ComplianceCoreDbContext db,
    FactSourceSyncWorkerSettingsService settingsService)
{
    public async Task<FactSourceSyncHealthResponse> GetHealthAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.GetAsync(tenantId, cancellationToken);
        var asOf = DateTimeOffset.UtcNow;
        var intervalMinutes = FactSourceSyncRules.NormalizeIntervalMinutes(settings.IntervalMinutes);

        var sources = await db.FactSources
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.IsActive
                && x.SourceType == FactSourceTypes.ProductApi)
            .Join(
                db.FactDefinitions.AsNoTracking().Where(d => d.TenantId == tenantId),
                source => source.FactDefinitionId,
                definition => definition.Id,
                (source, definition) => new { source, definition })
            .OrderBy(x => x.source.Priority)
            .ThenBy(x => x.source.Label)
            .ToListAsync(cancellationToken);

        var sourceIds = sources.Select(x => x.source.Id).ToList();
        var statuses = await db.FactSourceSyncStatuses
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && sourceIds.Contains(x.FactSourceId))
            .ToDictionaryAsync(x => x.FactSourceId, cancellationToken);

        var items = new List<FactSourceSyncHealthItem>();
        var healthy = 0;
        var stale = 0;
        var failed = 0;
        var pending = 0;

        foreach (var row in sources)
        {
            statuses.TryGetValue(row.source.Id, out var status);
            var configScope = FactSourceApiSyncConfigParser.Parse(row.source.ConfigJson, settings.DefaultScopeKey).ScopeKey;
            var health = status is null
                ? FactSourceSyncStatuses.Pending
                : FactSourceSyncRules.ResolveHealthStatus(
                    status.LastSuccessAt,
                    status.LastFailureAt,
                    intervalMinutes,
                    asOf);

            switch (health)
            {
                case FactSourceSyncStatuses.Healthy:
                    healthy++;
                    break;
                case FactSourceSyncStatuses.Stale:
                    stale++;
                    break;
                case FactSourceSyncStatuses.Failed:
                    failed++;
                    break;
                default:
                    pending++;
                    break;
            }

            items.Add(new FactSourceSyncHealthItem(
                row.source.Id,
                row.source.SourceKey,
                row.definition.FactKey,
                row.source.SourceType,
                row.source.ProductKey,
                status?.ScopeKey ?? configScope,
                health,
                status?.LastAttemptAt,
                status?.LastSuccessAt,
                status?.LastFailureAt,
                status?.LastErrorMessage,
                status?.ConsecutiveFailureCount ?? 0));
        }

        return new FactSourceSyncHealthResponse(
            tenantId,
            settings.IsEnabled,
            intervalMinutes,
            settings.LastBatchRunAt,
            sources.Count,
            healthy,
            stale,
            failed,
            pending,
            items);
    }
}
