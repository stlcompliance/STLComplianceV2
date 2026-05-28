using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class RiskScoringEndpoints
{
    public static void MapComplianceCoreRiskScoringEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/risk-scores")
            .WithTags("RiskScores")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            ComplianceCoreAuthorizationService authorization,
            RiskScoringService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRiskScoringRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetSummaryAsync(tenantId, cancellationToken));
        })
        .WithName("GetRiskScoreSummary");

        group.MapGet("/", async (
            string? scopeKey,
            string? rulePackKey,
            Guid? runId,
            int? limit,
            ComplianceCoreAuthorizationService authorization,
            RiskScoringService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRiskScoringRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListScoresAsync(
                tenantId,
                scopeKey,
                rulePackKey,
                runId,
                limit,
                cancellationToken));
        })
        .WithName("ListRiskScores");

        group.MapPost("/evaluate", async (
            EvaluateRiskScoresRequest request,
            ComplianceCoreAuthorizationService authorization,
            RiskScoringService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRiskScoringEvaluate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.EvaluateAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken));
        })
        .WithName("EvaluateRiskScores");
    }
}
