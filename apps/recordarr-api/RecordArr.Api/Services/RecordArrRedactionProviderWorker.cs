using System.Text.Json;
using Microsoft.Extensions.Options;
using RecordArr.Api.Data;
using RecordArr.Api.Models;
using RecordArr.Api.Options;

namespace RecordArr.Api.Services;

public sealed record RecordArrRedactionProviderManifestResult(
    string ProviderName,
    string ProviderJobRef,
    string ProviderCallbackStatus,
    string ProviderCallbackRef,
    string RedactionPackageHash);

public interface IRecordArrRedactionProviderManifestProvider
{
    Task<IReadOnlyList<RecordArrRedactionProviderManifestResult>> GetManifestsAsync(
        string tenantId,
        IReadOnlyList<RecordArrRedactionProviderJobResponse> submittedJobs,
        CancellationToken cancellationToken);
}

public sealed class ManifestRecordArrRedactionProviderManifestProvider(
    IOptionsMonitor<RedactionProviderWorkerOptions> options,
    ILogger<ManifestRecordArrRedactionProviderManifestProvider> logger) : IRecordArrRedactionProviderManifestProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<RecordArrRedactionProviderManifestResult>> GetManifestsAsync(
        string tenantId,
        IReadOnlyList<RecordArrRedactionProviderJobResponse> submittedJobs,
        CancellationToken cancellationToken)
    {
        var manifestPath = options.CurrentValue.ManifestPath;
        if (string.IsNullOrWhiteSpace(manifestPath))
        {
            logger.LogWarning("RecordArr redaction provider worker is enabled without a manifest path; no redaction provider reconciliation will be recorded.");
            return Array.Empty<RecordArrRedactionProviderManifestResult>();
        }

        if (!File.Exists(manifestPath))
        {
            logger.LogWarning("RecordArr redaction provider manifest {ManifestPath} was not found; no redaction provider reconciliation will be recorded.", manifestPath);
            return Array.Empty<RecordArrRedactionProviderManifestResult>();
        }

        await using var stream = File.OpenRead(manifestPath);
        var manifest = await JsonSerializer.DeserializeAsync<RedactionProviderManifest>(stream, JsonOptions, cancellationToken);
        if (manifest?.Manifests is null || manifest.Manifests.Count == 0)
        {
            return Array.Empty<RecordArrRedactionProviderManifestResult>();
        }

        var submittedByProviderJob = submittedJobs
            .Where(job => string.Equals(job.Status, "submitted", StringComparison.OrdinalIgnoreCase))
            .GroupBy(job => $"{job.ProviderName}\u001f{job.ProviderJobRef}", StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        return manifest.Manifests
            .Where(row =>
                !string.IsNullOrWhiteSpace(row.ProviderName) &&
                !string.IsNullOrWhiteSpace(row.ProviderJobRef) &&
                !string.IsNullOrWhiteSpace(row.ProviderCallbackStatus) &&
                !string.IsNullOrWhiteSpace(row.ProviderCallbackRef) &&
                !string.IsNullOrWhiteSpace(row.RedactionPackageHash) &&
                (string.IsNullOrWhiteSpace(row.TenantId) || string.Equals(row.TenantId, tenantId, StringComparison.OrdinalIgnoreCase)))
            .Where(row => submittedByProviderJob.ContainsKey($"{row.ProviderName.Trim()}\u001f{row.ProviderJobRef.Trim()}"))
            .Select(row => new RecordArrRedactionProviderManifestResult(
                row.ProviderName.Trim(),
                row.ProviderJobRef.Trim(),
                row.ProviderCallbackStatus.Trim(),
                row.ProviderCallbackRef.Trim(),
                row.RedactionPackageHash.Trim()))
            .ToArray();
    }

    private sealed record RedactionProviderManifest(IReadOnlyList<RedactionProviderManifestRow> Manifests);

    private sealed record RedactionProviderManifestRow(
        string? TenantId,
        string ProviderName,
        string ProviderJobRef,
        string ProviderCallbackStatus,
        string ProviderCallbackRef,
        string RedactionPackageHash);
}

public sealed class RecordArrRedactionProviderWorker(
    IServiceScopeFactory scopeFactory,
    IRecordArrRedactionProviderManifestProvider manifestProvider,
    IOptionsMonitor<RedactionProviderWorkerOptions> options,
    ILogger<RecordArrRedactionProviderWorker> logger) : BackgroundService
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
            logger.LogWarning("RecordArr redaction provider worker is enabled without tenant ids; no redaction provider reconciliation will be recorded.");
            return;
        }

        foreach (var tenantId in tenantIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var scope = scopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var submittedJobs = store.GetRedactionProviderJobs(tenantId)
                .Where(job => string.Equals(job.Status, "submitted", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (submittedJobs.Length == 0)
            {
                continue;
            }

            var manifests = await manifestProvider.GetManifestsAsync(tenantId, submittedJobs, cancellationToken);
            if (manifests.Count == 0)
            {
                logger.LogInformation(
                    "RecordArr redaction provider worker found {SubmittedJobCount} submitted job(s) for tenant {TenantId}, but no explicit provider manifests were available.",
                    submittedJobs.Length,
                    tenantId);
                continue;
            }

            foreach (var manifest in manifests)
            {
                var job = store.ProcessRedactionProviderJobManifest(
                    tenantId,
                    string.IsNullOrWhiteSpace(currentOptions.RequestedByPersonId)
                        ? "recordarr-redaction-provider-worker"
                        : currentOptions.RequestedByPersonId.Trim(),
                    manifest.ProviderName,
                    manifest.ProviderJobRef,
                    manifest.ProviderCallbackStatus,
                    manifest.ProviderCallbackRef,
                    manifest.RedactionPackageHash);

                logger.LogInformation(
                    "RecordArr redaction provider worker reconciled job {ProviderJobId} for tenant {TenantId} with status {Status}.",
                    job.ProviderJobId,
                    tenantId,
                    job.Status);
            }
        }
    }
}
