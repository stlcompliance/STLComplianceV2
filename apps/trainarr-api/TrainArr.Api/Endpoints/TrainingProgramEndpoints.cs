using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class TrainingProgramEndpoints
{
    public static void MapTrainArrTrainingProgramEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/training-programs").WithTags("TrainingPrograms").RequireAuthorization(), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/training-programs").WithTags("TrainingPrograms").RequireAuthorization(), "V1TrainingPrograms");
        MapRoutes(app.MapGroup("/api/v1/programs").WithTags("TrainingPrograms").RequireAuthorization(), "V1Programs");
    }

    private static void MapRoutes(RouteGroupBuilder programs, string nameSuffix)
    {
        programs.MapGet("/", async (
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingProgramService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingProgramsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, cancellationToken));
        })
        .WithName($"ListTrainingPrograms{nameSuffix}");

        programs.MapPost("/", async (
            CreateTrainingProgramRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingProgramService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingProgramsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/training-programs/{created.ProgramId}", created);
        })
        .WithName($"CreateTrainingProgram{nameSuffix}");

        programs.MapGet("/{programId:guid}", async (
            Guid programId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingProgramService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingProgramsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, programId, cancellationToken));
        })
        .WithName($"GetTrainingProgram{nameSuffix}");

        programs.MapPut("/{programId:guid}", async (
            Guid programId,
            UpdateTrainingProgramRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingProgramService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingProgramsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateAsync(tenantId, actorUserId, programId, request, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"UpdateTrainingProgram{nameSuffix}");
    }
}
