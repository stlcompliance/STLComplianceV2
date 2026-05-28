using System.Security.Claims;
using NexArr.Api.Contracts;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Services;

public sealed class PlatformLifecycleOverviewService(
    ServiceTokenCleanupSettingsService serviceTokenCleanupSettings,
    ServiceTokenCleanupWorkerService serviceTokenCleanupWorker,
    EntitlementReconciliationSettingsService entitlementReconciliationSettings,
    EntitlementReconciliationWorkerService entitlementReconciliationWorker,
    TenantLifecycleSettingsService tenantLifecycleSettings,
    TenantLifecycleWorkerService tenantLifecycleWorker,
    PlatformAuthorizationService authorization,
    IPlatformAuditService audit)
{
    public async Task<PlatformLifecycleOverviewResponse> GetOverviewAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();
        var generatedAt = DateTimeOffset.UtcNow;

        var tokenSettings = await serviceTokenCleanupSettings.GetAsync(principal, cancellationToken);
        var tokenPending = await serviceTokenCleanupWorker.ListPendingAsync(null, 50, cancellationToken);
        var tokenRuns = await serviceTokenCleanupWorker.ListRecentRunsAsync(1, cancellationToken);

        var entitlementSettings = await entitlementReconciliationSettings.GetAsync(principal, cancellationToken);
        var entitlementPending = await entitlementReconciliationWorker.ListPendingAsync(null, 50, cancellationToken);
        var entitlementRuns = await entitlementReconciliationWorker.ListRecentRunsAsync(1, cancellationToken);

        var tenantSettings = await tenantLifecycleSettings.GetAsync(principal, cancellationToken);
        var tenantPending = await tenantLifecycleWorker.ListPendingAsync(null, 50, cancellationToken);
        var tenantRuns = await tenantLifecycleWorker.ListRecentRunsAsync(1, cancellationToken);

        var workers = new[]
        {
            BuildServiceTokenCleanupStatus(tokenSettings, tokenPending, tokenRuns),
            BuildEntitlementReconciliationStatus(entitlementSettings, entitlementPending, entitlementRuns),
            BuildTenantLifecycleStatus(tenantSettings, tenantPending, tenantRuns),
        };

        await audit.WriteAsync(
            "platform_lifecycle.overview.read",
            "platform_lifecycle_overview",
            null,
            "Success",
            actorUserId: actorUserId,
            reasonCode: workers.Length.ToString(),
            cancellationToken: cancellationToken);

        return new PlatformLifecycleOverviewResponse(generatedAt, workers);
    }

    private static PlatformLifecycleWorkerStatus BuildServiceTokenCleanupStatus(
        ServiceTokenCleanupSettingsResponse settings,
        PendingServiceTokenCleanupResponse pending,
        ServiceTokenCleanupRunsResponse runs)
    {
        var latest = runs.Items.FirstOrDefault();
        return new PlatformLifecycleWorkerStatus(
            WorkerKey: "service_token_cleanup",
            Label: "Service token cleanup",
            Description: "Purges expired and revoked service tokens after grace periods.",
            IsEnabled: settings.IsEnabled,
            PendingCount: pending.Items.Count,
            LatestRun: latest is null
                ? null
                : new PlatformLifecycleLatestRunSummary(
                    latest.RunId,
                    latest.Outcome,
                    latest.ProcessedAt,
                    latest.PurgedCount,
                    "purged"),
            ServiceTokenScope: ServiceTokenCleanupWorkerService.ProcessCleanupActionScope,
            PlatformSettingsPath: "/api/platform-admin/service-token-cleanup/settings",
            SuiteAdminPath: "/app/platform-admin/service-tokens");
    }

    private static PlatformLifecycleWorkerStatus BuildEntitlementReconciliationStatus(
        EntitlementReconciliationSettingsResponse settings,
        PendingEntitlementReconciliationResponse pending,
        EntitlementReconciliationRunsResponse runs)
    {
        var latest = runs.Items.FirstOrDefault();
        return new PlatformLifecycleWorkerStatus(
            WorkerKey: "entitlement_reconciliation",
            Label: "Entitlement reconciliation",
            Description: "Aligns tenant product entitlements with license records.",
            IsEnabled: settings.IsEnabled,
            PendingCount: pending.Items.Count,
            LatestRun: latest is null
                ? null
                : new PlatformLifecycleLatestRunSummary(
                    latest.RunId,
                    latest.Outcome,
                    latest.ProcessedAt,
                    latest.GrantedCount + latest.RevokedCount,
                    "grant/revoke"),
            ServiceTokenScope: EntitlementReconciliationWorkerService.ProcessReconciliationActionScope,
            PlatformSettingsPath: "/api/platform-admin/entitlement-reconciliation/settings",
            SuiteAdminPath: "/app/platform-admin/entitlements");
    }

    private static PlatformLifecycleWorkerStatus BuildTenantLifecycleStatus(
        TenantLifecycleSettingsResponse settings,
        PendingTenantLifecycleResponse pending,
        TenantLifecycleRunsResponse runs)
    {
        var latest = runs.Items.FirstOrDefault();
        return new PlatformLifecycleWorkerStatus(
            WorkerKey: "tenant_lifecycle",
            Label: "Tenant lifecycle",
            Description: "Suspends or reactivates tenants based on license coverage and policy.",
            IsEnabled: settings.IsEnabled,
            PendingCount: pending.Items.Count,
            LatestRun: latest is null
                ? null
                : new PlatformLifecycleLatestRunSummary(
                    latest.RunId,
                    latest.Outcome,
                    latest.ProcessedAt,
                    latest.SuspendedCount + latest.ReactivatedCount,
                    "suspend/reactivate"),
            ServiceTokenScope: TenantLifecycleWorkerService.ProcessLifecycleActionScope,
            PlatformSettingsPath: "/api/platform-admin/tenant-lifecycle/settings",
            SuiteAdminPath: "/app/platform-admin/tenant-lifecycle");
    }
}
