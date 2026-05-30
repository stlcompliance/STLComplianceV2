using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class QualificationCheckEndpoints
{
    public static void MapTrainArrQualificationCheckEndpoints(this WebApplication app)
    {
        var routes = new[]
        {
            (Route: "/api/qualification-checks", Suffix: string.Empty),
            (Route: "/api/v1/qualification-checks", Suffix: "V1QualificationChecks"),
            (Route: "/api/v1/authorization-checks", Suffix: "V1AuthorizationChecks")
        };

        foreach (var (route, suffix) in routes)
        {
            MapRoutes(
                app.MapGroup(route)
                    .WithTags("QualificationChecks")
                    .RequireAuthorization(),
                suffix);
        }
    }

    private static void MapRoutes(RouteGroupBuilder checks, string nameSuffix)
    {
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
        .WithName($"CreateQualificationCheck{nameSuffix}");

        checks.MapGet("/", async (
            Guid? staffarrPersonId,
            string? qualificationKey,
            int? limit,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            QualificationCheckService service,
            CancellationToken cancellationToken) =>
        {
            if (staffarrPersonId is Guid personId)
            {
                authorization.RequireQualificationChecks(context.User, personId);
            }
            else
            {
                authorization.RequireBatchQualificationChecks(context.User);
            }

            var tenantId = context.User.GetTenantId();
            var history = await service.ListRecentAsync(
                tenantId,
                staffarrPersonId,
                qualificationKey,
                limit,
                cancellationToken);
            return Results.Ok(history);
        })
        .WithName($"ListQualificationChecks{nameSuffix}");

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
        .WithName($"CreateBatchQualificationCheck{nameSuffix}");
    }
}
