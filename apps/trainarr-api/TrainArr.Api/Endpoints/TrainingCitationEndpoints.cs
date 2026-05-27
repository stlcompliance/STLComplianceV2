using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class TrainingCitationEndpoints
{
    public static void MapTrainArrTrainingCitationEndpoints(this WebApplication app)
    {
        MapForEntity(app, TrainingCitationEntityTypes.TrainingDefinition, "training-definitions");
        MapForEntity(app, TrainingCitationEntityTypes.TrainingProgram, "training-programs");
        MapForEntity(app, TrainingCitationEntityTypes.TrainingAssignment, "training-assignments");
    }

    private static void MapForEntity(WebApplication app, string entityType, string routePrefix)
    {
        var group = app.MapGroup($"/api/{routePrefix}/{{entityId:guid}}/citations")
            .WithTags("TrainingCitations")
            .RequireAuthorization();

        group.MapGet("/", async (
            Guid entityId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingCitationService service,
            CancellationToken cancellationToken,
            bool includeMetadata = false) =>
        {
            authorization.RequireCitationsRead(context.User, entityType);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(
                tenantId,
                entityType,
                entityId,
                includeMetadata,
                cancellationToken));
        })
        .WithName($"List{routePrefix}Citations");

        group.MapPost("/", async (
            Guid entityId,
            AttachTrainingCitationRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingCitationService service,
            CancellationToken cancellationToken,
            bool validateWithComplianceCore = false) =>
        {
            authorization.RequireCitationsManage(context.User, entityType);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.AttachAsync(
                tenantId,
                actorUserId,
                entityType,
                entityId,
                request,
                validateWithComplianceCore,
                cancellationToken);
            return Results.Created(
                $"/api/{routePrefix}/{entityId}/citations/{created.AttachmentId}",
                created);
        })
        .WithName($"Attach{routePrefix}Citation");

        group.MapDelete("/{attachmentId:guid}", async (
            Guid entityId,
            Guid attachmentId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingCitationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireCitationsManage(context.User, entityType);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            await service.RemoveAsync(
                tenantId,
                actorUserId,
                entityType,
                entityId,
                attachmentId,
                cancellationToken);
            return Results.NoContent();
        })
        .WithName($"Remove{routePrefix}Citation");
    }
}
