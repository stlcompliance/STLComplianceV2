using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class TrainingAssignmentMaterialDemandEndpoints
{
    public static void MapTrainArrTrainingAssignmentMaterialDemandEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/training-assignments/{assignmentId:guid}/material-demand")
            .WithTags("TrainingAssignmentMaterialDemand")
            .RequireAuthorization();

        group.MapGet("/", async (
            Guid assignmentId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingAssignmentService assignmentService,
            TrainingAssignmentMaterialDemandService materialDemandService,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            var assignment = await assignmentService.GetAsync(tenantId, assignmentId, cancellationToken);
            authorization.RequireAssignmentsRead(context.User, assignment.StaffarrPersonId);
            return Results.Ok(await materialDemandService.ListAsync(tenantId, assignmentId, cancellationToken));
        })
        .WithName("ListTrainingAssignmentMaterialDemand");

        group.MapGet("/status-events", async (
            Guid assignmentId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingAssignmentService assignmentService,
            TrainingAssignmentMaterialDemandService materialDemandService,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            var assignment = await assignmentService.GetAsync(tenantId, assignmentId, cancellationToken);
            authorization.RequireAssignmentsRead(context.User, assignment.StaffarrPersonId);
            return Results.Ok(await materialDemandService.ListStatusEventsAsync(
                tenantId,
                assignmentId,
                cancellationToken));
        })
        .WithName("ListTrainingAssignmentMaterialDemandStatusEvents");

        group.MapPost("/", async (
            Guid assignmentId,
            CreateTrainingAssignmentMaterialDemandLineRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingAssignmentService assignmentService,
            TrainingAssignmentMaterialDemandService materialDemandService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssignmentsCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var assignment = await assignmentService.GetAsync(tenantId, assignmentId, cancellationToken);
            authorization.RequireAssignmentsRead(context.User, assignment.StaffarrPersonId);
            var created = await materialDemandService.CreateAsync(
                tenantId,
                actorUserId,
                assignmentId,
                request,
                cancellationToken);
            return Results.Created($"/api/training-assignments/{assignmentId}/material-demand/{created.DemandLineId}", created);
        })
        .WithName("CreateTrainingAssignmentMaterialDemandLine");

        group.MapPost("/publish", async (
            Guid assignmentId,
            PublishTrainingAssignmentMaterialDemandRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingAssignmentService assignmentService,
            TrainingAssignmentMaterialDemandService materialDemandService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssignmentsCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var assignment = await assignmentService.GetAsync(tenantId, assignmentId, cancellationToken);
            authorization.RequireAssignmentsRead(context.User, assignment.StaffarrPersonId);
            return Results.Ok(await materialDemandService.PublishAsync(
                tenantId,
                actorUserId,
                assignmentId,
                request,
                cancellationToken));
        })
        .WithName("PublishTrainingAssignmentMaterialDemand");
    }
}
