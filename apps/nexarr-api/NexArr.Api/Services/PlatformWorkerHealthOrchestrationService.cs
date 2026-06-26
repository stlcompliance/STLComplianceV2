using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class PlatformWorkerHealthOrchestrationService(
    NexArrDbContext db,
    PlatformAuthorizationService authorization,
    PlatformHealthService platformHealth,
    PlatformLifecycleOverviewService lifecycleOverview,
    ServiceTokenCleanupSettingsService serviceTokenCleanupSettings,
    ServiceTokenCleanupWorkerService serviceTokenCleanupWorker,
    LaunchDestinationReconciliationSettingsService launchDestinationReconciliationSettings,
    LaunchDestinationReconciliationWorkerService launchDestinationReconciliationWorker,
    TenantLifecycleSettingsService tenantLifecycleSettings,
    TenantLifecycleWorkerService tenantLifecycleWorker,
    PlatformOutboxPublisherSettingsService platformOutboxPublisherSettings,
    PlatformOutboxPublisherWorkerService platformOutboxPublisherWorker,
    IPlatformAuditService audit)
{
    public async Task<PlatformWorkerHealthOrchestrationStatusResponse> GetStatusAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();
        var now = DateTimeOffset.UtcNow;
        var expiringThreshold = now.AddHours(24);

        var health = await platformHealth.GetAggregateHealthAsync(cancellationToken);

        var activeCount = await db.ServiceTokens.CountAsync(
            t => t.RevokedAt == null && t.ExpiresAt > now,
            cancellationToken);
        var expiringCount = await db.ServiceTokens.CountAsync(
            t => t.RevokedAt == null && t.ExpiresAt > now && t.ExpiresAt <= expiringThreshold,
            cancellationToken);
        var expiredRetained = await db.ServiceTokens.CountAsync(
            t => t.RevokedAt == null && t.ExpiresAt <= now,
            cancellationToken);
        var revokedRetained = await db.ServiceTokens.CountAsync(
            t => t.RevokedAt != null,
            cancellationToken);
        var activeServiceClientCount = await db.ServiceClients.CountAsync(c => c.IsActive, cancellationToken);

        var cleanupSettings = await serviceTokenCleanupSettings.GetAsync(principal, cancellationToken);
        var pendingCleanup = cleanupSettings.IsEnabled
            ? (await serviceTokenCleanupWorker.ListPendingAsync(null, 500, cancellationToken)).Items.Count
            : 0;

        var overview = await lifecycleOverview.GetOverviewAsync(principal, cancellationToken);
        var workers = overview.Workers
            .Select(w => new PlatformWorkerOrchestrationWorkerStatus(
                w.WorkerKey,
                w.Label,
                w.Description,
                w.IsEnabled,
                w.PendingCount,
                w.LatestRun,
                w.ServiceTokenScope,
                w.SuiteAdminPath))
            .ToList();

        await audit.WriteAsync(
            "platform_worker_health.orchestration.read",
            "platform_worker_health_orchestration",
            null,
            "Success",
            actorUserId: actorUserId,
            reasonCode: health.Status,
            cancellationToken: cancellationToken);

        return new PlatformWorkerHealthOrchestrationStatusResponse(
            now,
            health.Status,
            health.Products,
            new PlatformServiceTokenInventorySummary(
                activeCount,
                expiringCount,
                expiredRetained,
                revokedRetained,
                pendingCleanup),
            activeServiceClientCount,
            workers);
    }

    public async Task<TriggerServiceTokenCleanupOrchestrationResponse> TriggerServiceTokenCleanupAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();
        var settings = await serviceTokenCleanupSettings.GetAsync(principal, cancellationToken);
        if (!settings.IsEnabled)
        {
            throw new StlApiException(
                "service_token_cleanup.disabled",
                "Enable service token cleanup before running a manual batch.",
                409);
        }

        var result = await serviceTokenCleanupWorker.ProcessBatchAsync(
            new ProcessServiceTokenCleanupRequest(null, null),
            cancellationToken);

        await audit.WriteAsync(
            "platform_worker_health.trigger_service_token_cleanup",
            "service_token_cleanup",
            null,
            "Success",
            actorUserId: actorUserId,
            reasonCode: result.PurgedCount.ToString(),
            cancellationToken: cancellationToken);

        return new TriggerServiceTokenCleanupOrchestrationResponse(
            result.AsOfUtc,
            result.PurgedCount,
            result.SkippedCount);
    }

    public async Task<TriggerLaunchDestinationReconciliationOrchestrationResponse> TriggerLaunchDestinationReconciliationAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();
        var settings = await launchDestinationReconciliationSettings.GetAsync(principal, cancellationToken);
        if (!settings.IsEnabled)
        {
            throw new StlApiException(
                "launch_destination_reconciliation.disabled",
                "Enable launch-destination reconciliation before running a manual batch.",
                409);
        }

        var result = await launchDestinationReconciliationWorker.ProcessBatchAsync(
            new ProcessLaunchDestinationReconciliationRequest(null, null),
            cancellationToken);

        await audit.WriteAsync(
            "platform_worker_health.trigger_launch_destination_reconciliation",
            "launch_destination_reconciliation",
            null,
            "Success",
            actorUserId: actorUserId,
            reasonCode: $"{result.GrantedCount}/{result.RevokedCount}",
            cancellationToken: cancellationToken);

        return new TriggerLaunchDestinationReconciliationOrchestrationResponse(
            result.AsOfUtc,
            result.GrantedCount,
            result.RevokedCount,
            result.SkippedCount);
    }

    public async Task<TriggerLaunchAvailabilityReconciliationOrchestrationResponse> TriggerLaunchAvailabilityReconciliationAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var result = await TriggerLaunchDestinationReconciliationAsync(principal, cancellationToken);
        return new TriggerLaunchAvailabilityReconciliationOrchestrationResponse(
            result.AsOfUtc,
            result.GrantedCount,
            result.RevokedCount,
            result.SkippedCount);
    }

    public async Task<TriggerTenantLifecycleOrchestrationResponse> TriggerTenantLifecycleAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();
        var settings = await tenantLifecycleSettings.GetAsync(principal, cancellationToken);
        if (!settings.IsEnabled)
        {
            throw new StlApiException(
                "tenant_lifecycle.disabled",
                "Enable tenant lifecycle processing before running a manual batch.",
                409);
        }

        var result = await tenantLifecycleWorker.ProcessBatchAsync(
            new ProcessTenantLifecycleRequest(null, null),
            cancellationToken);

        await audit.WriteAsync(
            "platform_worker_health.trigger_tenant_lifecycle",
            "tenant_lifecycle",
            null,
            "Success",
            actorUserId: actorUserId,
            reasonCode: $"{result.SuspendedCount}/{result.ReactivatedCount}",
            cancellationToken: cancellationToken);

        return new TriggerTenantLifecycleOrchestrationResponse(
            result.AsOfUtc,
            result.SuspendedCount,
            result.ReactivatedCount,
            result.SkippedCount);
    }

    public async Task<TriggerPlatformOutboxPublisherOrchestrationResponse> TriggerPlatformOutboxPublisherAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();
        var settings = await platformOutboxPublisherSettings.GetAsync(principal, cancellationToken);
        if (!settings.IsEnabled)
        {
            throw new StlApiException(
                "platform_outbox.disabled",
                "Enable platform outbox publishing before running a manual batch.",
                409);
        }

        var result = await platformOutboxPublisherWorker.ProcessBatchAsync(
            new ProcessPlatformOutboxPublisherRequest(null, null),
            cancellationToken);

        await audit.WriteAsync(
            "platform_worker_health.trigger_platform_outbox",
            "platform_outbox_publisher",
            null,
            "Success",
            actorUserId: actorUserId,
            reasonCode: result.PublishedCount.ToString(),
            cancellationToken: cancellationToken);

        return new TriggerPlatformOutboxPublisherOrchestrationResponse(
            result.AsOfUtc,
            result.PublishedCount,
            result.FailedCount,
            result.DeadLetterCount,
            result.SkippedCount);
    }
}
