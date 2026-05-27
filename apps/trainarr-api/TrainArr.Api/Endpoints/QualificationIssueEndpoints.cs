using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class QualificationIssueEndpoints
{
    public static void MapTrainArrQualificationIssueEndpoints(this WebApplication app)
    {
        var qualifications = app.MapGroup("/api/qualification-issues")
            .WithTags("QualificationIssues")
            .RequireAuthorization();

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
        .WithName("GetQualificationIssue");

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
        .WithName("SuspendQualificationIssue");

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
        .WithName("RevokeQualificationIssue");

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
        .WithName("ExpireQualificationIssue");
    }
}
