using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class TenantEndpoints
{
    public static void MapTenantEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tenants").WithTags("Tenants").RequireAuthorization();

        group.MapGet("/", async (
            HttpContext context,
            TenantAdminService service,
            int page,
            int pageSize,
            CancellationToken cancellationToken) =>
        {
            var result = await service.ListAsync(context.User, page == 0 ? 1 : page, pageSize == 0 ? 50 : pageSize, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("ListTenants");

        group.MapGet("/{tenantId:guid}", async (
            Guid tenantId,
            HttpContext context,
            TenantAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetAsync(context.User, tenantId, cancellationToken));
        })
        .WithName("GetTenant");

        group.MapPost("/", async (
            CreateTenantRequest request,
            HttpContext context,
            TenantAdminService service,
            CancellationToken cancellationToken) =>
        {
            var created = await service.CreateAsync(context.User, request, cancellationToken);
            return Results.Created($"/api/tenants/{created.TenantId}", created);
        })
        .WithName("CreateTenant");

        group.MapPut("/{tenantId:guid}", async (
            Guid tenantId,
            UpdateTenantRequest request,
            HttpContext context,
            TenantAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.UpdateAsync(context.User, tenantId, request, cancellationToken));
        })
        .WithName("UpdateTenant");

        group.MapPatch("/{tenantId:guid}/status", async (
            Guid tenantId,
            UpdateTenantStatusRequest request,
            HttpContext context,
            TenantAdminService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.UpdateStatusAsync(context.User, tenantId, request, cancellationToken));
        })
        .WithName("UpdateTenantStatus");
    }
}
