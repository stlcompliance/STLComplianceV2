using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class EntitlementEndpoints
{
    public static void MapEntitlementEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/entitlements").WithTags("Entitlements").RequireAuthorization();

        group.MapGet("/", async (
            HttpContext context,
            EntitlementAdminService service,
            Guid? tenantId,
            int page,
            int pageSize,
            CancellationToken cancellationToken) =>
        {
            var result = await service.ListAsync(
                context.User,
                tenantId,
                page == 0 ? 1 : page,
                pageSize == 0 ? 50 : pageSize,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("ListEntitlements");

        group.MapPost("/", async (
            GrantEntitlementRequest request,
            HttpContext context,
            EntitlementAdminService service,
            CancellationToken cancellationToken) =>
        {
            var granted = await service.GrantAsync(context.User, request, cancellationToken);
            return Results.Created($"/api/entitlements/{granted.EntitlementId}", granted);
        })
        .WithName("GrantEntitlement");

        group.MapPost("/{entitlementId:guid}/revoke", async (
            Guid entitlementId,
            HttpContext context,
            EntitlementAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.RevokeAsync(context.User, entitlementId, cancellationToken));
        })
        .WithName("RevokeEntitlement");
    }
}
