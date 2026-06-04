using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class RuleTestCaseEndpoints
{
    public static void MapComplianceCoreRuleTestCaseEndpoints(this WebApplication app)
    {
        var testCases = app.MapGroup("/api/v1/rule-packs/{rulePackId:guid}/test-cases")
            .WithTags("RuleTestCases")
            .RequireAuthorization();

        testCases.MapGet("/", async (
            Guid rulePackId,
            ComplianceCoreAuthorizationService authorization,
            RuleTestCaseService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            return Results.Ok(await service.ListAsync(context.User.GetTenantId(), rulePackId, cancellationToken));
        })
        .WithName("ListRuleTestCasesV1");

        testCases.MapGet("/{testCaseId:guid}", async (
            Guid rulePackId,
            Guid testCaseId,
            ComplianceCoreAuthorizationService authorization,
            RuleTestCaseService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            return Results.Ok(await service.GetAsync(context.User.GetTenantId(), rulePackId, testCaseId, cancellationToken));
        })
        .WithName("GetRuleTestCaseV1");

        testCases.MapPost("/", async (
            Guid rulePackId,
            CreateRuleTestCaseRequest request,
            ComplianceCoreAuthorizationService authorization,
            RuleTestCaseService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksCreate(context.User);
            var created = await service.CreateAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                rulePackId,
                request,
                cancellationToken);
            return Results.Created($"/api/v1/rule-packs/{rulePackId}/test-cases/{created.RuleTestCaseId}", created);
        })
        .WithName("CreateRuleTestCaseV1");

        testCases.MapPatch("/{testCaseId:guid}", async (
            Guid rulePackId,
            Guid testCaseId,
            PatchRuleTestCaseRequest request,
            ComplianceCoreAuthorizationService authorization,
            RuleTestCaseService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksCreate(context.User);
            return Results.Ok(await service.PatchAsync(
                context.User.GetTenantId(),
                rulePackId,
                testCaseId,
                request,
                cancellationToken));
        })
        .WithName("PatchRuleTestCaseV1");

        testCases.MapDelete("/{testCaseId:guid}", async (
            Guid rulePackId,
            Guid testCaseId,
            ComplianceCoreAuthorizationService authorization,
            RuleTestCaseService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksCreate(context.User);
            await service.DeleteAsync(context.User.GetTenantId(), rulePackId, testCaseId, cancellationToken);
            return Results.NoContent();
        })
        .WithName("DeleteRuleTestCaseV1");

        testCases.MapPost("/{testCaseId:guid}/run", async (
            Guid rulePackId,
            Guid testCaseId,
            ComplianceCoreAuthorizationService authorization,
            RuleTestCaseService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRuleEvaluation(context.User);
            return Results.Ok(await service.RunAsync(context.User.GetTenantId(), rulePackId, testCaseId, cancellationToken));
        })
        .WithName("RunRuleTestCaseV1");
    }
}
