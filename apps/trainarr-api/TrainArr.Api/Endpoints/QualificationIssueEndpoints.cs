using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class QualificationIssueEndpoints
{
    public static void MapTrainArrQualificationIssueEndpoints(this WebApplication app)
    {
        MapRoutes(
            app.MapGroup("/api/qualification-issues")
                .WithTags("QualificationIssues")
                .RequireAuthorization(),
            string.Empty);
        MapRoutes(
            app.MapGroup("/api/v1/qualification-issues")
                .WithTags("QualificationIssues")
                .RequireAuthorization(),
            "V1QualificationIssues");
        MapRoutes(
            app.MapGroup("/api/v1/qualifications")
                .WithTags("QualificationIssues")
                .RequireAuthorization(),
            "V1Qualifications");
    }

    private static void MapRoutes(RouteGroupBuilder qualifications, string nameSuffix)
    {
        qualifications.MapGet("/", async (
            string? status,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            QualificationIssueService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireQualificationsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, status, cancellationToken));
        })
        .WithName($"ListQualificationIssues{nameSuffix}");

        qualifications.MapGet("/{qualificationIssueId:guid}", async (
            Guid qualificationIssueId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            QualificationIssueService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireQualificationsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetByIdAsync(tenantId, qualificationIssueId, cancellationToken));
        })
        .WithName($"GetQualificationIssue{nameSuffix}");

        qualifications.MapPost("/{qualificationIssueId:guid}/suspend", async (
            Guid qualificationIssueId,
            QualificationLifecycleActionRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            QualificationIssueService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireQualificationsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await service.SuspendAsync(
                tenantId,
                actorUserId,
                qualificationIssueId,
                request,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName($"SuspendQualificationIssue{nameSuffix}");

        qualifications.MapPost("/{qualificationIssueId:guid}/revoke", async (
            Guid qualificationIssueId,
            QualificationLifecycleActionRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            QualificationIssueService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireQualificationsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await service.RevokeAsync(
                tenantId,
                actorUserId,
                qualificationIssueId,
                request,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName($"RevokeQualificationIssue{nameSuffix}");

        qualifications.MapPost("/{qualificationIssueId:guid}/expire", async (
            Guid qualificationIssueId,
            QualificationLifecycleActionRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            QualificationIssueService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireQualificationsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await service.ExpireAsync(
                tenantId,
                actorUserId,
                qualificationIssueId,
                request,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName($"ExpireQualificationIssue{nameSuffix}");
    }
}
