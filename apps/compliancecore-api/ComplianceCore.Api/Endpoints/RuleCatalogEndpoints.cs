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

        rules.MapGet("/{id}", async (
            string id,
            ComplianceCoreAuthorizationService authorization,
            RuleCatalogService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, id, cancellationToken));
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

        rules.MapPatch("/{id}", async (
            string id,
            PatchRuleCatalogRequest request,
            ComplianceCoreAuthorizationService authorization,
            RuleCatalogService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksCreate(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.PatchAsync(tenantId, context.User.GetUserId(), id, request, cancellationToken));
        })
        .WithName("PatchRuleV1");

        rules.MapPost("/{id}/validate", async (
            string id,
            ComplianceCoreAuthorizationService authorization,
            RuleCatalogService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ValidateAsync(tenantId, id, cancellationToken));
        })
        .WithName("ValidateRuleV1");

        rules.MapPost("/{id}/test", async (
            string id,
            RuleCatalogTestRequest request,
            ComplianceCoreAuthorizationService authorization,
            RuleCatalogService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRuleEvaluation(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.TestAsync(tenantId, id, request, cancellationToken));
        })
        .WithName("TestRuleV1");

        rules.MapGet("/{id}/usage", async (
            string id,
            ComplianceCoreAuthorizationService authorization,
            RuleCatalogService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetUsageAsync(tenantId, id, cancellationToken));
        })
        .WithName("GetRuleUsageV1");

        rules.MapGet("/{id}/history", async (
            string id,
            ComplianceCoreAuthorizationService authorization,
            RuleCatalogService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetHistoryAsync(tenantId, id, cancellationToken));
        })
        .WithName("GetRuleHistoryV1");
    }
}
