using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Endpoints;

public static class TrainingDefinitionStepEndpoints
{
    public static void MapTrainArrTrainingDefinitionStepEndpoints(this WebApplication app)
    {
        var steps = app.MapGroup("/api/training-definitions/{trainingDefinitionId:guid}/steps")
            .WithTags("TrainingDefinitionSteps")
            .RequireAuthorization();

        steps.MapGet("/", async (
            Guid trainingDefinitionId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingDefinitionStepService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingDefinitionsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListForDefinitionAsync(tenantId, trainingDefinitionId, cancellationToken));
        })
        .WithName("ListTrainingDefinitionSteps");

        steps.MapPost("/", async (
            Guid trainingDefinitionId,
            CreateTrainingDefinitionStepRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingDefinitionStepService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingDefinitionsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(
                tenantId,
                actorUserId,
                trainingDefinitionId,
                request,
                cancellationToken);
            return Results.Created(
                $"/api/training-definitions/{trainingDefinitionId}/steps/{created.StepId}",
                created);
        })
        .WithName("CreateTrainingDefinitionStep");

        steps.MapPut("/{stepId:guid}", async (
            Guid trainingDefinitionId,
            Guid stepId,
            UpdateTrainingDefinitionStepRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingDefinitionStepService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingDefinitionsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateAsync(
                tenantId,
                actorUserId,
                trainingDefinitionId,
                stepId,
                request,
                cancellationToken));
        })
        .WithName("UpdateTrainingDefinitionStep");

        steps.MapDelete("/{stepId:guid}", async (
            Guid trainingDefinitionId,
            Guid stepId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingDefinitionStepService service,
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
                cancellationToken);
            return Results.NoContent();
        })
        .WithName("DeleteTrainingDefinitionStep");

        var assignmentSteps = app.MapGroup("/api/training-assignments/{assignmentId:guid}/steps")
            .WithTags("TrainingAssignmentSteps")
            .RequireAuthorization();

        assignmentSteps.MapGet("/", async (
            Guid assignmentId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingAssignmentService assignmentService,
            TrainingDefinitionStepService stepService,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            var assignment = await assignmentService.LoadAssignmentEntityAsync(tenantId, assignmentId, cancellationToken);
            authorization.RequireAssignmentsRead(context.User, assignment.StaffarrPersonId);
            return Results.Ok(await stepService.ListAssignmentProgressAsync(tenantId, assignment, cancellationToken));
        })
        .WithName("ListTrainingAssignmentSteps");

        assignmentSteps.MapPost("/{stepId:guid}/submit", async (
            Guid assignmentId,
            Guid stepId,
            SubmitTrainingAssignmentStepRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingAssignmentService assignmentService,
            TrainingDefinitionStepService stepService,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var assignment = await assignmentService.LoadAssignmentEntityAsync(tenantId, assignmentId, cancellationToken);
            authorization.RequireAssignmentsComplete(context.User, assignment.StaffarrPersonId);

            var isEvaluator = false;
            try
            {
                authorization.RequireEvaluationSubmit(context.User);
                isEvaluator = true;
            }
            catch (StlApiException)
            {
                isEvaluator = false;
            }

            return Results.Ok(await stepService.SubmitAssignmentStepAsync(
                tenantId,
                actorUserId,
                assignment,
                stepId,
                request,
                isEvaluator,
                cancellationToken));
        })
        .WithName("SubmitTrainingAssignmentStep");
    }
}
