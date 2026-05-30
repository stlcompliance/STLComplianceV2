using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class TrainingAssignmentEndpoints
{
    public static void MapTrainArrTrainingAssignmentEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/training-assignments").WithTags("TrainingAssignments").RequireAuthorization(), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/training-assignments").WithTags("TrainingAssignments").RequireAuthorization(), "V1TrainingAssignments");
        MapRoutes(app.MapGroup("/api/v1/assignments").WithTags("TrainingAssignments").RequireAuthorization(), "V1Assignments");
    }

    private static void MapRoutes(RouteGroupBuilder assignments, string nameSuffix)
    {
        assignments.MapGet("/", async (
            Guid? staffarrPersonId,
            Guid? staffarrIncidentRemediationId,
            string? status,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingAssignmentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssignmentsRead(context.User, staffarrPersonId);
            var tenantId = context.User.GetTenantId();
            var effectivePersonId = staffarrPersonId;
            if (effectivePersonId is null
                && !context.User.IsPlatformAdmin()
                && string.Equals(context.User.GetTenantRoleKey(), "tenant_member", StringComparison.OrdinalIgnoreCase))
            {
                effectivePersonId = context.User.GetPersonId();
            }

            return Results.Ok(await service.ListAsync(
                tenantId,
                effectivePersonId,
                staffarrIncidentRemediationId,
                status,
                cancellationToken));
        })
        .WithName($"ListTrainingAssignments{nameSuffix}");

        assignments.MapPost("/", async (
            CreateTrainingAssignmentRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingAssignmentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssignmentsCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/training-assignments/{created.AssignmentId}", created);
        })
        .WithName($"CreateTrainingAssignment{nameSuffix}");

        assignments.MapGet("/{assignmentId:guid}", async (
            Guid assignmentId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingAssignmentService service,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            var detail = await service.GetAsync(tenantId, assignmentId, cancellationToken);
            authorization.RequireAssignmentsRead(context.User, detail.StaffarrPersonId);
            return Results.Ok(detail);
        })
        .WithName($"GetTrainingAssignment{nameSuffix}");

        assignments.MapPost("/{assignmentId:guid}/complete", async (
            Guid assignmentId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingAssignmentService service,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var existing = await service.GetAsync(tenantId, assignmentId, cancellationToken);
            authorization.RequireAssignmentsComplete(context.User, existing.StaffarrPersonId);
            var completed = await service.CompleteAsync(tenantId, actorUserId, assignmentId, cancellationToken);
            return Results.Ok(completed);
        })
        .WithName($"CompleteTrainingAssignment{nameSuffix}");
    }
}
