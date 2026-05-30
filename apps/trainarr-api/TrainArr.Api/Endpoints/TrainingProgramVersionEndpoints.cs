using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class TrainingProgramVersionEndpoints
{
    public static void MapTrainArrTrainingProgramVersionEndpoints(this WebApplication app)
    {
        MapRoutes(
            app.MapGroup("/api/program-versions")
                .WithTags("ProgramVersions")
                .RequireAuthorization(),
            string.Empty);

        MapRoutes(
            app.MapGroup("/api/v1/program-versions")
                .WithTags("ProgramVersions")
                .RequireAuthorization(),
            "V1");
    }

    private static void MapRoutes(RouteGroupBuilder versions, string routeSuffix)
    {
        var nameSuffix = string.IsNullOrWhiteSpace(routeSuffix) ? string.Empty : routeSuffix;

        versions.MapGet("/", async (
            Guid? programId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingProgramVersionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingProgramsRead(context.User);
            if (programId is null || programId == Guid.Empty)
            {
                return Results.BadRequest(new { code = "program_versions.validation", message = "programId is required." });
            }

            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListForProgramAsync(tenantId, programId.Value, cancellationToken));
        })
        .WithName($"ListTrainingProgramVersions{nameSuffix}");

        versions.MapGet("/{programVersionId:guid}", async (
            Guid programVersionId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingProgramVersionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingProgramsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, programVersionId, cancellationToken));
        })
        .WithName($"GetTrainingProgramVersion{nameSuffix}");

        versions.MapPost("/start-revision", async (
            StartProgramRevisionRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingProgramVersionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingProgramsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.StartRevisionAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"StartTrainingProgramRevision{nameSuffix}");
    }
}
