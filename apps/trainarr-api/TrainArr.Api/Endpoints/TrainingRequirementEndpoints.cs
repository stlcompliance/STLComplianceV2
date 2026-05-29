using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class TrainingRequirementEndpoints
{
    public static void MapTrainArrTrainingRequirementEndpoints(this WebApplication app)
    {
        var requirements = app.MapGroup("/api/training-requirements")
            .WithTags("TrainingRequirements")
            .RequireAuthorization();

        requirements.MapGet("/", async (
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingRequirementService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingProgramsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, cancellationToken));
        })
        .WithName("ListTrainingRequirements");

        requirements.MapGet("/builder-view", async (
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingRequirementService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingProgramsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetBuilderViewAsync(tenantId, cancellationToken));
        })
        .WithName("GetTrainingRequirementBuilderView");

        requirements.MapPost("/", async (
            CreateTrainingRequirementRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingRequirementService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingProgramsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/training-requirements/{created.RequirementId}", created);
        })
        .WithName("CreateTrainingRequirement");

        requirements.MapPut("/{requirementId:guid}", async (
            Guid requirementId,
            UpdateTrainingRequirementRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingRequirementService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingProgramsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(
                await service.UpdateAsync(tenantId, actorUserId, requirementId, request, cancellationToken));
        })
        .WithName("UpdateTrainingRequirement");

        requirements.MapDelete("/{requirementId:guid}", async (
            Guid requirementId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingRequirementService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingProgramsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            await service.DeleteAsync(tenantId, actorUserId, requirementId, cancellationToken);
            return Results.NoContent();
        })
        .WithName("DeleteTrainingRequirement");

        requirements.MapPost("/sync-to-matrix", async (
            SyncRequirementToMatrixRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingRequirementService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingProgramsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(
                await service.SyncToMatrixAsync(tenantId, actorUserId, request.RequirementId, cancellationToken));
        })
        .WithName("SyncTrainingRequirementToMatrix");
    }
}
