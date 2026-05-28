using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class RuleChangeMonitoringEndpoints
{
    public static void MapComplianceCoreRuleChangeMonitoringEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/rule-changes")
            .WithTags("RuleChanges")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            ComplianceCoreAuthorizationService authorization,
            RuleChangeMonitoringService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRuleChangeMonitoringRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetSummaryAsync(tenantId, cancellationToken));
        })
        .WithName("GetRuleChangeMonitoringSummary");

        group.MapGet("/events", async (
            string? packKey,
            string? changeType,
            DateTimeOffset? since,
            int? limit,
            ComplianceCoreAuthorizationService authorization,
            RuleChangeMonitoringService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRuleChangeMonitoringRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListEventsAsync(
                tenantId,
                packKey,
                changeType,
                since,
                limit,
                cancellationToken));
        })
        .WithName("ListRuleChangeEvents");

        group.MapGet("/events/{eventId:guid}", async (
            Guid eventId,
            ComplianceCoreAuthorizationService authorization,
            RuleChangeMonitoringService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRuleChangeMonitoringRead(context.User);
            var tenantId = context.User.GetTenantId();
            var detail = await service.GetEventAsync(tenantId, eventId, cancellationToken);
            return detail is null ? Results.NotFound() : Results.Ok(detail);
        })
        .WithName("GetRuleChangeEvent");
    }
}
