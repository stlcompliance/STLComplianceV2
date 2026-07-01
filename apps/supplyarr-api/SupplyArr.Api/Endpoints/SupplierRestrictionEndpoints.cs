using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class SupplierRestrictionEndpoints
{
    public static void MapSupplyArrSupplierRestrictionEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix, string tagName)
        {
            group = group.WithTags(tagName).RequireAuthorization();

            group.MapGet("/", async (
                string? status,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierRestrictionService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSuppliersRead(context.User);
                var tenantId = context.User.GetTenantId();
                var rows = await service.ListAsync(tenantId, status, cancellationToken);
                return Results.Ok(rows);
            })
            .WithName($"ListSupplierRestrictions{nameSuffix}");

            group.MapGet("/{restrictionId:guid}", async (
                Guid restrictionId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierRestrictionService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSuppliersRead(context.User);
                var tenantId = context.User.GetTenantId();
                var response = await service.GetAsync(tenantId, restrictionId, cancellationToken);
                return Results.Ok(response);
            })
            .WithName($"GetSupplierRestriction{nameSuffix}");

            group.MapPost("/{restrictionId:guid}/lift", async (
                Guid restrictionId,
                LiftSupplierRestrictionRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierRestrictionService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSuppliersManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var response = await service.LiftAsync(
                    tenantId,
                    actorUserId,
                    restrictionId,
                    request,
                    cancellationToken);
                return Results.Ok(response);
            })
            .WithName($"LiftSupplierRestriction{nameSuffix}");

            group.MapPut("/{restrictionId:guid}", async (
                Guid restrictionId,
                UpdateSupplierRestrictionRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierRestrictionService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSuppliersManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var response = await service.UpdateAsync(
                    tenantId,
                    actorUserId,
                    restrictionId,
                    request,
                    cancellationToken);
                return Results.Ok(response);
            })
            .WithName($"UpdateSupplierRestriction{nameSuffix}");
        }

        static void MapSupplierRoutes(RouteGroupBuilder supplierGroup, string nameSuffix, string tagName)
        {
            supplierGroup = supplierGroup.WithTags(tagName).RequireAuthorization();

            supplierGroup.MapGet("/", async (
                Guid supplierId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierRestrictionService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSuppliersRead(context.User);
                var tenantId = context.User.GetTenantId();
                var rows = await service.ListBySupplierAsync(tenantId, supplierId, cancellationToken);
                return Results.Ok(rows);
            })
            .WithName($"ListSupplierRestrictionsBySupplier{nameSuffix}");

            supplierGroup.MapPost("/", async (
                Guid supplierId,
                CreateSupplierRestrictionRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierRestrictionService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSuppliersManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var response = await service.CreateAsync(
                    tenantId,
                    actorUserId,
                    supplierId,
                    request,
                    cancellationToken);
                return Results.Ok(response);
            })
            .WithName($"CreateSupplierRestrictionForSupplier{nameSuffix}");

            supplierGroup.MapGet("/enforcement", async (
                Guid supplierId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierRestrictionService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSuppliersRead(context.User);
                var tenantId = context.User.GetTenantId();
                var response = await service.GetEnforcementAsync(tenantId, supplierId, cancellationToken);
                return Results.Ok(response);
            })
            .WithName($"GetSupplierRestrictionEnforcementBySupplier{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/v1/supplier-restrictions"), "V1", "SupplierRestrictions");

        MapSupplierRoutes(app.MapGroup("/api/v1/suppliers/{supplierId:guid}/restrictions"), "V1BySupplier", "SupplierRestrictions");
    }
}
