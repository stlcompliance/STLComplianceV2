using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Services;

public class LaunchDestinationReconciliationWorkerService(
    NexArrDbContext db,
    LaunchDestinationReconciliationSettingsService settingsService,
    IPlatformAuditService audit)
{
    public const string ProcessLaunchDestinationReconciliationActionScope = "nexarr.launch_destination.reconcile";
    public const string ProcessLaunchAvailabilityReconciliationActionScope = "nexarr.launch_availability.reconcile";
    public const string CompatibilityLegacyEntitlementReconciliationActionScope = "nexarr.entitlements.reconcile";

    public static readonly string[] AcceptedActionScopes =
    [
        ProcessLaunchDestinationReconciliationActionScope,
        ProcessLaunchAvailabilityReconciliationActionScope,
        CompatibilityLegacyEntitlementReconciliationActionScope,
    ];

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000a9");

    public async Task<PendingLaunchDestinationReconciliationResponse> ListPendingAsync(
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadOrDefaultAsync(cancellationToken);
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = LaunchDestinationReconciliationRules.NormalizeBatchSize(batchSize);

        if (!settings.IsEnabled)
        {
            return new PendingLaunchDestinationReconciliationResponse(asOf, normalizedBatchSize, []);
        }

        var items = await LoadPendingDriftAsync(asOf, normalizedBatchSize, cancellationToken);
        return new PendingLaunchDestinationReconciliationResponse(asOf, normalizedBatchSize, items);
    }

    public async Task<ProcessLaunchDestinationReconciliationResponse> ProcessBatchAsync(
        ProcessLaunchDestinationReconciliationRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadOrDefaultAsync(cancellationToken);
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = LaunchDestinationReconciliationRules.NormalizeBatchSize(request.BatchSize);

        if (!settings.IsEnabled)
        {
            return new ProcessLaunchDestinationReconciliationResponse(
                asOf,
                batchSize,
                0,
                0,
                0,
                0,
                [],
                []);
        }

        var driftItems = await LoadPendingDriftAsync(asOf, batchSize, cancellationToken);
        var applied = new List<PendingLaunchDestinationReconciliationItem>();
        var skipped = new List<LaunchDestinationReconciliationActionSkip>();
        var grantedCount = 0;
        var revokedCount = 0;

        foreach (var item in driftItems)
        {
            try
            {
                var action = ResolveAction(item.DriftKind, settings);
                if (action is null)
                {
                    skipped.Add(new LaunchDestinationReconciliationActionSkip(
                        item.TenantId,
                        item.ProductKey,
                        item.DriftKind,
                        "Reconciliation action is disabled for this drift kind."));
                    continue;
                }

                if (action == "grant")
                {
                    await GrantLaunchDestinationRecordAsync(item, cancellationToken);
                    grantedCount++;
                    applied.Add(item);
                }
                else if (action == "revoke")
                {
                    await RevokeLaunchDestinationRecordAsync(item, cancellationToken);
                    revokedCount++;
                    applied.Add(item);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new LaunchDestinationReconciliationActionSkip(
                    item.TenantId,
                    item.ProductKey,
                    item.DriftKind,
                    ex.Message));
            }
        }

        if (driftItems.Count > 0 || applied.Count > 0 || skipped.Count > 0)
        {
            var outcome = applied.Count > 0
                ? "reconciled"
                : skipped.Count > 0
                    ? "skipped"
                    : "none";

            db.LaunchDestinationReconciliationRuns.Add(new LaunchDestinationReconciliationRun
            {
                Id = Guid.NewGuid(),
                Outcome = outcome,
                DriftFoundCount = driftItems.Count,
                GrantedCount = grantedCount,
                RevokedCount = revokedCount,
                SkippedCount = skipped.Count,
                SkipReason = skipped.Count > 0 && applied.Count == 0
                    ? Truncate("One or more launch-destination drifts could not be reconciled.", 512)
                    : null,
                ProcessedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
            });

            await db.SaveChangesAsync(cancellationToken);
        }

        return new ProcessLaunchDestinationReconciliationResponse(
            asOf,
            batchSize,
            driftItems.Count,
            grantedCount,
            revokedCount,
            skipped.Count,
            applied,
            skipped);
    }

    public async Task<LaunchDestinationReconciliationRunsResponse> ListRecentRunsAsync(
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = LaunchDestinationReconciliationRules.NormalizeRunListLimit(limit);
        var rows = await db.LaunchDestinationReconciliationRuns
            .AsNoTracking()
            .OrderByDescending(x => x.ProcessedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(x => new LaunchDestinationReconciliationRunItem(
                x.Id,
                x.Outcome,
                x.DriftFoundCount,
                x.GrantedCount,
                x.RevokedCount,
                x.SkippedCount,
                x.SkipReason,
                x.ProcessedAt))
            .ToList();

        return new LaunchDestinationReconciliationRunsResponse(items);
    }

    private static string? ResolveAction(string driftKind, PlatformLaunchDestinationReconciliationSettings settings)
    {
        var normalized = LaunchDestinationReconciliationRules.NormalizeDriftKind(driftKind);

        if (normalized == LaunchDestinationReconciliationRules.MissingLaunchDestinationDrift
            && settings.AutoGrantFromLicense)
        {
            return "grant";
        }

        if ((normalized == LaunchDestinationReconciliationRules.StaleLaunchDestinationDrift
                || normalized == LaunchDestinationReconciliationRules.SuspendedTenantDrift
                || normalized == LaunchDestinationReconciliationRules.InactiveProductDrift)
            && settings.AutoRevokeStaleLaunchDestinations)
        {
            return "revoke";
        }

        return null;
    }

    private async Task GrantLaunchDestinationRecordAsync(
        PendingLaunchDestinationReconciliationItem item,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var launchDestinationRecord = await db.Entitlements
            .FirstOrDefaultAsync(
                e => e.TenantId == item.TenantId && e.ProductKey == item.ProductKey,
                cancellationToken);

        if (launchDestinationRecord is null)
        {
            launchDestinationRecord = new TenantProductEntitlement
            {
                Id = Guid.NewGuid(),
                TenantId = item.TenantId,
                ProductKey = item.ProductKey,
                Status = EntitlementStatuses.Active,
                GrantedAt = now,
            };
            db.Entitlements.Add(launchDestinationRecord);
        }
        else
        {
            launchDestinationRecord.Status = EntitlementStatuses.Active;
            launchDestinationRecord.GrantedAt = now;
            launchDestinationRecord.RevokedAt = null;
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "launch_destination.reconcile.grant",
            "launch_destination_record",
            launchDestinationRecord.Id.ToString(),
            "Success",
            tenantId: item.TenantId,
            actorUserId: WorkerActorUserId,
            cancellationToken: cancellationToken);
    }

    private async Task RevokeLaunchDestinationRecordAsync(
        PendingLaunchDestinationReconciliationItem item,
        CancellationToken cancellationToken)
    {
        if (item.LaunchDestinationRecordId is not Guid launchDestinationRecordId)
        {
            throw new InvalidOperationException("Launch-destination record id is required to revoke.");
        }

        var launchDestinationRecord = await db.Entitlements
            .FirstOrDefaultAsync(e => e.Id == launchDestinationRecordId, cancellationToken)
            ?? throw new InvalidOperationException("Launch-destination compatibility record was not found.");

        if (launchDestinationRecord.Status == EntitlementStatuses.Revoked)
        {
            return;
        }

        launchDestinationRecord.Status = EntitlementStatuses.Revoked;
        launchDestinationRecord.RevokedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "launch_destination.reconcile.revoke",
            "launch_destination_record",
            launchDestinationRecord.Id.ToString(),
            "Success",
            tenantId: item.TenantId,
            actorUserId: WorkerActorUserId,
            cancellationToken: cancellationToken);
    }

    private async Task<IReadOnlyList<PendingLaunchDestinationReconciliationItem>> LoadPendingDriftAsync(
        DateTimeOffset asOfUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        _ = db;
        _ = asOfUtc;
        _ = batchSize;
        _ = cancellationToken;

        // Legacy license/entitlement reconciliation is retired. Fixed-suite launch
        // availability is derived from active tenant membership, product operational
        // state, and the Compliance Core studio platform-admin gate.
        return await Task.FromResult<IReadOnlyList<PendingLaunchDestinationReconciliationItem>>([]);
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}

public sealed class CompatibilityLegacyEntitlementReconciliationWorkerService(
    LaunchDestinationReconciliationWorkerService inner)
{
    public const string ProcessLaunchDestinationReconciliationActionScope =
        LaunchDestinationReconciliationWorkerService.ProcessLaunchDestinationReconciliationActionScope;
    public const string ProcessLaunchAvailabilityReconciliationActionScope =
        LaunchDestinationReconciliationWorkerService.ProcessLaunchAvailabilityReconciliationActionScope;
    public const string CompatibilityLegacyEntitlementReconciliationActionScope =
        LaunchDestinationReconciliationWorkerService.CompatibilityLegacyEntitlementReconciliationActionScope;

    public static readonly string[] AcceptedActionScopes =
        LaunchDestinationReconciliationWorkerService.AcceptedActionScopes;

    public static readonly Guid WorkerActorUserId =
        LaunchDestinationReconciliationWorkerService.WorkerActorUserId;

    public Task<PendingLaunchDestinationReconciliationResponse> ListPendingAsync(
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default) =>
        inner.ListPendingAsync(asOfUtc, batchSize, cancellationToken);

    public Task<ProcessLaunchDestinationReconciliationResponse> ProcessBatchAsync(
        ProcessLaunchDestinationReconciliationRequest request,
        CancellationToken cancellationToken = default) =>
        inner.ProcessBatchAsync(request, cancellationToken);

    public Task<LaunchDestinationReconciliationRunsResponse> ListRecentRunsAsync(
        int? limit,
        CancellationToken cancellationToken = default) =>
        inner.ListRecentRunsAsync(limit, cancellationToken);
}
