using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Entities;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class RulePackEndpoints
{
    public static void MapComplianceCoreRulePackEndpoints(this WebApplication app)
    {
        MapRoutes(
            app.MapGroup("/api/rule-packs")
                .WithTags("RulePacks")
                .RequireAuthorization(),
            string.Empty);
        MapRoutes(
            app.MapGroup("/api/v1/rule-packs")
                .WithTags("RulePacks")
                .RequireAuthorization(),
            "V1RulePacks");
    }

    private static void MapRoutes(RouteGroupBuilder rulePacks, string nameSuffix)
    {
        rulePacks.MapGet(string.Empty, async (
            Guid? regulatoryProgramId,
            ComplianceCoreAuthorizationService authorization,
            RulePackService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, regulatoryProgramId, cancellationToken));
        })
        .WithName($"ListRulePacks{nameSuffix}");

        rulePacks.MapGet("/{rulePackId:guid}", async (
            Guid rulePackId,
            ComplianceCoreAuthorizationService authorization,
            RulePackService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, rulePackId, cancellationToken));
        })
        .WithName($"GetRulePack{nameSuffix}");

        rulePacks.MapPost(string.Empty, async (
            CreateRulePackRequest request,
            ComplianceCoreAuthorizationService authorization,
            RulePackService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var created = await service.CreateAsync(
                tenantId,
                context.User.GetUserId(),
                request,
                cancellationToken);
            return Results.Created($"/api/rule-packs/{created.RulePackId}", created);
        })
        .WithName($"CreateRulePack{nameSuffix}");

        rulePacks.MapPatch("/{rulePackId:guid}", async (
            Guid rulePackId,
            PatchRulePackRequest request,
            ComplianceCoreAuthorizationService authorization,
            RulePackService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                if (string.Equals(request.Status, RulePackStatuses.Published, StringComparison.OrdinalIgnoreCase))
                {
                    authorization.RequireRulePacksPublish(context.User);
                }
                else if (string.Equals(request.Status, RulePackStatuses.Review, StringComparison.OrdinalIgnoreCase))
                {
                    authorization.RequireRulePacksCreate(context.User);
                }
                else
                {
                    authorization.RequireRegulatoryManage(context.User);
                }
            }
            else
            {
                authorization.RequireRulePacksCreate(context.User);
            }

            var tenantId = context.User.GetTenantId();
            var updated = await service.PatchAsync(
                tenantId,
                context.User.GetUserId(),
                rulePackId,
                request,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"PatchRulePack{nameSuffix}");

        rulePacks.MapPatch("/{rulePackId:guid}/status", async (
            Guid rulePackId,
            UpdateRulePackStatusRequest request,
            ComplianceCoreAuthorizationService authorization,
            RulePackService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            if (string.Equals(request.Status, RulePackStatuses.Published, StringComparison.OrdinalIgnoreCase))
            {
                authorization.RequireRulePacksPublish(context.User);
            }
            else if (string.Equals(request.Status, RulePackStatuses.Review, StringComparison.OrdinalIgnoreCase))
            {
                authorization.RequireRulePacksCreate(context.User);
            }
            else
            {
                authorization.RequireRegulatoryManage(context.User);
            }

            var tenantId = context.User.GetTenantId();
            var updated = await service.UpdateStatusAsync(
                tenantId,
                context.User.GetUserId(),
                rulePackId,
                request,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"UpdateRulePackStatus{nameSuffix}");

        rulePacks.MapPost("/{rulePackId:guid}/submit-review", async (
            Guid rulePackId,
            ComplianceCoreAuthorizationService authorization,
            RulePackService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var updated = await service.UpdateStatusAsync(
                tenantId,
                context.User.GetUserId(),
                rulePackId,
                new UpdateRulePackStatusRequest(RulePackStatuses.Review),
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"SubmitRulePackReview{nameSuffix}");

        rulePacks.MapPost("/{rulePackId:guid}/approve", async (
            Guid rulePackId,
            ComplianceCoreAuthorizationService authorization,
            RuleVersionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksPublish(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.PublishAsync(
                tenantId,
                context.User.GetUserId(),
                rulePackId,
                cancellationToken));
        })
        .WithName($"ApproveRulePack{nameSuffix}");

        rulePacks.MapPost("/{rulePackId:guid}/publish", async (
            Guid rulePackId,
            ComplianceCoreAuthorizationService authorization,
            RuleVersionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksPublish(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.PublishAsync(
                tenantId,
                context.User.GetUserId(),
                rulePackId,
                cancellationToken));
        })
        .WithName($"PublishRulePack{nameSuffix}");

        rulePacks.MapPost("/{rulePackId:guid}/retire", async (
            Guid rulePackId,
            ComplianceCoreAuthorizationService authorization,
            RulePackService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryManage(context.User);
            var tenantId = context.User.GetTenantId();
            var updated = await service.UpdateStatusAsync(
                tenantId,
                context.User.GetUserId(),
                rulePackId,
                new UpdateRulePackStatusRequest(RulePackStatuses.Archived),
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"RetireRulePack{nameSuffix}");

        rulePacks.MapPost("/{rulePackId:guid}/clone", async (
            Guid rulePackId,
            CloneRulePackRequest request,
            ComplianceCoreAuthorizationService authorization,
            RulePackService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var cloned = await service.CloneAsync(
                tenantId,
                context.User.GetUserId(),
                rulePackId,
                request,
                cancellationToken);
            return Results.Created($"/api/rule-packs/{cloned.RulePackId}", cloned);
        })
        .WithName($"CloneRulePack{nameSuffix}");

        rulePacks.MapGet("/{rulePackId:guid}/versions", async (
            Guid rulePackId,
            ComplianceCoreAuthorizationService authorization,
            RuleVersionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListForRulePackIdAsync(tenantId, rulePackId, cancellationToken));
        })
        .WithName($"ListRulePackVersions{nameSuffix}");

        rulePacks.MapGet("/{rulePackId:guid}/diff", async (
            Guid rulePackId,
            Guid? compareRulePackId,
            ComplianceCoreAuthorizationService authorization,
            RulePackService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.DiffAsync(
                tenantId,
                rulePackId,
                compareRulePackId,
                cancellationToken));
        })
        .WithName($"DiffRulePack{nameSuffix}");
    }
}
