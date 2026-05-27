using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class QualificationCheckEndpoints
{
    public static void MapTrainArrQualificationCheckEndpoints(this WebApplication app)
    {
        var checks = app.MapGroup("/api/qualification-checks")
            .WithTags("QualificationChecks")
            .RequireAuthorization();

        checks.MapPost("/", async (
            CreateQualificationCheckRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            QualificationCheckService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireQualificationChecks(context.User, request.StaffarrPersonId);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await service.CheckAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("CreateQualificationCheck");

        checks.MapPost("/batch", async (
            CreateBatchQualificationCheckRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            QualificationCheckService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireBatchQualificationChecks(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await service.CheckBatchAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("CreateBatchQualificationCheck");
    }
}
