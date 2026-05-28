using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class IntegrationEndpoints
{
    public const string MaintainarrDemandIngestActionScope = "supplyarr.demand_intake.write";

    public const string RoutarrDemandIngestActionScope = "supplyarr.demand_intake.write";

    public const string TrainarrDemandIngestActionScope = "supplyarr.demand_intake.write";

    public const string StaffarrDemandIngestActionScope = "supplyarr.demand_intake.write";

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

        integrations.MapPost("/routarr-demand", async (
            IngestRoutarrDemandRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            RoutArrDemandIntakeService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "routarr",
                    RequiredTargetProduct = "supplyarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = RoutarrDemandIngestActionScope
                });

            var result = await service.IngestAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("IngestRoutarrDemand");

        integrations.MapPost("/trainarr-demand", async (
            IngestTrainarrDemandRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            TrainArrDemandIntakeService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "trainarr",
                    RequiredTargetProduct = "supplyarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = TrainarrDemandIngestActionScope
                });

            var result = await service.IngestAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("IngestTrainarrDemand");

        integrations.MapPost("/staffarr-demand", async (
            IngestStaffarrDemandRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            StaffArrDemandIntakeService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "staffarr",
                    RequiredTargetProduct = "supplyarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = StaffarrDemandIngestActionScope
                });

            var result = await service.IngestAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("IngestStaffarrDemand");
    }
}
