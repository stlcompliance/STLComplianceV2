using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class RuleEvaluationEndpoints
{
    public static void MapComplianceCoreRuleEvaluationEndpoints(this WebApplication app)
    {
        var rulePacks = app.MapGroup("/api/rule-packs")
            .WithTags("RuleEvaluation")
            .RequireAuthorization();

        rulePacks.MapPost("/evaluate/batch", async (
            EvaluateRulePackBatchRequest request,
            ComplianceCoreAuthorizationService authorization,
            RulePackBatchEvaluationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRuleEvaluation(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.EvaluateBatchForUserAsync(
                tenantId,
                context.User.GetUserId(),
                request,
                cancellationToken));
        })
        .WithName("EvaluateRulePackBatch");

        rulePacks.MapGet("/{rulePackId:guid}/content", async (
            Guid rulePackId,
            ComplianceCoreAuthorizationService authorization,
            RuleContentService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetContentAsync(tenantId, rulePackId, cancellationToken));
        })
        .WithName("GetRulePackContent");

        rulePacks.MapPut("/{rulePackId:guid}/content", async (
            Guid rulePackId,
            UpdateRulePackContentRequest request,
            ComplianceCoreAuthorizationService authorization,
            RuleContentService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var updated = await service.UpdateContentAsync(
                tenantId,
                context.User.GetUserId(),
                rulePackId,
                request,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName("UpdateRulePackContent");

        rulePacks.MapPost("/{rulePackId:guid}/evaluate", async (
            Guid rulePackId,
            EvaluateRulePackRequest request,
            ComplianceCoreAuthorizationService authorization,
            RuleEvaluationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRuleEvaluation(context.User);
            var tenantId = context.User.GetTenantId();
            var result = await service.EvaluateAsync(
                tenantId,
                context.User.GetUserId(),
                rulePackId,
                request,
                cancellationToken);
            return Results.Created($"/api/rule-evaluations/{result.EvaluationRunId}", result);
        })
        .WithName("EvaluateRulePack");

        var evaluations = app.MapGroup("/api/rule-evaluations")
            .WithTags("RuleEvaluation")
            .RequireAuthorization();

        evaluations.MapGet("/", async (
            Guid? rulePackId,
            ComplianceCoreAuthorizationService authorization,
            RuleEvaluationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, rulePackId, cancellationToken));
        })
        .WithName("ListRuleEvaluations");

        evaluations.MapGet("/{evaluationRunId:guid}", async (
            Guid evaluationRunId,
            ComplianceCoreAuthorizationService authorization,
            RuleEvaluationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, evaluationRunId, cancellationToken));
        })
        .WithName("GetRuleEvaluation");

        evaluations.MapGet("/{evaluationRunId:guid}/audit-export", async (
            Guid evaluationRunId,
            ComplianceCoreAuthorizationService authorization,
            RuleEvaluationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuditPackageExport(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.BuildAuditExportAsync(
                tenantId,
                context.User.GetUserId(),
                evaluationRunId,
                cancellationToken));
        })
        .WithName("ExportRuleEvaluationAuditPackage");
    }
}
