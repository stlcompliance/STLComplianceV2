using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class SupplyReadinessEndpoints
{
    public static void MapSupplyArrSupplyReadinessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/supply-readiness")
            .WithTags("SupplyReadiness")
            .RequireAuthorization();

        group.MapGet("/dashboard", async (
            SupplyArrAuthorizationService authorization,
            SupplyReadinessService service,
            ISupplyArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSupplyReadinessRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var dashboard = await service.GetDashboardAsync(tenantId, cancellationToken);
            await audit.WriteAsync(
                "supplyarr.supply_readiness.dashboard",
                tenantId,
                actorUserId,
                "supply_readiness_dashboard",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(dashboard);
        })
        .WithName("GetSupplyArrSupplyReadinessDashboard");

        group.MapGet("/parts/{partId:guid}", async (
            Guid partId,
            decimal? quantity,
            SupplyArrAuthorizationService authorization,
            SupplyReadinessService service,
            ISupplyArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSupplyReadinessRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await service.GetPartReadinessAsync(tenantId, partId, quantity, cancellationToken);
            await audit.WriteAsync(
                "supplyarr.supply_readiness.part",
                tenantId,
                actorUserId,
                "part",
                partId.ToString(),
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetSupplyArrPartSupplyReadiness");

        group.MapGet("/vendors/{externalPartyId:guid}", async (
            Guid externalPartyId,
            SupplyArrAuthorizationService authorization,
            SupplyReadinessService service,
            ISupplyArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSupplyReadinessRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await service.GetVendorReadinessAsync(tenantId, externalPartyId, cancellationToken);
            await audit.WriteAsync(
                "supplyarr.supply_readiness.vendor",
                tenantId,
                actorUserId,
                "external_party",
                externalPartyId.ToString(),
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetSupplyArrVendorSupplyReadiness");

        group.MapGet("/procurement-path", async (
            Guid partId,
            Guid externalPartyId,
            decimal? quantity,
            SupplyArrAuthorizationService authorization,
            SupplyReadinessService service,
            ISupplyArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSupplyReadinessRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await service.GetProcurementPathReadinessAsync(
                tenantId,
                partId,
                externalPartyId,
                quantity,
                cancellationToken);
            await audit.WriteAsync(
                "supplyarr.supply_readiness.procurement_path",
                tenantId,
                actorUserId,
                "procurement_path",
                $"{partId}:{externalPartyId}",
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetSupplyArrProcurementPathReadiness");
    }
}
