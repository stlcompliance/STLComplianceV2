using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class TrainingMatrixEndpoints
{
    public static void MapTrainArrTrainingMatrixEndpoints(this WebApplication app)
    {
        var matrix = app.MapGroup("/api/training-matrix")
            .WithTags("TrainingMatrix")
            .RequireAuthorization();

        matrix.MapGet("/", async (
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingMatrixService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingProgramsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetViewAsync(tenantId, cancellationToken));
        })
        .WithName("GetTrainingMatrix");

        matrix.MapPost("/", async (
            CreateTrainingMatrixEntryRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingMatrixService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingProgramsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/training-matrix/{created.MatrixEntryId}", created);
        })
        .WithName("CreateTrainingMatrixEntry");

        matrix.MapPut("/{matrixEntryId:guid}", async (
            Guid matrixEntryId,
            UpdateTrainingMatrixEntryRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingMatrixService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingProgramsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateAsync(tenantId, actorUserId, matrixEntryId, request, cancellationToken));
        })
        .WithName("UpdateTrainingMatrixEntry");

        matrix.MapDelete("/{matrixEntryId:guid}", async (
            Guid matrixEntryId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingMatrixService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingProgramsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            await service.DeleteAsync(tenantId, actorUserId, matrixEntryId, cancellationToken);
            return Results.NoContent();
        })
        .WithName("DeleteTrainingMatrixEntry");
    }
}
