using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class RuleVersionEndpoints
{
    public static void MapComplianceCoreRuleVersionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/rule-versions")
            .WithTags("RuleVersions")
            .RequireAuthorization();

        group.MapGet("/", async (
            string? packKey,
            ComplianceCoreAuthorizationService authorization,
            RuleVersionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, packKey, cancellationToken));
        })
        .WithName("ListRuleVersions");

        group.MapPost("/{rulePackId:guid}/publish", async (
            Guid rulePackId,
            ComplianceCoreAuthorizationService authorization,
            RuleVersionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksPublish(context.User);
            var tenantId = context.User.GetTenantId();
            var published = await service.PublishAsync(
                tenantId,
                context.User.GetUserId(),
                rulePackId,
                cancellationToken);
            return Results.Ok(published);
        })
        .WithName("PublishRuleVersion");

        group.MapPost("/{rulePackId:guid}/rollback", async (
            Guid rulePackId,
            ComplianceCoreAuthorizationService authorization,
            RuleVersionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksPublish(context.User);
            var tenantId = context.User.GetTenantId();
            var rolledBack = await service.RollbackAsync(
                tenantId,
                context.User.GetUserId(),
                rulePackId,
                cancellationToken);
            return Results.Ok(rolledBack);
        })
        .WithName("RollbackRuleVersion");
    }
}
