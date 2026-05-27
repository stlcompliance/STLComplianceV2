using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class TrainingEvaluationEndpoints
{
    public static void MapTrainArrTrainingEvaluationEndpoints(this WebApplication app)
    {
        var evaluations = app.MapGroup("/api/evaluations")
            .WithTags("Evaluations")
            .RequireAuthorization();

        evaluations.MapGet("/", async (
            Guid? trainingAssignmentId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingAssignmentService assignmentService,
            TrainingEvaluationService evaluationService,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            if (trainingAssignmentId is Guid assignmentId)
            {
                var assignment = await assignmentService.GetAsync(tenantId, assignmentId, cancellationToken);
                authorization.RequireEvaluationsRead(context.User, assignment.StaffarrPersonId);
            }
            else
            {
                authorization.RequireTrainArrEntitlement(context.User);
            }

            return Results.Ok(await evaluationService.ListForAssignmentAsync(
                tenantId,
                trainingAssignmentId,
                cancellationToken));
        })
        .WithName("ListTrainingEvaluations");

        evaluations.MapPost("/", async (
            SubmitTrainingEvaluationRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingAssignmentService assignmentService,
            TrainingEvaluationService evaluationService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEvaluationSubmit(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var assignment = await assignmentService.GetAsync(tenantId, request.TrainingAssignmentId, cancellationToken);
            authorization.RequireEvaluationsRead(context.User, assignment.StaffarrPersonId);
            var created = await evaluationService.SubmitAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/evaluations?trainingAssignmentId={created.TrainingAssignmentId}", created);
        })
        .WithName("SubmitTrainingEvaluation");

        var nested = app.MapGroup("/api/training-assignments/{assignmentId:guid}/evaluations")
            .WithTags("Evaluations")
            .RequireAuthorization();

        nested.MapGet("/", async (
            Guid assignmentId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingAssignmentService assignmentService,
            TrainingEvaluationService evaluationService,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            var assignment = await assignmentService.GetAsync(tenantId, assignmentId, cancellationToken);
            authorization.RequireEvaluationsRead(context.User, assignment.StaffarrPersonId);
            var items = await evaluationService.ListForAssignmentAsync(tenantId, assignmentId, cancellationToken);
            return Results.Ok(items);
        })
        .WithName("ListTrainingEvaluationsForAssignment");

        nested.MapPost("/", async (
            Guid assignmentId,
            SubmitTrainingEvaluationRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingAssignmentService assignmentService,
            TrainingEvaluationService evaluationService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEvaluationSubmit(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var assignment = await assignmentService.GetAsync(tenantId, assignmentId, cancellationToken);
            authorization.RequireEvaluationsRead(context.User, assignment.StaffarrPersonId);
            if (request.TrainingAssignmentId != assignmentId)
            {
                return Results.BadRequest(new { code = "evaluations.assignment_mismatch", message = "Assignment id in route and body must match." });
            }

            var created = await evaluationService.SubmitAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/training-assignments/{assignmentId}/evaluations", created);
        })
        .WithName("SubmitTrainingEvaluationForAssignment");
    }
}
