using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class ControlEffectivenessEndpoints
{
    public static void MapComplianceCoreControlEffectivenessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/control-effectiveness")
            .WithTags("ControlEffectiveness")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            ComplianceCoreAuthorizationService authorization,
            ControlEffectivenessService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireControlEffectivenessRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetSummaryAsync(tenantId, cancellationToken));
        })
        .WithName("GetControlEffectivenessSummary");

        group.MapGet("/", async (
            string? scopeKey,
            string? rulePackKey,
            string? effectivenessLevel,
            Guid? runId,
            int? limit,
            ComplianceCoreAuthorizationService authorization,
            ControlEffectivenessService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireControlEffectivenessRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListRecordsAsync(
                tenantId,
                scopeKey,
                rulePackKey,
                effectivenessLevel,
                runId,
                limit,
                cancellationToken));
        })
        .WithName("ListControlEffectivenessRecords");

        group.MapPost("/evaluate", async (
            EvaluateControlEffectivenessRequest request,
            ComplianceCoreAuthorizationService authorization,
            ControlEffectivenessService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireControlEffectivenessEvaluate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.EvaluateAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken));
        })
        .WithName("EvaluateControlEffectiveness");
    }
}
