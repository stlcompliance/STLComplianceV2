using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class SupplyReadinessEndpoints
{
    public static void MapSupplyArrSupplyReadinessEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string namePrefix)
        {
            group.MapGet("/dashboard", GetDashboardAsync)
                .WithName($"{namePrefix}GetSupplyArrSupplyReadinessDashboard");
            group.MapGet("/parts/{partId:guid}", GetPartReadinessAsync)
                .WithName($"{namePrefix}GetSupplyArrPartSupplyReadiness");
            group.MapGet("/vendors/{externalPartyId:guid}", GetVendorReadinessAsync)
                .WithName($"{namePrefix}GetSupplyArrVendorSupplyReadiness");
            group.MapGet("/procurement-path", GetProcurementPathReadinessAsync)
                .WithName($"{namePrefix}GetSupplyArrProcurementPathReadiness");
        }

        static async Task<IResult> GetDashboardAsync(
            SupplyArrAuthorizationService authorization,
            SupplyReadinessService service,
            ISupplyArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken)
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
        }

        static async Task<IResult> GetPartReadinessAsync(
            Guid partId,
            decimal? quantity,
            SupplyArrAuthorizationService authorization,
            SupplyReadinessService service,
            ISupplyArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken)
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
        }

        static async Task<IResult> GetVendorReadinessAsync(
            Guid externalPartyId,
            SupplyArrAuthorizationService authorization,
            SupplyReadinessService service,
            ISupplyArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken)
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
        }

        static async Task<IResult> GetProcurementPathReadinessAsync(
            Guid partId,
            Guid externalPartyId,
            decimal? quantity,
            SupplyArrAuthorizationService authorization,
            SupplyReadinessService service,
            ISupplyArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken)
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
        }

        var legacyGroup = app.MapGroup("/api/supply-readiness")
            .WithTags("SupplyReadiness")
            .RequireAuthorization();
        MapRoutes(legacyGroup, string.Empty);

        var v1Group = app.MapGroup("/api/v1/supply-readiness")
            .WithTags("SupplyReadiness")
            .RequireAuthorization();
        MapRoutes(v1Group, "V1");

        var v1ReadinessAliasGroup = app.MapGroup("/api/v1/readiness")
            .WithTags("SupplyReadiness")
            .RequireAuthorization();
        MapRoutes(v1ReadinessAliasGroup, "V1Alias");
    }
}
