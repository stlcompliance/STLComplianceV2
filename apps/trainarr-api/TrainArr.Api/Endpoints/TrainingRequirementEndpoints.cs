using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class TrainingRequirementEndpoints
{
    public static void MapTrainArrTrainingRequirementEndpoints(this WebApplication app)
    {
        MapRoutes(
            app.MapGroup("/api/training-requirements")
                .WithTags("TrainingRequirements")
                .RequireAuthorization(),
            string.Empty);
        MapRoutes(
            app.MapGroup("/api/v1/training-requirements")
                .WithTags("TrainingRequirements")
                .RequireAuthorization(),
            "V1TrainingRequirements");
        MapRoutes(
            app.MapGroup("/api/v1/requirements")
                .WithTags("TrainingRequirements")
                .RequireAuthorization(),
            "V1Requirements");
    }

    private static void MapRoutes(RouteGroupBuilder requirements, string nameSuffix)
    {
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
        .WithName($"ListTrainingRequirements{nameSuffix}");

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
        .WithName($"GetTrainingRequirementBuilderView{nameSuffix}");

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
        .WithName($"CreateTrainingRequirement{nameSuffix}");

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
        .WithName($"UpdateTrainingRequirement{nameSuffix}");

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
        .WithName($"DeleteTrainingRequirement{nameSuffix}");

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
        .WithName($"SyncTrainingRequirementToMatrix{nameSuffix}");
    }
}
