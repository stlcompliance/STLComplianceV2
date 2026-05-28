using ComplianceCore.Api.Contracts;

using ComplianceCore.Api.Entities;

using ComplianceCore.Api.Services;

using STLCompliance.Shared.Auth;



namespace ComplianceCore.Api.Endpoints;



public static class RulePackEndpoints

{

    public static void MapComplianceCoreRulePackEndpoints(this WebApplication app)

    {

        var rulePacks = app.MapGroup("/api/rule-packs")

            .WithTags("RulePacks")

            .RequireAuthorization();



        rulePacks.MapGet("/", async (

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

        .WithName("ListRulePacks");



        rulePacks.MapPost("/", async (

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

        .WithName("CreateRulePack");



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

        .WithName("UpdateRulePackStatus");

    }

}


