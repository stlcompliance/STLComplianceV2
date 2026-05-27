using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class PlatformAdminEndpoints
{
    public static void MapPlatformAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/platform-admin")
            .WithTags("PlatformAdmin")
            .RequireAuthorization();

        group.MapGet("/dashboard", async (
            HttpContext context,
            PlatformAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetDashboardAsync(context.User, cancellationToken));
        })
        .WithName("PlatformAdminDashboard");

        group.MapGet("/launch-diagnostics", async (
            HttpContext context,
            PlatformAdminService service,
            Guid? tenantId,
            string? productKey,
            int? page,
            int? pageSize,
            CancellationToken cancellationToken) =>
        {
            var result = await service.GetLaunchDiagnosticsAsync(
                context.User,
                tenantId,
                productKey,
                page is null or 0 ? 1 : page.Value,
                pageSize is null or 0 ? 50 : pageSize.Value,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("PlatformAdminLaunchDiagnostics");

        group.MapGet("/overview/tenants", async (
            HttpContext context,
            PlatformAdminService service,
            int? page,
            int? pageSize,
            CancellationToken cancellationToken) =>
        {
            var result = await service.GetTenantOverviewAsync(
                context.User,
                page is null or 0 ? 1 : page.Value,
                pageSize is null or 0 ? 50 : pageSize.Value,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("PlatformAdminTenantOverview");

        group.MapGet("/overview/products", async (
            HttpContext context,
            PlatformAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetProductOverviewAsync(context.User, cancellationToken));
        })
        .WithName("PlatformAdminProductOverview");
    }
}
