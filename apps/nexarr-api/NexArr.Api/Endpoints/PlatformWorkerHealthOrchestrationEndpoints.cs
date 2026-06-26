using NexArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Endpoints;

public static class PlatformWorkerHealthOrchestrationEndpoints
{
    private const string RetiredEntitlementReconciliationCode = "entitlement_reconciliation.retired";
    private const string RetiredEntitlementReconciliationMessage =
        "Entitlement reconciliation is retired. Use the launch-destination reconciliation workflow for remaining compatibility review.";
    private const string RetiredEntitlementReconciliationSummary =
        "Retired entitlement reconciliation compatibility endpoint.";

    public static void MapPlatformWorkerHealthOrchestrationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/platform-admin/worker-health-orchestration")
            .WithTags("PlatformAdmin")
            .RequireAuthorization();

        group.MapGet("/", async (
            HttpContext context,
            PlatformWorkerHealthOrchestrationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetStatusAsync(context.User, cancellationToken));
        })
        .WithName("GetPlatformWorkerHealthOrchestration");

        group.MapPost("/trigger-service-token-cleanup", async (
            HttpContext context,
            PlatformWorkerHealthOrchestrationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.TriggerServiceTokenCleanupAsync(context.User, cancellationToken));
        })
        .WithName("TriggerPlatformServiceTokenCleanupOrchestration");

        group.MapPost("/trigger-entitlement-reconciliation", () =>
        {
            throw new StlApiException(
                RetiredEntitlementReconciliationCode,
                RetiredEntitlementReconciliationMessage,
                410);
        })
        .WithName("TriggerPlatformRetiredEntitlementReconciliationCompatibility")
        .WithSummary(RetiredEntitlementReconciliationSummary)
        .WithDescription(RetiredEntitlementReconciliationMessage);

        group.MapPost("/trigger-launch-destination-reconciliation", async (
            HttpContext context,
            PlatformWorkerHealthOrchestrationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.TriggerLaunchDestinationReconciliationAsync(context.User, cancellationToken));
        })
        .WithName("TriggerPlatformLaunchDestinationReconciliationOrchestration");

        group.MapPost("/trigger-launch-availability-reconciliation", async (
            HttpContext context,
            PlatformWorkerHealthOrchestrationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.TriggerLaunchAvailabilityReconciliationAsync(context.User, cancellationToken));
        })
        .WithName("TriggerPlatformLaunchAvailabilityReconciliationOrchestration");

        group.MapPost("/trigger-tenant-lifecycle", async (
            HttpContext context,
            PlatformWorkerHealthOrchestrationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.TriggerTenantLifecycleAsync(context.User, cancellationToken));
        })
        .WithName("TriggerPlatformTenantLifecycleOrchestration");

        group.MapPost("/trigger-platform-outbox", async (
            HttpContext context,
            PlatformWorkerHealthOrchestrationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.TriggerPlatformOutboxPublisherAsync(context.User, cancellationToken));
        })
        .WithName("TriggerPlatformOutboxPublisherOrchestration");
    }
}
