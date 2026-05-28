using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class IntegrationEndpoints
{
    public const string SupplyarrDemandStatusIngestActionScope = "routarr.demand_status.write";

    public static void MapRoutArrIntegrationEndpoints(this WebApplication app)
    {
        var integrations = app.MapGroup("/api/integrations").WithTags("Integrations");

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
        .WithName("IngestSupplyarrDemandStatus");
    }
}
