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

        rulePacks.MapGet("/{id:guid}", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            RulePackService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, id, cancellationToken));
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

        rulePacks.MapPatch("/{id:guid}", async (
            Guid id,
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
                id,
                request,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"PatchRulePack{nameSuffix}");

        rulePacks.MapPatch("/{id:guid}/status", async (
            Guid id,
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
                id,
                request,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"UpdateRulePackStatus{nameSuffix}");

        rulePacks.MapPost("/{id:guid}/submit-review", async (
            Guid id,
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
                id,
                new UpdateRulePackStatusRequest(RulePackStatuses.Review),
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"SubmitRulePackReview{nameSuffix}");

        rulePacks.MapPost("/{id:guid}/approve", async (
            Guid id,
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
                id,
                cancellationToken));
        })
        .WithName($"ApproveRulePack{nameSuffix}");

        rulePacks.MapPost("/{id:guid}/publish", async (
            Guid id,
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
                id,
                cancellationToken));
        })
        .WithName($"PublishRulePack{nameSuffix}");

        rulePacks.MapPost("/{id:guid}/retire", async (
            Guid id,
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
                id,
                new UpdateRulePackStatusRequest(RulePackStatuses.Archived),
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"RetireRulePack{nameSuffix}");

        rulePacks.MapPost("/{id:guid}/clone", async (
            Guid id,
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
                id,
                request,
                cancellationToken);
            return Results.Created($"/api/rule-packs/{cloned.RulePackId}", cloned);
        })
        .WithName($"CloneRulePack{nameSuffix}");

        rulePacks.MapGet("/{id:guid}/versions", async (
            Guid id,
            ComplianceCoreAuthorizationService authorization,
            RuleVersionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListForRulePackIdAsync(tenantId, id, cancellationToken));
        })
        .WithName($"ListRulePackVersions{nameSuffix}");

        rulePacks.MapGet("/{id:guid}/diff", async (
            Guid id,
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
                id,
                compareRulePackId,
                cancellationToken));
        })
        .WithName($"DiffRulePack{nameSuffix}");
    }
}
