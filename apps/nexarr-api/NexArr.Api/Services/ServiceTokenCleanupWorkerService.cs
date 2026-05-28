using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Services;

public sealed class ServiceTokenCleanupWorkerService(
    NexArrDbContext db,
    ServiceTokenCleanupSettingsService settingsService,
    IPlatformAuditService audit)
{
    public const string ProcessCleanupActionScope = "nexarr.service_tokens.cleanup.purge";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000a8");

    public async Task<PendingServiceTokenCleanupResponse> ListPendingAsync(
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadOrDefaultAsync(cancellationToken);
        if (!settings.IsEnabled)
        {
            var asOfDisabled = asOfUtc ?? DateTimeOffset.UtcNow;
            return new PendingServiceTokenCleanupResponse(
                asOfDisabled,
                ServiceTokenCleanupRules.NormalizeBatchSize(batchSize),
                []);
        }

        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = ServiceTokenCleanupRules.NormalizeBatchSize(batchSize);
        var items = await LoadPendingCandidatesAsync(settings, asOf, normalizedBatchSize, cancellationToken);
        return new PendingServiceTokenCleanupResponse(asOf, normalizedBatchSize, items);
    }

    public async Task<ProcessServiceTokenCleanupResponse> ProcessBatchAsync(
        ProcessServiceTokenCleanupRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadOrDefaultAsync(cancellationToken);
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = ServiceTokenCleanupRules.NormalizeBatchSize(request.BatchSize);

        if (!settings.IsEnabled)
        {
            return new ProcessServiceTokenCleanupResponse(
                asOf,
                batchSize,
                0,
                0,
                0,
                0,
                0,
                [],
                []);
        }

        var candidates = await LoadPendingCandidatesAsync(settings, asOf, batchSize, cancellationToken);
        var purgedTokenIds = new List<Guid>();
        var skipped = new List<ServiceTokenCleanupPurgeSkip>();
        var expiredPurgeCount = 0;
        var revokedPurgeCount = 0;

        foreach (var candidate in candidates)
        {
            try
            {
                var record = await db.ServiceTokens
                    .FirstOrDefaultAsync(x => x.Id == candidate.TokenId, cancellationToken);

                if (record is null)
                {
                    skipped.Add(new ServiceTokenCleanupPurgeSkip(candidate.TokenId, "Service token record was not found."));
                    continue;
                }

                if (!ServiceTokenCleanupRules.IsPurgeCandidate(
                        record.RevokedAt,
                        record.ExpiresAt,
                        asOf,
                        settings.RetentionDaysAfterExpiry,
                        settings.RetentionDaysAfterRevoke))
                {
                    skipped.Add(new ServiceTokenCleanupPurgeSkip(candidate.TokenId, "Service token is no longer eligible for purge."));
                    continue;
                }

                var reason = ServiceTokenCleanupRules.ResolveCleanupReason(
                    record.RevokedAt,
                    record.ExpiresAt,
                    asOf,
                    settings.RetentionDaysAfterExpiry,
                    settings.RetentionDaysAfterRevoke);

                db.ServiceTokens.Remove(record);
                await db.SaveChangesAsync(cancellationToken);

                purgedTokenIds.Add(record.Id);
                if (string.Equals(reason, "revoked", StringComparison.Ordinal))
                {
                    revokedPurgeCount++;
                }
                else
                {
                    expiredPurgeCount++;
                }

                await audit.WriteAsync(
                    "service_token.cleanup.purge",
                    "service_token",
                    record.Id.ToString(),
                    "Success",
                    tenantId: record.TenantId,
                    actorUserId: WorkerActorUserId,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new ServiceTokenCleanupPurgeSkip(candidate.TokenId, ex.Message));
            }
        }

        if (candidates.Count > 0 || purgedTokenIds.Count > 0 || skipped.Count > 0)
        {
            var outcome = purgedTokenIds.Count > 0
                ? "purged"
                : skipped.Count > 0
                    ? "skipped"
                    : "none";

            db.ServiceTokenCleanupRuns.Add(new ServiceTokenCleanupRun
            {
                Id = Guid.NewGuid(),
                Outcome = outcome,
                PurgedCount = purgedTokenIds.Count,
                ExpiredPurgeCount = expiredPurgeCount,
                RevokedPurgeCount = revokedPurgeCount,
                SkippedCount = skipped.Count,
                SkipReason = skipped.Count > 0 && purgedTokenIds.Count == 0
                    ? Truncate("One or more service tokens could not be purged.", 512)
                    : null,
                ProcessedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
            });

            await db.SaveChangesAsync(cancellationToken);
        }

        return new ProcessServiceTokenCleanupResponse(
            asOf,
            batchSize,
            candidates.Count,
            purgedTokenIds.Count,
            expiredPurgeCount,
            revokedPurgeCount,
            skipped.Count,
            purgedTokenIds,
            skipped);
    }

    public async Task<ServiceTokenCleanupRunsResponse> ListRecentRunsAsync(
        int? limit,
        CancellationToken cancellationToken = default)
    {

        var take = ServiceTokenCleanupRules.NormalizeRunListLimit(limit);
        var rows = await db.ServiceTokenCleanupRuns
            .AsNoTracking()
            .OrderByDescending(x => x.ProcessedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(x => new ServiceTokenCleanupRunItem(
                x.Id,
                x.Outcome,
                x.PurgedCount,
                x.ExpiredPurgeCount,
                x.RevokedPurgeCount,
                x.SkippedCount,
                x.SkipReason,
                x.ProcessedAt))
            .ToList();

        return new ServiceTokenCleanupRunsResponse(items);
    }

    private async Task<IReadOnlyList<PendingServiceTokenCleanupItem>> LoadPendingCandidatesAsync(
        PlatformServiceTokenCleanupSettings settings,
        DateTimeOffset asOfUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var expiredCutoff = asOfUtc.AddDays(-settings.RetentionDaysAfterExpiry);
        var revokedCutoff = asOfUtc.AddDays(-settings.RetentionDaysAfterRevoke);

        var records = await db.ServiceTokens
            .AsNoTracking()
            .Include(x => x.ServiceClient)
            .Where(x =>
                (x.RevokedAt != null && x.RevokedAt <= revokedCutoff)
                || (x.RevokedAt == null && x.ExpiresAt <= expiredCutoff))
            .OrderBy(x => x.RevokedAt ?? x.ExpiresAt)
            .ThenBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        return records
            .Select(x => new PendingServiceTokenCleanupItem(
                x.Id,
                x.ServiceClientId,
                x.ServiceClient.ClientKey,
                x.TenantId,
                ServiceTokenCleanupRules.ResolveCleanupReason(
                    x.RevokedAt,
                    x.ExpiresAt,
                    asOfUtc,
                    settings.RetentionDaysAfterExpiry,
                    settings.RetentionDaysAfterRevoke),
                x.RevokedAt ?? x.ExpiresAt))
            .ToList();
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
