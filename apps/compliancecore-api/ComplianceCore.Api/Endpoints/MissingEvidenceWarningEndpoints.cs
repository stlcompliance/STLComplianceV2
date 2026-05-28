using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class MissingEvidenceWarningEndpoints
{
    public static void MapComplianceCoreMissingEvidenceWarningEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/missing-evidence-warnings")
            .WithTags("MissingEvidenceWarnings")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            ComplianceCoreAuthorizationService authorization,
            MissingEvidenceWarningService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireMissingEvidenceWarningRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetSummaryAsync(tenantId, cancellationToken));
        })
        .WithName("GetMissingEvidenceWarningSummary");

        group.MapGet("/", async (
            string? scopeKey,
            string? rulePackKey,
            string? severity,
            Guid? runId,
            int? limit,
            ComplianceCoreAuthorizationService authorization,
            MissingEvidenceWarningService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireMissingEvidenceWarningRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListWarningsAsync(
                tenantId,
                scopeKey,
                rulePackKey,
                severity,
                runId,
                limit,
                cancellationToken));
        })
        .WithName("ListMissingEvidenceWarnings");

        group.MapPost("/evaluate", async (
            EvaluateMissingEvidenceWarningsRequest request,
            ComplianceCoreAuthorizationService authorization,
            MissingEvidenceWarningService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireMissingEvidenceWarningEvaluate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.EvaluateAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken));
        })
        .WithName("EvaluateMissingEvidenceWarnings");
    }
}
