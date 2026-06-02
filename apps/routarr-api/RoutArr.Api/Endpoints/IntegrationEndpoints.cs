using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class IntegrationEndpoints
{
    public const string SupplyarrDemandStatusIngestActionScope = "routarr.demand_status.write";
    public const string SupplyarrShipmentCreateActionScope = "routarr.shipments.create";

    public static void MapRoutArrIntegrationEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder integrations, string nameSuffix)
        {
            integrations = integrations.WithTags("Integrations");

            integrations.MapPost("/supplyarr-demand-status", async (
            IngestSupplyarrDemandStatusRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            TripPartsDemandStatusIngestionService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "supplyarr",
                    RequiredTargetProduct = "routarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = SupplyarrDemandStatusIngestActionScope
                });

            var result = await service.IngestAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName($"IngestSupplyarrDemandStatus{nameSuffix}");

            integrations.MapPost("/supplyarr-shipments", async (
            CreateSupplyArrShipmentIntentRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            SupplyArrShipmentIntentService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "supplyarr",
                    RequiredTargetProduct = "routarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = SupplyarrShipmentCreateActionScope
                });

            var result = await service.CreateAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName($"CreateSupplyarrShipmentIntent{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/integrations"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/integrations"), "V1");
    }
}
