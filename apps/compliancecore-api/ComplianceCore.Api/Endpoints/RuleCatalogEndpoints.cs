using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class RuleCatalogEndpoints
{
    public static void MapComplianceCoreRuleCatalogEndpoints(this WebApplication app)
    {
        var rules = app.MapGroup("/api/v1/rules")
            .WithTags("Rules")
            .RequireAuthorization();

        rules.MapGet("/", async (
            ComplianceCoreAuthorizationService authorization,
            RuleCatalogService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, cancellationToken));
        })
        .WithName("ListRulesV1");

        rules.MapGet("/{ruleId}", async (
            string ruleId,
            ComplianceCoreAuthorizationService authorization,
            RuleCatalogService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, ruleId, cancellationToken));
        })
        .WithName("GetRuleV1");

        rules.MapPost("/", async (
            CreateRuleCatalogRequest request,
            ComplianceCoreAuthorizationService authorization,
            RuleCatalogService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var created = await service.CreateAsync(tenantId, context.User.GetUserId(), request, cancellationToken);
            return Results.Created($"/api/v1/rules/{Uri.EscapeDataString(created.RuleId)}", created);
        })
        .WithName("CreateRuleV1");

        rules.MapPatch("/{ruleId}", async (
            string ruleId,
            PatchRuleCatalogRequest request,
            ComplianceCoreAuthorizationService authorization,
            RuleCatalogService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksCreate(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.PatchAsync(tenantId, context.User.GetUserId(), ruleId, request, cancellationToken));
        })
        .WithName("PatchRuleV1");

        rules.MapPost("/{ruleId}/validate", async (
            string ruleId,
            ComplianceCoreAuthorizationService authorization,
            RuleCatalogService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ValidateAsync(tenantId, ruleId, cancellationToken));
        })
        .WithName("ValidateRuleV1");

        rules.MapPost("/{ruleId}/test", async (
            string ruleId,
            RuleCatalogTestRequest request,
            ComplianceCoreAuthorizationService authorization,
            RuleCatalogService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRuleEvaluation(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.TestAsync(tenantId, ruleId, request, cancellationToken));
        })
        .WithName("TestRuleV1");

        rules.MapGet("/{ruleId}/usage", async (
            string ruleId,
            ComplianceCoreAuthorizationService authorization,
            RuleCatalogService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetUsageAsync(tenantId, ruleId, cancellationToken));
        })
        .WithName("GetRuleUsageV1");

        rules.MapGet("/{ruleId}/history", async (
            string ruleId,
            ComplianceCoreAuthorizationService authorization,
            RuleCatalogService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetHistoryAsync(tenantId, ruleId, cancellationToken));
        })
        .WithName("GetRuleHistoryV1");
    }
}
