using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class TrainingRulePackRequirementEndpoints
{
    public static void MapTrainArrTrainingRulePackRequirementEndpoints(this WebApplication app)
    {
        MapForEntity(app, TrainingRulePackRequirementEntityTypes.TrainingDefinition, "training-definitions");
        MapForEntity(app, TrainingRulePackRequirementEntityTypes.TrainingProgram, "training-programs");
    }

    private static void MapForEntity(WebApplication app, string entityType, string routePrefix)
    {
        var group = app.MapGroup($"/api/{routePrefix}/{{entityId:guid}}/rule-pack-requirements")
            .WithTags("TrainingRulePackRequirements")
            .RequireAuthorization();

        group.MapGet("/", async (
            Guid entityId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingRulePackRequirementService service,
            CancellationToken cancellationToken,
            bool includeMetadata = false) =>
        {
            authorization.RequireRulePackRequirementsRead(context.User, entityType);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(
                tenantId,
                entityType,
                entityId,
                includeMetadata,
                cancellationToken));
        })
        .WithName($"List{routePrefix}RulePackRequirements");

        group.MapPut("/", async (
            Guid entityId,
            UpsertTrainingRulePackRequirementRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingRulePackRequirementService service,
            CancellationToken cancellationToken,
            bool validateWithComplianceCore = false) =>
        {
            authorization.RequireRulePackRequirementsManage(context.User, entityType);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var upserted = await service.UpsertAsync(
                tenantId,
                actorUserId,
                entityType,
                entityId,
                request,
                validateWithComplianceCore,
                cancellationToken);
            return Results.Ok(upserted);
        })
        .WithName($"Upsert{routePrefix}RulePackRequirement");

        group.MapDelete("/{requirementId:guid}", async (
            Guid entityId,
            Guid requirementId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingRulePackRequirementService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePackRequirementsManage(context.User, entityType);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            await service.RemoveAsync(
                tenantId,
                actorUserId,
                entityType,
                entityId,
                requirementId,
                cancellationToken);
            return Results.NoContent();
        })
        .WithName($"Remove{routePrefix}RulePackRequirement");
    }
}
