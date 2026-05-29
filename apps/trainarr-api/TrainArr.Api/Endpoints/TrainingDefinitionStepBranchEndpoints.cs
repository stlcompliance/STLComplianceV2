using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Endpoints;

public static class TrainingDefinitionStepBranchEndpoints
{
    public static void MapTrainArrTrainingDefinitionStepBranchEndpoints(this WebApplication app)
    {
        var catalog = app.MapGroup("/api/training-step-branches")
            .WithTags("TrainingStepBranches")
            .RequireAuthorization();

        catalog.MapGet("/catalog", (
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingDefinitionStepBranchService service) =>
        {
            authorization.RequireTrainingDefinitionsRead(context.User);
            return Results.Ok(service.ListCatalog());
        })
        .WithName("ListTrainingStepBranchCatalog");

        var branches = app.MapGroup(
                "/api/training-definitions/{trainingDefinitionId:guid}/steps/{stepId:guid}/branches")
            .WithTags("TrainingStepBranches")
            .RequireAuthorization();

        branches.MapGet("/", async (
            Guid trainingDefinitionId,
            Guid stepId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingDefinitionStepBranchService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingDefinitionsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListForStepAsync(
                tenantId,
                trainingDefinitionId,
                stepId,
                cancellationToken));
        })
        .WithName("ListTrainingDefinitionStepBranches");

        branches.MapPost("/", async (
            Guid trainingDefinitionId,
            Guid stepId,
            CreateTrainingDefinitionStepBranchRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingDefinitionStepBranchService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingDefinitionsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(
                tenantId,
                actorUserId,
                trainingDefinitionId,
                stepId,
                request,
                cancellationToken);
            return Results.Created(
                $"/api/training-definitions/{trainingDefinitionId}/steps/{stepId}/branches/{created.BranchId}",
                created);
        })
        .WithName("CreateTrainingDefinitionStepBranch");

        branches.MapDelete("/{branchId:guid}", async (
            Guid trainingDefinitionId,
            Guid stepId,
            Guid branchId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingDefinitionStepBranchService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingDefinitionsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            await service.DeleteAsync(
                tenantId,
                actorUserId,
                trainingDefinitionId,
                stepId,
                branchId,
                cancellationToken);
            return Results.NoContent();
        })
        .WithName("DeleteTrainingDefinitionStepBranch");
    }
}
