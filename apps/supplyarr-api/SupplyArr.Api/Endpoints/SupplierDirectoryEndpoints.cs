using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class SupplierDirectoryEndpoints
{
    public static void MapSupplyArrSupplierDirectoryEndpoints(this WebApplication app)
    {
        MapSupplierDirectoryGroup(app, "/api/suppliers");
        MapSupplierDirectoryGroup(app, "/api/v1/suppliers");
    }

    private static void MapSupplierDirectoryGroup(WebApplication app, string routePrefix)
    {
        var group = app.MapGroup(routePrefix).WithTags("SupplierDirectory").RequireAuthorization();

        group.MapGet("/", async (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDirectoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSuppliersRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListSuppliersAsync(tenantId, cancellationToken));
        })
        .WithName($"ListSupplierDirectory{RouteSuffix(routePrefix)}");

        group.MapGet("/metadata", (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDirectoryService service) =>
        {
            authorization.RequireSuppliersRead(context.User);
            return Results.Ok(service.GetSupplierMetadata());
        })
        .WithName($"GetSupplierDirectoryMetadata{RouteSuffix(routePrefix)}");

        group.MapGet("/{supplierId:guid}", async (
            Guid supplierId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDirectoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSuppliersRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetSupplierAsync(tenantId, supplierId, cancellationToken));
        })
        .WithName($"GetSupplierDirectoryItem{RouteSuffix(routePrefix)}");

        group.MapPost("/", async (
            CreateSupplierRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDirectoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSuppliersManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateSupplierAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken);
            return Results.Created($"{routePrefix}/{created.SupplierId}", created);
        })
        .WithName($"CreateSupplierDirectoryItem{RouteSuffix(routePrefix)}");

        group.MapPut("/{supplierId:guid}", async (
            Guid supplierId,
            UpdateSupplierRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDirectoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSuppliersManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateSupplierAsync(tenantId, actorUserId, supplierId, request, cancellationToken));
        })
        .WithName($"UpdateSupplierDirectoryItem{RouteSuffix(routePrefix)}");

        group.MapPatch("/{supplierId:guid}/approval-status", async (
            Guid supplierId,
            UpdateSupplierApprovalStatusRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDirectoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSuppliersManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateSupplierApprovalStatusAsync(tenantId, actorUserId, supplierId, request, cancellationToken));
        })
        .WithName($"UpdateSupplierDirectoryApprovalStatus{RouteSuffix(routePrefix)}");

        group.MapPatch("/{supplierId:guid}/status", async (
            Guid supplierId,
            UpdateSupplierStatusRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDirectoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSuppliersManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateSupplierStatusAsync(tenantId, actorUserId, supplierId, request, cancellationToken));
        })
        .WithName($"UpdateSupplierDirectoryStatus{RouteSuffix(routePrefix)}");

        group.MapPost("/{supplierId:guid}/contacts", async (
            Guid supplierId,
            CreateSupplierContactRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDirectoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSuppliersManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var contact = await service.AddSupplierContactAsync(tenantId, actorUserId, supplierId, request, cancellationToken);
            return Results.Created($"{routePrefix}/{supplierId}/contacts/{contact.ContactId}", contact);
        })
        .WithName($"CreateSupplierDirectoryContact{RouteSuffix(routePrefix)}");
    }

    private static string RouteSuffix(string routePrefix) =>
        routePrefix.Contains("/v1/", StringComparison.OrdinalIgnoreCase) ? "SuppliersV1" : "Suppliers";
}
