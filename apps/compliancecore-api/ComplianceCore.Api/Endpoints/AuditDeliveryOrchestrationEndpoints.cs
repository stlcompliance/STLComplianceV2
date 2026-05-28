using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class AuditDeliveryOrchestrationEndpoints
{
    public static void MapComplianceCoreAuditDeliveryOrchestrationEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/audit-delivery-orchestration")
            .WithTags("AuditDeliveryOrchestration")
            .RequireAuthorization();

        api.MapGet("/", async (
            HttpContext context,
            ComplianceCoreAuthorizationService authorization,
            AuditDeliveryOrchestrationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuditDeliveryOrchestrationRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetStatusAsync(tenantId, cancellationToken));
        })
        .WithName("GetComplianceCoreAuditDeliveryOrchestrationStatus");

        api.MapPost("/trigger-scheduled-evaluation", async (
            HttpContext context,
            ComplianceCoreAuthorizationService authorization,
            AuditDeliveryOrchestrationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuditDeliveryOrchestrationManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.TriggerScheduledEvaluationAsync(tenantId, cancellationToken));
        })
        .WithName("TriggerComplianceCoreScheduledRuleEvaluation");

        api.MapPost("/trigger-m12-batch", async (
            HttpContext context,
            ComplianceCoreAuthorizationService authorization,
            AuditDeliveryOrchestrationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuditDeliveryOrchestrationManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.TriggerM12BatchAsync(tenantId, actorUserId, cancellationToken));
        })
        .WithName("TriggerComplianceCoreM12AnalyticsBatch");
    }
}
