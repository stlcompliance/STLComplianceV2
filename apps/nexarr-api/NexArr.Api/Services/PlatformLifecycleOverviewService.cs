using System.Security.Claims;
using NexArr.Api.Contracts;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Services;

public sealed class PlatformLifecycleOverviewService(
    ServiceTokenCleanupSettingsService serviceTokenCleanupSettings,
    ServiceTokenCleanupWorkerService serviceTokenCleanupWorker,
    LaunchDestinationReconciliationSettingsService launchDestinationReconciliationSettings,
    LaunchDestinationReconciliationWorkerService launchDestinationReconciliationWorker,
    TenantLifecycleSettingsService tenantLifecycleSettings,
    TenantLifecycleWorkerService tenantLifecycleWorker,
    PlatformOutboxPublisherSettingsService platformOutboxPublisherSettings,
    PlatformOutboxPublisherWorkerService platformOutboxPublisherWorker,
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

        var launchDestinationSettings = await launchDestinationReconciliationSettings.GetAsync(principal, cancellationToken);
        var launchDestinationPending = await launchDestinationReconciliationWorker.ListPendingAsync(null, 50, cancellationToken);
        var launchDestinationRuns = await launchDestinationReconciliationWorker.ListRecentRunsAsync(1, cancellationToken);

        var tenantSettings = await tenantLifecycleSettings.GetAsync(principal, cancellationToken);
        var tenantPending = await tenantLifecycleWorker.ListPendingAsync(null, 50, cancellationToken);
        var tenantRuns = await tenantLifecycleWorker.ListRecentRunsAsync(1, cancellationToken);

        var outboxSettings = await platformOutboxPublisherSettings.GetAsync(principal, cancellationToken);
        var outboxPending = await platformOutboxPublisherWorker.ListPendingAsync(null, 50, cancellationToken);
        var outboxRuns = await platformOutboxPublisherWorker.ListRecentRunsAsync(1, cancellationToken);

        var workers = new[]
        {
            BuildServiceTokenCleanupStatus(tokenSettings, tokenPending, tokenRuns),
            BuildLaunchDestinationReconciliationStatus(launchDestinationSettings, launchDestinationPending, launchDestinationRuns),
            BuildTenantLifecycleStatus(tenantSettings, tenantPending, tenantRuns),
            BuildPlatformOutboxPublisherStatus(outboxSettings, outboxPending, outboxRuns),
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

    private static PlatformLifecycleWorkerStatus BuildLaunchDestinationReconciliationStatus(
        LaunchDestinationReconciliationSettingsResponse settings,
        PendingLaunchDestinationReconciliationResponse pending,
        LaunchDestinationReconciliationRunsResponse runs)
    {
        var latest = runs.Items.FirstOrDefault();
        return new PlatformLifecycleWorkerStatus(
            WorkerKey: "launch_destination_reconciliation",
            Label: "Launch destination reconciliation",
            Description: "Maintains compatibility-era launch-destination snapshots for audit and support workflows.",
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
            ServiceTokenScope: LaunchDestinationReconciliationWorkerService.ProcessLaunchDestinationReconciliationActionScope,
            PlatformSettingsPath: "/api/platform-admin/launch-destination-reconciliation/settings",
            SuiteAdminPath: "/app/platform-admin/lifecycle");
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
            Description: "Retains audit visibility for legacy tenant lifecycle automation settings; license-based suspension is retired.",
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

    private static PlatformLifecycleWorkerStatus BuildPlatformOutboxPublisherStatus(
        PlatformOutboxPublisherSettingsResponse settings,
        PendingPlatformOutboxPublisherResponse pending,
        PlatformOutboxPublisherRunsResponse runs)
    {
        var latest = runs.Items.FirstOrDefault();
        return new PlatformLifecycleWorkerStatus(
            WorkerKey: "platform_outbox_publisher",
            Label: "Platform event outbox",
            Description: "Publishes tenant, launch-destination, and identity integration events for downstream product mirrors.",
            IsEnabled: settings.IsEnabled,
            PendingCount: pending.Items.Count,
            LatestRun: latest is null
                ? null
                : new PlatformLifecycleLatestRunSummary(
                    latest.RunId,
                    latest.Outcome,
                    latest.ProcessedAt,
                    latest.PublishedCount,
                    "published"),
            ServiceTokenScope: PlatformOutboxPublisherWorkerService.ProcessPublishActionScope,
            PlatformSettingsPath: "/api/platform-admin/platform-outbox/settings",
            SuiteAdminPath: "/app/platform-admin/platform-outbox");
    }
}
