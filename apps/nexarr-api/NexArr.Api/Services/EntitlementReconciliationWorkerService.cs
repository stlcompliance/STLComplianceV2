using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Services;

public sealed class EntitlementReconciliationWorkerService(
    NexArrDbContext db,
    EntitlementReconciliationSettingsService settingsService,
    IPlatformAuditService audit)
{
    public const string ProcessReconciliationActionScope = "nexarr.entitlements.reconcile";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000a9");

    public async Task<PendingEntitlementReconciliationResponse> ListPendingAsync(
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadOrDefaultAsync(cancellationToken);
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = EntitlementReconciliationRules.NormalizeBatchSize(batchSize);

        if (!settings.IsEnabled)
        {
            return new PendingEntitlementReconciliationResponse(asOf, normalizedBatchSize, []);
        }

        var items = await LoadPendingDriftAsync(asOf, normalizedBatchSize, cancellationToken);
        return new PendingEntitlementReconciliationResponse(asOf, normalizedBatchSize, items);
    }

    public async Task<ProcessEntitlementReconciliationResponse> ProcessBatchAsync(
        ProcessEntitlementReconciliationRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadOrDefaultAsync(cancellationToken);
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = EntitlementReconciliationRules.NormalizeBatchSize(request.BatchSize);

        if (!settings.IsEnabled)
        {
            return new ProcessEntitlementReconciliationResponse(
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
        var applied = new List<PendingEntitlementReconciliationItem>();
        var skipped = new List<EntitlementReconciliationActionSkip>();
        var grantedCount = 0;
        var revokedCount = 0;

        foreach (var item in driftItems)
        {
            try
            {
                var action = ResolveAction(item.DriftKind, settings);
                if (action is null)
                {
                    skipped.Add(new EntitlementReconciliationActionSkip(
                        item.TenantId,
                        item.ProductKey,
                        item.DriftKind,
                        "Reconciliation action is disabled for this drift kind."));
                    continue;
                }

                if (action == "grant")
                {
                    await GrantEntitlementAsync(item, cancellationToken);
                    grantedCount++;
                    applied.Add(item);
                }
                else if (action == "revoke")
                {
                    await RevokeEntitlementAsync(item, cancellationToken);
                    revokedCount++;
                    applied.Add(item);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new EntitlementReconciliationActionSkip(
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

            db.EntitlementReconciliationRuns.Add(new EntitlementReconciliationRun
            {
                Id = Guid.NewGuid(),
                Outcome = outcome,
                DriftFoundCount = driftItems.Count,
                GrantedCount = grantedCount,
                RevokedCount = revokedCount,
                SkippedCount = skipped.Count,
                SkipReason = skipped.Count > 0 && applied.Count == 0
                    ? Truncate("One or more entitlement drifts could not be reconciled.", 512)
                    : null,
                ProcessedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
            });

            await db.SaveChangesAsync(cancellationToken);
        }

        return new ProcessEntitlementReconciliationResponse(
            asOf,
            batchSize,
            driftItems.Count,
            grantedCount,
            revokedCount,
            skipped.Count,
            applied,
            skipped);
    }

    public async Task<EntitlementReconciliationRunsResponse> ListRecentRunsAsync(
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = EntitlementReconciliationRules.NormalizeRunListLimit(limit);
        var rows = await db.EntitlementReconciliationRuns
            .AsNoTracking()
            .OrderByDescending(x => x.ProcessedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(x => new EntitlementReconciliationRunItem(
                x.Id,
                x.Outcome,
                x.DriftFoundCount,
                x.GrantedCount,
                x.RevokedCount,
                x.SkippedCount,
                x.SkipReason,
                x.ProcessedAt))
            .ToList();

        return new EntitlementReconciliationRunsResponse(items);
    }

    private static string? ResolveAction(string driftKind, PlatformEntitlementReconciliationSettings settings) =>
        driftKind switch
        {
            "missing_entitlement" when settings.AutoGrantFromLicense => "grant",
            "stale_entitlement" or "suspended_tenant" or "inactive_product"
                when settings.AutoRevokeStaleEntitlements => "revoke",
            _ => null,
        };

    private async Task GrantEntitlementAsync(
        PendingEntitlementReconciliationItem item,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var entitlement = await db.Entitlements
            .FirstOrDefaultAsync(
                e => e.TenantId == item.TenantId && e.ProductKey == item.ProductKey,
                cancellationToken);

        if (entitlement is null)
        {
            entitlement = new TenantProductEntitlement
            {
                Id = Guid.NewGuid(),
                TenantId = item.TenantId,
                ProductKey = item.ProductKey,
                Status = EntitlementStatuses.Active,
                GrantedAt = now,
            };
            db.Entitlements.Add(entitlement);
        }
        else
        {
            entitlement.Status = EntitlementStatuses.Active;
            entitlement.GrantedAt = now;
            entitlement.RevokedAt = null;
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "entitlement.reconcile.grant",
            "entitlement",
            entitlement.Id.ToString(),
            "Success",
            tenantId: item.TenantId,
            actorUserId: WorkerActorUserId,
            cancellationToken: cancellationToken);
    }

    private async Task RevokeEntitlementAsync(
        PendingEntitlementReconciliationItem item,
        CancellationToken cancellationToken)
    {
        if (item.EntitlementId is not Guid entitlementId)
        {
            throw new InvalidOperationException("Entitlement id is required to revoke.");
        }

        var entitlement = await db.Entitlements
            .FirstOrDefaultAsync(e => e.Id == entitlementId, cancellationToken)
            ?? throw new InvalidOperationException("Entitlement record was not found.");

        if (entitlement.Status == EntitlementStatuses.Revoked)
        {
            return;
        }

        entitlement.Status = EntitlementStatuses.Revoked;
        entitlement.RevokedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "entitlement.reconcile.revoke",
            "entitlement",
            entitlement.Id.ToString(),
            "Success",
            tenantId: item.TenantId,
            actorUserId: WorkerActorUserId,
            cancellationToken: cancellationToken);
    }

    private async Task<IReadOnlyList<PendingEntitlementReconciliationItem>> LoadPendingDriftAsync(
        DateTimeOffset asOfUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var tenants = await db.Tenants.AsNoTracking().ToListAsync(cancellationToken);
        var products = await db.ProductCatalog.AsNoTracking().ToListAsync(cancellationToken);
        var entitlements = await db.Entitlements.AsNoTracking().ToListAsync(cancellationToken);
        var licenses = await db.TenantProductLicenses.AsNoTracking().ToListAsync(cancellationToken);

        var drift = new List<PendingEntitlementReconciliationItem>();

        foreach (var tenant in tenants)
        {
            var tenantActive = tenant.Status == TenantStatuses.Active;

            foreach (var product in products)
            {
                var entitlement = entitlements.FirstOrDefault(
                    e => e.TenantId == tenant.Id && e.ProductKey == product.ProductKey);
                var license = licenses.FirstOrDefault(
                    l => l.TenantId == tenant.Id && l.ProductKey == product.ProductKey);

                var entitlementActive = entitlement?.Status == EntitlementStatuses.Active;
                var licenseValid = license is not null
                    && EntitlementReconciliationRules.IsLicenseCurrentlyValid(
                        license.Status,
                        license.ValidFrom,
                        license.ValidTo,
                        asOfUtc);

                var driftKind = EntitlementReconciliationRules.ResolveDriftKind(
                    tenantActive,
                    product.IsActive,
                    entitlementActive,
                    licenseValid);

                if (driftKind == "none")
                {
                    continue;
                }

                drift.Add(new PendingEntitlementReconciliationItem(
                    tenant.Id,
                    tenant.DisplayName,
                    product.ProductKey,
                    product.DisplayName,
                    driftKind,
                    entitlementActive,
                    licenseValid,
                    entitlement?.Id,
                    license?.Id));
            }
        }

        return drift
            .OrderBy(x => x.TenantDisplayName)
            .ThenBy(x => x.ProductKey)
            .Take(batchSize)
            .ToList();
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
