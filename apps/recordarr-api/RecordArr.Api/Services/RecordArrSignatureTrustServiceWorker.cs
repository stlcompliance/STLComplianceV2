using System.Text.Json;
using Microsoft.Extensions.Options;
using RecordArr.Api.Data;
using RecordArr.Api.Models;
using RecordArr.Api.Options;

namespace RecordArr.Api.Services;

public sealed record RecordArrSignatureTrustServiceManifestResult(
    string ProviderName,
    string ProviderEnvelopeRef,
    string ProviderCallbackStatus,
    string ProviderCallbackRef,
    string CertificateFingerprintSha256,
    string? TrustTimestampAuthorityRef,
    string? LongTermValidationStatus);

public interface IRecordArrSignatureTrustServiceManifestProvider
{
    Task<IReadOnlyList<RecordArrSignatureTrustServiceManifestResult>> GetManifestsAsync(
        string tenantId,
        IReadOnlyList<RecordArrSignatureTrustServiceJobResponse> submittedJobs,
        CancellationToken cancellationToken);
}

public sealed class ManifestRecordArrSignatureTrustServiceManifestProvider(
    IOptionsMonitor<SignatureTrustServiceWorkerOptions> options,
    ILogger<ManifestRecordArrSignatureTrustServiceManifestProvider> logger) : IRecordArrSignatureTrustServiceManifestProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<RecordArrSignatureTrustServiceManifestResult>> GetManifestsAsync(
        string tenantId,
        IReadOnlyList<RecordArrSignatureTrustServiceJobResponse> submittedJobs,
        CancellationToken cancellationToken)
    {
        var manifestPath = options.CurrentValue.ManifestPath;
        if (string.IsNullOrWhiteSpace(manifestPath))
        {
            logger.LogWarning("RecordArr signature trust-service worker is enabled without a manifest path; no signature trust-service reconciliation will be recorded.");
            return Array.Empty<RecordArrSignatureTrustServiceManifestResult>();
        }

        if (!File.Exists(manifestPath))
        {
            logger.LogWarning("RecordArr signature trust-service manifest {ManifestPath} was not found; no signature trust-service reconciliation will be recorded.", manifestPath);
            return Array.Empty<RecordArrSignatureTrustServiceManifestResult>();
        }

        await using var stream = File.OpenRead(manifestPath);
        var manifest = await JsonSerializer.DeserializeAsync<SignatureTrustServiceManifest>(stream, JsonOptions, cancellationToken);
        if (manifest?.Manifests is null || manifest.Manifests.Count == 0)
        {
            return Array.Empty<RecordArrSignatureTrustServiceManifestResult>();
        }

        var submittedByProviderEnvelope = submittedJobs
            .Where(job => string.Equals(job.Status, "submitted", StringComparison.OrdinalIgnoreCase))
            .GroupBy(job => $"{job.ProviderName}\u001f{job.ProviderEnvelopeRef}", StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        return manifest.Manifests
            .Where(row =>
                !string.IsNullOrWhiteSpace(row.ProviderName) &&
                !string.IsNullOrWhiteSpace(row.ProviderEnvelopeRef) &&
                !string.IsNullOrWhiteSpace(row.ProviderCallbackStatus) &&
                !string.IsNullOrWhiteSpace(row.ProviderCallbackRef) &&
                !string.IsNullOrWhiteSpace(row.CertificateFingerprintSha256) &&
                (string.IsNullOrWhiteSpace(row.TenantId) || string.Equals(row.TenantId, tenantId, StringComparison.OrdinalIgnoreCase)))
            .Where(row => submittedByProviderEnvelope.ContainsKey($"{row.ProviderName.Trim()}\u001f{row.ProviderEnvelopeRef.Trim()}"))
            .Select(row => new RecordArrSignatureTrustServiceManifestResult(
                row.ProviderName.Trim(),
                row.ProviderEnvelopeRef.Trim(),
                row.ProviderCallbackStatus.Trim(),
                row.ProviderCallbackRef.Trim(),
                row.CertificateFingerprintSha256.Trim(),
                string.IsNullOrWhiteSpace(row.TrustTimestampAuthorityRef) ? null : row.TrustTimestampAuthorityRef.Trim(),
                string.IsNullOrWhiteSpace(row.LongTermValidationStatus) ? null : row.LongTermValidationStatus.Trim()))
            .ToArray();
    }

    private sealed record SignatureTrustServiceManifest(IReadOnlyList<SignatureTrustServiceManifestRow> Manifests);

    private sealed record SignatureTrustServiceManifestRow(
        string? TenantId,
        string ProviderName,
        string ProviderEnvelopeRef,
        string ProviderCallbackStatus,
        string ProviderCallbackRef,
        string CertificateFingerprintSha256,
        string? TrustTimestampAuthorityRef,
        string? LongTermValidationStatus);
}

public sealed class RecordArrSignatureTrustServiceWorker(
    IServiceScopeFactory scopeFactory,
    IRecordArrSignatureTrustServiceManifestProvider manifestProvider,
    IOptionsMonitor<SignatureTrustServiceWorkerOptions> options,
    ILogger<RecordArrSignatureTrustServiceWorker> logger) : BackgroundService
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
            logger.LogWarning("RecordArr signature trust-service worker is enabled without tenant ids; no signature trust-service reconciliation will be recorded.");
            return;
        }

        foreach (var tenantId in tenantIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var scope = scopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var submittedJobs = store.GetSignatureTrustServiceJobs(tenantId)
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
                    "RecordArr signature trust-service worker found {SubmittedJobCount} submitted job(s) for tenant {TenantId}, but no explicit provider manifests were available.",
                    submittedJobs.Length,
                    tenantId);
                continue;
            }

            foreach (var manifest in manifests)
            {
                var job = store.ProcessSignatureTrustServiceManifest(
                    tenantId,
                    string.IsNullOrWhiteSpace(currentOptions.RequestedByPersonId)
                        ? "recordarr-signature-trust-worker"
                        : currentOptions.RequestedByPersonId.Trim(),
                    manifest.ProviderName,
                    manifest.ProviderEnvelopeRef,
                    manifest.ProviderCallbackStatus,
                    manifest.ProviderCallbackRef,
                    manifest.CertificateFingerprintSha256,
                    manifest.TrustTimestampAuthorityRef,
                    manifest.LongTermValidationStatus);

                logger.LogInformation(
                    "RecordArr signature trust-service worker reconciled job {TrustServiceJobId} for tenant {TenantId} with status {Status}.",
                    job.TrustServiceJobId,
                    tenantId,
                    job.Status);
            }
        }
    }
}
