using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class SupplierIncidentEndpoints
{
    public static void MapSupplyArrSupplierIncidentEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
        group = group.WithTags("SupplierIncidents").RequireAuthorization();

        group.MapGet("/", async (
            string? status,
            Guid? supplierId,
            string? severity,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierIncidentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSuppliersRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(
                tenantId,
                status,
                supplierId,
                severity,
                cancellationToken));
        })
        .WithName($"ListSupplierIncidents{nameSuffix}");

        group.MapGet("/{incidentId:guid}", async (
            Guid incidentId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierIncidentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSuppliersRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, incidentId, cancellationToken));
        })
        .WithName($"GetSupplierIncident{nameSuffix}");

        group.MapPost("/", async (
            CreateSupplierIncidentRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierIncidentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSuppliersManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.CreateAsync(tenantId, actorUserId, request, cancellationToken));
        })
        .WithName($"CreateSupplierIncident{nameSuffix}");

        group.MapPut("/{incidentId:guid}", async (
            Guid incidentId,
            UpdateSupplierIncidentRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierIncidentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSuppliersManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateAsync(
                tenantId,
                actorUserId,
                incidentId,
                request,
                cancellationToken));
        })
        .WithName($"UpdateSupplierIncident{nameSuffix}");

        group.MapPost("/{incidentId:guid}/start-investigation", async (
            Guid incidentId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierIncidentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSuppliersManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.StartInvestigationAsync(
                tenantId,
                actorUserId,
                incidentId,
                cancellationToken));
        })
        .WithName($"StartSupplierIncidentInvestigation{nameSuffix}");

        group.MapPost("/{incidentId:guid}/resolve", async (
            Guid incidentId,
            ResolveSupplierIncidentRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierIncidentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSuppliersManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.ResolveAsync(
                tenantId,
                actorUserId,
                incidentId,
                request,
                cancellationToken));
        })
        .WithName($"ResolveSupplierIncident{nameSuffix}");

        group.MapPost("/{incidentId:guid}/close", async (
            Guid incidentId,
            CloseSupplierIncidentRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierIncidentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSuppliersManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.CloseAsync(
                tenantId,
                actorUserId,
                incidentId,
                request,
                cancellationToken));
        })
        .WithName($"CloseSupplierIncident{nameSuffix}");

        group.MapPost("/{incidentId:guid}/cancel", async (
            Guid incidentId,
            CancelSupplierIncidentRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierIncidentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSuppliersManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.CancelAsync(
                tenantId,
                actorUserId,
                incidentId,
                request,
                cancellationToken));
        })
        .WithName($"CancelSupplierIncident{nameSuffix}");

        group.MapPost("/{incidentId:guid}/reopen", async (
            Guid incidentId,
            ReopenSupplierIncidentRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierIncidentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSuppliersManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.ReopenAsync(
                tenantId,
                actorUserId,
                incidentId,
                request,
                cancellationToken));
        })
        .WithName($"ReopenSupplierIncident{nameSuffix}");

        group.MapPost("/{incidentId:guid}/apply-procurement-restriction", async (
            Guid incidentId,
            ApplySupplierIncidentProcurementRestrictionRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierIncidentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSuppliersManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.ApplyProcurementRestrictionAsync(
                tenantId,
                actorUserId,
                incidentId,
                request,
                cancellationToken));
        })
        .WithName($"ApplySupplierIncidentProcurementRestriction{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/supplier-incidents"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/supplier-incidents"), "V1");

        static void MapSupplierRoutes(RouteGroupBuilder supplierGroup, string nameSuffix)
        {
        supplierGroup = supplierGroup.WithTags("SupplierIncidents").RequireAuthorization();
        supplierGroup.MapGet("/", async (
            Guid supplierId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierIncidentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSuppliersRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListBySupplierAsync(tenantId, supplierId, cancellationToken));
        })
        .WithName($"ListSupplierIncidentsBySupplier{nameSuffix}");
        }

        MapSupplierRoutes(app.MapGroup("/api/suppliers/{supplierId:guid}/supplier-incidents"), "Supplier");
        MapSupplierRoutes(app.MapGroup("/api/v1/suppliers/{supplierId:guid}/supplier-incidents"), "V1Supplier");
    }
}
