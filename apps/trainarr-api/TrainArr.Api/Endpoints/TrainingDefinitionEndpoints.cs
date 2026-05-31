using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class TrainingDefinitionEndpoints
{
    public static void MapTrainArrTrainingDefinitionEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/training-definitions"), string.Empty, "/api/training-definitions");
        MapRoutes(app.MapGroup("/api/v1/training-definitions"), "V1", "/api/v1/training-definitions");
    }

    private static void MapRoutes(RouteGroupBuilder definitions, string nameSuffix, string pathPrefix)
    {
        definitions = definitions
            .WithTags("TrainingDefinitions")
            .RequireAuthorization();

        definitions.MapGet("/", async (
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingDefinitionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingDefinitionsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, cancellationToken));
        })
        .WithName($"ListTrainingDefinitions{nameSuffix}");

        definitions.MapPost("/", async (
            CreateTrainingDefinitionRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingDefinitionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingDefinitionsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var created = await service.CreateAsync(tenantId, request, cancellationToken);
            return Results.Created($"{pathPrefix}/{created.TrainingDefinitionId}", created);
        })
        .WithName($"CreateTrainingDefinition{nameSuffix}");
    }
}
