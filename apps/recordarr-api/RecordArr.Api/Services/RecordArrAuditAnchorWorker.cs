using System.Text.Json;
using Microsoft.Extensions.Options;
using RecordArr.Api.Data;
using RecordArr.Api.Models;
using RecordArr.Api.Options;

namespace RecordArr.Api.Services;

public sealed record RecordArrAuditAnchorManifestResult(
    string AuditSealId,
    string AnchorProviderName,
    string AnchorReference,
    DateTimeOffset AnchoredAt,
    string AnchoredSealHash);

public interface IRecordArrAuditAnchorManifestProvider
{
    Task<IReadOnlyList<RecordArrAuditAnchorManifestResult>> GetManifestsAsync(
        string tenantId,
        IReadOnlyList<RecordArrAuditSealResponse> auditSeals,
        CancellationToken cancellationToken);
}

public sealed class ManifestRecordArrAuditAnchorManifestProvider(
    IOptionsMonitor<AuditAnchorWorkerOptions> options,
    ILogger<ManifestRecordArrAuditAnchorManifestProvider> logger) : IRecordArrAuditAnchorManifestProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<RecordArrAuditAnchorManifestResult>> GetManifestsAsync(
        string tenantId,
        IReadOnlyList<RecordArrAuditSealResponse> auditSeals,
        CancellationToken cancellationToken)
    {
        var manifestPath = options.CurrentValue.ManifestPath;
        if (string.IsNullOrWhiteSpace(manifestPath))
        {
            logger.LogWarning("RecordArr audit anchor worker is enabled without a manifest path; no audit anchors will be recorded.");
            return Array.Empty<RecordArrAuditAnchorManifestResult>();
        }

        if (!File.Exists(manifestPath))
        {
            logger.LogWarning("RecordArr audit anchor manifest {ManifestPath} was not found; no audit anchors will be recorded.", manifestPath);
            return Array.Empty<RecordArrAuditAnchorManifestResult>();
        }

        await using var stream = File.OpenRead(manifestPath);
        var manifest = await JsonSerializer.DeserializeAsync<AuditAnchorManifest>(stream, JsonOptions, cancellationToken);
        if (manifest?.Manifests is null || manifest.Manifests.Count == 0)
        {
            return Array.Empty<RecordArrAuditAnchorManifestResult>();
        }

        var anchorableSealIds = auditSeals
            .Where(seal => string.IsNullOrWhiteSpace(seal.AnchorStatus))
            .Select(seal => seal.AuditSealId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return manifest.Manifests
            .Where(row =>
                !string.IsNullOrWhiteSpace(row.AuditSealId) &&
                !string.IsNullOrWhiteSpace(row.AnchorProviderName) &&
                !string.IsNullOrWhiteSpace(row.AnchorReference) &&
                !string.IsNullOrWhiteSpace(row.AnchoredSealHash) &&
                (string.IsNullOrWhiteSpace(row.TenantId) || string.Equals(row.TenantId, tenantId, StringComparison.OrdinalIgnoreCase)))
            .Where(row => anchorableSealIds.Contains(row.AuditSealId.Trim()))
            .Select(row => new RecordArrAuditAnchorManifestResult(
                row.AuditSealId.Trim(),
                row.AnchorProviderName.Trim(),
                row.AnchorReference.Trim(),
                row.AnchoredAt,
                row.AnchoredSealHash.Trim()))
            .ToArray();
    }

    private sealed record AuditAnchorManifest(IReadOnlyList<AuditAnchorManifestRow> Manifests);

    private sealed record AuditAnchorManifestRow(
        string? TenantId,
        string AuditSealId,
        string AnchorProviderName,
        string AnchorReference,
        DateTimeOffset AnchoredAt,
        string AnchoredSealHash);
}

public sealed class RecordArrAuditAnchorWorker(
    IServiceScopeFactory scopeFactory,
    IRecordArrAuditAnchorManifestProvider manifestProvider,
    IOptionsMonitor<AuditAnchorWorkerOptions> options,
    ILogger<RecordArrAuditAnchorWorker> logger) : BackgroundService
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
            var delay = TimeSpan.FromSeconds(Math.Clamp(options.CurrentValue.PollIntervalSeconds, 60, 86400));
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
            logger.LogWarning("RecordArr audit anchor worker is enabled without tenant ids; no audit anchors will be recorded.");
            return;
        }

        foreach (var tenantId in tenantIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var scope = scopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var auditSeals = store.GetAuditSeals(tenantId)
                .Where(seal => string.IsNullOrWhiteSpace(seal.AnchorStatus))
                .ToArray();
            if (auditSeals.Length == 0)
            {
                continue;
            }

            var manifests = await manifestProvider.GetManifestsAsync(tenantId, auditSeals, cancellationToken);
            if (manifests.Count == 0)
            {
                logger.LogInformation(
                    "RecordArr audit anchor worker found {AuditSealCount} unanchored seal(s) for tenant {TenantId}, but no explicit anchor manifests were available.",
                    auditSeals.Length,
                    tenantId);
                continue;
            }

            foreach (var manifest in manifests)
            {
                var seal = store.AnchorAuditSeal(
                    tenantId,
                    manifest.AuditSealId,
                    string.IsNullOrWhiteSpace(currentOptions.RequestedByPersonId)
                        ? "recordarr-audit-anchor-worker"
                        : currentOptions.RequestedByPersonId.Trim(),
                    manifest.AnchorProviderName,
                    manifest.AnchorReference,
                    manifest.AnchoredAt,
                    manifest.AnchoredSealHash);

                logger.LogInformation(
                    "RecordArr audit anchor worker recorded anchor status {AnchorStatus} for seal {AuditSealId} in tenant {TenantId}.",
                    seal.AnchorStatus,
                    seal.AuditSealId,
                    tenantId);
            }
        }
    }
}
