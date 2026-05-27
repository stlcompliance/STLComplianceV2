using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class RulePackImpactEndpoints
{
    public static void MapTrainArrRulePackImpactEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/rule-pack-impact")
            .WithTags("RulePackImpact")
            .RequireAuthorization();

        group.MapGet("/", async (
            string rulePackKey,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            RulePackImpactService service,
            CancellationToken cancellationToken,
            int? expectedVersionNumber = null,
            string? expectedStatus = null) =>
        {
            authorization.RequireRulePackImpactRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await service.AssessAsync(
                tenantId,
                actorUserId,
                rulePackKey,
                expectedVersionNumber,
                expectedStatus,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetRulePackImpactAssessment");

        group.MapPost("/assess", async (
            AssessRulePackImpactRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            RulePackImpactService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePackImpactRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await service.AssessAsync(
                tenantId,
                actorUserId,
                request.RulePackKey,
                request.ExpectedVersionNumber,
                request.ExpectedStatus,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("AssessRulePackImpact");
    }
}
