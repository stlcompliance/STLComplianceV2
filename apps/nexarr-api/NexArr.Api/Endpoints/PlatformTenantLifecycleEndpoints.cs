using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class PlatformTenantLifecycleEndpoints
{
    public static void MapPlatformTenantLifecycleEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/platform-admin/tenant-lifecycle")
            .WithTags("PlatformAdmin")
            .RequireAuthorization();

        group.MapGet("/settings", async (
            HttpContext context,
            TenantLifecycleSettingsService settingsService,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await settingsService.GetAsync(context.User, cancellationToken));
        })
        .WithName("GetPlatformTenantLifecycleSettings");

        group.MapPut("/settings", async (
            UpsertTenantLifecycleSettingsRequest request,
            HttpContext context,
            TenantLifecycleSettingsService settingsService,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await settingsService.UpsertAsync(context.User, request, cancellationToken));
        })
        .WithName("UpsertPlatformTenantLifecycleSettings");

        group.MapGet("/runs", async (
            int? limit,
            HttpContext context,
            TenantLifecycleSettingsService settingsService,
            TenantLifecycleWorkerService workerService,
            CancellationToken cancellationToken) =>
        {
            await settingsService.GetAsync(context.User, cancellationToken);
            return Results.Ok(await workerService.ListRecentRunsAsync(limit, cancellationToken));
        })
        .WithName("ListPlatformTenantLifecycleRuns");

        group.MapGet("/pending", async (
            DateTimeOffset? asOfUtc,
            int? batchSize,
            HttpContext context,
            TenantLifecycleSettingsService settingsService,
            TenantLifecycleWorkerService workerService,
            CancellationToken cancellationToken) =>
        {
            await settingsService.GetAsync(context.User, cancellationToken);
            return Results.Ok(await workerService.ListPendingAsync(asOfUtc, batchSize, cancellationToken));
        })
        .WithName("ListPlatformTenantLifecyclePending");
    }
}
