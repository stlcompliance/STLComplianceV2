using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class TrainingAssignmentLaborEndpoints
{
    public static void MapTrainArrTrainingAssignmentLaborEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/training-assignments/{assignmentId:guid}/labor")
            .WithTags("TrainingAssignmentLabor")
            .RequireAuthorization();

        group.MapGet("/", async (
            Guid assignmentId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingAssignmentService assignmentService,
            TrainingAssignmentLaborService laborService,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            var assignment = await assignmentService.GetAsync(tenantId, assignmentId, cancellationToken);
            authorization.RequireAssignmentsRead(context.User, assignment.StaffarrPersonId);
            return Results.Ok(await laborService.ListAsync(tenantId, assignmentId, cancellationToken));
        })
        .WithName("ListTrainingAssignmentLabor");

        group.MapPost("/", async (
            Guid assignmentId,
            CreateTrainingAssignmentLaborEntryRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingAssignmentService assignmentService,
            TrainingAssignmentLaborService laborService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssignmentsCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var assignment = await assignmentService.GetAsync(tenantId, assignmentId, cancellationToken);
            authorization.RequireAssignmentsRead(context.User, assignment.StaffarrPersonId);
            var created = await laborService.CreateAsync(
                tenantId,
                actorUserId,
                assignmentId,
                request,
                cancellationToken);
            return Results.Created($"/api/training-assignments/{assignmentId}/labor/{created.LaborEntryId}", created);
        })
        .WithName("CreateTrainingAssignmentLabor");

        group.MapDelete("/{laborEntryId:guid}", async (
            Guid assignmentId,
            Guid laborEntryId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingAssignmentService assignmentService,
            TrainingAssignmentLaborService laborService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssignmentsCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var assignment = await assignmentService.GetAsync(tenantId, assignmentId, cancellationToken);
            authorization.RequireAssignmentsRead(context.User, assignment.StaffarrPersonId);
            await laborService.RemoveAsync(
                tenantId,
                actorUserId,
                assignmentId,
                laborEntryId,
                cancellationToken);
            return Results.NoContent();
        })
        .WithName("RemoveTrainingAssignmentLabor");
    }
}
