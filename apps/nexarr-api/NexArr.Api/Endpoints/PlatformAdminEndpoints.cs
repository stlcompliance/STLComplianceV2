using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class PlatformAdminEndpoints
{
    public static void MapPlatformAdminEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string suffix)
        {
            group.MapGet("/dashboard", async (
            HttpContext context,
            PlatformAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetDashboardAsync(context.User, cancellationToken));
        })
        .WithName($"PlatformAdminDashboard{suffix}");

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
        .WithName($"PlatformAdminLaunchDiagnostics{suffix}");

            group.MapGet("/launch-attempts", async (
            HttpContext context,
            PlatformAdminService service,
            Guid? tenantId,
            Guid? userId,
            string? productKey,
            Guid? correlationId,
            DateTimeOffset? fromUtc,
            DateTimeOffset? toUtc,
            string? result,
            int? page,
            int? pageSize,
            CancellationToken cancellationToken) =>
        {
            var attempts = await service.GetLaunchAttemptsAsync(
                context.User,
                tenantId,
                userId,
                productKey,
                correlationId,
                fromUtc,
                toUtc,
                result,
                page is null or 0 ? 1 : page.Value,
                pageSize is null or 0 ? 50 : pageSize.Value,
                cancellationToken);
            return Results.Ok(attempts);
        })
        .WithName($"PlatformAdminLaunchAttempts{suffix}");

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
        .WithName($"PlatformAdminTenantOverview{suffix}");

            group.MapGet("/overview/products", async (
            HttpContext context,
            PlatformAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetProductOverviewAsync(context.User, cancellationToken));
        })
        .WithName($"PlatformAdminProductOverview{suffix}");

            group.MapGet("/product-manifests", async (
            HttpContext context,
            ProductManifestService service,
            Guid? tenantId,
            string? productKey,
            int? page,
            int? pageSize,
            CancellationToken cancellationToken) =>
        {
            var result = await service.ListAsync(
                context.User,
                tenantId,
                productKey,
                page is null or 0 ? 1 : page.Value,
                pageSize is null or 0 ? 50 : pageSize.Value,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName($"PlatformAdminProductManifests{suffix}");

            group.MapPost("/users", async (
            CreatePlatformUserRequest request,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            var created = await service.CreateUserAsync(context.User, request, cancellationToken);
            return Results.Created($"/api/platform-admin/users/{created.UserId}", created);
        })
        .WithName($"PlatformAdminCreateUser{suffix}");

            group.MapPatch("/users/{userId:guid}", async (
            Guid userId,
            UpdatePlatformUserRequest request,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.UpdateUserAsync(context.User, userId, request, cancellationToken));
        })
        .WithName($"PlatformAdminUpdateUser{suffix}");

            group.MapPost("/users/{userId:guid}/enable", async (
            Guid userId,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.EnableUserAsync(context.User, userId, cancellationToken));
        })
        .WithName($"PlatformAdminEnableUser{suffix}");

            group.MapPost("/users/{userId:guid}/lock", async (
            Guid userId,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.LockUserAsync(context.User, userId, cancellationToken));
        })
        .WithName($"PlatformAdminLockUser{suffix}");

            group.MapPost("/users/{userId:guid}/unlock", async (
            Guid userId,
            HttpContext context,
            PlatformUserAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.UnlockUserAsync(context.User, userId, cancellationToken));
        })
        .WithName($"PlatformAdminUnlockUser{suffix}");
        }

        MapRoutes(
            app.MapGroup("/api/platform-admin")
                .WithTags("PlatformAdmin")
                .RequireAuthorization(),
            string.Empty);

        MapRoutes(
            app.MapGroup("/api/v1/platform-admin")
                .WithTags("PlatformAdmin")
                .RequireAuthorization(),
            "V1");
    }
}
