using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class IntegrationEndpoints
{
    public const string MaintainarrDemandIngestActionScope = "supplyarr.demand_intake.write";

    public static void MapSupplyArrIntegrationEndpoints(this WebApplication app)
    {
        var integrations = app.MapGroup("/api/integrations").WithTags("Integrations");

        integrations.MapPost("/maintainarr-demand", async (
            IngestMaintainarrDemandRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            MaintainArrDemandIntakeService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "maintainarr",
                    RequiredTargetProduct = "supplyarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = MaintainarrDemandIngestActionScope
                });

            var result = await service.IngestAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("IngestMaintainarrDemand");
    }
}
