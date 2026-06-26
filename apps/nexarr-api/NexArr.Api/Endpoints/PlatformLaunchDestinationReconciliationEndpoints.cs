using NexArr.Api.Contracts;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Endpoints;

public static class PlatformLaunchDestinationReconciliationEndpoints
{
    private const string RetiredLicenseCode = "tenant_product_licenses.retired";
    private const string RetiredLicenseMessage =
        "Tenant product license management is retired. Ordinary product availability now follows active tenant membership, product operational state, and product-local permissions.";
    private const string RetiredLicenseSummary = "Retired tenant product license compatibility endpoint.";

    public static void MapPlatformLaunchDestinationReconciliationEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            app.MapGroup("/api/platform-admin/launch-destination-reconciliation"),
            app.MapGroup("/api/platform-admin/entitlement-reconciliation"),
            app.MapGroup("/api/platform-admin/launch-availability-reconciliation"),
        };

        foreach (var group in groups)
        {
            group.WithTags("PlatformAdmin").RequireAuthorization();

            group.MapGet("/settings", async (
                HttpContext context,
                LaunchDestinationReconciliationSettingsService settingsService,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await settingsService.GetAsync(context.User, cancellationToken));
            });

            group.MapPut("/settings", async (
                UpsertLaunchDestinationReconciliationSettingsRequest request,
                HttpContext context,
                LaunchDestinationReconciliationSettingsService settingsService,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await settingsService.UpsertAsync(context.User, request, cancellationToken));
            });

            group.MapGet("/runs", async (
                int? limit,
                HttpContext context,
                LaunchDestinationReconciliationSettingsService settingsService,
                LaunchDestinationReconciliationWorkerService workerService,
                CancellationToken cancellationToken) =>
            {
                await settingsService.GetAsync(context.User, cancellationToken);
                return Results.Ok(await workerService.ListRecentRunsAsync(limit, cancellationToken));
            });

            group.MapGet("/pending", async (
                DateTimeOffset? asOfUtc,
                int? batchSize,
                HttpContext context,
                LaunchDestinationReconciliationSettingsService settingsService,
                LaunchDestinationReconciliationWorkerService workerService,
                CancellationToken cancellationToken) =>
            {
                await settingsService.GetAsync(context.User, cancellationToken);
                return Results.Ok(await workerService.ListPendingAsync(asOfUtc, batchSize, cancellationToken));
            });

            group.MapGet("/licenses", RetiredLicenseAsync)
                .WithSummary(RetiredLicenseSummary)
                .WithDescription(RetiredLicenseMessage);

            group.MapPut("/licenses", RetiredLicenseAsync)
                .WithSummary(RetiredLicenseSummary)
                .WithDescription(RetiredLicenseMessage);
        }
    }

    public static void MapLegacyPlatformEntitlementReconciliationEndpoints(this WebApplication app) =>
        app.MapPlatformLaunchDestinationReconciliationEndpoints();

    private static async Task<IResult> RetiredLicenseAsync(
        HttpContext context,
        PlatformAuthorizationService authorization,
        IPlatformAuditService audit,
        CancellationToken cancellationToken)
    {
        await authorization.RequirePlatformAdminAsync(context.User, cancellationToken);

        await audit.WriteAsync(
            "tenant_product_license.endpoint.retired",
            "tenant_product_license_endpoint",
            context.Request.Path,
            "Denied",
            actorUserId: context.User.GetUserId(),
            reasonCode: RetiredLicenseCode,
            cancellationToken: cancellationToken);

        throw new StlApiException(RetiredLicenseCode, RetiredLicenseMessage, 410);
    }
}
