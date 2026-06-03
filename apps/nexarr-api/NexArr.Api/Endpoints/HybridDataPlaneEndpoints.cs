using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class HybridDataPlaneEndpoints
{
    public static void MapHybridDataPlaneEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/platform-admin/data-plane")
            .WithTags("HybridDataPlane")
            .RequireAuthorization();

        group.MapGet("/", async (
            HttpContext context,
            HybridDataPlaneService service,
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
        .WithName("ListDataPlaneProfiles");

        group.MapGet("/effective", async (
            Guid tenantId,
            HttpContext context,
            HybridDataPlaneService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ListEffectiveAsync(context.User, tenantId, cancellationToken));
        })
        .WithName("ListEffectiveDataPlaneProfiles");

        group.MapPut("/", async (
            UpsertDataPlaneProfileRequest request,
            HttpContext context,
            HybridDataPlaneService service,
            CancellationToken cancellationToken) =>
        {
            var profile = await service.UpsertAsync(context.User, request, cancellationToken);
            return Results.Ok(profile);
        })
        .WithName("UpsertDataPlaneProfile");

        group.MapPost("/validate", async (
            ValidateDataPlaneProfileRequest request,
            HttpContext context,
            HybridDataPlaneService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.ValidateAsync(context.User, request, cancellationToken)))
        .WithName("ValidateDataPlaneProfile");

        group.MapDelete("/{tenantId:guid}/{productKey}", async (
            Guid tenantId,
            string productKey,
            HttpContext context,
            HybridDataPlaneService service,
            CancellationToken cancellationToken) =>
        {
            await service.DeleteAsync(context.User, tenantId, productKey, cancellationToken);
            return Results.NoContent();
        })
        .WithName("DeleteDataPlaneProfile");
    }
}
