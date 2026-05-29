using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Endpoints;

public static class TrainingDefinitionCompletionRuleEndpoints
{
    public static void MapTrainArrTrainingDefinitionCompletionRuleEndpoints(this WebApplication app)
    {
        var catalog = app.MapGroup("/api/training-completion-rules")
            .WithTags("TrainingCompletionRules")
            .RequireAuthorization();

        catalog.MapGet("/catalog", (
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingDefinitionCompletionRuleService service) =>
        {
            authorization.RequireTrainingDefinitionsRead(context.User);
            return Results.Ok(service.ListCatalog());
        })
        .WithName("ListTrainingCompletionRuleCatalog");

        var rules = app.MapGroup("/api/training-definitions/{trainingDefinitionId:guid}/completion-rules")
            .WithTags("TrainingCompletionRules")
            .RequireAuthorization();

        rules.MapGet("/", async (
            Guid trainingDefinitionId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingDefinitionCompletionRuleService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingDefinitionsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListForDefinitionAsync(tenantId, trainingDefinitionId, cancellationToken));
        })
        .WithName("ListTrainingDefinitionCompletionRules");

        rules.MapPost("/", async (
            Guid trainingDefinitionId,
            CreateTrainingDefinitionCompletionRuleRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingDefinitionCompletionRuleService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingDefinitionsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(
                tenantId,
                actorUserId,
                trainingDefinitionId,
                request,
                cancellationToken);
            return Results.Created(
                $"/api/training-definitions/{trainingDefinitionId}/completion-rules/{created.CompletionRuleId}",
                created);
        })
        .WithName("CreateTrainingDefinitionCompletionRule");

        rules.MapPut("/{completionRuleId:guid}", async (
            Guid trainingDefinitionId,
            Guid completionRuleId,
            UpdateTrainingDefinitionCompletionRuleRequest request,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingDefinitionCompletionRuleService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingDefinitionsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateAsync(
                tenantId,
                actorUserId,
                trainingDefinitionId,
                completionRuleId,
                request,
                cancellationToken));
        })
        .WithName("UpdateTrainingDefinitionCompletionRule");

        rules.MapDelete("/{completionRuleId:guid}", async (
            Guid trainingDefinitionId,
            Guid completionRuleId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            TrainingDefinitionCompletionRuleService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingDefinitionsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            await service.DeleteAsync(
                tenantId,
                actorUserId,
                trainingDefinitionId,
                completionRuleId,
                cancellationToken);
            return Results.NoContent();
        })
        .WithName("DeleteTrainingDefinitionCompletionRule");
    }
}
