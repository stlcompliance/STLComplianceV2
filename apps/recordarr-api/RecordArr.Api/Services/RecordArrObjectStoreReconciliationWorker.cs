using System.Text.Json;
using Microsoft.Extensions.Options;
using RecordArr.Api.Data;
using RecordArr.Api.Models;
using RecordArr.Api.Options;

namespace RecordArr.Api.Services;

public interface IRecordArrObjectStoreInventoryProvider
{
    Task<IReadOnlyList<RecordArrObjectStoreInventoryResult>> GetInventoryAsync(
        string tenantId,
        IReadOnlyList<RecordArrFileResponse> candidateFiles,
        CancellationToken cancellationToken);
}

public sealed class ManifestRecordArrObjectStoreInventoryProvider(
    IOptionsMonitor<ObjectStoreReconciliationWorkerOptions> options,
    ILogger<ManifestRecordArrObjectStoreInventoryProvider> logger) : IRecordArrObjectStoreInventoryProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<RecordArrObjectStoreInventoryResult>> GetInventoryAsync(
        string tenantId,
        IReadOnlyList<RecordArrFileResponse> candidateFiles,
        CancellationToken cancellationToken)
    {
        var manifestPath = options.CurrentValue.InventoryManifestPath;
        if (string.IsNullOrWhiteSpace(manifestPath))
        {
            logger.LogWarning("RecordArr object-store reconciliation worker is enabled without an inventory manifest path; no reconciliation will be recorded.");
            return Array.Empty<RecordArrObjectStoreInventoryResult>();
        }

        if (!File.Exists(manifestPath))
        {
            logger.LogWarning("RecordArr object-store inventory manifest {ManifestPath} was not found; no reconciliation will be recorded.", manifestPath);
            return Array.Empty<RecordArrObjectStoreInventoryResult>();
        }

        await using var stream = File.OpenRead(manifestPath);
        var manifest = await JsonSerializer.DeserializeAsync<ObjectStoreInventoryManifest>(stream, JsonOptions, cancellationToken);
        if (manifest?.Inventories is null || manifest.Inventories.Count == 0)
        {
            return Array.Empty<RecordArrObjectStoreInventoryResult>();
        }

        var knownFiles = candidateFiles
            .GroupBy(file => file.FileId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        return manifest.Inventories
            .Where(row => string.IsNullOrWhiteSpace(row.TenantId) || string.Equals(row.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
            .Select(row => ToInventoryResult(row, knownFiles))
            .Where(result => HasExplicitEvidence(result))
            .ToArray();
    }

    private static RecordArrObjectStoreInventoryResult ToInventoryResult(
        ObjectStoreInventoryRow row,
        IReadOnlyDictionary<string, RecordArrFileResponse> knownFiles)
    {
        var recordId = string.IsNullOrWhiteSpace(row.RecordId) ? null : row.RecordId.Trim();
        var scopedFileIds = knownFiles
            .Where(pair => string.IsNullOrWhiteSpace(recordId) || string.Equals(pair.Value.RecordId, recordId, StringComparison.OrdinalIgnoreCase))
            .Select(pair => pair.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return new RecordArrObjectStoreInventoryResult(
            row.Scope,
            recordId,
            FilterKnown(row.VerifiedFileIds, scopedFileIds),
            FilterKnown(row.MissingFileIds, scopedFileIds),
            FilterKnown(row.CorruptFileIds, scopedFileIds),
            FilterKnown(row.RestoredFileIds, scopedFileIds),
            FilterKnown(row.AcceptedMissingFileIds, scopedFileIds),
            FilterKnown(row.RecheckedCorruptFileIds, scopedFileIds),
            FilterKnown(row.ReleasedQuarantinedFileIds, scopedFileIds),
            FilterKnown(row.ScannedPendingFileIds, scopedFileIds));
    }

    private static IReadOnlyList<string> FilterKnown(IReadOnlyList<string>? fileIds, ISet<string> knownFileIds) =>
        (fileIds ?? Array.Empty<string>())
            .Where(fileId => !string.IsNullOrWhiteSpace(fileId) && knownFileIds.Contains(fileId.Trim()))
            .Select(fileId => fileId.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static bool HasExplicitEvidence(RecordArrObjectStoreInventoryResult result) =>
        HasAny(result.VerifiedFileIds) ||
        HasAny(result.MissingFileIds) ||
        HasAny(result.CorruptFileIds) ||
        HasAny(result.RestoredFileIds) ||
        HasAny(result.AcceptedMissingFileIds) ||
        HasAny(result.RecheckedCorruptFileIds) ||
        HasAny(result.ReleasedQuarantinedFileIds) ||
        HasAny(result.ScannedPendingFileIds);

    private static bool HasAny(IReadOnlyList<string>? values) => values is { Count: > 0 };

    private sealed record ObjectStoreInventoryManifest(IReadOnlyList<ObjectStoreInventoryRow> Inventories);

    private sealed record ObjectStoreInventoryRow(
        string? TenantId,
        string? Scope,
        string? RecordId,
        IReadOnlyList<string>? VerifiedFileIds,
        IReadOnlyList<string>? MissingFileIds,
        IReadOnlyList<string>? CorruptFileIds,
        IReadOnlyList<string>? RestoredFileIds,
        IReadOnlyList<string>? AcceptedMissingFileIds,
        IReadOnlyList<string>? RecheckedCorruptFileIds,
        IReadOnlyList<string>? ReleasedQuarantinedFileIds,
        IReadOnlyList<string>? ScannedPendingFileIds);
}

public sealed class RecordArrObjectStoreReconciliationWorker(
    IServiceScopeFactory scopeFactory,
    IRecordArrObjectStoreInventoryProvider inventoryProvider,
    IOptionsMonitor<ObjectStoreReconciliationWorkerOptions> options,
    ILogger<RecordArrObjectStoreReconciliationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.CurrentValue.Enabled)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunOnceAsync(stoppingToken);
            var delay = TimeSpan.FromSeconds(Math.Clamp(options.CurrentValue.PollIntervalSeconds, 5, 3600));
            await Task.Delay(delay, stoppingToken);
        }
    }

    public async Task RunOnceAsync(CancellationToken cancellationToken = default)
    {
        var currentOptions = options.CurrentValue;
        if (!currentOptions.Enabled)
        {
            return;
        }

        var tenantIds = currentOptions.TenantIds
            .Where(tenantId => !string.IsNullOrWhiteSpace(tenantId))
            .Select(tenantId => tenantId.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (tenantIds.Length == 0)
        {
            logger.LogWarning("RecordArr object-store reconciliation worker is enabled without tenant ids; no reconciliation will be recorded.");
            return;
        }

        foreach (var tenantId in tenantIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var scope = scopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var candidateFiles = store.GetStorageReconciliationCandidateFiles(tenantId);
            if (candidateFiles.Count == 0)
            {
                continue;
            }

            var inventoryResults = await inventoryProvider.GetInventoryAsync(tenantId, candidateFiles, cancellationToken);
            if (inventoryResults.Count == 0)
            {
                logger.LogInformation(
                    "RecordArr object-store reconciliation worker found {CandidateFileCount} candidate file(s) for tenant {TenantId}, but no explicit external inventory evidence was available.",
                    candidateFiles.Count,
                    tenantId);
                continue;
            }

            foreach (var inventory in inventoryResults)
            {
                var checkedFileIds = MergeFileIds(inventory.VerifiedFileIds, inventory.MissingFileIds, inventory.CorruptFileIds);
                if (checkedFileIds.Length == 0)
                {
                    continue;
                }

                var reconciliation = store.RunStorageReconciliation(
                    tenantId,
                    string.IsNullOrWhiteSpace(currentOptions.RequestedByPersonId)
                        ? "recordarr-object-store-worker"
                        : currentOptions.RequestedByPersonId.Trim(),
                    string.IsNullOrWhiteSpace(inventory.Scope) ? "external_object_store_inventory" : inventory.Scope,
                    inventory.RecordId,
                    inventory.MissingFileIds,
                    inventory.CorruptFileIds,
                    checkedFileIds);

                if (HasRemediationEvidence(inventory))
                {
                    store.RemediateStorageReconciliation(
                        tenantId,
                        reconciliation.ReconciliationId,
                        string.IsNullOrWhiteSpace(currentOptions.RequestedByPersonId)
                            ? "recordarr-object-store-worker"
                            : currentOptions.RequestedByPersonId.Trim(),
                        inventory.RestoredFileIds,
                        inventory.AcceptedMissingFileIds,
                        inventory.RecheckedCorruptFileIds,
                        inventory.ReleasedQuarantinedFileIds,
                        inventory.ScannedPendingFileIds);
                }

                logger.LogInformation(
                    "RecordArr object-store reconciliation worker recorded reconciliation {ReconciliationId} for tenant {TenantId}.",
                    reconciliation.ReconciliationId,
                    tenantId);
            }
        }
    }

    private static bool HasRemediationEvidence(RecordArrObjectStoreInventoryResult result) =>
        HasAny(result.RestoredFileIds) ||
        HasAny(result.AcceptedMissingFileIds) ||
        HasAny(result.RecheckedCorruptFileIds) ||
        HasAny(result.ReleasedQuarantinedFileIds) ||
        HasAny(result.ScannedPendingFileIds);

    private static bool HasAny(IReadOnlyList<string>? values) => values is { Count: > 0 };

    private static string[] MergeFileIds(params IReadOnlyList<string>?[] fileIdGroups) =>
        fileIdGroups
            .Where(group => group is not null)
            .SelectMany(group => group!)
            .Where(fileId => !string.IsNullOrWhiteSpace(fileId))
            .Select(fileId => fileId.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
}
