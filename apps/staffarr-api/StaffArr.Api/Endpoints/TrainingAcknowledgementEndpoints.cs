using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class TrainingAcknowledgementEndpoints
{
    public static void MapStaffArrTrainingAcknowledgementEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/training-acknowledgements")
            .WithTags("TrainingAcknowledgements")
            .RequireAuthorization();

        group.MapGet("/", async (
            Guid? personId,
            string? status,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            TrainingAcknowledgementService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingAcknowledgementRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, personId, status, cancellationToken));
        })
        .WithName("ListTrainingAcknowledgements");

        group.MapPost("/{acknowledgementId:guid}/acknowledge", async (
            Guid acknowledgementId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            TrainingAcknowledgementService service,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            var preview = await service.GetAsync(tenantId, acknowledgementId, cancellationToken);
            authorization.RequireTrainingAcknowledgementAcknowledge(context.User, preview.PersonId);
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.AcknowledgeAsync(
                tenantId,
                actorUserId,
                acknowledgementId,
                cancellationToken));
        })
        .WithName("AcknowledgeTrainingAssignment");
    }
}
