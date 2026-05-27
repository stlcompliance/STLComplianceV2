using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class TrainingEvidenceEndpoints
{
    public static void MapTrainArrTrainingEvidenceEndpoints(this WebApplication app)
    {
        var evidence = app.MapGroup("/api/training-assignments/{assignmentId:guid}/evidence")
            .WithTags("TrainingEvidence")
            .RequireAuthorization();

        evidence.MapGet("/", async (
            Guid assignmentId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingAssignmentService assignmentService,
            TrainingEvidenceService evidenceService,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            var assignment = await assignmentService.GetAsync(tenantId, assignmentId, cancellationToken);
            authorization.RequireEvidenceRead(context.User, assignment.StaffarrPersonId);
            return Results.Ok(await evidenceService.ListForAssignmentAsync(tenantId, assignmentId, cancellationToken));
        })
        .WithName("ListTrainingEvidence");

        evidence.MapPost("/", async (
            Guid assignmentId,
            CreateTrainingEvidenceRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingAssignmentService assignmentService,
            TrainingEvidenceService evidenceService,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var assignment = await assignmentService.GetAsync(tenantId, assignmentId, cancellationToken);
            authorization.RequireEvidenceUpload(context.User, assignment.StaffarrPersonId);
            var created = await evidenceService.CreateForAssignmentAsync(
                tenantId,
                actorUserId,
                assignmentId,
                request,
                cancellationToken);
            return Results.Created($"/api/training-assignments/{assignmentId}/evidence/{created.EvidenceId}", created);
        })
        .WithName("CreateTrainingEvidence");
    }
}
