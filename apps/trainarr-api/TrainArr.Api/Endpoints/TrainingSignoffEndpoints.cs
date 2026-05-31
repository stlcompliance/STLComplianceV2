using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class TrainingSignoffEndpoints
{
    public static void MapTrainArrTrainingSignoffEndpoints(this WebApplication app)
    {
        MapTopLevelRoutes(
            app.MapGroup("/api/signoffs")
                .WithTags("Signoffs")
                .RequireAuthorization(),
            string.Empty,
            "/api/signoffs");
        MapTopLevelRoutes(
            app.MapGroup("/api/v1/signoffs")
                .WithTags("Signoffs")
                .RequireAuthorization(),
            "V1Signoffs",
            "/api/v1/signoffs");

        var nested = app.MapGroup("/api/training-assignments/{assignmentId:guid}/signoffs")
            .WithTags("Signoffs")
            .RequireAuthorization();

        nested.MapGet("/", async (
            Guid assignmentId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingAssignmentService assignmentService,
            TrainingSignoffService signoffService,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            var assignment = await assignmentService.GetAsync(tenantId, assignmentId, cancellationToken);
            authorization.RequireSignoffsRead(context.User, assignment.StaffarrPersonId);
            return Results.Ok(await signoffService.ListForAssignmentAsync(tenantId, assignmentId, cancellationToken));
        })
        .WithName("ListTrainingSignoffsForAssignment");

        nested.MapPost("/", async (
            Guid assignmentId,
            SubmitTrainingSignoffRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingAssignmentService assignmentService,
            TrainingSignoffService signoffService,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var assignment = await assignmentService.GetAsync(tenantId, assignmentId, cancellationToken);
            authorization.RequireSignoffSubmit(context.User, assignment.StaffarrPersonId, request.SignoffRole);
            if (request.TrainingAssignmentId != assignmentId)
            {
                return Results.BadRequest(new { code = "signoffs.assignment_mismatch", message = "Assignment id in route and body must match." });
            }

            var created = await signoffService.SubmitAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/training-assignments/{assignmentId}/signoffs", created);
        })
        .WithName("SubmitTrainingSignoffForAssignment");
    }

    private static void MapTopLevelRoutes(RouteGroupBuilder signoffs, string nameSuffix, string routePrefix)
    {
        signoffs.MapGet("/", async (
            Guid? trainingAssignmentId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingAssignmentService assignmentService,
            TrainingSignoffService signoffService,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            if (trainingAssignmentId is Guid assignmentId)
            {
                var assignment = await assignmentService.GetAsync(tenantId, assignmentId, cancellationToken);
                authorization.RequireSignoffsRead(context.User, assignment.StaffarrPersonId);
            }
            else
            {
                authorization.RequireTrainArrEntitlement(context.User);
            }

            return Results.Ok(await signoffService.ListForAssignmentAsync(
                tenantId,
                trainingAssignmentId,
                cancellationToken));
        })
        .WithName($"ListTrainingSignoffs{nameSuffix}");

        signoffs.MapPost("/", async (
            SubmitTrainingSignoffRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingAssignmentService assignmentService,
            TrainingSignoffService signoffService,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var assignment = await assignmentService.GetAsync(tenantId, request.TrainingAssignmentId, cancellationToken);
            authorization.RequireSignoffSubmit(context.User, assignment.StaffarrPersonId, request.SignoffRole);
            var created = await signoffService.SubmitAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"{routePrefix}?trainingAssignmentId={created.TrainingAssignmentId}", created);
        })
        .WithName($"SubmitTrainingSignoff{nameSuffix}");
    }
}
