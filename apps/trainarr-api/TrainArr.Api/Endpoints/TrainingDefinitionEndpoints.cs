using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class TrainingDefinitionEndpoints
{
    public static void MapTrainArrTrainingDefinitionEndpoints(this WebApplication app)
    {
        var definitions = app.MapGroup("/api/training-definitions")
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
        .WithName("ListTrainingDefinitions");

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
            return Results.Created($"/api/training-definitions/{created.TrainingDefinitionId}", created);
        })
        .WithName("CreateTrainingDefinition");
    }
}
