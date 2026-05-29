using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class PlatformWorkerHealthOrchestrationEndpoints
{
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

        group.MapPost("/trigger-entitlement-reconciliation", async (
            HttpContext context,
            PlatformWorkerHealthOrchestrationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.TriggerEntitlementReconciliationAsync(context.User, cancellationToken));
        })
        .WithName("TriggerPlatformEntitlementReconciliationOrchestration");

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
