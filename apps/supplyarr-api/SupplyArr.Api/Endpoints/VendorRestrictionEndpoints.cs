using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class VendorRestrictionEndpoints
{
    public static void MapSupplyArrVendorRestrictionEndpoints(this WebApplication app)
    {
        static VendorRestrictionResponse MapVendorAlias(SupplierRestrictionResponse response)
            => new(
                response.RestrictionId,
                response.SupplierId,
                response.SupplierKey,
                response.SupplierDisplayName,
                response.ParentSupplierId,
                response.ParentSupplierDisplayName,
                response.SupplierUnitKind,
                response.SupplierServiceTypes,
                response.RestrictionKey,
                response.Scopes,
                response.Reason,
                response.Status,
                response.EffectiveFrom,
                response.EffectiveUntil,
                response.CreatedByUserId,
                response.LiftedByUserId,
                response.LiftedAt,
                response.LiftNotes,
                response.CreatedAt,
                response.UpdatedAt,
                response.SupplierRestrictionId ?? response.RestrictionId,
                response.SupplierId,
                response.SupplierKey,
                response.SupplierDisplayName,
                "supplier",
                "supplier");

        static VendorRestrictionEnforcementResponse MapVendorEnforcementAlias(SupplierRestrictionEnforcementResponse response)
            => new(
                response.SupplierId,
                response.SupplierKey,
                response.SupplierDisplayName,
                response.ParentSupplierId,
                response.ParentSupplierDisplayName,
                response.SupplierUnitKind,
                response.SupplierServiceTypes,
                response.IsBlocked,
                response.BlockReason,
                response.ActiveScopes,
                response.SupplierId);

        static void MapRoutes(RouteGroupBuilder group, string nameSuffix, string tagName, bool useVendorAliasResponse = false)
        {
            group = group.WithTags(tagName).RequireAuthorization();

            group.MapGet("/", async (
                string? status,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                VendorRestrictionService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePartiesRead(context.User);
                var tenantId = context.User.GetTenantId();
                var rows = await service.ListAsync(tenantId, status, cancellationToken);
                return Results.Ok(useVendorAliasResponse ? rows.Select(MapVendorAlias) : rows);
            })
            .WithName($"ListSupplierRestrictions{nameSuffix}");

            group.MapGet("/{restrictionId:guid}", async (
                Guid restrictionId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                VendorRestrictionService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePartiesRead(context.User);
                var tenantId = context.User.GetTenantId();
                var response = await service.GetAsync(tenantId, restrictionId, cancellationToken);
                return Results.Ok(useVendorAliasResponse ? MapVendorAlias(response) : response);
            })
            .WithName($"GetSupplierRestriction{nameSuffix}");

            group.MapPost("/{restrictionId:guid}/lift", async (
                Guid restrictionId,
                LiftSupplierRestrictionRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                VendorRestrictionService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePartiesManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var response = await service.LiftAsync(
                    tenantId,
                    actorUserId,
                    restrictionId,
                    request,
                    cancellationToken);
                return Results.Ok(useVendorAliasResponse ? MapVendorAlias(response) : response);
            })
            .WithName($"LiftSupplierRestriction{nameSuffix}");

            group.MapPut("/{restrictionId:guid}", async (
                Guid restrictionId,
                UpdateSupplierRestrictionRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                VendorRestrictionService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePartiesManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var response = await service.UpdateAsync(
                    tenantId,
                    actorUserId,
                    restrictionId,
                    request,
                    cancellationToken);
                return Results.Ok(useVendorAliasResponse ? MapVendorAlias(response) : response);
            })
            .WithName($"UpdateSupplierRestriction{nameSuffix}");
        }

        static void MapSupplierRoutes(RouteGroupBuilder supplierGroup, string nameSuffix, string tagName, bool useVendorAliasResponse = false)
        {
            supplierGroup = supplierGroup.WithTags(tagName).RequireAuthorization();

            supplierGroup.MapGet("/", async (
                Guid supplierId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                VendorRestrictionService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePartiesRead(context.User);
                var tenantId = context.User.GetTenantId();
                var rows = await service.ListBySupplierAsync(tenantId, supplierId, cancellationToken);
                return Results.Ok(useVendorAliasResponse ? rows.Select(MapVendorAlias) : rows);
            })
            .WithName($"ListSupplierRestrictionsBySupplier{nameSuffix}");

            supplierGroup.MapPost("/", async (
                Guid supplierId,
                CreateSupplierRestrictionRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                VendorRestrictionService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePartiesManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var response = await service.CreateAsync(
                    tenantId,
                    actorUserId,
                    supplierId,
                    request,
                    cancellationToken);
                return Results.Ok(useVendorAliasResponse ? MapVendorAlias(response) : response);
            })
            .WithName($"CreateSupplierRestrictionForSupplier{nameSuffix}");

            supplierGroup.MapGet("/enforcement", async (
                Guid supplierId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                VendorRestrictionService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePartiesRead(context.User);
                var tenantId = context.User.GetTenantId();
                var response = await service.GetEnforcementAsync(tenantId, supplierId, cancellationToken);
                return Results.Ok(useVendorAliasResponse ? MapVendorEnforcementAlias(response) : response);
            })
            .WithName($"GetSupplierRestrictionEnforcementBySupplier{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/supplier-restrictions"), string.Empty, "SupplierRestrictions");
        MapRoutes(app.MapGroup("/api/v1/supplier-restrictions"), "V1", "SupplierRestrictions");
        MapRoutes(app.MapGroup("/api/vendor-restrictions"), "VendorAlias", "VendorRestrictions", useVendorAliasResponse: true);
        MapRoutes(app.MapGroup("/api/v1/vendor-restrictions"), "V1VendorAlias", "VendorRestrictions", useVendorAliasResponse: true);

        MapSupplierRoutes(app.MapGroup("/api/suppliers/{supplierId:guid}/restrictions"), "BySupplier", "SupplierRestrictions");
        MapSupplierRoutes(app.MapGroup("/api/v1/suppliers/{supplierId:guid}/restrictions"), "V1BySupplier", "SupplierRestrictions");
        MapSupplierRoutes(app.MapGroup("/api/suppliers/{supplierId:guid}/vendor-restrictions"), "SupplierVendorAlias", "VendorRestrictions", useVendorAliasResponse: true);
        MapSupplierRoutes(app.MapGroup("/api/v1/suppliers/{supplierId:guid}/vendor-restrictions"), "V1SupplierVendorAlias", "VendorRestrictions", useVendorAliasResponse: true);
        MapSupplierRoutes(app.MapGroup("/api/parties/{supplierId:guid}/vendor-restrictions"), "PartyAlias", "VendorRestrictions", useVendorAliasResponse: true);
        MapSupplierRoutes(app.MapGroup("/api/v1/parties/{supplierId:guid}/vendor-restrictions"), "V1PartyAlias", "VendorRestrictions", useVendorAliasResponse: true);
    }
}
