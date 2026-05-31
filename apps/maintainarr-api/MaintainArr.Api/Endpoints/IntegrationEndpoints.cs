using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Endpoints;

public static class IntegrationEndpoints
{
    public const string SupplyarrDemandStatusIngestActionScope = "maintainarr.demand_status.write";

    public const string RoutarrAssetReadinessDispatchActionScope = "maintainarr.asset_readiness.dispatch_gate";
    public const string AssetReadinessReadActionScope = "maintainarr.asset_readiness.read";
    public const string RoutarrEventIngestActionScope = "maintainarr.routarr_events.ingest";

    public const string StaffarrPersonSyncActionScope = "maintainarr.technician_refs.sync";

    public static void MapMaintainArrIntegrationEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder integrations, string nameSuffix)
        {
            integrations = integrations.WithTags("Integrations");

        integrations.MapGet("/", (
            HttpContext context,
            StlServiceTokenValidator tokenValidator) =>
        {
            var bearer = ServiceTokenBearerParser.ParseAuthorizationHeader(
                context.Request.Headers.Authorization.ToString());
            var preview = tokenValidator.TryValidate(bearer)
                ?? throw new StlApiException(
                    "auth.service_token_invalid",
                    "Service token is invalid.",
                    401);

            tokenValidator.ValidateOrThrow(
                bearer,
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = preview.SourceProductKey,
                    RequiredTargetProduct = "maintainarr",
                    TenantId = preview.TenantScope ?? Guid.Empty
                });

            return Results.Ok(new
            {
                items = new[]
                {
                    new { key = "asset-readiness", path = "/api/v1/integrations/asset-readiness" },
                    new { key = "routarr-asset-readiness", path = "/api/v1/integrations/routarr-asset-readiness" },
                    new { key = "routarr-events", path = "/api/v1/integrations/routarr-events" },
                    new { key = "supplyarr-demand-status", path = "/api/v1/integrations/supplyarr-demand-status" },
                    new { key = "staffarr-person-sync", path = "/api/v1/integrations/staffarr-person-sync" },
                }
            });
        })
        .WithName($"ListMaintainArrIntegrationEndpoints{nameSuffix}");

        integrations.MapGet("/routarr-asset-readiness", async (
            Guid tenantId,
            Guid? assetId,
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
                assetId,
                vehicleRefKey,
                assetTag,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName($"RoutarrAssetReadinessCheck{nameSuffix}");

        integrations.MapGet("/asset-readiness", async (
            Guid tenantId,
            Guid? assetId,
            string? vehicleRefKey,
            string? assetTag,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            AssetReadinessService service,
            CancellationToken cancellationToken) =>
        {
            ValidateAssetReadinessServiceToken(tokenValidator, context, tenantId);

            var result = await service.GetByDispatchRefAsync(
                tenantId,
                assetId,
                vehicleRefKey,
                assetTag,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName($"AssetReadinessIntegration{nameSuffix}");

        integrations.MapPost("/routarr-events", async (
            IngestRoutarrEventRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            RoutarrEventIngestionService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "routarr",
                    RequiredTargetProduct = "maintainarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = RoutarrEventIngestActionScope
                });

            var result = await service.IngestAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName($"IngestRoutarrEvent{nameSuffix}");

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
        .WithName($"IngestSupplyarrDemandStatus{nameSuffix}");

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
        .WithName($"IngestStaffarrPersonSync{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/integrations"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/integrations"), "V1");
    }

    private static void ValidateAssetReadinessServiceToken(
        StlServiceTokenValidator tokenValidator,
        HttpContext context,
        Guid tenantId)
    {
        var bearer = ServiceTokenBearerParser.ParseAuthorizationHeader(
            context.Request.Headers.Authorization.ToString());
        var preview = tokenValidator.TryValidate(bearer)
            ?? throw new StlApiException(
                "auth.service_token_invalid",
                "Service token is invalid.",
                401);

        var source = preview.SourceProductKey?.Trim().ToLowerInvariant();
        if (source is not "maintainarr" and not "routarr" and not "staffarr" and not "supplyarr" and not "trainarr" and not "compliancecore")
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Service token source product is not authorized for asset readiness reads.",
                403);
        }

        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = source,
                RequiredTargetProduct = "maintainarr",
                TenantId = tenantId,
                RequiredActionScope = AssetReadinessReadActionScope
            });
    }
}
