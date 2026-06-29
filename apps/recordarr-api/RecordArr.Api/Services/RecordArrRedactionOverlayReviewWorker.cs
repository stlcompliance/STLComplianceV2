using System.Text.Json;
using Microsoft.Extensions.Options;
using RecordArr.Api.Data;
using RecordArr.Api.Models;
using RecordArr.Api.Options;

namespace RecordArr.Api.Services;

public sealed record RecordArrRedactionOverlayReviewManifestResult(
    string RedactionId,
    string RedactionPackageHash,
    string OverlayReviewStatus,
    IReadOnlyList<string> OverlayEvidenceRefs,
    IReadOnlyList<string> OverlayIssueRefs);

public interface IRecordArrRedactionOverlayReviewManifestProvider
{
    Task<IReadOnlyList<RecordArrRedactionOverlayReviewManifestResult>> GetManifestsAsync(
        string tenantId,
        IReadOnlyList<RecordArrRedactionResponse> redactions,
        CancellationToken cancellationToken);
}

public sealed class ManifestRecordArrRedactionOverlayReviewManifestProvider(
    IOptionsMonitor<RedactionOverlayReviewWorkerOptions> options,
    ILogger<ManifestRecordArrRedactionOverlayReviewManifestProvider> logger) : IRecordArrRedactionOverlayReviewManifestProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<RecordArrRedactionOverlayReviewManifestResult>> GetManifestsAsync(
        string tenantId,
        IReadOnlyList<RecordArrRedactionResponse> redactions,
        CancellationToken cancellationToken)
    {
        var manifestPath = options.CurrentValue.ManifestPath;
        if (string.IsNullOrWhiteSpace(manifestPath))
        {
            logger.LogWarning("RecordArr redaction overlay review worker is enabled without a manifest path; no overlay review will be recorded.");
            return Array.Empty<RecordArrRedactionOverlayReviewManifestResult>();
        }

        if (!File.Exists(manifestPath))
        {
            logger.LogWarning("RecordArr redaction overlay review manifest {ManifestPath} was not found; no overlay review will be recorded.", manifestPath);
            return Array.Empty<RecordArrRedactionOverlayReviewManifestResult>();
        }

        await using var stream = File.OpenRead(manifestPath);
        var manifest = await JsonSerializer.DeserializeAsync<RedactionOverlayReviewManifest>(stream, JsonOptions, cancellationToken);
        if (manifest?.Manifests is null || manifest.Manifests.Count == 0)
        {
            return Array.Empty<RecordArrRedactionOverlayReviewManifestResult>();
        }

        var reviewableById = redactions
            .Where(redaction =>
                !string.IsNullOrWhiteSpace(redaction.RedactionPackageHash) &&
                string.IsNullOrWhiteSpace(redaction.OverlayReviewStatus))
            .ToDictionary(redaction => redaction.RedactionId, StringComparer.OrdinalIgnoreCase);

        return manifest.Manifests
            .Where(row =>
                !string.IsNullOrWhiteSpace(row.RedactionId) &&
                !string.IsNullOrWhiteSpace(row.RedactionPackageHash) &&
                !string.IsNullOrWhiteSpace(row.OverlayReviewStatus) &&
                row.OverlayEvidenceRefs is { Count: > 0 } &&
                (string.IsNullOrWhiteSpace(row.TenantId) || string.Equals(row.TenantId, tenantId, StringComparison.OrdinalIgnoreCase)))
            .Where(row =>
                reviewableById.TryGetValue(row.RedactionId.Trim(), out var redaction) &&
                string.Equals(redaction.RedactionPackageHash, row.RedactionPackageHash.Trim(), StringComparison.OrdinalIgnoreCase))
            .Select(row => new RecordArrRedactionOverlayReviewManifestResult(
                row.RedactionId.Trim(),
                row.RedactionPackageHash.Trim().ToLowerInvariant(),
                row.OverlayReviewStatus.Trim(),
                NormalizeRefs(row.OverlayEvidenceRefs),
                NormalizeRefs(row.OverlayIssueRefs)))
            .ToArray();
    }

    private static string[] NormalizeRefs(IReadOnlyList<string>? refs)
        => refs?
            .Select(refValue => refValue?.Trim())
            .Where(refValue => !string.IsNullOrWhiteSpace(refValue))
            .Select(refValue => refValue!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(refValue => refValue, StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];

    private sealed record RedactionOverlayReviewManifest(IReadOnlyList<RedactionOverlayReviewManifestRow> Manifests);

    private sealed record RedactionOverlayReviewManifestRow(
        string? TenantId,
        string RedactionId,
        string RedactionPackageHash,
        string OverlayReviewStatus,
        IReadOnlyList<string>? OverlayEvidenceRefs,
        IReadOnlyList<string>? OverlayIssueRefs);
}

public sealed class RecordArrRedactionOverlayReviewWorker(
    IServiceScopeFactory scopeFactory,
    IRecordArrRedactionOverlayReviewManifestProvider manifestProvider,
    IOptionsMonitor<RedactionOverlayReviewWorkerOptions> options,
    ILogger<RecordArrRedactionOverlayReviewWorker> logger) : BackgroundService
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
            logger.LogWarning("RecordArr redaction overlay review worker is enabled without tenant ids; no overlay review will be recorded.");
            return;
        }

        foreach (var tenantId in tenantIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var scope = scopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var redactions = store.GetRedactions(tenantId)
                .Where(redaction =>
                    !string.IsNullOrWhiteSpace(redaction.RedactionPackageHash) &&
                    string.IsNullOrWhiteSpace(redaction.OverlayReviewStatus))
                .ToArray();
            if (redactions.Length == 0)
            {
                continue;
            }

            var manifests = await manifestProvider.GetManifestsAsync(tenantId, redactions, cancellationToken);
            if (manifests.Count == 0)
            {
                logger.LogInformation(
                    "RecordArr redaction overlay review worker found {RedactionCount} reviewable redaction(s) for tenant {TenantId}, but no explicit rendered overlay manifests were available.",
                    redactions.Length,
                    tenantId);
                continue;
            }

            foreach (var manifest in manifests)
            {
                var review = store.ReviewRedactionOverlay(
                    tenantId,
                    manifest.RedactionId,
                    string.IsNullOrWhiteSpace(currentOptions.RequestedByPersonId)
                        ? "recordarr-redaction-overlay-worker"
                        : currentOptions.RequestedByPersonId.Trim(),
                    manifest.OverlayReviewStatus,
                    manifest.OverlayEvidenceRefs,
                    manifest.OverlayIssueRefs);

                logger.LogInformation(
                    "RecordArr redaction overlay review worker recorded overlay review for redaction {RedactionId} in tenant {TenantId} with status {Status}.",
                    review.RedactionId,
                    tenantId,
                    review.OverlayReviewStatus);
            }
        }
    }
}
