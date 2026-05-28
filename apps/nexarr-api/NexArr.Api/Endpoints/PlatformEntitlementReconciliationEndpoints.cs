using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class PlatformEntitlementReconciliationEndpoints
{
    public static void MapPlatformEntitlementReconciliationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/platform-admin/entitlement-reconciliation")
            .WithTags("PlatformAdmin")
            .RequireAuthorization();

        group.MapGet("/settings", async (
            HttpContext context,
            EntitlementReconciliationSettingsService settingsService,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await settingsService.GetAsync(context.User, cancellationToken));
        })
        .WithName("GetPlatformEntitlementReconciliationSettings");

        group.MapPut("/settings", async (
            UpsertEntitlementReconciliationSettingsRequest request,
            HttpContext context,
            EntitlementReconciliationSettingsService settingsService,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await settingsService.UpsertAsync(context.User, request, cancellationToken));
        })
        .WithName("UpsertPlatformEntitlementReconciliationSettings");

        group.MapGet("/runs", async (
            int? limit,
            HttpContext context,
            EntitlementReconciliationSettingsService settingsService,
            EntitlementReconciliationWorkerService workerService,
            CancellationToken cancellationToken) =>
        {
            await settingsService.GetAsync(context.User, cancellationToken);
            return Results.Ok(await workerService.ListRecentRunsAsync(limit, cancellationToken));
        })
        .WithName("ListPlatformEntitlementReconciliationRuns");

        group.MapGet("/pending", async (
            DateTimeOffset? asOfUtc,
            int? batchSize,
            HttpContext context,
            EntitlementReconciliationSettingsService settingsService,
            EntitlementReconciliationWorkerService workerService,
            CancellationToken cancellationToken) =>
        {
            await settingsService.GetAsync(context.User, cancellationToken);
            return Results.Ok(await workerService.ListPendingAsync(asOfUtc, batchSize, cancellationToken));
        })
        .WithName("ListPlatformEntitlementReconciliationPending");

        group.MapGet("/licenses", async (
            Guid? tenantId,
            HttpContext context,
            TenantProductLicenseAdminService licenseService,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await licenseService.ListAsync(context.User, tenantId, cancellationToken));
        })
        .WithName("ListPlatformTenantProductLicenses");

        group.MapPut("/licenses", async (
            UpsertTenantProductLicenseRequest request,
            HttpContext context,
            TenantProductLicenseAdminService licenseService,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await licenseService.UpsertAsync(context.User, request, cancellationToken));
        })
        .WithName("UpsertPlatformTenantProductLicense");
    }
}
