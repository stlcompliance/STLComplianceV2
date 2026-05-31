using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class EntitlementEndpoints
{
    public static void MapEntitlementEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/entitlements").WithTags("Entitlements").RequireAuthorization();
        var v1TenantEntitlements = app.MapGroup("/api/v1/tenants/{tenantId:guid}/entitlements").WithTags("Entitlements").RequireAuthorization();
        var v1Entitlements = app.MapGroup("/api/v1/entitlements").WithTags("Entitlements").RequireAuthorization();

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

        v1TenantEntitlements.MapGet("/", async (
            Guid tenantId,
            HttpContext context,
            EntitlementAdminService service,
            int page,
            int pageSize,
            CancellationToken cancellationToken) =>
        {
            var result = await service.ListByTenantAsync(
                context.User,
                tenantId,
                page == 0 ? 1 : page,
                pageSize == 0 ? 50 : pageSize,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("ListTenantEntitlementsV1");

        v1TenantEntitlements.MapPost("/", async (
            Guid tenantId,
            GrantEntitlementRequest request,
            HttpContext context,
            EntitlementAdminService service,
            CancellationToken cancellationToken) =>
        {
            var granted = await service.GrantForTenantProductAsync(
                context.User,
                tenantId,
                request.ProductKey,
                cancellationToken);
            return Results.Created($"/api/v1/tenants/{tenantId}/entitlements/{granted.ProductKey}", granted);
        })
        .WithName("GrantTenantEntitlementV1");

        v1TenantEntitlements.MapPatch("/{productCode}", async (
            Guid tenantId,
            string productCode,
            UpdateTenantEntitlementRequest request,
            HttpContext context,
            EntitlementAdminService service,
            CancellationToken cancellationToken) =>
        {
            if (string.Equals(request.Status, "active", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Ok(await service.GrantForTenantProductAsync(context.User, tenantId, productCode, cancellationToken));
            }

            return Results.Ok(await service.RevokeForTenantProductAsync(context.User, tenantId, productCode, cancellationToken));
        })
        .WithName("UpdateTenantEntitlementV1");

        v1TenantEntitlements.MapDelete("/{productCode}", async (
            Guid tenantId,
            string productCode,
            HttpContext context,
            EntitlementAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.RevokeForTenantProductAsync(context.User, tenantId, productCode, cancellationToken));
        })
        .WithName("DeleteTenantEntitlementV1");

        v1Entitlements.MapGet("/check", async (
            Guid tenantId,
            string productCode,
            HttpContext context,
            EntitlementAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.CheckAsync(context.User, tenantId, productCode, cancellationToken));
        })
        .WithName("CheckEntitlementV1");
    }
}
