using System.Text.Json;
using Microsoft.Extensions.Options;
using RecordArr.Api.Data;
using RecordArr.Api.Models;
using RecordArr.Api.Options;

namespace RecordArr.Api.Services;

public sealed record RecordArrBackupVerificationManifestResult(
    string BackupProviderName,
    string BackupJobRef,
    string BackupManifestHash,
    string RecoveryPointId,
    DateTimeOffset RecoveryPointCreatedAt,
    int RpoTargetMinutes,
    IReadOnlyList<string> RecordIds,
    IReadOnlyList<string> MissingFileIds,
    IReadOnlyList<string> CorruptFileIds);

public interface IRecordArrBackupVerificationManifestProvider
{
    Task<IReadOnlyList<RecordArrBackupVerificationManifestResult>> GetManifestsAsync(
        string tenantId,
        IReadOnlyList<RecordArrFileResponse> candidateFiles,
        CancellationToken cancellationToken);
}

public sealed class ManifestRecordArrBackupVerificationManifestProvider(
    IOptionsMonitor<BackupVerificationWorkerOptions> options,
    ILogger<ManifestRecordArrBackupVerificationManifestProvider> logger) : IRecordArrBackupVerificationManifestProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<RecordArrBackupVerificationManifestResult>> GetManifestsAsync(
        string tenantId,
        IReadOnlyList<RecordArrFileResponse> candidateFiles,
        CancellationToken cancellationToken)
    {
        var manifestPath = options.CurrentValue.ManifestPath;
        if (string.IsNullOrWhiteSpace(manifestPath))
        {
            logger.LogWarning("RecordArr backup verification worker is enabled without a manifest path; no backup verification will be recorded.");
            return Array.Empty<RecordArrBackupVerificationManifestResult>();
        }

        if (!File.Exists(manifestPath))
        {
            logger.LogWarning("RecordArr backup verification manifest {ManifestPath} was not found; no backup verification will be recorded.", manifestPath);
            return Array.Empty<RecordArrBackupVerificationManifestResult>();
        }

        await using var stream = File.OpenRead(manifestPath);
        var manifest = await JsonSerializer.DeserializeAsync<BackupVerificationManifest>(stream, JsonOptions, cancellationToken);
        if (manifest?.Manifests is null || manifest.Manifests.Count == 0)
        {
            return Array.Empty<RecordArrBackupVerificationManifestResult>();
        }

        var knownFileIds = candidateFiles
            .Select(file => file.FileId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var knownRecordIds = candidateFiles
            .Select(file => file.RecordId)
            .Where(recordId => !string.IsNullOrWhiteSpace(recordId))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return manifest.Manifests
            .Where(row =>
                !string.IsNullOrWhiteSpace(row.BackupProviderName) &&
                !string.IsNullOrWhiteSpace(row.BackupJobRef) &&
                !string.IsNullOrWhiteSpace(row.BackupManifestHash) &&
                !string.IsNullOrWhiteSpace(row.RecoveryPointId) &&
                (string.IsNullOrWhiteSpace(row.TenantId) || string.Equals(row.TenantId, tenantId, StringComparison.OrdinalIgnoreCase)))
            .Select(row => ToResult(row, knownRecordIds, knownFileIds, options.CurrentValue.DefaultRpoTargetMinutes))
            .Where(result => result.RecordIds.Count > 0)
            .ToArray();
    }

    private static RecordArrBackupVerificationManifestResult ToResult(
        BackupVerificationManifestRow row,
        ISet<string> knownRecordIds,
        ISet<string> knownFileIds,
        int defaultRpoTargetMinutes)
    {
        var recordIds = FilterKnown(row.RecordIds, knownRecordIds);
        var rpoTargetMinutes = row.RpoTargetMinutes.GetValueOrDefault(defaultRpoTargetMinutes);

        return new RecordArrBackupVerificationManifestResult(
            row.BackupProviderName.Trim(),
            row.BackupJobRef.Trim(),
            row.BackupManifestHash.Trim(),
            row.RecoveryPointId.Trim(),
            row.RecoveryPointCreatedAt,
            rpoTargetMinutes,
            recordIds,
            FilterKnown(row.MissingFileIds, knownFileIds),
            FilterKnown(row.CorruptFileIds, knownFileIds));
    }

    private static string[] FilterKnown(IReadOnlyList<string>? values, ISet<string> knownValues) =>
        (values ?? Array.Empty<string>())
            .Where(value => !string.IsNullOrWhiteSpace(value) && knownValues.Contains(value.Trim()))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private sealed record BackupVerificationManifest(IReadOnlyList<BackupVerificationManifestRow> Manifests);

    private sealed record BackupVerificationManifestRow(
        string? TenantId,
        string BackupProviderName,
        string BackupJobRef,
        string BackupManifestHash,
        string RecoveryPointId,
        DateTimeOffset RecoveryPointCreatedAt,
        int? RpoTargetMinutes,
        IReadOnlyList<string>? RecordIds,
        IReadOnlyList<string>? MissingFileIds,
        IReadOnlyList<string>? CorruptFileIds);
}

public sealed class RecordArrBackupVerificationWorker(
    IServiceScopeFactory scopeFactory,
    IRecordArrBackupVerificationManifestProvider manifestProvider,
    IOptionsMonitor<BackupVerificationWorkerOptions> options,
    ILogger<RecordArrBackupVerificationWorker> logger) : BackgroundService
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
            logger.LogWarning("RecordArr backup verification worker is enabled without tenant ids; no backup verification will be recorded.");
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

            var manifests = await manifestProvider.GetManifestsAsync(tenantId, candidateFiles, cancellationToken);
            if (manifests.Count == 0)
            {
                logger.LogInformation(
                    "RecordArr backup verification worker found {CandidateFileCount} candidate file(s) for tenant {TenantId}, but no explicit provider backup manifest was available.",
                    candidateFiles.Count,
                    tenantId);
                continue;
            }

            foreach (var manifest in manifests)
            {
                var run = store.RunDisasterRecoveryBackupVerification(
                    tenantId,
                    string.IsNullOrWhiteSpace(currentOptions.RequestedByPersonId)
                        ? "recordarr-backup-verification-worker"
                        : currentOptions.RequestedByPersonId.Trim(),
                    manifest.BackupProviderName,
                    manifest.BackupJobRef,
                    manifest.BackupManifestHash,
                    manifest.RecoveryPointId,
                    manifest.RecoveryPointCreatedAt,
                    manifest.RpoTargetMinutes,
                    manifest.RecordIds,
                    manifest.MissingFileIds,
                    manifest.CorruptFileIds);

                logger.LogInformation(
                    "RecordArr backup verification worker recorded run {DisasterRecoveryRunId} for tenant {TenantId} with status {Status}.",
                    run.DisasterRecoveryRunId,
                    tenantId,
                    run.Status);
            }
        }
    }
}
