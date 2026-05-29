using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class IntegrationEndpoints
{
    public const string SupplyarrDemandStatusIngestActionScope = "maintainarr.demand_status.write";

    public const string RoutarrAssetReadinessDispatchActionScope = "maintainarr.asset_readiness.dispatch_gate";

    public const string StaffarrPersonSyncActionScope = "maintainarr.technician_refs.sync";

    public static void MapMaintainArrIntegrationEndpoints(this WebApplication app)
    {
        var integrations = app.MapGroup("/api/integrations").WithTags("Integrations");

        integrations.MapGet("/routarr-asset-readiness", async (
            Guid tenantId,
            string? vehicleRefKey,
            string? assetTag,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            AssetReadinessService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "routarr",
                    RequiredTargetProduct = "maintainarr",
                    TenantId = tenantId,
                    RequiredActionScope = RoutarrAssetReadinessDispatchActionScope
                });

            var result = await service.GetByDispatchRefAsync(
                tenantId,
                vehicleRefKey,
                assetTag,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("RoutarrAssetReadinessCheck");

        integrations.MapPost("/supplyarr-demand-status", async (
            IngestSupplyarrDemandStatusRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            WorkOrderPartsDemandStatusIngestionService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "supplyarr",
                    RequiredTargetProduct = "maintainarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = SupplyarrDemandStatusIngestActionScope
                });

            var result = await service.IngestAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("IngestSupplyarrDemandStatus");

        integrations.MapPost("/staffarr-person-sync", async (
            IngestStaffarrPersonSyncRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            StaffarrPersonSyncIngestionService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "staffarr",
                    RequiredTargetProduct = "maintainarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = StaffarrPersonSyncActionScope
                });

            var result = await service.IngestAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("IngestStaffarrPersonSync");
    }
}
