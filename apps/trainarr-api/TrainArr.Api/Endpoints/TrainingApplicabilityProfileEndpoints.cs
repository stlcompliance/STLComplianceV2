using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class TrainingApplicabilityProfileEndpoints
{
    public static void MapTrainArrTrainingApplicabilityProfileEndpoints(this WebApplication app)
    {
        var profiles = app.MapGroup("/api/applicability-profiles")
            .WithTags("TrainingApplicability")
            .RequireAuthorization();

        profiles.MapGet("/", async (
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingApplicabilityProfileService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingProgramsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, cancellationToken));
        })
        .WithName("ListTrainingApplicabilityProfiles");

        profiles.MapPost("/", async (
            CreateTrainingApplicabilityProfileRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingApplicabilityProfileService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingProgramsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/applicability-profiles/{created.ApplicabilityProfileId}", created);
        })
        .WithName("CreateTrainingApplicabilityProfile");

        profiles.MapPut("/{profileId:guid}", async (
            Guid profileId,
            UpdateTrainingApplicabilityProfileRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingApplicabilityProfileService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingProgramsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateAsync(tenantId, actorUserId, profileId, request, cancellationToken));
        })
        .WithName("UpdateTrainingApplicabilityProfile");

        profiles.MapDelete("/{profileId:guid}", async (
            Guid profileId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingApplicabilityProfileService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingProgramsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            await service.DeleteAsync(tenantId, actorUserId, profileId, cancellationToken);
            return Results.NoContent();
        })
        .WithName("DeleteTrainingApplicabilityProfile");
    }
}
