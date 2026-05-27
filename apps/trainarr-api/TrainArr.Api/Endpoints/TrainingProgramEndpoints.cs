using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class TrainingProgramEndpoints
{
    public static void MapTrainArrTrainingProgramEndpoints(this WebApplication app)
    {
        var programs = app.MapGroup("/api/training-programs")
            .WithTags("TrainingPrograms")
            .RequireAuthorization();

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
        .WithName("ListTrainingPrograms");

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
        .WithName("CreateTrainingProgram");

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
        .WithName("GetTrainingProgram");

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
        .WithName("UpdateTrainingProgram");
    }
}
