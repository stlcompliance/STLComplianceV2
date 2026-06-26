using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Services;

public sealed class TenantLifecycleWorkerService(
    NexArrDbContext db,
    TenantLifecycleSettingsService settingsService,
    IPlatformAuditService audit)
{
    public const string ProcessLifecycleActionScope = "nexarr.tenants.lifecycle.process";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-00000000000a");

    public async Task<PendingTenantLifecycleResponse> ListPendingAsync(
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadOrDefaultAsync(cancellationToken);
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = TenantLifecycleRules.NormalizeBatchSize(batchSize);

        if (!settings.IsEnabled)
        {
            return new PendingTenantLifecycleResponse(asOf, normalizedBatchSize, []);
        }

        var items = await LoadPendingActionsAsync(settings, asOf, normalizedBatchSize, cancellationToken);
        return new PendingTenantLifecycleResponse(asOf, normalizedBatchSize, items);
    }

    public async Task<ProcessTenantLifecycleResponse> ProcessBatchAsync(
        ProcessTenantLifecycleRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadOrDefaultAsync(cancellationToken);
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = TenantLifecycleRules.NormalizeBatchSize(request.BatchSize);

        if (!settings.IsEnabled)
        {
            return new ProcessTenantLifecycleResponse(
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

        var pending = await LoadPendingActionsAsync(settings, asOf, batchSize, cancellationToken);
        var applied = new List<PendingTenantLifecycleItem>();
        var skipped = new List<TenantLifecycleActionSkip>();
        var suspendedCount = 0;
        var reactivatedCount = 0;
        var sessionsRevokedCount = 0;

        foreach (var item in pending)
        {
            try
            {
                var action = ResolveAction(item.ActionKind, settings);
                if (action is null)
                {
                    skipped.Add(new TenantLifecycleActionSkip(
                        item.TenantId,
                        item.ActionKind,
                        "Lifecycle action is disabled for this tenant state."));
                    continue;
                }

                if (action == "suspend")
                {
                    var revoked = await SuspendTenantAsync(item, settings, cancellationToken);
                    suspendedCount++;
                    sessionsRevokedCount += revoked;
                    applied.Add(item);
                }
                else if (action == "reactivate")
                {
                    await ReactivateTenantAsync(item, cancellationToken);
                    reactivatedCount++;
                    applied.Add(item);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new TenantLifecycleActionSkip(
                    item.TenantId,
                    item.ActionKind,
                    ex.Message));
            }
        }

        if (pending.Count > 0 || applied.Count > 0 || skipped.Count > 0)
        {
            var outcome = applied.Count > 0
                ? "processed"
                : skipped.Count > 0
                    ? "skipped"
                    : "none";

            db.TenantLifecycleRuns.Add(new TenantLifecycleRun
            {
                Id = Guid.NewGuid(),
                Outcome = outcome,
                PendingCount = pending.Count,
                SuspendedCount = suspendedCount,
                ReactivatedCount = reactivatedCount,
                SessionsRevokedCount = sessionsRevokedCount,
                SkippedCount = skipped.Count,
                SkipReason = skipped.Count > 0 && applied.Count == 0
                    ? Truncate("One or more tenant lifecycle actions could not be applied.", 512)
                    : null,
                ProcessedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
            });

            await db.SaveChangesAsync(cancellationToken);
        }

        return new ProcessTenantLifecycleResponse(
            asOf,
            batchSize,
            pending.Count,
            suspendedCount,
            reactivatedCount,
            sessionsRevokedCount,
            skipped.Count,
            applied,
            skipped);
    }

    public async Task<TenantLifecycleRunsResponse> ListRecentRunsAsync(
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = TenantLifecycleRules.NormalizeRunListLimit(limit);
        var rows = await db.TenantLifecycleRuns
            .AsNoTracking()
            .OrderByDescending(x => x.ProcessedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(x => new TenantLifecycleRunItem(
                x.Id,
                x.Outcome,
                x.PendingCount,
                x.SuspendedCount,
                x.ReactivatedCount,
                x.SessionsRevokedCount,
                x.SkippedCount,
                x.SkipReason,
                x.ProcessedAt))
            .ToList();

        return new TenantLifecycleRunsResponse(items);
    }

    private static string? ResolveAction(string actionKind, PlatformTenantLifecycleSettings settings) =>
        actionKind switch
        {
            "suspend" when settings.AutoSuspendWhenNoValidLicense => "suspend",
            "reactivate" when settings.AutoReactivateWhenValidLicense => "reactivate",
            _ => null,
        };

    private async Task<int> SuspendTenantAsync(
        PendingTenantLifecycleItem item,
        PlatformTenantLifecycleSettings settings,
        CancellationToken cancellationToken)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == item.TenantId, cancellationToken)
            ?? throw new InvalidOperationException("Tenant record was not found.");

        if (tenant.Status == TenantStatuses.Suspended)
        {
            return 0;
        }

        tenant.Status = TenantStatuses.Suspended;
        tenant.ModifiedAt = DateTimeOffset.UtcNow;

        var sessionsRevoked = 0;
        if (settings.RevokeSessionsOnSuspend)
        {
            sessionsRevoked = await RevokeActiveSessionsAsync(item.TenantId, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "tenant.lifecycle.suspend",
            "tenant",
            tenant.Id.ToString(),
            "Success",
            tenantId: tenant.Id,
            actorUserId: WorkerActorUserId,
            cancellationToken: cancellationToken);

        return sessionsRevoked;
    }

    private async Task ReactivateTenantAsync(
        PendingTenantLifecycleItem item,
        CancellationToken cancellationToken)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == item.TenantId, cancellationToken)
            ?? throw new InvalidOperationException("Tenant record was not found.");

        if (tenant.Status == TenantStatuses.Active)
        {
            return;
        }

        tenant.Status = TenantStatuses.Active;
        tenant.ModifiedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "tenant.lifecycle.reactivate",
            "tenant",
            tenant.Id.ToString(),
            "Success",
            tenantId: tenant.Id,
            actorUserId: WorkerActorUserId,
            cancellationToken: cancellationToken);
    }

    private async Task<int> RevokeActiveSessionsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var sessions = await db.UserSessions
            .Where(s => s.ActiveTenantId == tenantId && s.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            session.RevokedAt = now;
        }

        if (sessions.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);

            foreach (var session in sessions)
            {
                await audit.WriteAsync(
                    "tenant.lifecycle.session_revoke",
                    "session",
                    session.Id.ToString(),
                    "Success",
                    tenantId: tenantId,
                    actorUserId: WorkerActorUserId,
                    cancellationToken: cancellationToken);
            }
        }

        return sessions.Count;
    }

    private async Task<IReadOnlyList<PendingTenantLifecycleItem>> LoadPendingActionsAsync(
        PlatformTenantLifecycleSettings settings,
        DateTimeOffset asOfUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        _ = settings;
        _ = asOfUtc;
        _ = batchSize;
        _ = cancellationToken;

        // Product-license coverage no longer drives tenant suspension or reactivation.
        // The fixed-suite launch model keeps ordinary product availability constant for
        // active tenant members, so this worker remains as a compatibility shell only.
        return [];
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
