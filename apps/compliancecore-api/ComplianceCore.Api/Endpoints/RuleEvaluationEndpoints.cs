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

        var v1Evaluations = app.MapGroup("/api/v1/evaluations")
            .WithTags("RuleEvaluation")
            .RequireAuthorization();

        v1Evaluations.MapPost("/run", async (
            EvaluateRulePackRunRequest request,
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
                request.RulePackId,
                new EvaluateRulePackRequest(request.Facts, request.EmitFindings),
                cancellationToken);
            return Results.Created($"/api/v1/evaluations/{result.EvaluationRunId}", result);
        })
        .WithName("RunRuleEvaluationV1");

        v1Evaluations.MapPost("/batch", async (
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
        .WithName("EvaluateRulePackBatchV1");

        v1Evaluations.MapGet("/", async (
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
        .WithName("ListRuleEvaluationsV1");

        v1Evaluations.MapGet("/{evaluationRunId:guid}", async (
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
        .WithName("GetRuleEvaluationV1");

        v1Evaluations.MapGet("/{evaluationRunId:guid}/audit-export", async (
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
        .WithName("ExportRuleEvaluationAuditPackageV1");

        v1Evaluations.MapPost("/simulate", async (
            EvaluateRulePackSimulationRequest request,
            ComplianceCoreAuthorizationService authorization,
            RuleEvaluationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRuleEvaluation(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.SimulateAsync(
                tenantId,
                request.RulePackId,
                request.Facts,
                cancellationToken));
        })
        .WithName("SimulateRuleEvaluationV1");

        v1Evaluations.MapPost("/{evaluationRunId:guid}/re-evaluate", async (
            Guid evaluationRunId,
            ReEvaluateRuleEvaluationRequest request,
            ComplianceCoreAuthorizationService authorization,
            RuleEvaluationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRuleEvaluation(context.User);
            var tenantId = context.User.GetTenantId();
            var result = await service.ReEvaluateAsync(
                tenantId,
                context.User.GetUserId(),
                evaluationRunId,
                request.EmitFindings,
                cancellationToken);
            return Results.Created($"/api/v1/evaluations/{result.EvaluationRunId}", result);
        })
        .WithName("ReEvaluateRuleEvaluationV1");

        v1Evaluations.MapGet("/{evaluationRunId:guid}/explanation", async (
            Guid evaluationRunId,
            ComplianceCoreAuthorizationService authorization,
            RuleEvaluationService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.BuildExplanationAsync(
                tenantId,
                evaluationRunId,
                cancellationToken));
        })
        .WithName("GetRuleEvaluationExplanationV1");
    }
}
